using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using EZObjectPools;

public class StructureObject : PooledObject {

    private STRUCTURE_TYPE _structureType;
    private STRUCTURE_STATE _structureState;

    private GameObject[] normalParents;
    private GameObject[] ruinedParents;

    private GameDate expiryDate;

    [SerializeField] private AgentObject _agentObj;

    [ContextMenu("List Ruined Structures")]
    public void ListRuinedObjects() {
        for (int i = 0; i < ruinedParents.Length; i++) {
            Debug.Log(ruinedParents[i].name);
        }
    }

    public void Initialize(STRUCTURE_TYPE structureType, Color structureColor,  STRUCTURE_STATE structureState) {
        if(normalParents == null) {
            normalParents = gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Normal").Select(x => x.gameObject).ToArray();
        }
        if (ruinedParents == null) {
            ruinedParents = gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Ruined").Select(x => x.gameObject).ToArray();
        }
        _structureType = structureType;
        SetStructureState(structureState);
        SetStructureColor(structureColor);

        if(_agentObj != null) {
            //Initialize Agent Object
            CityAgent newCityAgent = new CityAgent();
            AIBehaviour attackBehaviour = new AttackHostiles(newCityAgent);
            newCityAgent.SetAttackBehaviour(attackBehaviour);
            newCityAgent.SetAgentObj(_agentObj);
            _agentObj.Initialize(newCityAgent, new int[] { 0 });
        }
        
        gameObject.SetActive(true);
    }

    public void SetStructureState(STRUCTURE_STATE structureState) {
        _structureState = structureState;
        if(structureState == STRUCTURE_STATE.NORMAL) {
            for (int i = 0; i < normalParents.Length; i++) {
                normalParents[i].SetActive(true);
            }
            for (int i = 0; i < ruinedParents.Length; i++) {
                ruinedParents[i].SetActive(false);
            }
            Messenger.RemoveListener("OnDayEnd", CheckForExpiry);
        } else {
            for (int i = 0; i < normalParents.Length; i++) {
                normalParents[i].SetActive(false);
            }
            for (int i = 0; i < ruinedParents.Length; i++) {
                ruinedParents[i].SetActive(true);
            }
            QueueForExpiry();
        }
    }

    public void SetStructureColor(Color color) {
        SpriteRenderer[] allColorizers = transform.GetComponentsInChildren<SpriteRenderer>().
            Where(x => x.gameObject.tag == "StructureColorizers").ToArray();

        for (int i = 0; i < allColorizers.Length; i++) {
            allColorizers[i].color = color;
        }
    }

    public void DestroyStructure() {
        Debug.Log("DESTROY STRUCTURE!");
        ObjectPoolManager.Instance.DestroyObject(gameObject);
    }

    private void QueueForExpiry() {
        expiryDate = Utilities.GetNewDateAfterNumberOfDays(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 180);
        //SchedulingManager.Instance.AddEntry(expiryDate.month, expiryDate.day, expiryDate.year, () => DestroyStructure());
        Messenger.AddListener("OnDayEnd", CheckForExpiry);
    }

    private void CheckForExpiry() {
        if (expiryDate.year == GameManager.Instance.year && expiryDate.month == GameManager.Instance.month && expiryDate.day == GameManager.Instance.days) {
            DestroyStructure();
        }
    }

    #region overrides
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener("OnDayEnd", CheckForExpiry);
    }
    #endregion
}
