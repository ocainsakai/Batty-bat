using System.Threading;
using UnityEngine;

namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a character performs a dash action.
    /// Used to notify other systems (e.g., animation, effects, cooldown logic) of a dash event.
    /// </summary>
    public partial struct PlayerDashEvent
    {
        /// <summary>
        /// The character executing the dash.
        /// </summary>
        public CharacterEntity Target;

        /// <summary>
        /// The direction in which the dash is performed.
        /// </summary>
        public Vector3 dir;

        /// <summary>
        /// The speed applied during the dash.
        /// </summary>
        public float dashSpeed;

        /// <summary>
        /// The duration of the dash in seconds.
        /// </summary>
        public float dashDuration;


        /// <summary>
        /// A cancellation token source to interrupt the dash if needed.
        /// Useful for safely stopping coroutines or async operations.
        /// </summary>
        public CancellationToken dashCts;

        /// <summary>
        /// Creates a new instance of the PlayerDashEvent.
        /// </summary>
        /// <param name="target">The character executing the dash.</param>
        /// <param name="_dir">The direction of the dash.</param>
        /// <param name="_dashSpeed">The speed of the dash.</param>
        /// <param name="_dashDuration">The duration of the dash in seconds.</param>
        /// <param name="_dashCts">The cancellation token source for the dash operation.</param>
        public PlayerDashEvent(CharacterEntity target, Vector3 _dir, float _dashSpeed, float _dashDuration, CancellationToken _dashCts)
        {
            Target = target;
            dir = _dir;
            dashSpeed = _dashSpeed;
            dashDuration = _dashDuration;
            dashCts = _dashCts;
        }
    }
}
