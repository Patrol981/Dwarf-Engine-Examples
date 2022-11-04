const express = require('express');
const app = express();
const http = require('http');
const server = http.createServer(app);
const { Server } = require("socket.io");
const io = new Server(server);

class Vector3 {
  constructor(x,y,z) {
    this.x = x;
    this.y = y;
    this.z = z;
  }

  static stringToVec3(str) {
    return new Vector3()
  }

  static vec3ToString(vec3) {
    return `(${vec3.x};${vec3.y};${vec3.z})`;
  }
}

class Player {
  constructor(guid, x, y, z, rx, ry, rz) {
    this.Guid = guid;
    this.X = x.toString();
    this.Y = y.toString();
    this.Z = z.toString();
    this.RX = rx.toString();
    this.RY = ry.toString();
    this.RZ = rz.toString();
  }
}

class PlayersData {
  constructor(players) {
    this.Players = players;
  }
}

const players = [];

app.get('/', (req, res) => {
  var str = JSON.stringify(players);

  res.json(
    str
  );
});

io.on('connection', (socket) => {
  // console.log('a user connected');

  socket.on('spawn', (data) => {
    // console.log(data);

    //if(data.)
    players.push(new Player(data, "0.0", "0.0", "0.0", "0.0", "0.0", "0.0"));
    socket.emit('spawn', new PlayersData(players));
    socket.broadcast.emit('spawn', new PlayersData(players));
  });

  socket.on('updatePos', (data) => {
    // console.log(data.X.toString());

    for(let i=0; i<players.length; i++) {
      if(players[i].Guid == data.Guid) {
        players[i].X = data.X.toString();
        players[i].Y = data.Y.toString();
        players[i].Z = data.Z.toString();
        players[i].RX = data.RX.toString();
        players[i].RY = data.RY.toString();
        players[i].RZ = data.RZ.toString();

        //console.log('updated player pos');
      }
    }

    socket.emit('updatePos', new PlayersData(players));
    socket.broadcast.emit('updatePos', new PlayersData(players));
  })
});

server.listen(3000, () => {
  console.log('listening on *:3000');
});