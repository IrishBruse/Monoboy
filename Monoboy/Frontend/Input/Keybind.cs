using SFML.Window;

namespace Monoboy.Frontend
{
    public class Keybind
    {
        private Key[] buttons;
        private InputAction action;
        private bool oldState;

        public Keybind(InputAction action, params Key[] buttons)
        {
            this.buttons = buttons;
            this.action = action;
        }

        public bool IsActive()
        {
            if(Application.Focused == false)
            {
                return false;
            }

            for(int i = 0; i < buttons.Length; i++)
            {
                bool newState;

                if((int)buttons[i] <= 100)
                {
                    Keyboard.Key button = (Keyboard.Key)buttons[i];
                    newState = Keyboard.IsKeyPressed(button);
                }
                else if((int)buttons[i] <= 105)
                {
                    Mouse.Button button = (Mouse.Button)(buttons[i] - 101);
                    newState = Mouse.IsButtonPressed(button);
                }
                else if((int)buttons[i] <= 115)
                {
                    uint button = (uint)(buttons[i] - 106);
                    newState = Joystick.IsButtonPressed(0, button);
                }
                else
                {
                    uint button = (uint)(buttons[i] - 116);
                    newState = button switch
                    {
                        0 => Joystick.GetAxisPosition(0, Joystick.Axis.PovY) > 0.1,
                        1 => Joystick.GetAxisPosition(0, Joystick.Axis.PovY) < -0.1,
                        2 => Joystick.GetAxisPosition(0, Joystick.Axis.PovX) < -0.1,
                        3 => Joystick.GetAxisPosition(0, Joystick.Axis.PovX) > 0.1,
                        _ => throw new System.NotImplementedException(),
                    };
                }


                bool result = action switch
                {
                    InputAction.Pressed => oldState == false && newState == true,
                    InputAction.Held => newState,
                    InputAction.Released => oldState == true && newState == false,
                    _ => false,
                };

                oldState = newState;
                if(result == true)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum InputAction
    {
        Pressed,
        Held,
        Released,
    }

    public enum Key
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
        H = 7,
        I = 8,
        J = 9,
        K = 10,
        L = 11,
        M = 12,
        N = 13,
        O = 14,
        P = 15,
        Q = 16,
        R = 17,
        S = 18,
        T = 19,
        U = 20,
        V = 21,
        W = 22,
        X = 23,
        Y = 24,
        Z = 25,
        Num0 = 26,
        Num1 = 27,
        Num2 = 28,
        Num3 = 29,
        Num4 = 30,
        Num5 = 31,
        Num6 = 32,
        Num7 = 33,
        Num8 = 34,
        Num9 = 35,
        Escape = 36,
        LControl = 37,
        LeftShift = 38,
        LAlt = 39,
        LSystem = 40,
        RControl = 41,
        RShift = 42,
        RAlt = 43,
        RSystem = 44,
        Menu = 45,
        LBracket = 46,
        RBracket = 47,
        Semicolon = 48,
        SemiColon = 48,
        Comma = 49,
        Period = 50,
        Quote = 51,
        Slash = 52,
        Backslash = 53,
        BackSlash = 53,
        Tilde = 54,
        Equal = 55,
        Hyphen = 56,
        Dash = 56,
        Space = 57,
        Enter = 58,
        Return = 58,
        Backspace = 59,
        BackSpace = 59,
        Tab = 60,
        PageUp = 61,
        PageDown = 62,
        End = 63,
        Home = 64,
        Insert = 65,
        Delete = 66,
        Add = 67,
        Subtract = 68,
        Multiply = 69,
        Divide = 70,
        Left = 71,
        Right = 72,
        Up = 73,
        Down = 74,
        Numpad0 = 75,
        Numpad1 = 76,
        Numpad2 = 77,
        Numpad3 = 78,
        Numpad4 = 79,
        Numpad5 = 80,
        Numpad6 = 81,
        Numpad7 = 82,
        Numpad8 = 83,
        Numpad9 = 84,
        F1 = 85,
        F2 = 86,
        F3 = 87,
        F4 = 88,
        F5 = 89,
        F6 = 90,
        F7 = 91,
        F8 = 92,
        F9 = 93,
        F10 = 94,
        F11 = 95,
        F12 = 96,
        F13 = 97,
        F14 = 98,
        F15 = 99,
        Pause = 100,
        MouseLeft = 101,
        MouseRight = 102,
        MouseMiddle = 103,
        MouseXButton1 = 104,
        MouseXButton2 = 105,
        JoystickA = 106,//0
        JoystickB = 107,//1
        JoystickX = 108,//2
        JoystickY = 109,//3
        JoystickLB = 110,//4
        JoystickRB = 111,//5
        JoystickSelect = 112,//6
        JoystickStart = 113,//7
        JoystickLeftStickClick = 114,//8
        JoystickRightStickClick = 115,//9
        JoystickUp = 116,
        JoystickDown = 117,
        JoystickLeft = 118,
        JoystickRight = 119,
    }
}
