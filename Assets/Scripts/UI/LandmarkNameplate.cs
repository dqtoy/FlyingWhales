using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LandmarkNameplate : PooledObject {

    private BaseLandmark landmark;

    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private FactionEmblem factionEmblem;
    public void SetLandmark(BaseLandmark landmark) {
        this.landmark = landmark;
        name = landmark.tileLocation.region.name + " Nameplate";
        UpdateVisuals();
        UpdatePosition();
        UpdateFactionEmblem();
    }

    private void UpdateVisuals() {
        nameLbl.text = landmark.tileLocation.region.name;
    }

    private void UpdatePosition() {
        //Vector2 originalPos = settlement.coreTile.transform.position;
        //originalPos.y -= 1f;
        //Vector2 ScreenPosition = Camera.main.WorldToScreenPoint(settlement.nameplatePos);
        this.transform.position = landmark.nameplatePos;
    }

    public void UpdateFactionEmblem() {
        // factionEmblem.gameObject.SetActive(landmark.tileLocation.settlementOnTile.owner != null);
        // if (factionEmblem.gameObject.activeSelf) {
        //     factionEmblem.SetFaction(landmark.tileLocation.settlementOnTile.owner);
        // }
    }

    public void LateUpdate() {
        if (landmark == null) {
            return;
        }
        UpdatePosition();
    }
}

