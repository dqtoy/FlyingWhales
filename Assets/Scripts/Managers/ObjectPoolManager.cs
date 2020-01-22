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
    [SerializeField] internal GameObject[] otherPrefabs;
    [SerializeField] private GameObject UIObjectPoolParent;

    public List<GoapNode> goapNodesPool { get; private set; }
    public List<OpinionData> opinionDataPool { get; private set; }

    private void Awake() {
        Instance = this;
        allObjectPools = new Dictionary<string, EZObjectPool>();
    }
    //public void StartInitializeObjectPoolsCoroutine() {
    //    StartCoroutine(InitializeObjectPoolsCoroutine());
    //}
    public IEnumerator InitializeObjectPoolsCoroutine() {
        for (int i = 0; i < UIPrefabs.Length; i++) {
            GameObject currPrefab = UIPrefabs[i];
            EZObjectPool newUIPool = CreateNewPool(currPrefab, currPrefab.name, 300, true, true, false);
            newUIPool.transform.SetParent(UIObjectPoolParent.transform, false);
            yield return null;
        }
        for (int i = 0; i < otherPrefabs.Length; i++) {
            GameObject currPrefab = otherPrefabs[i];
            CreateNewPool(currPrefab, currPrefab.name, 80, true, true, false);
            yield return null;
        }
        ConstructGoapNodes();
        MapGenerator.Instance.SetIsCoroutineRunning(false);
    }
    public void InitializeObjectPools() {
        for (int i = 0; i < UIPrefabs.Length; i++) {
            GameObject currPrefab = UIPrefabs[i];
            EZObjectPool newUIPool = CreateNewPool(currPrefab, currPrefab.name, 300, true, true, false);
            newUIPool.transform.SetParent(UIObjectPoolParent.transform, false);
        }

        for (int i = 0; i < otherPrefabs.Length; i++) {
            GameObject currPrefab = otherPrefabs[i];
            CreateNewPool(currPrefab, currPrefab.name, 80, true, true, false);
        }

        ConstructGoapNodes();
        ConstructOpinionDataPool();
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

    #region Goap Node
    private void ConstructGoapNodes() {
        goapNodesPool = new List<GoapNode>();
    }
    public GoapNode CreateNewGoapPlanJob(int cost, int level, GoapAction action, IPointOfInterest target) {
        GoapNode node = GetGoapNodeFromPool();
        node.Initialize(cost, level, action, target);
        return node;
    }
    public void ReturnGoapNodeToPool(GoapNode node) {
        node.Reset();
        goapNodesPool.Add(node);
    }
    private GoapNode GetGoapNodeFromPool() {
        if(goapNodesPool.Count > 0) {
            GoapNode node = goapNodesPool[0];
            goapNodesPool.RemoveAt(0);
            return node;
        }
        return new GoapNode();
    }
    #endregion

    #region Opinion Data
    private void ConstructOpinionDataPool() {
        opinionDataPool = new List<OpinionData>();
    }
    public OpinionData CreateNewOpinionData() {
        OpinionData data = GetOpinionDataFromPool();
        data.Initialize();
        return data;
    }
    public void ReturnOpinionDataToPool(OpinionData data) {
        data.Reset();
        opinionDataPool.Add(data);
    }
    private OpinionData GetOpinionDataFromPool() {
        if (opinionDataPool.Count > 0) {
            OpinionData data = opinionDataPool[0];
            opinionDataPool.RemoveAt(0);
            return data;
        }
        return new OpinionData();
    }
    #endregion
}
