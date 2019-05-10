﻿using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntelNotificationItem : PlayerNotificationItem {

    public Intel intel { get; private set; }

    [SerializeField] private Button getIntelBtn;
    [SerializeField] private GameObject convertTooltip;

    public void Initialize(Intel intel, bool hasExpiry = true) {
        this.intel = intel;
        base.Initialize(intel.intelLog, hasExpiry);
    }
   
    public void GetIntel() {
        PlayerManager.Instance.player.AddIntel(intel);
        PlayerManager.Instance.player.ShowNotification(new Log(GameManager.Instance.Today(), "Character", "Generic", "intel_stored"));
        DeleteNotification();
    }

    protected override void OnExpire() {
        base.OnExpire();
        intel.OnIntelExpire();
    }
    public override void Reset() {
        base.Reset();
        convertTooltip.SetActive(false);
    }
}
