using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeTestAIPath : AIPath {

    public System.Action targetReachedAction;

    public override void OnTargetReached() {
        base.OnTargetReached();
        if (targetReachedAction != null) {
            targetReachedAction();
        }
    }
}
