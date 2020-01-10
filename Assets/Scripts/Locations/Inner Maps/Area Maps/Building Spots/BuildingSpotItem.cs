using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using TMPro;
using UnityEngine;

public class BuildingSpotItem : MonoBehaviour {

    private BuildingSpot buildingSpot;

    [SerializeField] private TextMeshPro text;
    [SerializeField] private BoxCollider2D pathfindingBlocker;
    
    public void SetBuildingSpot(BuildingSpot spot) {
        this.name = spot.id.ToString();
        this.buildingSpot = spot;
        Messenger.AddListener<BuildingSpot, bool>(Signals.MODIFY_BUILD_SPOT_WALKABILITY, SetWalkableState);
    }
    private void SetWalkableState(BuildingSpot spot, bool state) {
        if (spot == buildingSpot) {
            //if the state is set to walkable, then disable the pathfinding blocker
            pathfindingBlocker.gameObject.SetActive(!state);    
        }
    }
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector3 position = this.transform.position;
        Gizmos.DrawWireCube(position, new Vector3(InnerMapManager.BuildingSpotSize.x - 0.5f, InnerMapManager.BuildingSpotSize.y - 0.5f, 0));    
    }

}
