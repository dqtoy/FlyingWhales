using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlayerBuildLandmarkUI : MonoBehaviour {
    [Header("General")]
    public Button buildBtn;
    public Image buildProgress;

    public HexTile currentTile { get; private set; }

    #region General
    public void ShowPlayerBuildLandmarkUI(HexTile tile) {
        currentTile = tile;
        UpdatePlayerBuildLandmarkUI();
        gameObject.SetActive(true);
    }
    public void HidePlayerBuildLandmarkUI() {
        gameObject.SetActive(false);
    }
    public void OnClickBuild() {
        List<string> landmarkNames = new List<string>();
        for (int i = 0; i < PlayerManager.Instance.allLandmarksThatCanBeBuilt.Length; i++) {
            landmarkNames.Add(Utilities.NormalizeStringUpperCaseFirstLetters(PlayerManager.Instance.allLandmarksThatCanBeBuilt[i].ToString()));
        }
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), landmarkNames,
            "Choose a minion", "Choose a structure",
            CanChooseMinion, CanChooseLandmark,
            null, OnHoverLandmarkChoice,
            null, OnHoverExitLandmarkChoice,
            StartBuild, "Build");
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.BUILDER);
    }
    private bool CanChooseLandmark(string landmarkName) {
        if (landmarkName == "The Pit" || landmarkName == "The Fingers") {
            return false;
        }
        return true;
    }
    private void OnHoverLandmarkChoice(string landmarkName) {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        string info = data.description;
        if (info != string.Empty) {
            info += "\n";
        }
        info += "Duration: " + GameManager.Instance.GetCeilingHoursBasedOnTicks(data.buildDuration) + " hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    private void OnHoverExitLandmarkChoice(string landmarkName) {
        UIManager.Instance.HideSmallInfo();
    }
    private void StartBuild(object minionObj, object landmarkObj) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkObj as string);
        currentTile.region.StartBuildingStructure(landmarkData.landmarkType, (minionObj as Character).minion);
        UpdateBuildButton();
    }
    private void UpdateBuildButton() {
        buildProgress.gameObject.SetActive(false);
        buildBtn.interactable = currentTile.region.assignedMinion != null && currentTile.region.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE 
            && currentTile.region.demonicBuildingData.landmarkType == LANDMARK_TYPE.NONE && !currentTile.region.HasFeature(RegionFeatureDB.Hallowed_Ground_Feature);
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
    #endregion
}
