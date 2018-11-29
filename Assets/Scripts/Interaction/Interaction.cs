using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ECS;

public class Interaction {
    protected int _id;
    protected string _name;
    protected int _timeOutTicks;
    protected GameDate _timeDate;
    protected INTERACTION_TYPE _type;
    protected BaseLandmark _interactable;
    protected Dictionary<string, InteractionState> _states;
    //protected InteractionItem _interactionItem;
    protected bool _isActivated;
    protected bool _isDone;
    protected bool _hasActivatedTimeOut;
    protected bool _isSecondTimeOutCancelled;
    protected bool _isChosen;
    protected InteractionState _previousState;
    protected InteractionState _currentState;
    //protected Minion _explorerMinion;
    protected Character _characterInvolved;
    protected Action _endInteractionAction;
    protected Job _jobAssociated;
    protected JOB[] _jobFilter;
    protected object[] otherData;

    private bool _hasUsedBaseCreateStates;

    public const int secondTimeOutTicks = 30;

    #region getters/setters
    public INTERACTION_TYPE type {
        get { return _type; }
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
    public Minion explorerMinion {
        get { return _interactable.tileLocation.areaOfTile.areaInvestigation.assignedMinion; }
    }
    public Character characterInvolved {
        get { return _characterInvolved; }
    }
    //public InteractionItem interactionItem {
    //    get { return _interactionItem; }
    //}
    public BaseLandmark interactable {
        get { return _interactable; }
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
    public JOB[] jobFilter {
        get { return _jobFilter; }
    }
    #endregion
    public Interaction(BaseLandmark interactable, INTERACTION_TYPE type, int timeOutTicks) {
        _id = Utilities.SetID(this);
        _type = type;
        _interactable = interactable;
        _timeOutTicks = timeOutTicks;
        //_isFirstTimeOutCancelled = false;
        _hasActivatedTimeOut = false;
        _isSecondTimeOutCancelled = false;
        _hasUsedBaseCreateStates = false;
        _states = new Dictionary<string, InteractionState>();
        //_jobFilter = new JOB[] { JOB.NONE };
        //Debug.Log("Created new interaction " + type.ToString() + " at " + interactable.name);
    }

    #region Virtuals
    public virtual void Initialize() {
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
            _characterInvolved.SetDoNotDisturb(false);
        }
        _interactable.RemoveInteraction(this);
        InteractionUI.Instance.HideInteractionUI();
        if(_endInteractionAction != null) {
            _endInteractionAction();
            _endInteractionAction = null;
        }
        if (_jobAssociated != null) {
            _jobAssociated.SetCreatedInteraction(null);
            SetJobAssociated(null);
        }
    }
    public virtual void OnInteractionActive() {
        _isChosen = true;
        interactable.landmarkVisual.StopInteractionTimer();
        interactable.landmarkVisual.HideInteractionTimer();
        _currentState.CreateLogs();
        _currentState.SetDescription();
    } //this is called when the player clicks the "exclamation point" button and this interaction was chosen
    #endregion

    #region Utilities
    public void SetCurrentState(InteractionState state) {
        _previousState = _currentState;
        if(_currentState != null && _currentState.chosenOption != null) {
            //state.SetAssignedObjects(_currentState.assignedObjects);
            //if (_currentState.chosenOption != null) {
                state.SetAssignedMinion(_currentState.chosenOption.assignedMinion);
                _currentState.OnEndState();
            //}
        }
        _currentState = state;
        _currentState.OnStartState();
        Messenger.Broadcast(Signals.UPDATED_INTERACTION_STATE, this);
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
        _hasActivatedTimeOut = true;
        GameDate timeOutDate = GameManager.Instance.Today();
        timeOutDate.AddHours(secondTimeOutTicks);
        _timeDate = timeOutDate;
        SchedulingManager.Instance.AddEntry(_timeDate, () => SecondTimeOut());

        interactable.landmarkVisual.SetAndStartInteractionTimer(secondTimeOutTicks);
        //interactable.landmarkVisual.ShowInteractionForeground();
        interactable.landmarkVisual.ShowInteractionTimer();
    }
    public void SetEndInteractionAction(Action action) {
        _endInteractionAction = action;
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
            interactable.landmarkVisual.StopInteractionTimer();
            interactable.landmarkVisual.HideInteractionTimer();
            TimedOutRunDefault();
            //_interactable.tileLocation.areaOfTile.areaInvestigation.ExploreArea();
        }
    }
    public void TimedOutRunDefault() {
        if (_currentState == null) {
            throw new Exception(this.GetType().ToString() + " interaction at " + interactable.tileLocation.areaOfTile.name + " has a current state of null at second time out!");
        }
        if(!_currentState.isEnd && _currentState.defaultOption == null) {
            return;
        }
        while (!_isDone) {
            _currentState.ActivateDefault();
        }
    }
    //public void SetExplorerMinion(Minion minion) {
    //    _explorerMinion = minion;
    //    //if(_explorerMinion != null) {
    //    //    _currentState.CreateLogs();
    //    //    _currentState.SetDescription();
    //    //}
    //}
    public void SetCharacterInvolved(Character character) {
        _characterInvolved = character;
        if(_characterInvolved != null) {
            _characterInvolved.SetDoNotDisturb(true);
        }
    }
    public bool AssignedMinionIsOfClass(string className) {
        return this.explorerMinion != null && this.explorerMinion.icharacter.characterClass.className.ToLower() == className.ToLower();
    }
    public bool AssignedMinionIsOfClass(List<string> allowedClassNames) {
        if(this.explorerMinion != null) {
            for (int i = 0; i < allowedClassNames.Count; i++) {
                if(allowedClassNames[i].ToLower() == this.explorerMinion.icharacter.characterClass.className.ToLower()) {
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
        for (int i = 0; i < _jobFilter.Length; i++) {
            if (jobType == _jobFilter[i]) {
                return true;
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
            needsMinion = false,
            effect = () => ExploreContinuesOption(whatToDoNextState),
        };
        ActionOption noWayOption = new ActionOption {
            interactionState = whatToDoNextState,
            cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
            name = "No way.",
            duration = 0,
            needsMinion = false,
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
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }

    protected void ManaRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " discovered a source of magical energy. We have converted it into a small amount of Mana.");
        SetCurrentState(_states[effectName]);
        ManaRewardEffect(_states[effectName]);
    }
    protected void ManaRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Mana_Cache_Reward_1));
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    protected void NothingRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has returned with nothing to report.");
        SetCurrentState(_states[effectName]);
        NothingEffect(_states[effectName]);
    }
    protected void NothingEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }

    #region End Result Share States and Effects
    protected void DemonDisappearsRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has not returned. We can only assume the worst.");
        SetCurrentState(_states[effectName]);
    }
    protected void DemonDisappearsRewardEffect(InteractionState state) {
        explorerMinion.icharacter.Death();
        //PlayerManager.Instance.player.RemoveMinion(explorerMinion);
    }
    protected void ExploreContinuesRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription("We've instructed " + explorerMinion.name + " to continue its surveillance of the area.");
        SetCurrentState(_states[stateName]);
    }
    protected void ExploreContinuesRewardEffect(InteractionState state) {
        BaseLandmark landmark = _interactable;
        landmark.tileLocation.areaOfTile.areaInvestigation.ExploreArea();
    }
    protected void ExploreEndsRewardState(InteractionState state, string stateName) {
        if (explorerMinion != null) {
            //_states[stateName].SetDescription("We've instructed " + explorerMinion.name + " to return.");
        }
        SetCurrentState(_states[stateName]);
    }
    protected void ExploreEndsRewardEffect(InteractionState state) {
        if(explorerMinion == null) {
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
}
