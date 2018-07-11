using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructurePrioritySettingItem : MonoBehaviour {

    private StructurePrioritySetting _setting;
    private StructurePriorityItem _owner;

    [SerializeField] private Text actionTypeLbl;
    [SerializeField] private Text landmarkTypeLbl;
    //[SerializeField] private Text costSummaryLbl;

    public void SetSetting(StructurePrioritySetting setting, StructurePriorityItem owner) {
        _setting = setting;
        _owner = owner;
        UpdateInfo();
    }

    public void UpdateInfo() {
        actionTypeLbl.text = _setting.actionType.ToString();
        landmarkTypeLbl.text = _setting.landmarkType.ToString();
        //string costSummary = string.Empty;
        //for (int i = 0; i < _setting.resourceCost.Count; i++) {
        //    Resource cost = _setting.resourceCost[i];
        //    costSummary += cost.resource.ToString() + " " + cost.amount.ToString() + ", ";
        //}
        //costSummaryLbl.text = costSummary;
    }

    public void DeleteItem() {
        _owner.item.RemoveSettings(_setting);
        _owner.OnRemoveSetting();
    }
    public void EditSettings() {
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.ShowSettingsEditor(_setting);
    }
}
