using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UIMenu : MonoBehaviour {

    public bool isShowing;
    private Action openMenuAction;

    protected object _data;

    #region virtuals
    internal virtual void Initialize() { }
    /*
     When a menu is opened from being closed
         */
    public virtual void OpenMenu() {
        isShowing = true;
        this.gameObject.SetActive(true);
        if (openMenuAction != null) {
            openMenuAction();
            openMenuAction = null;
        }
        Messenger.Broadcast(Signals.MENU_OPENED, this);
    }
    public virtual void CloseMenu() {
        isShowing = false;
        this.gameObject.SetActive(false);
        Messenger.Broadcast(Signals.MENU_CLOSED, this);
    }
    public virtual void GoBack() {
        CloseMenu();
    }
    public virtual void SetData(object data) {
        _data = data;
    }
    public virtual void ShowTooltip(GameObject objectHovered) {

    }
    public void ToggleMenu(bool state) {
        if (state) {
            OpenMenu();
        } else {
            CloseMenu();
        }
    }
    #endregion

    public void SetOpenMenuAction(Action action) {
        openMenuAction = action;
    }

    //public void ApplyUnifiedSettings(UnifiedUISettings settings) {
    //    if (bgImage != null && outlineImage != null) {
    //        float bgWidth = bgImage.rectTransform.sizeDelta.x;
    //        float bgHeight = bgImage.rectTransform.sizeDelta.y;

    //        float outlineWidth = bgWidth + settings.outlineThickness;
    //        float outlineHeight = bgHeight + settings.outlineThickness;

    //        outlineImage.rectTransform.sizeDelta = new Vector2(outlineWidth, outlineHeight);
    //        outlineImage.rectTransform.localPosition = bgImage.rectTransform.localPosition;

    //        bgImage.color = settings.bgColor;
    //        outlineImage.color = settings.outlineColor;

    //        if (innerHeader != null && outerHeader != null) {
    //            innerHeader.rectTransform.sizeDelta = new Vector2(bgWidth, settings.headerHeight);
    //            outerHeader.rectTransform.sizeDelta = new Vector2(outlineWidth, settings.headerHeight + settings.outlineThickness);
    //            innerHeader.rectTransform.localPosition = new Vector3(bgImage.rectTransform.localPosition.x, innerHeader.rectTransform.localPosition.y, 0f);
    //            outerHeader.rectTransform.localPosition = innerHeader.rectTransform.localPosition;

    //            innerHeader.color = settings.innerHeaderColor;
    //            outerHeader.color = settings.outerHeaderColor;

    //            if (closeBtn != null) {
    //                (closeBtn.transform as RectTransform).sizeDelta = new Vector2(settings.closeBtnSize, settings.closeBtnSize);
    //                closeBtn.transform.localPosition = new Vector3(closeBtn.transform.localPosition.x, innerHeader.transform.localPosition.y, 0f);
    //            }
    //        }
    //    }
    //    ScrollRect[] scrollViews = this.GetComponentsInChildren<ScrollRect>(true);
    //    if (scrollViews != null) {
    //        for (int i = 0; i < scrollViews.Length; i++) {
    //            ScrollRect sr = scrollViews[i];
    //            sr.movementType = settings.scrollMovementType;
    //            sr.scrollSensitivity = settings.scrollSensitivity;
    //            sr.horizontalScrollbarVisibility = settings.scrollbarVisibility;
    //            sr.verticalScrollbarVisibility = settings.scrollbarVisibility;
    //        }
    //    }
    //}
}
