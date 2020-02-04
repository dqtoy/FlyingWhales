using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AreaNameplate : MonoBehaviour {

    private Settlement _settlement;

    [SerializeField] private FactionEmblem emblem;
    [SerializeField] private TextMeshProUGUI areaNameLbl;

    [Header("Residents")]
    [SerializeField] private GameObject residentsGO;
    [SerializeField] private TextMeshProUGUI residentsLbl;

    [Header("Visitors")]
    [SerializeField] private GameObject visitorsGO;
    [SerializeField] private TextMeshProUGUI visitorsLbl;

    public void SetArea(Settlement settlement) {
        this._settlement = settlement;
        name = settlement.name + " Nameplate";
        UpdateVisuals();
        UpdatePosition();

        Messenger.AddListener<Settlement>(Signals.AREA_OWNER_CHANGED, OnAreaOwnerChanged);
    }

    private void UpdateVisuals() {
        emblem.SetFaction(_settlement.owner);
        areaNameLbl.text = _settlement.name;
    }

    private void UpdatePosition() {
        //Vector2 originalPos = settlement.coreTile.transform.position;
        //originalPos.y -= 1f;
        //Vector2 ScreenPosition = Camera.main.WorldToScreenPoint(settlement.nameplatePos);
        this.transform.position = _settlement.nameplatePos;
    }

    public void LateUpdate() {
        UpdatePosition();
    }

    private void OnAreaOwnerChanged(Settlement settlement) {
        if (this._settlement.id == settlement.id) {
            UpdateVisuals();
        }
    }

    public void ShowResidentsAndVisitors() {
        if (!residentsGO.activeSelf) {
            residentsGO.SetActive(true);
            visitorsGO.SetActive(true);
        }
        // residentsLbl.text = _settlement.region.residents.Count.ToString();
        // visitorsLbl.text = _settlement.visitors.Count.ToString();
    }

    public void HideResidentsAndVisitors() {
        residentsGO.SetActive(false);
        visitorsGO.SetActive(false);
    }
}
