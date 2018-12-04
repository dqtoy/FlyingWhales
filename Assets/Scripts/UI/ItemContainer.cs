using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemContainer : PooledObject, IPointerEnterHandler, IPointerExitHandler {

    private Item item;
    private bool isHovering = false;

    [SerializeField] private Image itemIcon;

    public void SetItem(Item item, bool hasBeenInspected = true) {
        this.item = item;
        UpdateVisual(hasBeenInspected);
    }
    private void UpdateVisual(bool hasBeenInspected = true) {
        if (item == null) {
            itemIcon.sprite = null;
            itemIcon.gameObject.SetActive(false);
        } else {
            if (!hasBeenInspected) {
                itemIcon.sprite = ItemManager.Instance.notInspectedSprite;
            } else {
                itemIcon.sprite = ItemManager.Instance.GetIconSprite(item.iconName);
            }
            itemIcon.gameObject.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (item != null) {
            isHovering = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        UIManager.Instance.HideSmallInfo();
    }

    public override void Reset() {
        base.Reset();
        item = null;
        isHovering = false;
    }

    private void Update() {
        if (isHovering && itemIcon.sprite.name != ItemManager.Instance.notInspectedSprite.name) {
            UIManager.Instance.ShowSmallInfo(item.itemName);
        }
    }
}
