# Singleton Pattern System - Hướng Dẫn Sử Dụng

## Tổng Quan

Hệ thống Singleton pattern cung cấp 4 loại singleton cho Unity:
1. **PureSingleton<T>** - Pure C# singleton (non-MonoBehaviour)
2. **MonoSingleton<T>** - Regular MonoBehaviour singleton
3. **PersistentSingleton<T>** - Persistent MonoBehaviour singleton (DontDestroyOnLoad)
4. **RegulatorSingleton<T>** - Regulator singleton (newer instance replaces older)

## Khi Nào Dùng Pattern Nào?

### PureSingleton<T> - Pure C# Singleton

**Sử dụng khi:**
- ✅ Class không cần MonoBehaviour features (Update, Coroutines, etc.)
- ✅ Quản lý data, logic, hoặc services
- ✅ Cần thread-safe access
- ✅ Không phụ thuộc vào Unity lifecycle

**Ví dụ use cases:**
- Game data manager
- Save/Load system
- Network manager
- Analytics manager

**Ưu điểm:**
- Thread-safe
- Lightweight (không cần GameObject)
- Có thể access từ bất kỳ thread nào
- Không bị ảnh hưởng bởi scene changes

**Nhược điểm:**
- Không có Unity lifecycle methods
- Không thể attach vào GameObject
- Không thể sử dụng Coroutines

---

### MonoSingleton<T> - Regular Singleton

**Sử dụng khi:**
- ✅ Cần MonoBehaviour features
- ✅ Manager chỉ tồn tại trong một scene
- ✅ Muốn manager bị destroy khi scene change

**Ví dụ use cases:**
- Level manager (specific to current level)
- Scene UI manager
- Enemy spawner
- Level-specific audio

**Ưu điểm:**
- Full MonoBehaviour features
- Auto-cleanup khi scene change
- Có thể attach components
- Có thể sử dụng Coroutines

**Nhược điểm:**
- Bị destroy khi scene change
- Không thread-safe
- Phụ thuộc vào Unity lifecycle

---

### PersistentSingleton<T> - Persistent Singleton

**Sử dụng khi:**
- ✅ Cần MonoBehaviour features
- ✅ Manager phải tồn tại across all scenes
- ✅ Muốn DontDestroyOnLoad behavior

**Ví dụ use cases:**
- Global audio manager
- Game state manager
- Input manager
- Achievement manager

**Ưu điểm:**
- Full MonoBehaviour features
- Persist across scenes
- Có thể attach components
- Có thể sử dụng Coroutines

**Nhược điểm:**
- Tồn tại suốt lifetime của app
- Cần careful cleanup
- Không thread-safe

---

### RegulatorSingleton<T> - Regulator Singleton

**Sử dụng khi:**
- ✅ Cần MonoBehaviour features
- ✅ Muốn instance mới nhất thay thế instance cũ
- ✅ Scene mới có audio/manager riêng cần override scene cũ

**Ví dụ use cases:**
- Scene-specific audio manager
- Level-specific game manager
- Scene transition managers
- Background music manager

**Ưu điểm:**
- Full MonoBehaviour features
- Newer instance auto-replaces older one
- Useful cho scene-specific managers
- Có thể sử dụng Coroutines

**Nhược điểm:**
- Phức tạp hơn regular singleton
- Cần hiểu rõ initialization order
- Không thread-safe

## Cách Sử Dụng

### 1. PureSingleton<T>

```csharp
using Core.Patterns;

public class GameDataManager : PureSingleton<GameDataManager>
{
    public int PlayerScore { get; set; }
    
    // Constructor phải public (required by new() constraint)
    public GameDataManager()
    {
        PlayerScore = 0;
    }
    
    public void AddScore(int points)
    {
        PlayerScore += points;
    }
}

// Sử dụng:
GameDataManager.Instance.AddScore(100);
int score = GameDataManager.Instance.PlayerScore;
```

### 2. MonoSingleton<T>

```csharp
using Core.Patterns;
using UnityEngine;

public class LevelManager : MonoSingleton<LevelManager>
{
    protected override void Awake()
    {
        base.Awake(); // QUAN TRỌNG: Gọi base.Awake()
        // Your initialization here
    }
    
    public void CompleteLevel()
    {
        Debug.Log("Level completed!");
    }
}

// Sử dụng:
LevelManager.Instance.CompleteLevel();
```

### 3. PersistentSingleton<T>

```csharp
using Core.Patterns;
using UnityEngine;

public class AudioManager : PersistentSingleton<AudioManager>
{
    protected override void Awake()
    {
        base.Awake(); // QUAN TRỌNG: Gọi base.Awake()
        // Your initialization here
    }
    
    public void PlayMusic(AudioClip clip)
    {
        // Play music logic
    }
}

// Sử dụng:
AudioManager.Instance.PlayMusic(myClip);
```

### 4. RegulatorSingleton<T>

```csharp
using Core.Patterns;
using UnityEngine;

public class SceneAudioManager : RegulatorSingleton<SceneAudioManager>
{
    [SerializeField] private AudioClip sceneMusic;
    
    protected override void Awake()
    {
        base.Awake(); // QUAN TRỌNG: Gọi base.Awake()
        
        // Only initialize if this is the current instance
        if (IsCurrentInstance)
        {
            PlayMusic(sceneMusic);
        }
    }
    
    public void PlayMusic(AudioClip clip)
    {
        // Play music logic
    }
}

// Sử dụng:
// Khi load scene mới, instance mới sẽ tự động thay thế instance cũ
SceneAudioManager.Instance.PlayMusic(newClip);
```

