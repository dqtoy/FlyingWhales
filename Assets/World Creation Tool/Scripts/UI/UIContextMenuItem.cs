using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIContextMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    private ContextMenuItemSettings currItemSettings;
    private UnityAction onHoverEnterAction;
    private UnityAction onHoverOutAction;
    private UnityAction onClickAction;

    [SerializeField] private Text menuItemText;
    [SerializeField] private GameObject nextArrowGO;
    [SerializeField] private UIContextMenu subMenu;

    public void LoadSettings(ContextMenuItemSettings settings) {
        currItemSettings = settings;
        menuItemText.text = settings.menuItemName;
        onHoverEnterAction = settings.onHoverEnterAction;
        onHoverOutAction = settings.onHoverExitAction;
        if (settings.onClickAction != null) {
            onClickAction = settings.onClickAction;
            onClickAction += worldcreator.WorldCreatorUI.Instance.HideContextMenu;
        } else {
            onClickAction = null;
        }
        

        if (settings.subMenu != null) {
            onHoverEnterAction += ShowSubMenu;
            onHoverOutAction += HideSubMenu;
            subMenu.LoadSettings(settings.subMenu);
            nextArrowGO.gameObject.SetActive(true);
        } else {
            nextArrowGO.gameObject.SetActive(false);
        }
    }

    public void ShowSubMenu() {
        subMenu.gameObject.SetActive(true);
    }

    public void HideSubMenu() {
        subMenu.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (onClickAction != null) {
            onClickAction();
        }
    }
    public void OnPointerEnter(PointerEventData eventData) {
        if (onHoverEnterAction != null) {
            onHoverEnterAction();
        }
    }
    public void OnPointerExit(PointerEventData eventData) {
        if (onHoverOutAction != null) {
            onHoverOutAction();
        }
    }


}
