using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Pathfinding;
using System;

public class LandmarkVisual : MonoBehaviour {

    private BaseLandmark _landmark;

    [SerializeField] private TextMeshProUGUI landmarkLbl;
    [SerializeField] private TextMeshProUGUI charCountLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;
    [SerializeField] private GameObject landmarkNameplateGO;
    [SerializeField] private GameObject characterIndicatorGO;
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private Slider hpProgressBar;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private Transform hoverContent;
    [SerializeField] private Transform playerContent;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private Seeker seeker;
    [SerializeField] private AIDestinationSetter destinationSetter;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private InteractionTimer interactionTimer;
    public GameObject landmarkHPGO;

    private int _charCount;

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
            if (!string.IsNullOrEmpty(_landmark.landmarkName)) {
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
    public void OnCharacterEnteredLandmark(Party iparty) {
        //add character portrait to grid
        CharacterPortrait portrait = iparty.icon.characterPortrait;
        //if(iparty.mainCharacter is ECS.Character && iparty.mainCharacter.role.roleType == CHARACTER_ROLE.PLAYER) {
        //    portrait.transform.SetParent(playerContent);
        //} else {
            portrait.transform.SetParent(hoverContent);
            portrait.ignoreInteractions = true;
            UpdateCharCount();
        //}
        portrait.transform.localScale = Vector3.one;
        //(portrait.transform as RectTransform).pivot = new Vector2(0.5f, 0f);
        (portrait.transform as RectTransform).sizeDelta = new Vector2(64, 64);
        //portrait.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        ShowPartyPortrait(iparty);
        iparty.icon.SetVisualState(false);
    }
    public void OnCharacterExitedLandmark(Party iparty) {
        //remove character portrait from grid
        //iparty.icon.gameObject.SetActive(true);
        if (!iparty.icon.avatarVisual.activeSelf) {
            iparty.icon.ReclaimPortrait();
            UpdateCharCount();
            //if (iparty.mainCharacter is ECS.Character && iparty.mainCharacter.role.roleType == CHARACTER_ROLE.PLAYER) {
            //    iparty.icon.ReclaimPortraitAsPlayer();
            //} else {
            //    iparty.icon.ReclaimPortrait();
            //    AdjustCharCount(-1);
            //}
            iparty.icon.SetVisualState(true);
            iparty.icon.characterPortrait.SetBorderState(false);
        }
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

    public void SnapTo(RectTransform target) {
        Canvas.ForceUpdateCanvases();

        charactersScrollView.content.anchoredPosition =
            (Vector2)charactersScrollView.transform.InverseTransformPoint(charactersScrollView.content.position)
            - (Vector2)charactersScrollView.transform.InverseTransformPoint(target.position);
    }

    public void ShowHPAndName(bool state) {
        if (_landmark.tileLocation.areaOfTile == null) {
            landmarkNameplateGO.SetActive(state);
        } else {
            if (state) {
                UIManager.Instance.ShowSmallInfo(_landmark.tileLocation.areaOfTile.name);
            }
        }
        landmarkHPGO.SetActive(state);
    }
    private void AdjustCharCount(int amount) {
        _charCount += amount;
        charCountLbl.text = _charCount.ToString();
        if (_charCount > 0) {
            characterIndicatorGO.SetActive(true);
        } else {
            characterIndicatorGO.SetActive(false);
        }
    }
    private void UpdateCharCount() {
        int charCount = _landmark.charactersAtLocation.Count;
        if (!GameManager.Instance.allCharactersAreVisible && !_landmark.isBeingInspected) {
            charCount = _landmark.GetInspectedCharactersCount();
        }
        charCountLbl.text = "" + charCount;
        if(charCount > 0) {
            characterIndicatorGO.SetActive(true);
        } else {
            characterIndicatorGO.SetActive(false);
        }
    }
    public void ShowPartyPortrait(Party party) {
        CharacterPortrait portrait = party.icon.characterPortrait;
        if (!GameManager.Instance.allCharactersAreVisible) {
            if (_landmark.isBeingInspected) {
                portrait.gameObject.SetActive(true);
                portrait.SetBorderState(true);
            } else {
                if (party.IsPartyBeingInspected()) {
                    portrait.gameObject.SetActive(true);
                    portrait.SetBorderState(true);
                } else {
                    portrait.gameObject.SetActive(false);
                    portrait.SetBorderState(false);
                }
            }
        } else {
            portrait.gameObject.SetActive(true);
            portrait.SetBorderState(true);
        }
    }
    public void ToggleCharactersVisibility() {
        UpdateCharCount();
        for (int i = 0; i < _landmark.charactersAtLocation.Count; i++) {
            ShowPartyPortrait(_landmark.charactersAtLocation[i]);
        }
    }
    #region Monobehaviour
    //private void OnMouseOver() {
    //    _landmark.tileLocation.MouseOver();
    //    if (Input.GetMouseButtonDown(0)) {
    //        _landmark.tileLocation.LeftClick();
    //    }
    //    if (Input.GetMouseButtonDown(1)) {
    //        _landmark.tileLocation.RightClick();
    //    }
    //}
    //private void OnMouseExit() {
    //    _landmark.tileLocation.MouseExit();
    //}
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

    #region Pointer Actions
    public void ShowCharactersInLandmark(bool state) {
        hoverContent.gameObject.SetActive(state);
    }
    #endregion

    #region Interaction
    public void ShowInteractionTimer() {
        interactionTimer.gameObject.SetActive(true);
    }
    public void HideInteractionTimer() {
        interactionTimer.gameObject.SetActive(false);
    }
    public void SetAndStartInteractionTimer(int ticks, InteractionTimer.OnStopTimer stopTimerAction = null) {
        if(stopTimerAction != null) {
            interactionTimer.onStopTimer += stopTimerAction;
        }
        interactionTimer.SetTimer(ticks);
        interactionTimer.StartTimer();
    }
    public void StopInteractionTimer() {
        interactionTimer.StopTimer();
    }
    public void ResetInteractionTimer() {
        interactionTimer.ResetTimer();
    }
    public void SetInteractionTimerButtonState(bool state) {
        interactionTimer.interactionButton.interactable = state;
    }
    public void OnClickInteractionButton() {
        Messenger.Broadcast(Signals.CLICKED_INTERACTION_BUTTON, _landmark);
    }
    #endregion
}
