using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PooledObject {

    private const int Expiration_Ticks = 10;

    public Intel intel { get; private set; }

    [SerializeField] private EnvelopContentUnityUI envelopContent;
    [SerializeField] private TextMeshProUGUI intelLbl;
    [SerializeField] private Button getIntelBtn;

    public void Initialize(Intel intel) {
        this.intel = intel;
        intelLbl.SetText(Utilities.LogReplacer(intel.intelLog));
        envelopContent.Execute();

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
        //TODO: Make this disabled instead
        //DeleteNotification();
        getIntelBtn.interactable = false;
    }

    public override void Reset() {
        base.Reset();
        getIntelBtn.interactable = true;
    }
}
