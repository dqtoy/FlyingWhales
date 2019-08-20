using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegionInfoUI : UIMenu {

    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI regionNameLbl;
    [SerializeField] private TextMeshProUGUI regionTypeLbl;

    [Header("Main")]
    [SerializeField] private TextMeshProUGUI worldObjLbl;
    [SerializeField] private Button invadeBtn;
    [SerializeField] private Image invadeProgress;

    public Region activeRegion { get; private set; }

    public override void OpenMenu() {
        base.OpenMenu();
        Region previousRegion = activeRegion;
        if (previousRegion != null) {
            previousRegion.ShowTransparentBorder();
        }

        activeRegion = _data as Region;
        UpdateBasicInfo();
        UpdateRegionInfo();
        UpdateInvadeBtnState();
        activeRegion.CenterCameraOnRegion();
        activeRegion.ShowSolidBorder();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        activeRegion.ShowTransparentBorder();
        activeRegion = null;
    }

    public void UpdateAllInfo() {
        UpdateRegionInfo();
        UpdateInvadeBtnState();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        regionNameLbl.text = activeRegion.name;
        regionTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(activeRegion.mainLandmark.specificLandmarkType.ToString());
    }
    #endregion

    #region Main
    private void UpdateRegionInfo() {
        worldObjLbl.text = "<b>World Object: </b>" + (activeRegion.mainLandmark.worldObj?.worldObjectName ?? "None");
    }
    #endregion

    #region Invade
    private void UpdateInvadeBtnState() {
        invadeBtn.interactable = activeRegion.CanBeInvaded();
        if (activeRegion == PlayerManager.Instance.player.invadingRegion) {
            invadeProgress.gameObject.SetActive(true);
            invadeProgress.fillAmount = ((float)activeRegion.ticksInInvasion / (float)activeRegion.mainLandmark.invasionTicks);
        } else {
            invadeProgress.gameObject.SetActive(false);
        }
    }
    public void OnClickInvade() {
        activeRegion.StartInvasion();
        UpdateInvadeBtnState();
    }
    #endregion
}
