using UnityEngine;
using System.Collections;
using System;

public class ExpirableModifier {

    private GameEvent _modifierGameEvent;
    private string _modifierReason;
    private DateTime _dueDate;
    private int _modifier;

    #region getters/setters
    public string modifierReason {
        get { return this._modifierReason; }
    }
    public DateTime dueDate {
        get { return this._dueDate; }
    }
    public int modifier {
        get { return this._modifier; }
    }
    #endregion
    public ExpirableModifier(GameEvent _modifierGameEvent, string _modifierReason, DateTime _dueDate, int _modifier) {
        this._modifierGameEvent = _modifierGameEvent;
        this._modifierReason = _modifierReason;
        this._dueDate = _dueDate;
        this._modifier = _modifier;
    }

    internal void SetDueDate(DateTime newDueDate) {
        this._dueDate = newDueDate;
    }

    internal void SetModifier(int newModifier) {
        this._modifier = newModifier;
    }
}
