
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestAction : CharacterAction {
    //private StructureObj _structure;

    public HarvestAction() : base(ACTION_TYPE.HARVEST) {
       
    }

    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
    //    if (state.obj is StructureObj) {
    //        _structure = state.obj as StructureObj;
    //    }
    //}
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }

        int resourceGiven = Random.Range(this.actionData.minResourceGiven, this.actionData.maxResourceGiven);
        party.icharacterObject.AdjustResource(this.actionData.resourceGiven, resourceGiven);
        ActionSuccess(targetObject);
        //int objectResourceAmount = _structure.resourceInventory[this.actionData.resourceGiven];
        //if (objectResourceAmount > 0 && !character.DoesSatisfiesPrerequisite(character.actionData.currentChainAction.prerequisite)) { //if object's resource count is still greater than 0 and character doesn't have the required resource amount yet
        //    int minResource = this.actionData.minResourceGiven;
        //    if (minResource > objectResourceAmount) {
        //        //the minimum required resource is greater than the amount of resource that the object has
        //        minResource = objectResourceAmount;
        //    }
        //    int maxResource = this.actionData.maxResourceGiven;
        //    if (objectResourceAmount < maxResource) {
        //        maxResource = objectResourceAmount;
        //    }

        //    //give the character resource amount between min and max (inclusive)
        //    int resourceAmount = Random.Range(minResource, maxResource);
        //    _structure.TransferResourceTo(this.actionData.resourceGiven, resourceAmount, character.characterObject as CharacterObj);
        //    ActionSuccess();
        //} else {
        //    EndAction(character);
        //}

    }
    public override CharacterAction Clone() {
        HarvestAction harvestAction = new HarvestAction();
        SetCommonData(harvestAction);
        harvestAction.Initialize();
        return harvestAction;
    }
    public override SCHEDULE_ACTION_CATEGORY GetSchedActionCategory() {
        return SCHEDULE_ACTION_CATEGORY.WORK;
    }
    #endregion
}
