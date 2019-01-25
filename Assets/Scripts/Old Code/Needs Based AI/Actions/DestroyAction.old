using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DestroyAction : CharacterAction {
    //private StructureObj _structure;
    //private int _amountToReduce;

    public DestroyAction() : base(ACTION_TYPE.DESTROY) {
    }

    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
        //if (state.obj is StructureObj) {
        //    _structure = state.obj as StructureObj;
        //}
        //if (_amountToReduce == 0) {
        //    _amountToReduce = Mathf.RoundToInt((float) _structure.maxHP / (float) _actionData.duration);
        //}
    //}
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if(targetObject is StructureObj) {
            StructureObj structure = targetObject as StructureObj;
            if (structure.isHPZero) {
                EndAction(party, structure);
                return;
            }
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < actionData.successRate) {
                ActionSuccess(structure);
                if (party is CharacterParty) {
                    GiveAllReward(party as CharacterParty);
                }

                int amountToReduce = Mathf.RoundToInt((float) structure.maxHP / (float) _actionData.duration); ;
                structure.AdjustHP(-amountToReduce);
                if (structure.isHPZero) {
                    EndAction(party, structure);
                }
            } else {
                ActionFail(structure);
                if (party is CharacterParty) {
                    GiveAllReward(party as CharacterParty);
                }
            }
        }

    }
    public override CharacterAction Clone() {
        DestroyAction destroyAction = new DestroyAction();
        SetCommonData(destroyAction);
        destroyAction.Initialize();
        return destroyAction;
    }
    #endregion
}