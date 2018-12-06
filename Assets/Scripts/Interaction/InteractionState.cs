using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InteractionState {
    private Interaction _interaction;
    private string _name;
    private string _description;
    private bool _isEnd;
    //private bool _isTimed;
    private Action _effect;
    //private GameDate _timeDate;
    private Minion _assignedMinion;
    private ActionOption _chosenOption;
    private ActionOption _defaultOption;
    private Log _descriptionLog;
    private ActionOption[] _actionOptions;
    private List<object> _assignedObjects;
    private List<LogFiller> logFillers;
    private List<Log> otherLogs;

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

    public Log descriptionLog {
        get { return _descriptionLog; }
    }
    public ActionOption[] actionOptions {
        get { return _actionOptions; }
    }
    //public GameDate timeDate {
    //    get { return _timeDate; }
    //}
    public IUnit assignedUnit {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(IUnit)) as IUnit); }
    }
    public LocationIntel assignedLocation {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(LocationIntel)) as LocationIntel); }
    }
    public CharacterIntel assignedCharacter {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(CharacterIntel)) as CharacterIntel); }
    }
    public Minion assignedMinion {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(Minion)) as Minion); }
    }
    public List<object> assignedObjects {
        get { return _assignedObjects; }
    }
    #endregion

    public InteractionState(string name, Interaction interaction) {
        _interaction = interaction;
        _name = name;
        _chosenOption = null;
        _actionOptions = new ActionOption[4];
        logFillers = new List<LogFiller>();
    }

    //public void SetDescription(string desc) {
    //    _description = desc;
    //}
    public void SetAssignedMinion(Minion minion) {
        _assignedMinion = minion;
    }
    public void SetAssignedObjects(List<object> objects) {
        _assignedObjects = objects;
    }
    public void AddActionOption(ActionOption option) {
        for (int i = 0; i < _actionOptions.Length; i++) {
            if(_actionOptions[i] == null) {
                _actionOptions[i] = option;
                break;
            }
        }
    }
    public void SetEffect(Action effect, bool isEnd = true) {
        //if (endEffect != null) {
        //    _isEnd = true;
        //} else {
        //    _isEnd = false;
        //}
        _isEnd = isEnd;
        _effect = effect;
    }
    public void OnStartState() {
        CreateLogs();
        if (_effect != null) {
            _effect();
        }
        SetDescription();
        //if(_isTimed && _defaultOption != null) {
        //    SchedulingManager.Instance.AddEntry(_timeDate, () => ActivateDefault());
        //}
    }
    public void OnEndState() {
        //AssignedMinionGoesBack();
    }
    public void CreateLogs() {
        if (_descriptionLog == null) {
            _descriptionLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_description");
        }

        //Only put logs in minions and landmarks when the particualar interaction is chosen to be interfered by the player
        //if (_interaction.isChosen) {
        otherLogs = new List<Log>();
        List<string> keysForState = LocalizationManager.Instance.GetKeysLike("Events", _interaction.GetType().ToString(), _name.ToLower(), "_description");
        for (int i = 0; i < keysForState.Count; i++) {
            string currentKey = keysForState[i];
            otherLogs.Add(new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), currentKey));
        }

        if (_interaction.explorerMinion != null) {
            logFillers.Add(new LogFiller(_interaction.explorerMinion, _interaction.explorerMinion.name, LOG_IDENTIFIER.MINION_1));
        }
        if (interaction.characterInvolved != null) {
            logFillers.Add(new LogFiller(interaction.characterInvolved, interaction.characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        }
        if (!AlreadyHasLogFiller(LOG_IDENTIFIER.LANDMARK_1)) {
            logFillers.Add(new LogFiller(interaction.interactable.tileLocation.areaOfTile, interaction.interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1));
        }
        //}
    }
    public void OverrideDescriptionLog(Log descriptionLog) {
        _descriptionLog = descriptionLog;
    }
    public void SetDescription() {
        //TODO: make this more performant
        if(_descriptionLog != null) {
            if (_interaction.explorerMinion != null) {
                _descriptionLog.AddToFillers(_interaction.explorerMinion, _interaction.explorerMinion.name, LOG_IDENTIFIER.MINION_1);
            }
            if (interaction.characterInvolved != null) {
                _descriptionLog.AddToFillers(interaction.characterInvolved, interaction.characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            }
            if (!_descriptionLog.HasFillerForIdentifier(LOG_IDENTIFIER.LANDMARK_1)) {
                _descriptionLog.AddToFillers(_interaction.interactable.tileLocation.areaOfTile, _interaction.interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1);
            }
            _description = Utilities.LogReplacer(descriptionLog);
            //InteractionUI.Instance.interactionItem.SetDescription(_description, descriptionLog);
        }
        if (otherLogs != null) {
            for (int i = 0; i < otherLogs.Count; i++) {
                Log currLog = otherLogs[i];
                currLog.SetFillers(logFillers);
                _interaction.interactable.specificLocation.tileLocation.landmarkOnTile.AddHistory(currLog);
                if (_interaction.explorerMinion != null) {
                    _interaction.explorerMinion.character.AddHistory(currLog);
                }
                currLog.AddLogToInvolvedObjects();
            }
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
        //AssignedMinionGoesBack();
        interaction.EndInteraction();
    }
    public void AssignedMinionGoesBack() {
        if (_assignedMinion != null) {
            if (_assignedMinion.character.currentParty.currentAction == null || _assignedMinion.character.currentParty.iactionData.isDoneAction) {
                _assignedMinion.GoBackFromAssignment();
            } else {
                _assignedMinion.character.currentParty.currentAction.SetOnEndAction(() => _assignedMinion.GoBackFromAssignment());
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
    public object GetAssignedObjectOfType(System.Type type) {
        for (int i = 0; i < _assignedObjects.Count; i++) {
            object currObject = _assignedObjects[i];
            if (type == typeof(IUnit)) {
                if (currObject is IUnit) { //TODO: Make this more elegant!
                    return currObject;
                }
            } else {
                if (currObject.GetType() == type) {
                    return currObject;
                }
            }
        }
        return null;
    }

    #region Log Fillers
    public void AddLogFiller(LogFiller filler, bool replaceExisting = true) {
        if (AlreadyHasLogFiller(filler.identifier)) {
            if (replaceExisting) {
                logFillers.Remove(GetLogFiller(filler.identifier));
                logFillers.Add(filler);
            }
        } else {
            logFillers.Add(filler);
        }
    }
    public bool AlreadyHasLogFiller(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < logFillers.Count; i++) {
            if(logFillers[i].identifier == identifier) {
                return true;
            }
        }
        return false;
    }
    public LogFiller GetLogFiller(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < logFillers.Count; i++) {
            if (logFillers[i].identifier == identifier) {
                return logFillers[i];
            }
        }
        return new LogFiller();
    }
    #endregion
}
