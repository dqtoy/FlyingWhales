using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RestAction : CharacterAction {
    public RestAction() : base(ACTION_TYPE.REST) {

    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
        //if (party.IsFull(NEEDS.ENERGY)) {
        //    EndAction(party, targetObject);
        //}
    }
    public override bool CanBeDoneBy(Party party, IObject targetObject) {
        //Filter: Residents of this Structure
        if (targetObject is StructureObj) {
            StructureObj structureObj = targetObject as StructureObj;
            BaseLandmark landmark = structureObj.objectLocation;
            Character character = party.owner;
            if (landmark.charactersWithHomeOnLandmark.Contains(character)) {
                return true;
            } else if (structureObj.specificObjectType == LANDMARK_TYPE.INN) {
                if (character.homeLandmark != null) { //if the character still has a home
                    if (targetObject.objectLocation.tileLocation.areaOfTile != null 
                        && targetObject.objectLocation.tileLocation.areaOfTile.id == character.homeLandmark.tileLocation.areaOfTile.id) { //check if this inn is in the same area as his home
                        return false; //if it is, do not rest at inn
                    }
                }
                //if this character has no home or this inn is not part of his home area, check for faction hostility
                if (character.faction != null && structureObj.objectLocation.tileLocation.areaOfTile != null) {
                    Faction landmarkFaction = structureObj.objectLocation.tileLocation.areaOfTile.owner;
                    if (landmarkFaction != null) {
                        Faction characterFaction = character.faction;
                        if (characterFaction.id == landmarkFaction.id) {
                            return true; //same factions
                        }

                        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(landmarkFaction, characterFaction);
                        if (rel != null && rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY) {
                            return true;
                        }
                    }
                } else {
                    return true; //if factionless, or inn is not part of an area, allow.
                }
            }
            
        }
        return false;
    }
    public override CharacterAction Clone() {
        RestAction restAction = new RestAction();
        SetCommonData(restAction);
        restAction.Initialize();
        return restAction;
    }
    public override SCHEDULE_ACTION_CATEGORY GetSchedActionCategory() {
        return SCHEDULE_ACTION_CATEGORY.REST;
    }
    #endregion
}
