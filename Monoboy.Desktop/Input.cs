namespace Monoboy.Desktop;

using System.Collections.Generic;

using Silk.NET.Input;

public class Input
{
    internal static IKeyboard Keyboard { get; set; }
    internal static IGamepad Gamepad { get; set; }

    private static Dictionary<KeyCode, bool> oldKeys = new();

    public static bool GetKey(KeyCode key)
    {
        return Keyboard.IsKeyPressed((Key)key);
    }

    public static bool GetKeyDown(KeyCode key)
    {
        bool state = Keyboard.IsKeyPressed((Key)key);
        oldKeys[key] = state;
        return state && oldKeys[key] == false;
    }
}
