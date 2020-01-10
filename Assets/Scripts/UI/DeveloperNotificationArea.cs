using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DeveloperNotificationArea : UIMenu {

    [SerializeField] private GameObject notificationItemPrefab;
    [SerializeField] private ScrollRect notificationsScrollView;

    [SerializeField] private Vector2 defaultPos;
    [SerializeField] private Vector2 otherMenuOpenedPos;

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }

    public void ShowNotification(string text, int expirationTicks, UnityAction onClickAction = null) {
        GameObject notificationGO = UIManager.Instance.InstantiateUIObject(notificationItemPrefab.name, notificationsScrollView.content);
        DeveloperNotificationItem notificationItem = notificationGO.GetComponent<DeveloperNotificationItem>();
        notificationGO.SetActive(true);
        notificationItem.SetNotification(text, expirationTicks, onClickAction);
    }

    private void OnMenuOpened(UIMenu openedMenu) {
        if (openedMenu is CharacterInfoUI || openedMenu is FactionInfoUI) {
            this.transform.localPosition = otherMenuOpenedPos;
        }
        
    }
    private void OnMenuClosed(UIMenu openedMenu) {
        if (openedMenu is CharacterInfoUI || openedMenu is FactionInfoUI) {
            this.transform.localPosition = defaultPos;
        }
    }
}
