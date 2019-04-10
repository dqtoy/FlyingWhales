using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNotificationItem : PooledObject {

    private const int Expiration_Ticks = 10;
    private int ticksAlive = 0;

    [SerializeField] private EnvelopContentUnityUI mainEnvelopContent;
    [SerializeField] private EnvelopContentUnityUI logEnvelopContent;
    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private LogItem logItem;

    public void Initialize(Log log, bool hasExpiry = true) {
        logLbl.SetText(Utilities.LogReplacer(log));
        logItem.SetLog(log);
        logEnvelopContent.Execute();
        mainEnvelopContent.Execute();

        if (hasExpiry) {
            //schedule expiry
            Messenger.AddListener(Signals.TICK_ENDED, CheckForExpiry);
        }
    }
    private void CheckForExpiry() {
        if (ticksAlive == Expiration_Ticks) {
            DeleteNotification();
        } else {
            ticksAlive++;
        }
    }
    private void OnExpire() {
        DeleteNotification();
        //getIntelBtn.interactable = false;
    }
    public override void Reset() {
        base.Reset();
        //getIntelBtn.interactable = true;
        ticksAlive = 0;
        this.transform.localScale = Vector3.one;
    }
    public void DeleteNotification() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
            Messenger.RemoveListener(Signals.TICK_ENDED, CheckForExpiry);
        }
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
}
