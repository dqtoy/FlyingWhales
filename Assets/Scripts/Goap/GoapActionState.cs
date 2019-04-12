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
    public Func<Character, List<string>> shareIntelReaction { get; private set; }
    public string status { get; private set; }
    public bool shouldAddLogs { get; private set; }

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
        this.shouldAddLogs = true;
        CreateLog();
    }

    public void SetIntelReaction(Func<Character, List<string>> intelReaction) {
        shareIntelReaction = intelReaction;
    }

    #region Logs
    private void CreateLog() {
        descriptionLog = new Log(GameManager.Instance.Today(), "GoapAction", parentAction.GetType().ToString(), name.ToLower() + "_description");
        AddLogFiller(parentAction.actor, parentAction.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if(parentAction.poiTarget is Character) {
            AddLogFiller(parentAction.poiTarget as Character, parentAction.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }
    public void OverrideDescriptionLog(Log log) {
        descriptionLog = log;
    }
    public void AddLogFiller(object obj, string value, LOG_IDENTIFIER identifier) {
        descriptionLog.AddToFillers(obj, value, identifier);
    }
    public void SetShouldAddLogs(bool state) {
        shouldAddLogs = state;
    }
    #endregion


    public void Execute() {
        preEffect?.Invoke();
        parentAction.SetExecutionDate(GameManager.Instance.Today());

        if(duration > 0) {
            _currentDuration = 0;
            StartPerTickEffect();
        } else {
            EndPerTickEffect();
        }
    }
    private void StartPerTickEffect() {
        Messenger.AddListener(Signals.TICK_STARTED, PerTickEffect);
    }
    public void EndPerTickEffect() {
        //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
        if (afterEffect != null) {
            afterEffect();
        }
        if (parentAction.shouldAddLogs && this.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
            descriptionLog.SetDate(GameManager.Instance.Today());
            descriptionLog.AddLogToInvolvedObjects();
        }
        parentAction.ReturnToActorTheActionResult(status);
    }
    public void StopPerTickEffect() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
        }
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
