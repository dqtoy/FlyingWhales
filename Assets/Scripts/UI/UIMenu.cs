using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Actionables;

public class UIMenu : MonoBehaviour {
    public Button backButton;
    public bool isShowing;
    private Action openMenuAction;

    protected object _data;

    private IPlayerActionTarget _playerActionTarget;
    
    [Header("Actions")]
    [SerializeField] protected RectTransform actionsTransform;
    [SerializeField] protected GameObject actionItemPrefab;

    #region virtuals
    internal virtual void Initialize() {
        Messenger.AddListener<UIMenu>(Signals.BEFORE_MENU_OPENED, BeforeMenuOpens);
        Messenger.AddListener<PlayerAction>(Signals.PLAYER_ACTION_EXECUTED, OnPlayerActionExecuted);
        Messenger.AddListener<PlayerAction, IPlayerActionTarget>(Signals.PLAYER_ACTION_ADDED_TO_TARGET, OnPlayerActionAddedToTarget);
        Messenger.AddListener<PlayerAction, IPlayerActionTarget>(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, OnPlayerActionRemovedFromTarget);
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
        UIManager.Instance.minionCommandsUI.HideUI();
        _playerActionTarget = _data as IPlayerActionTarget;
        if (_playerActionTarget != null) {
            LoadActions(_playerActionTarget);    
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
            } 
            // else if (data is SpecialToken) {
            //     UIManager.Instance.ShowItemInfo(data as SpecialToken);
            // }
        }
    }

    private void BeforeMenuOpens(UIMenu menuToOpen) {
        if (this.isShowing && menuToOpen != this) {
            CloseMenu();
        }
    }

    #region Actions
    private List<ActionItem> activeActionItems = new List<ActionItem>();
    protected virtual void LoadActions(IPlayerActionTarget target) {
        UtilityScripts.Utilities.DestroyChildren(actionsTransform);
        activeActionItems.Clear();
        for (int i = 0; i < target.actions.Count; i++) {
            PlayerAction action = target.actions[i];
            if (PlayerManager.Instance.player.archetype.CanDoAction(action.actionName)) {
                ActionItem actionItem = AddNewAction(action);
                actionItem.SetInteractable(action.isActionValidChecker.Invoke() && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
            }
        }
    }
    protected ActionItem AddNewAction(PlayerAction playerAction) {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(actionItemPrefab.name, Vector3.zero,
            Quaternion.identity, actionsTransform);
        ActionItem item = obj.GetComponent<ActionItem>();
        item.SetAction(playerAction);
        activeActionItems.Add(item);
        return item;
    }
    private ActionItem GetActionItem(PlayerAction action) {
        for (int i = 0; i < activeActionItems.Count; i++) {
            ActionItem item = activeActionItems[i];
            if (item.playerAction == action) {
                return item;
            }
        }
        return null;
    }
    private void OnPlayerActionExecuted(PlayerAction action) {
        if (_playerActionTarget != null && _playerActionTarget.actions.Contains(action)) {
            LoadActions(_playerActionTarget);
        }
        // ActionItem actionItem = GetActionItem(action);
        // if (actionItem != null) {
        //     LoadActions();
        // }
    }
    private void OnPlayerActionAddedToTarget(PlayerAction playerAction, IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget) {
            LoadActions(actionTarget);
        }
    }
    private void OnPlayerActionRemovedFromTarget(PlayerAction playerAction, IPlayerActionTarget actionTarget) {
        if (_playerActionTarget == actionTarget) {
            LoadActions(actionTarget);
        }
    }
    #endregion
}
