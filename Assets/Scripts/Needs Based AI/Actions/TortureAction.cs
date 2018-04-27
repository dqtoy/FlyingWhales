using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureAction : CharacterAction {
    public TortureAction(ObjectState state) : base(state, ACTION_TYPE.TORTURE) {

    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        StructureObj obj = state.obj as StructureObj;
         if (obj.GetTotalCivilians() > 0) {//check if there are civilians in the object
            //if yes, 
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        TortureAction populateAction = new TortureAction(state);
        SetCommonData(populateAction);
        return populateAction;
    }
    #endregion
}
