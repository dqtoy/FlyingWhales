using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LandmarkData {
    [Header("General Data")]
    public string landmarkTypeString;
    public LANDMARK_TYPE landmarkType;
    public int minimumTileCount = 1; //how many tiles does this landmark need
    public HEXTILE_DIRECTION connectedTileDirection;
    public List<LANDMARK_TAG> uniqueTags;
    public Sprite landmarkObjectSprite;
    public Sprite landmarkTypeIcon;
    public BiomeLandmarkSpriteListDictionary biomeTileSprites;
    public List<LandmarkStructureSprite> neutralTileSprites; //These are the sprites that will be used if landmark is not owned by a race
    public List<LandmarkStructureSprite> humansLandmarkTileSprites;
    public List<LandmarkStructureSprite> elvenLandmarkTileSprites;
    public LandmarkDefenderWeightDictionary defenderWeightsDict;
    public InteractionWeightDictionary interactionWeightsDict;
    public List<PASSABLE_TYPE> possibleSpawnPoints;
    public bool isUnique;
    public int dailySupplyProduction;
    public int initialDefenderCount;
    public int eventTriggerRate;
    public int noEventTriggerRate;

    [Header("Monster Spawner")]
    public MonsterPartyComponent startingMonsterSpawn;
    public bool isMonsterSpawner;
    public List<MonsterSet> monsterSets;
    public int monsterSpawnCooldown;

    public WeightedDictionary<LandmarkDefender> defenderWeights { get; private set; }
    public WeightedDictionary<LandmarkDefender> firstElementDefenderWeights { get; private set; }
    public WeightedDictionary<INTERACTION_TYPE> interactionWeights { get; private set; }
    public WeightedDictionary<bool> eventTriggerWeights { get; private set; } //true - trigger event, false - do not trigger event

    public void ConstructData() {
        defenderWeights = GetDefenderWeights();
        firstElementDefenderWeights = GetFirstDefenderWeights();
        interactionWeights = GetInteractionWeights();
        eventTriggerWeights = GetEventTriggerWeights();
    }

    private WeightedDictionary<LandmarkDefender> GetDefenderWeights() {
        WeightedDictionary<LandmarkDefender> weights = new WeightedDictionary<LandmarkDefender>();
        foreach (KeyValuePair<LandmarkDefender, int> kvp in defenderWeightsDict) {
            weights.AddElement(kvp.Key, kvp.Value);
        }
        return weights;
    }
    private WeightedDictionary<LandmarkDefender> GetFirstDefenderWeights() {
        WeightedDictionary<LandmarkDefender> weights = new WeightedDictionary<LandmarkDefender>();
        foreach (KeyValuePair<LandmarkDefender, int> kvp in defenderWeightsDict) {
            if (kvp.Key.includeInFirstWeight) {
                weights.AddElement(kvp.Key, kvp.Value);
            }
        }
        return weights;
    }
    private WeightedDictionary<INTERACTION_TYPE> GetInteractionWeights() {
        WeightedDictionary<INTERACTION_TYPE> weights = new WeightedDictionary<INTERACTION_TYPE>();
        foreach (KeyValuePair<INTERACTION_TYPE, int> kvp in interactionWeightsDict) {
            weights.AddElement(kvp.Key, kvp.Value);
        }
        return weights;
    }
    private WeightedDictionary<bool> GetEventTriggerWeights() {
        WeightedDictionary<bool> weights = new WeightedDictionary<bool>();
        weights.AddElement(true, eventTriggerRate);
        weights.AddElement(false, noEventTriggerRate);
        return weights;
    }
}
