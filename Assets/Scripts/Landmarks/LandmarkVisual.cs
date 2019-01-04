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
    //[SerializeField] private TextMeshProUGUI charCountLbl;
    [SerializeField] private SpriteRenderer topSprite;
    [SerializeField] private SpriteRenderer botSprite;
    [SerializeField] private GameObject exploredGO;
    [SerializeField] private GameObject landmarkNameplateGO;
    //[SerializeField] private GameObject characterIndicatorGO;
    [SerializeField] private SpriteRenderer iconSprite;
    [SerializeField] private Slider hpProgressBar;
    //[SerializeField] private ScrollRect charactersScrollView;
    //[SerializeField] private Transform hoverContent;
    //[SerializeField] private Transform playerContent;
    //[SerializeField] private AIPath aiPath;
    //[SerializeField] private Seeker seeker;
    //[SerializeField] private AIDestinationSetter destinationSetter;
    //[SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private InteractionTimer interactionTimer;
    [SerializeField] private InteractionTimer interactionTimerJobAction;
    [SerializeField] private TweenRotation interactionTimerTween;
    [SerializeField] private TweenRotation interactionTimerJobActionTween;
    public Canvas landmarkCanvas;
    public GameObject landmarkHPGO;

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
        Messenger.AddListener<bool>(Signals.PAUSED, OnPauseGame);
    }
    private void OnPauseGame(bool state) {
        if (interactionTimer.gameObject.activeSelf) {
            interactionTimerTween.enabled = !state;
        }
        if (interactionTimerJobAction.gameObject.activeSelf) {
            interactionTimerJobActionTween.enabled = !state;
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
        //CharacterPortrait portrait = iparty.icon.characterPortrait;
        //portrait.transform.SetParent(hoverContent);
        //portrait.ignoreInteractions = true;
        //UpdateCharCount();
        //portrait.transform.localScale = Vector3.one;
        //(portrait.transform as RectTransform).sizeDelta = new Vector2(64, 64);
        //ShowPartyPortrait(iparty);
        //iparty.icon.SetVisualState(false);
    }
    public void OnCharacterExitedLandmark(Party iparty) {
        //remove character portrait from grid
        //iparty.icon.gameObject.SetActive(true);
        //if (!iparty.icon.avatarVisual.activeSelf) {
            //iparty.icon.ReclaimPortrait();
            //UpdateCharCount();
            //iparty.icon.SetVisualState(true);
            //iparty.icon.characterPortrait.SetBorderState(false);
        //}
    }
    //public void SnapTo(RectTransform target) {
    //    Canvas.ForceUpdateCanvases();

    //    charactersScrollView.content.anchoredPosition =
    //        (Vector2)charactersScrollView.transform.InverseTransformPoint(charactersScrollView.content.position)
    //        - (Vector2)charactersScrollView.transform.InverseTransformPoint(target.position);
    //}

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
    //public void ShowPartyPortrait(Party party) {
    //    CharacterPortrait portrait = party.icon.characterPortrait;
    //    if (!GameManager.Instance.allCharactersAreVisible) {
    //        if (_landmark.isBeingInspected) {
    //            portrait.gameObject.SetActive(true);
    //            portrait.SetBorderState(true);
    //        } else {
    //            if (party.IsPartyBeingInspected()) {
    //                portrait.gameObject.SetActive(true);
    //                portrait.SetBorderState(true);
    //            } else {
    //                portrait.gameObject.SetActive(false);
    //                portrait.SetBorderState(false);
    //            }
    //        }
    //    } else {
    //        portrait.gameObject.SetActive(true);
    //        portrait.SetBorderState(true);
    //    }
    //}

    #region Interaction
    public void ShowInteractionTimer(Interaction interaction) {
        interactionTimer.gameObject.SetActive(true);
        interactionTimer.ShowInteractionForeground(interaction);
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
    public void SetTimerPauseState(bool state) {
        interactionTimer.SetPauseState(state);
    }

    public void ShowInteractionTimerJob() {
        interactionTimerJobAction.gameObject.SetActive(true);
        interactionTimerJobAction.ShowNoInteractionForeground();
    }
    public void HideInteractionTimerJob() {
        interactionTimerJobAction.gameObject.SetActive(false);
    }
    public void SetAndStartInteractionTimerJob(int ticks, InteractionTimer.OnStopTimer stopTimerAction = null) {
        if (stopTimerAction != null) {
            interactionTimerJobAction.onStopTimer += stopTimerAction;
        }
        interactionTimerJobAction.SetTimer(ticks);
        interactionTimerJobAction.StartTimer();
    }
    public void StopInteractionTimerJob() {
        interactionTimerJobAction.StopTimer();
    }
    public void ResetInteractionTimerJob() {
        interactionTimerJobAction.ResetTimer();
    }
    public void SetTimerPauseStateJob(bool state) {
        interactionTimerJobAction.SetPauseState(state);
    }
    public void OnClickInteractionButton() {
        interactionTimer.OnClickTimer();
    }
    #endregion
}
