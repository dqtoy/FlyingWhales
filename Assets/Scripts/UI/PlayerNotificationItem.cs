using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerNotificationItem : PooledObject, IPointerClickHandler {

    [SerializeField] private TextMeshProUGUI notificationLbl;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    private UnityAction onClickAction;

    private GameDate expirationDate;

    public void SetNotification(string text, int expirationTicks, UnityAction onClickAction = null) {
        notificationLbl.text = text;
        this.onClickAction = onClickAction;
        envelopContent.Execute();
        if (expirationTicks != -1) {
            expirationDate = GameManager.Instance.Today();
            expirationDate.AddDays(expirationTicks);
            SchedulingManager.Instance.AddEntry(expirationDate, () => DestroyObject());
        }
    }

    #region Click Actions
    public void OnPointerClick(PointerEventData eventData) {
        if (onClickAction != null) {
            onClickAction();
            DestroyObject();
        }
    }
    #endregion

    private void DestroyObject() {
        SchedulingManager.Instance.RemoveSpecificEntry(expirationDate, DestroyObject);
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }

    public override void Reset() {
        base.Reset();
        onClickAction = null;
    }

}
