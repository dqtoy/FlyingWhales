using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveOutState : CharacterState {

    private int travelTimeInTicks {
        get {
            int travel = 3 * GameManager.ticksPerHour; //3 hours
            if (stateComponent.character.traitContainer.GetNormalTrait("Fast") != null) { //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
                travel -= (int)(travel * 0.25f); //NOTE: Did not create a new world travel time modifier in character because it seems unneccessary if this is the only thing it is used for. Will put variable if more things need it.
            }
            return travel;
        }
    }

    public MoveOutState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Move Out State";
        characterState = CHARACTER_STATE.MOVE_OUT;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Explore_Icon;
    }

    #region Overrides
    protected override void StartState() {
        base.StartState();
        stateComponent.character.AdjustDoNotDisturb(1);
        stateComponent.character.AdjustDoNotGetHungry(1);
        stateComponent.character.AdjustDoNotGetLonely(1);
        stateComponent.character.AdjustDoNotGetTired(1);
    }
    public override void PauseState() {
        base.PauseState();
        stateComponent.character.AdjustDoNotDisturb(-1);
        stateComponent.character.AdjustDoNotGetHungry(-1);
        stateComponent.character.AdjustDoNotGetLonely(-1);
        stateComponent.character.AdjustDoNotGetTired(-1);
    }
    public override void ResumeState() {
        base.ResumeState();
        stateComponent.character.AdjustDoNotDisturb(1);
        stateComponent.character.AdjustDoNotGetHungry(1);
        stateComponent.character.AdjustDoNotGetLonely(1);
        stateComponent.character.AdjustDoNotGetTired(1);
    }
    //protected override void DoMovementBehavior() {
    //    base.DoMovementBehavior();
    //    if (stateComponent.character.specificLocation == stateComponent.character.homeArea) {
    //        //if the character is still at his/her home area, go to the nearest edge tile
    //        LocationGridTile nearestEdgeTile = stateComponent.character.GetNearestUnoccupiedEdgeTileFromThis();
    //        stateComponent.character.marker.GoTo(nearestEdgeTile, OnArriveAtNearestEdgeTile);
    //    }
    //}
    protected override void OnJobSet() {
        base.OnJobSet();
        Region currRegion = stateComponent.character.currentRegion;
        if(currRegion == null) {
            currRegion = stateComponent.character.specificLocation.region;
        }
        if (currRegion.area != null) {
            //if the character is still at his/her home area, go to the nearest edge tile
            LocationGridTile nearestEdgeTile = stateComponent.character.GetNearestUnoccupiedEdgeTileFromThis();
            stateComponent.character.marker.GoTo(nearestEdgeTile, OnArriveAtNearestEdgeTile);
        } else {
            OnArriveAtNearestEdgeTile();
        }
    }
    public override void OnExitThisState() {
        base.OnExitThisState();
        if (!string.IsNullOrEmpty(goHomeSchedID)) { //if this state is exited, and its goHomeSchedID is not empty (Usually because character died mid way). Cancel that schedule.
            SchedulingManager.Instance.RemoveSpecificEntry(goHomeSchedID);
        }
        stateComponent.character.SetPOIState(POI_STATE.ACTIVE);
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);

        Region currRegion = stateComponent.character.currentRegion;
        if (currRegion == null) {
            currRegion = stateComponent.character.specificLocation.region;
        }
        if (currRegion.area != null) {
            if (!stateComponent.character.marker.gameObject.activeSelf) {
                stateComponent.character.marker.PlaceMarkerAt(currRegion.area.GetRandomUnoccupiedEdgeTile());
            }
        }
        stateComponent.character.AdjustDoNotDisturb(-1);
        stateComponent.character.AdjustDoNotGetHungry(-1);
        stateComponent.character.AdjustDoNotGetLonely(-1);
        stateComponent.character.AdjustDoNotGetTired(-1);
        SchedulingManager.Instance.ClearAllSchedulesBy(this);
    }
    protected override void PerTickInState() { }
    protected override void CreateThoughtBubbleLog() {
        base.CreateThoughtBubbleLog();
        if (thoughtBubbleLog != null) {
            thoughtBubbleLog.AddToFillers(stateComponent.character.specificLocation, stateComponent.character.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    #endregion

    bool hasSceduledArriveAtRandomRegion;

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
        log.AddToFillers(stateComponent.character.specificLocation, stateComponent.character.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }

    private Region chosenRegion;
    private void ArriveAtRegion() {
        chosenRegion = GetRegionToDoJob(stateComponent.character);
        if (chosenRegion != null) {
            stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);
            //chosenRegion = choices[Random.Range(0, choices.Count)];
            stateComponent.character.specificLocation.RemoveCharacterFromLocation(stateComponent.character);
            chosenRegion.AddCharacterToLocation(stateComponent.character);
            OnArriveAtRegion();
        } else {
            job.CancelJob(false, "no valid regions");
        }
    }

    public string goHomeSchedID { get; private set; }
    private void OnArriveAtRegion() {
        if(job.jobType == JOB_TYPE.RETURN_HOME) {
            //Show log
            Log log = new Log(GameManager.Instance.Today(), "CharacterState", stateName, "arrive_home");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(stateComponent.character.homeRegion, stateComponent.character.homeRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            thoughtBubbleLog = log;

            OnExitThisState();
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
        OnExitThisState();
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
        if (stateComponent.character.isStarving) {
            summary += "\n" + stateComponent.character.name + " is starving. Planning fullness recovery actions...";
            stateComponent.character.PlanFullnessRecoveryActions();
        }
        if (stateComponent.character.isExhausted) {
            summary += "\n" + stateComponent.character.name + " is exhausted. Planning tiredness recovery actions...";
            stateComponent.character.PlanTirednessRecoveryActions();
        }
        if (stateComponent.character.isForlorn) {
            summary += "\n" + stateComponent.character.name + " is forlorn. Planning happiness recovery actions...";
            stateComponent.character.PlanHappinessRecoveryActions();
        }
        Debug.Log(summary);
    }

    //public override string ToString() {
    //    return "Move Out State by " + stateComponent.character.name;
    //}
    private Region GetRegionToDoJob(Character character) {
        if(job.targetRegion != null) {
            //If job has a designated region to go to, always use it
            return job.targetRegion;
        }
        if(job.jobType == JOB_TYPE.RETURN_HOME) {
            return character.homeRegion;
        } else {
            List<Region> regionPool = GetValidRegionsToDoJob(character);
            if(regionPool.Count > 0) {
                return regionPool[Random.Range(0, regionPool.Count)];
            }
        }
        return null;
    }
    private List<Region> GetValidRegionsToDoJob(Character character) {
        if (job == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + character.name + " is checking for valid regions to do job but his/her job is null.");
        }

        List<LANDMARK_TYPE> validLandmarkTypes = new List<LANDMARK_TYPE>();
        if (job.jobType == JOB_TYPE.DESTROY_PROFANE_LANDMARK) {
            validLandmarkTypes.Add(LANDMARK_TYPE.THE_PROFANE);
        } else if (job.jobType == JOB_TYPE.CORRUPT_CULTIST) {
            validLandmarkTypes.Add(LANDMARK_TYPE.THE_PROFANE);
        } else {
            validLandmarkTypes.AddRange(Utilities.GetEnumValues<LANDMARK_TYPE>());
        }
        validLandmarkTypes.Remove(LANDMARK_TYPE.THE_PORTAL); //Always removing the portal for now, to prevent characters going there.

        List<Region> choices = null;
        if (job.jobType.ProducesWorldEvent()) {
            choices = GridMap.Instance.allRegions.Where(x =>
                x.activeEvent == null &&
                x != stateComponent.character.homeRegion &&
                validLandmarkTypes.Contains(x.mainLandmark.specificLandmarkType) &&
                StoryEventsManager.Instance.GetEventsThatCanProvideEffects(x, character, job.jobType.GetAllowedEventEffects()).Count > 0
            ).ToList();
        } else {
            choices = GridMap.Instance.allRegions.Where(x =>
               x != stateComponent.character.homeRegion &&
               validLandmarkTypes.Contains(x.mainLandmark.specificLandmarkType)
            ).ToList();
        }
        //else {
        //    if (job.jobType == JOB_TYPE.RETURN_HOME) { //TODO: Find a way to make getting valid regions per job type prettier.
        //        if (!LandmarkManager.Instance.TryGetUncorruptedRegionsExcept(out choices)) {
        //            //there are no uncorrupted regions
        //            choices = GridMap.Instance.allRegions.Where(x =>
        //               x.coreTile.areaOfTile != stateComponent.character.homeArea &&
        //               validLandmarkTypes.Contains(x.mainLandmark.specificLandmarkType)
        //            ).ToList();
        //        }

        //    } else {
        //        choices = GridMap.Instance.allRegions.Where(x =>
        //           x.coreTile.areaOfTile != stateComponent.character.homeArea &&
        //           validLandmarkTypes.Contains(x.mainLandmark.specificLandmarkType) 
        //        ).ToList();
        //    }

        //}
        return choices;
    }
}
