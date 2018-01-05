using UnityEngine;
using System.Collections;

public class LandmarkObject : MonoBehaviour {

    //private BaseLandmark _landmark;

    [SerializeField] private UILabel landmarkLbl;

    public void SetLandmark(BaseLandmark landmark) {
        //_landmark = landmark;
        landmarkLbl.text = Utilities.NormalizeString(landmark.specificLandmarkType.ToString());
    }
}
