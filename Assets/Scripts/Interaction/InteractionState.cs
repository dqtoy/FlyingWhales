using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class InteractionState {
    protected Interaction _interaction;
    protected string _name;
    protected string _description;
    protected bool _isEnd;
    protected bool _useInvestigatorMinionOnly, _useTokeneerMinionOnly;
    //protected bool _isTimed;
    protected Action _effect;
    //protected GameDate _timeDate;
    protected Character _assignedPlayerCharacter;
    protected ActionOption _chosenOption;
    protected ActionOption _defaultOption;
    protected Log _descriptionLog;
    protected ActionOption[] _actionOptions;
    protected List<object> _assignedObjects;
    protected List<LogFiller> logFillers;
    protected List<Log> otherLogs;

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
    public LocationToken assignedLocation {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(LocationToken)) as LocationToken); }
    }
    public CharacterToken assignedCharacter {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(CharacterToken)) as CharacterToken); }
    }
    public SpecialToken assignedSpecialToken {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(SpecialToken)) as SpecialToken); }
    }
    public Minion assignedMinion {
        get { return (_assignedObjects == null ? null : GetAssignedObjectOfType(typeof(Minion)) as Minion); }
    }
    public Character assignedPlayerCharacter {
        get { return _assignedPlayerCharacter; }
    }
    public List<object> assignedObjects {
        get { return _assignedObjects; }
    }
    #endregion

    public InteractionState(string name, Interaction interaction) {
        _interaction = interaction;
        _name = name;
        _chosenOption = null;
        SetUseInvestigatorMinionOnly(false);
        SetUseTokeneerMinionOnly(false);
        _actionOptions = new ActionOption[4];
        logFillers = new List<LogFiller>();
    }

    //public void SetDescription(string desc) {
    //    _description = desc;
    //}
    public void SetUseInvestigatorMinionOnly(bool state) {
        _useInvestigatorMinionOnly = state;
    }
    public void SetUseTokeneerMinionOnly(bool state) {
        _useTokeneerMinionOnly = state;
    }
    public void SetAssignedPlayerCharacter(Character character) {
        _assignedPlayerCharacter = character;
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
        if (descriptionLog == null) {
            Debug.LogWarning("State " + this.name + " in " + interaction.name  + " does not have a description log!");
        }
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
    public virtual void CreateLogs() {
        if (_descriptionLog == null) {
            _descriptionLog = new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), _name.ToLower() + "_description", this.interaction);
        }
        bool hasKeys = LocalizationManager.Instance.HasKeysLike("Events", _interaction.GetType().ToString(), _name.ToLower(), new string[] { "_description", "_special" });
        if (!hasKeys) {
            otherLogs = null;
        } else {
            otherLogs = new List<Log>();
            if (interaction.characterInvolved == null || interaction.characterInvolved.isTracked) {
                List<string> keysForState = LocalizationManager.Instance.GetKeysLike("Events", _interaction.GetType().ToString(), _name.ToLower(), new string[] { "_description", "_special" });
                for (int i = 0; i < keysForState.Count; i++) {
                    string currentKey = keysForState[i];
                    otherLogs.Add(new Log(GameManager.Instance.Today(), "Events", _interaction.GetType().ToString(), currentKey, this.interaction));
                }
            } else {
                //If character involved is untracked
                if (interaction.name.ToLower().Contains("move to")) {
                    //If move to, use vague left log
                    //TODO: left structure log
                    otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "left_area", this.interaction));
                } else {
                    if (interaction.targetCharacter != null) {
                        InteractionAttributes attributes = InteractionManager.Instance.GetCategoryAndAlignment(interaction.type, interaction.characterInvolved);
                        if (attributes.categories.Contains(INTERACTION_CATEGORY.SOCIAL) || attributes.categories.Contains(INTERACTION_CATEGORY.ROMANTIC)) {
                            //Log "did something with"
                            otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "did_with", this.interaction));
                        } else {
                            //Log "did something to"
                            otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "did_to", this.interaction));
                        }
                    } else {
                        //No target character log
                        //TODO: Something happened to Character at Structure
                        otherLogs.Add(new Log(GameManager.Instance.Today(), "Character", "Generic", "did_something", this.interaction));
                    }
                }
            }
        }
    }
    public void AddOtherLog(Log log) {
        otherLogs.Add(log);
    }
    public void OverrideDescriptionLog(Log descriptionLog) {
        _descriptionLog = descriptionLog;
    }
    public virtual void SetDescription() {
        //TODO: make this more performant
        if(_descriptionLog != null) {
            //if (!_useInvestigatorMinionOnly && !_useTokeneerMinionOnly) {
            //    if (_interaction.investigatorCharacter != null) {
            //        _descriptionLog.AddToFillers(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
            //    }
            //} else if (_useInvestigatorMinionOnly && _useTokeneerMinionOnly) {
            //    if (_interaction.investigatorCharacter != null) {
            //        _descriptionLog.AddToFillers(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
            //    }
            //    if (_interaction.tokeneerMinion != null) {
            //        _descriptionLog.AddToFillers(_interaction.tokeneerMinion.character, _interaction.tokeneerMinion.character.name, LOG_IDENTIFIER.MINION_2);
            //    }
            //} else {
            //    if (_useInvestigatorMinionOnly) {
            //        if (_interaction.investigatorCharacter != null) {
            //            _descriptionLog.AddToFillers(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
            //        }
            //    }
            //    if (_useTokeneerMinionOnly) {
            //        if (_interaction.tokeneerMinion != null) {
            //            _descriptionLog.AddToFillers(_interaction.tokeneerMinion.character, _interaction.tokeneerMinion.character.name, LOG_IDENTIFIER.MINION_1);
            //        }
            //    }
            //}
            if (_interaction.investigatorCharacter != null) {
                _descriptionLog.AddToFillers(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
            }
            if (interaction.characterInvolved != null) {
                if(!_descriptionLog.HasFillerForIdentifier(LOG_IDENTIFIER.ACTIVE_CHARACTER)){
                    _descriptionLog.AddToFillers(interaction.characterInvolved, interaction.characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                }
                _descriptionLog.AddToFillers(interaction.characterInvolved.currentStructure, interaction.characterInvolved.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_1);
            }
            if (interaction.targetCharacter != null) {
                _descriptionLog.AddToFillers(interaction.targetCharacter.currentStructure, interaction.targetCharacter.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_2);
            }
            if (!_descriptionLog.HasFillerForIdentifier(LOG_IDENTIFIER.LANDMARK_1)) {
                _descriptionLog.AddToFillers(_interaction.interactable, _interaction.interactable.name, LOG_IDENTIFIER.LANDMARK_1);
            }

            _description = Utilities.LogReplacer(descriptionLog);
            //InteractionUI.Instance.interactionItem.SetDescription(_description, descriptionLog);
        }
        if (otherLogs != null && otherLogs.Count > 0) {
            //if (!_useInvestigatorMinionOnly && !_useTokeneerMinionOnly) {
            //    if (_interaction.investigatorCharacter != null) {
            //        logFillers.Add(new LogFiller(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
            //    }
            //} else if (_useInvestigatorMinionOnly && _useTokeneerMinionOnly) {
            //    if (_interaction.investigatorCharacter != null) {
            //        logFillers.Add(new LogFiller(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
            //    }
            //    if (_interaction.tokeneerMinion != null) {
            //        logFillers.Add(new LogFiller(_interaction.tokeneerMinion.character, _interaction.tokeneerMinion.character.name, LOG_IDENTIFIER.MINION_2));
            //    }
            //} else {
            //    if (_useInvestigatorMinionOnly) {
            //          if (_interaction.investigatorCharacter != null) {
            //              logFillers.Add(new LogFiller(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
            //          }
            //    }
            //    if (_useTokeneerMinionOnly) {
            //        if (_interaction.tokeneerMinion != null) {
            //            logFillers.Add(new LogFiller(_interaction.tokeneerMinion.character, _interaction.tokeneerMinion.character.name, LOG_IDENTIFIER.MINION_1));
            //        }
            //    }
            //}
            if (_interaction.investigatorCharacter != null) {
                logFillers.Add(new LogFiller(_interaction.investigatorCharacter, _interaction.investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
            }
            if (interaction.characterInvolved != null) {
                logFillers.Add(new LogFiller(interaction.characterInvolved, interaction.characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
                logFillers.Add(new LogFiller(interaction.characterInvolved.currentStructure, interaction.characterInvolved.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_1));
            }
            if (interaction.targetCharacter != null) {
                logFillers.Add(new LogFiller(interaction.targetCharacter.currentStructure, interaction.targetCharacter.currentStructure.ToString(), LOG_IDENTIFIER.STRUCTURE_2));
            }
            if (!AlreadyHasLogFiller(LOG_IDENTIFIER.LANDMARK_1)) {
                logFillers.Add(new LogFiller(interaction.interactable, interaction.interactable.name, LOG_IDENTIFIER.LANDMARK_1));
            }
            for (int i = 0; i < otherLogs.Count; i++) {
                Log currLog = otherLogs[i];
                currLog.SetFillers(logFillers);
                AddLogToInvolvedObjects(currLog);
            }
        }
    }
    public void AddLogToInvolvedObjects(Log log) {
        _interaction.interactable.AddHistory(log);
        //if (_interaction.explorerMinion != null) {
        //    _interaction.explorerMinion.character.AddHistory(log);
        //}
        for (int i = 0; i < log.fillers.Count; i++) {
            LogFiller currFiller = log.fillers[i];
            object obj = currFiller.obj;
            if (obj != null) {
                if (obj is Character) {
                    (obj as Character).AddHistory(log);
                }  else if (obj is Minion) {
                    (obj as Minion).character.AddHistory(log);
                }
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
            if (currObject.GetType() == type || currObject.GetType().BaseType == type) {
                return currObject;
            }
        }
        return null;
    }

    #region Log Fillers
    public void AddLogFiller(LogFiller filler, bool replaceExisting = true) {
        if (replaceExisting) {
            if (AlreadyHasLogFiller(filler.identifier)) {
                logFillers.Remove(GetLogFiller(filler.identifier));
            }
            logFillers.Add(filler);
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
