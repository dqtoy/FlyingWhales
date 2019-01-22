using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortraitAssetCollection {

    public GENDER gender;
    public List<Sprite> bodyAssets;
    public List<Sprite> hairAssets;
    public List<Sprite> skinAssets;
    public List<Sprite> underAssets;
    public List<Sprite> topAssets;

    public PortraitAssetCollection(GENDER gender) {
        this.gender = gender;
        bodyAssets = new List<Sprite>();
        hairAssets = new List<Sprite>();
        skinAssets = new List<Sprite>();
        underAssets = new List<Sprite>();
        topAssets = new List<Sprite>();
    }
}
