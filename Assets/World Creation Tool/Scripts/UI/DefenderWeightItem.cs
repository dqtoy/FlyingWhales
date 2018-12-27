using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefenderWeightItem : MonoBehaviour {

    private Faction owner;
    private AreaCharacterClass defender;
    private int weight;

    [SerializeField] private Dropdown classDropdown;
    [SerializeField] private InputField weightField;
    //[SerializeField] private Toggle includeInFirstToggle;

    public void SetDefender(Faction owner, AreaCharacterClass defender, int weight) {
        classDropdown.ClearOptions();
        classDropdown.AddOptions(Utilities.GetFileChoices(Utilities.dataPath + "CharacterClasses/", "*.json"));

        classDropdown.value = Utilities.GetOptionIndex(classDropdown, defender.className);
        //includeInFirstToggle.isOn = defender.includeInFirstWeight;
        weightField.text = weight.ToString();

        this.owner = owner;
        this.defender = defender;
        this.weight = weight;
    }

    public void OnChangeDefenderClass(int choice) {
        if (this.owner == null) {
            return;
        }
        string chosenClass = classDropdown.options[choice].text;
        defender.className = chosenClass;
    }
    //public void OnIncludeInFirstToggleValueChange(bool value) {
    //    if (this.owner == null) {
    //        return;
    //    }
    //    defender.includeInFirstWeight = value;
    //}
    public void OnChangeWeight(string valueStr) {
        if (this.owner == null) {
            return;
        }
        int value = System.Int32.Parse(valueStr);
        //owner.defenderWeights.ChangeElementWeight(defender, value);
    }

    public void DeleteItem() {
        //owner.defenderWeights.RemoveElement(defender);
        GameObject.Destroy(this.gameObject);
    }
}
