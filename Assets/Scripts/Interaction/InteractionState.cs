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
        SetDescription();
        if (_isEnd && _endEffect != null) {
            _endEffect();
        }
        //if(_isTimed && _defaultOption != null) {
        //    SchedulingManager.Instance.AddEntry(_timeDate, () => ActivateDefault());
        //}
    }
    public void OnEndState() {
        AssignedMinionGoesBack();
    }
    private void SetDescription() {
        //TODO: make this more performant
        Log descriptionLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_description");
        if (interaction.interactable.explorerMinion != null) {
            descriptionLog.AddToFillers(_interaction.interactable.explorerMinion, _interaction.interactable.explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
        }
        descriptionLog.AddToFillers(_interaction.interactable.specificLocation.tileLocation.landmarkOnTile, _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);

        _description = Utilities.LogReplacer(descriptionLog);
        InteractionUI.Instance.interactionItem.SetDescription(_description);

        //Minion Log
        if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("Events", _interaction.GetType().ToString(), _name.ToLower() + "_logminion"))) {
            if (interaction.interactable.explorerMinion != null) {
                Log minionLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_logminion");
                minionLog.AddToFillers(_interaction.interactable.explorerMinion, _interaction.interactable.explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
                minionLog.AddToFillers(_interaction.interactable.specificLocation.tileLocation.landmarkOnTile, _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
                interaction.interactable.explorerMinion.icharacter.AddHistory(minionLog);
            }
        }

        //Landmark Log
        if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("Events", _interaction.GetType().ToString(), _name.ToLower() + "_loglandmark"))) {
            Log landmarkLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_loglandmark");
            if (interaction.interactable.explorerMinion != null) {
                landmarkLog.AddToFillers(_interaction.interactable.explorerMinion, _interaction.interactable.explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
            }
            landmarkLog.AddToFillers(_interaction.interactable.specificLocation.tileLocation.landmarkOnTile, _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
            _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.AddHistory(landmarkLog);
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
