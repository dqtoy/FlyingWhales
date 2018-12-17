using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrinkAction : CharacterAction {
    public DrinkAction() : base(ACTION_TYPE.DRINK) {

    }
    #region Overrides
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        //Add history log
        for (int i = 0; i < party.characters.Count; i++) {
            party.characters[i].AddAttribute(ATTRIBUTE.DRUNK);
        }
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
        //if (party.IsFull(NEEDS.FUN)) {
        //    EndAction(party, targetObject);
        //}
    }
    public override bool CanBeDoneBy(Party party, IObject targetObject) {
        if (party.mainCharacter.faction != null) {
            if (targetObject is StructureObj) {
                Faction landmarkFaction = (targetObject as StructureObj).objectLocation.tileLocation.areaOfTile.owner;
                if (landmarkFaction != null) {
                    Faction characterFaction = party.mainCharacter.faction;
                    if (characterFaction.id == landmarkFaction.id) {
                        return true; //same factions
                    }
                    FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(landmarkFaction, characterFaction);
                    if (rel != null && rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY) {
                        return true;
                    }
                }
            }
        } else {
            return true; //if factionless allow
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
