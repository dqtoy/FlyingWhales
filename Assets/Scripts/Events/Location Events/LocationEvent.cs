using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocationEvent {
    public string name { get; protected set; }
    public int triggerTick { get; protected set; }
    public int triggerChance { get; protected set; }
    public Func<Settlement, bool> triggerCondition { get; protected set; }

    #region Virtuals
    public virtual void TriggerEvent(Settlement location) {
        Debug.Log(GameManager.Instance.TodayLogString() + "TRIGGER SETTLEMENT EVENT: " + name + " FOR " + location);
    }
    #endregion
}
