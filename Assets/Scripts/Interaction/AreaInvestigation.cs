using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ECS;

public class AreaInvestigation {
    private Area _area;
    private Minion _assignedMinion;
    private Minion _assignedMinionAttack;
    private bool _isExploring;
    private bool _isAttacking;

    private bool _isMinionRecalledAttack;
    private bool _isMinionRecalledExplore;

    //Explore
    private int _duration;
    private int _currentTick;
    private Interaction _currentInteraction;

    #region getters/setters
    public Area area {
        get { return _area; }
    }
    public Minion assignedMinion {
        get { return _assignedMinion; }
    }
    public Minion assignedMinionAttack {
        get { return _assignedMinionAttack; }
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
    #endregion

    public AreaInvestigation(Area area) {
        _area = area;
        //Messenger.AddListener<Area>(Signals.CLICKED_INTERACTION_BUTTON, ClickedInteractionTimerButton);
    }

    //public void SetAssignedMinion(Minion minion) {
    //    _assignedMinion = minion;
    //}
    //public void SetAssignedMinionAttack(Minion minion) {
    //    _assignedMinionAttack = minion;
    //}
    //public void InvestigateLandmark(Minion minion) {
    //    SetAssignedMinion(minion);
    //    _assignedMinion.SetEnabledState(false);
    //    _assignedMinion.SetExploringArea(_area);

    //    MinionGoToAssignment(ExploreLandmark, "explore");

    //    _isExploring = true;
    //}
    //public void AttackRaidLandmark(string whatTodo, Minion[] minion) {
    //    _assignedMinionAttack = null;
    //    for (int i = 0; i < minion.Length; i++) {
    //        if (minion[i] != null) {
    //            if (_assignedMinionAttack == null) {
    //                SetAssignedMinionAttack(minion[i]);
    //            } else {
    //                _assignedMinionAttack.icharacter.ownParty.AddCharacter(minion[i].icharacter);
    //            }
    //        }
    //    }
    //    _assignedMinionAttack.SetEnabledState(false);
    //    _assignedMinionAttack.SetAttackingArea(_area);
    //    MinionGoToAssignment(AttackLandmark, "attack");
    //    _isAttacking = true;
    //}
    //private void MinionGoToAssignment(Action action, string whatToDo) {
    //    if (whatToDo == "explore") {
    //        _assignedMinion.TravelToAssignment(_area, action);
    //    } else if (whatToDo == "attack") {
    //        _assignedMinionAttack.TravelToAssignment(_area, action);
    //    }
    //}
    //public void RecallMinion(string action) {
    //    if (_isExploring && action == "explore") {
    //        _assignedMinion.TravelBackFromAssignment(() => SetMinionRecallExploreState(false));
    //        _area.landmarkVisual.StopInteractionTimer();
    //        _area.landmarkVisual.HideInteractionTimer();
    //        Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
    //        UnexploreLandmark();
    //        SetMinionRecallExploreState(true);
    //        UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("explore");
    //    }
    //    if (_isAttacking && action == "attack") {
    //        _assignedMinionAttack.TravelBackFromAssignment(() => SetMinionRecallAttackState(false));
    //        UnattackLandmark();
    //        SetMinionRecallAttackState(true);
    //        UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("attack");
    //    }
    //}
    //public void CancelInvestigation(string action) {
    //    if (_isExploring && action == "explore") {
    //        _assignedMinion.SetEnabledState(true);
    //        _area.landmarkVisual.StopInteractionTimer();
    //        _area.landmarkVisual.HideInteractionTimer();
    //        Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
    //        UnexploreLandmark();
    //    }
    //    if (_isAttacking && action == "attack") {
    //        _assignedMinionAttack.SetEnabledState(true);
    //        UnattackLandmark();
    //    }
    //}
    //public void SetMinionRecallExploreState(bool state) {
    //    _isMinionRecalledExplore = state;
    //}
    //public void SetMinionRecallAttackState(bool state) {
    //    _isMinionRecalledAttack = state;
    //}
    //#region Explore
    //public void ExploreLandmark() {

    //    if (_assignedMinion == null) {
    //        return;
    //    }
    //    if (_area.landmarkObj.isRuined) {
    //        return;
    //    }
    //    if (!_area.hasBeenInspected) {
    //        _area.SetHasBeenInspected(true);
    //    }
    //    _area.SetIsBeingInspected(true);
    //    _duration = 30;
    //    _currentTick = 0;
    //    Messenger.AddListener(Signals.HOUR_STARTED, OnExploreTick);
    //    _area.landmarkVisual.SetAndStartInteractionTimer(_duration);
    //    _area.landmarkVisual.ShowNoInteractionForeground();
    //    _area.landmarkVisual.ShowInteractionTimer();
    //}
    //public void UnexploreLandmark() {
    //    if (_area.isBeingInspected) {
    //        _area.SetIsBeingInspected(false);
    //        _area.EndedInspection();
    //    }
    //    _isExploring = false;
    //    _assignedMinion.SetExploringArea(null);
    //    SetAssignedMinion(null);
    //}
    //public void UnattackLandmark() {
    //    _isAttacking = false;
    //    _assignedMinionAttack.SetAttackingArea(null);
    //    SetAssignedMinionAttack(null);
    //}
    //private void OnExploreTick() {
    //    if (_currentTick >= _duration) {
    //        Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
    //        ExploreDoneCheckForExistingEvents();
    //        return;
    //    }
    //    _currentTick++;
    //}
    //public void OnDestroyLandmark() {
    //    if (_isExploring) {
    //        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
    //            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
    //        }
    //        _area.landmarkVisual.StopInteractionTimer();
    //        _area.landmarkVisual.HideInteractionTimer();
    //        if (InteractionUI.Instance.interactionItem.interaction != null && InteractionUI.Instance.interactionItem.interaction.interactable == _area) {
    //            InteractionUI.Instance.HideInteractionUI();
    //        }
    //        RecallMinion("attack");
    //    }
    //}
    //private void ExploreDoneCheckForExistingEvents() {
    //    _area.landmarkVisual.StopInteractionTimer();
    //    if (_area.currentInteractions.Count > 0) {
    //        _currentInteraction = GetRandomInteraction();
    //        _area.landmarkVisual.SetAndStartInteractionTimer(Interaction.secondTimeOutTicks, new InteractionTimer.OnStopTimer(_area.landmarkVisual.HideInteractionTimer));
    //        _area.landmarkVisual.ShowInteractionForeground();
    //    } else {
    //        ExploreLandmark();
    //    }

    //}
    //private Interaction GetRandomInteraction() {
    //    List<Interaction> choices = _area.currentInteractions;
    //    Interaction chosenInteraction = choices[UnityEngine.Random.Range(0, choices.Count)];
    //    chosenInteraction.CancelFirstTimeOut();
    //    chosenInteraction.ScheduleSecondTimeOut();
    //    return chosenInteraction;
    //}
    //private Interaction GetNothingHappenedInteraction() {
    //    Interaction chosenInteraction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.NOTHING_HAPPENED, _area);
    //    chosenInteraction.CancelFirstTimeOut();
    //    chosenInteraction.ScheduleSecondTimeOut();
    //    return chosenInteraction;
    //}
    //private void ClickedInteractionTimerButton(Area area) {
    //    if (_area == area) {
    //        _area.landmarkVisual.StopInteractionTimer();
    //        _currentInteraction.CancelSecondTimeOut();
    //        _currentInteraction.SetExplorerMinion(_assignedMinion);
    //        _currentInteraction.OnInteractionActive();
    //        InteractionUI.Instance.OpenInteractionUI(_currentInteraction);
    //    }
    //}
    //#endregion


    //#region Attack
    //private void AttackLandmark() {
    //    if (_area.defenders != null) {
    //        Combat combat = _assignedMinionAttack.icharacter.currentParty.CreateCombatWith(_area.defenders);
    //        combat.Fight(() => AttackCombatResult(combat));
    //    } else {
    //        _area.DestroyLandmark();
    //    }
    //}
    //private void AttackCombatResult(Combat combat) {
    //    if (_isAttacking) { //when the minion dies, isActivated will become false, hence, it must not go through the result
    //        if (combat.winningSide == _assignedMinionAttack.icharacter.currentSide) {
    //            _area.DestroyLandmark();
    //        }
    //    }
    //}
    //#endregion
}
