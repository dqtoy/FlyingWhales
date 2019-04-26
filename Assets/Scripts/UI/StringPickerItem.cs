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
            iconImg.sprite = AttributeManager.Instance.GetTraitIcon(str);
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
}
