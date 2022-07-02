namespace Monoboy.Desktop;

using System;

using Monoboy.Desktop.Data;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

public class VeldridWindow
{
    public string Title { get => Window.Title; set => Window.Title = value; }

    public Action<double> Update { get; internal set; }
    public Action<string[]> FileDrop { get; internal set; }

    private IWindow Window { get; set; }

    private GraphicsDevice GraphicsDevice { get; set; }

    private CommandList commandList;
    private DeviceBuffer vertexBuffer;
    private DeviceBuffer indexBuffer;
    private Shader[] shaders;
    private Pipeline pipeline;
    private ResourceSet resourceSet;
    private Texture renderBufferTexture;
    private TextureView textureView;
    private readonly GraphicsBackend preferredBackend;

    public VeldridWindow(GraphicsBackend preferredBackend, byte[] data)
    {
        this.preferredBackend = preferredBackend;

        const int windowScale = 4;

        // Window Init
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Size = new Vector2D<int>(Emulator.WindowWidth * windowScale, Emulator.WindowHeight * windowScale);
        windowOptions.API = preferredBackend.ToGraphicsAPI();
        windowOptions.ShouldSwapAutomatically = false;
        windowOptions.Title = "Monoboy";
        windowOptions.VSync = false;
        Window = Silk.NET.Windowing.Window.Create(windowOptions);

        // Events
        Window.Load += () => OnLoad(data);
        Window.Update += Update;
        Window.FileDrop += FileDrop;
        Window.Render += OnRender;
        Window.Closing += OnClosing;
        Window.Resize += OnResize;
    }

    private void OnLoad(byte[] data)
    {
        // GraphicsDevice Init
        GraphicsDeviceOptions graphicsOptions = new();
        graphicsOptions.PreferStandardClipSpaceYDirection = true;
        graphicsOptions.PreferDepthRangeZeroToOne = true;
        graphicsOptions.SyncToVerticalBlank = true;
        GraphicsDevice = Window.CreateGraphicsDevice(graphicsOptions, preferredBackend);

        // Window Icon Init
        Silk.NET.Core.RawImage icon = new(32, 32, data);
        Window.SetWindowIcon(ref icon);

        LoadInput();
    }



    private void LoadInput()
    {
        IInputContext input = Window.CreateInput();

        foreach (IKeyboard keyboard in input.Keyboards)
        {
            if (!keyboard.IsConnected)
            {
                continue;
            }

            Input.Keyboard = keyboard;
            break;
        }

        input.ConnectionChanged += (sender, args) =>
        {
            if (sender is IGamepad controller)
            {
                if (controller.IsConnected)
                {
                    Input.Gamepad = controller;
                }
                else
                {
                    Input.Gamepad = null;
                }
            }
        };

        foreach (IGamepad gamepad in input.Gamepads)
        {
            if (!gamepad.IsConnected)
            {
                continue;
            }

            Input.Gamepad = gamepad;
            break;
        }
    }

    private void OnResize(Vector2D<int> size)
    {
        size = new Vector2D<int>(Math.Max(size.X, Emulator.WindowWidth), Math.Max(size.Y, Emulator.WindowHeight));

        float scale = size.Y / Emulator.WindowHeight;
        float height = scale * Emulator.WindowHeight / size.Y;
        float width = scale * Emulator.WindowWidth / size.X;

        Vector4D<float>[] vertices =
        {
            new(-width, height,0.0f,0.0f),
            new(+width, height,1.0f,0.0f),
            new(-width, -height,0.0f,1.0f),
            new(+width, -height,1.0f,1.0f),
        };

        GraphicsDevice.UpdateBuffer(vertexBuffer, 0, vertices);
        GraphicsDevice.ResizeMainWindow((uint)size.X, (uint)size.Y);

        Window.Size = size;

        Window.DoRender();
    }

    private void GenerateResources()
    {
        ResourceFactory factory = GraphicsDevice.ResourceFactory;

        Vector4D<float>[] vertices =
        {
            new(-0.5f, +0.5f,0.0f,0.0f),
            new(+0.5f, +0.5f,1.0f,0.0f),
            new(-0.5f, -0.5f,0.0f,1.0f),
            new(+0.5f, -0.5f,1.0f,1.0f),
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

        ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, System.Text.Encoding.UTF8.GetBytes(BuiltinShader.VertCode), "main");
        ShaderDescription fragmentShaderDesc = new(ShaderStages.Fragment, System.Text.Encoding.UTF8.GetBytes(BuiltinShader.FragCode), "main");

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

    private void OnRender(double deltaTime)
    {
        GraphicsDevice.UpdateTexture(renderBufferTexture, app.Framebuffer, 0, 0, 0, Emulator.WindowWidth, Emulator.WindowHeight, 1, 0, 0);

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

    public void Run()
    {
        Window.Run();
    }

    private void OnClosing()
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

    public void Screenshot()
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
