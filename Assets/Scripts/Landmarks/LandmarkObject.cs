using UnityEngine;
using System.Collections;

public class LandmarkObject : MonoBehaviour {

    private Landmark _landmark;

    [SerializeField] private UILabel landmarkLbl;

    public void SetLandmark(Landmark landmark) {
        _landmark = landmark;
        landmarkLbl.text = Utilities.NormalizeString(landmark.landmarkType.ToString());
    }
}
