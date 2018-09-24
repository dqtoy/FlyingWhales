using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemContainer : PooledObject, IPointerEnterHandler, IPointerExitHandler {

    private ECS.Item item;
    private bool isHovering = false;

    [SerializeField] private Image itemIcon;

    public void SetItem(ECS.Item item) {
        this.item = item;
        UpdateVisual();
    }
    private void UpdateVisual() {
        if (item == null) {
            itemIcon.sprite = null;
            itemIcon.gameObject.SetActive(false);
        } else {
            itemIcon.sprite = ItemManager.Instance.GetIconSprite(item.iconName);
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
        if (isHovering) {
            UIManager.Instance.ShowSmallInfo(item.itemName);
        }
    }
}
