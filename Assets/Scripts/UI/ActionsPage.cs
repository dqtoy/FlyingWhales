using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsPage : MonoBehaviour {

    public static int Items_Per_Page = 12;
    public ActionItem[] items;

    [SerializeField] private GameObject actionItemPrefab;

    public void Initialize() {
        items = new ActionItem[Items_Per_Page];
        for (int i = 0; i < items.Length; i++) {
            GameObject itemGO =  GameObject.Instantiate(actionItemPrefab, this.transform);
            ActionItem item = itemGO.GetComponent<ActionItem>();
            item.SetAction(null);
            items[i] = item;
        }
    }

    public ActionItem GetUnoccupiedActionItem() {
        for (int i = 0; i < items.Length; i++) {
            ActionItem currItem = items[i];
            if (currItem.obj == null) {
                return currItem;
            }
        }
        return null;
    }
}
