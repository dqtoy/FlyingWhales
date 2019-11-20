using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaceMarkerAsset {
    public string raceName;
    public RACE race;
    public GenderMarkerAsset maleAssets;
    public GenderMarkerAsset femaleAssets;

    public RaceMarkerAsset(RACE race) {
        this.race = race;
        raceName = race.ToString();
        maleAssets = new GenderMarkerAsset(GENDER.MALE);
        femaleAssets = new GenderMarkerAsset(GENDER.FEMALE);
    }

    public GenderMarkerAsset GetMarkerAsset(GENDER gender) {
        switch (gender) {
            case GENDER.MALE:
                return maleAssets;
            case GENDER.FEMALE:
                return femaleAssets;
            default:
                return null;
        }
    }
}
