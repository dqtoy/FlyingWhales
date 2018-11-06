using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ECS;

public class InteractionState {
    private Interaction _interaction;
    private string _name;
    private string _description;
    private bool _isEnd;
    //private bool _isTimed;
    private Action _endEffect;
    //private GameDate _timeDate;
    private Minion _assignedMinion;
    private ActionOption _chosenOption;
    private ActionOption _defaultOption;
    private Log _descriptionLog;
    private Log _minionLog;
    private Log _landmarkLog;
    private ActionOption[] _actionOptions;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string description {
        get { return _description; }
    }
    public bool isEnd {
        get { return _isEnd; }
    }
    //public bool isTimed {
    //    get { return _isTimed; }
    //}
    public ActionOption chosenOption {
        get { return _chosenOption; }
    }
    public ActionOption defaultOption {
        get { return _defaultOption; }
    }
    public Interaction interaction {
        get { return _interaction; }
    }
    public Minion assignedMinion {
        get { return _assignedMinion; }
    }
    public Log descriptionLog {
        get { return _descriptionLog; }
    }
    public Log minionLog {
        get { return _minionLog; }
    }
    public Log landmarkLog {
        get { return _landmarkLog; }
    }
    public ActionOption[] actionOptions {
        get { return _actionOptions; }
    }
    //public GameDate timeDate {
    //    get { return _timeDate; }
    //}
    #endregion

    public InteractionState(string name, Interaction interaction) {
        _interaction = interaction;
        _name = name;
        _chosenOption = null;
        _actionOptions = new ActionOption[4];
    }

    //public void SetDescription(string desc) {
    //    _description = desc;
    //}
    public void SetAssignedMinion(Minion minion) {
        _assignedMinion = minion;
    }
    public void AddActionOption(ActionOption option) {
        for (int i = 0; i < _actionOptions.Length; i++) {
            if(_actionOptions[i] == null) {
                _actionOptions[i] = option;
                break;
            }
        }
    }
    public void SetEndEffect(Action endEffect) {
        if (endEffect != null) {
            _isEnd = true;
        } else {
            _isEnd = false;
        }
        _endEffect = endEffect;
    }
    public void OnStartState() {
        CreateLogs();
        if (_isEnd && _endEffect != null) {
            _endEffect();
        }
        SetDescription();
        //if(_isTimed && _defaultOption != null) {
        //    SchedulingManager.Instance.AddEntry(_timeDate, () => ActivateDefault());
        //}
    }
    public void OnEndState() {
        AssignedMinionGoesBack();
    }
    public void CreateLogs() {
        if (_interaction.explorerMinion != null) {
            _descriptionLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_description");
            if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("Events", _interaction.GetType().ToString(), _name.ToLower() + "_logminion"))) {
                _minionLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_logminion");
            }
            if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("Events", _interaction.GetType().ToString(), _name.ToLower() + "_loglandmark"))) {
                _landmarkLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_loglandmark");
            }
        }
    }
    public void SetDescription() {
        //TODO: make this more performant
        if(_descriptionLog != null) {
            _descriptionLog.AddToFillers(_interaction.explorerMinion, _interaction.explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
            _descriptionLog.AddToFillers(_interaction.interactable.specificLocation.tileLocation.landmarkOnTile, _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
            _description = Utilities.LogReplacer(descriptionLog);
            InteractionUI.Instance.interactionItem.SetDescription(_description);
        }

        //Minion Log
        if (_minionLog != null) {
            _minionLog.AddToFillers(_interaction.explorerMinion, _interaction.explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
            _minionLog.AddToFillers(_interaction.interactable.specificLocation.tileLocation.landmarkOnTile, _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
            interaction.explorerMinion.icharacter.AddHistory(_minionLog);
        }

        //Landmark Log
        if (_landmarkLog != null) {
            _landmarkLog.AddToFillers(_interaction.explorerMinion, _interaction.explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
            _landmarkLog.AddToFillers(_interaction.interactable.specificLocation.tileLocation.landmarkOnTile, _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
            _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.AddHistory(_landmarkLog);
        }

    }
    public void SetChosenOption(ActionOption option) {
        _chosenOption = option;
    }
    //public void SetTimeSchedule(ActionOption defaultOption, GameDate timeSched) {
        //_isTimed = true;
        //_timeDate = timeSched;
        //SetDefaultOption(defaultOption);
    //}
    public void SetDefaultOption(ActionOption defaultOption) {
        _defaultOption = defaultOption;
    }
    public void EndResult() {
        AssignedMinionGoesBack();
        interaction.EndInteraction();
    }
    public void AssignedMinionGoesBack() {
        if (_assignedMinion != null) {
            if (_assignedMinion.icharacter.currentParty.currentAction == null || _assignedMinion.icharacter.currentParty.iactionData.isDoneAction) {
                _assignedMinion.GoBackFromAssignment();
            } else {
                _assignedMinion.icharacter.currentParty.currentAction.SetOnEndAction(() => _assignedMinion.GoBackFromAssignment());
            }
        }
    }
    public void ActivateDefault() {
        if(_interaction.currentState == this && !_interaction.isActivated) {
            if (_isEnd) {
                EndResult();
            } else {
                _defaultOption.ActivateOption(_interaction.interactable);
            }
        }
    }
    public ActionOption GetOption(string optionName) {
        for (int i = 0; i < actionOptions.Length; i++) {
            ActionOption option = actionOptions[i];
            if (option != null && option.name == optionName) {
                return option;
            }
        }
        return null;
    }
}
