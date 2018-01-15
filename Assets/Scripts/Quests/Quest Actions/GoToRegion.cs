using UnityEngine;
using System.Collections;

public class GoToRegion : QuestAction {

    private Region _target;

    public GoToRegion(Quest quest) : base(quest) {
    }

    #region overrides
    public override void InititalizeAction(Region target) {
        _target = target;
    }
    #endregion
}
