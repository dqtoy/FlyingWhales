using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ECS;

public class LandmarkInvestigation {

    private BaseLandmark _landmark;
    private Minion _assignedMinion;
    private Minion _assignedMinionAttack;
    //private Party _assignedPartyMinion;
    private bool _isActivated;
    private bool _isExploring;
    private bool _isAttacking;

    private bool _isMinionRecalledAttack;
    private bool _isMinionRecalledExplore;
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
    public Minion assignedMinionAttack {
        get { return _assignedMinionAttack; }
    }
    //public Party assignedPartyMinion {
    //    get { return _assignedPartyMinion; }
    //}
    public bool isActivated {
        get { return _isActivated; }
    }
    public bool isExploring {
        get { return _isExploring; }
    }
    public bool isAttacking {
        get { return _isAttacking; }
    }
    public bool isMinionRecalledAttack {
        get { return _isMinionRecalledAttack; }
    }
    public bool isMinionRecalledExplore {
        get { return _isMinionRecalledExplore; }
    }
    public string whatToDo {
        get { return _whatToDo; }
    }
    #endregion

    public LandmarkInvestigation(BaseLandmark landmark) {
        _landmark = landmark;
        //_assignedPartyMinion = null;
        Messenger.AddListener<BaseLandmark>(Signals.CLICKED_INTERACTION_BUTTON, ClickedInteractionTimerButton);
    }

