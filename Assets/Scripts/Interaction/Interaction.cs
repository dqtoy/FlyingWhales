using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class Interaction {
    protected int _id;
    protected string _name;
    protected int _timeOutTicks;
    protected GameDate _timeDate;
    protected INTERACTION_TYPE _type;
    protected INTERACTION_CATEGORY _category;
    protected INTERACTION_ALIGNMENT _alignment;
    protected Area _interactable;
    protected Dictionary<string, InteractionState> _states;
    //protected InteractionItem _interactionItem;
    protected bool _isActivated;
    protected bool _isDone;
    protected bool _hasActivatedTimeOut;
    protected bool _isSecondTimeOutCancelled;
    protected bool _isChosen;
    protected bool _hasInitialized;
    protected Func<bool> _canInteractionBeDone;
    protected Action _initializeAction;
    protected InteractionState _previousState;
    protected InteractionState _currentState;
    protected Character _defaultInvestigatorCharacter;
    protected Character _characterInvolved;
    protected Token _tokenTrigger;
    protected Action _minionSuccessfulAction;
    protected List<Action> _endInteractionActions;
    protected Job _jobAssociated;
    protected JOB[] _jobFilter;
    protected object[] otherData;
    private string interactionDebugLog;

    private bool _hasUsedBaseCreateStates;

    public const int secondTimeOutTicks = 30;

    #region getters/setters
    public INTERACTION_TYPE type {
        get { return _type; }
    }
    public INTERACTION_CATEGORY category {
        get { return _category; }
    }
    public INTERACTION_ALIGNMENT alignment {
        get { return _alignment; }
    }
    public string name {
        get { return _name; }
    }
    public GameDate timeDate {
        get { return _timeDate; }
    }
    public InteractionState currentState {
        get { return _currentState; }
    }
    public InteractionState previousState {
        get { return _previousState; }
    }
    public Character investigatorCharacter {
        get {
            //if(_interactable.areaInvestigation.assignedMinion != null && _interactable == _interactable.areaInvestigation.assignedMinion.character.specificLocation) {
            //    return _interactable.areaInvestigation.assignedMinion;
            //}
            //Only used for Minion Critical Fail Event since assignedMinion will be null, interaction must still have reference of the dead minion
            if (currentState != null && currentState.assignedPlayerCharacter != null) {
               return currentState.assignedPlayerCharacter;
            }
            if (_defaultInvestigatorCharacter != null) {
                return _defaultInvestigatorCharacter;
            }
            return PlayerManager.Instance.player.roleSlots[JOB.SPY].assignedCharacter; //TODO: Change this when design has said which minion to use as the default for interactions
        }
    }
    public Minion tokeneerMinion {
        get {
            if (_interactable.areaInvestigation.tokenCollector != null && _interactable == _interactable.areaInvestigation.tokenCollector.character.specificLocation) {
                return _interactable.areaInvestigation.tokenCollector;
            }
            return null;
        }
    }
    public Character characterInvolved {
        get { return _characterInvolved; }
    }
    //public InteractionItem interactionItem {
    //    get { return _interactionItem; }
    //}
    public Area interactable {
        get { return _interactable; }
    }
    public Token tokenTrigger {
        get { return _tokenTrigger; }
    }
    public bool isActivated {
        get { return _isActivated; }
    }
    public bool hasActivatedTimeOut {
        get { return _hasActivatedTimeOut; }
    }
    public bool isChosen {
        get { return _isChosen; }
    }
    public bool hasInitialized {
        get { return _hasInitialized; }
    }
    public JOB[] jobFilter {
        get { return _jobFilter; }
    }
    public virtual Character targetCharacter {
        get { return null; }
    }
    #endregion

    public Interaction(Area interactable, INTERACTION_TYPE type, int timeOutTicks) {
        _id = Utilities.SetID(this);
        _type = type;
        _interactable = interactable;
        _timeOutTicks = timeOutTicks;
        //_isFirstTimeOutCancelled = false;
        SetActivatedTimeOutState(false);
        _isSecondTimeOutCancelled = false;
        _hasUsedBaseCreateStates = false;
        _states = new Dictionary<string, InteractionState>();
        _endInteractionActions = new List<Action>();
        //_jobFilter = new JOB[] { JOB.NONE };
        //Debug.Log("Created new interaction " + type.ToString() + " at " + interactable.name);
        interactionDebugLog = type.ToString() + " Event at " + interactable.name + "(" + interactable.name + ") Summary: \n" +
            GameManager.Instance.TodayLogString() + " Event Created.";
    }

    #region Virtuals
    public virtual void Initialize() {
        _hasInitialized = true;
        if(_initializeAction != null) {
            _initializeAction();
        }
        //SetCharacterInvolved(characterInvolved);
        CreateStates();
        //SetExplorerMinion(explorerMinion);
        //ScheduleFirstTimeOut();
    }
    public virtual void CreateStates() {
    }
    public virtual void CreateActionOptions(InteractionState state) { }
    public virtual void EndInteraction() {
        _isDone = true;
        if(_characterInvolved != null) {
            _characterInvolved.AdjustDoNotDisturb(-1);
        }
        _interactable.RemoveInteraction(this);
        if (_jobAssociated != null) {
            _jobAssociated.SetCreatedInteraction(null);
            SetJobAssociated(null);
        }
        for (int i = 0; i < _endInteractionActions.Count; i++) {
            _endInteractionActions[i]();
        }
        _endInteractionActions.Clear();
        Debug.Log(interactionDebugLog);
        if(InteractionUI.Instance.interaction == this) {
            InteractionUI.Instance.HideInteractionUI();
        }
        Messenger.Broadcast<Interaction>(Signals.INTERACTION_ENDED, this);
    }
    public virtual void OnInteractionActive() {
        _isChosen = true;
        //interactable.landmarkVisual.StopInteractionTimer();
        //interactable.landmarkVisual.HideInteractionTimer();
        _currentState.CreateLogs();
        _currentState.SetDescription();
        Messenger.Broadcast(Signals.UPDATED_INTERACTION_STATE, this);
    } //this is called when the player clicks the "exclamation point" button and this interaction was chosen
    public virtual bool CanInteractionBeDoneBy(Character character) { //Converted this to virtual so each instance of interaction can also have trigger requirements other than CanCreateInteraction at InteractionManager
        if (_canInteractionBeDone != null) {
            return _canInteractionBeDone();
        }
        return true;
    }
    //public virtual bool CanStillDoInteraction() { return true; }
    #endregion

    #region Utilities
    public void SetCurrentState(InteractionState state) {
        _previousState = _currentState;
        //if(_currentState != null && _currentState.chosenOption != null) {
        if (_currentState != null) {
            //state.SetAssignedObjects(_currentState.assignedObjects);
            //if (_currentState.chosenOption != null) {
            state.SetAssignedPlayerCharacter(_currentState.assignedPlayerCharacter);
            _currentState.OnEndState();
            //}
        }
        _currentState = state;
        AddToDebugLog("Set current state to " + _currentState.name);
        _currentState.OnStartState();
        Messenger.Broadcast(Signals.UPDATED_INTERACTION_STATE, this);
    }
    public void AddState(InteractionState state) {
        _states.Add(state.name, state);
    }
    public void SetActivatedState(bool state) {
        _isActivated = state;
        //if (!state) {
        //    _currentState.SetChosenOption(null);
        //}
        Messenger.Broadcast(Signals.CHANGED_ACTIVATED_STATE, this);
    }
    //public void CancelFirstTimeOut() {
    //    _isFirstTimeOutCancelled = true;
    //}
    public void CancelSecondTimeOut() {
        _isSecondTimeOutCancelled = true;
    }
    //public void ScheduleFirstTimeOut() {
    //    GameDate timeOutDate = GameManager.Instance.Today();
    //    timeOutDate.AddHours(_timeOutTicks);
    //    _timeDate = timeOutDate;
    //    SchedulingManager.Instance.AddEntry(_timeDate, () => FirstTimeOut());
    //}
    public void ScheduleSecondTimeOut() {
        SetActivatedTimeOutState(true);
        GameDate timeOutDate = GameManager.Instance.Today();
        timeOutDate.AddDays(secondTimeOutTicks);
        _timeDate = timeOutDate;
        SchedulingManager.Instance.AddEntry(_timeDate, () => SecondTimeOut());

        //interactable.landmarkVisual.SetAndStartInteractionTimer(secondTimeOutTicks);
        ////interactable.landmarkVisual.ShowInteractionForeground();
        //interactable.landmarkVisual.ShowInteractionTimer(this);
    }
    public void SetActivatedTimeOutState(bool state) {
        _hasActivatedTimeOut = state;
    }
    public void AddEndInteractionAction(Action action) {
        _endInteractionActions.Add(action);
    }
    public void SetMinionSuccessAction(Action action) {
        _minionSuccessfulAction = action;
    }
    //public void SetInteractionItem(InteractionItem interactionItem) {
    //    _interactionItem = interactionItem;
    //}
    //protected int GetRemainingDurationFromState(InteractionState state) {
    //    return GameManager.Instance.GetTicksDifferenceOfTwoDates(GameManager.Instance.Today(), state.timeDate);
    //}
    //protected void SetDefaultActionDurationAsRemainingTicks(string optionName, InteractionState stateFrom) {
    //    ActionOption option = stateFrom.GetOption(optionName);
    //    int remainingTicks = GameManager.Instance.GetTicksDifferenceOfTwoDates(GameManager.Instance.Today(), stateFrom.timeDate);
    //    option.duration = remainingTicks;
    //}
    //protected void FirstTimeOut() {
    //    if (!_isFirstTimeOutCancelled) {
    //        TimedOutRunDefault();
    //    }
    //}
    public void SecondTimeOut() {
        if (!_isSecondTimeOutCancelled) {
            //interactable.landmarkVisual.StopInteractionTimer();
            //interactable.landmarkVisual.HideInteractionTimer();
            SetActivatedTimeOutState(false);
            //TimedOutRunDefault();
            //_interactable.areaInvestigation.ExploreArea();
        }
    }
    public void TimedOutRunDefault(ref string summary) {
        //|| !CanStillDoInteraction()
        if ((_characterInvolved != null && _characterInvolved.isDead)) {
            EndInteraction();
            return;
        }
        if (!_hasInitialized) {
            Initialize();
        }
        if (_currentState == null) {
            throw new Exception(this.GetType().ToString() + " interaction at " + interactable.name + " has a current state of null at second time out!");
        }
        if(!_currentState.isEnd && _currentState.defaultOption == null) {
            return;
        }
        while (!_isDone) {
            _currentState.ActivateDefault();
        }
        if (!string.IsNullOrEmpty(summary)) {
            summary += "\nInteraction summary :" + interactionDebugLog;
        }
    }
    public void SetDefaultInvestigatorCharacter(Character character) {
        _defaultInvestigatorCharacter = character;
        //if(_explorerMinion != null) {
        //    _currentState.CreateLogs();
        //    _currentState.SetDescription();
        //}
    }
    public void SetCharacterInvolved(Character character) {
        _characterInvolved = character;
        if(_characterInvolved != null) {
            AddToDebugLog("Set character involved to " + character.name);
            _characterInvolved.AdjustDoNotDisturb(1);
        }
    }
    public bool AssignedMinionIsOfClass(string className) {
        return this.investigatorCharacter != null && this.investigatorCharacter.characterClass.className.ToLower() == className.ToLower();
    }
    public bool AssignedMinionIsOfClass(List<string> allowedClassNames) {
        if(this.investigatorCharacter != null) {
            for (int i = 0; i < allowedClassNames.Count; i++) {
                if(allowedClassNames[i].ToLower() == this.investigatorCharacter.characterClass.className.ToLower()) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool DoesJobTypeFitsJobFilter(Character character) {
        if(_jobFilter == null) {
            return true;
        }
        for (int i = 0; i < _jobFilter.Length; i++) {
            if(character.job.jobType == _jobFilter[i]) {
                return true;
            }
        }
        return false;
    }
    public bool DoesJobTypeFitsJobFilter(JOB jobType) {
        if (_jobFilter != null) {
            for (int i = 0; i < _jobFilter.Length; i++) {
                if (jobType == _jobFilter[i]) {
                    return true;
                }
            }
        }
        return false;
    }
    public void SetOtherData(object[] data) {
        otherData = data;
    }
    public void SetJobAssociated(Job job) {
        _jobAssociated = job;
    }
    public void AddToDebugLog(string message) {
        interactionDebugLog += "\n" + GameManager.Instance.TodayLogString() + message;
    }
    protected void MinionSuccess() {
        if(_minionSuccessfulAction != null) {
            _minionSuccessfulAction();
        }
    }
    public void SetInitializeAction(Action action) {
        _initializeAction = action;
    }
    public void SetCanInteractionBeDoneAction(Func<bool> func) {
        _canInteractionBeDone = func;
    }
    public void SetTokenTrigger(Token token) {
        _tokenTrigger = token;
    }
    
    public void AddLogFillerToAllStates(LogFiller filler) {
        foreach (KeyValuePair<string, InteractionState> kvp in _states) {
            kvp.Value.AddLogFiller(filler);
            if (kvp.Value.descriptionLog != null) {
                kvp.Value.descriptionLog.AddToFillers(filler);
            }
        }
        
    }
    protected void AdjustFactionsRelationship(Faction faction1, Faction faction2, int adjustment, InteractionState state) {
        if (faction1 == null || faction2 == null || faction1.id == FactionManager.Instance.neutralFaction.id 
            || faction2.id == FactionManager.Instance.neutralFaction.id) {
            return;
        }
        faction1.AdjustRelationshipFor(faction2, adjustment);
        Log factionRelationshipLog = new Log(GameManager.Instance.Today(), "Events", "Generic", "faction_relationship_changed");
        factionRelationshipLog.AddToFillers(new LogFiller(faction1, faction1.name, LOG_IDENTIFIER.FACTION_1));
        factionRelationshipLog.AddToFillers(new LogFiller(faction2, faction2.name, LOG_IDENTIFIER.FACTION_2));
        factionRelationshipLog.AddToFillers(new LogFiller(null,
            Utilities.NormalizeString(faction1.GetRelationshipWith(faction2).relationshipStatus.ToString()), LOG_IDENTIFIER.OTHER));
        if (_characterInvolved != null) {
            factionRelationshipLog.AddToFillers(new LogFiller(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        }
        if (investigatorCharacter != null) {
            factionRelationshipLog.AddToFillers(new LogFiller(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));
        }
        factionRelationshipLog.AddToFillers(new LogFiller(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1));
        factionRelationshipLog.SetFillerLockedState(true);
        //state.AddOtherLog(factionRelationshipLog);
        factionRelationshipLog.AddLogToInvolvedObjects();
    }
    #endregion

    #region Shared States and Effects
    protected void CreateExploreStates() {
        _hasUsedBaseCreateStates = true;
        InteractionState exploreContinuesState = new InteractionState("Explore Continues", this);
        InteractionState exploreEndsState = new InteractionState("Explore Ends", this);

        exploreContinuesState.SetEffect(() => ExploreContinuesRewardEffect(exploreContinuesState));
        exploreEndsState.SetEffect(() => ExploreEndsRewardEffect(exploreEndsState));

        _states.Add(exploreContinuesState.name, exploreContinuesState);
        _states.Add(exploreEndsState.name, exploreEndsState);
    }
    protected void CreateWhatToDoNextState(string description) {
        InteractionState whatToDoNextState = new InteractionState("What To Do Next", this);
        //whatToDoNextState.SetDescription(description);

        ActionOption yesPleaseOption = new ActionOption {
            interactionState = whatToDoNextState,
            cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
            name = "Yes, please.",
            duration = 0,
            effect = () => ExploreContinuesOption(whatToDoNextState),
        };
        ActionOption noWayOption = new ActionOption {
            interactionState = whatToDoNextState,
            cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
            name = "No way.",
            duration = 0,
            effect = () => ExploreEndsOption(whatToDoNextState),
        };

        whatToDoNextState.AddActionOption(yesPleaseOption);
        whatToDoNextState.AddActionOption(noWayOption);
        whatToDoNextState.SetDefaultOption(noWayOption);

        _states.Add(whatToDoNextState.name, whatToDoNextState);

        if (!_hasUsedBaseCreateStates) {
            CreateExploreStates();
        }
    }
    protected void WhatToDoNextState() {
        SetCurrentState(_states["What To Do Next"]);
    }
    protected void LeaveAloneEffect(InteractionState state) {
        state.EndResult();
    }
    protected void SupplyRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " discovered a small cache of Supplies.");
        SetCurrentState(_states[effectName]);
        SupplyRewardEffect(_states[effectName]);
    }
    protected void SupplyRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
    }

    protected void ManaRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " discovered a source of magical energy. We have converted it into a small amount of Mana.");
        SetCurrentState(_states[effectName]);
        ManaRewardEffect(_states[effectName]);
    }
    protected void ManaRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Mana_Cache_Reward_1));
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
    }
    protected void NothingRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has returned with nothing to report.");
        SetCurrentState(_states[effectName]);
        NothingEffect(_states[effectName]);
    }
    protected void NothingEffect(InteractionState state) {
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
    }

    #region End Result Share States and Effects
    protected void DemonDisappearsRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has not returned. We can only assume the worst.");
        SetCurrentState(_states[effectName]);
    }
    protected void DemonDisappearsRewardEffect(InteractionState state) {
        investigatorCharacter.Death();
        //PlayerManager.Instance.player.RemoveMinion(explorerMinion);
    }
    protected void ExploreContinuesRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription("We've instructed " + explorerMinion.name + " to continue its surveillance of the area.");
        SetCurrentState(_states[stateName]);
    }
    protected void ExploreContinuesRewardEffect(InteractionState state) {
        //BaseLandmark landmark = _interactable;
        //landmark.tileLocation.areaOfTile.areaInvestigation.ExploreArea();
    }
    protected void ExploreEndsRewardState(InteractionState state, string stateName) {
        if (investigatorCharacter != null) {
            //_states[stateName].SetDescription("We've instructed " + explorerMinion.name + " to return.");
        }
        SetCurrentState(_states[stateName]);
    }
    protected void ExploreEndsRewardEffect(InteractionState state) {
        if(investigatorCharacter == null) {
            return;
        }
        //if (_interactable is BaseLandmark) {
        //    BaseLandmark landmark = _interactable;
        //    //landmark.landmarkInvestigation.RecallMinion("explore");
        //}
    }
    #endregion
    #endregion

    #region Shared Action Option
    protected void ExploreContinuesOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Continues", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Continues") {
            ExploreContinuesRewardState(state, chosenEffect);
        }
    }
    protected void ExploreEndsOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Ends", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Ends") {
            ExploreEndsRewardState(state, chosenEffect);
        }
    }
    #endregion

    public override string ToString() {
        return name;
    }
}
