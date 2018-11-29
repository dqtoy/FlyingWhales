using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Job {
    protected string _name;
    protected JOB _jobType;
    protected int _actionDuration; //-1 means no limits and no progress
    protected bool _hasCaptureEvent;
    protected Character _character;
    protected Interaction _createdInteraction;
    protected INTERACTION_TYPE[] _characterInteractions; //For non-minion characters only

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
    public Interaction createdInteraction {
        get { return _createdInteraction; }
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
    public virtual void DoJobAction() {
        Debug.Log(GameManager.Instance.TodayLogString() + " Doing job action: " + character.name + "(" + jobType.ToString() + ")");
    }
    public virtual int GetSuccessRate() { return 0; }
    public virtual int GetFailRate() { return 40; }
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
            Messenger.AddListener(Signals.HOUR_ENDED, CatchRandomEvent);
        }
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetAndStartInteractionTimerJob(_actionDuration);
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.ShowInteractionTimerJob();
    }

    //Stops Job Action entirely
    //Uses - when a minion is recalled, when job action duration ends
    public void StopJobAction() {
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimerJob();
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimerJob();
        if (_actionDuration != -1) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, CheckJobAction);
        }
        if (_hasCaptureEvent) {
            Messenger.RemoveListener(Signals.HOUR_ENDED, CatchRandomEvent);
        }
    }
    public void StopCreatedInteraction() {
        if(_createdInteraction != null) {
            _createdInteraction.interactable.landmarkVisual.StopInteractionTimer();
            _createdInteraction.interactable.landmarkVisual.HideInteractionTimer();
            _createdInteraction.TimedOutRunDefault();
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
        _character.specificLocation.tileLocation.areaOfTile.coreTile.landmarkOnTile.landmarkVisual.SetTimerPauseStateJob(_isJobActionPaused);
    }
    public void SetCreatedInteraction(Interaction interaction) {
        _createdInteraction = interaction;
        if(_createdInteraction != null) {
            _createdInteraction.SetJobAssociated(this);
        }
    }
    private void CatchRandomEvent() {
        if (_isJobActionPaused) { return; }
        CaptureRandomLandmarkEvent();
    }
    public void CreateRandomInteractionForNonMinionCharacters() {
        if(_characterInteractions != null) {
            INTERACTION_TYPE type = _characterInteractions[UnityEngine.Random.Range(0, _characterInteractions.Length)];
            Interaction interaction = InteractionManager.Instance.CreateNewInteraction(type, character.specificLocation as BaseLandmark);
            character.AddInteraction(interaction);
        }
    }
    #endregion
}
