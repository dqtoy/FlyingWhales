using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterAIPath : AIPath {
    public CharacterMarker marker;
    public int doNotMove { get; private set; }
    public bool isStopMovement { get; private set; }
    public Path currentPath { get; private set; }

    public override void OnTargetReached() {
        base.OnTargetReached();
        marker.ArrivedAtLocation();
        currentPath = null;
    }

    protected override void OnPathComplete(Path newPath) {
        base.OnPathComplete(newPath);
        currentPath = newPath;
    }

    public override void UpdateMe() {
        marker.UpdatePosition();
         marker.visualsParent.localRotation = Quaternion.LookRotation(Vector3.forward, this.velocity);
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
    //NOTE: use SetDestination in CharacterDestinationSetter instead
    //public void SetDestination(Vector3 target) {
    //    destination = target;
    //}

}
