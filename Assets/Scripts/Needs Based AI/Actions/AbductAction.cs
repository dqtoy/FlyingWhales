using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AbductAction : CharacterAction {
    //private StructureObj _structure;
    private int structureResourceAmount;

    public AbductAction() : base(ACTION_TYPE.ABDUCT) {
    }

    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
        //if (state.obj is StructureObj) {
        //    _structure = state.obj as StructureObj;
        //}
    //}
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if (targetObject is StructureObj) {
            StructureObj structure = targetObject as StructureObj;
            structureResourceAmount = structure.resourceInventory[this.actionData.resourceGiven];
            if (structureResourceAmount > 0) {
                if (party is CharacterParty) {
                    GiveAllReward(party as CharacterParty);
                }

                int minResourceAmount = this.actionData.minResourceGiven;
                int maxResourceAmount = this.actionData.maxResourceGiven;
                if (minResourceAmount > structureResourceAmount) {
                    minResourceAmount = structureResourceAmount;
                }
                if (structureResourceAmount < maxResourceAmount) {
                    maxResourceAmount = structureResourceAmount;
                }

                int resourceAmount = UnityEngine.Random.Range(minResourceAmount, maxResourceAmount + 1);
                //_structure.TransferResourceTo(this.actionData.resourceGiven, resourceAmount, character.characterObject);
                ActionSuccess(structure);
            } else {
                EndAction(party, structure);
            }
        }
    }
    public override CharacterAction Clone() {
        AbductAction abductAction = new AbductAction();
        SetCommonData(abductAction);
        abductAction.Initialize();
        return abductAction;
    }
    #endregion
}