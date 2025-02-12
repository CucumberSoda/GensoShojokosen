﻿using HouraiTeahouse.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace HouraiTeahouse.FantasyCrescendo {

public abstract class RegisteredBehaviour<T, TID> : MonoBehaviour, IEntity where T : RegisteredBehaviour<T, TID> {

  [ReadOnly] public TID Id;
  uint IEntity.Id => Convert.ToUInt32(Id);

  /// <summary>
  /// Awake is called when the script instance is being loaded.
  /// </summary>
  protected virtual void Awake() => Registry.Get<T>().Add((T)this);

  /// <summary>
  /// This function is called when the MonoBehaviour will be destroyed.
  /// </summary>
  protected virtual void OnDestroy() => Registry.Get<T>().Remove((T)this);

}

}