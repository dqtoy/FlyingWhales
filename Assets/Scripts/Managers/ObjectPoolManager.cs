using UnityEngine;
using System.Collections;
using EZObjectPools;
using System.Collections.Generic;
using System;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour {

    public static ObjectPoolManager Instance = null;

    private Dictionary<string, EZObjectPool> allObjectPools;

    [SerializeField] private GameObject[] UIPrefabs;
    //[SerializeField] internal GameObject[] citizenAvatarPrefabs;
    //[SerializeField] private GameObject[] monsterAvatarPrefabs;
    //[SerializeField] private GameObject[] agentPrefabs;
    [SerializeField] internal GameObject[] otherPrefabs;
    [SerializeField] private GameObject UIObjectPoolParent;

    private void Awake() {
        Instance = this;
        allObjectPools = new Dictionary<string, EZObjectPool>();
    }

    internal void InitializeObjectPools() {
        for (int i = 0; i < UIPrefabs.Length; i++) {
            GameObject currPrefab = UIPrefabs[i];
            EZObjectPool newUIPool = CreateNewPool(currPrefab, currPrefab.name, 500, true, true, false);
            newUIPool.transform.SetParent(UIObjectPoolParent.transform, false);
        }

        //for (int i = 0; i < citizenAvatarPrefabs.Length; i++) {
        //    GameObject currPrefab = citizenAvatarPrefabs[i];
        //    CreateNewPool(currPrefab, currPrefab.name, 300, true, true, false);
        //}

        //for (int i = 0; i < agentPrefabs.Length; i++) {
        //    GameObject currPrefab = agentPrefabs[i];
        //    CreateNewPool(currPrefab, currPrefab.name, 300, true, true, false);
        //}

        for (int i = 0; i < otherPrefabs.Length; i++) {
            GameObject currPrefab = otherPrefabs[i];
            CreateNewPool(currPrefab, currPrefab.name, 100, true, true, false);
        }

        //for (int i = 0; i < CityGenerator.Instance.humanStructures.structures.Length; i++) {
        //    Structures currStructure = CityGenerator.Instance.humanStructures.structures[i];
        //    GameObject[] structurePrefabs = currStructure.structureGameObjects;
        //    for (int j = 0; j < structurePrefabs.Length; j++) {
        //        CreateNewPool(structurePrefabs[j], structurePrefabs[j].name, 100, true, true, false);
        //    }
        //}

        //for (int i = 0; i < CityGenerator.Instance.elvenStructures.structures.Length; i++) {
        //    Structures currStructure = CityGenerator.Instance.elvenStructures.structures[i];
        //    GameObject[] structurePrefabs = currStructure.structureGameObjects;
        //    for (int j = 0; j < structurePrefabs.Length; j++) {
        //        CreateNewPool(structurePrefabs[j], structurePrefabs[j].name, 100, true, true, false);
        //    }
        //}

        //for (int i = 0; i < monsterAvatarPrefabs.Length; i++) {
        //    GameObject currPrefab = monsterAvatarPrefabs[i];
        //    CreateNewPool(currPrefab, currPrefab.name, 100, true, true, false);
        //}
    }

    public GameObject InstantiateObjectFromPool(string poolName, Vector3 position, Quaternion rotation, Transform parent = null) {
        poolName = poolName.ToUpper();
        if (!allObjectPools.ContainsKey(poolName)) {
            throw new Exception("Object Pool does not have key " + poolName);
        }
        GameObject instantiatedObj = null;
        EZObjectPool objectPoolToUse = allObjectPools[poolName];

        if(objectPoolToUse == null) {
            throw new Exception("Cannot find an object pool with name " + poolName);
        } else {
            if(objectPoolToUse.TryGetNextObject(Vector3.zero, rotation, out instantiatedObj)) {
                if(parent != null) {
                    instantiatedObj.transform.SetParent(parent, false);
                }
                instantiatedObj.transform.localPosition = position;
            }
        }
        instantiatedObj.SetActive(true);
        return instantiatedObj;
    }

    public void DestroyObject(GameObject go) {
        PooledObject po = go.GetComponent<PooledObject>();
        if(po == null) {
             Debug.LogWarning("Cannot Destroy Object via Object Pool! Object " + go.name + " is not from an object pool");
        } else {
            po.SendObjectBackToPool();
            po.Reset();
            po.transform.SetParent(po.ParentPool.transform);
            Messenger.Broadcast(Signals.POOLED_OBJECT_DESTROYED, go);
        }
    }

    public EZObjectPool CreateNewPool(GameObject template, string poolName, int size, bool autoResize, bool instantiateImmediate, bool shared) {
        poolName = poolName.ToUpper();
        EZObjectPool newPool = EZObjectPool.CreateObjectPool(template, poolName, size, autoResize, instantiateImmediate, shared);
        //try {
            allObjectPools.Add(poolName, newPool);
        //}catch(Exception e) {
        //    throw new Exception(e.Message + " Pool name " + poolName);
        //}
        
        return newPool;
    }

    public bool HasPool(string key) {
        if (allObjectPools.ContainsKey(key)) {
            return true;
        }
        return false;
    }
}
