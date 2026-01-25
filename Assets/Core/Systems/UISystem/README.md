# Core UI System

A comprehensive UI framework for managing screens, popups, and UI components.

## Features

- **UIView** - Base class with show/hide animations
- **UIScreen** - Full-screen views with navigation history
- **UIPopup** - Overlay dialogs with background dimming
- **UIManager** - Central navigation and popup management
- **Inventory UI** - Ready-to-use inventory components

## Quick Start

### 1. Setup UIManager

Add `UIManager` component to a persistent GameObject in your scene (it will persist across scenes).

### 2. Create a Screen

```csharp
public class MainMenuScreen : UIScreen
{
    protected override void OnAfterShow()
    {
        base.OnAfterShow();
        Debug.Log("Main menu is now visible");
    }
}
```

### 3. Create a Popup

```csharp
public class SettingsPopup : UIPopup
{
    public void OnSaveClicked()
    {
        // Save settings
        Close(); // Close popup
    }
}
```

### 4. Navigate Between Screens

```csharp
// Show a screen
await UIManager.Instance.ShowScreen("MainMenu");

// Show a popup
await UIManager.Instance.ShowPopup("Settings");

// Go back (closes popup or returns to previous screen)
await UIManager.Instance.GoBack();
```

## Components

### UIView (Base)

| Property | Description |
|----------|-------------|
| `viewId` | Unique identifier for this view |
| `showOnStart` | Show automatically on Start() |
| `destroyOnHide` | Destroy GameObject when hidden |
| `animationType` | Animation style (Fade, Scale, Slide, etc.) |
| `showDuration` | Animation duration for show |
| `hideDuration` | Animation duration for hide |

**Animation Types:**
- `None` - No animation
- `Fade` - Alpha fade in/out
- `Scale` - Scale from 0 to 1
- `SlideFromLeft/Right/Top/Bottom` - Slide in from screen edge
- `FadeAndScale` - Combination of fade and scale

### UIScreen

Extends UIView with:
- `hideOthersOnShow` - Hide other screens when this one shows
- `canGoBack` - Allow going back to this screen
- Automatic navigation history

### UIPopup

Extends UIView with:
- `showBackground` - Show dimmed background
- `closeOnBackgroundClick` - Click background to close
- `closeOnBackButton` - Android back button closes popup
- `backgroundColor` - Background overlay color

## Inventory UI

### InventorySlotUI

Individual inventory slot component.

```csharp
// Set item
slot.SetItem("item_001", itemSprite, 5, "Health Potion");

// Update count
slot.UpdateCount(3);

// Clear slot
slot.SetEmpty();

// Play animations
slot.PlayAddAnimation();
slot.PlayRemoveAnimation();
```

### InventoryGridUI

Grid of inventory slots that syncs with CollectableInventory.

```csharp
// Assign target inventory
inventoryGrid.targetInventory = playerInventory;

// Refresh from inventory
inventoryGrid.RefreshFromInventory();

// Listen for item clicks
inventoryGrid.OnItemClicked = (itemId, count) => {
    Debug.Log($"Clicked: {itemId} x{count}");
};
```

### InventoryPopup

Ready-to-use popup for displaying inventory.

```csharp
// Show inventory popup
var popup = UIManager.Instance.GetPopup("Inventory") as InventoryPopup;
popup.SetInventory(playerInventory);
popup.SetTitle("Your Items");
await popup.Show();
```

## Events

### Screen Events

```csharp
EventBus.Subscribe<ScreenChangedEvent>(evt => {
    Debug.Log($"Screen changed to: {evt.NewScreen.ViewId}");
});
```

### View Events (Per-Instance)

```csharp
myView.OnShowStarted.AddListener(() => Debug.Log("Showing..."));
myView.OnShowCompleted.AddListener(() => Debug.Log("Shown!"));
myView.OnHideStarted.AddListener(() => Debug.Log("Hiding..."));
myView.OnHideCompleted.AddListener(() => Debug.Log("Hidden!"));
```

## Setup Hierarchy

```
Canvas
├── Screens
│   ├── MainMenuScreen (UIScreen)
│   ├── GameplayScreen (UIScreen)
│   └── PauseScreen (UIScreen)
├── Popups
│   ├── SettingsPopup (UIPopup)
│   ├── InventoryPopup (InventoryPopup)
│   └── ConfirmPopup (UIPopup)
└── UIManager
```

## Dependencies

- **LeanTween** - For animations (or you can modify to use DOTween)
- **UniTask** - For async/await patterns
- **TextMeshPro** - For text components
