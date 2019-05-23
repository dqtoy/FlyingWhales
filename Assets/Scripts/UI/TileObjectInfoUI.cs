using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TileObjectInfoUI : UIMenu {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    public TileObject activeTileObject { get; private set; }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        activeTileObject = null;
    }
    public override void OpenMenu() {
        activeTileObject = _data as TileObject;
        bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != activeTileObject.specificLocation;
        AreaMapCameraMove.Instance.CenterCameraOn(activeTileObject.collisionTrigger.gameObject, instantCenter);
        base.OpenMenu();
        UIManager.Instance.HideObjectPicker();
        UpdateBasicInfo();
        UpdateTileObjectInfo();
    }
    #endregion

    public void UpdateTileObjectInfo() {
        if(activeTileObject == null) {
            return;
        }
    }
    private void UpdateBasicInfo() {
        nameLbl.text = activeTileObject.name;
    }
}
