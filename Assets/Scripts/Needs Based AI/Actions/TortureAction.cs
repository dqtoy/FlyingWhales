
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureAction : CharacterAction {
    private StructureObj _structure;
    public TortureAction() : base(ACTION_TYPE.TORTURE) {
       
    }

    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
    //    if (_state.obj is StructureObj) {
    //        _structure = _state.obj as StructureObj;
    //    }
    //}
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //if (obj.GetTotalCivilians() > 0) {//check if there are civilians in the object
        //    //if yes, 
        //}

        if(_structure.objectLocation.civilianCount > 0) {
            ActionSuccess(targetObject);
            if (party is CharacterParty) {
                CharacterParty characterParty = party as CharacterParty;
                GiveAllReward(characterParty);
                if (characterParty.IsFull(NEEDS.FUN)) {
                    EndAction(party, targetObject);
                }
            }
            
        } 
        //else {
        //    EndAction(character);
        //}
    }
    public override CharacterAction Clone() {
        TortureAction tortureAction = new TortureAction();
        SetCommonData(tortureAction);
        tortureAction.Initialize();
        return tortureAction;
    }
    #endregion
}
