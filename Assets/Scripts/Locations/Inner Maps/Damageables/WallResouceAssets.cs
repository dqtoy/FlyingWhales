using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallResouceAssets {

    [SerializeField] private WallAssetDictionary wallAssets;

    public WallAsset GetWallAsset(string assetName) {
        return wallAssets[assetName];
    }
}
