
using BulletHellTemplate.PVP;
using Cysharp.Threading.Tasks;
#if FUSION2
using Fusion;
using Fusion.Sockets;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages lobby flow (create, join-by-code, auto-join) using Photon Fusion 2 in Shared mode.
    /// Stores public player data and exposes it to the UI layer.
    /// </summary>
    public class FusionLobbyManager : MonoBehaviour
        #if FUSION2
        , INetworkRunnerCallbacks
        #endif
    {
        /* ------------------------------------------------------------------ */
        #region Inspector

        [Header("Lobby Settings")]
        public GameObject masterClientTrackerPrefab;

        [Header("Coop")]
        public GameObject gameSyncPrefab;
        [Tooltip("Maximum simultaneous players per session.")]
        public int defaultMaxPlayers = 3;
        
        [Header("PVP")]
        public GameObject pvpSyncPrefab;

        [Header("PVP Debug")]
        public bool pvpDebugStartSolo = false;
        #endregion
        public static FusionLobbyManager Instance { get; private set; }

        /* ------------------------------------------------------------------ */
#if FUSION2
        #region Runtime fields



        private NetworkRunner _runner;
        public NetworkRunner Runner => _runner;
        private PoolObjectProvider _provider;
        private readonly Dictionary<PlayerRef, PlayerData> _players = new();
        private PlayerRef _currentHost;
        private readonly Dictionary<int, int> _ref2idx = new();
        private string _currentSession;
        private string _selectedMap;
        private bool _isHostCached = false;
        private bool _waitingGameplayScene;
        private SceneRef _waitingRef;

        private bool _matchStarted = false;
        private int _matchBuildIndex = -1;
        private float _startGuardUntil = 0f;

        // Helper objects for async session search
        private readonly AutoResetEvent _searchDone = new(false);
        private List<SessionInfo> _lastSessionList = new();

        private PlayerData _pendingLocalData;
        private readonly HashSet<PlayerRef> _matchAcks = new();
        private readonly HashSet<PlayerRef> _sceneReadyAcks = new();
        private enum LobbyMsg : byte { PlayerInfo = 1, MatchStart = 2, MatchStartAck = 3, SceneReady = 4 }
        private int _reliableSeq = 0;

        //PVP
        public event Action<int, int> OnPlayerCountChanged; // (count, capacity)

        public int CurrentPlayerCount
        {
            get
            {
                if (_runner == null || !_runner.IsRunning) return 0;
                return _runner.ActivePlayers.Count();
            }
        }
        public int CurrentMaxPlayers { get; private set; }
        public string CurrentPvpModeKey { get; private set; }

        private bool isPvpGame;
#endregion
        /* ------------------------------------------------------------------ */
        #region Mono-life-cycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else { Destroy(gameObject); }

        }

        #endregion
        /* ------------------------------------------------------------------ */
        #region Public API (Create / Join / AutoJoin / Leave)

        /// <summary>Creates a new shared-mode session and becomes the host player.</summary>
        /// <summary>
        /// Creates a new Shared-mode session and becomes host player.
        /// Scene is resolved by name using <see cref="GetBuildIndexBySceneName"/>.
        /// </summary>
        public async void CreateRoom(string mapName, PlayerData _playerData, Action onJoined)
        {
            EnsureRunnerClosed();
            PrepareRunner();

            _pendingLocalData = _playerData;

            _currentSession = GenerateCode();
            _selectedMap = mapName;

            int buildIdx = GetBuildIndexBySceneName(mapName);
            if (buildIdx < 0)
            {
                Debug.LogError($"Scene '{mapName}' is not in Build Settings!");
                return;
            }

            SceneRef lobbyRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = _currentSession,
                Scene = lobbyRef,
                PlayerCount = defaultMaxPlayers,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                SessionProperties = new() { { "map", (SessionProperty)mapName } },
                ObjectProvider = _provider
            });

            if (result.Ok)
            {
                _pendingLocalData = _playerData;
                if (result.Ok)
                {
                    if (_runner.IsSharedModeMasterClient && masterClientTrackerPrefab)
                    {
                        await _runner.SpawnAsync(masterClientTrackerPrefab);                       
                    }             

                    onJoined?.Invoke();
                    Debug.Log($"Room {_currentSession} created");
                }
                else
                {
                    Debug.LogError($"StartGame failed: {result.ShutdownReason}");
                }
                onJoined?.Invoke();
                Debug.Log($"Room {_currentSession} created on map {mapName}");
            }
            else
                Debug.LogError($"StartGame failed: {result.ShutdownReason}");
        }


        /// <summary>Joins an existing session by its six-digit code.</summary>
        public async void JoinRoom(string code, PlayerData data, Action onJoined, Action<string> onFailed)
        {
            EnsureRunnerClosed();
            PrepareRunner();

            _pendingLocalData = data;
            _currentSession = code;

            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = code,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                ObjectProvider = _provider
            });

            if (result.Ok)
            {
                onJoined?.Invoke();
                Debug.Log($"Joined room {code}");
            }
            else
            {
                Debug.LogError($"Join failed: {result.ShutdownReason}");
                onFailed?.Invoke(result.ShutdownReason.ToString());
            }
        }


        /// <summary>
        /// Tries to find an open session running the same map; if none is found, creates a new one.
        /// </summary>
        public async void AutoJoinRoom(PlayerData data, Action<string> onJoined, Action<string> onFailed)
        {
            EnsureRunnerClosed();
            PrepareRunner();

            var any = await FindAnyAvailableSession();
            if (any == null || !any.IsValid)
            {
                onFailed?.Invoke("NO_ROOMS");
                return;
            }

            JoinRoom(any.Name, data,
                onJoined: () => onJoined?.Invoke(any.Name),
                onFailed: reason => onFailed?.Invoke(reason));
        }

        public void StartMatch()
        {
            StartMatchAsync().Forget();
        }

        private async UniTask StartMatchAsync()
        {
            if (_runner == null || !_runner.IsSceneAuthority)
                return;

            isPvpGame = !string.IsNullOrEmpty(CurrentPvpModeKey);

            var now = Time.realtimeSinceStartup;
            if (now < _startGuardUntil)
                await UniTask.Delay(TimeSpan.FromSeconds(_startGuardUntil - now));

            int buildIdx = GetBuildIndexBySceneName(_selectedMap);
            if (buildIdx < 0)
            {
                Debug.LogError("[Match] Map not found in Build Settings.");
                return;
            }

            await WaitForAllPlayerInfos(1500);

            _matchStarted = true;
            _matchBuildIndex = buildIdx;

            byte[] msg = EncodeMatchStart(buildIdx);
            foreach (var p in _runner.ActivePlayers)
                if (p != _runner.LocalPlayer)
                    SendBytesToPlayer(p, msg);

            LoadingManager.Singleton?.Open(0.2f);

            await WaitForMatchAcks(2000);

            _waitingGameplayScene = true;
            _waitingRef = SceneRef.FromIndex(buildIdx);

            var op = _runner.LoadScene(_waitingRef, LoadSceneMode.Single);
            await op;

            if (gameSyncPrefab && !GameplaySync.Instance)
                await _runner.SpawnAsync(gameSyncPrefab);

            if (!string.IsNullOrEmpty(CurrentPvpModeKey) && pvpSyncPrefab && _runner.IsSharedModeMasterClient)
            {
                var pvpNobj = await _runner.SpawnAsync(pvpSyncPrefab);
                var pvp = pvpNobj.GetComponent<PvpSync>();
                if (pvp)
                {
                    var mode = GameInstance.Singleton.pvpModes.FirstOrDefault(m => m && m.GetModeKey() == CurrentPvpModeKey);
                    if (mode) pvp.InitializeFrom(mode);
                }
            }

            if (GameplaySync.Instance && GameplaySync.Instance.HasStateAuthority)
            {
                GameplaySync.Instance.TimerSecs = GameplayManager.Singleton.GetSurvivalTime();
                GameplaySync.Instance.MatchStarted = true;
            }

            _matchAcks.Clear();
        }

        private async UniTask WaitForMatchAcks(int timeoutMs)
        {
            float end = Time.realtimeSinceStartup + timeoutMs / 1000f;
            while (Time.realtimeSinceStartup < end)
            {
                bool allAck =
                    _runner.ActivePlayers
                           .Where(p => p != _runner.LocalPlayer)
                           .All(p => _matchAcks.Contains(p));

                if (allAck) break;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        public async void StartPvpMatchmaking(
          PvpModeData mode,
          PlayerData data,
          Action<string> onJoinedOrCreated,
          Action<string> onFailed)
        {
            EnsureRunnerClosed();
            if (!PrepareRunner())
            {
                onFailed?.Invoke("CHAR_PREFAB_INVALID");
                return;
            }

            // ----- Guards NRE -----
            if (mode == null)
            {
                onFailed?.Invoke("MODE_NULL");
                return;
            }
            if (string.IsNullOrWhiteSpace(mode.sceneName))
            {
                onFailed?.Invoke("SCENE_INVALID");
                return;
            }
            int buildIdx = GetBuildIndexBySceneName(mode.sceneName);
            if (buildIdx < 0)
            {
                onFailed?.Invoke("SCENE_NOT_IN_BUILD");
                return;
            }
            int cap = mode.GetMaxPlayers();
            if (cap <= 0)
            {
                onFailed?.Invoke("CAPACITY_INVALID");
                return;
            }
            if (_runner == null)
            {
                onFailed?.Invoke("RUNNER_NULL");
                return;
            }

            try
            {
                isPvpGame = true;
                CurrentPvpModeKey = mode.GetModeKey() ?? string.Empty;
                CurrentMaxPlayers = cap;
                _selectedMap = mode.sceneName;
                _pendingLocalData = data;

                var session = await FindAvailablePvpSession(CurrentPvpModeKey, CurrentMaxPlayers);
                if (session != null && session.IsValid)
                {
                    var result = await _runner.StartGame(new StartGameArgs
                    {
                        GameMode = GameMode.Shared,
                        SessionName = session.Name,
                        SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                        ObjectProvider = _provider
                    });

                    if (!result.Ok)
                    {
                        Debug.LogError($"[PVP] Join Failed: {result.ShutdownReason}");
                        onFailed?.Invoke(result.ShutdownReason.ToString());
                        return;
                    }

                    _currentSession = session.Name;
                    onJoinedOrCreated?.Invoke(_currentSession);
                    Debug.Log($"[PVP] Enter on queue {session.Name} ({CurrentPvpModeKey})");
                }
                else
                {
                    var queueName = $"PVP-{CurrentPvpModeKey}-{CurrentMaxPlayers}-{_selectedMap}";
                    var lobbyRef = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

                    var create = await _runner.StartGame(new StartGameArgs
                    {
                        GameMode = GameMode.Shared,
                        SessionName = queueName,
                        Scene = lobbyRef,
                        PlayerCount = CurrentMaxPlayers,
                        SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                        ObjectProvider = _provider,
                        SessionProperties = new() {
                    { "pvp",  (SessionProperty)1 },
                    { "mode", (SessionProperty)CurrentPvpModeKey },
                    { "cap",  (SessionProperty)CurrentMaxPlayers },
                    { "map",  (SessionProperty)_selectedMap }
                }
                    });

                    if (!create.Ok)
                    {
                        var join = await _runner.StartGame(new StartGameArgs
                        {
                            GameMode = GameMode.Shared,
                            SessionName = queueName,
                            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                            ObjectProvider = _provider
                        });

                        if (!join.Ok)
                        {
                            Debug.LogError($"[PVP] Create/Join Failed: {join.ShutdownReason}");
                            onFailed?.Invoke(join.ShutdownReason.ToString());
                            return;
                        }
                    }

                    _currentSession = queueName;
                    if (_runner.IsSharedModeMasterClient && masterClientTrackerPrefab)
                        await _runner.SpawnAsync(masterClientTrackerPrefab);

                    onJoinedOrCreated?.Invoke(_currentSession);
                    Debug.Log($"[PVP] Queue {queueName} ({CurrentPvpModeKey}) cap={CurrentMaxPlayers}");
                }

                int safeCount = CurrentPlayerCount; 
                OnPlayerCountChanged?.Invoke(safeCount, CurrentMaxPlayers);
                AutoStartWhenFullAsync().Forget();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PVP] StartPvpMatchmaking exception: {ex}");
                onFailed?.Invoke(ex.Message);
            }
        }

        private async UniTaskVoid AutoStartWhenFullAsync()
        {
            await UniTask.WaitUntil(() => _runner != null);

            float queuedAt = Time.realtimeSinceStartup;
            const float grace = 3.0f;

            while (_runner && _runner.IsRunning)
            {
                int count = _runner.ActivePlayers.Count();
                OnPlayerCountChanged?.Invoke(count, CurrentMaxPlayers);

                if (_runner.IsSharedModeMasterClient &&
                    !string.IsNullOrEmpty(CurrentPvpModeKey) &&
                    !_matchStarted)
                {
                    if (count >= CurrentMaxPlayers)
                    {
                        await WaitForAllPlayerInfos(2000);
                        Debug.Log($"[PVP] AutoStart (full). players={count}/{CurrentMaxPlayers}");
                        StartMatch();
                        break;
                    }

                    if (pvpDebugStartSolo && (Time.realtimeSinceStartup - queuedAt) > grace && count >= 1)
                    {
                        await WaitForAllPlayerInfos(2000);
                        Debug.Log($"[PVP] AutoStart (solo DEV). players={count}/{CurrentMaxPlayers}");
                        StartMatch();
                        break;
                    }
                }

                await UniTask.Delay(250);
            }
        }

        /// <summary>Leaves the current session and clears local data.</summary>
        public void LeaveRoom()
        {
            ForceLeaveAndResetAsync(destroyManager: false).Forget();
        }

        #endregion
        /* ------------------------------------------------------------------ */
        #region INetworkRunnerCallbacks (only essential implementations)

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            bool isLocal = player == runner.LocalPlayer;
            bool isHost = runner.IsSharedModeMasterClient;

            if (!_players.ContainsKey(player))
                _players[player] = isLocal ? _pendingLocalData : default;

            if (isLocal && !isHost)
                SendBytesToHost(EncodePlayerInfo(player, _pendingLocalData));

            if (isLocal && isHost)
            {
                UILobby.Instance?.SetLeader();
                UILobby.Instance?.RefreshSlots();
            }

            _currentHost = isHost ? player : _currentHost;
            _isHostCached = isHost;

            if (isHost)
            {
                _startGuardUntil = Time.realtimeSinceStartup + 2f;
                UILobby.Instance?.TriggerStartCooldown(1.5f);

                if (_matchStarted && _matchBuildIndex >= 0)
                {
                    SendBytesToPlayer(player, EncodeMatchStart(_matchBuildIndex));
                }
            }
            int safeCount = CurrentPlayerCount;
            OnPlayerCountChanged?.Invoke(safeCount, CurrentMaxPlayers);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            _players.Remove(player);
            UILobby.Instance?.RefreshSlots();

            bool becameHost = runner.IsSharedModeMasterClient && !_isHostCached;
            _isHostCached = runner.IsSharedModeMasterClient;

            if (becameHost)
                PromoteToHost(runner);

            if (runner.IsSharedModeMasterClient)
                BroadcastFullSnapshot();

            int safeCount = CurrentPlayerCount; 
            OnPlayerCountChanged?.Invoke(safeCount, CurrentMaxPlayers);
        }


        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef sender,
                                    ReliableKey k, ArraySegment<byte> seg)
        {
            using var br = new BinaryReader(new MemoryStream(seg.Array!, seg.Offset, seg.Count));
            var tag = (LobbyMsg)br.ReadByte();

            if (tag == LobbyMsg.PlayerInfo)
            {
                var (who, data) = DecodePlayerInfo(br);
                bool iAmHost = runner.IsSharedModeMasterClient;

                if (iAmHost)
                {
                    _players[who] = data;
                    BroadcastFullSnapshot();
                }
                else
                {
                    _players[who] = data;
                }
                UILobby.Instance?.RefreshSlots();
            }
            else if (tag == LobbyMsg.MatchStart)
            {
                int buildIdx = br.ReadInt32();
                _waitingGameplayScene = true;
                _waitingRef = SceneRef.FromIndex(buildIdx);
                Debug.Log($"[PVP][Client] MatchStart recv. waitingGameplayScene=TRUE expected={_waitingRef.AsIndex}");
                LoadingManager.Singleton?.Open(0.2f);
                SendBytesToHost(EncodeMatchStartAck());
                return;
            }
            else if (tag == LobbyMsg.MatchStartAck)
            {
                if (runner.IsSharedModeMasterClient)
                    _matchAcks.Add(sender);
                return;
            }
            else if (tag == LobbyMsg.SceneReady)
            {
                if (runner.IsSharedModeMasterClient)
                    _sceneReadyAcks.Add(sender);
                return;
            }
        }
        
        public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> list)
        {
            _lastSessionList = list;
            _searchDone.Set();
        }

        public void OnSceneLoadStart(NetworkRunner r)
        {
            if (_waitingGameplayScene)
                LoadingManager.Singleton?.Open(1.2f);
        }

        /// <summary>
        /// Called by Fusion when a new scene has finished loading on this client.
        /// It initializes the PvP session, spawns the local character for both host and clients,
        /// waits for all players to signal readiness, assigns teams and enables PvP rules.
        /// </summary>
        public async void OnSceneLoadDone(NetworkRunner runner)
        {
            var activeBuildIndex = SceneManager.GetActiveScene().buildIndex;
            var expectedIndex = _waitingRef.IsValid ? _waitingRef.AsIndex : -1;
            bool isGameplayScene = (_waitingGameplayScene || (expectedIndex >= 0 && activeBuildIndex == expectedIndex));

            if (!isGameplayScene)
            {
                if (LoadingManager.Singleton && LoadingManager.Singleton.isLoading)
                    LoadingManager.Singleton.Close();
                return;
            }

            _waitingGameplayScene = false;
            _waitingRef = default;

            GameplayManager.Singleton?.SetPvpSession(isPvpGame);

            if (runner.IsSharedModeMasterClient)
            {
                try
                {
                    await SpawnCharacterAtStartPositionAsync(stagingSpawn: true);
                    await FastLocalPlaceAndCloseLoading(2.0f);

                    AssignTeamsAndReadyAsync().Forget();
                    PlaceAllByRpcAsync().Forget();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SceneDone][Host] Exception: {e}");
                }
                finally
                {
                    await UniTask.Delay(2000);
                    if (LoadingManager.Singleton && LoadingManager.Singleton.isLoading)
                        LoadingManager.Singleton.Close();
                }
                return;
            }

            // Client
            try
            {
                await SpawnCharacterAtStartPositionAsync(stagingSpawn: true);
                SendBytesToHost(EncodeSceneReady());

                await FastLocalPlaceAndCloseLoading(2.0f);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Spawn][Client] Exception: {e}");
            }
            finally
            {
                await UniTask.Delay(2000);
                if (LoadingManager.Singleton && LoadingManager.Singleton.isLoading)
                    LoadingManager.Singleton.Close();
            }
        }

        // Unused callbacks kept empty to satisfy the interface
        public void OnInput(NetworkRunner r, NetworkInput i) { }
        public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput i) { }
        public void OnShutdown(NetworkRunner r, ShutdownReason s)
        {
        }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner r) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason rsn) { }
        public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] token) { }
        public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason rsn) { }
        public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
        public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner r, HostMigrationToken t) { }
        public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey k, float progress) { }
        public void OnObjectEnterAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
        public void OnObjectExitAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }

        #endregion
        /* ------------------------------------------------------------------ */
        #region Private helpers

        private bool PrepareRunner()
        {
            if (_runner == null)
            {
                var runnerObj = new GameObject("NetworkRunner");
                runnerObj.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
                _runner = runnerObj.AddComponent<NetworkRunner>();
                _provider = runnerObj.AddComponent<PoolObjectProvider>();
                _runner.AddCallbacks(this);

                _players.Clear();
                _ref2idx.Clear();
                _isHostCached = false;
            }

            var charPrefab = GameInstance.Singleton?.characterEntity?.gameObject;
            if (!charPrefab)
            {
                Debug.LogError("[Fusion] Character prefab not configured in GameInstance.");
                return false;
            }
            if (!charPrefab.TryGetComponent<NetworkObject>(out _))
            {
                Debug.LogError($"[Fusion] Prefab '{charPrefab.name}' needs a NetworkObject.");
                return false;
            }
            return true;
        }

        private static bool IsValidNetworkPrefabAsset(GameObject go)
        {
            if (!go) return false;
            var no = go.GetComponent<NetworkObject>();
            if (!no) return false;
            return !no.IsValid;
        }

        private void PromoteToHost(NetworkRunner runner)
        {
            Debug.Log("<color=cyan>[Lobby] This client is the new Shared-Mode Master.</color>");
            if (masterClientTrackerPrefab && IsValidNetworkPrefabAsset(masterClientTrackerPrefab))
            {
                if (SharedModeMasterClientTracker.GetSharedModeMasterClientPlayerRef() == null)
                {
                    try
                    {
                        runner.Spawn(masterClientTrackerPrefab);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Lobby] Failed to spawn masterClientTrackerPrefab: {e}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[Lobby] masterClientTrackerPrefab is null.");
            }

            BroadcastFullSnapshot();
            UILobby.Instance?.SetLeader();
            UILobby.Instance?.RefreshSlots();
        }

        private async Task<SessionInfo> FindAnyAvailableSession()
        {
            var go = new GameObject("FusionQueryRunner");
            var queryRunner = go.AddComponent<NetworkRunner>();
            queryRunner.AddCallbacks(this);

            _searchDone.Reset();
            await queryRunner.JoinSessionLobby(SessionLobby.Shared);
            await Task.Run(() => _searchDone.WaitOne());

            var session = _lastSessionList.FirstOrDefault(s => s.PlayerCount < defaultMaxPlayers);

            Destroy(go); 
            return session.IsValid ? session : (SessionInfo)null;
        }

        private void EnsureRunnerClosed()
        {
            if (_runner != null)
            {
                _runner.Shutdown();
                Destroy(_runner.gameObject);
                _runner = null;
            }

        }

        private async Task<SessionInfo> FindAvailablePvpSession(string modeKey, int capacity)
        {
            GameObject go = null;
            try
            {
                go = new GameObject("FusionQueryRunner_PVP");
                var queryRunner = go.AddComponent<NetworkRunner>();
                queryRunner.AddCallbacks(this);

                _searchDone.Reset();
                await queryRunner.JoinSessionLobby(SessionLobby.Shared);

                await Task.Run(() => _searchDone.WaitOne());

                if (_lastSessionList == null || _lastSessionList.Count == 0)
                    return default; 

                foreach (var s in _lastSessionList)
                {
                    if (!s.IsValid) continue;
                    if (s.PlayerCount >= s.MaxPlayers) continue;

                    var props = s.Properties;
                    if (props == null || props.Count == 0) continue;

                    if (!props.TryGetValue("pvp", out var p)) continue;
                    if ((int)(SessionProperty)p != 1) continue;

                    bool sameMode = props.TryGetValue("mode", out var m) &&
                                    ((string)(SessionProperty)m) == modeKey;

                    bool sameCap = props.TryGetValue("cap", out var c) &&
                                   ((int)(SessionProperty)c) == capacity;

                    if (sameMode && sameCap)
                        return s; 
                }

                return default;
            }
            finally
            {
                if (go) Destroy(go);
            }
        }

        public byte ComputeTeamFor(PlayerRef pref)
        {
            // Use declared team count if available; if <=1, switch to FFA/BR (one team per player)
            int teamCount = 2;
            if (PvpSync.Instance != null && PvpSync.Instance.Object != null && PvpSync.Instance.Object.IsValid)
                teamCount = Mathf.Max(1, PvpSync.Instance.TeamCount);

            var ordered = _runner.ActivePlayers.OrderBy(p => p.RawEncoded).ToList();
            int idx = Mathf.Max(0, ordered.IndexOf(pref));

            if (teamCount <= 1)
                teamCount = ordered.Count; // FFA/BR: each player is its own team

            return (byte)(idx % teamCount);
        }

        private async UniTask AssignTeamsAndReadyAsync()
        {
            await UniTask.WaitUntil(() => PvpSync.Instance == null ||
                                          (PvpSync.Instance.Object != null && PvpSync.Instance.Object.IsValid));

            var players = _runner.ActivePlayers.OrderBy(p => p.RawEncoded).ToList();

            int teamCount = 2;
            if (PvpSync.Instance != null && PvpSync.Instance.Object && PvpSync.Instance.Object.IsValid)
                teamCount = Mathf.Max(1, PvpSync.Instance.TeamCount);

            if (teamCount <= 1)
                teamCount = players.Count; // FFA/BR

            foreach (var p in players)
            {
                NetworkObject obj = null;
                float tEnd = Time.realtimeSinceStartup + 1.5f; 
                while (Time.realtimeSinceStartup < tEnd)
                {
                    if (_runner.TryGetPlayerObject(p, out obj) && obj) break;
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
                if (!obj) continue;

                var ce = obj.GetComponent<CharacterEntity>();
                if (!ce) continue;

                byte team = (byte)(players.IndexOf(p) % teamCount);
                ce.RPC_AssignTeam(team);
            }

            if (PvpSync.Instance)
                PvpSync.Instance.MarkTeamsAssignedAndReady();
        }

        /// <summary>
        /// Finds the build index of a scene by its file name (without extension),
        /// no matter which folder it is located in. Returns -1 if not found.
        /// </summary>
        private static int GetBuildIndexBySceneName(string sceneName)
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; ++i)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string file = Path.GetFileNameWithoutExtension(path);
                if (file.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        private ReliableKey NextKey() => ReliableKey.FromInts(++_reliableSeq);

        /* Sends bytes to a specific peer (host or client).                          */
        private void SendBytesToPlayer(PlayerRef player, byte[] bytes)
        {
            _runner.SendReliableDataToPlayer(player, NextKey(), bytes);
        }

        private void SendBytesToHost(byte[] bytes)
        {
            // Robust way: use the tracker object if it already exists.
            PlayerRef? hostMaybe = SharedModeMasterClientTracker
                                   .GetSharedModeMasterClientPlayerRef();

            PlayerRef host = hostMaybe ??                      // tracker spawned?
                             _runner.ActivePlayers             // fallback: lowest id
                                    .OrderBy(p => p.RawEncoded)
                                    .First();

            // Defensive check – avoids sending to ourselves when we *are* the host.
            if (host == _runner.LocalPlayer)
                return;

            _runner.SendReliableDataToPlayer(host, NextKey(), bytes);
        }

        private async Task<SessionInfo> FindAvailableSession(string mapName)
        {
            var queryRunner = gameObject.AddComponent<NetworkRunner>();
            queryRunner.AddCallbacks(this);

            await queryRunner.JoinSessionLobby(SessionLobby.Shared);

            await Task.Run(() => _searchDone.WaitOne());

            foreach (var s in _lastSessionList)
            {
                if (s.Properties.TryGetValue("map", out var p)
                    && (string)p == mapName
                    && s.PlayerCount < defaultMaxPlayers)
                    return s;
            }
            Destroy(queryRunner.gameObject);
            return null;
        }
        private async UniTask WaitForAllPlayerInfos(int timeoutMs)
        {
            if (_runner == null) return;
            float end = Time.realtimeSinceStartup + (timeoutMs / 1000f);

            while (Time.realtimeSinceStartup < end)
            {
                bool allOk = true;
                foreach (var p in _runner.ActivePlayers)
                {
                    if (!_players.TryGetValue(p, out var d) || string.IsNullOrEmpty(d.playerName))
                    {
                        allOk = false;
                        break;
                    }
                }
                if (allOk) break;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private static string GenerateCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
            var rand = new System.Random();
            Span<char> buf = stackalloc char[6];
            for (int i = 0; i < buf.Length; ++i)
                buf[i] = chars[rand.Next(chars.Length)];
            return new string(buf);
        }

        #region Lobby-messages -----------------------------------
        private static byte[] EncodePlayerInfo(PlayerRef who, in PlayerData d)
        {
            using var ms = new MemoryStream(128);
            using var bw = new BinaryWriter(ms);

            bw.Write((byte)LobbyMsg.PlayerInfo);
            bw.Write(who.RawEncoded);
            bw.Write(d.playerName ?? "");
            bw.Write(d.playerFrame ?? "");
            bw.Write(d.playerIcon ?? "");
            bw.Write(d.characterId);
            bw.Write(d.characterSkin);
            bw.Write(d.characterMastery);
            return ms.ToArray();
        }

        private static (PlayerRef, PlayerData) DecodePlayerInfo(BinaryReader br)
        {
            int raw = br.ReadInt32();
            var pref = PlayerRef.FromEncoded(raw);

            var data = new PlayerData(
                br.ReadString(), br.ReadString(), br.ReadString(),
                br.ReadInt32(), br.ReadInt32(), br.ReadInt32());

            return (pref, data);
        }
        private static byte[] EncodeMatchStart(int buildIndex)
        {
            using var ms = new MemoryStream(16);
            using var bw = new BinaryWriter(ms);
            bw.Write((byte)LobbyMsg.MatchStart);
            bw.Write(buildIndex);
            return ms.ToArray();
        }

        private static byte[] EncodeMatchStartAck()
        {
            using var ms = new MemoryStream(8);
            using var bw = new BinaryWriter(ms);
            bw.Write((byte)LobbyMsg.MatchStartAck);
            return ms.ToArray();
        }

        private static byte[] EncodeSceneReady()
        {
            using var ms = new MemoryStream(8);
            using var bw = new BinaryWriter(ms);
            bw.Write((byte)LobbyMsg.SceneReady);
            return ms.ToArray();
        }

        private void BroadcastFullSnapshot()
        {
            foreach (var kv in _players)
            {
                byte[] msg = EncodePlayerInfo(kv.Key, kv.Value);
                foreach (var p in _runner.ActivePlayers)
                    if (p != _runner.LocalPlayer)
                        SendBytesToPlayer(p, msg);
            }
        }

        /// <summary>
        /// Instantiates the local player's character at the appropriate spawn location.
        /// In PvP modes it uses the team spawn positions defined in GameplayManager; in coop it defaults to the origin.
        /// The method waits until the GameplayManager is available instead of skipping the spawn.
        /// </summary>
        public async Task SpawnCharacterAtStartPositionAsync(bool stagingSpawn = false)
        {
            if (_runner == null)
            {
                Debug.LogError("[Spawn] NetworkRunner not initialized.");
                return;
            }
            if (_runner.TryGetPlayerObject(_runner.LocalPlayer, out _))
                return;

            GameObject prefab = GameInstance.Singleton.characterEntity?.gameObject;
            if (prefab == null)
            {
                Debug.LogError("[Spawn] Character prefab null in GameInstance.");
                return;
            }

            byte cid = (byte)_pendingLocalData.characterId;
            byte skin = (byte)_pendingLocalData.characterSkin;
            string nick = _pendingLocalData.playerName;

            byte team = ComputeTeamFor(_runner.LocalPlayer);

            GameplayManager gm = GameplayManager.Singleton;
            float elapsed = 0f;
            while (gm == null && elapsed < 5f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                elapsed += Time.deltaTime;
                gm = GameplayManager.Singleton;
            }

            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            if (!stagingSpawn && gm != null && gm.IsPvp)
            {
                spawnPos = gm.GetTeamSpawnPosition(team);
                spawnRot = gm.GetTeamSpawnRotation(team);
            }

            NetworkObject netObj = await _runner.SpawnAsync(
                prefab,
                spawnPos,
                spawnRot,
                _runner.LocalPlayer,
                (runner, obj) =>
                {
                    var ce = obj.GetComponent<CharacterEntity>();
                    ce.SetInitialNetworkData(cid, skin, nick, team);
                });

            if (netObj == null)
            {
                Debug.LogError("[Spawn] SpawnAsync returned null!");
                return;
            }

            // If staging, disable attack until placement RPC arrives
            if (stagingSpawn)
            {
                var ceStage = netObj.GetComponent<CharacterEntity>();
                if (ceStage)
                {
                    ceStage.CharacterAttackComponent?.StopAutoAttack();
                    ceStage.CharacterControllerComponent?.StopMovement(false);
                }
            }

            _runner.SetPlayerObject(_runner.LocalPlayer, netObj);

            if (netObj.HasInputAuthority)
            {
                var ce = netObj.GetComponent<CharacterEntity>();
                if (ce != null)
                {
                    var data = GameInstance.Singleton.GetCharacterDataById(cid);
                    ce.SetCharacterData(data, skin);
                }

                if (UIGameplay.Singleton == null)
                {
                    var hud = UnityEngine.Object.Instantiate(GameInstance.Singleton.GetUIGameplayForPlatform());
                    if (hud.minimapImage) hud.minimapImage.sprite = hud.unlockedSprite;
                }
            }
        }

        //dispatch placement to each owner (InputAuthority) so Shared mode respects authority.
        private async UniTask PlaceAllByRpcAsync()
        {
            if (_runner == null) return;

            var gm = GameplayManager.Singleton;
            if (!gm) return;

            var players = _runner.ActivePlayers.OrderBy(p => p.RawEncoded).ToList();

            int teamCount = 2;
            if (PvpSync.Instance != null && PvpSync.Instance.Object && PvpSync.Instance.Object.IsValid)
                teamCount = Mathf.Max(1, PvpSync.Instance.TeamCount);
            if (teamCount <= 1)
                teamCount = players.Count; // FFA/BR

            foreach (var p in players)
            {
                if (!_runner.TryGetPlayerObject(p, out var obj) || !obj) continue;
                var ce = obj.GetComponent<CharacterEntity>();
                if (!ce) continue;

                byte team = (byte)(players.IndexOf(p) % teamCount);
                ce.RPC_RequestPlaceAtTeamSpawn(team);
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        private async UniTask FastLocalPlaceAndCloseLoading(double maxWaitSeconds)
        {
            var deadline = Time.realtimeSinceStartup + (float)maxWaitSeconds;

            while (Time.realtimeSinceStartup < deadline)
            {
                if (TryGetLocalCharacterEntity(out var ce) && GameplayManager.Singleton != null)
                {
                    byte team = ComputeTeamFor(_runner.LocalPlayer);
                    ce.RPC_RequestPlaceAtTeamSpawn(team);

                    float end = Time.realtimeSinceStartup + 2f;
                    while (Time.realtimeSinceStartup < end)
                    {
                        if (LoadingManager.Singleton && !LoadingManager.Singleton.isLoading) return;
                        await UniTask.Yield(PlayerLoopTiming.Update);
                    }
 
                    if (LoadingManager.Singleton && LoadingManager.Singleton.isLoading)
                        LoadingManager.Singleton.Close();
                    return;
                }
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // Fallback: close loading even if we couldn't place (avoid stuck screens)
            if (LoadingManager.Singleton && LoadingManager.Singleton.isLoading)
                LoadingManager.Singleton.Close();
        }

        private bool TryGetLocalCharacterEntity(out CharacterEntity ce)
        {
            ce = null;
            if (_runner == null) return false;
            if (!_runner.TryGetPlayerObject(_runner.LocalPlayer, out var obj) || !obj) return false;
            ce = obj.GetComponent<CharacterEntity>();
            return ce != null;
        }

        #endregion

        /// <summary>
        /// Ends the current network session, shuts down the runner and cleans up.
        /// </summary>
        public void EndGameSession()
        {
            ForceLeaveAndResetAsync(destroyManager: false).Forget();
        }
        private async UniTaskVoid ForceLeaveAndResetAsync(bool destroyManager = false)
        {
            try
            {
                _matchAcks.Clear();
                _sceneReadyAcks.Clear();

                if (_runner != null)
                {
                    try
                    {
                        if (_runner.IsRunning)
                            await _runner.Shutdown();
                    }
                    catch { /* error shutdown */ }

                    if (_runner)
                    {
                        _runner.RemoveCallbacks(this);
                        Destroy(_runner.gameObject);
                    }
                    _runner = null;
                }

#if UNITY_6000_0_OR_NEWER
                foreach (var r in UnityEngine.Object.FindObjectsByType<NetworkRunner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
#else
                foreach (var r in UnityEngine.Object.FindObjectsOfType<NetworkRunner>(true))
#endif
                {
                    try { if (r.IsRunning) await r.Shutdown(); } catch { }
                    Destroy(r.gameObject);
                }

                KillIfAlive<GameplaySync>();
                KillIfAlive<PvpSync>();
                KillIfAlive<SharedModeMasterClientTracker>();

#if UNITY_6000_0_OR_NEWER
                foreach (var p in UnityEngine.Object.FindObjectsByType<PvpSync>(FindObjectsInactive.Include, FindObjectsSortMode.None))
#else
                foreach (var p in UnityEngine.Object.FindObjectsOfType<PvpSync>(true))
#endif
                    if (p) Destroy(p.gameObject);

#if UNITY_6000_0_OR_NEWER
                foreach (var g in UnityEngine.Object.FindObjectsByType<GameplaySync>(FindObjectsInactive.Include, FindObjectsSortMode.None))
#else
                foreach (var g in UnityEngine.Object.FindObjectsOfType<GameplaySync>(true))
#endif
                    if (g) Destroy(g.gameObject);

                ResetLocalState();

                if (destroyManager)
                {
                    if (Instance == this) Instance = null;
                    Destroy(gameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[FusionCleanup] Cleanup Exception: {e}");
            }
        }

        private static void KillIfAlive<T>() where T : Component
        {
            var instField = typeof(T).GetProperty("Instance",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (instField != null)
            {
                var inst = instField.GetValue(null) as Component;
                if (inst && inst.gameObject) UnityEngine.Object.Destroy(inst.gameObject);
            }
        }

        private void ResetLocalState()
        {
            _players.Clear();
            _ref2idx.Clear();
            _currentSession = null;
            _selectedMap = null;
            _waitingGameplayScene = false;
            _waitingRef = default;
            _matchStarted = false;
            _matchBuildIndex = -1;
            _isHostCached = false;
            _currentHost = default;
            CurrentMaxPlayers = 0;
            CurrentPvpModeKey = null;
            isPvpGame = false;
            OnPlayerCountChanged = null;
            _pendingLocalData = default;
            _reliableSeq = 0;
            _searchDone.Reset();
        }

        #endregion
        /* ------------------------------------------------------------------ */
        #region Public getters

        public (PlayerRef, PlayerData)[] OrderedPlayers() => _players.OrderBy(kv => kv.Key.RawEncoded).Select(kv => (kv.Key, kv.Value)).ToArray();
        public IReadOnlyList<string> ConnectedNicknames => _players.Values.Select(p => p.playerName).ToList();
        public string CurrentSessionCode => _currentSession;
        public string SelectedMap => _selectedMap;
        static int Id(PlayerRef p) => p.RawEncoded;
        #endregion
#endif
    }

    /// <summary>Lightweight struct holding public player data.</summary>
    [Serializable]
    public struct PlayerData
    {
        public string playerName;
        public string playerFrame;
        public string playerIcon;
        public int characterId;
        public int characterSkin;
        public int characterMastery;

        public PlayerData(string _nickname, string _frame, string _icon, int _character, int _skin, int _mastery)
        {
            playerName = _nickname;
            playerFrame = _frame;
            playerIcon = _icon;
            characterId = _character;
            characterSkin = _skin;
            characterMastery = _mastery;
        }
    }
}