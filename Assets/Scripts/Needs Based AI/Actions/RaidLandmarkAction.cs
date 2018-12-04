using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RaidLandmarkAction : CharacterAction {
    public RaidLandmarkAction() : base(ACTION_TYPE.RAID_LANDMARK) {
        _actionData.duration = 5;
    }

    #region Overrides
    public override void OnFirstEncounter(Party party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        BaseLandmark landmarkToRaid = targetObject.objectLocation;
        landmarkToRaid.SetRaidedState(true);
        //Party defenderParty = null; //TODO
        DefenderGroup defender = landmarkToRaid.tileLocation.areaOfTile.GetFirstDefenderGroup();
        if(defender != null) {
            Combat combat = party.CreateCombatWith(defender.party);
            combat.Fight();
        }
        //Raid their area: -1 Favor Count
        Faction raidedFaction = landmarkToRaid.owner;
        if (raidedFaction != null && party.faction != null) {
            raidedFaction.AdjustFavorFor(party.faction, -1);
        }
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        Area areaToRaid = targetObject.objectLocation.tileLocation.areaOfTile;
        if(areaToRaid != null) {
            int amountToRaid = (int)((float)areaToRaid.suppliesInBank * ((float)(UnityEngine.Random.Range(5, 16)) / 100f));
            areaToRaid.AdjustSuppliesInBank(-amountToRaid);
            PlayerManager.Instance.player.AdjustCurrency(CURRENCY.SUPPLY, amountToRaid);
        }
    }
    public override CharacterAction Clone() {
        RaidLandmarkAction action = new RaidLandmarkAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
