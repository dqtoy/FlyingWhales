using UnityEngine;
using System.Collections;

public class UIMenu : MonoBehaviour {
    [SerializeField] protected UIButton goBackBtn;
    [SerializeField] protected UIButton closeBtn;
    internal bool isShowing;

    protected object _data;

    #region virtuals
    internal virtual void Initialize() {
        if(closeBtn != null) {
            EventDelegate.Add(closeBtn.onClick, CloseMenu);
        }
        if(goBackBtn != null) {
            EventDelegate.Add(goBackBtn.onClick, GoBack);
        }
    }
    public virtual void ShowMenu() {
        isShowing = true;
        goBackBtn.gameObject.SetActive(CanGoBack());
        this.gameObject.SetActive(true);
    }
    public virtual void HideMenu() {
        isShowing = false;
        this.gameObject.SetActive(false);
    }
    public virtual void OpenMenu() {
        UIManager.Instance.AddMenuToQueue(this, _data);
        ShowMenu();
    }
    public virtual void CloseMenu() {
        UIManager.Instance.ClearMenuHistory();
        HideMenu();
    }
    public virtual void GoBack() {
        HideMenu();
        UIManager.Instance.ShowPreviousMenu();
    }
    public virtual void SetData(object data) {
        _data = data;
    }
    #endregion

    private bool CanGoBack() {
        if(UIManager.Instance.menuHistory.Count > 1) {
            return true;
        }
        return false;
    }
}
