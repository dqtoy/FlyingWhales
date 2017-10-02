using UnityEngine;
using System.Collections;

[System.Serializable]
public struct PopulationRates {
    public float draftRate;
    public float researchRate;
    public float productionRate;

    public PopulationRates(float draftRate, float researchRate, float productionRate) {
        this.draftRate = draftRate;
        this.researchRate = researchRate;
        this.productionRate = productionRate;
    }
}
