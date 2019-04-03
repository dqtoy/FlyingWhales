using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureItem : MonoBehaviour {

    public LocationStructure structure { get; private set; }

    [SerializeField] private Text structureTypeLbl;
    [SerializeField] private Toggle isInsideToggle;

    public void SetStructure(LocationStructure structure) {
        this.structure = structure;
        structureTypeLbl.text = structure.structureType.ToString();
        isInsideToggle.isOn = structure.isInside;
    }

    public void SetIsInsideState(bool isOn) {
        structure.SetInsideState(isOn);
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.UpdateStructureSummary();
    }

    public void DeleteStructure() {
        structure.DestroyStructure();
        worldcreator.WorldCreatorUI.Instance.editAreasMenu.infoEditor.UpdateStructureSummary();
        GameObject.Destroy(this.gameObject);
    }
}
