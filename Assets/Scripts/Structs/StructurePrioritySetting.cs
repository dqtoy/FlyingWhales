using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StructurePrioritySetting {

    public ACTION_TYPE actionType;
    public List<Resource> resourceCost;
    public LANDMARK_TYPE landmarkType;

    public StructurePrioritySetting(ACTION_TYPE actionType, List<Resource> resourceCost, LANDMARK_TYPE landmarkType) {
        this.actionType = actionType;
        this.resourceCost = resourceCost;
        this.landmarkType = landmarkType;
    }
}
