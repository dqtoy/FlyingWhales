using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerBuildLandmarkUI : MonoBehaviour {
    [Header("General")]
    public Button buildBtn;
    public Image buildProgress;
    public TextMeshProUGUI descriptionLbl;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    [Header("Landmark")]
    public TextMeshProUGUI landmarkText;
    public Image landmarkImg;
    public Button selectLandmarkBtn;

    public Minion chosenMinion { get; private set; }
    public LANDMARK_TYPE chosenLandmark { get; private set; }

    public HexTile currentTile { get; private set; }

    #region General
    public void ShowPlayerBuildLandmarkUI(HexTile tile) {
        currentTile = tile;

        if(currentTile.region.demonicBuildingData.landmarkType == LANDMARK_TYPE.NONE) {
            chosenMinion = null;
            chosenLandmark = LANDMARK_TYPE.NONE;
            buildBtn.interactable = false;
            buildProgress.fillAmount = 0;
            minionName.gameObject.SetActive(false);
            minionPortrait.gameObject.SetActive(false);
            landmarkText.gameObject.SetActive(false);
            landmarkImg.gameObject.SetActive(false);
            selectMinionBtn.interactable = true;
            selectLandmarkBtn.interactable = true;
        } else {
            SetChosenMinion(currentTile.region.assignedMinion.character);
            SetChosenLandmark(currentTile.region.demonicBuildingData.landmarkName);
            UpdateSelectMinionBtn();
            UpdateSelectLandmarkBtn();
            UpdatePlayerBuildLandmarkUI();
        }
        UpdateDescriptionText();
        gameObject.SetActive(true);
    }
    public void HidePlayerBuildLandmarkUI() {
        gameObject.SetActive(false);
    }
    public void OnClickBuild() {
        currentTile.region.StartBuildingStructure(chosenLandmark, chosenMinion);
        UpdateBuildButton();
        UpdateSelectMinionBtn();
        UpdateSelectLandmarkBtn();
    }
    private void UpdateBuildButton() {
        buildProgress.gameObject.SetActive(false);
        buildBtn.interactable = chosenMinion != null && chosenLandmark != LANDMARK_TYPE.NONE 
            && currentTile.region.demonicBuildingData.landmarkType == LANDMARK_TYPE.NONE && !currentTile.HasTileTag(TILE_TAG.HALLOWED_GROUNDS);
        if (!buildBtn.interactable) {
            if(currentTile.region.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
                buildProgress.gameObject.SetActive(true);
                buildProgress.fillAmount = 0;
            }
        }
    }
    public void UpdatePlayerBuildLandmarkUI() {
        if(currentTile.region.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE && buildProgress.gameObject.activeSelf) {
            buildProgress.fillAmount = currentTile.region.demonicBuildingData.currentDuration / (float)currentTile.region.demonicBuildingData.buildDuration;
        }
    }
    private void UpdateDescriptionText() {
        List<string> cannotBuildReasons;
        if (!currentTile.CanBuildDemonicLandmarksOnTile(out cannotBuildReasons)) {
            descriptionLbl.text = "You cannot build on this because of the following reasons: ";
            for (int i = 0; i < cannotBuildReasons.Count; i++) {
                descriptionLbl.text += "\n- " + cannotBuildReasons[i];
            }
        } else {
            descriptionLbl.text = "Assign a minion here to build a Demonic Structure of your choosing.";
        }
        descriptionLbl.gameObject.SetActive(true);
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion to Build Structure";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.CONSTRUCT);
    }
    private void SetChosenMinion(Character character) {
        chosenMinion = character.minion;
        minionPortrait.GeneratePortrait(chosenMinion.character);
        minionName.text = chosenMinion.character.name;
        minionPortrait.gameObject.SetActive(true);
        minionName.gameObject.SetActive(true);
        UpdateBuildButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        selectMinionBtn.interactable = currentTile.region.demonicBuildingData.landmarkType == LANDMARK_TYPE.NONE && !currentTile.HasTileTag(TILE_TAG.HALLOWED_GROUNDS);
    }
    #endregion

    #region Landmark
    public void OnClickSelectLandmark() {
        List<string> landmarkNames = new List<string>();
        for (int i = 0; i < PlayerManager.Instance.allLandmarksThatCanBeBuilt.Length; i++) {
            landmarkNames.Add(Utilities.NormalizeStringUpperCaseFirstLetters(PlayerManager.Instance.allLandmarksThatCanBeBuilt[i].ToString()));
        }
        string title = "Select Structure to Build";
        UIManager.Instance.ShowClickableObjectPicker(landmarkNames, SetChosenLandmark, null, CanChooseLandmark, title, OnHoverLandmarkChoice, OnHoverExitLandmarkChoice, "landmark");
    }
    private bool CanChooseLandmark(string landmarkName) {
        return true;
    }
    private void OnHoverLandmarkChoice(string landmarkName) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        string info = data.buildDescription;
        if (info != string.Empty) {
            info += "\n";
        }
        info += "Duration: " + GameManager.Instance.GetCeilingHoursBasedOnTicks(data.buildDuration) + " hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitLandmarkChoice(string landmarkName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void SetChosenLandmark(string landmarkName) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        chosenLandmark = data.landmarkType;
        landmarkImg.sprite = data.landmarkPortrait;
        landmarkText.text = data.landmarkTypeString;
        landmarkImg.gameObject.SetActive(true);
        landmarkText.gameObject.SetActive(true);
        UpdateBuildButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectLandmarkBtn() {
        selectLandmarkBtn.interactable = currentTile.region.demonicBuildingData.landmarkType == LANDMARK_TYPE.NONE && !currentTile.HasTileTag(TILE_TAG.HALLOWED_GROUNDS);
    }
    #endregion
}
