using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaceMarkerAssets {
    public string raceName;
    public RACE race;
    public MarkerAsset maleAssets;
    public MarkerAsset femaleAssets;

    public RaceMarkerAssets(RACE race) {
        this.race = race;
        maleAssets = new MarkerAsset(GENDER.MALE);
        femaleAssets = new MarkerAsset(GENDER.FEMALE);
    }
}
