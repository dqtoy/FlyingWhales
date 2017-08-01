using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class StructureObject : MonoBehaviour {

    private STRUCTURE_TYPE _structureType;
    private STRUCTURE_STATE _structureState;

    private GameObject[] normalParents;
    private GameObject[] ruinedParents;

    private DateTime expiryDate;

    [ContextMenu("List Ruined Structures")]
    public void ListRuinedObjects() {
        for (int i = 0; i < ruinedParents.Length; i++) {
            Debug.Log(ruinedParents[i].name);
        }
    }

    public void Initialize(STRUCTURE_TYPE structureType, Color structureColor,  STRUCTURE_STATE structureState) {
        normalParents = gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Normal").Select(x => x.gameObject).ToArray();
        ruinedParents = gameObject.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Ruined").Select(x => x.gameObject).ToArray();
        _structureType = structureType;
        SetStructureState(structureState);
        SetStructureColor(structureColor);
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
        EventManager.Instance.onWeekEnd.RemoveListener(CheckForExpiry);
        Destroy(gameObject);
    }

    private void QueueForExpiry() {
        expiryDate = Utilities.GetNewDateAfterNumberOfDays(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 180);
        EventManager.Instance.onWeekEnd.AddListener(CheckForExpiry);
    }

    private void CheckForExpiry() {
        if (expiryDate.Year == GameManager.Instance.year && expiryDate.Month == GameManager.Instance.month && expiryDate.Day == GameManager.Instance.days) {
            DestroyStructure();
        }
    }
}
