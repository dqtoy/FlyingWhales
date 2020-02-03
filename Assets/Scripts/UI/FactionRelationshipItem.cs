using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionRelationshipItem : MonoBehaviour {
    [SerializeField] private FactionEmblem emblem;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI statusLbl;

    public void SetData(Faction faction, FactionRelationship rel) {
        emblem.SetFaction(faction);
        nameLbl.text = faction.name;
<<<<<<< Updated upstream
        statusLbl.text = Utilities.NormalizeString(rel.relationshipStatus.ToString());
=======
        statusLbl.text = Ruinarch.Utilities.NormalizeStringUpperCaseFirstLetterOnly(rel.relationshipStatus.ToString());
>>>>>>> Stashed changes
    }
}
