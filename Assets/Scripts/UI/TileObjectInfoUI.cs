using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Inner_Maps;

public class TileObjectInfoUI : UIMenu {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    [Space(10)]
    [Header("Users")]
    [SerializeField] private Toggle charactersToggle;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject charactersGO;

    public TileObject activeTileObject { get; private set; }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        if(activeTileObject != null && activeTileObject.mapVisual != null) {
            activeTileObject.mapVisual.UnlockHoverObject();
            activeTileObject.mapVisual.SetHoverObjectState(false);
            if (InnerMapCameraMove.Instance.target == activeTileObject.collisionTrigger.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeTileObject = null;
    }
    public override void OpenMenu() {
        TileObject previousTileObject = activeTileObject;
        if (previousTileObject != null && previousTileObject.mapVisual != null) {
            previousTileObject.mapVisual.UnlockHoverObject();
            previousTileObject.mapVisual.SetHoverObjectState(false);    
        }
        
        activeTileObject = _data as TileObject;
        if(activeTileObject.gridTileLocation != null) {
            bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(activeTileObject.currentRegion);
            InnerMapCameraMove.Instance.CenterCameraOn(activeTileObject.collisionTrigger.gameObject, instantCenter);
        }
        activeTileObject.mapVisual.SetHoverObjectState(true);
        activeTileObject.mapVisual.LockHoverObject();
        base.OpenMenu();
        UIManager.Instance.HideObjectPicker();
        UpdateCharactersToggle();
        UpdateTileObjectInfo();
        UpdateCharacters();
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
        if(activeTileObject is ResourcePile) {
            nameLbl.text += " (x" + (activeTileObject as ResourcePile).resourceInPile + ")";
        }else if (activeTileObject is Table) {
            nameLbl.text += " (x" + (activeTileObject as Table).food + ")";
        }
        if (activeTileObject.isDisabledByPlayer) {
            nameLbl.text += " (Disabled)";
        }
    }
    private void UpdateCharactersToggle() {
        if (activeTileObject.users != null) {
            charactersToggle.isOn = false;
            charactersToggle.gameObject.SetActive(true);
        } else {
            charactersToggle.isOn = false;
            charactersToggle.gameObject.SetActive(false);
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


    #region Actions
    protected override void LoadActions() {
        Utilities.DestroyChildren(actionsTransform);

        ActionItem destroyItem = AddNewAction("Destroy", null, Destroy);
        destroyItem.SetInteractable(CanBeDestroyed());

        ActionItem ignitedItem = AddNewAction("Ignite", null, Ignite);
        ignitedItem.SetInteractable(CanBeIgnited());

        ActionItem poisonItem = AddNewAction("Poison", null, Poison);
        poisonItem.SetInteractable(CanBePoisoned());

        ActionItem animateItem = AddNewAction("Animate", null, Animate);

        ActionItem seizeItem = AddNewAction("Seize", null, () => PlayerManager.Instance.player.seizeComponent.SeizePOI(activeTileObject));
        bool isInteractable = !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && activeTileObject.mapVisual != null && (activeTileObject.isBeingCarriedBy != null || activeTileObject.gridTileLocation != null);
        seizeItem.SetInteractable(isInteractable);
    }
    #endregion

    #region Destroy
    protected void Destroy() {
        TileObject tileObject = activeTileObject;
        CloseMenu();
        PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.DESTROY].ActivateAbility(tileObject);
    }
    protected bool CanBeDestroyed() {
        return PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.DESTROY].CanPerformAbilityTowards(activeTileObject);
    }
    #endregion

    #region Ignite
    protected void Ignite() {
        PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.IGNITE].ActivateAbility(activeTileObject);
        //Find better way to load only the button that was clicked
        LoadActions();
    }
    protected bool CanBeIgnited() {
        return PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.IGNITE].CanPerformAbilityTowards(activeTileObject);
    }
    #endregion

    #region Poison
    protected void Poison() {
        PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.SPOIL].ActivateAbility(activeTileObject);
        //Find better way to load only the button that was clicked
        LoadActions();
    }
    protected bool CanBePoisoned() {
        return PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.SPOIL].CanPerformAbilityTowards(activeTileObject);
    }
    #endregion

    #region Animate
    protected void Animate() {
        //Animate
    }
    #endregion
}
