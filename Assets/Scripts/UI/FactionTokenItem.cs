using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FactionTokenItem : MonoBehaviour, IDragParentItem {

    private FactionToken token;

    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private DraggableItem draggable;

    public FactionEmblem factionEmblem;

    public object associatedObj {
        get { return token; }
    }

    public void SetFactionToken(FactionToken token) {
        this.token = token;
        factionNameLbl.text = token.faction.name;
        factionEmblem.SetFaction(token.faction);
        draggable.SetAssociatedObject(token);
    }
}
