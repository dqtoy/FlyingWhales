using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureAction : CharacterAction {
    private StructureObj _structure;
    public TortureAction(ObjectState state) : base(state, ACTION_TYPE.TORTURE) {
        if(_state.obj is StructureObj) {
            _structure = _state.obj as StructureObj;
        }
    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        //if (obj.GetTotalCivilians() > 0) {//check if there are civilians in the object
        //    //if yes, 
        //}

        if(_structure.GetTotalCivilians() > 0) {
            GiveReward(NEEDS.FULLNESS, character);
            GiveReward(NEEDS.PRESTIGE, character);
            GiveReward(NEEDS.ENERGY, character);
            GiveReward(NEEDS.JOY, character);
            ActionSuccess();
            if (character.role.IsFull(NEEDS.JOY)) {
                EndAction(character);
            }
        } 
        //else {
        //    EndAction(character);
        //}
    }
    public override CharacterAction Clone(ObjectState state) {
        TortureAction populateAction = new TortureAction(state);
        SetCommonData(populateAction);
        return populateAction;
    }
    #endregion
}
