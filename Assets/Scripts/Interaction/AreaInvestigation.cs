using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ECS;

public class AreaInvestigation {
    private Area _area;
    private Minion _assignedMinion;
    private Minion _assignedMinionAttack;
    private BaseLandmark _currentlyExploredLandmark;
    private BaseLandmark _currentlyAttackedLandmark;
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
        Messenger.AddListener<BaseLandmark>(Signals.CLICKED_INTERACTION_BUTTON, ClickedInteractionTimerButton);
    }

    public void SetAssignedMinion(Minion minion) {
        _assignedMinion = minion;
    }
    public void SetAssignedMinionAttack(Minion minion) {
        _assignedMinionAttack = minion;
    }
    public void InvestigateLandmark(Minion minion) {
        SetAssignedMinion(minion);
        _assignedMinion.SetEnabledState(false);
        _assignedMinion.SetExploringArea(_area);

        MinionGoToAssignment(ExploreArea, "explore");

        _isExploring = true;
    }
    public void AttackRaidLandmark(string whatTodo, Minion[] minion, BaseLandmark targetLandmark) {
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
        _assignedMinionAttack.SetAttackingArea(_area);
        _currentlyAttackedLandmark = targetLandmark;
        MinionGoToAssignment(AttackLandmark, "attack");
        _isAttacking = true;
    }
    private void MinionGoToAssignment(Action action, string whatToDo) {
        if (whatToDo == "explore") {
            _assignedMinion.TravelToAssignment(_area.coreTile.landmarkOnTile, action);
        } else if (whatToDo == "attack") {
            _assignedMinionAttack.TravelToAssignment(_currentlyAttackedLandmark, action);
        }
    }
    public void RecallMinion(string action) {
        if (_isExploring && action == "explore") {
            _assignedMinion.TravelBackFromAssignment(() => SetMinionRecallExploreState(false));
            _area.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimer();
            _area.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimer();
            if (_currentlyExploredLandmark != null) {
                _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
                _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
            }
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
            _area.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimer();
            _area.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimer();
            if(_currentlyExploredLandmark != null) {
                _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
                _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
            }
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            UnexploreLandmark();
        }
        if (_isAttacking && action == "attack") {
            _assignedMinionAttack.SetEnabledState(true);
            UnattackLandmark();
        }
    }
    public void SetMinionRecallExploreState(bool state) {
        _isMinionRecalledExplore = state;
    }
    public void SetMinionRecallAttackState(bool state) {
        _isMinionRecalledAttack = state;
    }
    #region Explore
    public void ExploreArea() {

        if (_assignedMinion == null) {
            return;
        }
        //if (_area.landmarkObj.isRuined) {
        //    return;
        //}
        if (!_area.hasBeenInspected) {
            _area.SetHasBeenInspected(true);
        }
        //_area.SetIsBeingInspected(true);

        _duration = 30;
        _currentTick = 0;
        Messenger.AddListener(Signals.HOUR_STARTED, OnExploreTick);
        if (_currentlyExploredLandmark != null) {
            _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
            _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
        }
        _area.coreTile.landmarkOnTile.landmarkVisual.SetAndStartInteractionTimer(_duration);
        _area.coreTile.landmarkOnTile.landmarkVisual.ShowNoInteractionForeground();
        _area.coreTile.landmarkOnTile.landmarkVisual.ShowInteractionTimer();
    }
    public void UnexploreLandmark() {
        //if (_area.isBeingInspected) {
        //    _area.SetIsBeingInspected(false);
        //    _area.EndedInspection();
        //}
        _isExploring = false;
        _assignedMinion.SetExploringArea(null);
        SetAssignedMinion(null);
        _currentlyExploredLandmark = null;
    }
    public void UnattackLandmark() {
        _isAttacking = false;
        _assignedMinionAttack.SetAttackingArea(null);
        SetAssignedMinionAttack(null);
        _currentlyAttackedLandmark = null;
    }
    private void OnExploreTick() {
        if (_currentTick >= _duration) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            ExploreDoneCheckForExistingEvents();
            return;
        }
        _currentTick++;
    }
    public void OnDestroyLandmark(BaseLandmark landmark) {
        if (_isExploring) {
            if (_currentlyExploredLandmark == landmark) {
                _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
                _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
                if (InteractionUI.Instance.interactionItem.interaction != null && InteractionUI.Instance.interactionItem.interaction.interactable == landmark) {
                    InteractionUI.Instance.HideInteractionUI();
                }
            }
            if (_area.areAllLandmarksDead) {
                if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                    Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
                }
                _area.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimer();
                _area.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimer();
            }
        }
    }
    private void ExploreDoneCheckForExistingEvents() {
        _area.coreTile.landmarkOnTile.landmarkVisual.StopInteractionTimer();
        _area.coreTile.landmarkOnTile.landmarkVisual.HideInteractionTimer();

        List<Interaction> choices = new List<Interaction>();
        for (int i = 0; i < _area.landmarks.Count; i++) {
            choices.AddRange(_area.landmarks[i].currentInteractions);
        }
        if (choices.Count > 0) {
            _currentInteraction = GetRandomInteraction(choices);
            _currentlyExploredLandmark = _currentInteraction.interactable as BaseLandmark;
            _currentlyExploredLandmark.landmarkVisual.SetAndStartInteractionTimer(Interaction.secondTimeOutTicks, new InteractionTimer.OnStopTimer(_currentlyExploredLandmark.landmarkVisual.HideInteractionTimer));
            _currentlyExploredLandmark.landmarkVisual.ShowInteractionForeground();
            _currentlyExploredLandmark.landmarkVisual.ShowInteractionTimer();
        } else {
            ExploreArea();
        }

    }
    private Interaction GetRandomInteraction(List<Interaction> choices) {
        Interaction chosenInteraction = choices[UnityEngine.Random.Range(0, choices.Count)];
        chosenInteraction.CancelFirstTimeOut();
        chosenInteraction.ScheduleSecondTimeOut();
        return chosenInteraction;
    }
  
    private void ClickedInteractionTimerButton(BaseLandmark landmark) {
        if (_currentlyExploredLandmark == landmark) {
            _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
            _currentInteraction.CancelSecondTimeOut();
            _currentInteraction.SetExplorerMinion(_assignedMinion);
            _currentInteraction.OnInteractionActive();
            InteractionUI.Instance.OpenInteractionUI(_currentInteraction);
        }
    }
    #endregion


    #region Attack
    private void AttackLandmark() {
        //_currentlyAttackedLandmark = null;
        //float highestWinRate = 0f;
        //float loseRate = 0f;
        //for (int i = 0; i < _area.exposedTiles.Count; i++) {
        //    if(_currentlyAttackedLandmark == null) {
        //        _currentlyAttackedLandmark = _area.exposedTiles[i];
        //        CombatManager.Instance.GetCombatChanceOfTwoLists(_assignedMinionAttack.icharacter.currentParty.icharacters, _currentlyAttackedLandmark.defenders.icharacters, out highestWinRate, out loseRate);
        //    } else {
        //        float winRate = 0f;
        //        CombatManager.Instance.GetCombatChanceOfTwoLists(_assignedMinionAttack.icharacter.currentParty.icharacters, _area.exposedTiles[i].defenders.icharacters, out winRate, out loseRate);
        //        if(winRate > highestWinRate) {
        //            _currentlyAttackedLandmark = _area.exposedTiles[i];
        //            highestWinRate = winRate;
        //        }
        //    }
        //}
        if (_currentlyAttackedLandmark.defenders != null) {
            //_assignedMinionAttack.icharacter.currentParty.specificLocation.RemoveCharacterFromLocation(_assignedMinionAttack.icharacter.currentParty);
            //_currentlyAttackedLandmark.AddCharacterToLocation(_assignedMinionAttack.icharacter.currentParty);
            Combat combat = _assignedMinionAttack.icharacter.currentParty.CreateCombatWith(_currentlyAttackedLandmark.defenders);
            combat.Fight(() => AttackCombatResult(combat));
        } else {
            _currentlyAttackedLandmark.DestroyLandmark();
            RecallMinion("attack");
        }
    }
    private void AttackCombatResult(Combat combat) {
        if (_isAttacking) { //when the minion dies, isActivated will become false, hence, it must not go through the result
            if (combat.winningSide == _assignedMinionAttack.icharacter.currentSide) {
                _currentlyAttackedLandmark.DestroyLandmark();
                RecallMinion("attack");
            }
        }
    }
    #endregion
}
