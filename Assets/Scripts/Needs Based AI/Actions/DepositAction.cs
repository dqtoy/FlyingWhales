using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DepositAction : CharacterAction {
    int depositingAmount = 20;

    public DepositAction() : base(ACTION_TYPE.DEPOSIT) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        RESOURCE resource = RESOURCE.NONE;
        if (party.actionData.questDataAssociatedWithCurrentAction is BuildStructureQuestData) {
            resource = (party.actionData.questDataAssociatedWithCurrentAction as BuildStructureQuestData).currentDepositingResource;
        } 
        if(resource != RESOURCE.NONE) {
            int deposit = depositingAmount;
            if(party.characterObject.resourceInventory[resource] < deposit) {
                deposit = party.characterObject.resourceInventory[resource];
            }
            party.characterObject.resourceInventory[resource] -= deposit;
            targetObject.resourceInventory[resource] += deposit;
        }

    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        if(party.actionData.questDataAssociatedWithCurrentAction.parentQuest is BuildStructureQuest) {
            BuildStructureQuest buildStructureQuest = party.actionData.questDataAssociatedWithCurrentAction.parentQuest as BuildStructureQuest;
            buildStructureQuest.UpdateLackingResources();
        }
    }
    public override CharacterAction Clone() {
        DepositAction depositAction = new DepositAction();
        SetCommonData(depositAction);
        depositAction.Initialize();
        return depositAction;
    }
    #endregion
}
