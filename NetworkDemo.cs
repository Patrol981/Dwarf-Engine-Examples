using Dwarf.Engine;
using Dwarf.Engine.Controllers;
using Dwarf.Engine.DataStructures;
using Dwarf.Engine.DataStructures.Enums;
using Dwarf.Engine.ECS;
using Dwarf.Engine.Globals;
using Dwarf.Engine.Scenes;
using Dwarf.Engine.Toolbox;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SocketIOClient;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DwarfDemo;
public class NetworkDemo {
  private Dwarf.Engine.Windowing.Window _window;
  private Scene _scene;
  private EngineClass _engine;
  private SocketIO _client = null;

  private Entity _myPlayerRef;
  public NetworkDemo() {
    _window = new Dwarf.Engine.Windowing.Window(GameWindowSettings.Default, WindowSettings.GetNativeWindowSettings());
    WindowGlobalState.SetWindow(_window);
    _window.CenterWindow();

    _scene = new DemoScene();
    _engine = new EngineClass(_window, _scene);
    // ConnectToServer();

    _engine.SetGUICallback(OnDrawGUI);
    _engine.SetRenderCallback(OnRender);
    _engine.SetUpdateCallback(OnUpdate);
    _engine.SetOnLoadCallback(ConnectToServer);

    // ConnectToServer();


    _engine.Run();

    GLFW.Terminate();
  }

  void OnRender() {

  }

  void OnDrawGUI() {

    if (ImGui.BeginMainMenuBar()) {
      if (ImGui.Button("Spawn Player")) {
        SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
        SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
        SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
        SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
        SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
        SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
      }

      if (ImGui.Button("Get Entities")) {
        // var items = _engine.GetEntities();
        var items = _engine.GetEntities<NetworkEntity>();
        Console.WriteLine(items);
      }
    }

    if (ImGui.Begin("Entities")) {
      for (int i = 0; i < _engine.Scene.Entities.Count; i++) {
        ImGui.Text(_engine.Scene.Entities[i].Name.ToString());
      }
    }



    if (ImGui.Begin("Network Data")) {
      ImGui.Text("ID");
      if (_myPlayerRef != null) {
        ImGui.Text(_myPlayerRef.EntityID.ToString());
      }

      if (SocketClient.Client == null) return;

      ImGui.Text("Attempts");
      ImGui.Text(SocketClient.Client.Attempts.ToString());

      ImGui.Text("Connected");
      ImGui.Text(SocketClient.Client.Connected.ToString());

      ImGui.Text("Disconnected");
      ImGui.Text(SocketClient.Client.Disconnected.ToString());

      ImGui.Text("ServerUri");
      ImGui.Text(SocketClient.Client.ServerUri.ToString());

      if (ImGui.Button("Join") && SocketClient.Client.Connected) {
        CreatePlayer();
      }
    }
  }

