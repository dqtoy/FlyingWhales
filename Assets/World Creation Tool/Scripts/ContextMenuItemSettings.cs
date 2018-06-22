using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContextMenuItemSettings {
    public string menuItemName;
    public UnityAction onHoverEnterAction;
    public UnityAction onHoverExitAction;
    public UnityAction onClickAction;
    public ContextMenuSettings subMenu;

    public ContextMenuItemSettings(string itemName) {
        menuItemName = itemName;
    }

    public void SetSubMenu(ContextMenuSettings subMenu) {
        this.subMenu = subMenu;
    }
}
