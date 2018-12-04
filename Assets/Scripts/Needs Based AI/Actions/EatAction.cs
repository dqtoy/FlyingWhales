using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EatAction : CharacterAction {
    public EatAction() : base(ACTION_TYPE.EAT) {

    }
    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
        //if (party.IsFull(NEEDS.FULLNESS)) {
        //    EndAction(party, targetObject);
        //}
    }
    public override bool CanBeDoneBy(Party party, IObject targetObject) {
        //Filter: Residents of this Structure
        if (targetObject is StructureObj) {
            StructureObj structureObj = targetObject as StructureObj;
            if (structureObj.specificObjectType == LANDMARK_TYPE.HOUSES) {
                BaseLandmark landmark = structureObj.objectLocation;
                if (landmark.charactersWithHomeOnLandmark.Contains(party.mainCharacter)) {
                    return true;
                }
            } else if (structureObj.specificObjectType == LANDMARK_TYPE.INN) {
                if (party.mainCharacter.faction != null) {
                    Faction landmarkFaction = structureObj.objectLocation.tileLocation.areaOfTile.owner;
                    if (landmarkFaction != null) {
                        Faction characterFaction = party.mainCharacter.faction;
                        if (characterFaction.id == landmarkFaction.id) {
                            return true; //same factions
                        }
                        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(landmarkFaction, characterFaction);
                        if (rel != null && rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NON_HOSTILE) {
                            return true;
                        }
                    }
                } else {
                    return true; //if factionless allow
                }
            }

        }
        return false;
    }
    public override CharacterAction Clone() {
        EatAction eatAction = new EatAction();
        SetCommonData(eatAction);
        eatAction.Initialize();
        return eatAction;
    }
    #endregion
}
