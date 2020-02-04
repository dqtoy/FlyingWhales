using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenderMarkerAsset {

    public GENDER gender;

    public CharacterClassAssetDictionary characterClassAssets;

    public GenderMarkerAsset(GENDER gender) {
        this.gender = gender;
        characterClassAssets = new CharacterClassAssetDictionary();
    }
}
