/*
 * AudioDirector: persistent routing point for sound effects and adaptive music.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Game.Audio
{
    public enum AudioCue
    {
        None,
        Walk,
        CrouchWalk,
        Run,
        Slide,
        Jump,
        DoubleJump,
        Dash,
        PlayerCrouch,
        Land,
        HeavyLand,
        BasicAttack,
        BasicAttackHit,
        Charge,
        ChargedAttack,
        ChargedAttackHit,
        ChargedCrouchAttack,
        LauncherJump,
        LauncherHit,
        GroundSlamJump,
        GroundSlamHit,
        SpikeSecondJump,
        JumpBack,
        AerialPush,
        GoldPickup,
        GoldPurchase,
        Interact,
        InteractDenied,
        UiHover,
        UiClick,
        Bgm,
        ArcherShot,
        Collection,
        GenericPickup,
        ArcherHit
    }

    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        [Header("Master Toggle")]
        [SerializeField] private bool audioEnabled = false;
        [SerializeField] private bool keepAliveAcrossScenes = true;
        [SerializeField] private bool autoPlayBgm = false;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float movementVolume = 0.55f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.9f;
        [SerializeField, Range(0f, 1f)] private float uiVolume = 0.75f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.55f;
        [SerializeField, Range(0f, 1f)] private float sfxSettingsVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmSettingsVolume = 0.6f;
        [SerializeField] private bool playWorldSoundsAtPosition = true;
        [SerializeField] private float duplicateCueWindow = 0.03f;
        [SerializeField, Min(0f)] private float startupSfxDelaySeconds = 0.5f;

        [Header("Sources")]
        [SerializeField] private AudioSource movementSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;
        [FormerlySerializedAs("bgmSource")]
        [SerializeField] private AudioSource explorationBgmSource;
        [SerializeField] private AudioSource actionBgmSource;
        [SerializeField] private AudioSource deathSource;

        [Header("Movement")]
        [SerializeField] private AudioClip walkLoop;
        [SerializeField] private AudioClip[] walkVariations;
        [SerializeField] private AudioClip crouchWalkLoop;
        [SerializeField] private AudioClip runLoop;
        [SerializeField] private AudioClip[] runVariations;
        [SerializeField] private AudioClip slideLoop;
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip doubleJumpClip;
        [SerializeField] private AudioClip dashClip;
        [SerializeField] private AudioClip playerCrouchClip;
        [SerializeField] private AudioClip landClip;
        [SerializeField] private AudioClip heavyLandClip;

        [Header("Moveset")]
        [SerializeField] private AudioClip basicAttackClip;
        [SerializeField] private AudioClip basicAttackHitClip;
        [SerializeField] private AudioClip chargeClip;
        [SerializeField] private AudioClip chargedAttackClip;
        [SerializeField] private AudioClip chargedAttackHitClip;
        [SerializeField] private AudioClip chargedCrouchAttackClip;
        [SerializeField] private AudioClip launcherJumpClip;
        [SerializeField] private AudioClip launcherHitClip;
        [SerializeField] private AudioClip groundSlamJumpClip;
        [SerializeField] private AudioClip groundSlamHitClip;
        [SerializeField] private AudioClip spikeSecondJumpClip;
        [SerializeField] private AudioClip jumpBackClip;
        [SerializeField] private AudioClip aerialPushClip;

        [Header("Enemy Combat")]
        [SerializeField] private AudioClip archerShotClip;
        [SerializeField] private AudioClip archerHitClip;
        [SerializeField] private Vector2 enemyAttackPitchVariationRange = new Vector2(0.96f, 1.04f);
        [SerializeField] private Vector2 enemyHitPitchVariationRange = new Vector2(0.94f, 1.06f);

        [Header("Interactions")]
        [SerializeField] private AudioClip goldPickupClip;
        [SerializeField] private AudioClip goldPurchaseClip;
        [SerializeField] private AudioClip collectionClip;
        [SerializeField] private AudioClip genericPickupClip;
        [SerializeField] private AudioClip interactClip;
        [SerializeField] private AudioClip interactDeniedClip;

        [Header("Death")]
        [SerializeField] private AudioClip deathClip;

        [Header("UI")]
        [SerializeField] private AudioClip uiHoverClip;
        [SerializeField] private AudioClip uiClickClip;

        [Header("Adaptive BGM")]
        [FormerlySerializedAs("bgmClip")]
        [SerializeField] private AudioClip explorationBgmClip;
        [SerializeField] private AudioClip actionBgmClip;
        [SerializeField, Min(0f)] private float bgmCrossfadeSeconds = 1f;
        [SerializeField, Min(0f)] private float bgmScheduleLeadSeconds = 0.1f;

        private readonly Dictionary<AudioCue, float> lastCueTimes = new Dictionary<AudioCue, float>();
        private readonly HashSet<int> activeCombatReporters = new HashSet<int>();
        private readonly List<ActiveWorldSound> activeWorldSounds = new List<ActiveWorldSound>();
        private float lastMovementReportTime = -99f;
        private AudioCue currentMovementCue = AudioCue.None;
        private int lastWalkVariationIndex = -1;
        private int lastRunVariationIndex = -1;
        private float currentActionMix;
        private float targetActionMix;
        private bool qaMixOverrideActive;
        private bool bgmPlaying;
        private bool deathTransitionActive;
        private float nonDeathVolumeScale = 1f;
        private float targetNonDeathVolumeScale = 1f;
        private float nonDeathFadeSeconds;
        private float sfxReadyTime;
        private const float MovementReportTimeout = 0.18f;

        private sealed class ActiveWorldSound
        {
            public AudioSource Source;
            public float VolumeScale;
        }

        public bool AudioEnabled => audioEnabled && isActiveAndEnabled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (keepAliveAcrossScenes) DontDestroyOnLoad(gameObject);

            sfxReadyTime = Time.unscaledTime + startupSfxDelaySeconds;
            EnsureSources();
            ApplyVolumes();

            if (audioEnabled && autoPlayBgm) PlayBgm();
        }

        private void LateUpdate()
        {
            if (movementSource != null && movementSource.isPlaying &&
                (!AudioEnabled || Time.unscaledTime - lastMovementReportTime > MovementReportTimeout))
            {
                movementSource.Stop();
            }

            UpdateBgmCrossfade();
            UpdateDeathFade();
            CleanUpWorldSounds();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetAudioEnabled(bool enabled)
        {
            audioEnabled = enabled;

            if (!audioEnabled)
            {
                StopMovementLoop();
                StopBgm();
                return;
            }

            if (autoPlayBgm) PlayBgm();
        }

        // if you feed this file to an ai for a settings menu, ask it to wire the two public volume functions below to zero-to-one sliders.
        public void SetSfxVolume(float volume)
        {
            sfxSettingsVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
        }

        public void SetBgmVolume(float volume)
        {
            bgmSettingsVolume = Mathf.Clamp01(volume);
            ApplyBgmVolumes();
        }

        public static void ReportPlayerMovement(
            Vector3 position,
            bool isMoving,
            bool isCrouching,
            bool isRunning,
            float speedRatio = 1f
        )
        {
            if (Instance == null) return;

            Instance.PlayMovementLoop(position, isMoving, isCrouching, isRunning, speedRatio);
        }

        public static bool TryPlayPlayerJump(Vector3 position, bool isAirJump)
        {
            return TryPlayWorld(isAirJump ? AudioCue.DoubleJump : AudioCue.Jump, position);
        }

        public static bool TryPlayPlayerDash(Vector3 position)
        {
            return TryPlayWorld(AudioCue.Dash, position);
        }

        public static bool TryPlayPlayerCrouch(Vector3 position)
        {
            return TryPlayWorld(AudioCue.PlayerCrouch, position);
        }

        public static bool TryPlayPlayerLand(Vector3 position, bool heavy = false)
        {
            return TryPlayWorld(heavy ? AudioCue.HeavyLand : AudioCue.Land, position);
        }

        public static bool TryPlayMove(AudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return TryPlayWorld(cue, position, volumeScale);
        }

        public static bool TryPlayInteraction(AudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return TryPlayWorld(cue, position, volumeScale);
        }

        public static bool TryPlayGoldPickup(Vector3 position, float volumeScale = 1f)
        {
            return TryPlayInteraction(AudioCue.GoldPickup, position, volumeScale);
        }

        public static bool TryPlayGoldPurchase(Vector3 position, float volumeScale = 1f)
        {
            return TryPlayInteraction(AudioCue.GoldPurchase, position, volumeScale);
        }

        public static bool TryPlayUiHover(float volumeScale = 1f)
        {
            return Instance != null && Instance.PlayOneShot(AudioCue.UiHover, Vector3.zero, volumeScale, false, true);
        }

        public static bool TryPlayUiClick(float volumeScale = 1f)
        {
            return Instance != null && Instance.PlayOneShot(AudioCue.UiClick, Vector3.zero, volumeScale, false, true);
        }

        public static bool TryPlayWorld(AudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return Instance != null && Instance.PlayOneShot(cue, position, volumeScale, true, false);
        }

        /// <summary>
        /// Starts the V4 Exploration and Action loops on one DSP timeline.
        /// </summary>
        public bool PlayBgm(AudioClip overrideClip = null, bool loop = true)
        {
            if (!AudioEnabled) return false;

            EnsureSources();

            AudioClip explorationClip = overrideClip != null ? overrideClip : explorationBgmClip;
            if (explorationClip == null) return false;

            explorationBgmSource.Stop();
            actionBgmSource.Stop();
            explorationBgmSource.clip = explorationClip;
            explorationBgmSource.loop = loop;
            explorationBgmSource.timeSamples = 0;

            if (overrideClip == null && actionBgmClip != null)
            {
                actionBgmSource.clip = actionBgmClip;
                actionBgmSource.loop = loop;
                actionBgmSource.timeSamples = 0;

                double startTime = AudioSettings.dspTime + bgmScheduleLeadSeconds;
                explorationBgmSource.PlayScheduled(startTime);
                actionBgmSource.PlayScheduled(startTime);
            }
            else
            {
                explorationBgmSource.Play();
            }

            bgmPlaying = true;
            ApplyVolumes();
            return true;
        }

        public static bool TryPlayCollection(Vector3 position, float volumeScale = 1f)
        {
            return TryPlayInteraction(AudioCue.Collection, position, volumeScale);
        }

        public static bool TryPlayGenericPickup(Vector3 position, float volumeScale = 1f)
        {
            return TryPlayInteraction(AudioCue.GenericPickup, position, volumeScale);
        }

        public static bool TryPlayArcherShot(Vector3 position, float volumeScale = 1f)
        {
            return TryPlayEnemyAttack(AudioCue.ArcherShot, position, volumeScale);
        }

        public static bool TryPlayArcherHit(Vector3 position, float volumeScale = 1f)
        {
            return TryPlayEnemyHit(AudioCue.ArcherHit, position, volumeScale);
        }

        public static bool TryPlayEnemyAttack(AudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return Instance != null && Instance.PlayOneShot(
                cue,
                position,
                volumeScale,
                true,
                false,
                GetRandomPitch(Instance.enemyAttackPitchVariationRange)
            );
        }

        public static bool TryPlayEnemyHit(AudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return Instance != null && Instance.PlayOneShot(
                cue,
                position,
                volumeScale,
                true,
                false,
                GetRandomPitch(Instance.enemyHitPitchVariationRange)
            );
        }

        public static bool BeginDeathSequence(float fadeOutSeconds = 1f)
        {
            return Instance != null && Instance.BeginDeathFade(fadeOutSeconds);
        }

        public static void EndDeathSequence(float fadeInSeconds = 1f)
        {
            if (Instance != null) Instance.EndDeathFade(fadeInSeconds);
        }

        public void StopBgm()
        {
            if (explorationBgmSource != null) explorationBgmSource.Stop();
            if (actionBgmSource != null) actionBgmSource.Stop();
            bgmPlaying = false;
        }

        /// <summary>
        /// Direct hook for cutscenes, arenas, bosses, and other single-owner systems.
        /// </summary>
        public static void SetCombatState(bool inCombat)
        {
            if (Instance != null) Instance.SetActionMixTarget(inCombat);
        }

        [ContextMenu("Audio QA/Enter Combat")]
        private void QaEnterCombat()
        {
            qaMixOverrideActive = true;
            SetActionMixTarget(true);
        }

        [ContextMenu("Audio QA/Return to Exploration")]
        private void QaReturnToExploration()
        {
            qaMixOverrideActive = true;
            SetActionMixTarget(false);
        }

        [ContextMenu("Audio QA/Resume Automatic")]
        private void QaResumeAutomatic()
        {
            qaMixOverrideActive = false;
            SetActionMixTarget(activeCombatReporters.Count > 0);
        }

        /// <summary>
        /// Multi-owner combat hook. Each caller should report true on engagement and
        /// false on disengagement/disable; music returns to Exploration only after
        /// the final active reporter leaves combat.
        /// </summary>
        public static void ReportCombatState(Object reporter, bool inCombat)
        {
            if (Instance == null || reporter == null) return;

            int reporterId = reporter.GetInstanceID();

            if (inCombat)
            {
                Instance.activeCombatReporters.Add(reporterId);
            }
            else
            {
                Instance.activeCombatReporters.Remove(reporterId);
            }

            if (!Instance.qaMixOverrideActive)
            {
                Instance.SetActionMixTarget(Instance.activeCombatReporters.Count > 0);
            }
        }

        private void SetActionMixTarget(bool inCombat)
        {
            targetActionMix = inCombat ? 1f : 0f;

            if (AudioEnabled && autoPlayBgm && !bgmPlaying)
            {
                PlayBgm();
            }
        }

        private void UpdateBgmCrossfade()
        {
            if (!bgmPlaying) return;

            currentActionMix = bgmCrossfadeSeconds <= 0f
                ? targetActionMix
                : Mathf.MoveTowards(
                    currentActionMix,
                    targetActionMix,
                    Time.unscaledDeltaTime / bgmCrossfadeSeconds
                );

            ApplyBgmVolumes();
        }

        private bool PlayOneShot(
            AudioCue cue,
            Vector3 position,
            float volumeScale,
            bool worldSound,
            bool uiSound,
            float pitch = 1f
        )
        {
            if (!AudioEnabled || cue == AudioCue.None) return false;
            if (deathTransitionActive) return true;
            if (Time.unscaledTime < sfxReadyTime) return true;

            AudioClip clip = GetClip(cue);
            if (clip == null) return false;

            if (ShouldSuppressDuplicate(cue)) return true;

            EnsureSources();

            float safeVolumeScale = Mathf.Max(0f, volumeScale);
            float safePitch = Mathf.Clamp(pitch, 0.5f, 2f);

            if (uiSound)
            {
                uiSource.pitch = safePitch;
                uiSource.PlayOneShot(clip, safeVolumeScale);
                return true;
            }

            if (worldSound && playWorldSoundsAtPosition)
            {
                PlayWorldOneShot(clip, position, safeVolumeScale, safePitch);
                return true;
            }

            sfxSource.pitch = safePitch;
            sfxSource.PlayOneShot(clip, safeVolumeScale);
            return true;
        }

        private static float GetRandomPitch(Vector2 range)
        {
            float minimum = Mathf.Clamp(Mathf.Min(range.x, range.y), 0.5f, 2f);
            float maximum = Mathf.Clamp(Mathf.Max(range.x, range.y), minimum, 2f);
            return Random.Range(minimum, maximum);
        }

        private bool BeginDeathFade(float fadeOutSeconds)
        {
            if (!AudioEnabled) return false;

            EnsureSources();
            deathTransitionActive = true;
            targetNonDeathVolumeScale = 0f;
            nonDeathFadeSeconds = Mathf.Max(0f, fadeOutSeconds);

            if (deathClip != null)
            {
                deathSource.Stop();
                deathSource.clip = deathClip;
                deathSource.volume = Mathf.Clamp01(masterVolume * sfxVolume);
                deathSource.Play();
            }

            return deathClip != null;
        }

        private void EndDeathFade(float fadeInSeconds)
        {
            deathTransitionActive = false;
            deathSource.Stop();
            targetNonDeathVolumeScale = 1f;
            nonDeathFadeSeconds = Mathf.Max(0f, fadeInSeconds);
        }

        private void UpdateDeathFade()
        {
            if (Mathf.Approximately(nonDeathVolumeScale, targetNonDeathVolumeScale)) return;

            nonDeathVolumeScale = nonDeathFadeSeconds <= 0f
                ? targetNonDeathVolumeScale
                : Mathf.MoveTowards(
                    nonDeathVolumeScale,
                    targetNonDeathVolumeScale,
                    Time.unscaledDeltaTime / nonDeathFadeSeconds
                );

            ApplyVolumes();

            if (nonDeathVolumeScale > 0f || targetNonDeathVolumeScale > 0f) return;

            StopMovementLoop();
            sfxSource.Stop();
            uiSource.Stop();

            foreach (ActiveWorldSound worldSound in activeWorldSounds)
            {
                if (worldSound.Source != null) worldSound.Source.Stop();
            }
        }

        private void PlayWorldOneShot(AudioClip clip, Vector3 position, float volumeScale, float pitch)
        {
            GameObject sourceObject = new GameObject($"World SFX - {clip.name}");
            sourceObject.transform.SetParent(transform, true);
            sourceObject.transform.position = position;

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.clip = clip;
            source.pitch = pitch;

            ActiveWorldSound worldSound = new ActiveWorldSound
            {
                Source = source,
                VolumeScale = volumeScale
            };

            activeWorldSounds.Add(worldSound);
            ApplyWorldSoundVolume(worldSound);
            source.Play();
        }

        private void CleanUpWorldSounds()
        {
            for (int i = activeWorldSounds.Count - 1; i >= 0; i--)
            {
                AudioSource source = activeWorldSounds[i].Source;
                if (source != null && source.isPlaying) continue;

                if (source != null) Destroy(source.gameObject);
                activeWorldSounds.RemoveAt(i);
            }
        }

        private void ApplyWorldSoundVolume(ActiveWorldSound worldSound)
        {
            if (worldSound.Source == null) return;

            worldSound.Source.volume = Mathf.Clamp01(
                masterVolume * sfxVolume * sfxSettingsVolume * nonDeathVolumeScale * worldSound.VolumeScale
            );
        }

        private void PlayMovementLoop(
            Vector3 position,
            bool isMoving,
            bool isCrouching,
            bool isRunning,
            float speedRatio
        )
        {
            if (!AudioEnabled || !isMoving || Time.unscaledTime < sfxReadyTime)
            {
                StopMovementLoop();
                return;
            }

            EnsureSources();

            AudioCue cue = isCrouching ? AudioCue.CrouchWalk : isRunning ? AudioCue.Run : AudioCue.Walk;
            lastMovementReportTime = Time.unscaledTime;
            movementSource.transform.position = position;
            movementSource.loop = false;
            movementSource.pitch = Mathf.Clamp(speedRatio, 0.75f, 1.35f);

            if (currentMovementCue == cue && movementSource.isPlaying) return;

            AudioClip clip = GetMovementClip(cue);

            if (clip == null)
            {
                StopMovementLoop();
                return;
            }

            currentMovementCue = cue;
            movementSource.clip = clip;
            movementSource.Play();
        }

        private void StopMovementLoop()
        {
            if (movementSource != null && movementSource.isPlaying) movementSource.Stop();
            currentMovementCue = AudioCue.None;
        }

        private AudioClip GetMovementClip(AudioCue cue)
        {
            switch (cue)
            {
                case AudioCue.Walk:
                    return PickVariation(walkVariations, walkLoop, ref lastWalkVariationIndex);
                case AudioCue.Run:
                    return PickVariation(runVariations, runLoop, ref lastRunVariationIndex);
                default:
                    return GetClip(cue);
            }
        }

        private static AudioClip PickVariation(AudioClip[] variations, AudioClip fallback, ref int lastIndex)
        {
            if (variations == null || variations.Length == 0) return fallback;

            int startIndex = Random.Range(0, variations.Length);

            for (int offset = 0; offset < variations.Length; offset++)
            {
                int index = (startIndex + offset) % variations.Length;
                AudioClip clip = variations[index];

                if (clip != null && (index != lastIndex || variations.Length == 1))
                {
                    lastIndex = index;
                    return clip;
                }
            }

            if (lastIndex >= 0 && lastIndex < variations.Length && variations[lastIndex] != null)
            {
                return variations[lastIndex];
            }

            return fallback;
        }

        private bool ShouldSuppressDuplicate(AudioCue cue)
        {
            if (duplicateCueWindow <= 0f) return false;

            float now = Time.unscaledTime;

            if (lastCueTimes.TryGetValue(cue, out float lastTime) && now - lastTime < duplicateCueWindow)
            {
                return true;
            }

            lastCueTimes[cue] = now;
            return false;
        }

        private AudioClip GetClip(AudioCue cue)
        {
            switch (cue)
            {
                case AudioCue.Walk:
                    return walkLoop;
                case AudioCue.CrouchWalk:
                    return crouchWalkLoop;
                case AudioCue.Run:
                    return runLoop;
                case AudioCue.Slide:
                    return slideLoop;
                case AudioCue.Jump:
                    return jumpClip;
                case AudioCue.DoubleJump:
                    return doubleJumpClip != null ? doubleJumpClip : jumpClip;
                case AudioCue.Dash:
                    return dashClip;
                case AudioCue.PlayerCrouch:
                    return playerCrouchClip;
                case AudioCue.Land:
                    return landClip != null ? landClip : heavyLandClip;
                case AudioCue.HeavyLand:
                    return heavyLandClip != null ? heavyLandClip : landClip;
                case AudioCue.BasicAttack:
                    return basicAttackClip;
                case AudioCue.BasicAttackHit:
                    return basicAttackHitClip;
                case AudioCue.Charge:
                    return chargeClip;
                case AudioCue.ChargedAttack:
                    return chargedAttackClip != null ? chargedAttackClip : basicAttackClip;
                case AudioCue.ChargedAttackHit:
                    return chargedAttackHitClip != null ? chargedAttackHitClip : basicAttackHitClip;
                case AudioCue.ChargedCrouchAttack:
                    return chargedCrouchAttackClip != null ? chargedCrouchAttackClip : chargedAttackClip;
                case AudioCue.LauncherJump:
                    return launcherJumpClip;
                case AudioCue.LauncherHit:
                    return launcherHitClip != null ? launcherHitClip : basicAttackHitClip;
                case AudioCue.GroundSlamJump:
                    return groundSlamJumpClip != null ? groundSlamJumpClip : launcherJumpClip;
                case AudioCue.GroundSlamHit:
                    return groundSlamHitClip != null ? groundSlamHitClip : launcherHitClip;
                case AudioCue.SpikeSecondJump:
                    return spikeSecondJumpClip != null ? spikeSecondJumpClip : doubleJumpClip;
                case AudioCue.JumpBack:
                    return jumpBackClip;
                case AudioCue.AerialPush:
                    return aerialPushClip;
                case AudioCue.ArcherShot:
                    return archerShotClip;
                case AudioCue.ArcherHit:
                    return archerHitClip;
                case AudioCue.GoldPickup:
                    return goldPickupClip;
                case AudioCue.GoldPurchase:
                    return goldPurchaseClip != null ? goldPurchaseClip : goldPickupClip;
                case AudioCue.Collection:
                    return collectionClip;
                case AudioCue.GenericPickup:
                    return genericPickupClip;
                case AudioCue.Interact:
                    return interactClip;
                case AudioCue.InteractDenied:
                    return interactDeniedClip;
                case AudioCue.UiHover:
                    return uiHoverClip;
                case AudioCue.UiClick:
                    return uiClickClip;
                case AudioCue.Bgm:
                    return explorationBgmClip;
                default:
                    return null;
            }
        }

        private void EnsureSources()
        {
            movementSource = GetOrCreateSource(movementSource, "Movement Source", false, 0.7f);
            sfxSource = GetOrCreateSource(sfxSource, "SFX Source", false, 0f);
            uiSource = GetOrCreateSource(uiSource, "UI Source", false, 0f);
            explorationBgmSource = GetOrCreateSource(
                explorationBgmSource,
                "Exploration BGM Source",
                true,
                0f
            );
            actionBgmSource = GetOrCreateSource(actionBgmSource, "Action BGM Source", true, 0f);
            deathSource = GetOrCreateSource(deathSource, "Death Source", false, 0f);

            ApplyVolumes();
        }

        private AudioSource GetOrCreateSource(AudioSource source, string childName, bool loop, float spatialBlend)
        {
            if (source == null)
            {
                Transform child = transform.Find(childName);

                if (child == null)
                {
                    GameObject sourceObject = new GameObject(childName);
                    sourceObject.transform.SetParent(transform, false);
                    child = sourceObject.transform;
                }

                source = child.GetComponent<AudioSource>();
                if (source == null) source = child.gameObject.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            return source;
        }

        private void ApplyVolumes()
        {
            if (movementSource != null) movementSource.volume = Mathf.Clamp01(masterVolume * movementVolume * sfxSettingsVolume * nonDeathVolumeScale);
            if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(masterVolume * sfxVolume * sfxSettingsVolume * nonDeathVolumeScale);
            if (uiSource != null) uiSource.volume = Mathf.Clamp01(masterVolume * uiVolume * sfxSettingsVolume * nonDeathVolumeScale);
            if (deathSource != null) deathSource.volume = Mathf.Clamp01(masterVolume * sfxVolume * sfxSettingsVolume);

            foreach (ActiveWorldSound worldSound in activeWorldSounds)
            {
                ApplyWorldSoundVolume(worldSound);
            }
            ApplyBgmVolumes();
        }

        private void ApplyBgmVolumes()
        {
            float musicVolume = Mathf.Clamp01(masterVolume * bgmVolume * bgmSettingsVolume * nonDeathVolumeScale);

            if (explorationBgmSource != null)
            {
                explorationBgmSource.volume = musicVolume * (1f - currentActionMix);
            }

            if (actionBgmSource != null)
            {
                actionBgmSource.volume = musicVolume * currentActionMix;
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            audioEnabled = false;
            GuessAudioClips();
        }

        private void OnValidate()
        {
            GuessAudioClips();
            ApplyVolumes();
        }

        private void GuessAudioClips()
        {
            AssignIfEmpty(ref walkLoop, "Stone Walk 1");
            AssignIfEmpty(ref walkVariations, "Stone Walk 1", "Stone Walk 5");
            AssignIfEmpty(ref crouchWalkLoop, "Player Crouch");
            AssignIfEmpty(ref runLoop, "Stone Run 1");
            AssignIfEmpty(ref runVariations, "Stone Run 1", "Stone Run 2", "Stone Run 3");
            AssignIfEmpty(ref slideLoop, "Slide");
            AssignIfEmpty(ref jumpClip, "Stone Jump");
            AssignIfEmpty(ref doubleJumpClip, "Spike (Launcher, Jump, Late 2nd Jump) - 2nd Jump");
            AssignIfEmpty(ref dashClip, "Dash");
            AssignIfEmpty(ref playerCrouchClip, "Player Crouch");
            AssignIfEmpty(ref landClip, "Stone Land");
            AssignIfEmpty(ref heavyLandClip, "Heavy Stone Land");
            AssignIfEmpty(ref basicAttackClip, "Basic Attack");
            AssignIfEmpty(ref basicAttackHitClip, "Hit of Basic Attack");
            AssignIfEmpty(ref chargeClip, "Charge");
            AssignIfEmpty(ref chargedAttackClip, "Charged Crouch Attack");
            AssignIfEmpty(ref chargedAttackHitClip, "Charged Attack Hit");
            AssignIfEmpty(ref chargedCrouchAttackClip, "Charged Crouch Attack");
            AssignIfEmpty(ref launcherJumpClip, "Ground Slam (Launcher, Jump, Early 2nd Jump) - Jump");
            AssignIfEmpty(ref launcherHitClip, "Ground Slam (Launcher, Jump, Early 2nd Jump) - Hit");
            AssignIfEmpty(ref groundSlamJumpClip, "Ground Slam (Launcher, Jump, Early 2nd Jump) - Jump");
            AssignIfEmpty(ref groundSlamHitClip, "Ground Slam (Launcher, Jump, Early 2nd Jump) - Hit");
            AssignIfEmpty(ref spikeSecondJumpClip, "Spike (Launcher, Jump, Late 2nd Jump) - 2nd Jump");
            AssignIfEmpty(ref jumpBackClip, "Jump Back");
            AssignIfEmpty(ref aerialPushClip, "Dash and Jump - Jump and Dash");
            AssignIfEmpty(ref archerShotClip, "Weapon Arrow Shot 01");
            AssignIfEmpty(ref archerHitClip, "Weapon Bow And Arrow Thump 01");
            AssignIfEmpty(ref goldPickupClip, "Coin_Wood_Table_Singles_Drop_Spin_Takes_5");
            AssignIfEmpty(ref goldPurchaseClip, "264604 - Stack Coin Bag 04");
            AssignIfEmpty(ref collectionClip, "collect_6");
            AssignIfEmpty(ref genericPickupClip, "foley_object_grab_pickup_04");
            AssignIfEmpty(ref interactClip, "Piano_Ui (1)");
            AssignIfEmpty(ref interactDeniedClip, "Piano_Ui (7)");
            AssignIfEmpty(ref uiHoverClip, "Piano_Ui (1)");
            AssignIfEmpty(ref uiClickClip, "Piano_Ui (7)");
            AssignIfEmpty(ref deathClip, "Dark Souls ' You Died ' Sound Effect [j_nV2jcTFvA]");
            AssignIfEmpty(ref explorationBgmClip, "armamation_clockwork_keep_v4_exploration_loop", "Assets/Audio/Scores");
            AssignIfEmpty(ref actionBgmClip, "armamation_clockwork_keep_v4_action_loop", "Assets/Audio/Scores");
        }

        private static void AssignIfEmpty(ref AudioClip clip, string exactFileName)
        {
            if (clip != null) return;

            clip = FindAudioClip(exactFileName, "Assets/Audio/Sound Effects");
        }

        private static void AssignIfEmpty(ref AudioClip clip, string exactFileName, string searchFolder)
        {
            if (clip != null) return;

            clip = FindAudioClip(exactFileName, searchFolder);
        }

        private static void AssignIfEmpty(ref AudioClip[] clips, params string[] exactFileNames)
        {
            if (clips != null && clips.Length > 0) return;

            clips = new AudioClip[exactFileNames.Length];

            for (int i = 0; i < exactFileNames.Length; i++)
            {
                clips[i] = FindAudioClip(exactFileNames[i], "Assets/Audio/Sound Effects");
            }
        }

        private static AudioClip FindAudioClip(string exactFileName, string searchFolder)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { searchFolder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetFileNameWithoutExtension(path) == exactFileName)
                {
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
            }

            return null;
        }
#endif
    }
}
