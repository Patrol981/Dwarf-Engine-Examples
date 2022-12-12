using Dwarf.Engine.DataStructures.Enums;
using Dwarf.Engine.DataStructures;
using Dwarf.Engine.ECS;
using Dwarf.Engine.Generators;
using Dwarf.Engine.Globals;
using Dwarf.Engine.Primitives;
using Dwarf.Engine.Toolbox;
using Dwarf.Engine.Scenes;
using OpenTK.Mathematics;
using Dwarf.Engine.Physics;

namespace DwarfDemo;
public class DemoScene2D : Scene {
  public DemoScene2D() : base() {
    var window = WindowGlobalState.GetWindow();

    var camera = new Entity();

    var chr_sword = Entity.CreateMeshWithCollision<Entity>(
      "chr_sword",
      new Vector3(-2, 0, 0),
      "Resources/chr_sword/chr_sword.obj",
      "Shaders/vertexShader.vert",
      "Shaders/fragmentShader.frag",
      true,
      null!,
      CollisionType.BoundingBox
    );
    Entities.Add(chr_sword);

    var chr_sword2 = Entity.CreateMeshWithCollision<Entity>(
      "chr_sword",
      new Vector3(2, 0, 0),
      "Resources/chr_sword/chr_sword.obj",
      "Shaders/vertexShader.vert",
      "Shaders/fragmentShader.frag",
      true,
      null!,
      CollisionType.BoundingBox
    );
    Entities.Add(chr_sword2);

    var sprite = Entity.CreateSpriteWithCollision<Entity>(
      "sprite",
      new Vector3(-0.25f, 0, 0),
      "Resources/3.png",
      "Shaders/Sprite.vert",
      "Shaders/Sprite.frag",
      true
    );
    Entities.Add(sprite);

    EntityGlobalState.ClearEntities();
    EntityGlobalState.SetEntities(Entities);

    CameraToolbox.CreateOrthographicCamera(ref camera, ref window);
    //CameraToolbox.CreateStaticCamera(ref camera, ref window);
  }

  public override void RenderScene() {
    throw new NotImplementedException();
  }
}
