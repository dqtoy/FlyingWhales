using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionEmblem : MonoBehaviour {

    private Faction faction;

    [SerializeField] private Image frameImage;
    [SerializeField] private Image colorImage;
    [SerializeField] private Image emblemImage;

    public void SetFaction(Faction faction) {
        this.faction = faction;

        frameImage.sprite = faction.emblemBG.frame;
        colorImage.sprite = faction.emblemBG.tint;
        emblemImage.sprite = faction.emblemSymbol;

        colorImage.color = faction.factionColor;
        emblemImage.color = Color.black;
    }
}
