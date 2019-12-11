using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapVisualFactory {

    public static readonly string Item_Object_Prefab_Name = "ItemGameObject";
    public static readonly string Tile_Object_Prefab_Name = "TileObjectGameObject";

    public GameObject CreateNewItemAreaMapObject(POINT_OF_INTEREST_TYPE poiType) {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(Item_Object_Prefab_Name, Vector3.zero, Quaternion.identity, null);
        return obj;
    }

    public GameObject CreateNewTileObjectAreaMapObject(TILE_OBJECT_TYPE objType) {
        GameObject obj = null;
        switch (objType) {
            case TILE_OBJECT_TYPE.TORNADO:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TornadoVisualObject", Vector3.zero, Quaternion.identity, null);
                break;
            default:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(Tile_Object_Prefab_Name, Vector3.zero, Quaternion.identity, null);
                break;
        }
        return obj;
    }
}
