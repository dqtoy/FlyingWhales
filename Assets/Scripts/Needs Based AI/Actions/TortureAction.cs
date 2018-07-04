using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureAction : CharacterAction {
    private StructureObj _structure;
    public TortureAction(ObjectState state) : base(state, ACTION_TYPE.TORTURE) {
       
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if (_state.obj is StructureObj) {
            _structure = _state.obj as StructureObj;
        }
    }
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        //if (obj.GetTotalCivilians() > 0) {//check if there are civilians in the object
        //    //if yes, 
        //}

        if(_structure.objectLocation.civilianCount > 0) {
            GiveAllReward(party);
            ActionSuccess();
            if (party.IsFull(NEEDS.FUN)) {
                EndAction(party);
            }
        } 
        //else {
        //    EndAction(character);
        //}
    }
    public override CharacterAction Clone(ObjectState state) {
        TortureAction tortureAction = new TortureAction(state);
        SetCommonData(tortureAction);
        tortureAction.Initialize();
        return tortureAction;
    }
    #endregion
}
