using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RacePortraitAssets {
    public RACE race;
    public PortraitAssetCollection maleAssets;
    public PortraitAssetCollection femaleAssets;

    public RacePortraitAssets(RACE race) {
        this.race = race;
        maleAssets = new PortraitAssetCollection(GENDER.MALE);
        femaleAssets = new PortraitAssetCollection(GENDER.FEMALE);
    }
}
