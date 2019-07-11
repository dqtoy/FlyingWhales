using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIHoverHandler))]
public class PartyEmblem : MonoBehaviour {

    private Party party;

    //[SerializeField] private Image frame, outline, sigil;

    public void SetParty(Party party) {
        this.party = party;
        if (party == null) {
            this.gameObject.SetActive(false);
        } else {
            SetVisuals(party.emblemBG, party.emblem, party.partyColor);
            //frame.sprite = party.emblemBG.frame;
            //tint.sprite = party.emblemBG.tint;
            //outline.sprite = party.emblemBG.outline;
            //sigil.sprite = party.emblem;
            //tint.color = party.partyColor;
            //this.gameObject.SetActive(true);
        }
    }
    public void SetVisuals(EmblemBG bg, Sprite emblem, Color color) {
        //frame.sprite = bg.frame;
        //outline.sprite = bg.outline;
        //sigil.sprite = emblem;
        this.gameObject.SetActive(true);
    }

    public void ShowPartyInfo() {
        if (party == null) {
            return;
        }
        UIManager.Instance.ShowSmallInfo(this.party.name);
    }
    public void HidePartyInfo() {
        if (party == null) {
            return;
        }
        UIManager.Instance.HideSmallInfo();
    }
}
