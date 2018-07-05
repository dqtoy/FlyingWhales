using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Pathfinding;

public class LandmarkObject : MonoBehaviour {

    private BaseLandmark _landmark;

    [SerializeField] private TextMeshProUGUI landmarkLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private Slider hpProgressBar;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private Seeker seeker;
    [SerializeField] private AIDestinationSetter destinationSetter;
    [SerializeField] private LineRenderer lineRenderer;

    #region getters/setters
    public BaseLandmark landmark {
        get { return _landmark; }
    }
    #endregion

    public void SetLandmark(BaseLandmark landmark) {
        _landmark = landmark;
        UpdateName();
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_landmark.specificLandmarkType);
        if (data.landmarkObjectSprite != null) {
            iconSprite.sprite = data.landmarkObjectSprite;
        }
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
        iconSprite.gameObject.SetActive(state);
    }
    public void UpdateProgressBar() {
        hpProgressBar.value = (float) _landmark.landmarkObj.currentHP / (float) _landmark.landmarkObj.maxHP;
    }
    public void OnCharacterEnteredLandmark(IParty iparty) {
        //add character portrait to grid
        CharacterPortrait portrait = iparty.icon.characterPortrait;
        portrait.transform.SetParent(charactersScrollView.content.transform);
        portrait.transform.localScale = Vector3.one;
        (portrait.transform as RectTransform).sizeDelta = new Vector2(65, 65);
        portrait.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        portrait.gameObject.SetActive(true);
        //iparty.icon.gameObject.SetActive(false);
        iparty.icon.SetVisualState(false);
    }
    public void OnCharacterExitedLandmark(IParty iparty) {
        //remove character portrait from grid
        iparty.icon.ReclaimPortrait();
        //iparty.icon.gameObject.SetActive(true);
        iparty.icon.SetVisualState(true);
    }
    public void DrawPathTo(BaseLandmark otherLandmark) {
        if (destinationSetter.target != otherLandmark.tileLocation.transform) {
            destinationSetter.target = otherLandmark.tileLocation.transform;
            aiPath.SearchPath();
        }
    }
    public void HidePathVisual() {
        destinationSetter.target = null;
        lineRenderer.positionCount = 0;
        //lineRenderer.gameObject.SetActive(false);
    }
    #region Monobehaviour
    private void OnMouseOver() {
        _landmark.tileLocation.MouseOver();
        if (Input.GetMouseButtonDown(0)) {
            _landmark.tileLocation.LeftClick();
        }
        if (Input.GetMouseButtonDown(1)) {
            _landmark.tileLocation.RightClick();
        }
    }
    private void OnMouseExit() {
        _landmark.tileLocation.MouseExit();
    }
    //private void OnMouseDown() {
    //    if (Input.GetMouseButtonDown(0)) {
    //        _landmark.tileLocation.LeftClick();
    //    }
    //    if (Input.GetMouseButtonDown(1)) {
    //        _landmark.tileLocation.RightClick();
    //    }
    //}
    private void Update() {
        if (destinationSetter.target != null) {
            Path path = seeker.GetCurrentPath();
            if (path != null && path.vectorPath.Count > 0) {
                List<Vector3> vectorPath = path.vectorPath;
                lineRenderer.positionCount = vectorPath.Count;
                lineRenderer.SetPositions(vectorPath.ToArray());
                
            }
        }
    }
    #endregion
}
