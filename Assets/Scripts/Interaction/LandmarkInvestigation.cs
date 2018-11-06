using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ECS;

public class LandmarkInvestigation {

    private BaseLandmark _landmark;
    private Minion _assignedMinion;
    private bool _isActivated;
    private bool _isMinionRecalled;
    private string _whatToDo;

    //Explore
    private int _duration;
    private int _currentTick;
    private Interaction _currentInteraction;

    #region getters/setters
    public BaseLandmark landmark {
        get { return _landmark; }
    }
    public Minion assignedMinion {
        get { return _assignedMinion; }
    }
    public bool isActivated {
        get { return _isActivated; }
    }
    public bool isMinionRecalled {
        get { return _isMinionRecalled; }
    }
    public string whatToDo {
        get { return _whatToDo; }
    }
    #endregion

    public LandmarkInvestigation(BaseLandmark landmark) {
        _landmark = landmark;
        Messenger.AddListener<BaseLandmark>(Signals.CLICKED_INTERACTION_BUTTON, ClickedInteractionTimerButton);
    }

    public void SetAssignedMinion(Minion minion) {
        _assignedMinion = minion;
    }
    public void InvestigateLandmark(string whatTodo) {
        _assignedMinion.SetEnabledState(false);
        if (whatTodo == "explore") {
            MinionGoToAssignment(ExploreLandmark);
        } else if (whatTodo == "attack") {
            MinionGoToAssignment(AttackLandmark);
        } else if (whatTodo == "raid") {
            MinionGoToAssignment(RaidLandmark);
        }
        _whatToDo = whatTodo;
        SetActivatedState(true);
    }
    public void UninvestigateLandmark() {
        _assignedMinion.SetEnabledState(true);
        SetAssignedMinion(null);
        _isMinionRecalled = false;
        SetActivatedState(false);
        _whatToDo = string.Empty;
    }
    private void MinionGoToAssignment(Action action) {
        _assignedMinion.icharacter.currentParty.GoToLocation(_landmark, PATHFINDING_MODE.PASSABLE, () => action());
    }
    public void MinionGoBackFromAssignment(Action action) {
        if (_assignedMinion.icharacter.currentParty.icon.isTravelling) {
            _assignedMinion.icharacter.currentParty.CancelTravel(() => action());
        } else {
            _assignedMinion.icharacter.currentParty.GoToLocation(PlayerManager.Instance.player.demonicPortal, PATHFINDING_MODE.PASSABLE, () => action());
        }
    }
    public void RecallMinion() {
        if (_landmark.isBeingInspected) {
            _landmark.landmarkVisual.StopInteractionTimer();
            _landmark.landmarkVisual.HideInteractionTimer();
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            MinionGoBackFromAssignment(UnexploreLandmark);
        } else {
            MinionGoBackFromAssignment(UninvestigateLandmark);
        }
        _isMinionRecalled = true;
    }
    public void SetActivatedState(bool state) {
        _isActivated = state;
        Messenger.Broadcast(Signals.LANDMARK_INVESTIGATION_ACTIVATED, _landmark);
    }

