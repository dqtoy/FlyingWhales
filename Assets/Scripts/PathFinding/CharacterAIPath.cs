using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CharacterAIPath : AIPath {
    public CharacterMarker marker;

    public override void OnTargetReached() {
        base.OnTargetReached();
        marker.ArrivedAtLocation();
    }

    public override void UpdateMe() {
        marker.UpdatePosition();
        base.UpdateMe();
        marker.visualsParent.localRotation = Quaternion.LookRotation(Vector3.forward, this.velocity);
    }

    public void ClearPath() {
        destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    }
    public void SetDestination(Vector3 target) {
        destination = target;
    }
}
