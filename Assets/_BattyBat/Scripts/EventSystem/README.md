# Event Bus System - Hướng Dẫn Sử Dụng

## Tổng Quan

Event Bus là một hệ thống messaging pattern giúp các components trong game giao tiếp với nhau mà không cần biết về sự tồn tại của nhau. Điều này giúp code dễ maintain, test và mở rộng hơn.

## Cấu Trúc Hệ Thống

```
EventSystem/
├── IEvent.cs                    # Base interface cho tất cả events
├── EventBus.cs                  # Core Event Bus singleton
├── EventDebugger.cs            # Debug tool
├── Events/
│   ├── GameEvents.cs           # Game-related events
│   ├── PlayerEvents.cs         # Player-related events
│   └── UIEvents.cs             # UI-related events
└── Examples/
    └── EventBusExample.cs      # Usage examples
```

## Cách Sử Dụng Cơ Bản

### 1. Subscribe to Events (Đăng ký nhận events)

```csharp
private void OnEnable()
{
    EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted);
    EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
}

private void OnGameStarted(GameStartedEvent evt)
{
    Debug.Log($"Game started with speed: {evt.GameSpeed}");
}

private void OnScoreChanged(ScoreChangedEvent evt)
{
    Debug.Log($"Score: {evt.NewScore}");
}
```

### 2. Unsubscribe from Events (Hủy đăng ký)

**QUAN TRỌNG**: Luôn luôn unsubscribe để tránh memory leaks!

```csharp
private void OnDisable()
{
    EventBus.Instance.Unsubscribe<GameStartedEvent>(OnGameStarted);
    EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
}
```

### 3. Publish Events (Phát sự kiện)

```csharp
// Publish một event đơn giản
EventBus.Instance.Publish(new GameStartedEvent(1.5f));

// Publish event với nhiều data
EventBus.Instance.Publish(new ScoreChangedEvent(oldScore: 10, newScore: 20));

// Publish event khi player chết
EventBus.Instance.Publish(new PlayerDiedEvent(
    deathPosition: transform.position,
    causeOfDeath: "Collision with obstacle"
));
```

## Events Có Sẵn

### Game Events
- `GameStartedEvent` - Khi game bắt đầu
- `GameOverEvent` - Khi game kết thúc
- `GamePausedEvent` - Khi game pause
- `GameResumedEvent` - Khi game resume
- `ScoreChangedEvent` - Khi điểm thay đổi
- `GameStateChangedEvent` - Khi game state thay đổi
- `HighScoreAchievedEvent` - Khi đạt high score mới

### Player Events
- `PlayerDiedEvent` - Khi player chết
- `PlayerJumpedEvent` - Khi player nhảy
- `PlayerCollisionEvent` - Khi player va chạm

### UI Events
- `PopupOpenedEvent` - Khi popup mở
- `PopupClosedEvent` - Khi popup đóng
- `ButtonClickedEvent` - Khi button được click
- `UIScreenChangedEvent` - Khi màn hình UI thay đổi

## Tạo Custom Events

### Bước 1: Tạo Event Class

```csharp
public class PowerUpCollectedEvent : IEvent
{
    public string PowerUpType { get; set; }
    public int BonusPoints { get; set; }
    public Vector3 CollectionPosition { get; set; }

    public PowerUpCollectedEvent(string powerUpType, int bonusPoints, Vector3 position)
    {
        PowerUpType = powerUpType;
        BonusPoints = bonusPoints;
        CollectionPosition = position;
    }
}
```

### Bước 2: Subscribe và Publish

```csharp
// Subscribe
EventBus.Instance.Subscribe<PowerUpCollectedEvent>(OnPowerUpCollected);

// Handler
private void OnPowerUpCollected(PowerUpCollectedEvent evt)
{
    Debug.Log($"Collected {evt.PowerUpType} for {evt.BonusPoints} points!");
}

// Publish
EventBus.Instance.Publish(new PowerUpCollectedEvent(
    powerUpType: "Speed Boost",
    bonusPoints: 50,
    position: transform.position
));
```

## Best Practices

### ✅ DO (Nên làm)

1. **Luôn unsubscribe trong OnDisable/OnDestroy**
   ```csharp
   private void OnDisable()
   {
       EventBus.Instance.Unsubscribe<GameStartedEvent>(OnGameStarted);
   }
   ```

2. **Sử dụng OnEnable/OnDisable cho MonoBehaviour**
   ```csharp
   private void OnEnable() { /* Subscribe */ }
   private void OnDisable() { /* Unsubscribe */ }
   ```

