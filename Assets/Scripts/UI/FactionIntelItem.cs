using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionIntelItem : MonoBehaviour {

    private FactionIntel intel;

    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private FactionEmblem factionEmblem;

    public void SetFactionIntel(FactionIntel intel) {
        this.intel = intel;
        factionNameLbl.text = intel.faction.name;
        factionEmblem.SetFaction(intel.faction);
    }
}
