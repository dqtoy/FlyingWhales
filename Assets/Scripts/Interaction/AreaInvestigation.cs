using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class AreaInvestigation {
    private Area _area;
    private Minion _assignedMinion;
    private Minion _assignedMinionAttack;
    private Minion _assignedTokeneerMinion;
    private BaseLandmark _currentlyExploredLandmark;
    private BaseLandmark _currentlyAttackedLandmark;
    private CombatSlot[] _combatSlots;
    private bool _isExploring;
    private bool _isAttacking;
    private bool _isCollectingTokens;
    private bool _isMinionRecalledAttack;
    private bool _isMinionRecalledExplore;
    private bool _isMinionRecalledCollect;

    //Explore
    private int _duration;
    private int _currentTick;
    private Interaction _currentInteraction;

    //Token Collection
    public Minion tokenCollector { get; private set; }

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
    public CombatSlot[] combatSlots {
        get { return _combatSlots; }
    }
    //public Minion tokenCollector {
    //    get { return _assignedTokeneerMinion; }
    //}
    public bool isExploring {
        get { return _isExploring; }
    }
    public bool isAttacking {
        get { return _isAttacking; }
    }
    public bool isCollectingTokens {
        get { return _isCollectingTokens; }
    }
    public bool isMinionRecalledAttack {
        get { return _isMinionRecalledAttack; }
    }
    public bool isMinionRecalledExplore {
        get { return _isMinionRecalledExplore; }
    }
    public bool isMinionRecalledCollect {
        get { return _isMinionRecalledCollect; }
    }
    public bool isActivelyCollectingToken {
        get { return IsActivelyCollectingTokens(); }
    }
    #endregion

    public AreaInvestigation(Area area) {
        _area = area;
        _combatSlots = new CombatSlot[4];
        for (int i = 0; i < _combatSlots.Length; i++) {
            _combatSlots[i] = new CombatSlot() { gridNumber = i };
        }
        //Messenger.AddListener<BaseLandmark>(Signals.CLICKED_INTERACTION_BUTTON, ClickedInteractionTimerButton);
        Messenger.AddListener<Area>(Signals.AREA_OCCUPANY_CHANGED, OnAreaOccupancyChanged);
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
    public void AttackRaidLandmark(string whatTodo, CombatGrid combatGrid, BaseLandmark targetLandmark) {
        _assignedMinionAttack = null;
        for (int i = 0; i < _combatSlots.Length; i++) {
            _combatSlots[i].character = combatGrid.slots[i].character;
            if (_combatSlots[i].character != null) {
                if (_assignedMinionAttack == null) {
                    SetAssignedMinionAttack(_combatSlots[i].character.minion);
                } else {
                    _assignedMinionAttack.character.ownParty.AddCharacter(_combatSlots[i].character);
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
        } else if (whatToDo == "collect") {
            tokenCollector.TravelToAssignment(_area.coreTile.landmarkOnTile, action);
        }
    }
    public void RecallMinion(string action) {
        if (_isExploring && action == "explore") {
            _assignedMinion.TravelBackFromAssignment(() => SetMinionRecallExploreState(false));
            _assignedMinion.character.job.StopJobAction();
            _assignedMinion.character.job.StopCreatedInteraction();
            _assignedMinion.character.job.SetToken(null);
            //Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            UnexploreLandmark();
            SetMinionRecallExploreState(true);
            //UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("explore");
        }
        if (_isAttacking && action == "attack") {
            _assignedMinionAttack.TravelBackFromAssignment(() => SetMinionRecallAttackState(false));
            UnattackLandmark();
            SetMinionRecallAttackState(true);
            //UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("attack");
        }
        if (_isCollectingTokens && action == "collect") {
            tokenCollector.TravelBackFromAssignment(() => SetMinionRecallCollectState(false));
            SetMinionRecallCollectState(true);
            StopTokenCollection();
            //UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("collect");
        }
    }
    public void CancelInvestigation(string action) {
        if (_isExploring && action == "explore") {
            _assignedMinion.SetEnabledState(true);
            _assignedMinion.character.job.StopJobAction();
            //character.job.StopCreatedInteraction();
            if (!_assignedMinion.character.isDead) {
                _assignedMinion.character.job.StopCreatedInteraction();
            }

            //if (_currentlyExploredLandmark != null) {
            //    _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
            //    _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
            //}
            //Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
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
        _isMinionRecalledExplore = state;
    }
    public void SetMinionRecallCollectState(bool state) {
        _isMinionRecalledCollect = state;
    }
    public void SetCurrentInteraction(Interaction interaction) {
        _currentInteraction = interaction;
    }

    #region Explore
    public void ExploreArea() {
        if (_assignedMinion == null) {
            return;
        }
        if (!_area.hasBeenInspected) {
            _area.SetHasBeenInspected(true);
        }
        _assignedMinion.character.job.StartJobAction();
        Messenger.Broadcast(Signals.MINION_STARTS_INVESTIGATING_AREA, _assignedMinion, _area);
        //_duration = 30;
        //_currentTick = 0;
        //Messenger.AddListener(Signals.HOUR_STARTED, OnExploreTick);
        //if (_currentlyExploredLandmark != null) {
        //    _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
        //    _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
        //}
        //_area.coreTile.landmarkOnTile.landmarkVisual.SetAndStartInteractionTimer(_duration);
        //_area.coreTile.landmarkOnTile.landmarkVisual.ShowNoInteractionForeground();
        //_area.coreTile.landmarkOnTile.landmarkVisual.ShowInteractionTimer();
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
        UIManager.Instance.areaInfoUI.ResetMinionAssignment();
    }
    public void UnattackLandmark() {
        _isAttacking = false;
        _assignedMinionAttack.SetAttackingArea(null);
        SetAssignedMinionAttack(null);
        _currentlyAttackedLandmark = null;
        UIManager.Instance.areaInfoUI.ResetMinionAssignmentParty();
    }
    //private void ClickedInteractionTimerButton(BaseLandmark landmark) {
    //    if (_assignedMinion != null && landmark.tileLocation.areaOfTile == _area) {
    //        Character character = _assignedMinion.icharacter as Character;
    //        character.job.createdInteraction.CancelSecondTimeOut();
    //        //_currentInteraction.SetExplorerMinion(_assignedMinion);
    //        character.job.createdInteraction.OnInteractionActive();
    //        InteractionUI.Instance.OpenInteractionUI(character.job.createdInteraction);
    //    }
    //}
    #endregion


    #region Attack
    private void AttackLandmark() {
        DefenderGroup defender = _area.GetFirstDefenderGroup();
        if (defender != null) {
            //_assignedMinionAttack.icharacter.currentParty.specificLocation.RemoveCharacterFromLocation(_assignedMinionAttack.icharacter.currentParty);
            //_currentlyAttackedLandmark.AddCharacterToLocation(_assignedMinionAttack.icharacter.currentParty);
            Combat combat = _assignedMinionAttack.character.currentParty.CreateCombatWith(defender.party);
            combat.Fight(() => AttackCombatResult(combat));
        } else {
            RecallMinion("attack");
            //Destroy Area
        }
    }
    private void AttackCombatResult(Combat combat) {
        if (_isAttacking) { //when the minion dies, isActivated will become false, hence, it must not go through the result
            if (combat.winningSide == _assignedMinionAttack.character.currentSide) {
                RecallMinion("attack");
                if(_area.GetFirstDefenderGroup() == null) {
                    //Destroy Area
                }
            }
        }
    }
    #endregion

    #region Token Collector
    public void AssignTokenCollector(Minion minion) {
        SetTokenCollector(minion);
        tokenCollector.SetEnabledState(false);
        tokenCollector.SetCollectingTokenArea(_area);

        MinionGoToAssignment(StartTokenCollection, "collect");
        _isCollectingTokens = true;
    }
    private void SetTokenCollector(Minion minion) {
        tokenCollector = minion;
    }
    public void StartTokenCollection() {
        tokenCollector.character.job.StartJobAction();
        PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, -50);
        Messenger.Broadcast(Signals.AREA_TOKEN_COLLECTION_CHANGED, _area);
    }
    public void StopTokenCollection() {
        tokenCollector.character.job.StopJobAction();
        _isCollectingTokens = false;
        tokenCollector.SetCollectingTokenArea(null);
        SetTokenCollector(null);
        if (UIManager.Instance.areaInfoUI.isShowing && UIManager.Instance.areaInfoUI.activeArea.id == _area.id) {
            UIManager.Instance.areaInfoUI.ResetTokenCollectorAssignment();
        }
        Messenger.Broadcast(Signals.AREA_TOKEN_COLLECTION_CHANGED, _area);
    }
    private void OnAreaOccupancyChanged(Area area) {
        if (_area.id == area.id) {
            if (tokenCollector != null) {
                Log retreatLog = new Log(GameManager.Instance.Today(), "Job", tokenCollector.character.job.GetType().ToString(), "token_retreat");
                retreatLog.AddToFillers(tokenCollector.character, tokenCollector.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                retreatLog.AddToFillers(area, area.name, LOG_IDENTIFIER.LANDMARK_1);
                if (tokenCollector.character.job.jobType == JOB.SPY) {
                    retreatLog.AddToFillers(area.previousOwner, area.previousOwner.name, LOG_IDENTIFIER.FACTION_1);
                } else {
                    retreatLog.AddToFillers(area.owner, area.owner.name, LOG_IDENTIFIER.FACTION_1);
                }
                retreatLog.AddLogToInvolvedObjects();
                RecallMinion("collect");
            }
        }
    }
    private bool IsActivelyCollectingTokens() {
        if (tokenCollector != null && tokenCollector.character.specificLocation.tileLocation.areaOfTile != null 
            && tokenCollector.character.specificLocation.tileLocation.areaOfTile.id == _area.id) {
            return true;
        }
        return false;
    }
    public bool CanCollectTokensHere(Minion minion) {
        if (_area.owner == null) {
            return minion.character.job.jobType == JOB.EXPLORER;
        } else {
            return minion.character.job.jobType == JOB.SPY;
        }
    }
    #endregion
}
