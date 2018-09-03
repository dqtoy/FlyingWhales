using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : CharacterAction {
    public MoveAction() : base(ACTION_TYPE.MOVE) {

    }

    #region Overrides
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        iparty.GoToLocation(targetObject.objectLocation, PATHFINDING_MODE.PASSABLE);
    }
    #endregion
}
