# Collection Framework - Complete System

## Tổng Quan

**Collection Framework** là một hệ thống tổng quát cho Collection, Inventory, và Gallery có thể tái sử dụng cho nhiều loại game khác nhau.

## Cấu Trúc

```
Core/Systems/CollectionFramework/
├── Shared/
│   ├── Interfaces/
│   │   └── ICollectionFramework.cs (6 interfaces)
│   └── Data/
│       └── ItemDefinition.cs
├── Collection/
│   └── (Existing Collectable System)
├── Inventory/
│   └── Core/
│       ├── InventorySlot.cs
│       └── InventorySystem.cs
└── Gallery/
    └── Core/
        ├── GalleryItem.cs
        └── GallerySystem.cs
```

## Core Components

### 1. ItemDefinition (ScriptableObject)
Base definition cho tất cả items:
- Identity (ID, Name, Description)
- Visuals (Icon, Sprite, Prefab, Color)
- Classification (Category, Rarity, Type)
- Inventory Properties (Stack size, Consumable, Unique)
- Value (Points, Sell price)
- Gallery (Show in gallery, Unlock conditions)

### 2. Interfaces
- `ICollectible` - Objects có thể collect
- `ICollector` - Objects collect items
- `IInventoryItem` - Items trong inventory
- `IInventory` - Inventory management
- `IGalleryItem` - Items trong gallery
- `IGallery` - Gallery/collection tracking

### 3. Inventory System
- **InventorySlot**: Single slot với stacking
- **InventorySystem**: Complete inventory management
  - Add/Remove items
  - Stacking support
  - Capacity management
  - Sorting & filtering
  - Category/Type filtering

### 4. Gallery System
- **GalleryItem**: Track unlock status & stats
- **GallerySystem**: Collection progress tracking
  - Register items
  - Unlock tracking
  - Completion percentage
  - Category/Rarity completion
  - Milestone achievements

## Quick Start

### Setup Inventory

```csharp
GameObject: Player
└── InventorySystem
    - Capacity: 50
    - Auto Sort: true
```

### Create Item Definition

```
Right-click → Create → Collection Framework → Item Definition
```

### Use Inventory

```csharp
// Get inventory
var inventory = GetComponent<InventorySystem>();

// Add items
inventory.Add(coinDefinition, 10);
inventory.Add(swordDefinition, 1);

// Remove items
inventory.Remove(potionDefinition, 1);

// Check items
bool hasSword = inventory.Has(swordDefinition);
int coinCount = inventory.GetCount(coinDefinition);
```

### Use Gallery

```csharp
// Register items (do once at game start)
GallerySystem.Instance.RegisterItem(itemDefinition);

// Unlock when collected
GallerySystem.Instance.UnlockItem(itemDefinition);

// Check progress
float completion = GallerySystem.Instance.GetCompletionPercentage();
int unlocked = GallerySystem.Instance.UnlockedItems;
```

## Events

```csharp
// Inventory events
EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
EventBus.Subscribe<InventoryClearedEvent>(OnInventoryCleared);

// Gallery events
EventBus.Subscribe<ItemUnlockedEvent>(OnItemUnlocked);
EventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
EventBus.Subscribe<CollectionMilestoneEvent>(OnMilestone);
EventBus.Subscribe<CollectionCompletedEvent>(OnCollectionComplete);
```

## Use Cases

### Pokemon-style Game
```csharp
// Catch pokemon
GallerySystem.Instance.UnlockItem(pikachuDef);

// Check pokedex
float pokedexCompletion = GallerySystem.Instance.GetCompletionPercentage();
```

### RPG Inventory
```csharp
// Loot items
inventory.Add(goldCoin, 50);
inventory.Add(healthPotion, 3);

// Use potion
if (inventory.Has(healthPotion))
{
    inventory.Remove(healthPotion, 1);
    player.Heal(50);
}
```

### Museum/Collection Game
```csharp
// Collect artifact
GallerySystem.Instance.UnlockItem(artifactDef);

// Display in museum
var artifacts = GallerySystem.Instance.GetUnlockedItems();
foreach (var artifact in artifacts)
{
    DisplayInMuseum(artifact);
}
```

## Integration

### With Existing Systems
- ✅ **EventBus** - All events published via EventBus
- ✅ **Object Pooling** - Collection system uses pooling
- ✅ **Singleton** - GallerySystem is persistent singleton

### With Worm Game
```csharp
// Replace ResourceDefinition with ItemDefinition
// Replace ResourceInventory with InventorySystem
// Add GallerySystem for collection tracking
```

## Benefits

✅ **Universal** - Works for any game type  
✅ **Modular** - Use components independently  
✅ **Extensible** - Easy to add custom logic  
✅ **Data-Driven** - ScriptableObject configuration  
✅ **Event-Driven** - Decoupled communication  
✅ **Save-Ready** - Serializable data structures  

## Next Steps

1. Create UI components (InventoryUI, GalleryUI)
2. Add save/load system
3. Create example scenes
4. Integrate with existing games
