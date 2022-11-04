using Dwarf.Engine.Cameras;
using Dwarf.Engine.Controllers;
using Dwarf.Engine.DataStructures;
using Dwarf.Engine.ECS;
using Dwarf.Engine.Generators;
using Dwarf.Engine.Globals;
using Dwarf.Engine.Loaders;
using Dwarf.Engine.Physics;
using Dwarf.Engine.Primitives;
using Dwarf.Engine.Scenes;
using Dwarf.Engine.Toolbox;
using Dwarf.Engine.DataStructures.Enums;
using OpenTK.Mathematics;
using System;

namespace DwarfDemo;
public class DemoScene : Scene {
  public DemoScene() : base() {
    var window = WindowGlobalState.GetWindow();

    var camera = new Entity();
    
    var newTerrain = new Entity();
    Entities.Add(newTerrain);
    newTerrain.AddComponent(new Transform(new Vector3(-20, 0, -20)));
    newTerrain.AddComponent(new Material(new Vector3(1, 1, 1)));
    newTerrain.AddComponent(new MasterMesh());
    newTerrain.AddComponent(new MeshRenderer());
    var chunk = Chunk.SetupMesh(120);
    newTerrain.GetComponent<MasterMesh>().Meshes.Add(chunk);
    newTerrain.GetComponent<MeshRenderer>().Init("./Shaders/terrain.vert", "./Shaders/terrain.frag");
    newTerrain.Name = "chunk";
    newTerrain.Type = EntityType.Terrain;

    /*
    var objTest = Entity.CreateWithCollision(
      "chr_knight",
      new Vector3(0, 0, 0),
      "Resources/chr_knight/chr_knight.obj",
      "./Shaders/vertexShader.vert",
      "./Shaders/fragmentShader.frag",
      true,
      newTerrain,
      CollisionType.BoundingBox
    );
    Entities.Add(objTest);
    */

    /*
    var objTest = new Entity();
    Entities.Add(objTest);
    objTest.AddComponent(new Transform(new Vector3(0, 12, 0)));
    objTest.AddComponent(new Material(new Vector3(1f, 1f, 1f)));
    objTest.AddComponent(new ObjLoader().Load("Resources/chr_knight"));
    objTest.AddComponent(new MeshRenderer());
    objTest.GetComponent<MeshRenderer>().Init("./Shaders/vertexShader.vert", "./Shaders/fragmentShader.frag");
    objTest.AddComponent(new BoundingBox());
    objTest.GetComponent<BoundingBox>().Setup(objTest.GetComponent<MasterMesh>());
    objTest.AddComponent(new Rigidbody());
    objTest.Name = "chr knight";
    */

    /*
    var fbx = Entity.CreateWithCollision(
      "Yuna2",
      new Vector3(0,0,0),
      "Resources/Yuna/Yuna2.fbx",
      "Shaders/vertexShader.vert",
      "Shaders/fragmentShader.frag",
      true,
      newTerrain,
      CollisionType.BoundingBox
    );
    Entities.Add(fbx);
    */

    var chr_sword = Entity.CreateWithCollision<Entity>(
      "chr_sword",
      new Vector3(0, 64, 0),
      "Resources/chr_sword/chr_sword.obj",
      "Shaders/vertexShader.vert",
      "Shaders/fragmentShader.frag",
      true,
      newTerrain,
      CollisionType.BoundingBox
    );
    Entities.Add(chr_sword);

    /*
    var fbx = new Entity();
    Entities.Add(fbx);
    fbx.Name = "Yuna2";
    fbx.AddComponent(new Transform(new Vector3(5, 9, 0)));
    fbx.GetComponent<Transform>().Rotation = new Vector3(0, 0, 0);
    fbx.AddComponent(new Material(new Vector3(1, 1, 1)));
    fbx.AddComponent(new FbxLoader().Load($"Resources/{fbx.Name}"));
    fbx.AddComponent(new MeshRenderer());
    fbx.GetComponent<MeshRenderer>().Init("./Shaders/vertexShader.vert", "./Shaders/fragmentShader.frag");
    fbx.AddComponent(new BoundingBox());
    fbx.GetComponent<BoundingBox>().Setup(fbx.GetComponent<MasterMesh>());
    //fbx.GetComponent<Transform>().Rotation = new Vector3(-90, 180, 0);
    */

    var line = new Entity();
    Entities.Add(line);
    line.Name = "Line 3D";
    line.AddComponent(new Transform(new Vector3(0, 0, 0)));
    line.AddComponent(new Line3D());
    line.AddComponent(new Line3D().Load());
    line.AddComponent(new Material(new Vector3(120, 1, 1)));
    line.AddComponent(new MeshRenderer());
    line.GetComponent<MeshRenderer>().Init(Line3D.vertexCode, Line3D.fragmentCode, false);


    EntityGlobalState.ClearEntities();
    EntityGlobalState.SetEntities(Entities);

    //CameraToolbox.CreateThirdPersonCamera(ref camera, ref window, null!);
    //objTest.AddComponent(new TransformController(10));
    CameraToolbox.CreateFreeCamera(ref camera, ref window);
  }

  public override void RenderScene() {
    throw new NotImplementedException();
  }
}
