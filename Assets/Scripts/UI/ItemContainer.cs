﻿using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemContainer : PooledObject, IPointerClickHandler {

    private SpecialToken item;

    [SerializeField] private Image itemIcon;

    public void SetItem(SpecialToken item) {
        this.item = item;
        itemIcon.gameObject.SetActive(item != null);
        if (item != null) {
            Sprite sprite = TokenManager.Instance.GetItemSprite(item.specialTokenType);
            if (sprite != null) {
                itemIcon.sprite = sprite;
            }
        }
        //UpdateVisual(hasBeenInspected);
    }

    public void ShowItemInfo() {
        string summary = item.name + " at " + item.structureLocation?.ToString() ?? "No Location";
        string ownerName = item.characterOwner?.ToString() ?? "No one";
        summary += "\nOwned by: " + ownerName;
        summary += "\nCarried by: " + (item.carriedByCharacter?.name ?? "No one");
        UIManager.Instance.ShowSmallInfo(summary);
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

    public void OnPointerClick(PointerEventData eventData) {
        if (item == null) {
            return;
        }
        //center camera on item location
        LocationGridTile tileLocation = item.gridTileLocation;
        if (tileLocation != null) {
            bool instantCenter = InnerMapManager.Instance.currentlyShowingLocation != tileLocation.structure.location;
            if (InnerMapManager.Instance.currentlyShowingMap == tileLocation.parentMap) {
                InnerMapManager.Instance.ShowInnerMap(tileLocation.structure.areaLocation);
            }
            InnerMapCameraMove.Instance.CenterCameraOn(item.collisionTrigger.gameObject, instantCenter);
        }
    }

    //private void Update() {
    //    if (isHovering && itemIcon.sprite.name != ItemManager.Instance.notInspectedSprite.name) {
    //        UIManager.Instance.ShowSmallInfo(item.itemName);
    //    }
    //}
}
