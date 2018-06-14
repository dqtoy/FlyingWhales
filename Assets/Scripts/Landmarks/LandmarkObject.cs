using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class LandmarkObject : MonoBehaviour {

    private BaseLandmark _landmark;

    [SerializeField] private GameObject nameplateGO;
    [SerializeField] private TextMeshProUGUI landmarkLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private Slider hpProgressBar;
    [SerializeField] private ScrollRect charactersScrollView;

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
            //iconSprite.gameObject.SetActive(true);
        } else {
            //iconSprite.gameObject.SetActive(false);
        }

        //}
        //UpdateLandmarkVisual();
        ////For Testing of portrait visibility, remove after testing
        //CharacterPortrait[] portraits = Utilities.GetComponentsInDirectChildren<CharacterPortrait>(charactersScrollView.content.gameObject);
        //for (int i = 0; i < portraits.Length; i++) {
        //    CharacterPortrait currPortrait = portraits[i];
        //    currPortrait.GeneratePortrait(CharacterManager.Instance.GenerateRandomPortrait(GENDER.FEMALE));
        //}
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

    public void SetIconState(bool state) {
        //topSprite.enabled = state;
        //botSprite.enabled = state;
        iconSprite.gameObject.SetActive(state);
    }

    public void UpdateProgressBar() {
        hpProgressBar.value = (float) _landmark.landmarkObj.currentHP / (float) _landmark.landmarkObj.maxHP;
    }

    ////For Testing
    //public void SetIconActive(bool state) {
    //    iconSprite.gameObject.SetActive(state);
    //}

    public void OnCharacterEnteredLandmark(ECS.Character character) {
        //add character portrait to grid
        CharacterPortrait portrait = character.icon.characterPortrait;
        portrait.transform.SetParent(charactersScrollView.content.transform);
        portrait.transform.localScale = Vector3.one;
        (portrait.transform as RectTransform).sizeDelta = new Vector2(65, 65);
        portrait.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        portrait.gameObject.SetActive(true);
        character.icon.gameObject.SetActive(false);
    }
    public void OnCharacterExitedLandmark(ECS.Character character) {
        //remove character portrait from grid
        character.icon.ReclaimPortrait();
        character.icon.gameObject.SetActive(true);
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
