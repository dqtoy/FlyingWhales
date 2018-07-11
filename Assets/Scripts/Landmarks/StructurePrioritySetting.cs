using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructurePrioritySetting {

    public ACTION_TYPE actionType;
    public List<Resource> resourceCost;
    public LANDMARK_TYPE landmarkType;

    public StructurePrioritySetting(ACTION_TYPE actionType, List<Resource> resourceCost, LANDMARK_TYPE landmarkType) {
        this.actionType = actionType;
        this.resourceCost = resourceCost;
        this.landmarkType = landmarkType;
    }
    public StructurePrioritySetting() {
        this.actionType = ACTION_TYPE.ABDUCT;
        this.resourceCost = new List<Resource>();
        this.landmarkType = LANDMARK_TYPE.DEMONIC_PORTAL;
    }

    public void AddResourceCost(Resource newCost) {
        resourceCost.Add(newCost);
    }
    public void RemoveResourceCost(Resource cost) {
        resourceCost.Remove(cost);
    }
}
