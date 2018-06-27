using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContextMenu : MonoBehaviour {
    private List<UIContextMenuItem> items;

    public void LoadSettings(ContextMenuSettings settings) {
        Utilities.DestroyChildren(this.transform);
        for (int i = 0; i < settings.items.Count; i++) {
            ContextMenuItemSettings currItem = settings.items[i];
#if WORLD_CREATION_TOOL
            UIContextMenuItem item = GameObject.Instantiate(worldcreator.WorldCreatorUI.Instance.contextMenuItemPrefab, this.transform).GetComponent<UIContextMenuItem>();
#else
            UIContextMenuItem item = UIManager.Instance.InstantiateUIObject(UIManager.Instance.contextMenuItemPrefab.name, this.transform).GetComponent<UIContextMenuItem>();
#endif
            item.LoadSettings(currItem);
        }
    }
}
