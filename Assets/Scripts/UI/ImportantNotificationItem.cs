using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImportantNotificationItem : PooledObject {

    [SerializeField] private TextMeshProUGUI messageLbl;

    private System.Action onClickAction;

    public void Initialize(string message, System.Action onClickAction) {
        messageLbl.text = message;
        this.onClickAction = onClickAction;
    }

    public void OnPointerClick(BaseEventData eventData) {
        onClickAction?.Invoke();
        //destroy on click
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
}
