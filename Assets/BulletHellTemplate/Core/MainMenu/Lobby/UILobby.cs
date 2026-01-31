#if FUSION2
using Fusion;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using BulletHellTemplate.PVP;
using UnityEngine.TextCore.Text;

namespace BulletHellTemplate
{
    /// <summary>
    /// Lobby UI (Create / Join / Auto-Join) +  PVP (select mode and matchmaking).
    /// </summary>
    public class UILobby : MonoBehaviour
    {
        /* ─────────────────────────  Inspector References  ───────────────────────── */
        [Header("Root Selection (opcional)")]
        [Tooltip("Se atribuído, a cena inicia neste painel com os botões COOP e PVP.")]
        public GameObject modeSelectMenu;

        [Header("Menus COOP")]
        [Tooltip("Root container para o menu Join-or-Create.")]
        public GameObject joinOrCreateMenu;
        public GameObject createMenu;
        public GameObject roomMenu;

        [Header("Cena")]
        public GameObject cameraLobby;

        [Header("Join / Create")]
        public TMP_InputField roomNameInput;
        public Button joinRoomButton;
        public Button autoJoinRoomButton;
        public Button createRoomMenuButton;
        public GameObject joinErrorPopup;
        public TextMeshProUGUI joinErrorText;

        [Header("Create Room")]
        public CreateRoomMapEntry createRoomMapEntry;
        public Transform createRoomMapContainer;
        public GameObject creatingRoomPopup;
        public Button createRoomGameButton;

        [Header("Room UI")]
        public PlayerLobbySlot[] playerSlots;
        public Sprite emptySlotImage;
        public TextMeshProUGUI roomIdText;
        public Image selectedMapIcon;
        public TextMeshProUGUI selectedMapName;
        public Button startGameButton;

        [Header("PVP Menus")]
        public GameObject pvpRootMenu;              
        public Transform pvpModeContainer;            
        public PvpModeEntryUI pvpModeEntryPrefab;

        [Header("PVP - Actions")]
        public Button openPvpMenuButton;             
        public Button openCoopMenuButton;             
        public Button startPvpMatchmakingButton;
        public Button cancelPvpMatchmakingButton;

        [Header("PVP - Searching")]
        public GameObject searchingPanel;
        public TextMeshProUGUI searchingStatus;
        public TextMeshProUGUI searchingTimer;

        [Header("Start Cooldown")]
        public Image startCooldownFill;
        public float defaultStartCooldown = 5f;

        [Header("Status")]
        public TextMeshProUGUI statusLabel;

        private PvpModeData _selectedPvp;
        private CancellationTokenSource _mmCts;

        private readonly System.Collections.Generic.List<CreateRoomMapEntry> _mapEntries =
            new System.Collections.Generic.List<CreateRoomMapEntry>();

        private CancellationTokenSource _joinErrCts;

        /* ─────────────────────────────  Runtime State  ───────────────────────────── */
        private CancellationTokenSource _startCooldownCts;
        private bool _startCooldownActive = false;
        private int selectedMapId;
        private string currentRoomId;
        private bool isLeader;

        public static UILobby Instance { get; private set; }

        [Serializable]
        public struct PlayerLobbySlot
        {
            public Transform playerModelSlot;
            public TextMeshProUGUI playerNameLabel;
            public Image playerIcon;
            public Image playerFrame;
            public Image characterMastery;
        }


        /* ───────────────────────────────  Life-cycle  ────────────────────────────── */
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            joinRoomButton.onClick.AddListener(OnJoinRoom);
            autoJoinRoomButton.onClick.AddListener(OnAutoJoinRoom);
            createRoomMenuButton.onClick.AddListener(OnPressCreateMenu);
            createRoomGameButton.onClick.AddListener(OnCreateRoom);
            startGameButton.onClick.AddListener(OnStartGame);

            // PVP
            if (openPvpMenuButton) openPvpMenuButton.onClick.AddListener(OnOpenPvpMenu);
            if (openCoopMenuButton) openCoopMenuButton.onClick.AddListener(OnOpenCoopMenu);
            if (startPvpMatchmakingButton) startPvpMatchmakingButton.onClick.AddListener(OnStartPvpMatchmaking);
            if (cancelPvpMatchmakingButton) cancelPvpMatchmakingButton.onClick.AddListener(CancelPvpMatchmaking);
        }

