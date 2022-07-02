namespace Monoboy.Desktop;

using System;

using Monoboy.Desktop.Data;

using NativeFileDialogSharp;


using Veldrid;

public class Application
{
    private VeldridWindow Window { get; set; }

    private Emulator emulator;

    public Application()
    {
        Window = new(GraphicsBackend.Vulkan, Icon.Data);

        emulator = new(Boot.DMG);

        Window.Update += OnUpdate;
        Window.FileDrop += OnFilesDrop;
    }

    public void Run()
    {
        Window.Run();
    }

    public void OnUpdate(double deltaTime)
    {
        Window.Title = "Monoboy" + (emulator.GameTitle != "" ? $" - {emulator.GameTitle}" : "");

        emulator.SetButtonState(GameboyButton.Right, Input.GetKey(Key.D));
        emulator.SetButtonState(GameboyButton.Left, Input.GetKey(Key.A));
        emulator.SetButtonState(GameboyButton.Up, Input.GetKey(Key.W));
        emulator.SetButtonState(GameboyButton.Down, Input.GetKey(Key.S));

        emulator.SetButtonState(GameboyButton.A, Input.GetKey(Key.ShiftLeft));
        emulator.SetButtonState(GameboyButton.B, Input.GetKey(Key.Space));
        emulator.SetButtonState(GameboyButton.Start, Input.GetKey(Key.Escape));
        emulator.SetButtonState(GameboyButton.Select, Input.GetKey(Key.Enter));

        if (Input.GetKey(Key.F2))
        {
            Window.Screenshot();
        }

        if (Input.GetKeyDown(Key.ControlLeft) && Input.GetKey(Key.O))
        {
            DialogResult file = Dialog.FileOpen("gb,gbc", Environment.CurrentDirectory);
            if (file.IsOk)
            {
                emulator.Open(file.Path);
            }
        }

        emulator.StepFrame();
    }

    public void OnFilesDrop(string[] files)
    {
        if (files.Length == 0)
        {
            return;
        }
        emulator.Open(files[0]);
    }
}