    public void SetAssignedMinion(Minion minion) {
        _assignedMinion = minion;
    }
    public void SetAssignedMinionAttack(Minion minion) {
        _assignedMinionAttack = minion;
    }
    //public void SetAssignedMinionParty(Party party) {
    //    _assignedPartyMinion = party;
    //}
    public void InvestigateLandmark(Minion minion) {
        SetAssignedMinion(minion);
        _assignedMinion.SetEnabledState(false);
        //_assignedMinion.SetExploringLandmark(_landmark);
        
        MinionGoToAssignment(ExploreLandmark, "explore");

        _isExploring = true;
        SetActivatedState(true);
    }
    public void AttackRaidLandmark(string whatTodo, Minion[] minion) {
        _assignedMinionAttack = null;
        for (int i = 0; i < minion.Length; i++) {
            if (minion[i] != null) {
                if (_assignedMinionAttack == null) {
                    SetAssignedMinionAttack(minion[i]);
                } else {
                    _assignedMinionAttack.icharacter.ownParty.AddCharacter(minion[i].icharacter);
                }
            }
        }
        _assignedMinionAttack.SetEnabledState(false);
        //_assignedMinionAttack.SetExploringLandmark(_landmark);
        MinionGoToAssignment(AttackLandmark, "attack");
        _isAttacking = true;
        SetActivatedState(true);
    }
    private void MinionGoToAssignment(Action action, string whatToDo) {
        if (whatToDo == "explore") {
            _assignedMinion.TravelToAssignment(_landmark, action);
        } else if (whatToDo == "attack") {
            _assignedMinionAttack.TravelToAssignment(_landmark, action);
        }
    }
    //private void MinionGoBackFromAssignment(Action action) {
    //    if (_assignedMinion.icharacter.currentParty.icon.isTravelling) {
    //        _assignedMinion.icharacter.currentParty.CancelTravel(() => action());
    //    } else {
    //        _assignedMinion.icharacter.currentParty.GoToLocation(PlayerManager.Instance.player.demonicPortal, PATHFINDING_MODE.PASSABLE, () => action());
    //    }
    //}
    public void RecallMinion(string action) {
        if (_isExploring && action == "explore") {
            _assignedMinion.TravelBackFromAssignment(() => SetMinionRecallExploreState(false));
            _landmark.landmarkVisual.StopInteractionTimer();
            _landmark.landmarkVisual.HideInteractionTimer();
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            UnexploreLandmark();
            SetMinionRecallExploreState(true);
            UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("explore");
        }
        if (_isAttacking && action == "attack") {
            _assignedMinionAttack.TravelBackFromAssignment(() => SetMinionRecallAttackState(false));
            UnattackLandmark();
            SetMinionRecallAttackState(true);
            UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("attack");
        }
    }
    public void CancelInvestigation(string action) {
        if (_isExploring && action == "explore") {
            _assignedMinion.SetEnabledState(true);
            _landmark.landmarkVisual.StopInteractionTimer();
            _landmark.landmarkVisual.HideInteractionTimer();
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            UnexploreLandmark();
        }
        if (_isAttacking && action == "attack") {
            _assignedMinionAttack.SetEnabledState(true);
            UnattackLandmark();
        }
    }
    public void SetActivatedState(bool state) {
        _isActivated = state;
        //Messenger.Broadcast(Signals.LANDMARK_INVESTIGATION_ACTIVATED, _landmark);
    }
    public void SetMinionRecallExploreState(bool state) {
        _isMinionRecalledExplore = state;
    }
    public void SetMinionRecallAttackState(bool state) {
        _isMinionRecalledAttack = state;
    }
    #region Explore
    public void ExploreLandmark() {

        if(_assignedMinion == null) {
            return;
        }
        if (_landmark.landmarkObj.isRuined) {
            return;
        }
        if (!_landmark.hasBeenInspected) {
            _landmark.SetHasBeenInspected(true);
        }
        _landmark.SetIsBeingInspected(true);
        _duration = 30;
        _currentTick = 0;
        Messenger.AddListener(Signals.HOUR_STARTED, OnExploreTick);
        _landmark.landmarkVisual.SetAndStartInteractionTimer(_duration);
        //_landmark.landmarkVisual.ShowNoInteractionForeground();
        _landmark.landmarkVisual.ShowInteractionTimer(_currentInteraction);
    }
    public void UnexploreLandmark() {
        if (_landmark.isBeingInspected) {
            _landmark.SetIsBeingInspected(false);
            _landmark.EndedInspection();
        }
        _isExploring = false;
        _assignedMinion.SetExploringArea(null);
        SetAssignedMinion(null);
        UninvestigateLandmark();
    }
    public void UnattackLandmark() {
        _isAttacking = false;
        _assignedMinionAttack.SetAttackingArea(null);
        SetAssignedMinionAttack(null);
        UninvestigateLandmark();
    }
    public void UninvestigateLandmark() {
        SetActivatedState(false);
        _whatToDo = string.Empty;
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
        if (_isActivated) {
            if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            }
            _landmark.landmarkVisual.StopInteractionTimer();
            _landmark.landmarkVisual.HideInteractionTimer();
            if(InteractionUI.Instance.interactionItem.interaction != null && InteractionUI.Instance.interactionItem.interaction.interactable == _landmark) {
                InteractionUI.Instance.HideInteractionUI();
            }
            RecallMinion("attack");
        }
    }
    private void ExploreDoneCheckForExistingEvents() {
        _landmark.landmarkVisual.StopInteractionTimer();
        if(_landmark.currentInteractions.Count > 0) {
            _currentInteraction = GetRandomInteraction();
            _landmark.landmarkVisual.SetAndStartInteractionTimer(Interaction.secondTimeOutTicks, new InteractionTimer.OnStopTimer(_landmark.landmarkVisual.HideInteractionTimer));
            //_landmark.landmarkVisual.ShowInteractionForeground();
        } else {
            ExploreLandmark();
        }

    }
    private Interaction GetRandomInteraction() {
        //GameManager.Instance.SetPausedState(true);
        List<Interaction> choices = _landmark.currentInteractions;
        Interaction chosenInteraction = choices[UnityEngine.Random.Range(0, choices.Count)];
        //chosenInteraction.CancelFirstTimeOut();
        chosenInteraction.ScheduleSecondTimeOut();
        return chosenInteraction;
        //Popup interaction
    }
    private Interaction GetNothingHappenedInteraction() {
        Interaction chosenInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.NOTHING_HAPPENED, _landmark);
        //chosenInteraction.CancelFirstTimeOut();
        chosenInteraction.ScheduleSecondTimeOut();
        return chosenInteraction;
    }
    private void ClickedInteractionTimerButton(BaseLandmark landmark) {
        if(_landmark == landmark) {
            _landmark.landmarkVisual.StopInteractionTimer();
            //_landmark.landmarkVisual.HideInteractionTimer();
            _currentInteraction.CancelSecondTimeOut();
            //_currentInteraction.SetExplorerMinion(_assignedMinion);
            _currentInteraction.OnInteractionActive();
            InteractionUI.Instance.OpenInteractionUI(_currentInteraction);
        }
    }
    #endregion


    #region Attack
    private void AttackLandmark() {
        //if (_landmark.defenders != null) {
        //    Combat combat = _assignedMinionAttack.icharacter.currentParty.CreateCombatWith(_landmark.defenders);
        //    combat.Fight(() => AttackCombatResult(combat));
        //    //combat.AddAfterCombatAction(() => AttackCombatResult(combat));
        //} else {
        //    _landmark.DestroyLandmark();
        //}
    }
    private void AttackCombatResult(Combat combat) {
        if (_isActivated) { //when the minion dies, isActivated will become false, hence, it must not go through the result
            if (combat.winningSide == _assignedMinionAttack.icharacter.currentSide) {
                _landmark.DestroyLandmark();
            }
        }
    }
    #endregion

    #region Raid
    private void RaidLandmark() {
        //if (_landmark.defenders != null) {
        //    Combat combat = _assignedMinion.icharacter.currentParty.CreateCombatWith(_landmark.defenders);
        //    combat.Fight(() => RaidCombatResult(combat));
        //    //combat.AddAfterCombatAction(() => RaidCombatResult(combat));
        //} else {
        //    RaidAndGoBack();
        //}
    }
    private void RaidCombatResult(Combat combat) {
        if (_isActivated) { //when the minion dies, isActivated will become false, hence, it must not go through the result
            if (combat.winningSide == _assignedMinion.icharacter.currentSide) {
                RaidAndGoBack();
            }
        }
    }
    private void RaidAndGoBack() {
        int amountToRaid = (int) (_landmark.tileLocation.areaOfTile.suppliesInBank * ((UnityEngine.Random.Range(25, 76)) / 100f));
        _landmark.tileLocation.areaOfTile.AdjustSuppliesInBank(-amountToRaid);
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, amountToRaid);
        RecallMinion("attack");
    }
    #endregion
}
