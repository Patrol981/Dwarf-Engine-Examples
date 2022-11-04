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
public class Demo {
  private Dwarf.Engine.Windowing.Window _window;
  private Scene _scene;
  private EngineClass _engine;

  private System.Numerics.Vector3 _lineVec = new(0,0,1);
  private System.Numerics.Vector3 _lineColor = new(1, 1, 1);

  private int _entityIndex = 0;
  public Demo() {
    _window = new Dwarf.Engine.Windowing.Window(GameWindowSettings.Default, WindowSettings.GetNativeWindowSettings());
    WindowGlobalState.SetWindow(_window);
    _window.CenterWindow();

    _scene = new DemoScene();
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
      ImGui.Text($"FPS: {FPSState.GetFrames()}");

      if(ImGui.Button("FreeCamera")) {
        var camera = CameraGlobalState.GetCameraEntity();
        var window = WindowGlobalState.GetWindow();
        CameraToolbox.RemoveCamera<ThirdPersonCamera>(ref camera);
        CameraToolbox.RemoveCamera<StaticCamera>(ref camera);
        CameraToolbox.CreateFreeCamera(ref camera, ref window);

        for(int i=0; i<_engine.GetEntities().Count; i++) {
          var entity = _engine.GetEntities()[i];
          entity.RemoveComponent<TransformController>();
        }
      }

      if (ImGui.BeginMenu("Debug Data")) {
        var entities = EntityGlobalState.GetEntities();
        for (int i = 0; i < entities.Count; i++) {
          ImGui.PushID(entities[i].Name);
          if (ImGui.TreeNodeEx(entities[i].Name)) {
            ImGui.Text($"Model {i} name: {entities[i].Name}");
            ImGui.Text($"Meshes: {entities[i].GetComponent<MasterMesh>().Meshes.Count}");
            //for(int x=0; x< entities[i].GetComponent<MasterMesh>().Meshes.Count; x++) {
            //  ImGui.Text(entities[i].GetComponent<MasterMesh>().Meshes[x].Name);
            //}
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
            if(ImGui.Button("Set Visible")) {
              entities[i].GetComponent<MasterMesh>().Render = !entities[i].GetComponent<MasterMesh>().Render;
            }
            if (ImGui.Button("Switch Controller")) {
              for (int x = 0; x < _engine.Scene.Entities.Count; x++) {
                var target = _engine.Scene.Entities[x];
                if (target.GetComponent<TransformController>() != null) {
                  target.RemoveComponent<TransformController>();
                  break;
                }
              }
              var camera = CameraGlobalState.GetCameraEntity();
              var window = WindowGlobalState.GetWindow();
              CameraToolbox.RemoveCamera<FreeCamera>(ref camera);
              CameraToolbox.RemoveCamera<StaticCamera>(ref camera);
              CameraToolbox.RemoveCamera<ThirdPersonCamera>(ref camera);
              CameraToolbox.CreateThirdPersonCamera(ref camera, ref window, entities[i]);
              var terrain = _engine.GetEntitiesByType(EntityType.Terrain).FirstOrDefault();
              if (terrain == null) return;
              entities[i].AddComponent(new TransformController(10, terrain.GetComponent<MasterMesh>()));

              WindowGlobalState.SetCursorVisible(false);
            }
          }
        }
      }
    }

    if (ImGui.Begin("Camera Test")) {
      if (EntityGlobalState.GetEntities().Count == 0) return;
      var cameraTransform = CameraGlobalState.GetCameraEntity().GetComponent<Transform>();
      var camera = CameraGlobalState.GetCameraEntity().GetComponent<ThirdPersonCamera>();

      ImGui.Text("Camera");
      ImGui.Text($"X:{cameraTransform.Position.X} Y:{cameraTransform.Position.Y} Z:{cameraTransform.Position.Z}");
      ImGui.Text($"X:{cameraTransform.Rotation.X} Y:{cameraTransform.Rotation.Y} Z:{cameraTransform.Rotation.Z}");
    }

    if(ImGui.Begin("Raycaster")) {
      var camera = CameraGlobalState.GetCameraEntity();

      _engine.Raycaster.Update();
      ImGui.Text("Ray");
      ImGui.Text(_engine.Raycaster.Ray.ToString());
      ImGui.Text("World Point");
      ImGui.Text(_engine.Raycaster.WorldPoint.ToString());

      var entities = _engine.GetEntities();
      var target = entities[4];
      var target2 = entities[2];

      ImGui.Text("World Model");

      var camPos = camera.GetComponent<Transform>().Position;
      camPos.Y -= 1;
      camPos.X -= 1;
      target.GetComponent<Line3D>().SetLine(
        camPos,
        _engine.Raycaster.WorldPoint
      );

      //target2.GetComponent<Transform>().Position = _engine.Raycaster.WorldPoint;
    }

