using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionPropertyItem : MonoBehaviour {

    //private Area area;

    [SerializeField] private TextMeshProUGUI areaNameLbl;

    public void SetArea(Area area) {
        //this.area = area;
        areaNameLbl.text = area.name;
    }
}
