using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionPropertyItem : MonoBehaviour {

    //private Settlement settlement;

    [SerializeField] private TextMeshProUGUI areaNameLbl;

    public void SetArea(Settlement settlement) {
        //this.settlement = settlement;
        areaNameLbl.text = settlement.name;
    }
}
