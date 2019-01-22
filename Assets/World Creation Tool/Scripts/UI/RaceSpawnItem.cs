using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceSpawnItem : MonoBehaviour {

    public InitialRaceSetup setup;

    [SerializeField] private Text raceLbl;
    [SerializeField] private Text raceSubTypeLbl;
    [SerializeField] private InputField spawnMinField;
    [SerializeField] private InputField spawnMaxField;
    [SerializeField] private InputField lvlMinField;
    [SerializeField] private InputField lvlMaxField;

    public void SetSetup(InitialRaceSetup setup) {
        this.setup = setup;
        UpdateVisuals();
    }

    public void UpdateVisuals() {
        raceLbl.text = setup.race.race.ToString();
        raceSubTypeLbl.text = setup.race.subType.ToString();
        spawnMinField.text = setup.spawnRange.lowerBound.ToString();
        spawnMaxField.text = setup.spawnRange.upperBound.ToString();
        lvlMinField.text = setup.levelRange.lowerBound.ToString();
        lvlMaxField.text = setup.levelRange.upperBound.ToString();
    }

    public void SetMinSpawnRange(string text) {
        if (!string.IsNullOrEmpty(text)) {
            int val = System.Int32.Parse(text);
            setup.spawnRange.lowerBound = val;
        }
    }
    public void SetMaxSpawnRange(string text) {
        if (!string.IsNullOrEmpty(text)) {
            int val = System.Int32.Parse(text);
            setup.spawnRange.upperBound = val;
        }
    }

    public void SetMinLevelRange(string text) {
        if (!string.IsNullOrEmpty(text)) {
            int val = System.Int32.Parse(text);
            setup.levelRange.lowerBound = val;
        }
    }
    public void SetMaxLevelRange(string text) {
        if (!string.IsNullOrEmpty(text)) {
            int val = System.Int32.Parse(text);
            setup.levelRange.upperBound = val;
        }
    }
}
