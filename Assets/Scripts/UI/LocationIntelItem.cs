using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationIntelItem : MonoBehaviour {

    private LocationIntel locationIntel;

    [SerializeField] private TextMeshProUGUI areaNameLbl;

    public void SetLocation(LocationIntel locationIntel) {
        this.locationIntel = locationIntel;
        areaNameLbl.text = locationIntel.location.name;
    }

}
