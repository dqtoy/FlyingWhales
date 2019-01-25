using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DepositAction : CharacterAction {
    int depositingAmount = 50;

    public DepositAction() : base(ACTION_TYPE.DEPOSIT) {

    }
    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            CharacterParty characterParty = party as CharacterParty;
            GiveAllReward(characterParty);
            RESOURCE resource = RESOURCE.NONE;
            //if (party.actionData.questDataAssociatedWithCurrentAction is BuildStructureQuestData) {
            //    resource = (party.actionData.questDataAssociatedWithCurrentAction as BuildStructureQuestData).currentDepositingResource;
            //}
            if (resource != RESOURCE.NONE) {
                int deposit = depositingAmount;
                if (characterParty.characterObject.resourceInventory[resource] < deposit) {
                    deposit = characterParty.characterObject.resourceInventory[resource];
                }
                characterParty.characterObject.AdjustResource(resource, -deposit);
                targetObject.AdjustResource(resource, deposit);
            }
            if (characterParty.characterObject.resourceInventory[resource] <= 0) {
                EndAction(characterParty, targetObject);
            }
        }
    }
    public override bool CanBeDone(IObject targetObject) {
        return false;
    }
    public override CharacterAction Clone() {
        DepositAction depositAction = new DepositAction();
        SetCommonData(depositAction);
        depositAction.Initialize();
        return depositAction;
    }
    #endregion
}
