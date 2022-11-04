using Dwarf.Engine;
using Dwarf.Engine.Cameras;
using Dwarf.Engine.Controllers;
using Dwarf.Engine.DataStructures;
using Dwarf.Engine.ECS;
using Dwarf.Engine.Globals;
using Dwarf.Engine.Physics;
using Dwarf.Engine.Scenes;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Dwarf.Engine.Loaders;
using System;

namespace DwarfDemo;
public class ObjViewerDemo {
  private Dwarf.Engine.Windowing.Window _window;
  private Scene _scene;
  private EngineClass _engine;

  private string _path = "";
  private bool _autoRotate = true;
  public ObjViewerDemo() {
    _window = new Dwarf.Engine.Windowing.Window(GameWindowSettings.Default, WindowSettings.GetNativeWindowSettings());
    WindowGlobalState.SetWindow(_window);

    _scene = new ViewerScene();
    _engine = new EngineClass(_window, _scene);

    _engine.SetGUICallback(OnDrawGUI);
    _engine.SetRenderCallback(OnRender);
    _engine.SetUpdateCallback(OnUpdate);
    _engine.Run();

    GLFW.Terminate();
  }

  void OnRender() {

  }

  void OnDrawGUI() {
    if (ImGui.BeginMainMenuBar()) {
      if (ImGui.BeginMenu("Load Model")) {
        ImGui.InputText("Path", ref _path, 40);

        if(ImGui.Button("Load")) {
          Load();
        }
      }
    }

    if(ImGui.Begin("Loaded Models")) {
      var entities = _engine.Scene.Entities;
      for (int i = 0; i < entities.Count; i++) {
        ImGui.PushID(entities[i].Name);
        if (ImGui.TreeNodeEx(entities[i].Name)) {
          ImGui.Text($"Model {i} name: {entities[i].Name}");
          var color = entities[i].GetComponent<Material>().GetColor();
          var pos = entities[i].GetComponent<Transform>();
          var cnvVec = new System.Numerics.Vector3(color.X, color.Y, color.Z);
          var cnvPos = new System.Numerics.Vector3(pos.Position.X, pos.Position.Y, pos.Position.Z);
          var cnvRot = new System.Numerics.Vector3(pos.Rotation.X, pos.Rotation.Y, pos.Rotation.Z);
          ImGui.DragFloat3("Material Color", ref cnvVec, 0.01f);
          entities[i].GetComponent<Material>().SetColor(new Vector3(cnvVec.X, cnvVec.Y, cnvVec.Z));
          ImGui.DragFloat3("Position", ref cnvPos, 0.01f);
          entities[i].GetComponent<Transform>().Position = new Vector3(cnvPos.X, cnvPos.Y, cnvPos.Z);
          ImGui.DragFloat3("Rotation", ref cnvRot, 0.01f);
          entities[i].GetComponent<Transform>().Rotation = new Vector3(cnvRot.X, cnvRot.Y, cnvRot.Z);
        }
      }
    }
  }

  void OnUpdate() {
    var speed = 15 * WindowGlobalState.GetTime();
    for(int i=0; i<_engine.Scene.Entities.Count; i++) {
      if(_autoRotate) _engine.Scene.Entities[i].GetComponent<Transform>().Rotation.Y += (float)speed;
      if (_engine.Scene.Entities[i].GetComponent<Transform>().Rotation.Y > 360)
        _engine.Scene.Entities[i].GetComponent<Transform>().Rotation.Y = 0;
    }
  }

  void Load() {
    var type = _path.Split(".");
    var actualPath = _path.Split("/");
    var combine = "";
    for(int i=0; i<actualPath.Length-1; i++) {
      combine += actualPath[i];
      combine += "/";
    }
    combine = combine.Remove(combine.Length - 1);

    Console.WriteLine(type);

    var entity = new Entity();
    _engine.Scene.Entities.Add(entity);
    entity.AddComponent(new Transform(new Vector3(0, -1, -2)));
    entity.AddComponent(new Material(new Vector3(1f, 1f, 1f)));
    entity.AddComponent(
      type[1] == "fbx" ? 
      new FbxLoader().Load(combine):
      new ObjLoader().Load(combine)
    );
    entity.AddComponent(new MeshRenderer());
    entity.GetComponent<MeshRenderer>().Init("./Shaders/vertexShader.vert", "./Shaders/fragmentShader.frag");
    entity.Name = actualPath[actualPath.Length - 1];
  }
}
