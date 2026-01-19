/// <summary>
/// UI-related events
/// </summary>

public class PopupOpenedEvent : IEvent
{
    public string PopupName { get; set; }

    public PopupOpenedEvent(string popupName)
    {
        PopupName = popupName;
    }
}

public class PopupClosedEvent : IEvent
{
    public string PopupName { get; set; }

    public PopupClosedEvent(string popupName)
    {
        PopupName = popupName;
    }
}

public class ButtonClickedEvent : IEvent
{
    public string ButtonName { get; set; }
    public string Action { get; set; }

    public ButtonClickedEvent(string buttonName, string action = "")
    {
        ButtonName = buttonName;
        Action = action;
    }
}

public class UIScreenChangedEvent : IEvent
{
    public string OldScreen { get; set; }
    public string NewScreen { get; set; }

    public UIScreenChangedEvent(string oldScreen, string newScreen)
    {
        OldScreen = oldScreen;
        NewScreen = newScreen;
    }
}
