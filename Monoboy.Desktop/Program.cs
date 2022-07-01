namespace Monoboy.Desktop;

using System;

using Monoboy.Desktop.Data;

using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

public class Program
{
    private static IWindow window;
    private static Emulator emulator;
    private const GraphicsBackend PreferredBackend = GraphicsBackend.Vulkan;

    private static GraphicsDevice GraphicsDevice { get; set; }
    private static CommandList commandList;
    private static DeviceBuffer vertexBuffer;
    private static DeviceBuffer indexBuffer;
    private static Shader[] shaders;
    private static Pipeline pipeline;
    private static ResourceSet resourceSet;
    private static Texture renderBufferTexture;
    private static TextureView textureView;
    private static double timer;

    private static IKeyboard Keyboard { get; set; }
    private static IGamepad Gamepad { get; set; }

    private static readonly string VertShaderCode = @"
    #version 450

    layout(location = 0) in vec2 Position;
    layout(location = 1) in vec2 TexCoords;
    layout(location = 0) out vec2 fsin_texCoords;

    void main()
    {
        gl_Position = vec4(Position, 0, 1);
        fsin_texCoords = TexCoords;
    }
";
    private static readonly string FragShaderCode = @"
    #version 450

    layout(location = 0) in vec2 fsin_texCoords;
    layout(location = 0) out vec4 fsout_Color;

    layout(set = 0, binding = 0) uniform texture2D SurfaceTexture;
    layout(set = 0, binding = 1) uniform sampler SurfaceSampler;

    void main()
    {
        fsout_Color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);
    }
