using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToAction : CharacterAction {
    public MoveToAction() : base(ACTION_TYPE.MOVE_TO) {

    }

    #region Overrides
    public override CharacterAction Clone() {
        MoveToAction action = new MoveToAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        iparty.GoToLocation(targetObject.objectLocation, PATHFINDING_MODE.PASSABLE, () => EndAction(iparty as CharacterParty, targetObject));
    }
    #endregion
}
