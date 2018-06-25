using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AbductAction : CharacterAction {
    private StructureObj _structure;
    private int structureResourceAmount;

    public AbductAction(ObjectState state) : base(state, ACTION_TYPE.ABDUCT) {
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if (state.obj is StructureObj) {
            _structure = state.obj as StructureObj;
        }
    }
    public override void PerformAction(Character character) {
        base.PerformAction(character);

        structureResourceAmount = _structure.resourceInventory[this.actionData.resourceGiven];
        if (structureResourceAmount > 0) {
            GiveAllReward(character);

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
            ActionSuccess();
        } else {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        AbductAction abductAction = new AbductAction(state);
        SetCommonData(abductAction);
        abductAction.Initialize();
        return abductAction;
    }
    #endregion
}