using UnityEngine;

/// <summary>
/// Player-related events
/// </summary>

public class PlayerDiedEvent : IEvent
{
    public Vector3 DeathPosition { get; set; }
    public string CauseOfDeath { get; set; }

    public PlayerDiedEvent(Vector3 deathPosition, string causeOfDeath = "Collision")
    {
        DeathPosition = deathPosition;
        CauseOfDeath = causeOfDeath;
    }
}

public class PlayerJumpedEvent : IEvent
{
    public Vector3 JumpPosition { get; set; }
    public float JumpForce { get; set; }

    public PlayerJumpedEvent(Vector3 jumpPosition, float jumpForce)
    {
        JumpPosition = jumpPosition;
        JumpForce = jumpForce;
    }
}

public class PlayerCollisionEvent : IEvent
{
    public GameObject CollidedObject { get; set; }
    public string CollisionTag { get; set; }
    public Vector3 CollisionPoint { get; set; }

    public PlayerCollisionEvent(GameObject collidedObject, string collisionTag, Vector3 collisionPoint)
    {
        CollidedObject = collidedObject;
        CollisionTag = collisionTag;
        CollisionPoint = collisionPoint;
    }
}
