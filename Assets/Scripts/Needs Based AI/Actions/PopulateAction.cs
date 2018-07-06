using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateAction : CharacterAction {

    public PopulateAction() : base(ACTION_TYPE.POPULATE) {
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //TODO: Lahat ng nasa baba, nakahardcode sya currently
        int characterResourceAmount = (party.characterObject as CharacterObj).resourceInventory[this.actionData.resourceGiven];
        if (characterResourceAmount > 0) { //if character's resource count is still greater than 0
            //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
            GiveAllReward(party);

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
            int resourceAmount = Random.Range(minResource, maxResource + 1);
            (party.characterObject as CharacterObj).TransferResourceTo(this.actionData.resourceGiven, resourceAmount, targetObject as StructureObj);
            ActionSuccess(targetObject);
        } else {
            EndAction(party, targetObject);
        }
    }
    public override CharacterAction Clone() {
        PopulateAction populateAction = new PopulateAction();
        SetCommonData(populateAction);
        populateAction.Initialize();
        return populateAction;
    }
    #endregion
}
