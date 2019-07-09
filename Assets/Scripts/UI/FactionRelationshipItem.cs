using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionRelationshipItem : MonoBehaviour {

    //private Faction faction;
    //private FactionRelationship rel;

    [SerializeField] private FactionEmblem emblem;
    [SerializeField] private TextMeshProUGUI statusLbl;

    public void SetData(Faction faction, FactionRelationship rel) {
        //this.faction = faction;
        //this.rel = rel;

        emblem.SetFaction(faction);
        statusLbl.text = Utilities.NormalizeString(rel.relationshipStatus.ToString());
    }
}
