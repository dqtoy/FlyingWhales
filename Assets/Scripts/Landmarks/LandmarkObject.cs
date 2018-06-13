using UnityEngine;
using System.Collections;
using TMPro;

public class LandmarkObject : MonoBehaviour {

    private BaseLandmark _landmark;

    [SerializeField] private GameObject nameplateGO;
    [SerializeField] private TextMeshProUGUI landmarkLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private UI2DSprite progressBarSprite;

    #region getters/setters
    public BaseLandmark landmark {
        get { return _landmark; }
    }
    #endregion

    public void SetLandmark(BaseLandmark landmark) {
        _landmark = landmark;
        UpdateName();
        //if (_landmark.specificLandmarkType != LANDMARK_TYPE.TOWN) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_landmark.specificLandmarkType);
        if (data.landmarkObjectSprite != null) {
            iconSprite.sprite = data.landmarkObjectSprite;
            iconSprite.gameObject.SetActive(true);
        } else {
            iconSprite.gameObject.SetActive(false);
        }
        //}
        //UpdateLandmarkVisual();
    }

    public void UpdateName() {
        if (landmarkLbl != null) {
            //Landmark object is an empty city
            if (_landmark.landmarkName != string.Empty) {
                landmarkLbl.text = Utilities.NormalizeString(_landmark.landmarkName);
            } else {
                landmarkLbl.text = Utilities.NormalizeString(_landmark.specificLandmarkType.ToString());
            }
        }
    }

    public void SetBGState(bool state) {
        topSprite.enabled = state;
        botSprite.enabled = state;
    }

    public void UpdateProgressBar() {
        progressBarSprite.fillAmount = (float) _landmark.landmarkObj.currentHP / (float) _landmark.landmarkObj.maxHP;
    }

    //For Testing
    public void SetIconActive(bool state) {
        iconSprite.gameObject.SetActive(state);
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

    #region Monobehaviour
    private void OnMouseOver() {
        _landmark.tileLocation.MouseOver();
    }
    private void OnMouseExit() {
        _landmark.tileLocation.MouseExit();
    }
    private void OnMouseDown() {
        _landmark.tileLocation.LeftClick();
    }
    #endregion
}
