using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocationEvent {
    public string name { get; protected set; }
    public int triggerTick { get; protected set; }
    public int triggerChance { get; protected set; }
    public Func<Area, bool> triggerCondition { get; protected set; }

    #region Virtuals
    public virtual void TriggerEvent(Area location) {
        Debug.Log("TRIGGER SETTLEMENT EVENT: " + name + " FOR " + location);
    }
    #endregion
}
