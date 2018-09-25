using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LandmarkData {
    [Header("General Data")]
    public string landmarkTypeString;
    public LANDMARK_TYPE landmarkType;
    public List<LANDMARK_TAG> uniqueTags;
    public LandmarkItemData[] itemData;
    public Sprite landmarkObjectSprite;
    public Sprite landmarkTypeIcon;
    public BiomeLandmarkSpriteListDictionary biomeTileSprites;
    public List<LandmarkStructureSprite> neutralTileSprites; //These are the sprites that will be used if landmark is not owned by a race
    public List<LandmarkStructureSprite> humansLandmarkTileSprites;
    public List<LandmarkStructureSprite> elvenLandmarkTileSprites;
    public List<PASSABLE_TYPE> possibleSpawnPoints;
    public MonsterPartyComponent startingMonsterSpawn;

    [Header("Monster Spawner")]
    public bool isMonsterSpawner;
    public List<MonsterSet> monsterSets;
    public int monsterSpawnCooldown;

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
