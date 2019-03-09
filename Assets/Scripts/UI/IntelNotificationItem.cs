using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PooledObject {

    private const int Expiration_Ticks = 10;
    private int ticksAlive = 0;

    public Intel intel { get; private set; }

    [SerializeField] private EnvelopContentUnityUI mainEnvelopContent;
    [SerializeField] private EnvelopContentUnityUI logEnvelopContent;
    [SerializeField] private TextMeshProUGUI intelLbl;
    [SerializeField] private Button getIntelBtn;
    [SerializeField] private LogItem intelLogItem;

    public void Initialize(Intel intel, bool hasExpiry = true) {
        this.intel = intel;
        intelLbl.SetText(Utilities.LogReplacer(intel.intelLog));
        intelLogItem.SetLog(intel.intelLog);
        logEnvelopContent.Execute();
        mainEnvelopContent.Execute();

        if (hasExpiry) {
            //schedule expiry
            //SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(Expiration_Ticks), () => OnExpire());
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

    public void DeleteNotification() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_ENDED)) {
            Messenger.RemoveListener(Signals.TICK_ENDED, CheckForExpiry);
        }
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }
    public void GetIntel() {
        PlayerManager.Instance.player.AddIntel(intel);
        DeleteNotification();
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
}
