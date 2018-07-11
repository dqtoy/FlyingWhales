using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfoEditor : MonoBehaviour {

    internal Area currentArea;

    [Header("Settlement Priorities")]
    public GameObject structurePriorityItemGO;
    public GameObject structurePrioritySettingItemGO;
    [SerializeField] private ScrollRect structurePriorityScrollView;
    [SerializeField] private StructurePrioritySettingsEditor settingsEditor;

    [Header("Class Priorities")]
    public GameObject classPriorityItemGO;
    [SerializeField] private ScrollRect classPriorityScrollView;
    [SerializeField] private Dropdown classChoicesDropdown;

    public void Initialize() {
        settingsEditor.Initialize();
        LoadClassChoices();
    }

    public void Show(Area area) {
        currentArea = area;
        this.gameObject.SetActive(true);
        LoadStructurePriorities();
        LoadClassPriorities();
    }
    public void Hide() {
        this.gameObject.SetActive(false);
    }

    #region Structure Priorities
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
        currentArea.AddStructurePriority(newPrio);
        LoadStructurePriorities();
    }
    public void ShowSettingsEditor(StructurePrioritySetting settings) {
        settingsEditor.ShowSettings(settings);
    }
    #endregion

    #region Class Priorities
    public void LoadClassChoices() {
        classChoicesDropdown.ClearOptions();
        List<string> choices = new List<string>();
        for (int i = 0; i < CharacterManager.Instance.classesDictionary.Keys.Count; i++) {
            choices.Add(CharacterManager.Instance.classesDictionary.Keys.ElementAt(i));
        }
        classChoicesDropdown.AddOptions(choices);
    }
    public void LoadClassPriorities() {
        Utilities.DestroyChildren(classPriorityScrollView.content);
        for (int i = 0; i < currentArea.orderClasses.Count; i++) {
            string currPriority = currentArea.orderClasses[i];
            GameObject classItemGO = GameObject.Instantiate(classPriorityItemGO, classPriorityScrollView.content);
            ClassPriorityItem item = classItemGO.GetComponent<ClassPriorityItem>();
            item.SetItem(currPriority, i);
        }
    }
    public void AddClassPriority() {
        currentArea.AddClassPriority(classChoicesDropdown.options[classChoicesDropdown.value].text);
        LoadClassPriorities();
    }
    #endregion
}
