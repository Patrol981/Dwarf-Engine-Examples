using Dwarf.Engine;
using Dwarf.Engine.Cameras;
using Dwarf.Engine.Controllers;
using Dwarf.Engine.DataStructures;
using Dwarf.Engine.ECS;
using Dwarf.Engine.Globals;
using Dwarf.Engine.Physics;
using Dwarf.Engine.Primitives;
using Dwarf.Engine.Scenes;
using Dwarf.Engine.Toolbox;
using Dwarf.Engine.Windowing;

using ImGuiNET;

using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using System;

namespace DwarfDemo;
public class Demo2D {
  private Dwarf.Engine.Windowing.Window _window;
  private Scene _scene;
  private EngineClass _engine;

  public Demo2D() {
    _window = new Dwarf.Engine.Windowing.Window(GameWindowSettings.Default, WindowSettings.GetNativeWindowSettings());
    WindowGlobalState.SetWindow(_window);
    _window.CenterWindow();

    _scene = new DemoScene2D();
    _engine = new EngineClass(_window, _scene);

    _engine.SetGUICallback(OnDrawGUI);
    _engine.SetRenderCallback(OnRender);
    _engine.SetUpdateCallback(OnUpdate);
    _engine.Run();

    GLFW.Terminate();
  }

  private void OnDrawGUI() {
    if (ImGui.Begin("Sprite Data")) {
      var sprite = _engine.GetEntities().Where(e => e.GetComponent<Sprite>() != null).First();
      var bounds = sprite.GetComponent<BoundingBox2D>();
      ImGui.Text(sprite.Name);
      ImGui.Text(bounds.Size.ToString());
      ImGui.Text(bounds.Center.ToString());
    }

    if (ImGui.BeginMainMenuBar()) {
      if (ImGui.Button("FreeCamera")) {
        var camera = CameraGlobalState.GetCameraEntity();
        var window = WindowGlobalState.GetWindow();
        CameraToolbox.RemoveCamera<ThirdPersonCamera>(ref camera);
        CameraToolbox.RemoveCamera<StaticCamera>(ref camera);
        CameraToolbox.CreateFreeCamera(ref camera, ref window);

        for (int i = 0; i < _engine.GetEntities().Count; i++) {
          var entity = _engine.GetEntities()[i];
          entity.RemoveComponent<TransformController>();
        }
      }
    }

    if (ImGui.BeginMenu("Debug Data")) {
      var entities = EntityGlobalState.GetEntities();
      for (int i = 0; i < entities.Count; i++) {
        ImGui.PushID(entities[i].Name);
        if (ImGui.TreeNodeEx(entities[i].Name)) {
          ImGui.Text($"Model {i} name: {entities[i].Name}");
          var color = entities[i].GetComponent<Material>().GetColor();
          var pos = entities[i].GetComponent<Transform>();
          var cnvVec = new System.Numerics.Vector3(color.X, color.Y, color.Z);
          var cnvPos = new System.Numerics.Vector3(pos.Position.X, pos.Position.Y, pos.Position.Z);
          var cnvRot = new System.Numerics.Vector3(pos.Rotation.X, pos.Rotation.Y, pos.Rotation.Z);
          var cnvSca = new System.Numerics.Vector3(pos.Scale.X, pos.Scale.Y, pos.Scale.Z);
          ImGui.DragFloat3("Material Color", ref cnvVec, 0.01f);
          entities[i].GetComponent<Material>().SetColor(new Vector3(cnvVec.X, cnvVec.Y, cnvVec.Z));
          ImGui.DragFloat3("Position", ref cnvPos, 0.01f);
          entities[i].GetComponent<Transform>().Position = new Vector3(cnvPos.X, cnvPos.Y, cnvPos.Z);
          ImGui.DragFloat3("Rotation", ref cnvRot, 0.01f);
          entities[i].GetComponent<Transform>().Rotation = new Vector3(cnvRot.X, cnvRot.Y, cnvRot.Z);
          ImGui.DragFloat3("Scale", ref cnvSca, 0.01f);
          entities[i].GetComponent<Transform>().Scale = new Vector3(cnvSca.X, cnvSca.Y, cnvSca.Z);
          if (ImGui.Button("Set Visible")) {
            entities[i].GetComponent<MasterMesh>().Render = !entities[i].GetComponent<MasterMesh>().Render;
          }
        }
      }
    }
  }
  private void OnRender() { }
  private void OnUpdate() { }
}
