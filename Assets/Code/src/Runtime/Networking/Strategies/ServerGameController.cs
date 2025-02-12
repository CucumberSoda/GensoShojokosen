﻿using HouraiTeahouse.FantasyCrescendo.Matches;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Networking {

public abstract class ServerGameController : MatchController, IDisposable {

  public INetworkServer NetworkServer { get; }

  protected ServerGameController(INetworkServer server, MatchConfig config) : base(config) {
    NetworkServer = Argument.NotNull(server);
    NetworkServer.PlayerRemoved += OnPlayerDisconnected;
  }

  public override void Update() => NetworkServer.Update();

  public override void Dispose() {
    NetworkServer.PlayerRemoved -= OnPlayerDisconnected;
    base.Dispose();
  }

  void OnPlayerDisconnected(int playerId) {
    DestroyPlayer(playerId);
    if (NetworkServer.Clients.Count <= 1) {
      var matchManager = MatchManager.Instance;
      if (matchManager != null) {
        matchManager.EndMatch(new MatchResult {
          Resolution = MatchResolution.NoContest
        });
      }
    }
  }

  void DestroyPlayer(int playerId) {
    if (playerId < 0 || playerId >= CurrentState.PlayerCount) return;
    // TODO(james7132): Disable player properly
    CurrentState[playerId].Stocks = 0;
  }

}

}