";

    public static void Main()
    {
        const int windowScale = 4;

        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(Emulator.WindowWidth * windowScale, Emulator.WindowHeight * windowScale);
        options.Title = "Monoboy";
        options.VSync = false;
        options.API = PreferredBackend.ToGraphicsAPI();
        options.ShouldSwapAutomatically = false;

        window = Window.Create(options);

        // Events
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClosing;
        window.Resize += OnResize;
        window.FileDrop += OnFilesDrop;

        window.Run();
    }

    private static void OnLoad()
    {
        RawImage icon = new(32, 32, Icon.Data);
        window.SetWindowIcon(ref icon);

        GraphicsDeviceOptions options = new();
        options.PreferStandardClipSpaceYDirection = true;
        options.PreferDepthRangeZeroToOne = true;
        options.SyncToVerticalBlank = true;

        GraphicsDevice = window.CreateGraphicsDevice(options, PreferredBackend);

        Input();

        emulator = new();
        emulator.Bios = Boot.DMG;
        emulator.Open("./Roms/Pokemon - Red Version.gb");

        GenerateResources();

        OnResize(window.Size);
    }

    private static void Input()
    {
        IInputContext input = window.CreateInput();

        foreach (IKeyboard keyboard in input.Keyboards)
        {
            if (!keyboard.IsConnected)
            {
                continue;
            }

            Keyboard = keyboard;
            break;
        }

        input.ConnectionChanged += (sender, args) =>
        {
            if (sender is IGamepad controller)
            {
                Console.WriteLine("Gamepad connected: " + controller.IsConnected);
                if (controller.IsConnected)
                {
                    Gamepad = controller;
                }
                else
                {
                    Gamepad = null;
                }
            }
        };

        foreach (IGamepad gamepad in input.Gamepads)
        {
            if (!gamepad.IsConnected)
            {
                continue;
            }

            Gamepad = gamepad;
            break;
        }
    }

    private static void OnResize(Vector2D<int> size)
    {
        size = new Vector2D<int>(Math.Max(size.X, Emulator.WindowWidth), Math.Max(size.Y, Emulator.WindowHeight));

        float scale = size.Y / Emulator.WindowHeight;
        float height = scale * Emulator.WindowHeight / size.Y;
        float width = scale * Emulator.WindowWidth / size.X;

        Vector4D<float>[] vertices =
        {
            new(-width, height,0.0f,1.0f),
            new(+width, height,1.0f,1.0f),
            new(-width, -height,0.0f,0.0f),
            new(+width, -height,1.0f,0.0f),
        };

        GraphicsDevice.UpdateBuffer(vertexBuffer, 0, vertices);
        GraphicsDevice.ResizeMainWindow((uint)size.X, (uint)size.Y);

        window.Size = size;

        window.DoRender();
    }

    private static void GenerateResources()
    {
        ResourceFactory factory = GraphicsDevice.ResourceFactory;

        Vector4D<float>[] vertices =
        {
            new(-0.5f, +0.5f,0.0f,1.0f),
            new(+0.5f, +0.5f,1.0f,1.0f),
            new(-0.5f, -0.5f,0.0f,0.0f),
            new(+0.5f, -0.5f,1.0f,0.0f),
        };
        ushort[] indices = { 0, 1, 2, 2, 1, 3 };

        BufferDescription vbDescription = new((uint)vertices.Length * 16, BufferUsage.VertexBuffer);
        BufferDescription ibDescription = new((uint)indices.Length * sizeof(ushort), BufferUsage.IndexBuffer);

        vertexBuffer = factory.CreateBuffer(vbDescription);
        indexBuffer = factory.CreateBuffer(ibDescription);

        GraphicsDevice.UpdateBuffer(indexBuffer, 0, indices);

        renderBufferTexture = factory.CreateTexture(TextureDescription.Texture2D(Emulator.WindowWidth, Emulator.WindowHeight, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
        textureView = factory.CreateTextureView(renderBufferTexture);

        VertexLayoutDescription vertexLayout = new(
            new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );

        ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, System.Text.Encoding.UTF8.GetBytes(VertShaderCode), "main");
        ShaderDescription fragmentShaderDesc = new(ShaderStages.Fragment, System.Text.Encoding.UTF8.GetBytes(FragShaderCode), "main");

        shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        ResourceLayout resourceLayout = factory.CreateResourceLayout(
                    new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                        new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment))
                );

        // Create pipeline
        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
        pipelineDescription.DepthStencilState = new(true, true, ComparisonKind.LessEqual);
        pipelineDescription.RasterizerState = new(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false);
        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
        pipelineDescription.ShaderSet = new(new[] { vertexLayout }, shaders);
        pipelineDescription.ResourceLayouts = new[] { resourceLayout };
        pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;

        pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
             resourceLayout,
             textureView,
             factory.CreateSampler(SamplerDescription.Point)));

        commandList = factory.CreateCommandList();
    }

    private static void OnUpdate(double deltaTime)
    {
        timer += deltaTime;

        if (timer > .25)
        {
            timer -= .25f;
            double fps = Math.Round(1 / deltaTime * 10) / 10;
            window.Title = $"Monoboy - {emulator.GameTitle} - FPS: {fps}";
        }

        emulator.SetButtonState(GameboyButton.Right, Keyboard.IsKeyPressed(Key.D));
        emulator.SetButtonState(GameboyButton.Left, Keyboard.IsKeyPressed(Key.A));
        emulator.SetButtonState(GameboyButton.Up, Keyboard.IsKeyPressed(Key.W));
        emulator.SetButtonState(GameboyButton.Down, Keyboard.IsKeyPressed(Key.S));

        emulator.SetButtonState(GameboyButton.A, Keyboard.IsKeyPressed(Key.ShiftLeft));
        emulator.SetButtonState(GameboyButton.B, Keyboard.IsKeyPressed(Key.Space));
        emulator.SetButtonState(GameboyButton.Start, Keyboard.IsKeyPressed(Key.Escape));
        emulator.SetButtonState(GameboyButton.Select, Keyboard.IsKeyPressed(Key.Enter));

        if (Keyboard.IsKeyPressed(Key.F2))
        {
            SaveScreenToFile();
        }

        emulator.StepFrame();
    }

    private static void OnRender(double deltaTime)
    {
        GraphicsDevice.UpdateTexture(renderBufferTexture, emulator.Framebuffer, 0, 0, 0, Emulator.WindowWidth, Emulator.WindowHeight, 1, 0, 0);

        commandList.Begin();// Begin() must be called before commands can be issued.
        {
            // We want to render directly to the output window.
            commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, new(25 / 255f, 29 / 255f, 31 / 255f, 1f));

            // Set all relevant state to draw our quad.
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(slot: 0, resourceSet);
            commandList.DrawIndexed(6);// Issue a Draw command for a single instance with 6 indices.
        }
        commandList.End();// End() must be called before commands can be submitted for execution.

        GraphicsDevice.SubmitCommands(commandList);
        GraphicsDevice.WaitForIdle();

        // Once commands have been submitted, the rendered image can be presented to the application window.
        GraphicsDevice.SwapBuffers();
    }

    private static void OnClosing()
    {
        pipeline.Dispose();
        foreach (Shader shader in shaders)
        {
            shader.Dispose();
        }

        commandList.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        GraphicsDevice.Dispose();
    }

    private static void OnFilesDrop(string[] files)
    {
        string rom = files[0];
        emulator.Open(rom);
    }

    private static void SaveScreenToFile()
    {
        DisposeCollectorResourceFactory rf = new(GraphicsDevice.ResourceFactory);

        Texture colorTargetTexture = GraphicsDevice.SwapchainFramebuffer.ColorTargets[0].Target;
        PixelFormat pixelFormat = colorTargetTexture.Format; // <- PixelFormat.B8_G8_R8_A8_UNorm, is it OK?

        Texture textureForRender = GraphicsDevice.SwapchainFramebuffer.ColorTargets[0].Target;

        FramebufferDescription framebufferDescription = new(null, textureForRender);
        _ = rf.CreateFramebuffer(framebufferDescription);

        Texture stage = rf.CreateTexture(
            TextureDescription.Texture2D(textureForRender.Width, textureForRender.Height, 1, 1, pixelFormat, TextureUsage.Staging)
        );

        commandList.Begin();
        commandList.CopyTexture(
            textureForRender, 0, 0, 0, 0, 0,
            stage, 0, 0, 0, 0, 0,
            stage.Width, stage.Height, 1, 1);
        commandList.End();

        GraphicsDevice.SubmitCommands(commandList);

        MappedResourceView<Rgba32> map = GraphicsDevice.Map<Rgba32>(stage, MapMode.Read);

        Image<Rgba32> image = new((int)stage.Width, (int)stage.Height);
        for (int y = 0; y < stage.Height; y++)
        {
            for (int x = 0; x < stage.Width; x++)
            {
                image[x, y] = new Rgba32(map[x, y].B, map[x, y].G, map[x, y].R, map[x, y].A);
            }
        }

        GraphicsDevice.Unmap(stage);
        rf.DisposeCollector.DisposeAll();

        image.SaveAsPng("Screenshot.png");
    }
}
