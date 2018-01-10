using UnityEngine;
using System.Collections;

public class GoToRegion : QuestAction {

    private Region _target;

    #region overrides
    public override void InititalizeAction(Region target) {
        _target = target;
    }
    #endregion
}
