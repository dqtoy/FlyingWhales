using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StringPickerItem : ObjectPickerItem<Area>, IPointerClickHandler {

    public Action<string> onClickAction;

    private string str;

    public GameObject portraitCover;

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