    if(ImGui.Begin("Entities")) {
      for(int i=0; i<_engine.Scene.Entities.Count; i++) {
        ImGui.Text(_engine.Scene.Entities[i].GetComponent<Transform>().Position.ToString());
      }
    }

    if(ImGui.Begin("Targeted Enitiy")) {
      ImGui.DragInt("Target Index", ref _entityIndex, 0.01f);

      var count = _engine.GetEntitiesByType(EntityType.Entity).Count;

      if (_entityIndex >= count) _entityIndex = count - 1 ;
      if (_entityIndex < 0) _entityIndex = 0;

      var target = _engine.GetEntitiesByType(EntityType.Entity)[_entityIndex];
      if (target == null) return;

      ImGui.Text($"Name: {target.Name}");
      ImGui.Text("Bounding Box");
      var bbTarget = target.GetComponent<BoundingBox>();
      if (bbTarget == null) return;
      ImGui.Text(new Vector3(bbTarget.MinX, bbTarget.MinY, bbTarget.MinZ).ToString());
      ImGui.Text(new Vector3(bbTarget.MaxX, bbTarget.MaxY, bbTarget.MaxZ).ToString());
      ImGui.Text(bbTarget.WorldModel.ToString());
      ImGui.Text(bbTarget.Center.ToString());
      ImGui.Text(bbTarget.WorldModel.ExtractRotation().ToString());
    }

    if(ImGui.Begin("Intersection Test")) {
      var bbs = _engine.GetEntitiesByType(EntityType.Entity).Where(e => e.GetComponent<BoundingBox>() != null);
      var myEntity = _engine.GetEntitiesByType(EntityType.Entity).FirstOrDefault();
      if (myEntity == null) return;
      var bList = bbs.ToList();
      bList.RemoveAll(x => x.Name == myEntity.Name);
      var intersections = BoundingBox.Intersect(myEntity.GetComponent<BoundingBox>(), bList);
      for(int i=0; i< intersections.Count; i++) {
        ImGui.Text(intersections[i].Name);
      }
      
    }

    if(ImGui.Begin("Terrain Test")) {

      ImGui.Text($"Terrain X: {TerrainDebug.terrainX}");
      ImGui.Text($"Terrain Z: {TerrainDebug.terrainZ}");
      ImGui.Text($"Grid Square Size: {TerrainDebug.gridSquareSize}");
      ImGui.Text($"GridX: {TerrainDebug.gridX}");
      ImGui.Text($"GridZ: {TerrainDebug.gridZ}");
      ImGui.Text($"X: {TerrainDebug.X}");
      ImGui.Text($"Z: {TerrainDebug.Z}");
      ImGui.Text($"R: {TerrainDebug.R}");
    }

    if(ImGui.Begin("Line Test")) {
      var entities = _engine.GetEntities();
      var target = entities[3];

      ImGui.Text(target.Name);
      if (target.GetComponent<Line3D>() == null) return;

      // var cnvPos = new System.Numerics.Vector3(_lineVec.X, _lineVec.Y, _lineVec.Z);
      ImGui.DragFloat3("Position", ref _lineVec, 0.01f);
      ImGui.DragFloat3("Color", ref _lineColor, 0.01f);

      //target.GetComponent<Line3D>().SetLine(new Vector3(0, 0, 0), new Vector3(_lineVec.X, _lineVec.Y, _lineVec.Z));
      target.GetComponent<Material>().SetColor(new Vector3(_lineColor.X, _lineColor.Y, _lineColor.Z));

      var floats = target.GetComponent<Line3D>().GetFloats();

      ImGui.Text(new Vector3(floats[0], floats[1], floats[2]).ToString());
      ImGui.Text(new Vector3(floats[3], floats[4], floats[5]).ToString());
      ImGui.Text(target.GetComponent<Material>().GetColor().ToString());
    }

    if (ImGui.Begin("Boundings")) {
      var target = _engine.Scene.Entities[1];

      ImGui.Text($"Target: {target.Name}");

      ImGui.Text($"Meshes: {target.GetComponent<MasterMesh>().Meshes.Count}");

      ImGui.Text("Size");
      ImGui.Text(target.GetComponent<BoundingBox>().Size.ToString());
      ImGui.Text("Center");
      ImGui.Text(target.GetComponent<BoundingBox>().Center.ToString());
      ImGui.Text("Transfrom");
      ImGui.Text(target.GetComponent<BoundingBox>().Tranform.ToString());
      ImGui.Text("Model");
      ImGui.Text(target.GetComponent<BoundingBox>().Model.ToString());
      ImGui.Text("WorldModel");
      ImGui.Text(target.GetComponent<BoundingBox>().WorldModel.ToString());

    }
  }

  void OnUpdate() {

  }
}
