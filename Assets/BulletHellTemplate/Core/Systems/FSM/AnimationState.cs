// Defines root-level and nested states for the animation HFSM.
namespace BulletHellTemplate.Core.FSM
{
    /// <summary>High-level character animation states.</summary>
    public enum AnimationState
    {
        Locomotion,
        Attack,
        Skill,
        ReceiveDamage,
        Dead
    }

    /// <summary>Nested substates for <see cref="AnimationState.Locomotion"/>.</summary>
    public enum LocomotionSubState
    {
        Idle,
        Move
    }
}
