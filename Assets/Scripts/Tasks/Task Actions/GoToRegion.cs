using UnityEngine;
using System.Collections;

public class GoToRegion : TaskAction {

    private Region _target;

    public GoToRegion(OldQuest.Quest quest) : base(quest) {
    }

    #region overrides
    public override void InititalizeAction(Region target) {
        _target = target;
    }
    #endregion
}
