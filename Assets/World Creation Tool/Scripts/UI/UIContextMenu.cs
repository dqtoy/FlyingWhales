using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContextMenu : MonoBehaviour {
    [SerializeField] private Transform itemsParent;
    [SerializeField] private EnvelopContentUnityUI envelopContent;
    private List<UIContextMenuItem> items;

    public void LoadSettings(ContextMenuSettings settings) {
        Utilities.DestroyChildren(itemsParent);
        for (int i = 0; i < settings.items.Count; i++) {
            ContextMenuItemSettings currItem = settings.items[i];
#if WORLD_CREATION_TOOL
            UIContextMenuItem item = GameObject.Instantiate(worldcreator.WorldCreatorUI.Instance.contextMenuItemPrefab, itemsParent).GetComponent<UIContextMenuItem>();
#else
            UIContextMenuItem item = UIManager.Instance.InstantiateUIObject(UIManager.Instance.contextMenuItemPrefab.name, itemsParent).GetComponent<UIContextMenuItem>();
#endif
            item.LoadSettings(currItem);
        }
        envelopContent.Execute();
    }
}
