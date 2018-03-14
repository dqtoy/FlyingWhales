using UnityEngine;
using System.Collections;

[System.Serializable]
public class LandmarkData {
    public LANDMARK_TYPE landmarkType;
    public int durabilityModifier;
    public int appearanceWeight;
    public bool isUnique;
    public MATERIAL[] possibleMaterials; //Possible materials that this landmark can be made of (this affects the landmarks durability)
    public LandmarkItemData[] itemData;

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
