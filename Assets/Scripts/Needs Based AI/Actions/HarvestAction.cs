using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestAction : CharacterAction {

    public HarvestAction(ObjectState state) : base(state, ACTION_TYPE.HARVEST) {
    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        StructureObj obj = state.obj as StructureObj;

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.PRESTIGE, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);

        int objectResourceAmount = obj.resourceInventory[this.actionData.resourceGiven];
        if (objectResourceAmount > 0) { //if object's resource count is still greater than 0
            int minResource = this.actionData.minResourceGiven;
            if (minResource > objectResourceAmount) {
                //the minimum required resource is greater than the amount of resource that the object has
                minResource = objectResourceAmount;
            }
            int maxResource = this.actionData.maxResourceGiven;
            if (objectResourceAmount < maxResource) {
                maxResource = objectResourceAmount;
            }

            //give the character resource amount between min and max (inclusive)
            int resourceAmount = Random.Range(minResource, maxResource);
            character.TransferResourceTo(this.actionData.resourceGiven, resourceAmount, this.state.obj as StructureObj);
            ActionSuccess();
        }

        //TODO: if character's required resource of the type provided has been reached, end Harvest action
        EndAction(character);
    }
    public override CharacterAction Clone(ObjectState state) {
        HarvestAction populateAction = new HarvestAction(state);
        SetCommonData(populateAction);
        return populateAction;
    }
    #endregion
}