        private void OnEnable()
        {
            if (cameraLobby) cameraLobby.SetActive(true);

            EnableStartButton(false);
            SetStatus("Idle");
            RefreshSlots();

            if (modeSelectMenu)
                OnBackToModeSelect();
            else
                OnOpenCoopMenu();
        }

        public void OnDisable()
        {
            if (cameraLobby) cameraLobby.SetActive(false);
        }
    
        /* ───────────────────────────────  Navegate  ─────────────────────────────── */

        public void OnBackToModeSelect()
        {
            if (modeSelectMenu) modeSelectMenu.SetActive(true);

            joinOrCreateMenu?.SetActive(false);
            createMenu?.SetActive(false);
            roomMenu?.SetActive(false);
            pvpRootMenu?.SetActive(false);
            searchingPanel?.SetActive(false);

            SetStatus("Select COOP or PVP");
        }

        public void OnOpenCoopMenu()
        {
            if (modeSelectMenu) modeSelectMenu.SetActive(false);

            pvpRootMenu?.SetActive(false);
            searchingPanel?.SetActive(false);
            createMenu?.SetActive(false);
            roomMenu?.SetActive(false);

            joinOrCreateMenu?.SetActive(true);
            SetStatus("COOP: Join or Create");
        }

        public void OnBackFromCreateToCoop() => OnOpenCoopMenu();

        public void OnBackFromRoomToCoop()
        {
#if FUSION2
            FusionLobbyManager.Instance?.LeaveRoom();
#endif
            OnOpenCoopMenu();
        }

        public void OnOpenPvpMenu()
        {
            if (modeSelectMenu) modeSelectMenu.SetActive(false);

            joinOrCreateMenu?.SetActive(false);
            createMenu?.SetActive(false);
            roomMenu?.SetActive(false);
            searchingPanel?.SetActive(false);

            pvpRootMenu?.SetActive(true);
            PopulatePvpEntries();
            SetStatus("Select a PVP mode");
        }

        public void OnBackFromPvpToModeSelect() => OnBackToModeSelect();

        public void OnBackFromSearchingToPvp() => CancelPvpMatchmaking();

        /* ───────────────────────────────  COOP  ─────────────────────────────── */

        public void OnPressCreateMenu()
        {
            joinOrCreateMenu.SetActive(false);
            createMenu.SetActive(true);
            PopulateCreateRoomMaps();
            SetStatus("Select a map");
        }

        public void OnCreateRoom()
        {
            creatingRoomPopup.SetActive(true);

            string nickname = PlayerSave.GetPlayerName();
            string playerFrame = PlayerSave.GetPlayerFrame();
            string playerIcon = PlayerSave.GetPlayerIcon();
            int characterId = PlayerSave.GetSelectedCharacter();
            int characterSkin = PlayerSave.GetCharacterSkin(characterId);
            int characterMastery = PlayerSave.GetCharacterMasteryLevel(characterId);

            PlayerData data = new PlayerData(nickname, playerFrame, playerIcon, characterId, characterSkin, characterMastery);
#if FUSION2
            FusionLobbyManager.Instance.CreateRoom(
                GameInstance.Singleton.GetMapInfoDataById(selectedMapId).scene,
                data,
                () =>
                {
                    currentRoomId = FusionLobbyManager.Instance.CurrentSessionCode;
                    isLeader = true;
                    SetupRoomUI(selectedMapId, currentRoomId);
                });
#endif
        }

        public void OnJoinRoom()
        {
            string code = roomNameInput.text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code)) return;

            string nickname = PlayerSave.GetPlayerName();
            string playerFrame = PlayerSave.GetPlayerFrame();
            string playerIcon = PlayerSave.GetPlayerIcon();
            int characterId = PlayerSave.GetSelectedCharacter();
            int characterSkin = PlayerSave.GetCharacterSkin(characterId);
            int characterMastery = PlayerSave.GetCharacterMasteryLevel(characterId);

            PlayerData data = new PlayerData(nickname, playerFrame, playerIcon, characterId, characterSkin, characterMastery);

            SetStatus("Connecting …");

#if FUSION2
            FusionLobbyManager.Instance.JoinRoom(
                code,
                data,
                onJoined: () =>
                {
                    currentRoomId = code;
                    isLeader = false;
                    joinOrCreateMenu.SetActive(false);
                    roomMenu.SetActive(true);
                    SetStatus("Connected");
                    RefreshSlots();
                },
                onFailed: reason =>
                {
                    joinOrCreateMenu.SetActive(true);
                    roomMenu.SetActive(false);
                    SetStatus("Idle");
                    ShowJoinErrorAsync("Room does not exist").Forget();
                });
