using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PortraitAssetCollection {

    public GENDER gender;
    public List<Sprite> bodyAssets;
    public List<HairSetting> hairAssets;
    public List<Sprite> headAssets;
    public List<Sprite> noseAssets;
    public List<Sprite> mouthAssets;
    public List<Sprite> eyeAssets;
    public List<Sprite> eyebrowAssets;
    public List<Color> hairColors;
}
