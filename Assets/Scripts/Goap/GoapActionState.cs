using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapActionState {

    public GoapAction parentAction { get; private set; }
	public string name { get; private set; }
    public int duration { get; private set; } //if 0, go instantly to after effect, if -1, endless (can only be ended manually)
    public Log descriptionLog { get; private set; } //Always set/create description logs on Pre effect because description logs are used in Memories and Memories are stored on start of the GoapActionState
    public Action preEffect { get; private set; }
    public Action perTickEffect { get; private set; }
    public Action afterEffect { get; private set; }
    public Func<Character, Intel, SHARE_INTEL_STATUS, List<string>> shareIntelReaction { get; private set; }
    public string status { get; private set; }
    public bool shouldAddLogs { get; private set; }
    public bool isDone { get; private set; }
    public string animationName { get; private set; } //specific animation per action state

    public bool hasPerTickEffect { get { return perTickEffect != null; } }
    public int currentDuration { get; private set; }

    public List<ActionLog> arrangedLogs { get; protected set; }

    public GoapActionState(string name, GoapAction parentAction, Action preEffect, Action perTickEffect, Action afterEffect, int duration, string status) {
        this.name = name;
        this.preEffect = preEffect;
        this.perTickEffect = perTickEffect;
        this.afterEffect = afterEffect;
        this.parentAction = parentAction;
        this.duration = duration;
        this.status = status;
        this.shouldAddLogs = true;
        this.isDone = false;
        this.arrangedLogs = new List<ActionLog>();
        CreateLog();
    }

    public void SetIntelReaction(Func<Character, Intel, SHARE_INTEL_STATUS, List<string>> intelReaction) {
        shareIntelReaction = intelReaction;
    }

    #region Duration
    public void OverrideDuration(int newDuration) {
        this.duration = newDuration;
    }
    #endregion

    #region Logs
    private void CreateLog() {
        descriptionLog = new Log(GameManager.Instance.Today(), "GoapAction", parentAction.GetType().ToString(), name.ToLower() + "_description", parentAction);
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
    public void AddLogFillers(List<LogFiller> fillers, bool replaceExisting = true) {
        descriptionLog.AddToFillers(fillers, replaceExisting);
    }
    public void SetShouldAddLogs(bool state) {
        shouldAddLogs = state;
    }
    public void AddArrangedLog(string priorityID, Log log, System.Action notifAction) {
        int index = parentAction.GetArrangedLogPriorityIndex(priorityID);
        if(index == -1 || arrangedLogs.Count <= index) {
            arrangedLogs.Add(new ActionLog() { log = log, notifAction = notifAction });
        } else {
            arrangedLogs.Insert(index, new ActionLog() { log = log, notifAction = notifAction });
        }
    }
    #endregion


    public void Execute() {
        preEffect?.Invoke();
        parentAction.SetExecutionDate(GameManager.Instance.Today());

        if(duration > 0) {
            currentDuration = 0;
            StartPerTickEffect();
        } else if (duration != -1){
            EndPerTickEffect();
        }
    }
    private void StartPerTickEffect() {
        Messenger.AddListener(Signals.TICK_STARTED, PerTickEffect);
    }
    public void EndPerTickEffect(bool shouldDoAfterEffect = true) {
        //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
        //if (isDone) {
        //    return;
        //}
        //isDone = true;
        if (shouldDoAfterEffect) {
            if (afterEffect != null) {
                afterEffect();
            }
            if (parentAction.shouldAddLogs && this.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
                AddArrangedLog("description", descriptionLog, null);
                for (int i = 0; i < arrangedLogs.Count; i++) {
                    arrangedLogs[i].log.SetDate(GameManager.Instance.Today());
                    arrangedLogs[i].log.AddLogToInvolvedObjects();
                }
                //descriptionLog.SetDate(GameManager.Instance.Today());
                //descriptionLog.AddLogToInvolvedObjects();
            }
        } else {
            parentAction.SetShowIntelNotification(false);
        }
        parentAction.ReturnToActorTheActionResult(status);
    }
    public void StopPerTickEffect() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickEffect);
        }
    }
    private void PerTickEffect() {
        currentDuration++;
        if (perTickEffect != null) {
            perTickEffect();
        }
        if(currentDuration >= duration) {
            EndPerTickEffect();
        }
    }

    #region Animation
    public void SetAnimation(string animationName) {
        this.animationName = animationName;
    }
    #endregion
}

public struct ActionLog {
    public Log log;
    public System.Action notifAction;
}