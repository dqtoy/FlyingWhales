using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Actionables;

public class UIMenu : MonoBehaviour {
    public Button backButton;
    public bool isShowing;
    private Action openMenuAction;

    protected object _data;

    [Header("Actions")]
    [SerializeField] protected RectTransform actionsTransform;
    [SerializeField] protected GameObject actionItemPrefab;

    #region virtuals
    internal virtual void Initialize() {
        Messenger.AddListener<UIMenu>(Signals.BEFORE_MENU_OPENED, BeforeMenuOpens);
    }
    public virtual void OpenMenu() {
        Messenger.Broadcast(Signals.BEFORE_MENU_OPENED, this);
        isShowing = true;
        this.gameObject.SetActive(true);
        if (openMenuAction != null) {
            openMenuAction();
            openMenuAction = null;
        }
        Messenger.Broadcast(Signals.MENU_OPENED, this);
        UIManager.Instance.AddToUIMenuHistory(_data);
        if (backButton != null) {
            backButton.interactable = UIManager.Instance.GetLastUIMenuHistory() != null;    
        }
        UIManager.Instance.poiTestingUI.HideUI();
        if (_data is Actionables.IPlayerActionTarget) {
            LoadActions(_data as IPlayerActionTarget);    
        }
    }
    public virtual void CloseMenu() {
        isShowing = false;
        this.gameObject.SetActive(false);
        Messenger.Broadcast(Signals.MENU_CLOSED, this);
    }
    public virtual void GoBack() {
        object data = UIManager.Instance.GetLastUIMenuHistory();
        if(data != null) {
            CloseMenu();
            UIManager.Instance.RemoveLastUIMenuHistory();
            GoBackToPreviousUIMenu(data);
        }
    }
    public virtual void SetData(object data) {
        _data = data;
    }
    public virtual void ShowTooltip(GameObject objectHovered) {

    }
    public void OnClickCloseMenu() {
        CloseMenu();
        UIManager.Instance.ClearUIMenuHistory();
    }
    #endregion

    private void GoBackToPreviousUIMenu(object data) {
        if(data != null) {
            if(data is Character) {
                UIManager.Instance.ShowCharacterInfo(data as Character);
            } else if (data is Settlement) {
                UIManager.Instance.ShowRegionInfo((data as Settlement).region);
            } else if(data is Faction) {
                UIManager.Instance.ShowFactionInfo(data as Faction);
            } else if (data is TileObject) {
                UIManager.Instance.ShowTileObjectInfo(data as TileObject);
            } else if (data is Region) {
                UIManager.Instance.ShowRegionInfo(data as Region);
            } else if (data is HexTile) {
                UIManager.Instance.ShowRegionInfo((data as HexTile).region);
            } else if (data is SpecialToken) {
                UIManager.Instance.ShowItemInfo(data as SpecialToken);
            }
        }
    }

    private void BeforeMenuOpens(UIMenu menuToOpen) {
        if (this.isShowing && menuToOpen != this) {
            CloseMenu();
        }
    }

    #region Actions
    protected virtual void LoadActions(IPlayerActionTarget target) {
        Utilities.DestroyChildren(actionsTransform);

        for (int i = 0; i < target.actions.Count; i++) {
            PlayerAction action = target.actions[i];
            ActionItem actionItem = AddNewAction(action.actionName, null, action.action);
            actionItem.SetInteractable(action.isActionValidChecker.Invoke());
        }
        
    }
    protected ActionItem AddNewAction(string actionName, Sprite actionIcon, System.Action action) {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(actionItemPrefab.name, Vector3.zero,
            Quaternion.identity, actionsTransform);
        ActionItem item = obj.GetComponent<ActionItem>();
        item.SetAction(action, actionIcon, actionName);
        return item;
    }
    #endregion
}
