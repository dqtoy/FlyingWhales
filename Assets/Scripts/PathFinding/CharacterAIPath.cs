using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterAIPath : AIPath {
    public CharacterMarker marker;
    public int doNotMove { get; private set; }
    public bool isStopMovement { get; private set; }

    public override void OnTargetReached() {
        base.OnTargetReached();
        marker.ArrivedAtLocation();
    }

    public override void UpdateMe() {
        marker.UpdatePosition();
        if (doNotMove > 0 || isStopMovement) { return; }
        base.UpdateMe();
    }
    public void AdjustDoNotMove(int amount) {
        doNotMove += amount;
        doNotMove = Mathf.Max(0, doNotMove);
    }
    public void SetIsStopMovement(bool state) {
        isStopMovement = state;
    }
}
