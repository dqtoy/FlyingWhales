using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldEventItem : MonoBehaviour {

    private Region region;

    [SerializeField] private TextMeshProUGUI eventLbl;

    public void Initialize(Region region) {
        this.region = region;
        eventLbl.text = region.activeEvent.name + " at " + region.name;
    }
}
