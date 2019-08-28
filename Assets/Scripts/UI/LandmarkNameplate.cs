using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LandmarkNameplate : MonoBehaviour {

    private BaseLandmark landmark;

    [SerializeField] private TextMeshProUGUI nameLbl;

    public void SetLandmark(BaseLandmark landmark) {
        this.landmark = landmark;
        name = landmark.tileLocation.region.name + " Nameplate";
        UpdateVisuals();
        UpdatePosition();
    }

    private void UpdateVisuals() {
        nameLbl.text = landmark.tileLocation.region.name;
    }

    private void UpdatePosition() {
        //Vector2 originalPos = area.coreTile.transform.position;
        //originalPos.y -= 1f;
        //Vector2 ScreenPosition = Camera.main.WorldToScreenPoint(area.nameplatePos);
        this.transform.position = landmark.nameplatePos;
    }

    public void LateUpdate() {
        if (landmark == null) {
            return;
        }
        UpdatePosition();
    }
}

