using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapActionState {

    public GoapAction parentAction { get; private set; }
	public string name { get; private set; }
    public Log descriptionLog { get; private set; }
    public Action preEffect { get; private set; }
    public Action perTickEffect { get; private set; }
    public Action afterEffect { get; private set; }

    public bool hasPerTickEffect { get { return perTickEffect != null; } }

    public GoapActionState(string name, GoapAction parentAction, Action preEffect, Action perTickEffect, Action afterEffect) {
        this.name = name;
        this.preEffect = preEffect;
        this.perTickEffect = perTickEffect;
        this.afterEffect = afterEffect;
        this.parentAction = parentAction;
        CreateLog();
    }

    private void CreateLog() {
        descriptionLog = new Log(GameManager.Instance.Today(), "GoapAction", parentAction.GetType().ToString(), name.ToLower() + "_description");
        AddLogFiller(parentAction.actor, parentAction.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    }

    public void AddLogFiller(object obj, string value, LOG_IDENTIFIER identifier) {
        descriptionLog.AddToFillers(obj, value, identifier);
    }

    public void Execute() {
        preEffect?.Invoke();
        descriptionLog.AddLogToInvolvedObjects();
    }
}
