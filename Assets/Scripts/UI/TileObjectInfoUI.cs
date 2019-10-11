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

    [Space(10)]
    [Header("Users")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;

    public TileObject activeTileObject { get; private set; }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        activeTileObject = null;
    }
    public override void OpenMenu() {
        activeTileObject = _data as TileObject;
        if(activeTileObject.gridTileLocation != null) {
            bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != activeTileObject.specificLocation;
            AreaMapCameraMove.Instance.CenterCameraOn(activeTileObject.collisionTrigger.gameObject, instantCenter);
            base.OpenMenu();
            UIManager.Instance.HideObjectPicker();
            //UpdateBasicInfo();
            UpdateTileObjectInfo();
            UpdateCharacters();
        }
    }
    #endregion

    public void UpdateTileObjectInfo() {
        if(activeTileObject == null) {
            return;
        }
        UpdateBasicInfo();
        //UpdateCharacters();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = activeTileObject.name;
        if (activeTileObject.isDisabledByPlayer) {
            nameLbl.text += " (Disabled)";
        }
    }
    private void UpdateCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        if (activeTileObject.users != null && activeTileObject.users.Length > 0) {
            for (int i = 0; i < activeTileObject.users.Length; i++) {
                Character character = activeTileObject.users[i];
                if(character != null) {
                    GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
                    CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                    item.SetObject(character);
                    item.SetAsDefaultBehaviour();
                }
            }
        }
    }
}
