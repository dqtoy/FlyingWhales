using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIMenu : MonoBehaviour {

    [Header("Header")]
    [SerializeField] protected Image innerHeader;
    [SerializeField] protected Image outerHeader;
    [SerializeField] protected Button closeBtn;

    [Header("Backgound")]
    [SerializeField] protected Image bgImage;
    [SerializeField] protected Image outlineImage;
    internal bool isShowing;

    protected object _data;

    #region virtuals
    internal virtual void Initialize() {
        //if(closeBtn != null) {
        //    EventDelegate.Add(closeBtn.onClick, CloseMenu);
        //}
        //if(goBackBtn != null) {
        //    EventDelegate.Add(goBackBtn.onClick, GoBack);
        //}
    }
    /*
     Used when a menu is currently being shown, but only the data has changed
         */
    public virtual void ShowMenu() {
        isShowing = true;
        //if(goBackBtn != null) {
        //    goBackBtn.gameObject.SetActive(CanGoBack());
        //}
        this.gameObject.SetActive(true);
    }
    public virtual void HideMenu() {
        isShowing = false;
        this.gameObject.SetActive(false);
    }
    /*
     When a menu is opened from being closed
         */
    public virtual void OpenMenu() {
        //UIManager.Instance.AddMenuToQueue(this, _data);
        ShowMenu();
    }
    public virtual void CloseMenu() {
        //UIManager.Instance.ClearMenuHistory();
        HideMenu();
        UIManager.Instance.ShowMainUI();
    }
    public virtual void GoBack() {
        HideMenu();
        //UIManager.Instance.ShowPreviousMenu();
    }
    public virtual void SetData(object data) {
        _data = data;
    }
    public virtual void ShowTooltip(GameObject objectHovered) {

    }
    #endregion

    public void ApplyUnifiedSettings(UnifiedUISettings settings) {
        if (bgImage != null && outlineImage != null) {
            float bgWidth = bgImage.rectTransform.sizeDelta.x;
            float bgHeight = bgImage.rectTransform.sizeDelta.y;

            float outlineWidth = bgWidth + settings.outlineThickness;
            float outlineHeight = bgHeight + settings.outlineThickness;

            outlineImage.rectTransform.sizeDelta = new Vector2(outlineWidth, outlineHeight);
            outlineImage.rectTransform.localPosition = bgImage.rectTransform.localPosition;

            bgImage.color = settings.bgColor;
            outlineImage.color = settings.outlineColor;

            if (innerHeader != null && outerHeader != null) {
                innerHeader.rectTransform.sizeDelta = new Vector2(bgWidth, settings.headerHeight);
                outerHeader.rectTransform.sizeDelta = new Vector2(outlineWidth, settings.headerHeight + settings.outlineThickness);
                innerHeader.rectTransform.localPosition = new Vector3(bgImage.rectTransform.localPosition.x, innerHeader.rectTransform.localPosition.y, 0f);
                outerHeader.rectTransform.localPosition = innerHeader.rectTransform.localPosition;

                innerHeader.color = settings.innerHeaderColor;
                outerHeader.color = settings.outerHeaderColor;

                if (closeBtn != null) {
                    (closeBtn.transform as RectTransform).sizeDelta = new Vector2(settings.closeBtnSize, settings.closeBtnSize);
                    closeBtn.transform.localPosition = new Vector3(closeBtn.transform.localPosition.x, innerHeader.transform.localPosition.y, 0f);
                }
            }
        }
        ScrollRect[] scrollViews = this.GetComponentsInChildren<ScrollRect>(true);
        if (scrollViews != null) {
            for (int i = 0; i < scrollViews.Length; i++) {
                ScrollRect sr = scrollViews[i];
                sr.movementType = settings.scrollMovementType;
                sr.scrollSensitivity = settings.scrollSensitivity;
                sr.horizontalScrollbarVisibility = settings.scrollbarVisibility;
                sr.verticalScrollbarVisibility = settings.scrollbarVisibility;
            }
        }
    }

    private bool CanGoBack() {
        if(UIManager.Instance.menuHistory.Count > 1) {
            return true;
        }
        return false;
    }
}
