using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AttackLandmarkAction : CharacterAction {

    public AttackLandmarkAction() : base(ACTION_TYPE.ATTACK_LANDMARK) {

    }

    #region Overrides
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        BaseLandmark landmarkToAttack = targetObject.objectLocation;
        //Party defenderParty = null; //TODO
        DefenderGroup defender = landmarkToAttack.tileLocation.areaOfTile.GetFirstDefenderGroup();
        if (defender != null) {
            Combat combat = party.CreateCombatWith(defender.party);
            combat.Fight();
        }
        //Attack their area: -3 Favor Count
        Faction attackedFaction = landmarkToAttack.owner;
        if (attackedFaction != null && party.faction != null) {
            attackedFaction.AdjustFavorFor(party.faction, -3);
        }
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (targetObject is StructureObj) {
            StructureObj structure = targetObject as StructureObj;
            structure.AdjustHP(-10);
            if (structure.currentHP <= 0) {
                EndAction(party, targetObject);
            }
        }
    }
    public override CharacterAction Clone() {
        AttackLandmarkAction action = new AttackLandmarkAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
