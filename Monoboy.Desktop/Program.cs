namespace Monoboy.Desktop;

using System;
using System.Numerics;
using System.Text;
using System.Threading;

using Monoboy.Desktop.Data;

using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;

using Veldrid;
using Veldrid.SPIRV;

public class Program
{
    private static IWindow window;
    private static Emulator emulator;
    private const GraphicsBackend PreferredBackend = GraphicsBackend.Vulkan;

    private static GraphicsDevice graphicsDevice;
    private static CommandList commandList;
    private static DeviceBuffer vertexBuffer;
    private static DeviceBuffer indexBuffer;
    private static Shader[] shaders;
    private static Pipeline pipeline;

    private const string VertexCode = @"
    #version 450
    layout(location = 0) in vec2 Position;
    void main()
    {
        gl_Position = vec4(Position, 0, 1);
    }";

    private const string FragmentCode = @"
    #version 450
    layout(location = 0) out vec4 fsout_Color;
    void main()
    {
        fsout_Color = vec4(1.0f, 0.0f, 0.0f, 1.0f);
    }";

    public static void Main()
    {

        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
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

        window.Run();
    }

    private static void OnLoad()
    {
        RawImage icon = new(32, 32, Icon.Data);
        window!.SetWindowIcon(ref icon);

        GraphicsDeviceOptions options = new()
        {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
            SyncToVerticalBlank = true,
        };
        graphicsDevice = window.CreateGraphicsDevice(options, PreferredBackend);

        NewMethod();

        emulator = new();
        emulator.Open("./Roms/Tetris.gb");
    }

    private static void NewMethod()
    {
        ResourceFactory factory = graphicsDevice.ResourceFactory;

        Vector2[] quadVertices =
        {
            new(0, .5f),
            new(.5f, -.5f),
            new(-.5f, -.5f)
        };

        BufferDescription vbDescription = new(3 * 16, BufferUsage.VertexBuffer);
        vertexBuffer = factory.CreateBuffer(vbDescription);
        graphicsDevice.UpdateBuffer(vertexBuffer, 0, quadVertices);

        ushort[] quadIndices = { 0, 1, 2 };
        BufferDescription ibDescription = new(3 * sizeof(ushort), BufferUsage.IndexBuffer);
        indexBuffer = factory.CreateBuffer(ibDescription);
        graphicsDevice.UpdateBuffer(indexBuffer, 0, quadIndices);

        VertexLayoutDescription vertexLayout = new(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );

        ShaderDescription vertexShaderDesc = new(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main");
        ShaderDescription fragmentShaderDesc = new(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main");

        shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

        // Create pipeline
        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new(true, true, ComparisonKind.LessEqual),
            RasterizerState = new(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
            PrimitiveTopology = PrimitiveTopology.TriangleStrip,
            ResourceLayouts = Array.Empty<ResourceLayout>(),
            ShaderSet = new(new[] { vertexLayout }, shaders),
            Outputs = graphicsDevice.SwapchainFramebuffer.OutputDescription
        };

        pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        commandList = factory.CreateCommandList();
    }

    private static void OnUpdate(double deltaTime)
    {
        Console.WriteLine(1 / deltaTime);

        emulator.StepFrame();
    }

    private static void OnRender(double deltaTime)
    {
        commandList.Begin();// Begin() must be called before commands can be issued.
        {
            // We want to render directly to the output window.
            commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

            // Set all relevant state to draw our quad.
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
            commandList.SetPipeline(pipeline);
            // Issue a Draw command for a single instance with 4 indices.
            commandList.DrawIndexed(3, 1, 0, 0, 0);
        }
        commandList.End();// End() must be called before commands can be submitted for execution.

        graphicsDevice.SubmitCommands(commandList);

        // Once commands have been submitted, the rendered image can be presented to the application window.
        graphicsDevice.SwapBuffers();
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
        graphicsDevice.Dispose();
    }
}
