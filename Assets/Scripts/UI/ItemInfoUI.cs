using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
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
        if (activeItem != null && activeItem.mapVisual != null) {
            activeItem.mapVisual.UnlockHoverObject();
            activeItem.mapVisual.SetHoverObjectState(false);
            if (AreaMapCameraMove.Instance.target == activeItem.collisionTrigger.transform) {
                AreaMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeItem = null;
    }
    public override void OpenMenu() {
        SpecialToken previousItem = activeItem;
        if (previousItem != null) {
            previousItem.mapVisual.UnlockHoverObject();
            previousItem.mapVisual.SetHoverObjectState(false);    
        }

        activeItem = _data as SpecialToken;
        if (activeItem.gridTileLocation != null) {
            bool instantCenter = !InnerMapManager.Instance.IsShowingAreaMap(activeItem.currentRegion.area);
            AreaMapCameraMove.Instance.CenterCameraOn(activeItem.collisionTrigger.gameObject, instantCenter);
        }
        activeItem.mapVisual.SetHoverObjectState(true);
        activeItem.mapVisual.LockHoverObject();
        base.OpenMenu();
        UIManager.Instance.HideObjectPicker();
        //UpdateBasicInfo();
        UpdateInfo();
    }
    #endregion

    public void UpdateInfo() {
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

        ActionItem seizeItem = AddNewAction("Seize", null, () => PlayerManager.Instance.player.seizeComponent.SeizePOI(activeItem));
        bool isInteractable = PlayerManager.Instance.player.seizeComponent.seizedPOI == null && activeItem.mapVisual != null && (activeItem.isBeingCarriedBy != null || activeItem.gridTileLocation != null);
        seizeItem.SetInteractable(isInteractable);
    }
    #endregion

    #region Destroy
    protected void Destroy() {
        SpecialToken item = activeItem;
        CloseMenu();
        PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.DESTROY].ActivateAbility(item);
    }
    protected bool CanBeDestroyed() {
        return PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.DESTROY].CanPerformAbilityTowards(activeItem);
    }
    #endregion

    #region Ignite
    protected void Ignite() {
        PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.IGNITE].ActivateAbility(activeItem);
        //Find better way to load only the button that was clicked
        LoadActions();
    }
    protected bool CanBeIgnited() {
        return PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.IGNITE].CanPerformAbilityTowards(activeItem);
    }
    #endregion

    #region Poison
    protected void Poison() {
        PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.SPOIL].ActivateAbility(activeItem);
        //Find better way to load only the button that was clicked
        LoadActions();
    }
    protected bool CanBePoisoned() {
        return PlayerManager.Instance.allInterventionAbilitiesData[INTERVENTION_ABILITY.SPOIL].CanPerformAbilityTowards(activeItem);
    }
    #endregion

    #region Animate
    protected void Animate() {
        //Animate
    }
    #endregion
}
