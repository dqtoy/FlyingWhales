using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructurePrioritySetting {

    public List<Resource> buildResourceCost;
    public List<Resource> repairResourceCost;
    public LANDMARK_TYPE landmarkType;

    public StructurePrioritySetting(List<Resource> buildCost, List<Resource> repairCost, LANDMARK_TYPE landmarkType) {
        this.buildResourceCost = buildCost;
        this.repairResourceCost = repairCost;
        this.landmarkType = landmarkType;
    }
    public StructurePrioritySetting() {
        this.buildResourceCost = new List<Resource>();
        this.repairResourceCost = new List<Resource>();
        this.landmarkType = LANDMARK_TYPE.THE_PORTAL;
    }

    public void AddBuildResourceCost(Resource newCost) {
        buildResourceCost.Add(newCost);
    }
    public void RemoveBuildResourceCost(Resource cost) {
        buildResourceCost.Remove(cost);
    }

    public void AddRepairResourceCost(Resource newCost) {
        repairResourceCost.Add(newCost);
    }
    public void RemoveRepairResourceCost(Resource cost) {
        repairResourceCost.Remove(cost);
    }
}
