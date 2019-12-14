using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;

public class MoveOutState : CharacterState {

    private int travelTimeInTicks {
        get {
            int travel = 3 * GameManager.ticksPerHour; //3 hours
            if (stateComponent.character.traitContainer.GetNormalTrait<Trait>("Fast") != null) { //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
                travel -= (int)(travel * 0.25f); //NOTE: Did not create a new world travel time modifier in character because it seems unneccessary if this is the only thing it is used for. Will put variable if more things need it.
            }
            return travel;
        }
    }
    private bool hasSceduledArriveAtRandomRegion;
    private Region chosenRegion;
    public string goHomeSchedID { get; private set; }

    public MoveOutState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Move Out State";
        characterState = CHARACTER_STATE.MOVE_OUT;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Explore_Icon;
    }

    #region Overrides
    protected override void StartState() {
        base.StartState();
        stateComponent.character.AdjustDoNotDisturb(1);
        stateComponent.character.needsComponent.AdjustDoNotGetHungry(1);
        stateComponent.character.needsComponent.AdjustDoNotGetLonely(1);
        stateComponent.character.needsComponent.AdjustDoNotGetTired(1);
    }
    protected override void EndState() {
        base.EndState();
        if (!string.IsNullOrEmpty(goHomeSchedID)) { //if this state is exited, and its goHomeSchedID is not empty (Usually because character died mid way). Cancel that schedule.
            SchedulingManager.Instance.RemoveSpecificEntry(goHomeSchedID);
        }
        stateComponent.character.SetPOIState(POI_STATE.ACTIVE);
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);

        Area currArea = stateComponent.character.currentRegion.area;
        if (currArea != null) {
            if (!stateComponent.character.marker.gameObject.activeSelf) {
                stateComponent.character.marker.PlaceMarkerAt(currArea.innerMap.GetRandomUnoccupiedEdgeTile());
            }
        }

        //Region currRegion = stateComponent.character.currentRegion;
        //if (currRegion == null) {
        //    currRegion = stateComponent.character.currentArea.region;
        //}
        //if (currRegion.area != null) {
        //    if (!stateComponent.character.marker.gameObject.activeSelf) {
        //        stateComponent.character.marker.PlaceMarkerAt(currRegion.innerMap.GetRandomUnoccupiedEdgeTile());
        //    }
        //}
        stateComponent.character.AdjustDoNotDisturb(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetHungry(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetLonely(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetTired(-1);
        SchedulingManager.Instance.ClearAllSchedulesBy(this);
    }
    public override void PauseState() {
        base.PauseState();
        stateComponent.character.AdjustDoNotDisturb(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetHungry(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetLonely(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetTired(-1);
    }
    public override void ResumeState() {
        base.ResumeState();
        stateComponent.character.AdjustDoNotDisturb(1);
        stateComponent.character.needsComponent.AdjustDoNotGetHungry(1);
        stateComponent.character.needsComponent.AdjustDoNotGetLonely(1);
        stateComponent.character.needsComponent.AdjustDoNotGetTired(1);
    }
    protected override void OnJobSet() {
        base.OnJobSet();
        Area currArea = stateComponent.character.currentRegion.area;
        if (currArea != null) {
            //if the character is still at his/her home area, go to the nearest edge tile
            LocationGridTile nearestEdgeTile = stateComponent.character.GetNearestUnoccupiedEdgeTileFromThis();
            stateComponent.character.marker.GoTo(nearestEdgeTile, OnArriveAtNearestEdgeTile);
        } else {
            OnArriveAtNearestEdgeTile();
        }
        //Region currRegion = stateComponent.character.currentRegion;
        //if(currRegion == null) {
        //    currRegion = stateComponent.character.currentArea.region;
        //}
        //if (currRegion.area != null) {
        //    //if the character is still at his/her home area, go to the nearest edge tile
        //    LocationGridTile nearestEdgeTile = stateComponent.character.GetNearestUnoccupiedEdgeTileFromThis();
        //    stateComponent.character.marker.GoTo(nearestEdgeTile, OnArriveAtNearestEdgeTile);
        //} else {
        //    OnArriveAtNearestEdgeTile();
        //}
    }
    public override void PerTickInState() { }
    protected override void CreateThoughtBubbleLog() {
        base.CreateThoughtBubbleLog();
        Region currRegion = stateComponent.character.currentRegion;
        if (thoughtBubbleLog != null) {
            thoughtBubbleLog.AddToFillers(currRegion, currRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    #endregion

    private void OnArriveAtNearestEdgeTile() {
        if (hasSceduledArriveAtRandomRegion) {
            return;
        }
        if (GetRegionToDoJob(stateComponent.character) == null) {
            job.CancelJob(false, "no valid regions");
            return;
        }
        hasSceduledArriveAtRandomRegion = true;
        stateComponent.character.CancelAllJobsExceptForCurrent();
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(true);
        stateComponent.character.SetPOIState(POI_STATE.INACTIVE);
        stateComponent.character.marker.gameObject.SetActive(false);
        stateComponent.character.marker.StopMovement();
        Messenger.Broadcast(Signals.PARTY_STARTED_TRAVELLING, this.stateComponent.character.ownParty);
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(travelTimeInTicks);
        SchedulingManager.Instance.AddEntry(dueDate, ArriveAtRegion, this);

        //Show log
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "left");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.currentRegion, stateComponent.character.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }
    private void ArriveAtRegion() {
        chosenRegion = GetRegionToDoJob(stateComponent.character);
        if (chosenRegion != null) {
            stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);
            stateComponent.character.currentRegion.RemoveCharacterFromLocation(stateComponent.character);
            chosenRegion.AddCharacterToLocation(stateComponent.character);
            OnArriveAtRegion();
        } else {
            job.CancelJob(false, "no valid regions");
        }
    }
    private void OnArriveAtRegion() {
        if(job.jobType == JOB_TYPE.RETURN_HOME) {
            //Show log
            Log log = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "arrive_home");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(stateComponent.character.homeRegion, stateComponent.character.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            thoughtBubbleLog = log;

            //OnExitThisState();
            stateComponent.ExitCurrentState();
        } else {
            //schedule go home
            GameDate dueDate = GameManager.Instance.Today();
            dueDate = dueDate.AddTicks(3 * GameManager.ticksPerHour);
            goHomeSchedID = SchedulingManager.Instance.AddEntry(dueDate, GoHome, this);
            Log log = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "arrived_region");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(chosenRegion, chosenRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            thoughtBubbleLog = log;

            chosenRegion.JobBasedEventGeneration(stateComponent.character);
        }
    }
    public void GoHome() {
        goHomeSchedID = string.Empty;
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(true);
        chosenRegion.RemoveCharacterFromLocation(stateComponent.character); //remove character from landmark. He/She is now just floating.
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(travelTimeInTicks);
        SchedulingManager.Instance.AddEntry(dueDate, ArriveHome, this);

        //Show log
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "going_home");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.homeRegion, stateComponent.character.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }
    private void ArriveHome() {
        //OnExitThisState();
        stateComponent.ExitCurrentState();
        Messenger.Broadcast(Signals.PARTY_DONE_TRAVELLING, stateComponent.character.currentParty);
        CheckNeeds();
        
        //Show log
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "arrive_home");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.homeRegion, stateComponent.character.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;

    }
    private void CheckNeeds() {
        string summary = GameManager.Instance.TodayLogString() + stateComponent.character.name + " has arrived home and will check his/her needs.";
        if (stateComponent.character.needsComponent.isStarving) {
            summary += "\n" + stateComponent.character.name + " is starving. Planning fullness recovery actions...";
            stateComponent.character.needsComponent.PlanFullnessRecoveryActions(stateComponent.character);
        }
        if (stateComponent.character.needsComponent.isExhausted) {
            summary += "\n" + stateComponent.character.name + " is exhausted. Planning tiredness recovery actions...";
            stateComponent.character.needsComponent.PlanTirednessRecoveryActions(stateComponent.character);
        }
        if (stateComponent.character.needsComponent.isForlorn) {
            summary += "\n" + stateComponent.character.name + " is forlorn. Planning happiness recovery actions...";
            stateComponent.character.needsComponent.PlanHappinessRecoveryActions(stateComponent.character);
        }
        Debug.Log(summary);
    }

    #region Region getters
    private Region GetRegionToDoJob(Character character) {
        if (job.targetRegion != null) {
            //If job has a designated region to go to, always use it
            return job.targetRegion;
        }
        return WorldEventsManager.Instance.GetValidRegionToMoveTo(job.jobType, character);
        
    }
    #endregion

}
