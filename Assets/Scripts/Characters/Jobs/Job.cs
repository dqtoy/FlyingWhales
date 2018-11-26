﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Job {
    protected string _name;
    protected JOB _jobType;
    protected int _actionDuration; //-1 means no limits and no progress
    protected bool _hasCaptureEvent;
    protected Character _character;

    private int _currentTick;
    private bool _isJobActionPaused;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public int actionDuration {
        get { return _actionDuration; }
    }
    public JOB jobType {
        get { return _jobType; }
    }
    public Character character {
        get { return _character; }
    }
    #endregion

    public Job (Character character, JOB jobType) {
        _jobType = jobType;
        _name = Utilities.NormalizeString(_jobType.ToString());
        _character = character;
    }

    #region Virtuals
    public virtual void OnAssignJob() {}
    public virtual void CaptureRandomLandmarkEvent() {}
    public virtual void ApplyActionDuration() {}
    public virtual void DoJobAction() {}
    #endregion

    #region Utilities
    public void StartJobAction() {
        ApplyActionDuration();
        _currentTick = 0;
        SetJobActionPauseState(false);
        if(_actionDuration != -1) {
            Messenger.AddListener(Signals.HOUR_STARTED, CheckJobAction);
        }
        if (_hasCaptureEvent) {
            Messenger.AddListener(Signals.HOUR_ENDED, CaptureRandomLandmarkEvent);
        }
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetAndStartInteractionTimer(_actionDuration);
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.ShowNoInteractionForeground();
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.ShowInteractionTimer();
    }

    //Stops Job Action entirely
    //Uses - when a minion is recalled, when job action duration ends
    public void StopJobAction() {
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimer();
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimer();
        if (_actionDuration != -1) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, CheckJobAction);
        }
        if (_hasCaptureEvent) {
            Messenger.RemoveListener(Signals.HOUR_ENDED, CaptureRandomLandmarkEvent);
        }
    }
    private void CheckJobAction() {
        if (_isJobActionPaused) { return; }
        if (_currentTick >= _actionDuration) {
            StopJobAction();
            DoJobAction();
            return;
        }
        _currentTick++;

    }
    protected void SetJobActionPauseState(bool state) {
        _isJobActionPaused = state;
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetTimerPauseState(_isJobActionPaused);
    }
    protected void StartTimerForCreatedInteraction(Interaction interaction) {

    }
    #endregion
}
