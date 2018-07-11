using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructurePriorityItem : MonoBehaviour {

    internal StructurePriority item;

    private int index;

    [SerializeField] private Text indexLbl;
    [SerializeField] private RectTransform settingsParent;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    public void SetItem(StructurePriority item, int index) {
        this.index = index;
        this.item = item;
        indexLbl.text = index.ToString();
        LoadSettings();
    }

    private void LoadSettings() {
        Utilities.DestroyChildren(settingsParent);
        for (int i = 0; i < item.settings.Count; i++) {
            StructurePrioritySetting setting = item.settings[i];
            GameObject settingsItemGO = GameObject.Instantiate(worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.structurePrioritySettingItemGO, settingsParent);
            settingsItemGO.GetComponent<StructurePrioritySettingItem>().SetSetting(setting, this);
        }
        envelopContent.Execute();
    }

    public void AddSetting() {
        StructurePrioritySetting newSetting = new StructurePrioritySetting();
        item.AddSettings(newSetting);
        LoadSettings();
    }
    public void OnRemoveSetting() {
        LoadSettings();
    }

    public void MoveUp() {
        int upIndex = index - 1;
        if (upIndex < 0) {
            return;
        }
        Area area = worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.currentArea;
        StructurePriority priorityToReplace = area.orderStructures[upIndex];
        area.orderStructures[upIndex] = item;
        area.orderStructures[index] = priorityToReplace;
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.LoadStructurePriorities();
    }
    public void MoveDown() {
        Area area = worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.currentArea;
        int downIndex = index + 1;
        if (downIndex > area.orderStructures.Count - 1) {
            return;
        }
        StructurePriority priorityToReplace = area.orderStructures[downIndex];
        area.orderStructures[downIndex] = item;
        area.orderStructures[index] = priorityToReplace;
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.LoadStructurePriorities();
    }
    public void Remove() {
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.currentArea.RemoveStructurePriority(item);
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.LoadStructurePriorities();
    }
}