  void OnUpdate() {

    if (WindowGlobalState.GetKeyboardState().IsKeyPressed(Keys.Enter)) {
      SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
    }
    if (SocketClient.Client == null) return;

    var netEntities = _engine.GetEntities<NetworkEntity>();
    foreach (var e in netEntities) {
      if (!e.Spawned) {
        Entity.AddMeshToEmpty(
          e,
          "Resources/chr_knight/chr_knight.obj",
          "Shaders/vertexShader.vert",
          "Shaders/fragmentShader.frag"
        );
        e.Spawned = true;
      }
    }



    //SocketClient.Client.On("updatePos", response => {
    //Console.WriteLine(response);
    //HandlePositions(response);
    //});

    SocketClient.Client.On("spawn", response => {
      HandleSpawn(response);
    });

    if (_myPlayerRef == null) return;

    SocketClient.Client.On("updatePos", response => {
      // Console.WriteLine("Recieving pos");

      var strip = response.GetValue(0).GetRawText();
      var jObj = JsonObject.Parse(strip);
      Root root = JsonSerializer.Deserialize<Root>(jObj);

      if (root!.Players.Count <= 0) return;
      if (_myPlayerRef == null) return;

      var myID = _myPlayerRef.EntityID.ToString();
      root.Players.RemoveAll(x => x.Guid == myID);
      foreach (var player in root.Players) {
        var target = SocketClient.OtherPlayers
          .Where(x => x.EntityID.ToString() == player.Guid)
          .FirstOrDefault();
        if (target != null) {
          var vec3 = new Vector3(
            float.Parse(player.X, CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(player.Y, CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(player.Z, CultureInfo.InvariantCulture.NumberFormat)
          );
          var rot3 = new Vector3(
            float.Parse(player.RX, CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(player.RY, CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(player.RZ, CultureInfo.InvariantCulture.NumberFormat)
          );

          target.GetComponent<Transform>().Position = vec3;
          target.GetComponent<Transform>().Rotation = rot3;
        }
      }
    });



    PlayerExport netData = new(
      _myPlayerRef.EntityID.ToString(),
      _myPlayerRef.GetComponent<Transform>().Position.X,
      _myPlayerRef.GetComponent<Transform>().Position.Y,
      _myPlayerRef.GetComponent<Transform>().Position.Z,
      _myPlayerRef.GetComponent<Transform>().Rotation.X,
      _myPlayerRef.GetComponent<Transform>().Rotation.Y,
      _myPlayerRef.GetComponent<Transform>().Rotation.Z
    );

    SocketClient.Client.EmitAsync("updatePos", netData);
  }

  void HandlePositions(SocketIOResponse response) {
    if (_myPlayerRef == null) return;

    var strip = response.GetValue(0).GetRawText();
    var jObj = JsonObject.Parse(strip);
    Root root = JsonSerializer.Deserialize<Root>(jObj);

    //Console.WriteLine("Players Count");
    //Console.WriteLine(root!.Players.Count);

    if (root == null) return;

    var myID = _myPlayerRef.EntityID.ToString();
    root.Players.RemoveAll(x => x.Guid == myID);

    for (int i = 0; i < root.Players.Count; i++) {
      var target = _engine.GetEntityById(Guid.Parse(root.Players[i].Guid));
      if (target == null) continue;
      var pos = new Vector3(
        float.Parse(root.Players[i].X),
        float.Parse(root.Players[i].Y),
        float.Parse(root.Players[i].Z)
      );
      target.GetComponent<Transform>().Position = pos;
    }
  }

  void HandleSpawn(SocketIOResponse response) {
    //SpawnOtherPlayer(Guid.NewGuid().ToString(), new Vector3(_engine.Scene.Entities.Count, 12, 0));
    //return;

    var strip = response.GetValue(0).GetRawText();
    var jObj = JsonObject.Parse(strip);
    Root root = JsonSerializer.Deserialize<Root>(jObj);

    Console.WriteLine("Players Count");
    Console.WriteLine(root!.Players.Count);


    var myID = _myPlayerRef.EntityID.ToString();
    root.Players.RemoveAll(x => x.Guid == myID);

    if (root!.Players.Count <= 0) return;

    Console.WriteLine($"Players to Spawn: {root.Players.Count}");

    for (int i = 0; i < root.Players.Count; i++) {
      //double.TryParse(root.Players[i].X, out var x);
      //double.TryParse(root.Players[i].Y, out var y);
      //double.TryParse(root.Players[i].Z, out var z);

      SpawnEmptyPlayerInstance(
        root.Players[i].Guid,
        new Vector3(
          0,0,0
        )
      );
    }

    //foreach(var player in root.Players) {
    //SpawnOtherPlayer(player.Guid, new Vector3(player.X, player.Y, player.Z));
    //} 
  }

  void CreatePlayer() {
    var terrain = _engine.GetEntitiesByType(EntityType.Terrain).FirstOrDefault();

    var player = Entity.CreateWithCollision<NetworkEntity>(
      "my_player",
      new Vector3(0, 0, 0),
      "Resources/chr_knight/chr_knight.obj",
      "Shaders/vertexShader.vert",
      "Shaders/fragmentShader.frag",
      true,
      terrain!,
      CollisionType.BoundingBox
    );
    _engine.AddToScene(player);
    // CameraToolbox.RemoveCamera(CameraGlobalState.GetCameraEntity());
    var window = WindowGlobalState.GetWindow();
    var camera = CameraGlobalState.GetCameraEntity();
    CameraToolbox.CreateThirdPersonCamera(ref camera, ref window, player);
    player.AddComponent(new TransformController(10));

    _myPlayerRef = player;

    SocketClient.Client.EmitAsync("spawn", player.EntityID);
  }

  void SpawnOtherPlayer(string guid, Vector3 pos) {
    Console.WriteLine("ELO");

    var player = Entity.CreateWithMesh<NetworkEntity>(
      "other player",
      pos,
      "Resources/chr_knight/chr_knight.obj",
      "Shaders/vertexShader.vert",
      "Shaders/fragmentShader.frag"
    );
    player.EntityID = Guid.Parse(guid);
    _engine.AddToScene(player);
    SocketClient.OtherPlayers.Add(player);
  }

  void SpawnEmptyPlayerInstance(string guid, Vector3 pos) {
    Console.WriteLine("ELO");

    var player = Entity.CreateEmpty<NetworkEntity>("other player", pos);
    player.EntityID = Guid.Parse(guid);
    _engine.AddToScene(player);
    SocketClient.OtherPlayers.Add(player);
  }

  async void ConnectToServer() {
    SocketClient.Client = new SocketIO("http://localhost:3000");

    await SocketClient.Client.ConnectAsync();
  }
}


public static class SocketClient {
  public static SocketIO Client = null!;

  public static List<Entity> OtherPlayers = new();
}

public class Player {
  public string Guid { get; set; }
  public string X { get; set; }
  public string Y { get; set; }
  public string Z { get; set; }
  public string RX { get; set; }
  public string RY { get; set; }
  public string RZ { get; set; }
}

public class Root {
  public List<Player> Players { get; set; }
}

public class PlayerExport {
  public string Guid { get; set; }
  public float X { get; set; }
  public float Y { get; set; }
  public float Z { get; set; }
  public float RX { get; set; }
  public float RY { get; set; }
  public float RZ { get; set; }

  public PlayerExport(string id, float x, float y, float z, float rx, float ry, float rz) {
    Guid = id;
    X = x;
    Y = y;
    Z = z;
    RX = rx;
    RY = ry;
    RZ = rz;
  }
}