#endif
        }

        private void OnAutoJoinRoom()
        {
            string nickname = PlayerSave.GetPlayerName();
            string playerFrame = PlayerSave.GetPlayerFrame();
            string playerIcon = PlayerSave.GetPlayerIcon();
            int characterId = PlayerSave.GetSelectedCharacter();
            int characterSkin = PlayerSave.GetCharacterSkin(characterId);
            int characterMastery = PlayerSave.GetCharacterMasteryLevel(characterId);

            PlayerData data = new PlayerData(nickname, playerFrame, playerIcon, characterId, characterSkin, characterMastery);

            SetStatus("Searching rooms …");
#if FUSION2
            FusionLobbyManager.Instance.AutoJoinRoom(
                data,
                onJoined: (code) =>
                {
                    currentRoomId = code;
                    isLeader = false;
                    joinOrCreateMenu.SetActive(false);
                    roomMenu.SetActive(true);
                    SetStatus("Connected");
                    RefreshSlots();
                },
                onFailed: reason =>
                {
                    ShowJoinErrorAsync("No room available").Forget();
                    SetStatus("Idle");
                });
#endif
        }

        /* ───────────────────────────  Thumbnails (Create)  ───────────────────── */
        public void PopulateCreateRoomMaps()
        {
            // limpa
            if (createRoomMapContainer)
                foreach (Transform child in createRoomMapContainer)
                    Destroy(child.gameObject);

            _mapEntries.Clear();

            // guards + logs
            if (!createRoomMapEntry)
            {
                Debug.LogWarning("[UILobby] createRoomMapEntry não atribuído no Inspector.");
                return;
            }
            if (!createRoomMapContainer)
            {
                Debug.LogWarning("[UILobby] createRoomMapContainer não atribuído no Inspector.");
                return;
            }
            if (GameInstance.Singleton == null || GameInstance.Singleton.mapInfoData == null)
            {
                Debug.LogWarning("[UILobby] GameInstance ou mapInfoData está vazio/nulo.");
                return;
            }
            var maps = GameInstance.Singleton.mapInfoData;
            if (maps.Length == 0)
            {
                Debug.LogWarning("[UILobby] Nenhum MapInfoData configurado em GameInstance.mapInfoData.");
                return;
            }

            int firstMapId = maps[0].mapId;
            foreach (var info in maps)
            {
                var entry = Instantiate(createRoomMapEntry, createRoomMapContainer);
                bool selected = info.mapId == firstMapId;
                entry.Setup(info.mapId, info.mapPreviewImage, info.mapName, selected);
                _mapEntries.Add(entry);
            }

            selectedMapId = firstMapId;
            Debug.Log($"[UILobby] populate {maps.Length} mapas. Selecionado: {selectedMapId}");
        }

        public void SetCreateRoomMap(int id)
        {
            selectedMapId = id;
            foreach (var e in _mapEntries)
                e.SetSelected(e.MapId == id);
        }

        /* ───────────────────────────────  PVP  ─────────────────────────────── */

        private void PopulatePvpEntries()
        {
            foreach (Transform c in pvpModeContainer) Destroy(c.gameObject);
            foreach (var mode in GameInstance.Singleton.pvpModes)
            {
                if (!mode) continue;
                var e = Instantiate(pvpModeEntryPrefab, pvpModeContainer);
                e.Setup(mode, SelectPvpMode);
            }
            _selectedPvp = GameInstance.Singleton.pvpModes != null && GameInstance.Singleton.pvpModes.Length > 0
                         ? GameInstance.Singleton.pvpModes[0] : null;
            if (startPvpMatchmakingButton) startPvpMatchmakingButton.interactable = _selectedPvp != null;
        }

        private void SelectPvpMode(PvpModeData data)
        {
            _selectedPvp = data;
            if (startPvpMatchmakingButton) startPvpMatchmakingButton.interactable = _selectedPvp != null;
            SetStatus($"Battle: {_selectedPvp?.battleName}");
        }

        private void OnStartPvpMatchmaking()
        {
            if (_selectedPvp == null) return;

            pvpRootMenu?.SetActive(false);
            searchingPanel?.SetActive(true);
            searchingStatus?.SetText("Looking for players…");

            _mmCts?.Cancel();
            _mmCts = new();

            string nickname = PlayerSave.GetPlayerName();
            string playerFrame = PlayerSave.GetPlayerFrame();
            string playerIcon = PlayerSave.GetPlayerIcon();
            int    characterId = PlayerSave.GetSelectedCharacter();
            int    characterSkin = PlayerSave.GetCharacterSkin(characterId);
            int    characterMastery = PlayerSave.GetCharacterMasteryLevel(characterId);
            var    data = new PlayerData(nickname, playerFrame, playerIcon, characterId, characterSkin, characterMastery);

            SearchingTimerAsync(_mmCts.Token).Forget();
#if FUSION2
            FusionLobbyManager.Instance.StartPvpMatchmaking(
                _selectedPvp, data,
                onJoinedOrCreated: code =>
                {
                    currentRoomId = code;
                    FusionLobbyManager.Instance.OnPlayerCountChanged -= OnPvpCountChanged;
                    FusionLobbyManager.Instance.OnPlayerCountChanged += OnPvpCountChanged;
                    OnPvpCountChanged(FusionLobbyManager.Instance.CurrentPlayerCount,
                                      FusionLobbyManager.Instance.CurrentMaxPlayers);
                },
                onFailed: reason =>
                {
                    _mmCts?.Cancel();
                    searchingStatus?.SetText($"Failed: {reason}");
                    UniTask.Void(async () =>
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(1.2));
                        OnOpenPvpMenu();
                    });
                });
