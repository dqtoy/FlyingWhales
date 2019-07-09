using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIContextMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    //private ContextMenuItemSettings currItemSettings;
    private UnityAction onHoverEnterAction;
    private UnityAction onHoverOutAction;
    private UnityAction onClickAction;

    [SerializeField] private Text menuItemText;
    [SerializeField] private GameObject nextArrowGO;
    [SerializeField] private UIContextMenu subMenu;
    //[SerializeField] private RectTransform subMenuRT;

    //[SerializeField] private Vector3 rightSubMenuPosition;
    //[SerializeField] private Vector3 leftSubMenuPosition;

    public void LoadSettings(ContextMenuItemSettings settings) {
        //currItemSettings = settings;
        menuItemText.text = settings.menuItemName;
        onHoverEnterAction = settings.onHoverEnterAction;
        onHoverOutAction = settings.onHoverExitAction;
        if (settings.onClickAction != null) {
            onClickAction = settings.onClickAction;
#if WORLD_CREATION_TOOL
            onClickAction += worldcreator.WorldCreatorUI.Instance.HideContextMenu;
#else
            onClickAction += UIManager.Instance.HideContextMenu;
#endif
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
        HideSubMenu();
    }

    public void ShowSubMenu() {
        //Vector3[] v = new Vector3[4];
        //subMenuRT.GetWorldCorners(v);
        //for (int i = 0; i < v.Length; i++) {
        //    Debug.Log(v[i].ToString());
        //}
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
