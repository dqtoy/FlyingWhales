using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImportantNotificationItem : PooledObject {

    [SerializeField] private TextMeshProUGUI messageLbl;

    private System.Action onClickAction;

    public void Initialize(GameDate date, string message, System.Action onClickAction) {
        messageLbl.text = "[" + GameManager.ConvertTickToTime(date.tick) + "] "+ message;
        this.onClickAction = onClickAction;
    }

    public void OnPointerClick(BaseEventData eventData) {
        onClickAction?.Invoke();
        //destroy on click
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
}