#endif
        }

        private void OnPvpCountChanged(int count, int capacity)
        {
            if (searchingStatus)
                searchingStatus.text = $"{count}/{capacity}";
        }

        private void CancelPvpMatchmaking()
        {
            _mmCts?.Cancel();
            searchingPanel?.SetActive(false);
            SetStatus("Canceled");
#if FUSION2
            FusionLobbyManager.Instance.LeaveRoom();
#endif
            OnOpenPvpMenu();
        }

        private async UniTaskVoid SearchingTimerAsync(System.Threading.CancellationToken ct)
        {
            var t0 = Time.realtimeSinceStartup;
            while (!ct.IsCancellationRequested)
            {
                var secs = Mathf.FloorToInt(Time.realtimeSinceStartup - t0);
                if (searchingTimer) searchingTimer.text = $"{secs:00}s";
                await UniTask.Delay(200, cancellationToken: ct);
            }
        }

        /* ───────────────────────────────  Player Slots  ─────────────────────────── */
        public void RefreshSlots()
        {
#if FUSION2
            var players = FusionLobbyManager.Instance.OrderedPlayers();
            int connected = players.Count();

            roomIdText.text = currentRoomId ?? string.Empty;

            for (int slotIdx = 0; slotIdx < playerSlots.Length; slotIdx++)
            {
                ref var slotUI = ref playerSlots[slotIdx];  // alias

                bool filled = slotIdx < connected;

                if (slotUI.playerModelSlot != null)
                    slotUI.playerModelSlot.gameObject.SetActive(filled);

                if (filled)
                {
                    var data = players[slotIdx].Item2;  // PlayerData

                    if (slotUI.playerNameLabel != null)
                        slotUI.playerNameLabel.text = data.playerName;

                    if (slotUI.playerIcon != null)
                        slotUI.playerIcon.sprite = GetPlayerIcon(data.playerIcon) ?? emptySlotImage;

                    if (slotUI.playerFrame != null)
                        slotUI.playerFrame.sprite = GetPlayerFrame(data.playerFrame) ?? emptySlotImage;

                    if (slotUI.characterMastery != null)
                    {
                        var m = GameInstance.Singleton.GetMasteryLevel(data.characterMastery);
                        slotUI.characterMastery.sprite = m.masteryIcon ?? emptySlotImage;
                    }

                    if (slotUI.playerModelSlot != null)
                    {
                        foreach (Transform c in slotUI.playerModelSlot) Destroy(c.gameObject);

                        CharacterModel prefab = null;
                        var cd = GameInstance.Singleton.GetCharacterDataById(data.characterId);

                        if (cd != null)
                        {
                            int skinIdx = data.characterSkin;
                            if (cd.characterSkins != null &&
                                skinIdx >= 0 &&
                                skinIdx < cd.characterSkins.Length &&
                                cd.characterSkins[skinIdx].skinCharacterModel != null)
                                prefab = cd.characterSkins[skinIdx].skinCharacterModel;
                            else
                                prefab = cd.characterModel;
                        }

                        if (prefab != null)
                            Instantiate(prefab, slotUI.playerModelSlot).transform.localPosition = Vector3.zero;
                    }
                }
                else
                {
                    if (slotUI.playerNameLabel != null)
                        slotUI.playerNameLabel.text = "Waiting…";

                    if (slotUI.playerIcon != null) slotUI.playerIcon.sprite = emptySlotImage;
                    if (slotUI.playerFrame != null) slotUI.playerFrame.sprite = emptySlotImage;
                    if (slotUI.characterMastery != null) slotUI.characterMastery.sprite = emptySlotImage;

                    if (slotUI.playerModelSlot != null)
                        foreach (Transform c in slotUI.playerModelSlot) Destroy(c.gameObject);
                }
            }

            EnableStartButton(isLeader && connected > 0 && !_startCooldownActive);
            SetStatus($"{connected}/{playerSlots.Length}");
#endif
        }

        /* ─────────────────────────────  Start / Leave  ──────────────────────────── */
        public void OnStartGame()
        {
            if (!isLeader) return;
#if FUSION2
            FusionLobbyManager.Instance.StartMatch();
#endif
            SetStatus("Loading battle scene …");
        }

        public void SetLeader()
        {
            isLeader = true;
        }

        public void EnableStartButton(bool value)
        {
            startGameButton.interactable = value && !_startCooldownActive;
        }

        public void OnLeaveRoom()
        {
#if FUSION2
            FusionLobbyManager.Instance.LeaveRoom();
#endif
            roomMenu.gameObject.SetActive(false);
            createMenu.gameObject.SetActive(false);
            joinOrCreateMenu.gameObject.SetActive(true);
        }

        /* ───────────────────────────────  Helpers  ─────────────────────────────── */
        private void SetStatus(string msg) { if (statusLabel) statusLabel.text = msg; }

        private void SetupRoomUI(int mapId, string roomCode)
        {
            var map = GameInstance.Singleton.GetMapInfoDataById(mapId);
            selectedMapIcon.sprite = map.mapPreviewImage;
            selectedMapName.text = map.mapName;
            roomIdText.text = $"Room: {roomCode}";
            joinOrCreateMenu.SetActive(false);
            createMenu.SetActive(false);
            creatingRoomPopup.SetActive(false);
            roomMenu.SetActive(true);
            RefreshSlots();
        }

        public void TriggerStartCooldown(float seconds)
        {
            if (!isLeader) return;
            _startCooldownCts?.Cancel();
            _startCooldownCts = new CancellationTokenSource();
            StartCooldownRoutine(seconds <= 0f ? defaultStartCooldown : seconds, _startCooldownCts.Token).Forget();
        }

        private async UniTaskVoid StartCooldownRoutine(float seconds, CancellationToken ct)
        {
            _startCooldownActive = true;
            EnableStartButton(false);

            if (startCooldownFill)
            {
                startCooldownFill.gameObject.SetActive(true);
                startCooldownFill.type = Image.Type.Filled;
                startCooldownFill.fillAmount = 1f;
            }

            float t0 = Time.unscaledTime;
            float end = t0 + seconds;

            while (!ct.IsCancellationRequested && Time.unscaledTime < end)
            {
                float rem = end - Time.unscaledTime;
                if (startCooldownFill) startCooldownFill.fillAmount = Mathf.Clamp01(rem / seconds);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                if (startCooldownFill) startCooldownFill.gameObject.SetActive(false);
                _startCooldownActive = false;
                RefreshSlots();
            }
        }

        private async UniTaskVoid ShowJoinErrorAsync(string msg)
        {
            joinErrorText.text = msg;
            joinErrorPopup.SetActive(true);

            _joinErrCts?.Cancel();
            _joinErrCts = new CancellationTokenSource();
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: _joinErrCts.Token);
            }
            catch { }
            finally
            {
                joinErrorPopup.SetActive(false);
            }
        }

        private Sprite GetPlayerIcon(string iconId)
        {
            foreach (IconItem item in GameInstance.Singleton.iconItems)
            {
                if (item.iconId == iconId) return item.icon;
            }
            return null;
        }

        private Sprite GetPlayerFrame(string frameId)
        {
            foreach (FrameItem item in GameInstance.Singleton.frameItems)
            {
                if (item.frameId == frameId) return item.icon;
            }
            return null;
        }
    }
}

