using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemInfoUI : UIMenu {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    public SpecialToken activeItem { get; private set; }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        activeItem = null;
    }
    public override void OpenMenu() {
        activeItem = _data as SpecialToken;
        if (activeItem.gridTileLocation != null) {
            bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != activeItem.specificLocation;
            AreaMapCameraMove.Instance.CenterCameraOn(activeItem.collisionTrigger.gameObject, instantCenter);
        }
        base.OpenMenu();
        UIManager.Instance.HideObjectPicker();
        //UpdateBasicInfo();
        UpdateTileObjectInfo();
    }
    #endregion

    public void UpdateTileObjectInfo() {
        if (activeItem == null) {
            return;
        }
        UpdateBasicInfo();
        //UpdateCharacters();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = activeItem.name;
        if (activeItem.isDisabledByPlayer) {
            nameLbl.text += " (Disabled)";
        }
    }
}
