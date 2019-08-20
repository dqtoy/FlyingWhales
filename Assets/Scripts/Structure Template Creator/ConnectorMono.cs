using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class ConnectorMono : MonoBehaviour {

    public Cardinal_Direction connectionDirection;
    public STRUCTURE_TYPE allowedStructureType;

    [SerializeField] private TextMeshPro connectionSummary;

    private void Update() {
        string summary = connectionDirection.ToString() + " - " + allowedStructureType.ToString();
        this.name = summary;
        connectionSummary.text = summary;
    }

    public StructureConnector GetConnector() {
        return new StructureConnector() {
            allowedStructureType = allowedStructureType,
            neededDirection = connectionDirection,
            location = new Vector3Int((int)(transform.localPosition.x), (int)(transform.localPosition.y), 0),
            isOpen = true
        };
    }
}
