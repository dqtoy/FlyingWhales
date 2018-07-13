using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortraitAssetCollection {

    public GENDER gender;
    public List<Sprite> bodyAssets;
    public List<HairSetting> hairAssets;
    public List<Sprite> headAssets;
    public List<Sprite> noseAssets;
    public List<Sprite> mouthAssets;
    public List<Sprite> eyeAssets;
    public List<Sprite> eyebrowAssets;

    public PortraitAssetCollection(GENDER gender) {
        this.gender = gender;
        bodyAssets = new List<Sprite>();
        hairAssets = new List<HairSetting>();
        headAssets = new List<Sprite>();
        noseAssets = new List<Sprite>();
        mouthAssets = new List<Sprite>();
        eyeAssets = new List<Sprite>();
        eyebrowAssets = new List<Sprite>();
    }
}
