using UnityEngine;
using System.Collections;

public class LandmarkObject : MonoBehaviour {

    private BaseLandmark _landmark;

    [SerializeField] private UILabel landmarkLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;

    public void SetLandmark(BaseLandmark landmark) {
        _landmark = landmark;
        if(landmarkLbl != null) {
            //Landmark object is an empty city
            landmarkLbl.text = Utilities.NormalizeString(landmark.specificLandmarkType.ToString());
        }
        UpdateLandmarkVisual();
    }

    public void UpdateLandmarkVisual() {
        if (_landmark.isHidden) {
            Color color = Color.white;
            color.a = 128f / 255f;
            topSprite.color = color;
            botSprite.color = color;
        } else {
            topSprite.color = Color.white;
            botSprite.color = Color.white;
        }

        exploredGO.SetActive(_landmark.isExplored); //Activate explored GO based on isExplored boolean
    }
}
