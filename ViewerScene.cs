using Dwarf.Engine.Cameras;
using Dwarf.Engine.Controllers;
using Dwarf.Engine.DataStructures;
using Dwarf.Engine.ECS;
using Dwarf.Engine.Generators;
using Dwarf.Engine.Globals;
using Dwarf.Engine.Loaders;
using Dwarf.Engine.Physics;
using Dwarf.Engine.Scenes;
using OpenTK.Mathematics;

namespace DwarfDemo;
public class ViewerScene : Scene {
  public ViewerScene() : base() {
    var window = WindowGlobalState.GetWindow();

    var camera = new Entity();
    camera.AddComponent(new Transform(new Vector3(0, 0, 0)));
    camera.AddComponent(new StaticCamera(window.Size.X / (float)window.Size.Y));
    CameraGlobalState.SetCameraEntity(camera);
    CameraGlobalState.SetCamera(camera.GetComponent<StaticCamera>());

    EntityGlobalState.ClearEntities();
    EntityGlobalState.SetEntities(Entities);
  }

  public override void RenderScene() {
    throw new NotImplementedException();
  }
}
