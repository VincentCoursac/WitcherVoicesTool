// Code sample taken from https://github.com/ImGuiNET/ImGui.NET/blob/master/src/ImGui.NET.SampleProgram/Program.cs and modified

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;

namespace WitcherVoicesTool.Application;

public class ApplicationWindow
{
    private static Sdl2Window? CurrentWindow;
    private static GraphicsDevice? GraphicsDevice;
    private static CommandList? CommandList;
    private static ImGuiController? Controller;

    private readonly Vector3 ClearColor = new Vector3(0.0f, 0.0f, 0.0f);
    
    public ApplicationWindow(int WindowWidth = 1280, int WindowHeight = 720, string WindowTitle = "Application",  bool bAllowResize = true)
    {
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, WindowWidth, WindowHeight, WindowState.Normal, WindowTitle),
            new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
            out CurrentWindow,
            out GraphicsDevice);
        
        CurrentWindow.Resizable = bAllowResize;

        if (bAllowResize)
        {
            CurrentWindow.Resized += () =>
            {
                GraphicsDevice?.MainSwapchain.Resize((uint)CurrentWindow.Width, (uint)CurrentWindow.Height);
                Controller?.WindowResized(CurrentWindow.Width, CurrentWindow.Height);
            };
        }
        
        CommandList = GraphicsDevice.ResourceFactory.CreateCommandList();
        Controller = new ImGuiController(GraphicsDevice, GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription, CurrentWindow.Width, CurrentWindow.Height);
    }

    public void Start()
    {
        var FrameWatch = Stopwatch.StartNew();
        float CurrentDeltaTime = 0f;

        while (CurrentWindow is { Exists: true })
        {
            CurrentDeltaTime = FrameWatch.ElapsedTicks / (float)Stopwatch.Frequency;
            FrameWatch.Restart();
            InputSnapshot Snapshot = CurrentWindow.PumpEvents();
      
            Controller?.Update(CurrentDeltaTime, Snapshot);

            ImGui.Begin("Test");
            ImGui.Text("Hello world!");
            ImGui.End();
            
            CommandList?.Begin();
            CommandList?.SetFramebuffer(GraphicsDevice?.MainSwapchain.Framebuffer);
            CommandList?.ClearColorTarget(0, new RgbaFloat(ClearColor.X, ClearColor.Y, ClearColor.Z, 1f));
            Controller?.Render(GraphicsDevice!, CommandList!);
            CommandList?.End();
            GraphicsDevice?.SubmitCommands(CommandList);
            GraphicsDevice?.SwapBuffers(GraphicsDevice.MainSwapchain);
        }
    }
}