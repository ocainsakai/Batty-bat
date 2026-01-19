# Batty-Bat 🦇

Unity game project featuring a bat character navigating through obstacles.

## 📋 Project Overview

Batty-Bat là một game 2D được phát triển bằng Unity, trong đó người chơi điều khiển một chú dơi bay qua các chướng ngại vật và ghi điểm.

## 🎮 Game Features

- **Simple Controls**: Tap/Click để điều khiển bat
- **Progressive Difficulty**: Game speed tăng dần theo thời gian
- **Score System**: Theo dõi điểm số và high score
- **Clean UI**: Start screen và game over popup

## 🏗️ Architecture

### Event Bus System

Project sử dụng một hệ thống Event Bus mạnh mẽ để quản lý communication giữa các components:

- **Decoupled Architecture**: Components giao tiếp qua events thay vì direct references
- **Type-Safe**: Sử dụng generics để đảm bảo type safety
- **Easy to Extend**: Dễ dàng thêm features mới mà không ảnh hưởng code cũ

📖 **Documentation**: Xem chi tiết tại [EventSystem/README.md](Assets/_BattyBat/Scripts/EventSystem/README.md)

### Project Structure

```
Assets/_BattyBat/
├── Scripts/
│   ├── EventSystem/          # Event Bus implementation
│   │   ├── EventBus.cs       # Core Event Bus singleton
│   │   ├── IEvent.cs         # Base event interface
│   │   ├── EventDebugger.cs  # Debug tool
│   │   ├── Events/           # Event definitions
│   │   ├── Examples/         # Usage examples
│   │   └── README.md         # Detailed documentation
│   ├── GameManager.cs        # Main game controller
│   ├── BattyBat.cs          # Player controller
│   ├── Environment.cs        # Environment management
│   ├── Spawner.cs           # Obstacle spawner
│   └── Rosin.cs             # Obstacle behavior
└── ...
```

## 🚀 Getting Started

### Prerequisites

- Unity 2021.3 or later
- Basic understanding of Unity and C#

### Setup

1. Clone the repository
2. Open project in Unity
3. Open the main scene
4. Press Play to test

## 🎯 How to Use Event Bus

### Quick Example

```csharp
// Subscribe to events
private void OnEnable()
{
    EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted);
}

// Handle events
private void OnGameStarted(GameStartedEvent evt)
{
    Debug.Log($"Game started with speed: {evt.GameSpeed}");
}

// Unsubscribe (important!)
private void OnDisable()
{
    EventBus.Instance.Unsubscribe<GameStartedEvent>(OnGameStarted);
}

// Publish events
EventBus.Instance.Publish(new GameStartedEvent(1.5f));
```

### Available Events

- **Game Events**: `GameStartedEvent`, `GameOverEvent`, `ScoreChangedEvent`, etc.
- **Player Events**: `PlayerDiedEvent`, `PlayerJumpedEvent`, `PlayerCollisionEvent`
- **UI Events**: `PopupOpenedEvent`, `ButtonClickedEvent`, etc.

## 🛠️ Development

### Adding New Features

1. Define your event in `Events/` folder
2. Subscribe to events in your component
3. Publish events when actions occur
4. Always unsubscribe to prevent memory leaks!

### Debug Tools

Attach `EventDebugger` component to any GameObject để monitor events real-time trong Console.

## 📝 Code Style

- Use PascalCase for public members
- Use camelCase with underscore prefix for private fields
- Always include XML documentation for public APIs
- Follow Unity best practices

## 🐛 Debugging

- Enable `EventDebugger` để track event flow
- Check Console logs for event activity
- Use Unity Profiler để monitor performance

## 📚 Resources

- [Event Bus Documentation](Assets/_BattyBat/Scripts/EventSystem/README.md)
- [Unity Documentation](https://docs.unity3d.com/)

## 🤝 Contributing

Khi contribute code:
1. Follow existing code style
2. Add XML documentation
3. Test thoroughly
4. Update documentation nếu cần

## 📄 License

[Add your license here]

## ✨ Credits

Developed with Unity and ❤️

---

**Happy Coding! 🦇✨**