## Best Practices

### ✅ DO (Nên làm)

1. **Luôn gọi base.Awake() trong override**
   ```csharp
   protected override void Awake()
   {
       base.Awake(); // CRITICAL!
       // Your code here
   }
   ```

2. **Check HasInstance trước khi access**
   ```csharp
   if (AudioManager.HasInstance)
   {
       AudioManager.Instance.PlaySound();
   }
   ```

3. **Cleanup trong OnDestroy**
   ```csharp
   protected override void OnDestroy()
   {
       base.OnDestroy();
       // Cleanup resources
   }
   ```

4. **Sử dụng đúng pattern cho đúng use case**
   - Data/Logic → PureSingleton
   - Scene-specific → MonoSingleton
   - Game-wide → PersistentSingleton

### ❌ DON'T (Không nên làm)

1. **Không quên gọi base.Awake()**
   ```csharp
   // BAD - Sẽ tạo duplicates!
   protected override void Awake()
   {
       // Missing base.Awake()!
       InitializeStuff();
   }
   ```

2. **Không access Instance trong Awake của other scripts**
   ```csharp
   // BAD - Order of execution issue
   void Awake()
   {
       MyManager.Instance.DoSomething(); // Might not exist yet!
   }
   
   // GOOD - Use Start instead
   void Start()
   {
       MyManager.Instance.DoSomething();
   }
   ```

3. **Không tạo nhiều instances manually**
   ```csharp
   // BAD - Breaks singleton pattern!
   GameObject go = new GameObject();
   go.AddComponent<MyManager>();
   ```

4. **Không dùng PersistentSingleton cho scene-specific logic**
   ```csharp
   // BAD - Will persist when you don't want it to
   public class LevelUI : PersistentSingleton<LevelUI> { }
   
   // GOOD - Use MonoSingleton instead
   public class LevelUI : MonoSingleton<LevelUI> { }
   ```

## Examples

Xem các examples trong folder `Examples/`:
- [GameDataManager.cs](file:///Users/anh.pt/Batty-Bat/Assets/Core/Patterns/Singleton/Examples/GameDataManager.cs) - PureSingleton example
- [SceneUIManager.cs](file:///Users/anh.pt/Batty-Bat/Assets/Core/Patterns/Singleton/Examples/SceneUIManager.cs) - MonoSingleton example
- [GlobalAudioManager.cs](file:///Users/anh.pt/Batty-Bat/Assets/Core/Patterns/Singleton/Examples/GlobalAudioManager.cs) - PersistentSingleton example
- [RegulatorAudioManager.cs](file:///Users/anh.pt/Batty-Bat/Assets/Core/Patterns/Singleton/Examples/RegulatorAudioManager.cs) - RegulatorSingleton example

## Comparison Table

| Feature | PureSingleton | MonoSingleton | PersistentSingleton | RegulatorSingleton |
|---------|---------------|---------------|---------------------|-------------------|
| **Thread-Safe** | ✅ Yes | ❌ No | ❌ No | ❌ No |
| **MonoBehaviour** | ❌ No | ✅ Yes | ✅ Yes | ✅ Yes |
| **Update/Coroutines** | ❌ No | ✅ Yes | ✅ Yes | ✅ Yes |
| **Persist Scenes** | N/A | ❌ No | ✅ Yes | ❌ No |
| **Auto-Create** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **Replace Behavior** | N/A | Destroy new | Keep old | **Replace old** |
| **Lightweight** | ✅ Yes | ⚠️ Medium | ⚠️ Medium | ⚠️ Medium |

## Advanced Usage

### Lazy Initialization

Tất cả singleton patterns đều sử dụng lazy initialization - instance chỉ được tạo khi first access.

```csharp
// Instance chưa tồn tại
Debug.Log(GameDataManager.HasInstance); // false

// First access - instance được tạo
GameDataManager.Instance.AddScore(10);

// Instance đã tồn tại
Debug.Log(GameDataManager.HasInstance); // true
```

### Cleanup (PureSingleton only)

```csharp
// Destroy instance manually (mainly for testing)
GameDataManager.DestroyInstance();
```

### Preventing Access During Shutdown

MonoSingleton và PersistentSingleton tự động return `null` khi app đang quit:

```csharp
void OnApplicationQuit()
{
    // This will return null safely
    var manager = MyManager.Instance;
    if (manager != null)
    {
        manager.DoSomething();
    }
}
```

## Troubleshooting

### Duplicate Instances
**Problem**: Nhiều instances được tạo  
**Solution**: Đảm bảo gọi `base.Awake()` trong override

### Missing Instance
**Problem**: Instance is null  
**Solution**: Kiểm tra order of execution, sử dụng Start thay vì Awake

### Instance Persists When It Shouldn't
**Problem**: Manager không bị destroy khi scene change  
**Solution**: Sử dụng MonoSingleton thay vì PersistentSingleton

## Kết Luận

Singleton pattern giúp:
- ✅ Global access point
- ✅ Ensure single instance
- ✅ Lazy initialization
- ✅ Type-safe
- ✅ Easy to use

Chọn đúng pattern cho đúng use case để tối ưu performance và maintainability!
