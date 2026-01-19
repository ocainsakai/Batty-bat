# Collectable System - Shared Core System

## Tổng Quan

Hệ thống thu thập vật phẩm (Collectable System) là một shared system có thể tái sử dụng cho tất cả games. Hỗ trợ collection mechanics, inventory tracking, và spawn management với object pooling.

## Cấu Trúc

```
Core/Systems/CollectableSystem/
├── Interfaces/
│   ├── ICollectable.cs          # Interface cho collectible objects
│   ├── ICollector.cs            # Interface cho collector objects
│   └── IInventory.cs            # Interface cho inventory systems
├── Data/
│   └── CollectibleDefinition.cs # ScriptableObject config
└── Components/
    ├── Collectable.cs           # Collectible component
    ├── CollectorComponent.cs    # Collector component
    ├── CollectableInventory.cs  # Inventory tracking
    └── CollectableSpawner.cs    # Spawn management
```

## Quick Start

### 1. Create Collectable Definition (ScriptableObject)

```
Right-click in Project → Create → Core → Collectable Definition
```

Configure:
- Name, ID, Color, Sprite
- Value (points, growth, currency)
- Type (Resource, Coin, PowerUp, etc.)
- Spawn settings (pattern, count, respawn time)

### 2. Setup Collector (Player/Entity)

```csharp
GameObject: Player
├── CollectorComponent
└── CollectableInventory
```

### 3. Setup Spawner

```csharp
GameObject: CollectableSpawner
└── CollectableSpawner (script)
    - Assign CollectibleDefinitions
    - Assign Collectable Prefab
```

### 4. Create Collectable Prefab

```
GameObject: Collectable
├── SpriteRenderer
├── CircleCollider2D (trigger)
└── Collectable (script)
```

## Usage Examples

### Example 1: Coin Collection

```csharp
// Create CoinDefinition.asset
CollectibleName = "Gold Coin"
CollectibleID = "coin_gold"
Value = 10
Type = Coin
RespawnTime = 0 (no respawn)
```

### Example 2: Health Pickup

```csharp
// Create HealthPickup.asset
CollectibleName = "Health Pack"
CollectibleID = "health_pack"
Value = 25
Type = Pickup
RespawnTime = 30
```

### Example 3: Custom Collectable

```csharp
public class PowerUpCollectable : Collectable
{
    public override void OnCollected(GameObject collector)
    {
        base.OnCollected(collector);
        
        // Custom logic
        var player = collector.GetComponent<Player>();
        if (player != null)
        {
            player.ApplyPowerUp(Definition.Value);
        }
    }
}
```

## Spawn Patterns

- **Random**: Spawn anywhere in area
- **Grid**: Spawn in grid pattern
- **Cluster**: Spawn in clusters
- **Circle**: Spawn in circle
- **Line**: Spawn along line

## Events

```csharp
// Subscribe to collection events
EventBus.Subscribe<CollectableCollectedEvent>(OnCollected);
EventBus.Subscribe<CollectableAddedEvent>(OnInventoryChanged);
```

## Integration với Worm Game

Worm game có thể sử dụng system này bằng cách:

1. Tạo `WormResource : Collectable`
2. Tạo `WormCollector : CollectorComponent`
3. Sử dụng `CollectableInventory` thay vì `ResourceInventory`

## Benefits

✅ **Reusable** - Dùng cho mọi game  
✅ **Flexible** - Customize qua ScriptableObjects  
✅ **Performant** - Object pooling built-in  
✅ **Event-driven** - Decoupled communication  
✅ **Extensible** - Interfaces cho custom logic  

## Use Cases

- Coins/Currency collection
- Resource gathering
- Power-up pickups
- Collectible items
- Loot drops
- Quest items
