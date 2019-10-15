using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileObjectDB {

    //tile objects
    public static Dictionary<TILE_OBJECT_TYPE, TileObjectData> tileObjectData = new Dictionary<TILE_OBJECT_TYPE, TileObjectData>() {
        { TILE_OBJECT_TYPE.SUPPLY_PILE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 10000,
            neededTraitType = typeof(Craftsman)
        } },

        { TILE_OBJECT_TYPE.BED, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitType = typeof(Craftsman),
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.TIREDNESS_RECOVERY, value = 20 }
            }
        } },

        { TILE_OBJECT_TYPE.DESK, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitType = typeof(Craftsman),
            providedFacilities = new ProvidedFacility[] {
                 new ProvidedFacility() { type = FACILITY_TYPE.SIT_DOWN_SPOT, value = 10 }
            }
        } },

        { TILE_OBJECT_TYPE.GUITAR, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitType = typeof(Craftsman),
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.HAPPINESS_RECOVERY, value = 10 }
            }
        } },
        { TILE_OBJECT_TYPE.TABLE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitType = typeof(Craftsman),
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.FULLNESS_RECOVERY, value = 20 },
                new ProvidedFacility() { type = FACILITY_TYPE.SIT_DOWN_SPOT, value = 5 }
            }
        } },
        { TILE_OBJECT_TYPE.TREE_OBJECT, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitType = typeof(Craftsman),
        } },
    };

    public static bool HasTileObjectData(TILE_OBJECT_TYPE objType) {
        return tileObjectData.ContainsKey(objType);
    }
    public static TileObjectData GetTileObjectData(TILE_OBJECT_TYPE objType) {
        if (tileObjectData.ContainsKey(objType)) {
            return tileObjectData[objType];
        }
        //Debug.LogWarning("No tile data for type " + objType.ToString() + " used default tileobject data");
        return TileObjectData.Default;
    }
    public static bool TryGetTileObjectData(TILE_OBJECT_TYPE objType, out TileObjectData data) {
        if (tileObjectData.ContainsKey(objType)) {
            data = tileObjectData[objType];
            return true;
        }
        data = new TileObjectData();
        return false;
    }
}

public struct TileObjectData {
    public int constructionCost;
    public int constructionTime; //in ticks
    public int maxHP;
    public System.Type neededTraitType;
    public ProvidedFacility[] providedFacilities;

    public bool CanProvideFacility(FACILITY_TYPE type) {
        if (providedFacilities != null) {
            for (int i = 0; i < providedFacilities.Length; i++) {
                if (providedFacilities[i].type == type) {
                    return true;
                }
            }
        }
        return false;
    }

    public static TileObjectData Default {
        get {
            return new TileObjectData() {
                constructionCost = 10,
                constructionTime = 12,
                maxHP = 1000,
                neededTraitType = typeof(Craftsman),
                providedFacilities = null
            };
        }
    }
}
public struct ProvidedFacility {
    public FACILITY_TYPE type;
    public int value;
}