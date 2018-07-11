using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettlementPrioritySettingItem : MonoBehaviour {

    private StructurePrioritySetting _setting;

    [SerializeField] private Text actionTypeLbl;
    [SerializeField] private Text landmarkTypeLbl;
    [SerializeField] private Text costSummaryLbl;

    public void SetSetting(StructurePrioritySetting setting) {
        _setting = setting;
        UpdateInfo();
    }

    public void UpdateInfo() {
        actionTypeLbl.text = _setting.actionType.ToString();
        landmarkTypeLbl.text = _setting.landmarkType.ToString();
        string costSummary = string.Empty;
        for (int i = 0; i < _setting.resourceCost.Count; i++) {
            Resource cost = _setting.resourceCost[i];
            costSummary += cost.resource.ToString() + " " + cost.amount.ToString() + ", ";
        }
        costSummaryLbl.text = costSummary;
    }
}
