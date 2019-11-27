﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildingSpotItem : MonoBehaviour {

    private BuildingSpot buildingSpot;

    [SerializeField] private TextMeshPro text;

    public void SetBuildingSpot(BuildingSpot spot) {
        this.name = spot.id.ToString();
        this.buildingSpot = spot;
    }

    private void Update() {
        if (buildingSpot != null) {
            text.text = buildingSpot.id.ToString();
            //text.text += $"\n<size=10%>isOpen {buildingSpot.isOpen.ToString()}, isOccupied {buildingSpot.isOccupied.ToString()}";
            //if (buildingSpot.neighbours != null) {
            //    text.text += "\nNeighbours:";
            //    foreach (KeyValuePair<GridNeighbourDirection, BuildingSpot> keyValuePair in buildingSpot.neighbours) {
            //        text.text += $"\n\t{keyValuePair.Key.ToString()} - {keyValuePair.Value.id.ToString()}";
            //    }
                
            //}
            
        }
    }

    void OnDrawGizmos() {
        if (buildingSpot != null) {
            Vector3 position = this.transform.position;
            if (buildingSpot.isOpen) {
                Gizmos.color = Color.white;
            } else {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireCube(position, InteriorMapManager.Building_Spot_Size);
        }
    }

}