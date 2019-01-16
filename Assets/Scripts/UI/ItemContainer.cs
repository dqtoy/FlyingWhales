using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemContainer : PooledObject {

    private SpecialToken item;

    [SerializeField] private Image itemIcon;

    public void SetItem(SpecialToken item) {
        this.item = item;
        itemIcon.gameObject.SetActive(item != null);
        //UpdateVisual(hasBeenInspected);
    }

    public void ShowItemInfo() {
        UIManager.Instance.ShowSmallInfo(item.name);
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    //private void UpdateVisual(bool hasBeenInspected = true) {
    //    if (item == null) {
    //        itemIcon.sprite = null;
    //        itemIcon.gameObject.SetActive(false);
    //    } else {
    //        if (!hasBeenInspected) {
    //            itemIcon.sprite = ItemManager.Instance.notInspectedSprite;
    //        } else {
    //            itemIcon.sprite = ItemManager.Instance.GetIconSprite(item.iconName);
    //        }
    //        itemIcon.gameObject.SetActive(true);
    //    }
    //}

    //public void OnPointerEnter(PointerEventData eventData) {
    //    if (item != null) {
    //        isHovering = true;
    //    }
    //}

    //public void OnPointerExit(PointerEventData eventData) {
    //    isHovering = false;
    //    UIManager.Instance.HideSmallInfo();
    //}

    public override void Reset() {
        base.Reset();
        item = null;
    }

    //private void Update() {
    //    if (isHovering && itemIcon.sprite.name != ItemManager.Instance.notInspectedSprite.name) {
    //        UIManager.Instance.ShowSmallInfo(item.itemName);
    //    }
    //}
}
