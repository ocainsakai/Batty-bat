// Triggers that drive transitions between states.
namespace BulletHellTemplate.Core.FSM
{
    public enum AnimationEvent
    {
        OnMove,
        OnStopMove,
        OnAttack,
        OnSkill,
        OnSkillEnded,
        OnReceiveDamage,
        OnDie
    }
}
