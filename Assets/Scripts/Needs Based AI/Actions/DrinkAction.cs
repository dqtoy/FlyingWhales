using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DrinkAction : CharacterAction {
    public DrinkAction() : base(ACTION_TYPE.DRINK) {

    }
    #region Overrides
    public override void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        //Add history log
        for (int i = 0; i < party.icharacters.Count; i++) {
            party.icharacters[i].AssignTag(CHARACTER_TAG.DRUNK);
        }
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        //if (party.IsFull(NEEDS.FUN)) {
        //    EndAction(party, targetObject);
        //}
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if (party.mainCharacter.faction != null && targetObject is StructureObj) {
            Faction landmarkFaction = (targetObject as StructureObj).objectLocation.tileLocation.areaOfTile.owner;
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
        return false;
    }
    public override CharacterAction Clone() {
        DrinkAction drinkAction = new DrinkAction();
        SetCommonData(drinkAction);
        drinkAction.Initialize();
        return drinkAction;
    }
    #endregion
}
