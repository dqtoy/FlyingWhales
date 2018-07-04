using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerNotificationArea : UIMenu {

    [SerializeField] private GameObject notificationItemPrefab;
    [SerializeField] private ScrollRect notificationsScrollView;

    public void ShowNotification(string text, int expirationTicks, UnityAction onClickAction = null) {
        GameObject notificationGO = UIManager.Instance.InstantiateUIObject(notificationItemPrefab.name, notificationsScrollView.content);
        PlayerNotificationItem notificationItem = notificationGO.GetComponent<PlayerNotificationItem>();
        notificationGO.SetActive(true);
        notificationItem.SetNotification(text, expirationTicks, onClickAction);
    }
}
