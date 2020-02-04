using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RacePortraitAssets {
    public string raceName;
    public RACE race;
    public PortraitAssetCollection maleAssets;
    public PortraitAssetCollection femaleAssets;

    public RacePortraitAssets(RACE race) {
        this.race = race;
        raceName = race.ToString();
        maleAssets = new PortraitAssetCollection(GENDER.MALE);
        femaleAssets = new PortraitAssetCollection(GENDER.FEMALE);
    }

    public PortraitAssetCollection GetPortraitAssetCollection(GENDER gender) {
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
