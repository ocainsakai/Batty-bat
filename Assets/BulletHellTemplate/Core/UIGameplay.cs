using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using BulletHellTemplate.Core.Events;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;



#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the user interface during gameplay, including player characterStatsComponent, skills, and input handling for both mobile and PC platforms.
    /// </summary>
    public class UIGameplay : MonoBehaviour
    {
        #region Mobile Controls

        [Header("Mobile Controls")]
        [Tooltip("Reference to the Joystick for mobile movement")]
        public Joystick joystick; // Reference to the Joystick for mobile movement

        [Header("Mobile Skill Joysticks")]
        [Tooltip("Array to hold skill joysticks for mobile")]
        public SkillJoystick[] skillJoysticks; // Array to hold skill joysticks for mobile

        #endregion

        #region PC Controls

        [Header("PC Controls")]
        [Tooltip("Reference to the PC Input Controller")]
        public PCInputController pcInputController; // Reference to the PC Input Controller

        [Tooltip("Reference to the PC Skill Controller")]
        public PCSkillController pcSkillController; // Reference to the PC Skill Controller

        #endregion

        [Header("Skill Icons")]
        [Tooltip("Array to hold skill images")]
        public SkillImage[] skillImages;

        [Header("Final Boss")]
        [Tooltip("Final boss message GameObject")]
        public GameObject finalBossMessage;
        private MonsterEntity finalBossEntity;
        [Tooltip("Time to close the final boss message")]
        public float timeToCloseMessage = 3f;

        [Header("Gameplay Rules Info")]
        [Tooltip("Timer text")]
        public TimeFormat timeDisplayFormat = TimeFormat.Seconds;
        public TextMeshProUGUI timer;
        [Tooltip("Monsters killed text")]
        public TextMeshProUGUI monstersKilled;
        [Tooltip("Gold gain text")]
        public TextMeshProUGUI goldGain;
        [Tooltip("Insufficient MP text")]
        public TextMeshProUGUI insufficientMPText;
        [Tooltip("Pause menu GameObject")]
        public GameObject pauseMenu;
        public Image minimapImage;

        [Header("Revive UI")]
        public GameObject revivePanel;
        public TextMeshProUGUI reviveCountdownText;

        [Header("Upgrade Infos")]
        [Tooltip("Perk entry passEntryPrefab")]
        public PerkEntry perkEntryPrefab;
        [Tooltip("Perk containerPassItems transform")]
        public Transform perkContainer;

        public Button openUpgradesButton;
        public GameObject updateAvailable;
        [Header("Skill Perk Images")]
        [Tooltip("Array to hold skill perk images")]
        public SkillsPerkImage[] skillPerkImages; // Array to hold skill perk images

        [Header("Stat Perk Images")]
        [Tooltip("Array to hold stat perk images")]
        public StatPerkImage[] statPerkImages; // Array to hold stat perk images

        [Header("Boss HP Bar")]
        [Tooltip("Boss HP bar Image")]
        public Image bossHpBar;
        [Tooltip("Boss HP containerPassItems GameObject")]
        public GameObject bossHpContainer;
        [Tooltip("Timer containerPassItems GameObject")]
        public GameObject timerContainer;

        [Header("Death Entries UI")]
        public Transform deathEntriesRoot;           
        public PlayerDeathEntryUI deathEntryPrefab;

        [Header("PVP — Allies")]
        public Transform alliesContainer;                 
        public PvpTeamPlayerEntryUI allyEntryPrefab;      

        [Header("PVP — Kill Feed")]
        public Transform killFeedContainer;              
        public KillFeedEntryUI killFeedEntryPrefab;    
        public int killFeedMaxEntries = 6;

        [Header("PVP — Scoreboard (TDM)")]
        public GameObject tdmScoreRoot;
        public TextMeshProUGUI tdmTimerText;
        public TextMeshProUGUI[] tdmTeamScores;          

        [Header("PVP — Scoreboard (Arena)")]
        public GameObject arenaScoreRoot;
        public TextMeshProUGUI[] arenaTeamScores;       
        public TextMeshProUGUI arenaKillLimitText;

        [Header("PVP — Scoreboard (BR)")]
        public GameObject brScoreRoot;
        public TextMeshProUGUI brAliveText;

        [Header("End Game Screen")]
        [Tooltip("End game screen GameObject")]
        public GameObject endGameScreen;
        [Tooltip("End game message text")]
        public TextMeshProUGUI endGameMessage;
        [Tooltip("Victory objects to activate")]
        public GameObject[] VictoryObjects;
        [Tooltip("Defeat objects to activate")]
        public GameObject[] DefeatObjects;
        [Tooltip("Victory audio clip")]
        public AudioClip VictoryAudio;
        [Tooltip("Defeat audio clip")]
        public AudioClip DefeatAudio;
        public Button reviveButton;

        // --- PVP End Panels --- // 
        [Header("PVP End Panels")]
        public GameObject pvpEndRoot;

        [Header("TDM End Panel")]
        public GameObject tdmEndPanel;
        public TextMeshProUGUI tdmWinnerText;
        public TextMeshProUGUI tdmScoreText; 

        [Header("Arena End Panel")]
        public GameObject arenaEndPanel;
        public TextMeshProUGUI arenaWinnerText;
        public TextMeshProUGUI arenaDetailText; 

        [Header("Battle Royale End Panel")]
        public GameObject brEndPanel;
        public TextMeshProUGUI brPlacementText; 
        public GameObject brTop3Badge;

        [Header("Locked Perk Sprites")]
        [Tooltip("Sprite for locked skills")]
        public Sprite lockedSkillSprite;
        [Tooltip("Sprite for locked characterStatsComponent")]
        public Sprite lockedStatSprite;
        [Tooltip("Sprite for unlocked slots")]
        public Sprite unlockedSprite;

        [Header("UI Character Info")]
        public Image characterIcon;
        [Tooltip("HealthComponent bar Image")]
        public Image hpBar;
        [Tooltip("Mana bar Image")]
        public Image mpBar;
        [Tooltip("Experience bar Image")]
        public Image xpBar;
        [Tooltip("Current  text")]
        public TextMeshProUGUI hpCurrent;
        public TextMeshProUGUI energyCurrent;
        public TextMeshProUGUI xpCurrent;

        [Tooltip("Level text")]
        public TextMeshProUGUI level;
        [Tooltip("HealthComponent text")]
        public TextMeshProUGUI hpText;
        [Tooltip("HealthComponent regeneration text")]
        public TextMeshProUGUI hpRegenText;
        [Tooltip("HealthComponent leech text")]
        public TextMeshProUGUI hpLeechText;
        [Tooltip("Mana text")]
        public TextMeshProUGUI mpText;
        [Tooltip("Mana regeneration text")]
        public TextMeshProUGUI mpRegenText;
        [Tooltip("Damage text")]
        public TextMeshProUGUI damageText;
        [Tooltip("Attack speed text")]
        public TextMeshProUGUI attackSpeedText;
        [Tooltip("Cooldown reduction text")]
        public TextMeshProUGUI cooldownReductionText;
        [Tooltip("Critical rate text")]
        public TextMeshProUGUI criticalRateText;
        [Tooltip("Critical damage multiplier text")]
        public TextMeshProUGUI criticalDamageMultiplierText;
        [Tooltip("Defense text")]
        public TextMeshProUGUI defenseText;
        [Tooltip("Shield text")]
        public TextMeshProUGUI shieldText;
        [Tooltip("Move speed text")]
        public TextMeshProUGUI moveSpeedText;
        [Tooltip("Collect range text")]
        public TextMeshProUGUI collectRangeText;

        private CharacterEntity characterEntity;
        private int upgradeAmount = 0;

        private int _lastKnownLevel = -1;
        private bool _perksOpen = false;
        public static UIGameplay Singleton { get; private set; }
#if FUSION2
        private readonly Dictionary<NetworkId, PlayerDeathEntryUI> _deathEntries = new();
#endif

#if FUSION2
        private readonly Dictionary<NetworkId, PvpTeamPlayerEntryUI> _allyEntries = new();
        private byte _localTeamId;
#endif

        private readonly Queue<GameObject> _killFeedQueue = new();

        //Cancel Tokens
        private CancellationTokenSource hpBarCts;
        private CancellationTokenSource blinkCts;

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

            reviveButton.onClick.AddListener(Revive);

            EventBus.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
            EventBus.Subscribe<PlayerEnergyChangedEvent>(OnPlayerEnergyChanged);
            EventBus.Subscribe<PlayerShieldChangedEvent>(OnPlayerShieldChanged);
            EventBus.Subscribe<StatPerkUpdatedEvent>(OnUpdateStatPerkUI);
            EventBus.Subscribe<PlayerStatsChangedEvent>(OnPlayerStatsChanged);
            EventBus.Subscribe<GameTimerTickEvent>(OnTimerTick);
            EventBus.Subscribe<MonstersKilledChangedEvent>(OnMonstersKilledChanged);
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Subscribe<PlayerSkillCooldownChangedEvent>(OnSkillCooldownChanged);
            EventBus.Subscribe<BossHealthChangedEvent>(OnBossHealthChanged);
            EventBus.Subscribe<UpgradePointsChangedEvent>(OnUpgradePointsChanged);
            EventBus.Subscribe<PlayerEXPChangeEvent>(OnPlayerLevelUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
            EventBus.Unsubscribe<PlayerEnergyChangedEvent>(OnPlayerEnergyChanged);
            EventBus.Unsubscribe<PlayerShieldChangedEvent>(OnPlayerShieldChanged);
            EventBus.Unsubscribe<StatPerkUpdatedEvent>(OnUpdateStatPerkUI);
            EventBus.Unsubscribe<PlayerStatsChangedEvent>(OnPlayerStatsChanged);
            EventBus.Unsubscribe<GameTimerTickEvent>(OnTimerTick);
            EventBus.Unsubscribe<MonstersKilledChangedEvent>(OnMonstersKilledChanged);
            EventBus.Unsubscribe<PlayerSkillCooldownChangedEvent>(OnSkillCooldownChanged);
            EventBus.Unsubscribe<BossHealthChangedEvent>(OnBossHealthChanged);
            EventBus.Unsubscribe<UpgradePointsChangedEvent>(OnUpgradePointsChanged);
            EventBus.Unsubscribe<PlayerEXPChangeEvent>(OnPlayerLevelUp);
        }

        private void Start()
        {
            monstersKilled.text = "0";
            goldGain.text = "0";
            InitializeAsync().Forget();

            if(GameplayManager.Singleton.reviveLimit <= 0 && reviveButton) reviveButton.gameObject.SetActive(false);
        }
           

        /// <summary>
        /// Attempts to initialize the upgrade button when GameplayManager is ready.
        /// </summary>
        private async UniTaskVoid InitializeAsync()
        {
            await UniTask.WaitUntil(() => GameplayManager.Singleton != null,
                                    cancellationToken: this.GetCancellationTokenOnDestroy());

            if (openUpgradesButton)
            {
                if (GameplayManager.Singleton.upgradeMode == UpgradeMode.UpgradeOnButtonClick)
                {
                    openUpgradesButton.interactable = false;
                    openUpgradesButton.onClick.AddListener(OnClickToChoicePowerUp);
                }
                else
                {
                    openUpgradesButton.gameObject.SetActive(false);
                }
            }
            if (updateAvailable)
                BlinkAsync(updateAvailable, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void OnPlayerHealthChanged(PlayerHealthChangedEvent evt)
        {
            if (evt.Target != characterEntity) return;

            hpBarCts?.Cancel();
            hpBarCts = new CancellationTokenSource();

            hpCurrent.text = $"{Mathf.CeilToInt(evt.CurrentHP)} / {Mathf.CeilToInt(evt.MaxHP)}";
            float startFill = hpBar.fillAmount;
            float endFill = evt.CurrentHP / evt.MaxHP;
            UpdateHpBarAsync(startFill, endFill, hpBarCts.Token).Forget();
        }

        private void OnPlayerEnergyChanged(PlayerEnergyChangedEvent evt)
        {
            if (evt.Target != characterEntity) return;

            mpBar.fillAmount = evt.CurrentMP / evt.MaxMP;
            energyCurrent.text = $"{Mathf.CeilToInt(evt.CurrentMP)} / {Mathf.CeilToInt(evt.MaxMP)}";
        }

        private void OnPlayerShieldChanged(PlayerShieldChangedEvent evt)
        {
            if (evt.Target != characterEntity) return;

            shieldText.text = $"Shield: {Mathf.CeilToInt(evt.CurrentShield)}";
            // shieldBar.fillAmount = evt.CurrentShield / evt.MaxHP;
        }

        private void OnPlayerLevelUp(PlayerEXPChangeEvent evt)
        {
            if (evt.Target != characterEntity) return;

            level.text = $"Level: {evt.NewLevel}";
            xpCurrent.text = $"{evt.CurrentXP} / {evt.NextLevelXP}";
            xpBar.fillAmount = evt.CurrentXP / evt.NextLevelXP;

            bool online = GameplayManager.Singleton.IsRunnerActive;

            if (_lastKnownLevel < 0)
            {
                _lastKnownLevel = evt.NewLevel;
                return;
            }

            if (evt.NewLevel <= _lastKnownLevel)
                return;

            _lastKnownLevel = evt.NewLevel;

            if (online)
            {
                AddUpgradePoints();
                return;
            }

            // Offline
            if (GameplayManager.Singleton.upgradeMode == UpgradeMode.UpgradeOnLevelUp)
            {
                GameplayManager.Singleton.TogglePause();
                if (!_perksOpen) OnLevelUpChoicePowerUp();
            }
            else if (GameplayManager.Singleton.upgradeMode == UpgradeMode.UpgradeOnButtonClick)
            {
                AddUpgradePoints();
            }
        }

        public void OnUpdateStatPerkUI(StatPerkUpdatedEvent evt)
        {
            if (characterEntity == null || characterEntity != evt.Target) return;

            UpdateStatPerkUI();
        }

        private void OnTimerTick(GameTimerTickEvent evt)
        {
            timer.text = FormatSeconds(evt.SecondsElapsed);
        }

        private void OnMonstersKilledChanged(MonstersKilledChangedEvent evt)
        {
            monstersKilled.text = evt.TotalKilled.ToString();
        }
        private void OnGoldChanged(GoldChangedEvent evt)
        {
            goldGain.text = evt.TotalGold.ToString();
        }

        private void OnSkillCooldownChanged(PlayerSkillCooldownChangedEvent evt)
        {
            if (evt.Target != characterEntity) return;
            SkillImage img = skillImages[evt.SkillIndex];
            if (img.ImageComponent == null) return;

            if (evt.MaxCooldown > 0 && evt.CurrentCooldown > 0)
            {
                img.CooldownImage.fillAmount = evt.CurrentCooldown / evt.MaxCooldown;
                img.CooldownText.text = Mathf.CeilToInt(evt.CurrentCooldown).ToString();
            }
            else
            {
                img.CooldownImage.fillAmount = 0;
                img.CooldownText.text = string.Empty;
            }
        }

        private void OnBossHealthChanged(BossHealthChangedEvent evt)
        {
            if (finalBossEntity == null)
                finalBossEntity = evt.Boss;

            if (evt.Boss != finalBossEntity) return;
            bossHpBar.fillAmount = evt.CurrentHP / evt.MaxHP;
        }

        private void OnUpgradePointsChanged(UpgradePointsChangedEvent evt)
        {
            if (updateAvailable == null) return;

            blinkCts?.Cancel();
            blinkCts = new();

            if (evt.Amount > 0)
                BlinkAsync(updateAvailable, blinkCts.Token).Forget();
            else
                updateAvailable.SetActive(false);
        }

        private async UniTaskVoid BlinkAsync(GameObject target, CancellationToken token)
        {
            var renderer = target; 
            bool state = true;

            while (!token.IsCancellationRequested)
            {
                if (upgradeAmount > 0)
                {
                    state = !state;
                    renderer.SetActive(state);
                    await UniTask.Delay(500, cancellationToken: token);
                }
                else
                {
                    renderer.SetActive(true);
                    await UniTask.NextFrame(token);
                }
            }
        }

        private void OnPlayerStatsChanged(PlayerStatsChangedEvent evt)
        {
            if (evt.Target != characterEntity) return;

            var s = evt.playerStats;   // snapshot
            hpText.text = $"HP: {Mathf.CeilToInt(s.baseHP)}";
            hpRegenText.text = $"HP Regen: {Mathf.CeilToInt(s.baseHPRegen)}";
            hpLeechText.text = $"HP Leech: {s.baseHPLeech:F2}%";
            mpText.text = $"MP: {Mathf.CeilToInt(s.baseMP)}";
            mpRegenText.text = $"MP Regen: {Mathf.CeilToInt(s.baseMPRegen)}";
            damageText.text = $"Damage: {Mathf.CeilToInt(s.baseDamage)}";
            attackSpeedText.text = $"Atk Spd: {s.baseAttackSpeed:F2}";
            cooldownReductionText.text = $"CDR: {s.baseCooldownReduction:F2}%";
            criticalRateText.text = $"Crit: {s.baseCriticalRate:F2}%";
            criticalDamageMultiplierText.text = $"Crit Dmg: x{Mathf.CeilToInt(s.baseCriticalDamageMultiplier)}";
            defenseText.text = $"Def: {Mathf.CeilToInt(s.baseDefense)}";
            shieldText.text = $"Shield: {Mathf.CeilToInt(s.baseShield)}";
            moveSpeedText.text = $"Move Spd: {s.baseMoveSpeed:F2}";
            collectRangeText.text = $"Collect: {Mathf.CeilToInt(s.baseCollectRange)}";
        }

        public string FormatSeconds(int seconds)
        {
            return timeDisplayFormat switch
            {
                TimeFormat.Seconds => $"{seconds}s",
                TimeFormat.MinutesSeconds => $"{seconds / 60:00}:{seconds % 60:00}",
                _ => seconds.ToString()
            };
        }

        void Update()
        {
            if (characterEntity == null) return;

            if (GameInstance.Singleton.platformType != PlatformType.PC && joystick)
            {
                var dir = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
                characterEntity.Move(dir);
            }
            // Update skill cooldowns
            UpdateSkillCooldowns();

            if (skillJoysticks != null)
            {
                foreach (var sj in skillJoysticks)
                {
                    if (sj == null) continue;
                    var d = new Vector2(sj.Horizontal, sj.Vertical);
                    characterEntity.UpdateDirectionalAim(d, sj.skillIndex);
                }
            }
#if FUSION2
            UpdatePvpScoreboard();
#endif
        }

        /// <summary>
        /// Shows the final boss message and triggers camera shake.
        /// </summary>
        public void ShowFinalBossMessage()
        {
            if (finalBossMessage != null)
            {
                finalBossMessage.SetActive(true);
                HideFinalBossMessageAsync(timeToCloseMessage, this.GetCancellationTokenOnDestroy()).Forget();
                if (TopDownCameraController.Singleton != null)
                {
                    TopDownCameraController.Singleton.TriggerCameraShake();
                }
            }
        }

        private async UniTaskVoid HideFinalBossMessageAsync(float delay, CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            finalBossMessage.SetActive(false);
            timerContainer.SetActive(false);
            bossHpContainer.SetActive(true);
        }

        /// <summary>
        /// Displays the power-up choices when leveling up.
        /// </summary>
        public void OnLevelUpChoicePowerUp()
        {
            foreach (Transform child in perkContainer) Destroy(child.gameObject);

            List<object> perks = GameplayManager.Singleton.GetRandomPerks();
            foreach (var perk in perks)
            {
                var entry = Instantiate(perkEntryPrefab, perkContainer);
                if (perk is SkillPerkData sp) entry.SetupEntry(sp);
                else if (perk is StatPerkData st) entry.SetupEntry(st);
                else if (perk is SkillData bs) entry.SetupEntry(bs);
            }

            pauseMenu.SetActive(true);
            _perksOpen = true;
        }

        public void AddUpgradePoints()
        {
            upgradeAmount++;
            openUpgradesButton.interactable = true;
        }
        public void OnClickToChoicePowerUp()
        {
            if (upgradeAmount > 0)
            {
                if (!GameplayManager.Singleton.IsRunnerActive)
                    GameplayManager.Singleton.TogglePause();

                OnLevelUpChoicePowerUp();
                upgradeAmount--;
            }
            else
            {
                Debug.Log("No upgrades available to choose.");
                CloseChoicePowerUp();
            }
        }

        /// <summary>
        /// Continuously checks upgradeAmount and makes a GameObject blink when upgradeAmount is greater than 0.
        /// </summary>
        /// <param name="targetObject">The GameObject to blink.</param>
        public IEnumerator BlinkGameObjectWhileUpgrading(GameObject targetObject)
        {
            if (targetObject == null)
            {
                Debug.LogWarning("Target GameObject is null. Cannot blink.");
                yield break;
            }
            bool initialActiveState = targetObject.activeSelf;

            while (true) // Loop for the duration of the game
            {
                if (upgradeAmount > 0)
                {
                    targetObject.SetActive(!targetObject.activeSelf);
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    if (!targetObject.activeSelf != initialActiveState)
                    {
                        targetObject.SetActive(initialActiveState);
                    }
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Automatically selects a random power-up for the player.
        /// </summary>
        public void OnRandomChoicePowerUp()
        {
            // Get shuffled perks
            List<object> perks = GameplayManager.Singleton.GetRandomPerks();

            if (perks.Count > 0)
            {
                // Choose a random perk
                object randomPerk = perks[UnityEngine.Random.Range(0, perks.Count)];

                // Apply the chosen perk
                if (randomPerk is SkillPerkData skillPerk)
                {
                    int currentLevel = GameplayManager.Singleton.GetSkillLevel(skillPerk);
                    if (currentLevel < skillPerk.maxLevel)
                    {
                        GameplayManager.Singleton.SetPerkLevel(skillPerk); // Set the new level
                        characterEntity.ApplySkillPerk(skillPerk); // Apply the perk
                    }
                    else
                    {
                        Debug.LogWarning($"SkillPerk {skillPerk.name} is already at max level.");
                    }
                }
                else if (randomPerk is StatPerkData statPerk)
                {
                    int currentLevel = GameplayManager.Singleton.GetPerkLevel(statPerk);
                    if (currentLevel < GameplayManager.Singleton.maxLevelStatPerks)
                    {
                        GameplayManager.Singleton.SetPerkLevel(statPerk); // Set the new level
                        characterEntity.ApplyStatPerk(statPerk, currentLevel + 1); // Apply the stat perk
                    }
                    else
                    {
                        Debug.LogWarning($"StatPerk {statPerk.statType} is already at max level.");
                    }
                }
                else if (randomPerk is SkillData baseSkill)
                {
                    int currentLevel = GameplayManager.Singleton.GetBaseSkillLevel(baseSkill);
                    if (currentLevel < baseSkill.skillLevels.Count - 1)
                    {
                        GameplayManager.Singleton.LevelUpBaseSkill(baseSkill); // Level up the base skill
                    }
                    else
                    {
                        Debug.LogWarning($"BaseSkill {baseSkill.skillName} is already at max level.");
                    }
                }

                UpdateSkillPerkUI();
                UpdateStatPerkUI();

#if FUSION2
                var sync = GameplaySync.Instance;
                if (sync && characterEntity && GameplayManager.Singleton.IsRunnerActive)
                {
                    if (randomPerk is SkillPerkData sp)
                    {
                        int idx = Array.IndexOf(GameplayManager.Singleton.skillPerkData, sp);
                        if (idx >= 0) sync.SyncPerkChosen(characterEntity, GameplaySync.PerkKind.SkillPerk, idx);
                    }
                    else if (randomPerk is StatPerkData st)
                    {
                        int idx = Array.IndexOf(GameplayManager.Singleton.statPerkData, st);
                        if (idx >= 0) sync.SyncPerkChosen(characterEntity, GameplaySync.PerkKind.StatPerk, idx);
                    }
                    else if (randomPerk is SkillData bs)
                    {
                        int idx = Array.IndexOf(characterEntity.GetCharacterData().skills, bs);
                        if (idx >= 0) sync.SyncPerkChosen(characterEntity, GameplaySync.PerkKind.BaseSkill, idx);
                    }
                }
#endif
            }
            else
            {
                Debug.LogWarning("No perks available to choose from.");
            }

            CloseChoicePowerUp();
        }

        /// <summary>
        /// Closes the power-up choice menu and re-enables the button if there are remaining upgrades.
        /// </summary>
        public void CloseChoicePowerUp()
        {
            foreach (Transform child in perkContainer) Destroy(child.gameObject);

            if (GameplayManager.Singleton.upgradeMode == UpgradeMode.UpgradeOnButtonClick)
                openUpgradesButton.interactable = upgradeAmount > 0;
            else
                openUpgradesButton.interactable = false;

            bool online = GameplayManager.Singleton.IsRunnerActive;
            if (!online &&
               (GameplayManager.Singleton.upgradeMode == UpgradeMode.UpgradeOnLevelUp ||
                GameplayManager.Singleton.upgradeMode == UpgradeMode.UpgradeOnButtonClick))
            {
                GameplayManager.Singleton.TogglePause();
            }

            pauseMenu.SetActive(false);
            _perksOpen = false;
            UpdateSkillPerkUI();
            UpdateSkillIcons();
        }

        /// <summary>
        /// Sets the final boss entity for HP tracking.
        /// </summary>
        /// <param name="boss">The final boss monster entity.</param>
        public void SetFinalBoss(MonsterEntity boss)
        {
            finalBossEntity = boss;
        }

        /// <summary>
        /// Updates the skill perk UI elements.
        /// </summary>
        public void UpdateSkillPerkUI()
        {
            if (characterEntity != null && skillPerkImages != null)
            {
                for (int i = 0; i < skillPerkImages.Length; i++)
                {
                    SkillsPerkImage perkImage = skillPerkImages[i];

                    if (i < characterEntity.GetSkillsPerkData().Count)
                    {
                        // Display active skill perks
                        SkillPerkData skillPerk = characterEntity.GetSkillsPerkData()[i];
                        int perkLevel = GameplayManager.Singleton.GetSkillLevel(skillPerk);

                        // Set the perk icon
                        perkImage.perkIcon.sprite = skillPerk.icon;
                        perkImage.perkLevel.text = perkLevel.ToString();

                        // Activate max level icon if the skill perk is evolved
                        bool isEvolved = (perkLevel >= skillPerk.maxLevel) && skillPerk.hasEvolution && characterEntity.HasStatPerk(skillPerk.perkRequireToEvolveSkill);
                        perkImage.maxLevelPerkIcon.gameObject.SetActive(isEvolved);
                    }
                    else if (i < GameplayManager.Singleton.maxSkills)
                    {
                        // Available slot but no skill assigned
                        perkImage.perkIcon.sprite = unlockedSprite;
                        perkImage.perkLevel.text = "";
                        perkImage.maxLevelPerkIcon.gameObject.SetActive(false);
                    }
                    else
                    {
                        // Locked slots
                        perkImage.perkIcon.sprite = lockedSkillSprite;
                        perkImage.perkLevel.text = "";
                        perkImage.maxLevelPerkIcon.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the stat perk UI elements.
        /// </summary>
        public void UpdateStatPerkUI()
        {
            if (characterEntity != null && statPerkImages != null)
            {
                for (int i = 0; i < statPerkImages.Length; i++)
                {
                    StatPerkImage statPerkImage = statPerkImages[i];

                    if (i < characterEntity.GetStatsPerkData().Count)
                    {
                        // Display active stat perks
                        StatPerkData statPerk = characterEntity.GetStatsPerkData()[i];
                        int perkLevel = GameplayManager.Singleton.GetPerkLevel(statPerk);

                        // Set the stat perk icon
                        statPerkImage.perkIcon.sprite = statPerk.icon;
                        statPerkImage.perkLevel.text = perkLevel.ToString();

                        // Activate max level icon if the stat perk reaches max level
                        bool isMaxLevel = perkLevel >= GameplayManager.Singleton.maxLevelStatPerks;
                        statPerkImage.maxLevelStatIcon.gameObject.SetActive(isMaxLevel);
                    }
                    else if (i < GameplayManager.Singleton.maxStats)
                    {
                        // Available slot but no stat assigned
                        statPerkImage.perkIcon.sprite = unlockedSprite;
                        statPerkImage.perkLevel.text = "";
                        statPerkImage.maxLevelStatIcon.gameObject.SetActive(false);
                    }
                    else
                    {
                        // Locked slots
                        statPerkImage.perkIcon.sprite = lockedStatSprite;
                        statPerkImage.perkLevel.text = "";
                        statPerkImage.maxLevelStatIcon.gameObject.SetActive(false);
                    }
                }
            }
        }


        /// <summary>
        /// Sets the character entity and initializes related components.
        /// </summary>
        /// <param name="entity">The character entity to set.</param>
        public void SetCharacterEntity(CharacterEntity entity)
        {
            characterEntity = entity;

            if (characterEntity.IsNetworked)
            {
                reviveButton.gameObject.SetActive(false);
            }

            if (skillJoysticks != null)
            {
                foreach (SkillJoystick skillJoystick in skillJoysticks)
                {
                    skillJoystick.Setup(characterEntity);
                }
            }

            if (pcSkillController != null)
            {
                pcSkillController.Setup(characterEntity);
            }

            characterIcon.sprite = characterEntity.GetCharacterData().icon;
            UpdateSkillIcons();

#if FUSION2
            var runner = entity && entity.IsNetworked && entity.Object ? entity.Object.Runner : null;
            _localTeamId = (runner != null) ? GetTeamOf(runner, entity.Object.InputAuthority) : (byte)0;
            RefreshAlliesPanel();
#endif
        }

#if FUSION2
        public void ShowDeathEntry(NetworkObject nobj, CharacterEntity ce, int seconds)
        {
            if (!deathEntriesRoot || !deathEntryPrefab || !nobj) return;

            var id = nobj.Id;
            if (_deathEntries.TryGetValue(id, out var entry))
            {
                entry.SetSeconds(seconds);
                return;
            }

            var e = Instantiate(deathEntryPrefab, deathEntriesRoot);
            var icon = ce.GetCharacterData().icon;
            var nick = ce.PlayerNick.ToString();
            e.Setup(icon, nick, seconds);
            _deathEntries[id] = e;
        }

        public void UpdateDeathCountdown(NetworkObject nobj, int seconds)
        {
            if (!nobj) return;
            if (_deathEntries.TryGetValue(nobj.Id, out var e))
                e.SetSeconds(seconds);

            if (_allyEntries.TryGetValue(nobj.Id, out var al))
                al.SetDeadCountdown(seconds);
        }

        public void HideDeathEntry(NetworkObject nobj)
        {
            if (!nobj) return;
            if (_deathEntries.TryGetValue(nobj.Id, out var e))
            {
                Destroy(e.gameObject);
                _deathEntries.Remove(nobj.Id);
            }
        }

        public void ClearAllDeathEntries()
        {
            foreach (var kv in _deathEntries)
                if (kv.Value) Destroy(kv.Value.gameObject);
            _deathEntries.Clear();
        }

        private void RefreshAlliesPanel()
        {
            if (!alliesContainer || !allyEntryPrefab) return;

            foreach (Transform c in alliesContainer) Destroy(c.gameObject);
            _allyEntries.Clear();

#if UNITY_6000_0_OR_NEWER
            var all = FindObjectsByType<CharacterEntity>(FindObjectsSortMode.None);
#else
            var all = FindObjectsOfType<CharacterEntity>();
#endif
            foreach (var ce in all)
                NotifyCharacterSpawned(ce);
        }

        public void NotifyCharacterSpawned(CharacterEntity ce)
        {
            if (!alliesContainer || !allyEntryPrefab || !ce) return;
            if (!characterEntity) return;         
            if (ce == characterEntity) return;       
            if (ce.IsDead) return;

            var localRunner = characterEntity.IsNetworked && characterEntity.Object ? characterEntity.Object.Runner : null;
            if (ce.IsNetworked && ce.Object && localRunner)
            {
                byte otherTeam = GetTeamOf(localRunner, ce.Object.InputAuthority);
                if (otherTeam != _localTeamId) return;  
            }

            var nobj = ce.GetComponent<NetworkObject>();
            if (!nobj) return;

            var id = nobj.Id;
            if (_allyEntries.ContainsKey(id)) return;

            var entry = Instantiate(allyEntryPrefab, alliesContainer);
            var icon = ce.GetCharacterData()?.icon;
            var nick = ce.PlayerNick.ToString();
            entry.Setup(icon, nick);
            _allyEntries[id] = entry;

            float hpNorm = Mathf.Approximately(ce.GetMaxHP(), 0) ? 0 : ce.GetCurrentHP() / ce.GetMaxHP();
            entry.SetHP(hpNorm);
        }

        public void NotifyCharacterDespawned(CharacterEntity ce)
        {
            if (!ce) return;
            var nobj = ce.GetComponent<NetworkObject>();
            if (!nobj) return;

            if (_allyEntries.TryGetValue(nobj.Id, out var ui))
            {
                Destroy(ui.gameObject);
                _allyEntries.Remove(nobj.Id);
            }
        }

        public void UpdateAllyMiniHud(CharacterEntity ce, float hpNormalized)
        {
            var nobj = ce.GetComponent<NetworkObject>();
            if (!nobj) return;

            if (_allyEntries.TryGetValue(nobj.Id, out var ui))
                ui.SetHP(hpNormalized);
        }

        public void PushKillFeed(Sprite killerIcon, string killerNick,
                         Sprite victimIcon, string victimNick,
                         float lifeSeconds = 4f)
        {
            if (!killFeedContainer || !killFeedEntryPrefab) return;

            var e = Instantiate(killFeedEntryPrefab, killFeedContainer);
            e.Setup(killerIcon, killerNick, victimIcon, victimNick);

            _killFeedQueue.Enqueue(e.gameObject);
            while (_killFeedQueue.Count > killFeedMaxEntries)
                Destroy(_killFeedQueue.Dequeue());

            Destroy(e.gameObject, lifeSeconds);
        }

        private void UpdatePvpScoreboard()
        {
            if (!PvpSync.IsSpawnedReady)
                return;

            var pvp = PvpSync.Instance;
            if (!pvp) return;

            timerContainer.SetActive(false);

            if (tdmScoreRoot) tdmScoreRoot.SetActive(pvp.Mode == PVP.PvpModeType.TeamDeathmatch);
            if (arenaScoreRoot) arenaScoreRoot.SetActive(pvp.Mode == PVP.PvpModeType.Arena);
            if (brScoreRoot) brScoreRoot.SetActive(pvp.Mode == PVP.PvpModeType.BattleRoyale);

            switch (pvp.Mode)
            {
                case PVP.PvpModeType.TeamDeathmatch:
                    if (tdmTimerText) tdmTimerText.text = FormatSeconds(pvp.TimeLeft);
                    if (tdmTeamScores != null)
                    {
                        for (int i = 0; i < tdmTeamScores.Length && i < pvp.TeamCount; i++)
                            if (tdmTeamScores[i]) tdmTeamScores[i].text = pvp.GetTeamScore((byte)i).ToString();
                    }
                    break;

                case PVP.PvpModeType.Arena:
                    if (arenaKillLimitText) arenaKillLimitText.text = $"Goal {pvp.KillLimit}";
                    if (arenaTeamScores != null)
                    {
                        for (int i = 0; i < arenaTeamScores.Length && i < pvp.TeamCount; i++)
                            if (arenaTeamScores[i]) arenaTeamScores[i].text = pvp.GetTeamScore((byte)i).ToString();
                    }
                    break;

                case PVP.PvpModeType.BattleRoyale:
                    if (brAliveText)
                    {
                        int alive = 0;
#if UNITY_6000_0_OR_NEWER
                        var all = FindObjectsByType<CharacterEntity>(FindObjectsSortMode.None);
#else
                        var all = FindObjectsOfType<CharacterEntity>();
#endif
                        foreach (var ce in all) if (!ce.IsDead) alive++;
                        brAliveText.text = $"Alive: {alive}";
                    }
                    break;
            }
        }

#if FUSION2
        private static byte GetTeamOf(NetworkRunner runner, PlayerRef pref)
        {
            if (runner == null || !runner.IsRunning || PvpSync.Instance == null)
                return 0;

            var list = runner.ActivePlayers.OrderBy(p => p.RawEncoded).ToList();
            int idx = list.IndexOf(pref);
            if (idx < 0) return 0;

            int teams = Mathf.Max(1, PvpSync.Instance.TeamCount);
            return (byte)(idx % teams);
        }
#endif

#endif

        /// <summary>
        /// Toggles the pause menu and game state.
        /// </summary>
        public void OnClickTogglePauseGame()
        {
            if (GameplayManager.Singleton == null)
            {
                Debug.LogError("GameplayManager Singleton is not available.");
                return;
            }

            GameplayManager.Singleton.TogglePause();

            if (pauseMenu == null)
            {
                Debug.LogError("Pause menu GameObject is not assigned.");
                return;
            }

            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }

        /// <summary>
        /// Smoothly updates the HP bar fill amount over time using values from event.
        /// </summary>
        /// <param name="startFill">Previous fill amount (0 to 1).</param>
        /// <param name="endFill">New fill amount (0 to 1).</param>
        /// <param name="token">Cancellation token.</param>
        private async UniTaskVoid UpdateHpBarAsync(float startFill, float endFill, CancellationToken token)
        {
            float elapsedTime = 0f;
            float duration = 0.5f;

            while (elapsedTime < duration)
            {
                if (token.IsCancellationRequested)
                    return;

                float t = elapsedTime / duration;
                hpBar.fillAmount = Mathf.Lerp(startFill, endFill, t);
                elapsedTime += Time.deltaTime;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            hpBar.fillAmount = endFill;
        }

        /// <summary>
        /// Updates the skill icons in the UI.
        /// </summary>
        private void UpdateSkillIcons()
        {
            if (skillImages != null && characterEntity != null)
            {
                for (int i = 0; i < skillImages.Length; i++)
                {
                    SkillImage skillImage = skillImages[i];
                    if (skillImage.ImageComponent != null)
                    {
                        try
                        {
                            Sprite skillIcon = characterEntity.GetSkillIcon(skillImage.Index);
                            skillImage.ImageComponent.sprite = skillIcon;

                            // Get skill data
                            SkillData skillData = characterEntity.GetSkillData(skillImage.Index);
                            int currentLevel = GameplayManager.Singleton.GetBaseSkillLevel(skillData);
                            int maxLevelIndex = skillData.skillLevels.Count - 1;
                            bool isEvolved = skillData.skillLevels[currentLevel].isEvolved;

                            // Activate max level icon if the skill is evolved
                            skillImage.maxLevelSkillIcon.gameObject.SetActive(isEvolved);

                            skillImage.CooldownImage.fillAmount = 0;
                            skillImage.CooldownText.text = "";
                        }
                        catch (System.Exception ex)
                        {
                            Debug.Log($"Error fetching skill icon for index {skillImage.Index}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"ImageComponent of SkillImage at index {i} is null.");
                    }
                }
            }
        }

        private void UpdateSkillCooldowns()
        {
            if (characterEntity != null)
            {
                PlatformType currentPlatform = GameInstance.Singleton.GetCurrentPlatform();

                if (currentPlatform == PlatformType.PC && pcSkillController != null)
                {
                    for (int i = 0; i < skillImages.Length; i++)
                    {
                        SkillImage skillImage = skillImages[i];

                        if (skillImage.ImageComponent != null)
                        {
                            float cooldown = pcSkillController.GetCurrentCooldown(skillImage.Index);
                            float maxCooldown = pcSkillController.GetCooldownTime(skillImage.Index);

                            if (cooldown > 0 && maxCooldown > 0)
                            {
                                skillImage.CooldownImage.fillAmount = cooldown / maxCooldown;
                                skillImage.CooldownText.text = Mathf.Ceil(cooldown).ToString();
                            }
                            else
                            {
                                skillImage.CooldownImage.fillAmount = 0;
                                skillImage.CooldownText.text = "";
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"SkillImage at index {i} has no ImageComponent.");
                        }
                    }
                }
                else if (currentPlatform == PlatformType.Mobile && skillJoysticks != null && skillJoysticks.Length > 0)
                {
                    for (int i = 0; i < skillImages.Length; i++)
                    {
                        SkillImage skillImage = skillImages[i];

                        if (skillImage.ImageComponent != null)
                        {
                            SkillJoystick skillJoystick = null;
                            if (i < skillJoysticks.Length)
                                skillJoystick = skillJoysticks[i];

                            if (skillJoystick != null)
                            {
                                float cooldown = skillJoystick.GetCurrentCooldown();
                                float maxCooldown = skillJoystick.GetCooldownTime();

                                if (cooldown > 0 && maxCooldown > 0)
                                {
                                    skillImage.CooldownImage.fillAmount = cooldown / maxCooldown;
                                    skillImage.CooldownText.text = Mathf.Ceil(cooldown).ToString();
                                }
                                else
                                {
                                    skillImage.CooldownImage.fillAmount = 0;
                                    skillImage.CooldownText.text = "";
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"SkillImage at index {i} has no ImageComponent.");
                        }
                    }
                }
            }
        }

        public void ShowRevivePanel(int seconds)
        {
            if (!revivePanel) return;
            revivePanel.SetActive(true);
            UpdateReviveCountdown(seconds);
        }
        public void UpdateReviveCountdown(int seconds)
        {
            if (!reviveCountdownText) return;
            reviveCountdownText.text = seconds > 0 ? seconds.ToString() : "0";
        }
        public void HideRevivePanel()
        {
            if (revivePanel) revivePanel.SetActive(false);
        }

        public void Revive()
        {
            if (characterEntity.IsNetworked) return;
            if(GameplayManager.Singleton.reviveLimit > 1)
            {
                GameplayManager.Singleton.reviveLimit--;
                endGameScreen.SetActive(false);
                GameplayManager.Singleton.ResumeGame();
                characterEntity.CharacterRevive();
            } 
            if(GameplayManager.Singleton.reviveLimit <= 0 && reviveButton) reviveButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Displays the end game screen, showing victory or defeat based on the result.
        /// </summary>
        /// <param name="won">Set to true if the player won the game, false if they lost.</param>
        public void DisplayEndGameScreen(bool won)
        {
            if(reviveButton) reviveButton.gameObject.SetActive(false);

            if (endGameScreen == null) return;
            // Show the end game screen
            endGameScreen.SetActive(true);

            // Set the end game message text and color based on whether the player won or lost
            endGameMessage.text = won ? "You Win!" : "You Lose!";
            endGameMessage.color = won ? Color.green : Color.red;

            // Activate objects and play audio based on victory or defeat
            if (won)
            {
                // Activate Victory objects
                ActivateObjects(VictoryObjects);
                // Play Victory audio
                AudioManager.Singleton.PlayAudio(VictoryAudio, "master");
            }
            else
            {
                // Activate Defeat objects
                ActivateObjects(DefeatObjects);
                // Play Defeat audio
                AudioManager.Singleton.PlayAudio(DefeatAudio, "master");
            }
        }
        public void HideEndGameScreen()
        {
            if (endGameScreen) endGameScreen.SetActive(false);
        }

        public void ShowPvpEndScreen(PVP.PvpModeType mode, int winnerTeam, int[] finalScores = null, int myPlacement = -1)
        {
            if (!pvpEndRoot) return;
            pvpEndRoot.SetActive(true);

            if (tdmEndPanel) tdmEndPanel.SetActive(false);
            if (arenaEndPanel) arenaEndPanel.SetActive(false);
            if (brEndPanel) brEndPanel.SetActive(false);

            int myTeam = 0;
#if FUSION2
            var me = GetCharacterEntity();
            var runner = me && me.IsNetworked && me.Object ? me.Object.Runner : null;
            if (runner)
            {
                var list = runner.ActivePlayers.OrderBy(p => p.RawEncoded).ToList();
                int idx = list.IndexOf(me.Object.InputAuthority);
                int teams = Mathf.Max(1, PvpSync.IsSpawnedReady ? PvpSync.Instance.TeamCount : 1);
                if (idx >= 0 && teams > 0) myTeam = idx % teams;
            }
#endif

            switch (mode)
            {
                case PVP.PvpModeType.TeamDeathmatch:
                    if (tdmEndPanel) tdmEndPanel.SetActive(true);
                    if (tdmWinnerText) tdmWinnerText.text = (winnerTeam == myTeam) ? "Victory!" : $"Team {winnerTeam + 1} Win";
                    if (tdmScoreText && finalScores != null && finalScores.Length >= 2)
                        tdmScoreText.text = $"Score: {finalScores[0]} x {finalScores[1]}";
                    break;

                case PVP.PvpModeType.Arena:
                    if (arenaEndPanel) arenaEndPanel.SetActive(true);
                    if (arenaWinnerText) arenaWinnerText.text = (winnerTeam == myTeam) ? "Victory!" : $"Team {winnerTeam + 1} Win";
                    if (arenaDetailText) arenaDetailText.text = "Reached the kill limit first";
                    break;

                case PVP.PvpModeType.BattleRoyale:
                    if (brEndPanel) brEndPanel.SetActive(true);
                    if (myPlacement <= 0) myPlacement = 99;
                    if (brPlacementText) brPlacementText.text = $"Your placement: {myPlacement}º";
                    if (brTop3Badge) brTop3Badge.SetActive(myPlacement <= 3);
                    break;
            }
        }

        /// <summary>
        /// Activates all the objects in the given array.
        /// </summary>
        /// <param name="objects">Array of GameObjects to activate.</param>
        private void ActivateObjects(GameObject[] objects)
        {
            foreach (GameObject obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
#if FUSION2
            FusionLobbyManager.Instance.EndGameSession();
#endif
            GameManager.Singleton.ReturnToMainMenu();
        }
           

        /// <summary>
        /// Return CharacterEntity reference.
        /// </summary>
        public CharacterEntity GetCharacterEntity() => characterEntity;

        private CancellationTokenSource mpMessageCts;

        /// <summary>
        /// Displays a status message with the specified color and hides it after a delay.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void DisplayMPInsufficientMessage(string message)
        {
            insufficientMPText.text = message;
            mpMessageCts?.Cancel();
            mpMessageCts = new CancellationTokenSource();

            HideStatusMessageAfterDelayAsync(insufficientMPText, 2f, mpMessageCts.Token).Forget();
        }

        /// <summary>
        /// Async method to hide the status message after a delay.
        /// </summary>
        /// <param name="statusText">The TextMeshProUGUI component displaying the message.</param>
        /// <param name="delay">The delay before hiding the message.</param>
        /// <param name="token">Cancellation token to cancel if needed.</param>
        private async UniTaskVoid HideStatusMessageAfterDelayAsync(TextMeshProUGUI statusText, float delay, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                statusText.text = string.Empty;
            }
            catch (OperationCanceledException) { }
        }

    }
}
