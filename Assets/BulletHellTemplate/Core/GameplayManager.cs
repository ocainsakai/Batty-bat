using BulletHellTemplate.Core.Events;
using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;


#if FUSION2
using Fusion; 
#endif

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the gameplay, including the survival timer, spawning waves of monsters, handling perk levels, and pooling system.
    /// </summary>
    /// 
    public class GameplayManager : MonoBehaviour
    {
         // The selected win condition for this game
        [Header("Revive")]
        public int reviveSeconds = 20;
        public int reviveLimit = 1;
        [Header("Game Mode")]
        public WinCondition winCondition;
        public UpgradeMode upgradeMode = UpgradeMode.UpgradeOnLevelUp;
        public MonsterEntity bossPrefab; // The boss to spawn in KillBoss or SurvivalTimeAndKillBoss modes
        public int survivalTime = 600; // Survival time in seconds (10 minutes)
        public List<Wave> waves; // List of configured waves     

        [Header("Game EXP settings")]
        public int[] xpToNextLevel;
        public int maxLevel;
        public int minDamage = 5;
        public TimeFormat timeDisplayFormat = TimeFormat.Seconds;

        [Header("Perks Settings")]
        public int maxLevelStatPerks = 5;
        public int maxLevelSkillPerks = 5;
        public int maxSkills = 5;
        public int maxStats = 5;
        public SkillPerkData[] skillPerkData;
        public StatPerkData[] statPerkData;

        [Header("Monster Spawn")]
        public List<SpawnPoints> spawnPointsList;
        private CharacterEntity character;

        [Header("PVP Spawn")]
        public Transform[] teamSpawnSlots;
        [Range(0f, 3f)] public float teamSpawnSpreadRadius = 1.5f;

        [Header("Pool Settings (not recommended for mobile)")]
        public bool PreloadAllMonsters;

        //(PVP WIP)
        private bool isPvp = false;
        public bool IsPvp => isPvp;

        private Dictionary<object, int> perkLevels = new Dictionary<object, int>();
        private Dictionary<SkillPerkData, int> skillLevels = new Dictionary<SkillPerkData, int>();
        private Dictionary<SkillData, int> skillBaseLevels = new Dictionary<SkillData, int>();
        private int currentWaveIndex = 0; // Index of the current wave
        private float timeRemaining; // Time remaining for the next wave or win condition
        private bool gameRunning = true; // Control the game state
        private bool isPaused = false; // Tracks if the game is paused
        private float waveStartTime; // Time when the current wave started
        private int monstersKilled; // Tracks the number of monsters killed
        private int goldGain; // Tracks the total gold gained
        private int xpGain; // Tracks the total XP gained

        public static GameplayManager Singleton { get; private set; }

        public readonly HashSet<Transform> ActiveCharactersList = new();
        public readonly List<Transform> ActiveMonstersList = new();
        public readonly List<Transform> ActiveBoxesList = new();
        private readonly HashSet<MonsterEntity> _finalBosses = new();
        private bool _bossSeenAliveOnce = false;
        private readonly HashSet<CharacterEntity> _knownCharacters = new();
        private bool _rosterReady = false;

        //PVP
        private readonly Dictionary<int, int> _teamScore = new();

        /// <summary>
        /// Stores currently alive player characters for wipe checks.
        /// </summary>
        private readonly HashSet<CharacterEntity> _aliveCharacters = new HashSet<CharacterEntity>();
        private readonly Dictionary<CharacterEntity, CancellationTokenSource> _reviveTimers = new();

        /// <summary>
        /// True when the Fusion runner is up (network match).
        /// </summary>
        public bool IsRunnerActive =>
#if FUSION2
            GameplaySync.Instance && GameplaySync.Instance.RunnerActive;
#else
            false;
#endif

        /// <summary>
        /// True only on the Shared‑Mode master (the one allowed to decide team defeat).
        /// </summary>
        public bool IsLeader =>
#if FUSION2
            IsRunnerActive && GameplaySync.Instance && GameplaySync.Instance.HasStateAuthority;
#else
            true;
#endif

        private bool matchFinished = false;

        //Cancel Tokens
        private CancellationTokenSource _survivalCts;
        private CancellationTokenSource _waveCts;
        private CancellationTokenSource _bossCheckCts;
        /* ─────────────────────────── Helpers ─────────────────────────── */

#if FUSION2
        /// <summary>
        /// Returns true when this client is the Shared‑Mode Master Client
        /// otherwise, false.
        /// </summary>
        public bool IsHost()
        {
            NetworkRunner runner = NetworkRunner.GetRunnerForGameObject(gameObject);

#if UNITY_6000_0_OR_NEWER
            if (runner == null)
                runner = FindFirstObjectByType<NetworkRunner>();
#else
            if (runner == null)
                runner = FindObjectOfType<NetworkRunner>();
#endif
            if (runner == null)
                return true;

            var masterRef = SharedModeMasterClientTracker.GetSharedModeMasterClientPlayerRef();
            if (masterRef.HasValue)
                return masterRef.Value == runner.LocalPlayer;

            return runner.IsSharedModeMasterClient;
        }
#endif


        /* ─────────────────────────── Life‑cycle ───────────────────────── */

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(gameObject);
            }
            perkLevels = new Dictionary<object, int>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private async void Start()
        {
            timeRemaining = survivalTime;
            if (IsRunnerActive) upgradeMode = UpgradeMode.UpgradeOnButtonClick;
#if FUSION2
            bool netRunning = GameplaySync.Instance && GameplaySync.Instance.RunnerActive;

            if (IsHost() && PreloadAllMonsters)
                await MonsterPool.Instance.PreloadAsync(waves);

            if (!netRunning)
                StartGameplay();
#else
            await Task.Yield();
            StartGameplay();
#endif
        }

        /// <summary>
        /// Asynchronously sets up the CharacterEntity and related gameplay settings.
        /// </summary>
        /// <param name="characterOwner">The character to configure.</param>
        /// <returns>A UniTask representing the async operation.</returns>
        public async UniTask SetupCharacterEntity(CharacterEntity characterOwner)
        {
            character = characterOwner;

            if (character.GetCharacterData().autoAttack != null)
            {
                var aaSkill = character.GetCharacterData().autoAttack;
                if (!skillBaseLevels.ContainsKey(aaSkill))
                {
                    skillBaseLevels[aaSkill] = 0;
                }
            }

            foreach (var skill in character.GetCharacterData().skills)
            {
                if (!skillBaseLevels.ContainsKey(skill))
                {
                    skillBaseLevels[skill] = 0;
                }
            }

            maxSkills = (int)character.GetCurrentMaxSkills();
            maxStats = (int)character.GetCurrentMaxStats();

            // Update gameplay UI
            UIGameplay.Singleton.UpdateSkillPerkUI();
            UIGameplay.Singleton.UpdateStatPerkUI();

            await UniTask.Yield();
        }

        public void RegisterCharacter(CharacterEntity ch)
        {
            if (!IsRunnerActive || IsLeader)
                ActiveCharactersList.Add(ch.transform);

            RegisterAlive(ch);
            _knownCharacters.Add(ch);
            RecomputeRosterReady();
        }

        public void UnregisterAlive(CharacterEntity ch)
        {
            _aliveCharacters.Remove(ch);
        }

        private void RecomputeRosterReady()
        {
#if FUSION2
            if (!IsRunnerActive || !IsLeader) { _rosterReady = false; return; }
            var runner = NetworkRunner.GetRunnerForGameObject(gameObject);
#if UNITY_6000_0_OR_NEWER
            if (runner == null) runner = FindFirstObjectByType<NetworkRunner>();
#else
    if (runner == null) runner = FindObjectOfType<NetworkRunner>();
#endif
            if (runner == null) { _rosterReady = false; return; }
            int expected = runner.ActivePlayers.Count();
            int seen = _knownCharacters.Count;
            _rosterReady = seen >= expected;
#endif
        }

        public override string ToString() => base.ToString();

        public CharacterEntity GetCharacterEntity()
        {
            return character;
        }

        public void SetTimeRemaining(int seconds)
        {
            timeRemaining = seconds;
        }

        public int GetSurvivalTime()
        {
            return survivalTime;
        }

        /// <summary>
        /// Starts the gameplay by initiating the survival countdown and spawning waves.
        /// </summary>
        public void StartGameplay()
        {
            if (IsRunnerActive && !IsLeader)
                return;

            if (isPvp) return;

            StartSurvivalCountdown();

            _waveCts?.Cancel();
            _waveCts = new CancellationTokenSource();
            SpawnWavesAsync(_waveCts.Token).Forget();
        }

        public void StartSurvivalCountdown()
        {
            if (IsRunnerActive && !IsLeader)      
                return;

            if (isPvp) return;

            _survivalCts?.Cancel();
            _survivalCts = new CancellationTokenSource();
            CountdownSurvivalAsync(_survivalCts.Token).Forget();
        }

        private async UniTaskVoid CountdownSurvivalAsync(CancellationToken token)
        {
            if (IsRunnerActive && !IsLeader) return;
            if (isPvp) return;

            EventBus.Publish(new GameTimerTickEvent((int)timeRemaining));

            while (timeRemaining > 0 && gameRunning && !token.IsCancellationRequested)
            {
                if (isPaused)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    continue;
                }

                await UniTask.Delay(1000,
                                    cancellationToken: token,
                                    delayTiming: PlayerLoopTiming.Update);

                if (isPaused || token.IsCancellationRequested) continue;

                timeRemaining--;
                EventBus.Publish(new GameTimerTickEvent((int)timeRemaining));
            }

            if (!token.IsCancellationRequested &&
                gameRunning &&
                (winCondition == WinCondition.SurvivalTime ||
                 winCondition == WinCondition.SurvivalTimeAndKillBoss))
            {
                HandleWinCondition();
            }
        }

        /// <summary>
        /// Registers this character as alive (called on spawn/enable/revive).
        /// </summary>
        public void RegisterAlive(CharacterEntity ch)
        {
            _aliveCharacters.Add(ch);
        }

        /// <summary>
        /// Marks a character as dead and checks for a full team wipe.
        /// </summary>
        public void MarkCharacterDead(CharacterEntity ch)
        {
            _aliveCharacters.Remove(ch);
            if (!IsRunnerActive || IsLeader)
                ActiveCharactersList.Remove(ch.transform);

            StartReviveCountdownIfOnline(ch);
            TryCheckTeamWipe();
        }

        /// <summary>
        /// Called when a character revives; puts it back into the alive set.
        /// </summary>
        public void MarkCharacterRevived(CharacterEntity ch)
        {
            _aliveCharacters.Add(ch);
            if (!IsRunnerActive || IsLeader)
                ActiveCharactersList.Add(ch.transform);

            if (matchFinished) return;

            UIGameplay.Singleton?.HideEndGameScreen();
            ResumeGame();
        }

        private void StartReviveCountdownIfOnline(CharacterEntity ch)
        {
#if FUSION2
            if (PvpSync.IsSpawnedReady && PvpSync.Instance.Mode == PVP.PvpModeType.BattleRoyale)
                return;
#endif
            if (!IsRunnerActive || !IsLeader) return;

#if FUSION2
            if(PvpSync.IsSpawnedReady && GameplayManager.Singleton.isPvp) reviveSeconds = (int)PvpSync.Instance.ReviveDelay;
            GameplaySync.Instance?.NotifyStartReviveCountdown(ch, reviveSeconds);
#endif
            _reviveTimers.TryGetValue(ch, out var old);
            old?.Cancel();
            var cts = new CancellationTokenSource();
            _reviveTimers[ch] = cts;
            ReviveCountdownAsync(ch, reviveSeconds, cts.Token).Forget();
        }

        //PVP
        public void SetPvpSession(bool enabled)
        {
            isPvp = enabled;

#if FUSION2
            foreach (var ch in _knownCharacters)
                if (ch) ch.RefreshTeamObjects();
#endif

            if (enabled)
            {
                _survivalCts?.Cancel();
                _waveCts?.Cancel();
                _bossCheckCts?.Cancel();

                if (UIGameplay.Singleton)
                {
                    if (UIGameplay.Singleton.bossHpContainer) UIGameplay.Singleton.bossHpContainer.SetActive(false);
                }
            }
        }

        public Vector3 GetTeamSpawnPosition(byte team)
        {
            var t = GetTeamSpawnTransform(team);
            if (!t) return Vector3.zero;

            var rnd = UnityEngine.Random.insideUnitCircle * teamSpawnSpreadRadius;
            return t.position + new Vector3(rnd.x, 0, rnd.y);
        }

        public Quaternion GetTeamSpawnRotation(byte team)
        {
            var t = GetTeamSpawnTransform(team);
            return t ? t.rotation : Quaternion.identity;
        }

        private Transform GetTeamSpawnTransform(byte team)
        {
            if (teamSpawnSlots == null || teamSpawnSlots.Length == 0) return null;
            int idx = Mathf.Abs(team) % teamSpawnSlots.Length;
            return teamSpawnSlots[idx];
        }

        public int GetTeamScore(int team)
        {
#if FUSION2
            if (IsRunnerActive && PvpSync.Instance)
                return PvpSync.Instance.GetTeamScore((byte)team);
#endif
            return _teamScore.TryGetValue(team, out var s) ? s : 0;
        }

        public void SetTeamScore(int team, int value)
        {
#if FUSION2
            if (IsRunnerActive && PvpSync.Instance)
            {
                PvpSync.Instance.SetTeamScore((byte)team, value);
                return;
            }
#endif
            _teamScore[team] = value;
        }

        public void AddTeamScore(int team, int delta)
        {
#if FUSION2
            if (IsRunnerActive && PvpSync.Instance)
            {
                PvpSync.Instance.AddTeamScore((byte)team, delta);
                return;
            }
#endif
            if (!_teamScore.ContainsKey(team)) _teamScore[team] = 0;
            _teamScore[team] += delta;
            // EventBus.Publish(new TeamScoreChangedEvent(team, _teamScore[team]));
        }

        /// <summary>
        /// Called by the leader (host) when the last alive player dies.
        /// </summary>
        private void TryCheckTeamWipe()
        {
            if (matchFinished) return;
            if (isPvp) return;

            if (!IsRunnerActive)
            {
                if (_aliveCharacters.Count == 0)
                {
                    if (reviveLimit > 0)
                        LocalDefeatWithRevive();
                    else
                        EndGame();
                }
                return;
            }

            if (!IsLeader) return;

            if (!_rosterReady)
                return;

            if (_aliveCharacters.Count == 0)
            {
#if FUSION2
                GameplaySync.Instance?.SyncTeamDefeat();
#endif
                CancelAllRevives();
                EndGame();
            }
        }

        /// <summary>
        /// Offline-only defeat flow: show end screen with revive option.
        /// </summary>
        private void LocalDefeatWithRevive()
        {
            if (matchFinished) return;
            if (isPvp) return;
            gameRunning = false;
            PauseGame();
            UIGameplay.Singleton.DisplayEndGameScreen(false); // reviveButton 
        }

        public void SetMatchFinished()
        {
            matchFinished = true;
        }

        private async UniTaskVoid SpawnWavesAsync(CancellationToken token)
        {
            if (IsRunnerActive && !IsLeader)       
                return;

            if (isPvp) return;

            currentWaveIndex = 0;

            while (currentWaveIndex < waves.Count && gameRunning && !token.IsCancellationRequested)
            {
                Wave wave = waves[currentWaveIndex];
                float waveStart = Time.time;

                var nextSpawn = new float[wave.monsters.Count];
                var remain = new int[wave.monsters.Count];

                for (int i = 0; i < wave.monsters.Count; ++i)
                {
                    nextSpawn[i] = waveStart + wave.monsters[i].spawnInterval;
                    remain[i] = Mathf.FloorToInt(wave.waveDuration / wave.monsters[i].spawnInterval);
                }

                while ((Time.time - waveStart) < wave.waveDuration &&
                       !token.IsCancellationRequested &&
                       Array.Exists(remain, r => r > 0) &&
                       gameRunning)
                {
                    if (!isPaused)
                    {
                        float t = Time.time;
                        for (int i = 0; i < wave.monsters.Count; ++i)
                        {
                            if (t >= nextSpawn[i] && remain[i] > 0)
                            {
                                var cfg = wave.monsters[i];
                                await UniTask.FromResult(SpawnMonster(cfg.monsterPrefab, cfg.goldPerMonster, cfg.xpPerMonster));
                                nextSpawn[i] = t + cfg.spawnInterval;
                                remain[i]--;
                            }
                        }
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                currentWaveIndex++;
            }
            if (gameRunning && winCondition != WinCondition.SurvivalTime)
                HandleWinCondition();
        }


        /// <summary>
        /// Handles the win condition logic based on the selected game mode.
        /// </summary>
        private async void HandleWinCondition()
        {
            if (IsRunnerActive && !IsLeader)
                return;

            if (isPvp) return;

            if (winCondition == WinCondition.SurvivalTime)
            {
                EndGame();
            }
            else if (winCondition == WinCondition.KillBoss)
            {
                // Spawn the boss immediately and show the final boss message
                await UniTask.FromResult(SpawnMonster(bossPrefab, 0, 0));

                UIGameplay.Singleton.ShowFinalBossMessage(); // Show the final boss message
                _bossCheckCts?.Cancel();
                _bossCheckCts = new CancellationTokenSource();
                CheckForBossDefeatAsync(_bossCheckCts.Token).Forget();
            }
            else if (winCondition == WinCondition.SurvivalTimeAndKillBoss)
            {
                _waveCts?.Cancel();

                if (timeRemaining <= 0)
                {
                    await UniTask.FromResult(SpawnMonster(bossPrefab, 0, 0));

                    UIGameplay.Singleton.ShowFinalBossMessage(); // Show the final boss message
                    _bossCheckCts?.Cancel();
                    _bossCheckCts = new CancellationTokenSource();
                    CheckForBossDefeatAsync(_bossCheckCts.Token).Forget();
                }
            }
        }


        private async UniTaskVoid CheckForBossDefeatAsync(CancellationToken token)
        {
            if (IsRunnerActive && !IsLeader) return;
            if (isPvp) return;

            float guardTimeout = 5f;
            float start = Time.time;
            while (!_bossSeenAliveOnce && !token.IsCancellationRequested && (Time.time - start) < guardTimeout)
            {
#if UNITY_6000_0_OR_NEWER
                var bosses = FindObjectsByType<MonsterEntity>(FindObjectsSortMode.None);
#else
                var bosses = FindObjectsOfType<MonsterEntity>();
#endif
                foreach (var b in bosses)
                {
                    if (b && b.IsFinalBoss)
                    {
                        _finalBosses.Add(b);
                        if (b.gameObject.activeInHierarchy)
                        {
                            _bossSeenAliveOnce = true;
                            break;
                        }
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            while (!token.IsCancellationRequested)
            {
                _finalBosses.RemoveWhere(b => !b || !b.gameObject); 

                bool anyAlive = false;
                foreach (var b in _finalBosses)
                {
                    if (b && b.gameObject.activeInHierarchy)
                    {
                        anyAlive = true;
                        break;
                    }
                }

                if (!anyAlive)
                {
                    EndGame();
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private async UniTaskVoid SpawnMonster(MonsterEntity prefab, int gold, int xp)
        {
            if (isPvp) return;
            Vector3 pos = GetRandomSpawnPoint();
#if FUSION2
            if (!IsHost()) return;
#endif

            GameObject go = await MonsterPool.Instance.Spawn(
               prefab.gameObject,
               pos,
               Quaternion.identity);

            var m = go.GetComponent<MonsterEntity>();
            m.ConfigureMonster(gold, xp);
            if (m.IsFinalBoss)
            {
                _finalBosses.Add(m);
                if (m.gameObject.activeInHierarchy) _bossSeenAliveOnce = true;
                UIGameplay.Singleton?.SetFinalBoss(m);

#if FUSION2
                if (IsRunnerActive && IsLeader)
                    GameplaySync.Instance?.SyncBossSpawned(m);
#endif
            }
#if FUSION2
            GameplaySync.Instance?.SyncConfigureMonster(
                go.GetComponent<NetworkObject>(),
                gold, xp);
#endif
        }

        /// <summary>
        /// Gets a random spawn point from the available spawn points and returns a random position within that point's radius.
        /// </summary>
        /// <returns>A random Vector3 position from a random spawn point in the list.</returns>
        Vector3 GetRandomSpawnPoint()
        {
            if (spawnPointsList == null || spawnPointsList.Count == 0)
            {
                Debug.LogWarning("No spawn points available.");
                return Vector3.zero;
            }

            // Select a random spawn point from the list
            int randomIndex = UnityEngine.Random.Range(0, spawnPointsList.Count);
            SpawnPoints selectedSpawnPoint = spawnPointsList[randomIndex];

            // Get a random position from the selected spawn point
            return selectedSpawnPoint.GetRandomSpawnPoint();
        }


        /// <summary>
        /// Ends the current game: pauses, waits a delay, syncs network end and shows end screen.
        /// </summary>
        public async void EndGame()
        {
            _finalBosses.Clear();
            _bossSeenAliveOnce = false;
            matchFinished = true;
            gameRunning = false;

#if FUSION2
            if (IsRunnerActive && IsLeader)
                GameplaySync.Instance?.SyncEndGame();
#endif

            await UniTask.Delay(TimeSpan.FromSeconds(1.5));
            PauseGame();

            if (!IsPvp) 
            {
                MonsterPool.Instance?.DestroyPool();
                ActiveMonstersList.Clear();
                ActiveBoxesList.Clear();
                await GameManager.Singleton.EndGameAsync(true);
            }
        }



        /// <summary>
        /// Toggles the pause state of the game, stopping or resuming waves accordingly.
        /// </summary>
        public void TogglePause()
        {
#if FUSION2
            if (character.IsNetworked) return;
#endif
            isPaused = !isPaused;
            // The SpawnWaves coroutine handles the pause internally, so no need to stop/restart it here.
        }

        /// <summary>
        /// Explicitly pauses the game, intended to be used for direct calls when pausing is needed without toggling.
        /// </summary>
        public void PauseGame()
        {
            if (!isPaused) // Only pause if not already paused.
            {
                isPaused = true;
            }
        }

        /// <summary>
        /// Explicitly resumes the game, intended to be used for direct calls when resuming is needed without toggling.
        /// </summary>
        public void ResumeGame()
        {
            if (isPaused) // Only resume if the game is currently paused.
            {
                isPaused = false;
                // The SpawnWaves coroutine will automatically resume respecting the isPaused state.
            }
        }

        /// <summary>
        /// Checks if the game is currently paused.
        /// </summary>
        /// <returns>True if the game is paused; otherwise, false.</returns>
        public bool IsPaused()
        {
            return isPaused;
        }

        public int GetMonstersKilled()
        {
            return monstersKilled;
        }
        public int GetGainGold()
        {
            return goldGain;
        }


        public void ForceSetGold(int value)
        {
            goldGain = value;
            EventBus.Publish(new GoldChangedEvent(goldGain));
        }

        public void ForceSetXP(int value)
        {
            character.AddXP(value);
            xpGain = value;
        }

        public void ForceSetMonstersKilled(int value)
        {
            monstersKilled = value;
            EventBus.Publish(new MonstersKilledChangedEvent(monstersKilled));
        }

        /* ───── MONSTERS KILLED ────────────────────────── */

        public void IncrementMonstersKilled(int delta = 1)
        {
            monstersKilled += delta;
            EventBus.Publish(new MonstersKilledChangedEvent(monstersKilled));
#if FUSION2
            GameplaySync.Instance?.SyncMonsters(monstersKilled);
#endif
        }

        /* ───── GOLD ───────────────────────────────────── */

        public void IncrementGainGold(int delta)
        {
            goldGain += delta;
            EventBus.Publish(new GoldChangedEvent(goldGain));
#if FUSION2
            GameplaySync.Instance?.SyncGold(goldGain);
#endif
        }

        /* ───── XP ─────────────────────────────────────── */

        public void IncrementGainXP(int delta)
        {
            xpGain += delta;
            character.AddXP(delta);
#if FUSION2
            GameplaySync.Instance?.SyncXP(xpGain);
#endif
        }

        /// <summary>
        /// Retrieves a list of available perks, filtering out any skill levels marked as evolved that the player cannot access yet.
        /// </summary>
        /// <returns>A list of perks available for the player to choose from.</returns>
        public List<object> GetRandomPerks()
        {
            List<object> availablePerks = new List<object>();

            foreach (var skill in character.GetCharacterData().skills)
            {
                int currentLevel = GetBaseSkillLevel(skill);
                int maxLevelIndex = skill.skillLevels.Count - 1;
                int nextLevel = currentLevel + 1;

                if (nextLevel <= maxLevelIndex)
                {
                    bool isNextLevelEvolved = skill.skillLevels[nextLevel].isEvolved;
                    bool hasRequiredStatPerk = skill.requireStatForEvolve == null || character.HasStatPerk(skill.requireStatForEvolve);

                    if (!isNextLevelEvolved || hasRequiredStatPerk)
                    {
                        availablePerks.Add(skill);
                    }
                }
            }
            if (character.GetCharacterData().autoAttack != null)
            {
                SkillData aaSkill = character.GetCharacterData().autoAttack;
                int currentLevel = GetBaseSkillLevel(aaSkill);
                int maxLevelIndex = aaSkill.skillLevels.Count - 1;
                int nextLevel = currentLevel + 1;

                if (nextLevel <= maxLevelIndex)
                {
                    bool isNextLevelEvolved = aaSkill.skillLevels[nextLevel].isEvolved;
                    bool hasRequiredStatPerk = aaSkill.requireStatForEvolve == null || character.HasStatPerk(aaSkill.requireStatForEvolve);

                    if (!isNextLevelEvolved || hasRequiredStatPerk)
                    {
                        availablePerks.Add(aaSkill);
                    }
                }
            }

            foreach (var skillPerk in skillPerkData)
            {
                int currentLevel = GetSkillLevel(skillPerk);
                int maxLevel = skillPerk.maxLevel;

                if (currentLevel < maxLevel)
                {
                    // SkillPerk can be leveled up
                    availablePerks.Add(skillPerk);
                }
                else if (currentLevel == maxLevel)
                {
                    if (skillPerk.hasEvolution && character.HasStatPerk(skillPerk.perkRequireToEvolveSkill))
                    {
                        // SkillPerk can be evolved
                        availablePerks.Add(skillPerk);
                    }
                }
            }

            foreach (var statPerk in statPerkData)
            {
                int currentLevel = GetPerkLevel(statPerk);
                if (currentLevel < maxLevelStatPerks)
                {
                    availablePerks.Add(statPerk);
                }
            }

            ShufflePerks(availablePerks);

            return availablePerks.GetRange(0, Mathf.Min(3, availablePerks.Count));
        }

        private void ShufflePerks(List<object> perks)
        {
            for (int i = perks.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                object temp = perks[i];
                perks[i] = perks[j];
                perks[j] = temp;
            }
        }

        /// <summary>
        /// Gets the current level of a given perk.
        /// </summary>
        /// <param name="perk">The perk whose level is to be retrieved.</param>
        /// <returns>The current level of the perk. Returns 0 if the perk is not found.</returns>
        public int GetPerkLevel(object perk)
        {
            if (perk is SkillPerkData skillPerk)
            {
                if (skillLevels.TryGetValue(skillPerk, out int level))
                {
                    return level;
                }
            }
            else if (perk is StatPerkData statPerk)
            {
                if (perkLevels.TryGetValue(statPerk, out int level))
                {
                    return level;
                }
            }
            return 0;
        }


        /// <summary>
        /// Sets the level of a given perk, handling skill and stat perks separately.
        /// </summary>
        /// <param name="perk">The perk whose level is to be set.</param>
        /// <param name="level">The new level of the perk.</param>
        public void SetPerkLevel(object perk)
        {
            if (perk is SkillPerkData skillPerk)
            {
                LevelUpSkill(skillPerk);
            }
            else if (perk is StatPerkData statPerk)
            {
                if (perkLevels.ContainsKey(statPerk))
                {
                    perkLevels[statPerk]++;
                }
                else
                {
                    perkLevels.Add(statPerk, 1);
                }
            }
            else
            {
                Debug.LogError("Unsupported perk type.");
            }
        }


        /// <summary>
        /// Levels up a skill to a specified level, including checks for evolution requirements.
        /// </summary>
        /// <param name="skillPerk">The skill perk to level up.</param>
        /// <param name="level">The new level to set, if requirements are met.</param>
        private void LevelUpSkill(SkillPerkData skillPerk)
        {
            if (skillPerk == null)
            {
                Debug.LogWarning("SkillPerkData is null.");
                return;
            }
            if (!skillLevels.ContainsKey(skillPerk))
            {
                skillLevels[skillPerk] = 0;
            }

            int currentLevel = skillLevels[skillPerk];

            if (currentLevel >= skillPerk.maxLevel)
            {
                Debug.LogWarning($"Attempting to level up {skillPerk.name} beyond max level {skillPerk.maxLevel}.");
                return;
            }

            if (currentLevel == skillPerk.maxLevel - 1 && skillPerk.hasEvolution && !character.HasStatPerk(skillPerk.perkRequireToEvolveSkill))
            {
                Debug.LogWarning($"Cannot level up {skillPerk.name} to max level without having {skillPerk.perkRequireToEvolveSkill.name}.");
                return;
            }

            skillLevels[skillPerk] = currentLevel + 1;
        }


        /// <summary>
        /// Gets the current level of a specific skill.
        /// </summary>
        /// <param name="skillPerk">The skill perk to check the level for.</param>
        /// <returns>The current level of the skill.</returns>
        public int GetSkillLevel(SkillPerkData skillPerk)
        {
            if (skillPerk == null)
            {
                return 0;
            }

            if (skillLevels.ContainsKey(skillPerk))
            {
                return skillLevels[skillPerk];
            }

            return 0;
        }

        /// <summary>
        /// Levels up a base skill of the character.
        /// </summary>
        /// <param name="skill">The skill to level up.</param>
        public void LevelUpBaseSkill(SkillData skill)
        {
            if (!skillBaseLevels.ContainsKey(skill))
            {
                skillBaseLevels[skill] = 0;
            }

            int currentLevel = skillBaseLevels[skill];
            int maxLevelIndex = skill.skillLevels.Count - 1;

            if (currentLevel <= maxLevelIndex)
            {
                skillBaseLevels[skill]++;
                Debug.Log($"Skill {skill.skillName} leveled up to {skillBaseLevels[skill]}.");
            }

        }

        /// <summary>
        /// Gets the current level of a base skill.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>The current level of the skill.</returns>
        public int GetBaseSkillLevel(SkillData skill)
        {
            if (skillBaseLevels.TryGetValue(skill, out int level))
            {
                return level;
            }
            return 0;
        }

        /// <summary>
        /// Called when the local player reaches 0 HP. Decides if we show revive or straight defeat,
        /// and guarantees game flow stop.
        /// </summary>
        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            var ch = evt.Target;
            if (ch == null) return;
            if (ch.GetCurrentHP() > 0.001f) return;
#if FUSION2
            if (ch.IsNetworked) return;
#endif
            if (!ch.IsDead) ch.OnDeath();
            OnPlayerDefeated(ch);
        }

        /// <summary>
        /// Centralizes defeat flow (pause, UI, end flags).
        /// </summary>
        public void OnPlayerDefeated(CharacterEntity ch)
        {
            if (!gameRunning)
                return;

            if (isPvp) return;

            gameRunning = false;
            PauseGame();

            bool canRevive = reviveLimit > 0 && !ch.IsNetworked;
            UIGameplay.Singleton.DisplayEndGameScreen(false);
            if (canRevive && UIGameplay.Singleton.reviveButton)
                UIGameplay.Singleton.reviveButton.gameObject.SetActive(true);

        }

        private async UniTaskVoid ReviveCountdownAsync(CharacterEntity ch, int secs, CancellationToken token)
        {
            int remain = secs;
            try
            {
                while (remain > 0 && !token.IsCancellationRequested && gameRunning)
                {
#if FUSION2
                    GameplaySync.Instance?.NotifyReviveTick(ch, remain);
#endif
                    await UniTask.Delay(1000, cancellationToken: token);
                    remain--;
                }
                if (token.IsCancellationRequested || !gameRunning) return;

                ch.CharacterRevive();

#if FUSION2
                GameplaySync.Instance?.SyncPlayerRevived(ch);
#endif
            }
            catch (OperationCanceledException) { }
            finally
            {
                _reviveTimers.Remove(ch);
#if FUSION2
                GameplaySync.Instance?.NotifyReviveTick(ch, 0);
#endif
            }
        }

        public void CancelAllRevives()
        {
            foreach (var kv in _reviveTimers)
                kv.Value.Cancel();
            _reviveTimers.Clear();
        }
    }
}
