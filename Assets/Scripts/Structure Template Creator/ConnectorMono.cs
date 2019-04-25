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
}
