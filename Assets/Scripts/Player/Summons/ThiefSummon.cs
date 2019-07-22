using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefSummon : Summon {

    private int itemsToSteal; //number of items that the thief will steal.

    public ThiefSummon() : base(SUMMON_TYPE.ThiefSummon, CharacterRole.BANDIT, RACE.HUMANS, Utilities.GetRandomGender()) {
        itemsToSteal = 2;
        AddInteractionType(INTERACTION_TYPE.STEAL);
        AddInteractionType(INTERACTION_TYPE.STEAL_CHARACTER);
    }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        AddInitialAwareness(tile.parentAreaMap.area);
        Messenger.AddListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        //add all characters that are not part of the player faction to this character's terrifying characters list, so this character can avoid them.
        for (int i = 0; i < tile.parentAreaMap.area.charactersAtLocation.Count; i++) {
            Character currCharacter = tile.parentAreaMap.area.charactersAtLocation[i];
            if (currCharacter.faction != this.faction) {
                marker.AddTerrifyingObject(currCharacter);
            }
        }
    }
    public override void ThisCharacterSaw(Character target) {
        base.ThisCharacterSaw(target);
        //if the target is not from the player faction, add them to the terrifying characters list
        if (target.faction != this.faction) {
            marker.AddTerrifyingObject(target);
        }
    }
    protected override void IdlePlans() {
        if (_hasAlreadyAskedForPlan) {
            return;
        }
        SetHasAlreadyAskedForPlan(true);
        if (items.Count >= itemsToSteal) {
            //if the thief has already reached his/her max number of stolen items, exit the area.
            LocationGridTile targetTile = GetNearestUnoccupiedEdgeTileFromThis();
            marker.GoTo(targetTile, OnReachExitAction);
        } else {
            //if the warehouse still has items in it, try to steal them.
            LocationStructure warehouse = specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
            if (warehouse.itemsInStructure.Count > 0) {
                SpecialToken chosenItem = warehouse.itemsInStructure[UnityEngine.Random.Range(0, warehouse.itemsInStructure.Count)];
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.STEAL, INTERACTION_TYPE.STEAL, chosenItem);
                job.SetCannotOverrideJob(true);
                job.SetCannotCancelJob(true);
                jobQueue.AddJobInQueue(job);
            } else {
                //just enter berserked mode.
                stateComponent.SwitchToState(CHARACTER_STATE.STROLL, null, specificLocation);
                SetHasAlreadyAskedForPlan(false);
            }
        }
    }
    #endregion

    private void OnReachExitAction() {
        //remove character from other character's awareness
        for (int i = 0; i < gridTileLocation.parentAreaMap.area.charactersAtLocation.Count; i++) {
            Character currCharacter = gridTileLocation.parentAreaMap.area.charactersAtLocation[i];
            currCharacter.RemoveAwareness(this);
        }

        marker.ClearTerrifyingObjects();
        specificLocation.RemoveCharacterFromLocation(this);
        DestroyMarker();
        UnsubscribeSignals();
        ClearAllAwareness();
    }
}
