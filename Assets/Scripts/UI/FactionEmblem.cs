using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionEmblem : MonoBehaviour {

    private Faction faction;

    //[SerializeField] private Image frameImage;
    //[SerializeField] private Image tintImage;
    //[SerializeField] private Image tintOutlineImage;
    [SerializeField] private Image emblemImage;

    public void SetFaction(Faction faction) {
        this.faction = faction;

        emblemImage.sprite = faction.emblem;
        //tintImage.sprite = faction.emblemBG.tint;
        //tintOutlineImage.sprite = faction.emblemBG.outline;
        //if (faction.emblemBG.outline == null) {
        //    tintOutlineImage.gameObject.SetActive(false);
        //} else {
        //    tintOutlineImage.gameObject.SetActive(true);
        //}
        
        //emblemImage.sprite = faction.emblemSymbol;

        //Color tintColor = faction.factionColor;
        ////tintColor.a = 145f/255f;
        //tintImage.color = tintColor;

        //Color emblemColor;
        //ColorUtility.TryParseHtmlString("#333333", out emblemColor);
        //emblemImage.color = emblemColor;
    }
}
