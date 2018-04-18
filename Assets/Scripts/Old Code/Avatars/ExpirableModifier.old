using UnityEngine;
using System.Collections;
using System;

public class ExpirableModifier {

    private GameEvent _modifierGameEvent;
    private string _modifierReason;
    private GameDate _dueDate;
//	private GameDate _expirationDate;
    private int _modifier;
	private string _summary;

    #region getters/setters
	public GameEvent modifierGameEvent {
		get { return this._modifierGameEvent; }
	}
    public string modifierReason {
        get { return this._modifierReason; }
    }
	public GameDate dueDate {
        get { return this._dueDate; }
    }
    public int modifier {
        get { return this._modifier; }
    }
	public string summary {
		get { return this._summary; }
	}
//	public GameDate expirationDate {
//		get { return this._expirationDate; }
//	}
    #endregion
	public ExpirableModifier(GameEvent _modifierGameEvent, string _modifierReason, GameDate _dueDate, int _modifier) {
        this._modifierGameEvent = _modifierGameEvent;
        this._modifierReason = _modifierReason;
        this._dueDate = _dueDate;
        this._modifier = _modifier;
		if(this._modifier < 0) {
			this._summary = this._modifier.ToString() + " " + this._modifierReason;
		} else if (this._modifier > 0) {
			this._summary = "+" + this._modifier.ToString() + " " + this._modifierReason;
        }
    }

	internal void SetDueDate(GameDate newDueDate) {
        this._dueDate = newDueDate;
    }

    internal void SetModifier(int newModifier) {
        this._modifier = newModifier;
    }
}
