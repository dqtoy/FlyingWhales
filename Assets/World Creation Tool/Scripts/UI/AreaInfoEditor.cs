using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfoEditor : MonoBehaviour {

    private Area currentArea;

    [Header("Settlement Priorities")]
    [SerializeField] private GameObject settlementPriorityItemGO;
    [SerializeField] private GameObject settlementPrioritySettingItemGO;
    [SerializeField] private ScrollRect settlementPriorityScrollView;

    public void Show(Area area) {
        currentArea = area;
        this.gameObject.SetActive(true);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }
}
