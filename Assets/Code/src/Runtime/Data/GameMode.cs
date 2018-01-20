﻿using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo {

public abstract class GameMode : GameDataBase {

  public const uint GlobalMaxPlayers = 4;

  [Range(1, GlobalMaxPlayers)] public uint MinPlayers = 1;
  [Range(1, GlobalMaxPlayers)] public uint MaxPlayers = GlobalMaxPlayers;

  protected abstract Task RunGame(MatchConfig config, bool loadStage = true);
  public virtual bool IsValidConfig(MatchConfig config) {
    var players = config.PlayerConfigs?.Length; 
    return players >= MinPlayers && players <= MaxPlayers;
  }

  public async Task Execute(MatchConfig config, bool loadStage = true) {
    if (!IsValidConfig(config)) {
      throw new ArgumentException("Attempted to start game with invalid config");
    }
    await RunGame(config, loadStage);
  }
}

}
