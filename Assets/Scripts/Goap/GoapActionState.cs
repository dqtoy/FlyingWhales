using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapActionState {

    public GoapAction parentAction { get; private set; }
	public string name { get; private set; }
    public int duration { get; private set; } //if 0, go instantly to after effect
    public Log descriptionLog { get; private set; }
    public Action preEffect { get; private set; }
    public Action perTickEffect { get; private set; }
    public Action afterEffect { get; private set; }
    public string status { get; private set; }

    public bool hasPerTickEffect { get { return perTickEffect != null; } }
    private int _currentDuration;

    public GoapActionState(string name, GoapAction parentAction, Action preEffect, Action perTickEffect, Action afterEffect, int duration, string status) {
        this.name = name;
        this.preEffect = preEffect;
        this.perTickEffect = perTickEffect;
        this.afterEffect = afterEffect;
        this.parentAction = parentAction;
        this.duration = duration;
        this.status = status;   
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

        if(duration > 0) {
            _currentDuration = 0;
            StartPerTickEffect();
        } else {
            if(afterEffect != null) {
                afterEffect();
            }
            parentAction.ReturnToActorTheActionResult(status);
        }
    }
    private void StartPerTickEffect() {
        Messenger.AddListener(Signals.TICK_STARTED, PerTickEffect);
    }
    private void EndPerTickEffect() {
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
        if (afterEffect != null) {
            afterEffect();
        }
        parentAction.ReturnToActorTheActionResult(status);
    }
    private void PerTickEffect() {
        _currentDuration++;
        if (perTickEffect != null) {
            perTickEffect();
        }
        if(_currentDuration >= duration) {
            EndPerTickEffect();
        }
    }
}
