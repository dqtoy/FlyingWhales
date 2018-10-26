using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionIntelItem : MonoBehaviour, IDragParentItem {

    private FactionIntel intel;

    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private DraggableItem draggable;

    public FactionEmblem factionEmblem;

    public object associatedObj {
        get { return intel; }
    }

    public void SetFactionIntel(FactionIntel intel) {
        this.intel = intel;
        factionNameLbl.text = intel.faction.name;
        factionEmblem.SetFaction(intel.faction);
        draggable.SetAssociatedObject(intel);
    }
}