    #region Explore
    public void ExploreLandmark() {
        _landmark.SetIsBeingInspected(true);
        if (!_landmark.hasBeenInspected) {
            _landmark.SetHasBeenInspected(true);
        }
        _duration = 30;
        _currentTick = 0;
        Messenger.AddListener(Signals.HOUR_STARTED, OnExploreTick);
        _landmark.landmarkVisual.SetAndStartInteractionTimer(_duration);
        _landmark.landmarkVisual.SetInteractionTimerButtonState(false);
        _landmark.landmarkVisual.ShowInteractionTimer();
    }
    public void UnexploreLandmark() {
        if (_landmark.isBeingInspected) {
            _landmark.SetIsBeingInspected(false);
            _landmark.EndedInspection();
        }
        UninvestigateLandmark();
    }
    private void OnExploreTick() {
        if(_currentTick >= _duration) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            ExploreDoneCheckForExistingEvents();
            return;
        }
        _currentTick++;
        //Update Timer Progress
    }
    public void OnDestroyLandmark() {
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
        }
        _landmark.landmarkVisual.StopInteractionTimer();
        _landmark.landmarkVisual.HideInteractionTimer();
        InteractionUI.Instance.HideInteractionUI();
        MinionGoBackFromAssignment(UnexploreLandmark);
    }
    private void ExploreDoneCheckForExistingEvents() {
        _landmark.landmarkVisual.StopInteractionTimer();
        if(_landmark.currentInteractions.Count > 0) {
            _currentInteraction = GetRandomInteraction();
            _landmark.landmarkVisual.SetAndStartInteractionTimer(Interaction.secondTimeOutTicks, new InteractionTimer.OnStopTimer(_landmark.landmarkVisual.HideInteractionTimer));
            _landmark.landmarkVisual.SetInteractionTimerButtonState(true);
        } else {
            ExploreLandmark();
        }

    }
    private Interaction GetRandomInteraction() {
        //GameManager.Instance.SetPausedState(true);
        Interaction chosenInteraction = _landmark.currentInteractions[UnityEngine.Random.Range(0, _landmark.currentInteractions.Count)];
        chosenInteraction.CancelFirstTimeOut();
        chosenInteraction.ScheduleSecondTimeOut();
        return chosenInteraction;
        //Popup interaction
    }
    private Interaction GetNothingHappenedInteraction() {
        Interaction chosenInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.NOTHING_HAPPENED, _landmark);
        chosenInteraction.CancelFirstTimeOut();
        chosenInteraction.ScheduleSecondTimeOut();
        return chosenInteraction;
    }
    private void ClickedInteractionTimerButton(BaseLandmark landmark) {
        if(_landmark == landmark) {
            _landmark.landmarkVisual.StopInteractionTimer();
            //_landmark.landmarkVisual.HideInteractionTimer();
            _currentInteraction.CancelSecondTimeOut();
            _currentInteraction.SetExplorerMinion(_assignedMinion);
            InteractionUI.Instance.OpenInteractionUI(_currentInteraction);
        }
    }
    #endregion


    #region Attack
    private void AttackLandmark() {
        if (_landmark.defenders != null) {
            Combat combat = _assignedMinion.icharacter.currentParty.StartCombatWith(_landmark.defenders);
            combat.AddAfterCombatAction(() => AttackCombatResult(combat));
        } else {
            _landmark.DestroyLandmark();
        }
    }
    private void AttackCombatResult(Combat combat) {
        if(combat.winningSide == _assignedMinion.icharacter.currentSide) {
            _landmark.DestroyLandmark();
        }
    }
    #endregion

    #region Raid
    private void RaidLandmark() {
        if (_landmark.defenders != null) {
            Combat combat = _assignedMinion.icharacter.currentParty.StartCombatWith(_landmark.defenders);
            combat.AddAfterCombatAction(() => RaidCombatResult(combat));
        } else {
            RaidAndGoBack();
        }
    }
    private void RaidCombatResult(Combat combat) {
        if (combat.winningSide == _assignedMinion.icharacter.currentSide) {
            RaidAndGoBack();
        }
    }
    private void RaidAndGoBack() {
        int amountToRaid = (int) (_landmark.tileLocation.areaOfTile.suppliesInBank * ((UnityEngine.Random.Range(25, 76)) / 100f));
        _landmark.tileLocation.areaOfTile.AdjustSuppliesInBank(-amountToRaid);
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, amountToRaid);
        MinionGoBackFromAssignment(UninvestigateLandmark);
    }
    #endregion

    //#region Overrides
    //public override void CreateStates() {
    //    InteractionState uninvestigatedState = new InteractionState("Uninvestigated", this);
    //    InteractionState investigatedState = new InteractionState("Investigated", this);
    //    InteractionState attackLandmarkState = new InteractionState("Attack Landmark", this);
    //    InteractionState raidLandmarkState = new InteractionState("Raid Landmark", this);

    //    if (_interactable is BaseLandmark) {
    //        BaseLandmark landmark = _interactable as BaseLandmark;
    //        string landmarkType = Utilities.NormalizeStringUpperCaseFirstLetters(landmark.specificLandmarkType.ToString());
    //        string uninvestigatedDesc = "This is a/an " + landmarkType.ToLower() + ". We must send an imp to gather further information about this place.";
    //        string investigatedDesc = "This is a/an " + landmarkType.ToLower() + ". We have an imp observing the place. You may recall the imp at any moment.";

    //        if (landmark.owner != null && landmark.owner.leader != null) {
    //            string landmarkOwner = Utilities.GetNormalizedRaceAdjective(landmark.owner.leader.race);
    //            uninvestigatedDesc = "This is a/an " + landmarkOwner.ToLower() + " " + landmarkType.ToLower() + ". We must send an imp to gather further information about this place.";
    //            investigatedDesc = "This is a/an " + landmarkOwner.ToLower() + " " + landmarkType.ToLower() + ". We have an imp observing the place. You may recall the imp at any moment.";
    //        }

    //        uninvestigatedState.SetDescription(uninvestigatedDesc);
    //        investigatedState.SetDescription(investigatedDesc);

    //        CreateActionOptions(uninvestigatedState);
    //        CreateActionOptions(investigatedState);
    //        CreateActionOptions(attackLandmarkState);
    //        CreateActionOptions(raidLandmarkState);

    //    }
    //    _states.Add(uninvestigatedState.name, uninvestigatedState);
    //    _states.Add(investigatedState.name, investigatedState);
    //    _states.Add(attackLandmarkState.name, attackLandmarkState);
    //    _states.Add(raidLandmarkState.name, raidLandmarkState);

    //    SetCurrentState(uninvestigatedState);
    //}
    //public override void CreateActionOptions(InteractionState state) {
    //    if(state.name == "Uninvestigated") {
    //        ActionOption investigateOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 1, currency = CURRENCY.IMP },
    //            name = "Send an Imp.",
    //            duration = 1,
    //            needsMinion = false,
    //            effect = () => InvestigatedState()
    //        };
    //        ActionOption attackOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
    //            name = "Attack it.",
    //            duration = 10,
    //            needsMinion = true,
    //            neededObjects = new List<System.Type>() { typeof(Minion) },
    //            effect = () => AttackItState(state)
    //        };
    //        ActionOption raidOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
    //            name = "Raid it.",
    //            duration = 10,
    //            needsMinion = true,
    //            neededObjects = new List<System.Type>() { typeof(Minion) },
    //            effect = () => RaidItState(state),
    //            canBeDoneAction = CanBeRaided,
    //        };
    //        state.AddActionOption(investigateOption);
    //        state.AddActionOption(attackOption);
    //        state.AddActionOption(raidOption);
    //    } else if (state.name == "Investigated") {
    //        ActionOption uninvestigateOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 0, currency = CURRENCY.IMP },
    //            name = "Recall an Imp.",
    //            duration = 1,
    //            needsMinion = false,
    //            effect = () => UninvestigatedState()
    //        };
    //        ActionOption attackOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
    //            name = "Attack it.",
    //            duration = 10,
    //            needsMinion = true,
    //            neededObjects = new List<System.Type>() { typeof(Minion) },
    //            effect = () => AttackItState(state)
    //        };
    //        ActionOption raidOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
    //            name = "Raid it.",
    //            duration = 10,
    //            needsMinion = true,
    //            neededObjects = new List<System.Type>() { typeof(Minion) },
    //            effect = () => RaidItState(state),
    //            canBeDoneAction = CanBeRaided,

    //        };
    //        state.AddActionOption(uninvestigateOption);
    //        state.AddActionOption(attackOption);
    //        state.AddActionOption(raidOption);
    //    } else if (state.name == "Attack Landmark") {
    //        ActionOption okayOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
    //            name = "Okay.",
    //            duration = 1,
    //            needsMinion = false,
    //            effect = () => OkayState(state)
    //        };
    //        state.AddActionOption(okayOption);
    //        state.SetDefaultOption(okayOption);
    //    } else if (state.name == "Raid Landmark") {
    //        ActionOption okayOption = new ActionOption {
    //            interactionState = state,
    //            cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
    //            name = "Okay.",
    //            duration = 1,
    //            needsMinion = false,
    //            effect = () => OkayState(state),
    //        };
    //        state.AddActionOption(okayOption);
    //        state.SetDefaultOption(okayOption);
    //    }
    //}
    //#endregion
    //private void AttackItState(InteractionState state) {
    //    AttackLandmarkState(state, "Attack Landmark");
    //}
    //private void RaidItState(InteractionState state) {
    //    RaidLandmarkState(state, "Raid Landmark");
    //}
    //private void OkayState(InteractionState state) {
    //    if (_interactable.isBeingInspected) {
    //        SetCurrentState(_states["Investigated"]);
    //    } else {
    //        SetCurrentState(_states["Uninvestigated"]);
    //    }
    //    state.AssignedMinionGoesBack();
    //}
    //private void AttackLandmarkState(InteractionState state, string effectName) {
    //    _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " has been sent to attack " + _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkName);
    //    SetCurrentState(_states[effectName]);
    //    AttackLandmarkEffect(_states[effectName]);
    //}
    //private void AttackLandmarkEffect(InteractionState state) {
    //    CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
    //    state.assignedMinion.icharacter.currentParty.iactionData.AssignAction(characterAction, _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkObj);
    //    state.assignedMinion.icharacter.currentParty.currentAction.SetOnEndAction(() => state.ActivateDefault());
    //}
    //private void RaidLandmarkState(InteractionState state, string effectName) {
    //    _states[effectName].SetDescription(state.chosenOption.assignedMinion.icharacter.name + " has been sent to " + _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkName + " to raid it for supplies");
    //    SetCurrentState(_states[effectName]);
    //    RaidLandmarkEffect(_states[effectName]);
    //}
    //private void RaidLandmarkEffect(InteractionState state) {
    //    CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
    //    state.assignedMinion.icharacter.currentParty.iactionData.AssignAction(characterAction, _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkObj);
    //    state.assignedMinion.icharacter.currentParty.currentAction.SetOnEndAction(() => state.ActivateDefault());
    //}
    //private bool CanBeRaided() {
    //    if (_interactable is BaseLandmark) {
    //        BaseLandmark landmark = _interactable as BaseLandmark;
    //        return !landmark.isRaided;
    //    }
    //    return false;
    //}
}
