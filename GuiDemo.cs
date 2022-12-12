using Dwarf.Engine;
using Dwarf.Engine.Globals;
using Dwarf.Engine.ImGuiNET.FileExplorer;
using Dwarf.Engine.Scenes;
using Dwarf.Engine.Toolbox.Gui;
using ImGuiNET;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DwarfDemo;
public class GuiDemo {
  private Dwarf.Engine.Windowing.Window _window;
  private Scene _scene;
  private EngineClass _engine;
  private ImGuiPreset _preset = new ImGuiPreset();

  private System.Numerics.Vector4 _color = System.Numerics.Vector4.Zero;

  public GuiDemo() {
    _window = new Dwarf.Engine.Windowing.Window(GameWindowSettings.Default, WindowSettings.GetNativeWindowSettings());
    WindowGlobalState.SetWindow(_window);
    _window.CenterWindow();

    _scene = new DemoScene();
    _engine = new EngineClass(_window, _scene);

    _engine.SetGUICallback(OnDrawGUI);
    _engine.SetRenderCallback(OnRender);
    _engine.SetUpdateCallback(OnUpdate);
    _engine.SetOnLoadCallback(OnLoad);
    _engine.Run();

    GLFW.Terminate();
  }

  void OnRender() {
    
  }

  void OnLoad() {
    _preset.Update();
    FileExplorer.SetupExplorer(DialogMode.Open);
    
  }

  void OnDrawGUI() {
    FileExplorer.GetFileExplorer()?.Update();

    ImGui.ShowDemoWindow();

    if(ImGui.BeginMainMenuBar()) {
      ImGui.Text("Gui Test");
      if(ImGui.Button("Click me")) { }
    }

    if(ImGui.Begin("Test")) {
      ImGui.ColorPicker4("Color", ref _color);
    }

    
  }

  void OnUpdate() {

  }
}
