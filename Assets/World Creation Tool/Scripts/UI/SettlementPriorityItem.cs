using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettlementPriorityItem : MonoBehaviour {

    private SettlementPriorityItem _item;

    [SerializeField] private Text indexLbl;
    [SerializeField] private RectTransform settingsParent;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    public void SetItem(SettlementPriorityItem item) {
        _item = item;
    }

    public void AddSetting() {

    }
    public void MoveUp() {

    }
    public void MoveDown() {

    }
}
