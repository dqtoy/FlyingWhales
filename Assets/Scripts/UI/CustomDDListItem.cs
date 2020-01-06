using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class CustomDDListItem : PooledObject {
    public TextMeshProUGUI itemText;

    private Action<CustomDDListItem> onClick;

    public void SetText(string text) {
        itemText.text = text;
    }
    public void SetClickAction(Action<CustomDDListItem> action) {
        onClick = action;
    }

    public void OnClickThis() {
        if (onClick != null) {
            onClick(this);
        }
    }

    #region Object Pooling
    public override void Reset() {
        base.Reset();
        onClick = null;
        itemText.text = string.Empty;
    }
    #endregion
}
