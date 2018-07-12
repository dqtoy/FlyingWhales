using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class EatAction : CharacterAction {
    public EatAction() : base(ACTION_TYPE.EAT) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        //if (party.IsFull(NEEDS.FULLNESS)) {
        //    EndAction(party, targetObject);
        //}
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        //Filter: Residents of this Structure
        if (targetObject is StructureObj) {
            StructureObj structureObj = targetObject as StructureObj;
            if (structureObj.specificObjectType == LANDMARK_TYPE.ELVEN_HOUSES || structureObj.specificObjectType == LANDMARK_TYPE.HUMAN_HOUSES) {
                BaseLandmark landmark = structureObj.objectLocation;
                if (landmark.charactersWithHomeOnLandmark.Contains(party.mainCharacter as ECS.Character)) {
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
