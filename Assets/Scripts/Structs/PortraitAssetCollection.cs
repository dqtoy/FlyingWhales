using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PortraitAssetCollection {

    public GENDER gender;
    public List<PortraitAsset> bodyAssets;
    public List<HairSetting> hairAssets;
    public List<PortraitAsset> headAssets;
    public List<PortraitAsset> noseAssets;
    public List<PortraitAsset> mouthAssets;
    public List<PortraitAsset> eyeAssets;
    public List<PortraitAsset> eyebrowAssets;
    public List<Color> hairColors;
}
