namespace Monoboy.Desktop;

using System.Collections.Generic;

using Silk.NET.Input;

public class Input
{
    internal static IKeyboard Keyboard { get; set; }
    internal static IGamepad Gamepad { get; set; }

    private static Dictionary<Key, bool> oldKeys = new();

    public static bool GetKey(Key key)
    {
        return Keyboard.IsKeyPressed(key);
    }

    public static bool GetKeyDown(Key key)
    {
        bool state = Keyboard.IsKeyPressed(key);
        oldKeys[key] = state;
        return state && oldKeys[key] == false;
    }
}
