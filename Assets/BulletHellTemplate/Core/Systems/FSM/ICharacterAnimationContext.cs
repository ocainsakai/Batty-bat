// Abstraction layer – decouples FSM from any concrete data source (e.g., CharacterModel, ScriptableObject, ECS buffer…)
using UnityEngine;

namespace BulletHellTemplate.Core.FSM
{
    /// <summary>
    /// Provides read-only access to all clips and Animator needed by the FSM.
    /// Implement this interface in a separate adapter component that knows where
    /// the data really comes from (ScriptableObject, network, etc.).
    /// </summary>
    public interface ICharacterAnimationContext
    {
        Animator Animator { get; }
        DirectionalAnimSet IdleSet { get; }
        DirectionalAnimSet MoveSet { get; }

        SkillAnimData Attack { get; }
        AnimClipData ReceiveDamage { get; }
        AnimClipData Death { get; }

        bool TryGetSkill(int index, out SkillAnimData data);
    }
}
