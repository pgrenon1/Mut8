using System.Diagnostics.CodeAnalysis;
using SadConsole.Entities;

namespace Mut8.Scripts.Screens.Surfaces;

/// <summary>
/// A very basic SadConsole Console subclass that acts as a game message log.
/// </summary>
public class MessageLogPanel : Console
{
    private string _lastMessage = "";
    private int _lastMessageCount = 0;

    public MessageLogPanel(int width, int height)
        : base(width, height)
    {
        Initialize();
    }

    public MessageLogPanel(int width, int height, int bufferWidth, int bufferHeight)
        : base(width, height, bufferWidth, bufferHeight)
    {
        Initialize();
    }

    public MessageLogPanel(ICellSurface surface, IFont? font = null, Point? fontSize = null)
        : base(surface, font, fontSize)
    {
        Initialize();
    }

    [MemberNotNull("_lastMessage")]
    private void Initialize()
    {
        Cursor.AutomaticallyShiftRowsUp = true;
        _lastMessage = "";
    }

    public void AddMessage(Entity? instigator, string message)
    {
        if (instigator != null && !instigator.IsVisible)
            return;
        
        if (_lastMessage == message)
            _lastMessageCount++;
        else
        {
            _lastMessageCount = 1;
            _lastMessage = message;
        }

        string currentMessage = $"{DateTime.Now.ToString("HH:mm:ss:ffff")} : {_lastMessage}";
        
        if (_lastMessageCount > 1)
        {
            Cursor.Position = Cursor.Position.Translate(0, -1);
            Cursor.Print($"{currentMessage} (x{_lastMessageCount})");
        }
        else
            Cursor.Print(currentMessage);

        Cursor.NewLine();
    }

    public override string ToString()
    {
        return "Message Log Panel";
    }
}