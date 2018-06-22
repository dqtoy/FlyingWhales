using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuSettings {
    public List<ContextMenuItemSettings> items;


    public ContextMenuSettings() {
        items = new List<ContextMenuItemSettings>();
    }

    public void AddMenuItem(ContextMenuItemSettings item) {
        items.Add(item);
    }
}
