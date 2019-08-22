using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveOutState : CharacterState {

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
    }
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        if (stateComponent.character.specificLocation == stateComponent.character.homeArea) {
            //if the character is still at his/her home area, go to the nearest edge tile
            LocationGridTile nearestEdgeTile = stateComponent.character.GetNearestUnoccupiedEdgeTileFromThis();
            stateComponent.character.marker.GoTo(nearestEdgeTile, OnArriveAtNearestEdgeTile);
        }
    }
    public override void OnExitThisState() {
        base.OnExitThisState();
        if (!string.IsNullOrEmpty(goHomeSchedID)) { //if this state is exited, and its goHomeSchedID is not empty (Usually because character died mid way). Cancel that schedule.
            SchedulingManager.Instance.RemoveSpecificEntry(goHomeSchedID);
        }
    }
    protected override void PerTickInState() { }
    #endregion

    private void OnArriveAtNearestEdgeTile() {
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(true);
        stateComponent.character.SetPOIState(POI_STATE.INACTIVE);
        stateComponent.character.marker.gameObject.SetActive(false);
        Messenger.Broadcast(Signals.PARTY_STARTED_TRAVELLING, this.stateComponent.character.ownParty);
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(3 * GameManager.ticksPerHour);
        SchedulingManager.Instance.AddEntry(dueDate, ArriveAtRandomRegion, this);
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "left");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.specificLocation, stateComponent.character.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }

    private Region chosenRegion;
    private void ArriveAtRandomRegion() {
        List<Region> choices = GridMap.Instance.allRegions.Where(x => !x.coreTile.isCorrupted && x.coreTile.areaOfTile != stateComponent.character.homeArea).ToList();
        if (choices.Count > 0) {
            stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);
            chosenRegion = choices[Random.Range(0, choices.Count)];
            stateComponent.character.specificLocation.RemoveCharacterFromLocation(stateComponent.character);
            chosenRegion.mainLandmark.AddCharacterHere(stateComponent.character);
            OnArriveAtRegion();
        } else {
            throw new System.Exception("There are no more uncorrupted regions for " + stateComponent.character.name);
        }
    }

    public string goHomeSchedID { get; private set; }
    private void OnArriveAtRegion() {
        //schedule go home
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(3 * GameManager.ticksPerHour);
        goHomeSchedID = SchedulingManager.Instance.AddEntry(dueDate, GoHome, this);
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "arrived_region");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(chosenRegion, chosenRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }

    public void GoHome() {
        goHomeSchedID = string.Empty;
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(true);
        chosenRegion.mainLandmark.RemoveCharacterHere(stateComponent.character); //remove character from landmark. He/She is now just floating.
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(3 * GameManager.ticksPerHour);
        SchedulingManager.Instance.AddEntry(dueDate, ArriveHome, this);
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "going_home");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.homeArea, stateComponent.character.homeArea.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }

    private void ArriveHome() {
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);
        stateComponent.character.marker.PlaceMarkerAt(stateComponent.character.specificLocation.GetRandomUnoccupiedEdgeTile());
        OnExitThisState();
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "arrive_home");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.homeArea, stateComponent.character.homeArea.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;

    }

    public override string ToString() {
        return "Move Out State by " + stateComponent.character.name;
    }
}
