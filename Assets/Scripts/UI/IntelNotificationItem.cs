using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PooledObject {

    private const int Expiration_Ticks = 10;

    public Intel intel { get; private set; }

    [SerializeField] private EnvelopContentUnityUI mainEnvelopContent;
    [SerializeField] private EnvelopContentUnityUI logEnvelopContent;
    [SerializeField] private TextMeshProUGUI intelLbl;
    [SerializeField] private Button getIntelBtn;
    [SerializeField] private LogItem intelLogItem;

    public void Initialize(Intel intel) {
        this.intel = intel;
        intelLbl.SetText(Utilities.LogReplacer(intel.intelLog));
        intelLogItem.SetLog(intel.intelLog);
        logEnvelopContent.Execute();
        mainEnvelopContent.Execute();

        //schedule expiry
        SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(Expiration_Ticks), () => OnExpire());
    }

    public void DeleteNotification() {
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
        this.transform.localScale = Vector3.one;
    }
}
