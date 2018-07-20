using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LandmarkData {
    public string landmarkTypeString;
    public LANDMARK_TYPE landmarkType;
    public List<LANDMARK_TAG> uniqueTags;
    public LandmarkItemData[] itemData;
    public Sprite landmarkObjectSprite;
    public List<LandmarkStructureSprite> landmarkTileSprites;
    public List<PASSABLE_TYPE> possibleSpawnPoints;

    private WeightedDictionary<string> _itemWeights;

    #region getter/setters
    public WeightedDictionary<string> itemWeights {
        get {
            if (_itemWeights == null) {
                _itemWeights = GetItemWeights();
            }
            return _itemWeights;
        }
    }
    #endregion

    private WeightedDictionary<string> GetItemWeights() {
        WeightedDictionary<string> itemWeights = new WeightedDictionary<string>();
        for (int i = 0; i < itemData.Length; i++) {
            itemWeights.AddElement(itemData[i].itemName, itemData[i].exploreWeight);
        }
        return itemWeights;
    }

    public void RemoveItemFromWeights(string itemName) {
        _itemWeights.RemoveElement(itemName);
    }
}
