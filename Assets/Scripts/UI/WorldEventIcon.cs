using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEventIcon : MonoBehaviour {

    private BaseLandmark landmark;

    public void PlaceAt(BaseLandmark landmark) {
        this.landmark = landmark;
        this.transform.position = landmark.tileLocation.transform.position;
        this.transform.localScale = Vector3.one;
    }

    private void UpdatePosition() {
        if (landmark == null) {
            return;
        }
        //Vector2 originalPos = area.coreTile.transform.position;
        //originalPos.y -= 1f;
        //Vector2 ScreenPosition = Camera.main.WorldToScreenPoint(area.nameplatePos);
        this.transform.position = landmark.tileLocation.transform.position;
    }

    public void LateUpdate() {
        UpdatePosition();
    }
}
