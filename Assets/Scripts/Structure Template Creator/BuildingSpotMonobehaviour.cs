using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class BuildingSpotMonobehaviour : MonoBehaviour {

    public int id;
    public List<BuildingSpotMonobehaviour> adjacentSpots;
    public TextMeshPro idText;
    public bool isOpen = false;

    private void Update() {
        id = transform.GetSiblingIndex();
        idText.text = id.ToString();
        string summary = id + " - " + this.transform.localPosition.ToString();
        this.name = summary;
    }

    public BuildingSpot Convert() {
        int[] converted = new int[adjacentSpots.Count];
        for (int i = 0; i < adjacentSpots.Count; i++) {
            BuildingSpotMonobehaviour currSpot = adjacentSpots[i];
            converted[i] = currSpot.id;
        }
        return new BuildingSpot() {
            id = this.id,
            location = new Vector3Int((int)(transform.localPosition.x), (int)(transform.localPosition.y), 0),
            isOpen = this.isOpen,
            adjacentSpots = converted
        };
    }
}
