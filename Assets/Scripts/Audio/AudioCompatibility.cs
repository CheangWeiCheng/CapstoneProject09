using System;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Compatibility names for teammate branches that still call the interim API.
    /// New code should use <see cref="AudioCue"/> and <see cref="AudioDirector"/>.
    /// </summary>
    [Obsolete("Use AudioCue instead.")]
    public enum InterimAudioCue
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
        Bgm
    }

    [Obsolete("Use AudioDirector instead.")]
    public static class InterimAudioDirector
    {
        public static AudioDirector Instance => AudioDirector.Instance;

        public static void ReportPlayerMovement(
            Vector3 position,
            bool isMoving,
            bool isCrouching,
            bool isRunning,
            float speedRatio = 1f
        )
        {
            AudioDirector.ReportPlayerMovement(position, isMoving, isCrouching, isRunning, speedRatio);
        }

        public static bool TryPlayPlayerJump(Vector3 position, bool isAirJump)
        {
            return AudioDirector.TryPlayPlayerJump(position, isAirJump);
        }

        public static bool TryPlayPlayerDash(Vector3 position)
        {
            return AudioDirector.TryPlayPlayerDash(position);
        }

        public static bool TryPlayPlayerCrouch(Vector3 position)
        {
            return AudioDirector.TryPlayPlayerCrouch(position);
        }

        public static bool TryPlayPlayerLand(Vector3 position, bool heavy = false)
        {
            return AudioDirector.TryPlayPlayerLand(position, heavy);
        }

        public static bool TryPlayMove(InterimAudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return AudioDirector.TryPlayMove((AudioCue)cue, position, volumeScale);
        }

        public static bool TryPlayInteraction(InterimAudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return AudioDirector.TryPlayInteraction((AudioCue)cue, position, volumeScale);
        }

        public static bool TryPlayGoldPickup(Vector3 position, float volumeScale = 1f)
        {
            return AudioDirector.TryPlayGoldPickup(position, volumeScale);
        }

        public static bool TryPlayGoldPurchase(Vector3 position, float volumeScale = 1f)
        {
            return AudioDirector.TryPlayGoldPurchase(position, volumeScale);
        }

        public static bool TryPlayUiHover(float volumeScale = 1f)
        {
            return AudioDirector.TryPlayUiHover(volumeScale);
        }

        public static bool TryPlayUiClick(float volumeScale = 1f)
        {
            return AudioDirector.TryPlayUiClick(volumeScale);
        }

        public static bool TryPlayWorld(InterimAudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            return AudioDirector.TryPlayWorld((AudioCue)cue, position, volumeScale);
        }

        public static void SetCombatState(bool inCombat)
        {
            AudioDirector.SetCombatState(inCombat);
        }

        public static void ReportCombatState(UnityEngine.Object reporter, bool inCombat)
        {
            AudioDirector.ReportCombatState(reporter, inCombat);
        }
    }
}
