using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StringPickerItem : ObjectPickerItem<string>, IPointerClickHandler {

    public Action<string> onClickAction;

    private string str;

    public GameObject portraitCover;
    public Image iconImg;

    public bool isTrait;

    public void SetString(string str) {
        this.str = str;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        mainLbl.text = str;
        if (isTrait) {
            if (AttributeManager.Instance.HasTraitIcon(str)) {
                iconImg.sprite = AttributeManager.Instance.GetTraitIcon(str);
                iconImg.gameObject.SetActive(true);
            } else {
                iconImg.gameObject.SetActive(false);
            }
            
        }
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(str);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnClick();
    }
    public override void OnHoverEnter() {
        if (onHoverEnterAction != null) {
            onHoverEnterAction.Invoke(str);
        }
    }
    public override void OnHoverExit() {
        //Only set as hide small info for now, but should use on hover exit action
        UIManager.Instance.HideSmallInfo();
    }
}
