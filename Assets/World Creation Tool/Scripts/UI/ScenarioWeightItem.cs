using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioWeightItem : MonoBehaviour {

    private BaseLandmark owner;
    //private INTERACTION_TYPE interactionType;
    //private int weight;

    [SerializeField] private Dropdown interactionTypeDropdown;
    [SerializeField] private InputField weightField;

    public void SetData(BaseLandmark owner, INTERACTION_TYPE interactionType, int weight) {
        interactionTypeDropdown.ClearOptions();
        interactionTypeDropdown.AddOptions(Utilities.GetEnumChoices<INTERACTION_TYPE>());
        SetInteractionType(interactionType);
        SetWeight(weight);

        this.owner = owner;
        
    }

    public void OnChangeInteractionType(int choice) {
        if (this.owner == null) {
            return;
        }
        INTERACTION_TYPE newType = (INTERACTION_TYPE)System.Enum.Parse(typeof(INTERACTION_TYPE), interactionTypeDropdown.options[choice].text);
        if (IsValid(newType)) {
            //owner.scenarios.ReplaceElement(this.interactionType, newType);
            SetInteractionType(newType);
        }
    }
    public void SetWeightFromField(string weightTxt) {
        if (this.owner == null) {
            return;
        }
        int weight = System.Int32.Parse(weightTxt);
        SetWeight(weight);
    }
    public void DeleteItem() {
        //owner.scenarios.RemoveElement(interactionType);
        GameObject.Destroy(this.gameObject);
    }

    private void SetWeight(int weight) {
        weight = Mathf.Max(0, weight);
        //this.weight = weight;
        weightField.text = weight.ToString();
        if (this.owner != null) {
            //owner.scenarios.ChangeElementWeight(interactionType, weight);
        }
    }

    private void SetInteractionType(INTERACTION_TYPE newType) {
        //this.interactionType = newType;
        interactionTypeDropdown.value = Utilities.GetOptionIndex(interactionTypeDropdown, newType.ToString());
    }
    private bool IsValid(INTERACTION_TYPE newType) {
        //return !owner.scenarios.dictionary.ContainsKey(newType); //new type is valid if scenarios dictionary does not yet have that interaction type
        return false;
    }
}
