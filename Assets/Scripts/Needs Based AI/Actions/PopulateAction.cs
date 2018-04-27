using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateAction : CharacterAction {

    public PopulateAction(ObjectState state) : base(state, ACTION_TYPE.POPULATE) {
    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        int characterResourceAmount = character.resourceInventory[this.actionData.resourceGiven];
        if (characterResourceAmount > 0) { //if character's resource count is still greater than 0
            //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
            GiveReward(NEEDS.FULLNESS, character);
            GiveReward(NEEDS.PRESTIGE, character);
            GiveReward(NEEDS.ENERGY, character);
            GiveReward(NEEDS.JOY, character);

            int minResource = this.actionData.minResourceGiven;
            if (minResource > characterResourceAmount) {
                //the minimum required resource is greater than the amount of resource that the character has
                minResource = characterResourceAmount;
            }

            int maxResource = this.actionData.maxResourceGiven;
            if (characterResourceAmount < maxResource) {
                maxResource = characterResourceAmount;
            }

            //transfer the character resource amount between min and max (inclusive) to the object
            int resourceAmount = Random.Range(minResource, maxResource);
            character.TransferResourceTo(this.actionData.resourceGiven, resourceAmount, this.state.obj as StructureObj);
            ActionSuccess();
        } else {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        PopulateAction populateAction = new PopulateAction(state);
        SetCommonData(populateAction);
        return populateAction;
    }
    #endregion
}
