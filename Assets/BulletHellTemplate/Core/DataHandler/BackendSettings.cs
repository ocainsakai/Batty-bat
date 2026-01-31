using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Enum that represents the available backend implementations.
    /// </summary>
    public enum BackendOption { Offline, Firebase, WebSocketSql }

    /// <summary>
    /// ScriptableObject that stores the chosen backend option and,
    /// when WebSocketSql is selected, the endpoint settings.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BackendSettings",
        menuName = "BulletHellTemplate/Backend/Backend Settings",
        order = 99)]
    public sealed class BackendSettings : ScriptableObject
    {
        [Header("Backend Selection")]
        public BackendOption option = BackendOption.Offline;

        [Header("WebSocket-SQL")]
        [Tooltip("Full Colyseus server URL, e.g. ws://localhost:2567")]
        public string serverUrl = "ws://localhost:2567";

        [Tooltip("Room name handling auth (register / login)")]
        public string authRoomName = "auth";
    }
}