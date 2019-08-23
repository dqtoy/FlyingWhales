using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using EZObjectPools;

public class StructureObject : PooledObject {

    //private HexTile _hexTile;

    //private STRUCTURE_TYPE _structureType;
    //private STRUCTURE_STATE _structureState;

    //private GameObject[] normalParents;
    //private GameObject[] ruinedParents;

    private GameDate expiryDate;

    //[ContextMenu("List Ruined Structures")]
    //public void ListRuinedObjects() {
    //    for (int i = 0; i < ruinedParents.Length; i++) {
    //        Debug.Log(ruinedParents[i].name);
    //    }
    //}

    #region getters/setters
    //internal HexTile hexTile {
    //    get { return _hexTile; }
    //}
    //public STRUCTURE_STATE structureState {
    //    get { return _structureState; }
    //}
    #endregion

    //public void Initialize(STRUCTURE_TYPE structureType, Color structureColor,  STRUCTURE_STATE structureState, HexTile hexTile) {
    //    _hexTile = hexTile;
    //    if(normalParents == null) {
    //        normalParents = gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Normal").Select(x => x.gameObject).ToArray();
    //    }
    //    if (ruinedParents == null) {
    //        ruinedParents = gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Ruined").Select(x => x.gameObject).ToArray();
    //    }
    //    //_structureType = structureType;
    //    SetStructureState(structureState);
    //    SetStructureColor(structureColor);

    //    //if(_agentObj != null && GameManager.Instance.enableGameAgents) {
    //    //    //Initialize Agent Object
    //    //    CityAgent newCityAgent = new CityAgent(this);
    //    //    AIBehaviour attackBehaviour = new AttackHostiles(newCityAgent);
    //    //    newCityAgent.SetAttackBehaviour(attackBehaviour);
    //    //    newCityAgent.SetAgentObj(_agentObj);
    //    //    _agentObj.Initialize(newCityAgent, new int[] { 0 });
    //    //    _agentObj.gameObject.SetActive(true);
    //    //} else {
    //    //    if(_agentObj != null) {
    //    //        _agentObj.gameObject.SetActive(false);
    //    //    }
            
    //    //}
        
    //    gameObject.SetActive(true);
    //}

    //public void SetStructureState(STRUCTURE_STATE structureState) {
    //    _structureState = structureState;
    //    if(structureState == STRUCTURE_STATE.NORMAL) {
    //        for (int i = 0; i < normalParents.Length; i++) {
    //            normalParents[i].SetActive(true);
    //        }
    //        for (int i = 0; i < ruinedParents.Length; i++) {
    //            ruinedParents[i].SetActive(false);
    //        }
    //        //Messenger.RemoveListener(Signals.HOUR_ENDED, CheckForExpiry);
    //    } else {
    //        for (int i = 0; i < normalParents.Length; i++) {
    //            normalParents[i].SetActive(false);
    //        }
    //        for (int i = 0; i < ruinedParents.Length; i++) {
    //            ruinedParents[i].SetActive(true);
    //        }
    //        //if (_agentObj != null && GameManager.Instance.enableGameAgents) {
    //        //    _agentObj.gameObject.SetActive(false);
    //        //    _agentObj.agent.BroadcastDeath();
    //        //}
    //        //QueueForExpiry();
    //    }
    //}

    public void SetStructureColor(Color color) {
        SpriteRenderer[] allColorizers = transform.GetComponentsInChildren<SpriteRenderer>().
            Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

        for (int i = 0; i < allColorizers.Length; i++) {
            allColorizers[i].color = color;
        }
    }

    public void DestroyStructure() {
        //Debug.Log("DESTROY STRUCTURE!");
        ObjectPoolManager.Instance.DestroyObject(gameObject);
		//if(this._hexTile.isHabitable){
		//	this._hexTile.emptyCityGO.SetActive (true);
		//}
        //if (_agentObj != null && GameManager.Instance.enableGameAgents) {
        //    _agentObj.gameObject.SetActive(false);
        //    _agentObj.agent.BroadcastDeath();
        //}
    }
}