3. **Đặt tên events rõ ràng và mô tả**
   ```csharp
   public class PlayerHealthChangedEvent : IEvent { }
   ```

4. **Bao gồm context data trong events**
   ```csharp
   public class DamageReceivedEvent : IEvent
   {
       public int DamageAmount { get; set; }
       public GameObject DamageSource { get; set; }
   }
   ```

### ❌ DON'T (Không nên làm)

1. **Không quên unsubscribe**
   ```csharp
   // BAD - sẽ gây memory leak!
   private void OnEnable()
   {
       EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted);
   }
   // Thiếu OnDisable để unsubscribe!
   ```

2. **Không subscribe nhiều lần cùng một handler**
   ```csharp
   // BAD
   EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted);
   EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted); // Duplicate!
   ```

3. **Không throw exceptions trong event handlers**
   ```csharp
   // BAD
   private void OnGameStarted(GameStartedEvent evt)
   {
       throw new Exception("This will break other listeners!");
   }
   
   // GOOD
   private void OnGameStarted(GameStartedEvent evt)
   {
       try
       {
           // Your code
       }
       catch (Exception ex)
       {
           Debug.LogError($"Error handling GameStartedEvent: {ex.Message}");
       }
   }
   ```

## Debug và Testing

### Sử dụng EventDebugger

1. Tạo một GameObject mới trong scene
2. Attach script `EventDebugger`
3. Enable logging trong Inspector
4. Chạy game và xem events trong Console

```csharp
// Trong code, bạn có thể:
var debugger = FindObjectOfType<EventDebugger>();
Debug.Log(debugger.GetEventHistory());
debugger.ClearHistory();
```

### Kiểm tra số lượng listeners

```csharp
int listenerCount = EventBus.Instance.GetListenerCount<GameStartedEvent>();
Debug.Log($"GameStartedEvent has {listenerCount} listeners");

bool hasListeners = EventBus.Instance.HasListeners<ScoreChangedEvent>();
if (hasListeners)
{
    EventBus.Instance.Publish(new ScoreChangedEvent(0, 100));
}
```

## Use Cases Thực Tế

### 1. Audio System
```csharp
// AudioManager.cs
private void OnEnable()
{
    EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted);
    EventBus.Instance.Subscribe<GameOverEvent>(OnGameOver);
    EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
}

private void OnGameStarted(GameStartedEvent evt)
{
    PlayMusic("GameplayMusic");
}

private void OnScoreChanged(ScoreChangedEvent evt)
{
    PlaySFX("ScoreSound");
}
```

### 2. Analytics Tracking
```csharp
// AnalyticsManager.cs
private void OnEnable()
{
    EventBus.Instance.Subscribe<GameOverEvent>(OnGameOver);
    EventBus.Instance.Subscribe<HighScoreAchievedEvent>(OnHighScore);
}

private void OnGameOver(GameOverEvent evt)
{
    Analytics.CustomEvent("game_over", new Dictionary<string, object>
    {
        { "score", evt.FinalScore },
        { "reason", evt.Reason }
    });
}
```

### 3. UI Updates
```csharp
// ScoreUI.cs
private void OnEnable()
{
    EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
}

private void OnScoreChanged(ScoreChangedEvent evt)
{
    scoreText.text = evt.NewScore.ToString();
    PlayScoreAnimation();
}
```

## Performance Tips

1. **Event Bus là singleton** - Không cần cache reference
2. **Unsubscribe ngay khi không cần** - Giảm overhead
3. **Tránh publish events quá thường xuyên** - Cân nhắc batching
4. **Sử dụng struct thay vì class cho simple events** (optional optimization)

## Troubleshooting

### Event không được nhận
- ✓ Kiểm tra đã subscribe chưa
- ✓ Kiểm tra event type có đúng không
- ✓ Kiểm tra object có bị disabled không

### Memory Leaks
- ✓ Đảm bảo unsubscribe trong OnDisable/OnDestroy
- ✓ Sử dụng EventDebugger để track listeners

### Events bị gọi nhiều lần
- ✓ Kiểm tra không subscribe duplicate
- ✓ Kiểm tra OnEnable/OnDisable lifecycle

## Kết Luận

Event Bus giúp:
- ✅ Decoupling components
- ✅ Code dễ test và maintain
- ✅ Dễ dàng thêm features mới
- ✅ Tránh tight coupling giữa các systems

Hãy sử dụng Event Bus cho mọi cross-component communication trong game!
