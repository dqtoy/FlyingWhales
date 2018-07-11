using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfoEditor : MonoBehaviour {

    internal Area currentArea;

    [Header("Settlement Priorities")]
    public GameObject structurePriorityItemGO;
    public GameObject structurePrioritySettingItemGO;
    [SerializeField] private ScrollRect structurePriorityScrollView;
    [SerializeField] private StructurePrioritySettingsEditor settingsEditor;

    public void Initialize() {
        settingsEditor.Initialize();
    }

    public void Show(Area area) {
        currentArea = area;
        this.gameObject.SetActive(true);
        LoadStructurePriorities();
    }
    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void LoadStructurePriorities() {
        Utilities.DestroyChildren(structurePriorityScrollView.content);
        for (int i = 0; i < currentArea.orderStructures.Count; i++) {
            StructurePriority currPriority = currentArea.orderStructures[i];
            GameObject structureItemGO = GameObject.Instantiate(structurePriorityItemGO, structurePriorityScrollView.content);
            StructurePriorityItem item = structureItemGO.GetComponent<StructurePriorityItem>();
            item.SetItem(currPriority, i);
        }
    }

    public void AddStructurePriority() {
        StructurePriority newPrio = new StructurePriority();
        StructurePrioritySetting newSetting = new StructurePrioritySetting();
        newPrio.AddSettings(newSetting);
        currentArea.AddPriority(newPrio);
        LoadStructurePriorities();
    }

    public void ShowSettingsEditor(StructurePrioritySetting settings) {
        settingsEditor.ShowSettings(settings);
    }
}
