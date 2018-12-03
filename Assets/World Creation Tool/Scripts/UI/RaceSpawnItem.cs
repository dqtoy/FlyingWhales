using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceSpawnItem : MonoBehaviour {

    public InitialRaceSetup setup;

    [SerializeField] private Text raceLbl;
    [SerializeField] private Text raceSubTypeLbl;
    [SerializeField] private InputField minField;
    [SerializeField] private InputField maxField;

    public void SetSetup(InitialRaceSetup setup) {
        this.setup = setup;
        UpdateVisuals();
    }

    public void UpdateVisuals() {
        raceLbl.text = setup.race.race.ToString();
        raceSubTypeLbl.text = setup.race.subType.ToString();
        minField.text = setup.spawnRange.lowerBound.ToString();
        maxField.text = setup.spawnRange.upperBound.ToString();
    }

    public void SetMinRange(string text) {
        if (!string.IsNullOrEmpty(text)) {
            int val = System.Int32.Parse(text);
            setup.spawnRange.lowerBound = val;
        }
    }
    public void SetMaxRange(string text) {
        if (!string.IsNullOrEmpty(text)) {
            int val = System.Int32.Parse(text);
            setup.spawnRange.upperBound = val;
        }
    }
}
