using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructurePrioritySettingItem : MonoBehaviour {

    private StructurePrioritySetting _setting;
    private StructurePriorityItem _owner;

    [SerializeField] private Text landmarkTypeLbl;
    [SerializeField] private Text costSummaryLbl;

    public void SetSetting(StructurePrioritySetting setting, StructurePriorityItem owner) {
        _setting = setting;
        _owner = owner;
        UpdateInfo();
    }

    public void UpdateInfo() {
        landmarkTypeLbl.text = _setting.landmarkType.ToString();
        string costSummary = "B: ";
        for (int i = 0; i < _setting.buildResourceCost.Count; i++) {
            Resource cost = _setting.buildResourceCost[i];
            costSummary += cost.resource.ToString() + " " + cost.amount.ToString() + ", ";
        }
        costSummary += " R: ";
        for (int i = 0; i < _setting.repairResourceCost.Count; i++) {
            Resource cost = _setting.repairResourceCost[i];
            costSummary += cost.resource.ToString() + " " + cost.amount.ToString() + ", ";
        }
        costSummaryLbl.text = costSummary;
    }
    public void EditSettings() {
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.ShowSettingsEditor(_setting, _owner.item);
    }
}
