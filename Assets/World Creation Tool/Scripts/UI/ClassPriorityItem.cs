using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassPriorityItem : MonoBehaviour {

    private int index;

    [SerializeField] private Text classNameLbl;

    public void SetItem(string className, int index) {
        classNameLbl.text = className;
        this.index = index;
    }
    public void MoveUp() {
        int upIndex = index - 1;
        if (upIndex < 0) {
            return;
        }
        Area area = worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.currentArea;
        string priorityToReplace = area.orderClasses[upIndex];
        area.orderClasses[upIndex] = classNameLbl.text;
        area.orderClasses[index] = priorityToReplace;
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.LoadClassPriorities();
    }
    public void MoveDown() {
        Area area = worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.currentArea;
        int downIndex = index + 1;
        if (downIndex > area.orderClasses.Count - 1) {
            return;
        }
        string priorityToReplace = area.orderClasses[downIndex];
        area.orderClasses[downIndex] = classNameLbl.text;
        area.orderClasses[index] = priorityToReplace;
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.LoadClassPriorities();
    }
    public void Remove() {
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.currentArea.RemoveClassPriority(index);
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.LoadClassPriorities();
    }
}
