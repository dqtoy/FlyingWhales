using UnityEngine;
using System.Collections;

public class LandmarkObject : MonoBehaviour {

    private BaseLandmark _landmark;

    [SerializeField] private GameObject nameplateGO;
    [SerializeField] private UILabel landmarkLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;
    [SerializeField] private SpriteRenderer iconSprite;

    public void SetLandmark(BaseLandmark landmark) {
        _landmark = landmark;
        if(landmarkLbl != null) {
            //Landmark object is an empty city
			if(_landmark.landmarkName != string.Empty){
				landmarkLbl.text = Utilities.NormalizeString(landmark.landmarkName);
			}else{
				landmarkLbl.text = Utilities.NormalizeString(landmark.specificLandmarkType.ToString());
			}
        }
        if (_landmark.specificLandmarkType != LANDMARK_TYPE.CITY) {
            LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_landmark.specificLandmarkType);
            if (data.landmarkObjectSprite != null) {
                iconSprite.sprite = data.landmarkObjectSprite;
                iconSprite.gameObject.SetActive(true);
            } else {
                iconSprite.gameObject.SetActive(false);
            }
        }
        //UpdateLandmarkVisual();
    }

    //public void UpdateLandmarkVisual() {
    //    if (_landmark.isHidden) {
    //        Color color = Color.white;
    //        color.a = 128f / 255f;
    //        topSprite.color = color;
    //        botSprite.color = color;
    //    } else {
    //        topSprite.color = Color.white;
    //        botSprite.color = Color.white;
    //    }
    //    if(nameplateGO != null) {
    //        nameplateGO.SetActive(!_landmark.isHidden);
    //    }

    //    exploredGO.SetActive(_landmark.isExplored); //Activate explored GO based on isExplored boolean
    //}
}
