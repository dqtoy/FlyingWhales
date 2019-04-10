using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PlayerNotificationItem {

    public Intel intel { get; private set; }

    [SerializeField] private Button getIntelBtn;

    public void Initialize(Intel intel, bool hasExpiry = true) {
        this.intel = intel;
        base.Initialize(intel.intelLog, hasExpiry);
    }
   
    public void GetIntel() {
        PlayerManager.Instance.player.AddIntel(intel);
        DeleteNotification();
    }

}
