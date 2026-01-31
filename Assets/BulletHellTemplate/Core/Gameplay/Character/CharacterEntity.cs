using BulletHellTemplate.Core.Events;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BulletHellTemplate.VFX;
using BulletHellTemplate.PVP;



#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate
{
    public class CharacterEntity : BaseCharacterEntity
    {
        [Header("Settings")]
        public float reviveInvincibleTime = 2.5f;
        public float decreaseHpBarDelay = 0.12f;
        public float decreaseHPBarSpeed = 1.5f;
        public float mpLerpSpeed = 4.0f;

        [Header("Transforms")]
        public Transform characterModelTransform;
        public Transform launchTransform;
        public Transform effectsTransform;
        public GameObject directionalAim;

        [Header("Managers")]
        [Tooltip("Reference to the indicator manager.")]
        public IndicatorManager indicatorManager;

        [Header("UI Elements")]
        public Image hpBar;
        public Image hpDecreaseBar;
        public Image mpBar;
        public Image shieldBar;
        public Image shieldIcon;
        public Image invincibleIcon;
        public Image buffIcon;
        public Image debuffIcon;
        public TextMeshProUGUI level;
        public TextMeshProUGUI HpText;
        public TextMeshProUGUI insufficientMPText;
        public TextMeshProUGUI playerNicknameText;

        [Header("PVP")]
        public GameObject[] teamObjects;
        public Image hpBarAlly;
        public Image hpBarEnemy;

       
        private bool isStunned = false;   
        public bool IsStunned => isStunned;
        private bool wasMoving;
        private Vector2 lastDir2D;

        public bool IsNetworked { get; private set; } = false;

        private CancellationTokenSource stunCts;

#if FUSION2
        private bool _visualsApplied = false;
        [HideInInspector][Networked, OnChangedRender(nameof(OnTeamChanged))] public byte TeamId { get; set; }
#else
        public int Team; // offline
#endif

#if FUSION2
        /* ---------- Network fields ---------- */
        [HideInInspector][Networked, OnChangedRender(nameof(OnNetworkDataChanged))]public byte CharacterId { get; set; }
        [HideInInspector][Networked, OnChangedRender(nameof(OnNetworkDataChanged))] public byte SkinId { get; set; }
        /// <summary>
        /// Networked player nickname (fixed-capacity string). Use .ToString() to read.
        /// </summary>
        [HideInInspector][Networked, OnChangedRender(nameof(OnNetworkDataChanged))]
        public NetworkString<_32> PlayerNick { get; set; }

        /* ---------- Network stats ---------- */
        [HideInInspector][Networked, OnChangedRender(nameof(OnMiniHudChanged))] public ushort NetHP { get; set; }
        [HideInInspector][Networked, OnChangedRender(nameof(OnMiniHudChanged))] public ushort NetHPMax { get; set; }
        [HideInInspector][Networked, OnChangedRender(nameof(OnMiniHudChanged))] public ushort NetMP { get; set; }
        [HideInInspector][Networked, OnChangedRender(nameof(OnMiniHudChanged))] public ushort NetMPMax { get; set; }
        [HideInInspector][Networked, OnChangedRender(nameof(OnMiniHudChanged))] public ushort NetShield { get; set; }
        [HideInInspector][Networked, OnChangedRender(nameof(OnMiniHudChanged))] public byte NetIcons { get; set; }

        private bool _wasNetworkedMoving = false;
        private Vector2 _lastSentDirection = Vector2.zero;

        private NetworkId _lastHitBy;

        private struct HitKey : System.IEquatable<HitKey>
        {
            public NetworkId Attacker;
            public ulong HitId;
            public bool Equals(HitKey other) => Attacker == other.Attacker && HitId == other.HitId;
            public override bool Equals(object obj) => obj is HitKey hk && Equals(hk);
            public override int GetHashCode() => (Attacker.GetHashCode() * 486187739) ^ HitId.GetHashCode();
        }
        private readonly Dictionary<HitKey, float> _seenPlayerHits = new();
        private const float PLAYER_HIT_TTL = 3f;
        private List<HitKey> _seenHitsTmp;

        private void PruneSeenPlayerHits()
        {
            if (_seenPlayerHits.Count == 0) return;
            float now = Time.time;
            _seenHitsTmp ??= new List<HitKey>(8);
            _seenHitsTmp.Clear();
            foreach (var kv in _seenPlayerHits)
                if (now - kv.Value > PLAYER_HIT_TTL)
                    _seenHitsTmp.Add(kv.Key);
            for (int i = 0; i < _seenHitsTmp.Count; i++)
                _seenPlayerHits.Remove(_seenHitsTmp[i]);
        }

        byte _cachedCid = byte.MaxValue;
        byte _cachedSkin = byte.MaxValue;
        private byte _cid = byte.MaxValue;
        private byte _skin = byte.MaxValue;
        private string _name = string.Empty;
        private byte _cachedTeam = 0;
#endif



        #region  -------------------------------  Init & Setup  -------------------------------
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            if (characterModelTransform == null) characterModelTransform = transform;
            if (launchTransform == null) launchTransform = transform;
            if (effectsTransform == null) effectsTransform = transform;

            shieldIcon.gameObject.SetActive(false);
            buffIcon.gameObject.SetActive(false);
            debuffIcon.gameObject.SetActive(false);
            invincibleIcon.gameObject.SetActive(false);
#if FUSION2
            IsNetworked = Runner != null && Runner.IsRunning;
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!GameplayManager.Singleton) return;

            if (!GameplayManager.Singleton.IsRunnerActive || GameplayManager.Singleton.IsLeader)
                GameplayManager.Singleton.ActiveCharactersList.Add(transform);

            GameplayManager.Singleton.RegisterAlive(this);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (!GameplayManager.Singleton) return;

            if (!GameplayManager.Singleton.IsRunnerActive || GameplayManager.Singleton.IsLeader)
                GameplayManager.Singleton.ActiveCharactersList.Remove(transform);

            GameplayManager.Singleton.UnregisterAlive(this);
#if FUSION2
            UIGameplay.Singleton?.NotifyCharacterDespawned(this);
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        private void OnPlayerDiedEvent(PlayerDiedEvent evt)
        {
            if (evt.Target != this) return;
            OnDeath();
        }

        /// <summary>
        /// Sets the character data and updates the character model.
        /// </summary>
        /// <param name="_characterData">The character data to set.</param>
        public async void SetCharacterData(CharacterData _characterData, int _skinIndex)
        {
            characterData = _characterData;
            skinIndex = _skinIndex;
            UpdateCharacterModel();

            if (directionalAim != null)
                directionalAim.SetActive(false);

#if FUSION2
            if (Runner != null && Runner.IsRunning && !Object.HasInputAuthority)
                return;
#endif
            playerNicknameText.text = PlayerSave.GetPlayerName();
            InitializeCharacter();

            if (UIGameplay.Singleton == null)
            {
                var hud = Instantiate(GameInstance.Singleton.GetUIGameplayForPlatform());
                if (hud.minimapImage) hud.minimapImage.sprite = hud.unlockedSprite;
            }
            UIGameplay.Singleton?.SetCharacterEntity(this);
            await GameplayManager.Singleton.SetupCharacterEntity(this);
            TopDownCameraController.Singleton.SetTarget(gameObject.transform);
            characterStatsComponent.SetUIGameplayStats();

#if FUSION2
            if (Runner && Runner.IsRunning && Object && Object.HasStateAuthority)
                PushMiniHudFromStats();
#endif
        }

#if FUSION2
        public void SetInitialNetworkData(byte cid, byte skin, string name, byte team = 0)
        {
            _cachedCid = cid;
            _cachedSkin = skin;
            _name = SanitizeNickname(name);
            _cachedTeam = team;
        }

        private void OnNetworkDataChanged()
        {
            ApplyNetworkVisuals();
        }

        public override void Spawned()
        {
            if (HasStateAuthority && _cachedCid != byte.MaxValue)
            {
                CharacterId = _cachedCid;
                SkinId = _cachedSkin;
                PlayerNick = _name;
                TeamId = _cachedTeam;
            }

            ApplyNetworkVisuals();

            if (Object.HasInputAuthority && UIGameplay.Singleton == null)
            {
                var hud = Instantiate(GameInstance.Singleton.GetUIGameplayForPlatform());
                if (hud.minimapImage) hud.minimapImage.sprite = hud.unlockedSprite;
            }
            UIGameplay.Singleton?.NotifyCharacterSpawned(this);
            OnMiniHudChanged();
            RefreshTeamObjects();
            GameplayManager.Singleton?.RegisterCharacter(this);
        }

        private void ApplyNetworkVisuals()
        {
            if (CharacterId == byte.MaxValue)
                return;

            bool changed = !_visualsApplied || _cid != CharacterId || _skin != SkinId;

            if (!changed)
                return;

            if (playerNicknameText != null)
                playerNicknameText.text = PlayerNick.ToString();

            _visualsApplied = true;
            _cid = CharacterId;
            _skin = SkinId;

            var data = GameInstance.Singleton.GetCharacterDataById(_cid);
            if (data == null)
            {
                Debug.LogError($"CharacterData id {_cid} not found.");
                return;
            }

            characterData = data;
            skinIndex = _skin;
            UpdateCharacterModel();

            if (directionalAim != null)
                directionalAim.SetActive(false);

            SelectHpBarVariant();
            InitializeCharacter();
            RefreshTeamObjects();
        }
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_RequestPlaceAtTeamSpawn(byte team, RpcInfo info = default)
        {
            var gm = GameplayManager.Singleton;
            Vector3 pos = gm ? gm.GetTeamSpawnPosition(team) : Vector3.zero;
            Quaternion rot = gm ? gm.GetTeamSpawnRotation(team) : Quaternion.identity;

            if (characterControllerComponent)
                characterControllerComponent.Teleport(pos, rot, snapToGround: true);
            else
                transform.SetPositionAndRotation(pos, rot);

            RPC_OnPlacedForOwner();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void RPC_OnPlacedForOwner()
        {
            characterControllerComponent?.ResumeMovement();
            characterAttackComponent?.ResumeAutoAttack();
            if (LoadingManager.Singleton && LoadingManager.Singleton.isLoading)
                LoadingManager.Singleton.Close();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestSetNickname(string nick)
        {
            if (!Object || !Object.HasStateAuthority)
                return;
            PlayerNick = SanitizeNickname(nick);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_AssignTeam(byte team)
        {
            TeamId = team;
            Debug.Log($"[PVP] TeamId set -> {team} ({PlayerNick})");
        }

        private void OnTeamChanged()
        {
            RefreshTeamObjects();
            SelectHpBarVariant();
        }

        public void RefreshTeamObjects()
        {
            bool isPvp = GameplayManager.Singleton && GameplayManager.Singleton.IsPvp;

            if (teamObjects == null || teamObjects.Length == 0) return;

            int myTeam = 0;
#if FUSION2
            myTeam = TeamId;
#else
    myTeam = Team;
#endif

            for (int i = 0; i < teamObjects.Length; i++)
            {
                if (!teamObjects[i]) continue;
                teamObjects[i].SetActive(isPvp && i == myTeam);
            }
            SelectHpBarVariant();
        }

        /// <summary>
        /// Sets the nickname locally; in online sessions it propagates to StateAuthority.
        /// </summary>
        /// <param name="newNick">Desired nickname</param>
        /// <param name="propagateIfOnline">If true and online, requests host to update the networked property.</param>
        public void SetNickname(string newNick, bool propagateIfOnline = true)
        {
            string sanitized = SanitizeNickname(newNick);

            if (Runner && Object)
            {
                if (Object.HasStateAuthority)
                {
                    PlayerNick = sanitized;
                }
                else if (Object.HasInputAuthority && propagateIfOnline)
                {
                    RPC_RequestSetNickname(sanitized);
                }
            }
            else
            {
                if (playerNicknameText != null)
                    playerNicknameText.text = sanitized;
            }
        }

        /// <summary>
        /// Trims and clamps nickname length.
        /// </summary>
        private static string SanitizeNickname(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Player";

            string trimmed = raw.Trim();
            return trimmed.Length > 31 ? trimmed.Substring(0, 31) : trimmed;
        }

        private void OnMiniHudChanged()
        {
            if (Object.HasStateAuthority) return;

            if (characterUIHandlerComponent != null)
            {
                characterUIHandlerComponent.SetHpImmediate(NetHP, NetHPMax);
                characterUIHandlerComponent.SetMpImmediate(NetMP, NetMPMax);
                characterUIHandlerComponent.SetShieldImmediate(NetShield, NetHPMax);
            }

            if (UIGameplay.Singleton)
            {
                float hpNorm = NetHPMax > 0 ? (float)NetHP / NetHPMax : 0f;
                UIGameplay.Singleton.UpdateAllyMiniHud(this, hpNorm);
            }

            invincibleIcon?.gameObject.SetActive((NetIcons & 1) != 0);
            buffIcon?.gameObject.SetActive((NetIcons & 2) != 0);
            debuffIcon?.gameObject.SetActive((NetIcons & 4) != 0);
        }

        private void SelectHpBarVariant()
        {
            bool isLocalOwner =
#if FUSION2
            Object && Object.HasInputAuthority;
#else
        true;
#endif
            if (hpBar) hpBar.gameObject.SetActive(false);
            if (hpBarAlly) hpBarAlly.gameObject.SetActive(false);
            if (hpBarEnemy) hpBarEnemy.gameObject.SetActive(false);

            Image chosen = hpBar;

            if (isLocalOwner || !IsNetworked)
            {
                if (hpBar) hpBar.gameObject.SetActive(true);
                chosen = hpBar ?? chosen;
            }
            else
            {
                bool isPvp = GameplayManager.Singleton && GameplayManager.Singleton.IsPvp;
                bool isAlly = true;

#if FUSION2
                if (isPvp)
                {
                    byte localTeam = 255;
                    CharacterEntity local = null;
#if UNITY_6000_0_OR_NEWER
                    var all = FindObjectsByType<CharacterEntity>(FindObjectsSortMode.None);
#else
                    var all = FindObjectsOfType<CharacterEntity>();
#endif
                    foreach (var c in all)
                        if (c && c.Object && c.Object.HasInputAuthority) { local = c; break; }
                    if (local) localTeam = local.TeamId;
                    isAlly = local && TeamId == localTeam;
                }
                else
                {
                    isAlly = true;
                }
#endif
                if (isAlly && hpBarAlly)
                {
                    hpBarAlly.gameObject.SetActive(true);
                    chosen = hpBarAlly;
                }
                else if (!isAlly && hpBarEnemy)
                {
                    hpBarEnemy.gameObject.SetActive(true);
                    chosen = hpBarEnemy;
                }
                else
                {
                    if (hpBar) hpBar.gameObject.SetActive(true);
                    chosen = hpBar ?? chosen;
                }
            }
            if (chosen != null) hpBar = chosen;
        }


        /// <summary>
        /// Pushes current HP/MP/Shield and status icons to networked mini-HUD (authority only).
        /// </summary>
        private void PushMiniHudFromStats()
        {
            if (!Object || !Object.HasStateAuthority) return;

            ushort hp = ToUShort(characterStatsComponent.CurrentHP);
            ushort hpMax = ToUShort(characterStatsComponent.MaxHp);
            ushort mp = ToUShort(characterStatsComponent.CurrentMP);
            ushort mpMax = ToUShort(characterStatsComponent.MaxMp);
            ushort shield = ToUShort(characterStatsComponent.CurrentShield);

            bool inv = isInvincible;
            bool buff = activeBuffCount > 0;
            bool debuf = false; 

            PushMiniHud(hp, hpMax, mp, mpMax, shield, inv, buff, debuf);
        }
        public void PushMiniHud(ushort hp, ushort hpMax, ushort mp, ushort mpMax, ushort shield, bool inv, bool buff, bool debuff)
        {
            if (!Object || !Object.HasStateAuthority) return;

            NetHP = hp;
            NetHPMax = hpMax;
            NetMP = mp;
            NetMPMax = mpMax;
            NetShield = shield;

            byte flags = 0;
            if (inv) flags |= 1 << 0;
            if (buff) flags |= 1 << 1;
            if (debuff) flags |= 1 << 2;
            NetIcons = flags;
        }
        private static ushort ToUShort(float v)
        {
            int iv = Mathf.RoundToInt(v);
            if (iv < 0) iv = 0;
            if (iv > 65535) iv = 65535;
            return (ushort)iv;
        }

 
        /// <summary>
        /// Server-side RPC that validates the caller and deduplicates each hit before applying.
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestPlayerDamage(float amount, bool critical, NetworkId attackerId, ulong hitId, RpcInfo info = default)
        {
            if (!Object || !Object.HasStateAuthority) return;
            if (amount <= 0f) return;

            var src = info.Source;
            if (attackerId.IsValid && Runner.TryFindObject(attackerId, out var attackerObj))
            {
                if (src != attackerObj.InputAuthority && src != Object.InputAuthority)
                    return;
            }
            var key = new HitKey { Attacker = attackerId, HitId = hitId };
            PruneSeenPlayerHits();
            if (_seenPlayerHits.ContainsKey(key)) return;

            _seenPlayerHits[key] = Time.time;
            if (attackerId.IsValid) _lastHitBy = attackerId;
            ApplyDamageAuthority(ScalePvpDamageIfNeeded(attackerId, amount), critical);

        }
#endif

#if FUSION2
        private float ScalePvpDamageIfNeeded(NetworkId attackerId, float raw)
        {
            if (!PvpSync.Instance) return raw;
            if (!attackerId.IsValid) return raw;

            if (Runner.TryFindObject(attackerId, out var attObj))
            {
                if (attObj && attObj.GetComponent<CharacterEntity>())
                {
                    var mult = Mathf.Clamp01(PvpSync.Instance.PlayerDamageTakenMultiplier);
                    return raw * mult;
                }
            }
            return raw;
        }
#endif

#if FUSION2
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnDamagePopup(int amount, bool critical)
        {
            DamagePopup.Show(amount,transform.position + Vector3.up * 1.6f, critical);
        }
#endif

        /// <summary>
        /// Instantiates the character model from CharacterData and sets up the _anim reference.
        /// </summary>
        public void UpdateCharacterModel()
        {
            if (characterData == null)
            {
                Debug.LogError("CharacterData is null.");
                return;
            }

            // Destroy any existing child models to avoid duplicates
            foreach (Transform child in characterModelTransform)
            {
                Destroy(child.gameObject);
            }

            // Choose model from skins or default
            CharacterModel selectedModel = null;

            if (characterData.characterSkins != null &&
                characterData.characterSkins.Length > 0 &&
                skinIndex >= 0 &&
                skinIndex < characterData.characterSkins.Length)
            {
                selectedModel = characterData.characterSkins[skinIndex].skinCharacterModel;
            }
            else
            {
                selectedModel = characterData.characterModel;
            }

            // Instantiate model if valid
            if (selectedModel != null)
            {
                characterModel = Instantiate(selectedModel, characterModelTransform.position, characterModelTransform.rotation, characterModelTransform);

                // Try to find the Animator on the instantiated model
                if (!characterModel.TryGetComponent(out animator))
                {
                    animator = characterModel.GetComponentInChildren<Animator>();
                }

                if (animator == null)
                {
                    Debug.LogError("Animator component not found on the character model or its children.");
                }
            }
            else
            {
                Debug.LogError("No valid character model found in CharacterData.");
            }
        }

        /// <summary>
        /// Initializes all core character components using the assigned CharacterData.
        /// </summary>
        public void InitializeCharacter()
        {
            // Cache component references if not assigned
            if (characterStatsComponent == null)
                characterStatsComponent = GetComponent<CharacterStatsComponent>();

            if (characterControllerComponent == null)
                characterControllerComponent = GetComponent<CharacterControllerComponent>();

            if (characterAttackComponent == null)
                characterAttackComponent = GetComponent<CharacterAttackComponent>();

            if (characterBuffsComponent == null)
                characterBuffsComponent = GetComponent<CharacterBuffsComponent>();

            if(characterUIHandlerComponent == null)
                characterUIHandlerComponent = GetComponent<CharacterUIHandlerComponent>();

            // Validate essential components before initializing
            if (characterData == null)
            {
                Debug.LogWarning("CharacterData is null. Initialization aborted.");
                return;
            }

            if (characterUIHandlerComponent != null)
                characterUIHandlerComponent.Initialize(this);
            else
                Debug.LogWarning("CharacterUIHandlerComponent not found.");

            if (characterStatsComponent != null)
                characterStatsComponent.Initialize(characterData);
            else
                Debug.LogWarning("CharacterStatsComponent not found.");

            if (characterAttackComponent != null)
                characterAttackComponent.Initialize(characterData, launchTransform, effectsTransform);
            else
                Debug.LogWarning("CharacterAttackComponent not found.");

            if (characterBuffsComponent != null)
                characterBuffsComponent.Initialize();
            else
                Debug.LogWarning("CharacterBuffsComponent not found.");
         
        }
#endregion
        #region  -------------------------------  Update Loop  -------------------------------      

        private void UpdateStatPerkUI()
        {
            UIGameplay.Singleton.UpdateSkillPerkUI();
        }

        #endregion

        #region  -------------------------------  Skill & Attack  -------------------------------

        /// <summary>
        /// Executes the auto-attack action using the AutoAttack skill from characterData.
        /// </summary>
        public override void Attack()
        {
            if (isStunned) return;
            base.Attack();

            if (characterData == null || characterData.autoAttack == null)
            {
                Debug.LogWarning("Cannot perform auto-attack: characterData or autoAttack skill is null.");
                return;
            }

            if (GameplayManager.Singleton.IsPaused() || !canAutoAttack)
                return;

            characterAttackComponent.Attack();

        }

        /// <summary>
        /// Uses the skill at the specified index and launches it in the specified direction.
        /// The skill is launched from the launchTransform in the direction specified.
        /// </summary>
        /// <param name="index">The index of the skill to use.</param>
        /// <param name="inputDirection">The input direction from a joystick or other source.</param>
        public override void UseSkill(int index, Vector2 inputDirection)
        {
            if (isStunned) return;
            base.UseSkill(index, inputDirection);

            if (characterData == null || characterData.skills == null)
            {
                Debug.LogWarning("Cannot use skill: characterData or skills array is null.");
                return;
            }

            if (index < 0 || index >= characterData.skills.Length)
            {
                Debug.LogWarning($"Skill index {index} is out of range.");
                return;
            }

            SkillData skill = characterData.skills[index];

            if (skill == null)
            {
                Debug.LogWarning($"Skill at index {index} is null.");
                return;
            }

            if (GameplayManager.Singleton.IsPaused())
                return;

            if (!ConsumeMP(skill.manaCost))
            {
                DisplayMPInsufficientMessage("Not enough MP to use skill.");
                return;
            }

            characterAttackComponent.UseSkill(index, skill, inputDirection);
        }

        public void PlayAttack()
        {
            if (characterModel == null) return;
#if FUSION2
            if (Runner)
            {
                if (!Object.HasInputAuthority || isStunned) return;
                RPC_PlayAttack();
            }        
#endif
            EventBus.Publish(new AnimationOnActionEvent(characterModel, true));
        }

        public void PlaySkill(int skillIndex)
        {
            if (characterModel == null) return;
#if FUSION2
            if (Runner)
            {
                if (!Object.HasInputAuthority || isStunned) return;
                RPC_PlaySkill(skillIndex);
            }
#endif
            EventBus.Publish(new AnimationOnActionEvent(characterModel, false, skillIndex));
        }

        public void PlayReceiveDamage()
        {
            if (characterModel == null) return;
            EventBus.Publish(new AnimationOnReceiveDamageEvent(characterModel));
        }

        public void PlayDeath()
        {
            if (characterModel == null) return;
            EventBus.Publish(new AnimationOnDiedEvent(characterModel));
        }

#if FUSION2
        //─────────────────────── RPCs ───────────────────────//
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        /// <summary>Play auto‐attack animation on all clients.</summary>
        public void RPC_PlayAttack()
        {
            if (Object.HasInputAuthority) return;
            EventBus.Publish(new AnimationOnActionEvent(characterModel, true));
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        /// <summary>Play skill animation on all clients.</summary>
        public void RPC_PlaySkill(int skillIndex)
        {
            if (Object.HasInputAuthority) return;
            EventBus.Publish(new AnimationOnActionEvent(characterModel, false, skillIndex));
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        /// <summary>Play receive‐damage animation on all clients.</summary>
        public void RPC_PlayReceiveDamage()
        {
            if (Object.HasInputAuthority) return;
            EventBus.Publish(new AnimationOnReceiveDamageEvent(characterModel));
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayMoveAnim(bool moving, Vector2 dir)
        {
            if (Object.HasInputAuthority)
                return;

            if (moving)
                EventBus.Publish(new AnimationOnRunEvent(characterModel, dir));
            else
                EventBus.Publish(new AnimationOnIdleEvent(characterModel));
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_ApplyRotateCharacterModel(Vector3 dir, float duration)
        {
            if (Object.HasInputAuthority)
                return;

            RotateCharacterModelAsync(dir, duration, this.GetCancellationTokenOnDestroy()).Forget();
        }
#endif

        /// <summary>
        /// Resets the ability to auto-attack after a delay.
        /// </summary>
        /// <param name="delay">The time to wait before resetting auto-attack.</param>
        public void ApplyResetAutoAttack(float delay)
        {
            ResetAutoAttackAsync(0.8f, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Applies a skill perk and manages its execution based on level and cooldown.
        /// </summary>
        /// <param name="skillPerk">The skill perk to apply.</param>
        public void ApplySkillPerk(SkillPerkData skillPerk)
        {
            characterAttackComponent.ApplySkillPerk(skillPerk, false);
        }

        #endregion

        #region  -------------------------------  Stats / Buffs / Debuffs  -------------------------------

        public void ApplyStatPerk(StatPerkData statPerk, int level) => characterStatsComponent.ApplyStatPerk(statPerk, level);

        /// <summary>
        /// Tries to consume the specified amount of MP. Returns true if successful.
        /// </summary>
        /// <param name="amount">Amount of mana to consume.</param>
        /// <returns>True if mana was consumed, false if insufficient mana.</returns>
        public bool ConsumeMP(float amount) => characterStatsComponent.ConsumeMP(amount);

        private void IncrementBuffCount()
        {
            activeBuffCount++;
        }

        private void DecrementBuffCount()
        {
            activeBuffCount--;
            activeBuffCount = Mathf.Max(activeBuffCount, 0);
        }

        /// <summary>
        /// Applies a positive move speed buff and reverts after duration.
        /// Ensures the amount is always positive.
        /// </summary>
        public void ApplyMoveSpeedBuff(float amount, float duration)
        {
            amount = Mathf.Abs(amount);
            characterBuffsComponent.ApplyMoveSpeedBuff(amount, duration);
        }

        /// <summary>
        /// Applies a negative move speed debuff and reverts after duration.
        /// Ensures the amount is always negative.
        /// </summary>
        public void ReceiveMoveSpeedDebuff(float amount, float duration)
        {
            amount = -Mathf.Abs(amount);
            characterBuffsComponent.ApplyMoveSpeedBuff(amount, duration);
        }

        /// <summary>
        /// Temporarily increases the character's attack speed and reverts after duration.
        /// </summary>
        public void ApplyAttackSpeedBuff(float amount, float duration)
        {
            characterBuffsComponent.ApplyAttackSpeedBuff(amount, duration);
        }

        /// <summary>
        /// Temporarily increases the character's defense and reverts after duration.
        /// </summary>
        public void ApplyDefenseBuff(float amount, float duration)
        {
            characterBuffsComponent.ApplyDefenseBuff(amount, duration);
        }

        /// <summary>
        /// Temporarily increases the character's damage and reverts after duration.
        /// </summary>
        public void ApplyDamageBuff(float extraDamage, float duration)
        {
            characterBuffsComponent.ApplyDamageBuff(extraDamage, duration);
        }

        /// <summary>
        /// Increases the shield of the character.
        /// </summary>
        /// <param name="amount">The amount of shield to increase.</param>
        public void ApplyShield(int amount)
        {
            characterBuffsComponent.ApplyShield(amount);
        }

        /// <summary>
        /// Increases the shield of the character for a specified duration.
        /// </summary>
        /// <param name="amount">The amount of shield to increase.</param>
        /// <param name="duration">Duration in seconds for which the shield remains.</param>
        /// <param name="isTemporary">Set to true if the buff is temporary. Default is false. </param>
        public void ApplyShield(int amount, float duration = 0f, bool isTemporary = false)
        {
            characterBuffsComponent.ApplyShield(amount, duration, isTemporary);
        }

        /// <summary>
        /// Increases the maximum HP of the character.
        /// </summary>
        /// <param name="amount">Amount to increase max HP by.</param>
        public void AddMaxHP(int amount)
        {
            amount = Mathf.Abs(amount);
            characterStatsComponent.AlterMaxHp(amount);
        }

        /// <summary>
        /// Decreases the maximum HP of the character.
        /// </summary>
        /// <param name="amount">Amount to increase max HP by.</param>
        public void RemoveMaxHP(int amount)
        {
            amount = -Mathf.Abs(amount);
            characterStatsComponent.AlterMaxHp(amount);
        }

        /// <summary>
        /// Sets the character invincible for a duration.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>     
        public override void ApplyInvincible(float duration)
        {
            isInvincible = true;
            RemoveInvincibilityAfterAsync(duration, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Heals the character by a specified amount up to their maximum health.
        /// </summary>
        /// <param name="amount">The amount to heal.</param>
        public void ApplyHealHP(int amount)
        {
            amount = Mathf.Abs(amount);
            characterStatsComponent.HealHP(amount);
        }

        /// <summary>
        /// Heals the character's MP.
        /// </summary>
        /// <param name="amount">The amount to heal.</param>
        public void ApplyHealMP(int amount)
        {
            amount = Mathf.Abs(amount);
            characterStatsComponent.HealMP(amount);
        }

        /// <summary>
        /// Applies health leech based on the damage dealt.
        /// </summary>
        /// <param name="damage">The total damage dealt.</param>
        public void ApplyHpLeech(float damage)
        {
            characterStatsComponent.ApplyHpLeech(damage);
        }

        /// <summary>
        /// Increases the cooldown reduction percentage.
        /// </summary>
        /// <param name="amount">The amount to increase cooldown reduction by.</param>
        public void AddCooldownReduction(float amount)
        {
            amount = Mathf.Abs(amount);
            characterStatsComponent.AlterCooldownReduction(amount);
        }

        /// <summary>
        /// Decreases the cooldown reduction percentage.
        /// </summary>
        /// <param name="amount">The amount to increase cooldown reduction by.</param>
        public void RemoveCooldownReduction(float amount)
        {
            amount = -Mathf.Abs(amount);
            characterStatsComponent.AlterCooldownReduction(amount);
        }

        /// <summary>
        /// Adds XP to the character and handles leveling up.
        /// </summary>
        /// <param name="xpAmount">The amount of XP to add.</param>
        public void AddXP(int xpAmount)
        {
            xpAmount = Mathf.Abs(xpAmount);
            characterStatsComponent.AddXP(xpAmount);
        }

        public void ReceiveKnockback(Vector3 senderDirection,float distance, float duration)
        {
            characterControllerComponent.ApplyKnockback(senderDirection,distance,duration);
        }

        public void ReceiveStun(float duration)
        {
            stunCts?.Cancel();                  
            stunCts = new CancellationTokenSource();
            StunRoutine(duration, stunCts.Token).Forget();
        }

        public void ReceiveDot(MonsterSkill sourceSkill, int totalDmg, float duration)
        {
            characterStatsComponent.ApplyDot(sourceSkill, totalDmg, duration);
        }


        #endregion

        #region  -------------------------------  Animation & Movement  -------------------------------

        /// <summary>
        /// Moves the character and handles animation locally. Triggers animation RPC only when state changes.
        /// </summary>
        /// <param name="worldDir">Direction to move in world space.</param>
        public override void Move(Vector3 worldDir)
        {
            base.Move(worldDir);

            bool isMoving = worldDir.sqrMagnitude > 0.01f;
            Vector2 currentDir2D = Vector2.zero;

            if (isMoving)
            {
                Vector3 local3D = characterModelTransform.InverseTransformDirection(new Vector3(worldDir.x, 0f, worldDir.z));
                currentDir2D = new Vector2(local3D.x, local3D.z).normalized;
            }

            // ─── Always trigger animation locally ──────────────────────
            if (isMoving)
                EventBus.Publish(new AnimationOnRunEvent(characterModel, currentDir2D));
            else
                EventBus.Publish(new AnimationOnIdleEvent(characterModel));

#if FUSION2
            if (Runner && Object.HasStateAuthority)
            {
                bool sendMoveRpc = false;

                // Start/stop movement change
                if (isMoving != _wasNetworkedMoving)
                {
                    sendMoveRpc = true;
                }

                // Direction change while moving
                if (isMoving && Vector2.Distance(currentDir2D, _lastSentDirection) > 0.1f)
                {
                    sendMoveRpc = true;
                }

                if (sendMoveRpc)
                {
                    _wasNetworkedMoving = isMoving;
                    _lastSentDirection = currentDir2D;

                    RPC_PlayMoveAnim(isMoving, currentDir2D);
                }
            }
#endif

            characterControllerComponent.Move(worldDir);
            wasMoving = isMoving;
        }

        public void ApplyStopMovement(float duration, bool allowRotation = false)
        {
            StopMovementAsync(duration, allowRotation, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Smoothly rotates the visible model to a direction.
        /// Cancels any previous rotation to avoid jitter.
        /// </summary>
        public void ApplyRotateCharacterModel(Vector3 dir, float duration, CancellationToken token)
        {
            RotateCharacterModelAsync(dir, duration, token).Forget();

#if FUSION2
            //if (Runner && Object.HasInputAuthority)
            //    RPC_ApplyRotateCharacterModel(dir, duration);
#endif
        }

        #endregion

        #region  -------------------------------  UI  -------------------------------

        /// <summary>
        /// Displays a status message related to insufficient MP or other notifications.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void DisplayMPInsufficientMessage(string message)
        {
            insufficientMPText.text = message;
            HideStatusMessageAfterDelayAsync(insufficientMPText, 1.5f, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Updates the directional aim's rotation based on the joystick input and updates the corresponding indicator.
        /// </summary>
        /// <param name="joystickDirection">The direction from the skill joystick.</param>
        /// <param name="skillIndex">Index of the skill being aimed.</param>
        public void UpdateDirectionalAim(Vector2 joystickDirection, int skillIndex)
        {
            if (directionalAim == null)
            {
                Debug.LogWarning("Directional aim object is not set.");
                return;
            }

            // Rotate visual
            if (joystickDirection.magnitude > 0)
            {
                Vector3 direction3D = new Vector3(joystickDirection.x, 0, joystickDirection.y).normalized;
                directionalAim.transform.rotation = Quaternion.LookRotation(direction3D, Vector3.up);
            }

            if (!directionalAim.activeSelf || indicatorManager == null)
                return;

            SkillData skillData = GetSkillData(skillIndex);
            if (skillData == null || skillData.rangeIndicatorType == RangeIndicatorType.None)
                return;

            float maxRange = skillData.rangeIndicatorType switch
            {
                RangeIndicatorType.RadialAoE => skillData.radialAoEIndicatorSettings.radiusMaxRange,
                RangeIndicatorType.Radial => skillData.radialIndicatorSettings.radiusArea,
                _ => 1f
            };

            Vector3 aimWorldPos = GetAimWorldPosition(joystickDirection, maxRange);

            switch (skillData.rangeIndicatorType)
            {
                case RangeIndicatorType.Radial:
                    indicatorManager.ShowCircleRangeIndicator(maxRange);
                    break;

                case RangeIndicatorType.RadialAoE:
                    indicatorManager.ShowCircleRangeIndicator(skillData.radialAoEIndicatorSettings.radiusMaxRange);
                    indicatorManager.ShowPositionAoECircle(skillData.radialAoEIndicatorSettings.radiusArea, aimWorldPos);
                    break;

                case RangeIndicatorType.Arrow:
                    indicatorManager.ShowArrowIndicator();
                    indicatorManager.UpdateArrowIndicator(transform.position, aimWorldPos.magnitude, maxRange, directionalAim.transform.rotation);
                    break;

                case RangeIndicatorType.Cone:
                    indicatorManager.ShowConeIndicator();
                    indicatorManager.UpdateConeIndicator(transform.position, aimWorldPos.magnitude, maxRange, directionalAim.transform.rotation);
                    break;
            }
        }

        /// <summary>
        /// Activates or deactivates the directional aim and manages indicator animations.
        /// </summary>
        /// <param name="isActive">True to activate the directional aim; false to deactivate.</param>
        /// <param name="skillIndex">Index of the skill being used.</param>
        /// <param name="skillUsed">True if the skill was used (triggering cast indicator animation); false if canceled.</param>
        public void SetDirectionalAimActive(bool isActive, int skillIndex, bool skillUsed)
        {
            if (directionalAim != null)
                directionalAim.SetActive(isActive);

            if (!isActive && indicatorManager != null)
            {
                SkillData skillData = GetSkillData(skillIndex);
                if (skillData == null || skillData.rangeIndicatorType == RangeIndicatorType.None)
                    return;

                indicatorManager.HidePositionAoECircle();

                if (!skillUsed)
                {
                    indicatorManager.CloseIndicators();
                    return;
                }

                float castDuration = skillData.delayToLaunch;
                Vector3 startPos = transform.position;
                Vector3 endPos = startPos + transform.forward * (
                    skillData.rangeIndicatorType == RangeIndicatorType.RadialAoE ?
                    skillData.radialAoEIndicatorSettings.radiusMaxRange : 1f
                );

                switch (skillData.rangeIndicatorType)
                {
                    case RangeIndicatorType.Radial:
                        if (skillData.radialIndicatorSettings.useCastIndicator)
                            indicatorManager.StartCastCircleIndicator(skillData.radialIndicatorSettings.radiusArea, castDuration);
                        else
                            indicatorManager.CloseIndicators();
                        break;

                    case RangeIndicatorType.RadialAoE:
                        if (skillData.radialAoEIndicatorSettings.useCastIndicator)
                        {
                            indicatorManager.StartCastDamageIndicator(skillData.radialAoEIndicatorSettings.radiusArea, castDuration);
                            indicatorManager.ShowAoECurveLine(startPos, endPos);
                            StartCoroutine(indicatorManager.HideAoECurveLineAfterDelay(castDuration + 0.1f));
                        }
                        else
                        {
                            indicatorManager.CloseIndicators();
                        }
                        break;

                    case RangeIndicatorType.Arrow:
                        if (skillData.arrowIndicatorSettings.useCastIndicator)
                            indicatorManager.StartCastArrowIndicator(skillData.arrowIndicatorSettings.arrowSize, castDuration);
                        else
                            indicatorManager.CloseIndicators();
                        break;

                    case RangeIndicatorType.Cone:
                        if (skillData.coneIndicatorSettings.useCastIndicator)
                            indicatorManager.StartCastConeIndicator(skillData.coneIndicatorSettings.ConeSize, castDuration);
                        else
                            indicatorManager.CloseIndicators();
                        break;
                }
            }
        }


        #endregion

        #region  -------------------------------  Damage & Life Cycle  -------------------------------

        /// <summary>
        /// Public entry point: only owner/authority should cause damage; dedup by (attacker, hitId).
        /// Offline falls back to local application.
        /// </summary>
        public void ApplyDamageToSelfFromHit(
#if FUSION2
            NetworkObject attacker,
#else
            object attacker,
#endif
            ulong hitId, float amount, bool critical)
        {
#if FUSION2
            if (Runner == null || !Runner.IsRunning || Object == null)
            {
                ApplyDamageAuthority(ScalePvpDamageIfNeeded(attacker ? attacker.Id : default, amount), critical);
                return;
            }

            if (Object.HasStateAuthority)
            {
                var key = new HitKey { Attacker = attacker ? attacker.Id : default, HitId = hitId };
                PruneSeenPlayerHits();
                if (_seenPlayerHits.ContainsKey(key)) return;
                _seenPlayerHits[key] = Time.time;
                
                if (attacker)
                    _lastHitBy = attacker.Id;

                ApplyDamageAuthority(ScalePvpDamageIfNeeded(attacker ? attacker.Id : default, amount), critical);
            }
            else
            {
                RPC_RequestPlayerDamage(amount, critical, attacker ? attacker.Id : default, hitId);
            }
#else
            ApplyDamageAuthority(amount, critical);
#endif
        }

        /// <summary>
        /// Applies damage on the authoritative instance and updates network/local UI.
        /// </summary>
        private void ApplyDamageAuthority(float amount, bool critical)
        {
            if (amount <= 0f) return;
            if (GameplayManager.Singleton.IsPaused()) return;
            if (isInvincible) return;

            characterStatsComponent.ReceiveDamage(amount);

#if FUSION2
            if (Runner && Object && Object.HasStateAuthority)
            {
                RPC_SpawnDamagePopup(Mathf.Max(Mathf.RoundToInt(amount)), critical);
                RPC_PlayReceiveDamage();
            }
#endif
            PlayReceiveDamage();

            if (characterUIHandlerComponent != null)
            {
                characterUIHandlerComponent.SetHpImmediate(characterStatsComponent.CurrentHP, characterStatsComponent.MaxHp);
                characterUIHandlerComponent.SetMpImmediate(characterStatsComponent.CurrentMP, characterStatsComponent.MaxMp);
                characterUIHandlerComponent.SetShieldImmediate(characterStatsComponent.CurrentShield, characterStatsComponent.MaxHp);
            }

#if FUSION2
            if (Runner && Object && Object.HasStateAuthority)
                PushMiniHudFromStats();
#endif

            if (characterStatsComponent.CurrentHP <= 0f)
                OnDeath();
        }

        /// <summary>
        /// Receives damage, reducing shield first if available and then HP.
        /// Damage is reduced by current defense but not lower than a global minimum.
        /// </summary>
        /// <param name="damage">The damage to receive.</param>
        public override void ReceiveDamage(float damage)
        {
#if FUSION2
            if (Runner && Runner.IsRunning && Object && !Object.HasStateAuthority)
                return;
#endif
            base.ReceiveDamage(damage);
            if (GameplayManager.Singleton.IsPaused()) return;
            if (isInvincible) return;

            characterStatsComponent.ReceiveDamage(damage);
        }

        public void CharacterRevive()
        {
            if (!IsDead) return;
            IsDead = false;
            characterStatsComponent.Revive();
            canAutoAttack = true;
            isMovementStopped = false;
            characterControllerComponent.ResumeMovement();

            characterAttackComponent.ResumeAutoAttack();
            GameplayManager.Singleton?.MarkCharacterRevived(this);
#if FUSION2
            GameplaySync.Instance?.SyncPlayerRevived(this);
#endif
            OnCharacterRevive();
        }

        /// <summary>
        /// called when the character revives.
        /// </summary>
        public override void OnCharacterRevive()
        {
            ApplyInvincible(reviveInvincibleTime);           
        }

        /// <summary>
        /// Final death routine – runs once. Stops movement/inputs, plays animation,
        /// notifies GameplayManager and (optionally) replicates via Fusion.
        /// </summary>
        public override void OnDeath()
        {
            if (IsDead) return;
            IsDead = true;

#if FUSION2
            if (Runner && Object && Object.HasStateAuthority && PvpSync.IsSpawnedReady && PvpSync.Instance)
            {
                NetworkObject killerObj = null;
                if (_lastHitBy.IsValid) Runner.TryFindObject(_lastHitBy, out killerObj);
                var killerCE = killerObj ? killerObj.GetComponent<CharacterEntity>() : null;

                if (killerCE)
                {
                    PvpSync.Instance.RPC_NotifyKill(killerCE.CharacterId, killerCE.PlayerNick,
                                                      this.CharacterId, this.PlayerNick, killerCE.TeamId);

                    if (PvpSync.Instance.Mode == PvpModeType.BattleRoyale)
                        PvpSync.Instance.AnnounceEliminationNow(this);
                }

                PvpSync.Instance.ScheduleRevive(this).Forget();
            }
#endif

            canAutoAttack = false;
            isMovementStopped = true;

            characterControllerComponent.CancelDash();
            characterControllerComponent.StopMovement();

            characterAttackComponent.StopAutoAttack();
            
            PlayDeath();
            directionalAim?.SetActive(false);

            GameplayManager.Singleton?.MarkCharacterDead(this);
#if FUSION2
            GameplaySync.Instance?.SyncPlayerDied(this);
#endif
        }

        #endregion

        #region  -------------------------------  Utility / Getters  -------------------------------

        /// <summary>
        /// Returns the current CharacterData assigned to this entity.
        /// </summary>
        /// <returns>The character data, or null if not set.</returns>
        public CharacterData GetCharacterData()
        {
            return characterData;
        }

        /// <summary>
        /// Returns the CharacterTypeData from the current character, if available.
        /// </summary>
        /// <returns>The CharacterTypeData, or null if CharacterData is not assigned.</returns>
        public CharacterTypeData GetCharacterType()
        {
            if (characterData == null)
            {
                Debug.LogWarning("CharacterData is null. Cannot retrieve CharacterType.");
                return null;
            }

            return characterData.characterType;
        }

        /// <summary>
        /// Retrieves the SkillData for a given skill index.
        /// </summary>
        /// <param name="index">The skill index.</param>
        /// <returns>The SkillData object, or null if invalid.</returns>
        public SkillData GetSkillData(int index)
        {
            if (characterData == null || characterData.skills == null)
            {
                Debug.LogWarning("CharacterData or skills array is null.");
                return null;
            }

            if (index < 0 || index >= characterData.skills.Length)
            {
                Debug.LogWarning($"Invalid skill index: {index}. Array length: {characterData.skills.Length}");
                return null;
            }

            return characterData.skills[index];
        }

        /// <summary>
        /// Returns the index of a specific skill in the character's skill array.
        /// </summary>
        /// <param name="skill">The skill to find.</param>
        /// <returns>The index of the skill, or -1 if not found.</returns>
        public int GetSkillIndex(SkillData skill)
        {
            if (skill == null)
            {
                Debug.LogWarning("Skill is null. Cannot find index.");
                return -1;
            }

            if (characterData == null || characterData.skills == null)
            {
                Debug.LogWarning("CharacterData or skills array is null. Cannot search for skill.");
                return -1;
            }

            for (int index = 0; index < characterData.skills.Length; index++)
            {
                if (characterData.skills[index] == skill)
                    return index;
            }

            Debug.LogWarning($"Skill '{skill.skillName}' not found in the character's skills.");
            return -1;
        }

        /// <summary>
        /// Retrieves the icon sprite for a specific skill.
        /// </summary>
        /// <param name="skillIndex">The index of the skill in the character's skill array.</param>
        /// <returns>The icon sprite of the skill, or null if invalid.</returns>
        public Sprite GetSkillIcon(int skillIndex)
        {
            SkillData skill = GetSkillData(skillIndex);
            return skill != null ? skill.icon : null;
        }

        /// <summary>
        /// Retrieves the cooldown duration for a specific skill.
        /// </summary>
        /// <param name="skillIndex">The index of the skill in the character's skill array.</param>
        /// <returns>The cooldown duration in seconds, or 0 if invalid.</returns>
        public float GetSkillCooldown(int skillIndex)
        {
            SkillData skill = GetSkillData(skillIndex);
            return skill != null ? skill.cooldown : 0f;
        }
        private Vector3 GetAimWorldPosition(Vector2 joystickDirection, float maxRange)
        {
            float joystickMag = Mathf.Clamp01(joystickDirection.magnitude);
            float actualDistance = joystickMag * maxRange;
            return transform.position + transform.forward * actualDistance;
        }

        /// <summary>
        /// Retrieves a list of perks available to the player at level-up.
        /// </summary>
        /// <returns>List of perk options</returns>
        private List<object> GetAvailablePerks() => GameplayManager.Singleton.GetRandomPerks();

        public bool HasStatPerk(StatPerkData perk) => characterStatsComponent.HasStatPerk(perk);

        /// <summary>
        /// Retrieves the list of skill perks associated with the character.
        /// </summary>
        public List<SkillPerkData> GetSkillsPerkData() => characterAttackComponent.GetSkillsPerkData();

        /// <summary>
        /// Retrieves the list of stat perks associated with the character.
        /// </summary>
        public List<StatPerkData> GetStatsPerkData() => characterStatsComponent.GetStatsPerkData();

        /// <summary>
        /// Gets the current level of the character.
        /// </summary>
        public int GetCurrentLevel() => characterStatsComponent.CurrentLevel;

        /// <summary>
        /// Gets the current XP of the character.
        /// </summary>
        public int GetCurrentXP() => characterStatsComponent.CurrentXP;

        /// <summary>
        /// Gets the XP required for the next level.
        /// </summary>
        public int GetXPToNextLevel() => characterStatsComponent.GetXPToNextLevel();

        public float GetCurrentHP() => characterStatsComponent.CurrentHP;
        public float GetMaxHP() => characterStatsComponent.MaxHp;
        public float GetCurrentHPRegen() => characterStatsComponent.CurrentHPRegen;
        public float GetCurrentHPLeech() => characterStatsComponent.CurrentHPLeech;
        public float GetCurrentMP() => characterStatsComponent.CurrentMP;
        public float GetMaxMP() => characterStatsComponent.MaxMp;
        public float GetCurrentMPRegen() => characterStatsComponent.CurrentMPRegen;
        public float GetCurrentDamage() => characterStatsComponent.CurrentDamage;
        public float GetCurrentAttackSpeed() => characterStatsComponent.CurrentAttackSpeed;
        public float GetCurrentCooldownReduction() => characterStatsComponent.CurrentCooldownReduction;
        public float GetCurrentCriticalRate() => characterStatsComponent.CurrentCriticalRate;
        public float GetCurrentCriticalDamageMultiplier() => characterStatsComponent.CurrentCriticalDamageMultiplier;
        public float GetCurrentDefense() => characterStatsComponent.CurrentDefense;
        public float GetCurrentShield() => characterStatsComponent.CurrentShield;
        public float GetCurrentMoveSpeed() => characterStatsComponent.CurrentMoveSpeed;
        public float GetCurrentCollectRange() => characterStatsComponent.CurrentCollectRange;
        public float GetCurrentMaxStats() => characterStatsComponent.CurrentMaxStats;
        public float GetCurrentMaxSkills() => characterStatsComponent.CurrentMaxSkills;

        #endregion

        #region  -------------------------------  Coroutines  -------------------------------

        /// <summary>
        /// Rotates the character model visually toward the given direction for a duration.
        /// Y component is ignored to prevent vertical tilt.
        /// </summary>
        /// <param name="direction">Direction to face.</param>
        /// <param name="duration">Duration of the rotation effect.</param>
        /// <param name="token">Optional cancellation token.</param>
        private async UniTask RotateCharacterModelAsync(Vector3 direction, float duration, CancellationToken token = default)
        {
            if (!this || transform == null) return;
            if (characterModelTransform == null)
            {
                Debug.LogWarning("Character model transform is not set.");
                return;
            }

            Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            if (flatDirection == Vector3.zero)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(flatDirection);

            float timer = 0f;
            try
            {
                while (timer < duration)
                {
                    token.ThrowIfCancellationRequested();

                    Quaternion desiredLocalRotation = Quaternion.Inverse(transform.rotation) * targetRotation;
                    characterModelTransform.localRotation = desiredLocalRotation;

                    timer += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                characterModelTransform.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Disables auto-attack for a duration, then re-enables it.
        /// Uses UniTask for better performance and cancelation.
        /// </summary>
        /// <param name="delay">Delay before allowing auto-attack again.</param>
        /// <param name="token">Optional cancellation token.</param>
        private async UniTask ResetAutoAttackAsync(float delay, CancellationToken token = default)
        {
            canAutoAttack = false;

            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            canAutoAttack = true;
        }

        /// <summary>
        /// Removes invincibility after a set duration.
        /// Uses UniTask for better performance and optional cancellation.
        /// </summary>
        /// <param name="duration">The time to remain invincible.</param>
        /// <param name="token">Optional cancellation token.</param>
        private async UniTask RemoveInvincibilityAfterAsync(float duration, CancellationToken token = default)
        {
            if (duration > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            isInvincible = false;
        }

        /// <summary>
        /// Stops the player's movement for a specified duration using UniTask.
        /// </summary>
        /// <param name="duration">Duration in seconds to stop movement.</param>
        /// <param name="allowRotation">If true, allows rotation while movement is stopped.</param>
        /// <param name="token">Optional cancellation token.</param>
        private async UniTask StopMovementAsync(float duration, bool allowRotation, CancellationToken token = default)
        {
            if (characterControllerComponent == null)
            {
                Debug.LogWarning("CharacterController is not assigned.");
                return;
            }

            characterControllerComponent.StopMovement(allowRotation);
            isMovementStopped = true;

            if (duration > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            characterControllerComponent.ResumeMovement();
            isMovementStopped = false;
        }

        /// <summary>
        /// Internal coroutine that enforces the stun and restores the previous state.
        /// </summary>
        private async UniTaskVoid StunRoutine(float duration, CancellationToken token)
        {
            if (isStunned) return;    

            bool prevAutoAttack = canAutoAttack;
            bool prevMovementStop = isMovementStopped;
            characterControllerComponent.CancelDash();
            characterControllerComponent.StopMovement();   
            isMovementStopped = true;
            canAutoAttack = false;
            isStunned = true;

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            }
            catch (OperationCanceledException) {  }
            finally
            {
                isStunned = false;
                canAutoAttack = prevAutoAttack;

                if (!prevMovementStop)
                {
                    characterControllerComponent.ResumeMovement();
                    isMovementStopped = false;
                }
            }
        }

        /// <summary>
        /// Hides the status message after a delay using UniTask.
        /// </summary>
        /// <param name="statusText">The TextMeshProUGUI component displaying the status message.</param>
        /// <param name="delay">The delay in seconds before hiding the message.</param>
        /// <param name="token">Optional cancellation token.</param>
        private async UniTask HideStatusMessageAfterDelayAsync(TextMeshProUGUI statusText, float delay, CancellationToken token = default)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            if (statusText != null)
                statusText.text = string.Empty;
        }

        #endregion
    }
}
