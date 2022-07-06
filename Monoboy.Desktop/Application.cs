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

        Window.Framebuffer = emulator.Framebuffer;

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

        emulator.SetButtonState(GameboyButton.Right, Input.GetKey(KeyCode.D));
        emulator.SetButtonState(GameboyButton.Left, Input.GetKey(KeyCode.A));
        emulator.SetButtonState(GameboyButton.Up, Input.GetKey(KeyCode.W));
        emulator.SetButtonState(GameboyButton.Down, Input.GetKey(KeyCode.S));

        emulator.SetButtonState(GameboyButton.A, Input.GetKey(KeyCode.ShiftLeft));
        emulator.SetButtonState(GameboyButton.B, Input.GetKey(KeyCode.Space));
        emulator.SetButtonState(GameboyButton.Start, Input.GetKey(KeyCode.Escape));
        emulator.SetButtonState(GameboyButton.Select, Input.GetKey(KeyCode.Enter));

        if (Input.GetKey(KeyCode.F2))
        {
            Window.Screenshot();
        }

        if (Input.GetKeyDown(KeyCode.ControlLeft) && Input.GetKey(KeyCode.O))
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
