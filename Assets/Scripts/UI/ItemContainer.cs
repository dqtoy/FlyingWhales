using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemContainer : PooledObject, IPointerEnterHandler, IPointerExitHandler {

    private ECS.Item item;

    [SerializeField] private Image itemIcon;

    public void SetItem(ECS.Item item) {
        this.item = item;
        UpdateVisual();
    }
    private void UpdateVisual() {
        if (item == null) {
            itemIcon.sprite = null;
        } else {
            itemIcon.sprite = ItemManager.Instance.GetItemSprite(item.itemName);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (item != null) {
            UIManager.Instance.ShowSmallInfo(item.itemName);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.Instance.HideSmallInfo();
    }

    public override void Reset() {
        base.Reset();
        item = null;
    }
}
