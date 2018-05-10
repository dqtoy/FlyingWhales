using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestAction : CharacterAction {
    private StructureObj _structure;

    public HarvestAction(ObjectState state) : base(state, ACTION_TYPE.HARVEST) {
        if(state.obj is StructureObj) {
            _structure = state.obj as StructureObj;
        }
    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.PRESTIGE, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);

        int objectResourceAmount = _structure.resourceInventory[this.actionData.resourceGiven];
        if (objectResourceAmount > 0 && !character.DoesSatisfiesPrerequisite(character.actionData.currentChainAction.prerequisite)) { //if object's resource count is still greater than 0 and character doesn't have the required resource amount yet
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
            _structure.TransferResourceTo(this.actionData.resourceGiven, resourceAmount, character.characterObject);
            ActionSuccess();
        } else {
            EndAction(character);
        }

    }
    public override CharacterAction Clone(ObjectState state) {
        HarvestAction populateAction = new HarvestAction(state);
        SetCommonData(populateAction);
        return populateAction;
    }
    #endregion
}
