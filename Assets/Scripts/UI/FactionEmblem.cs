using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIHoverHandler))]
public class FactionEmblem : MonoBehaviour{

    private Faction faction;

    [SerializeField] private Image emblemImage;

    public void SetFaction(Faction faction) {
        this.faction = faction;
        if (faction == null) {
            this.gameObject.SetActive(false);
        } else {
            emblemImage.sprite = faction.emblem;
            this.gameObject.SetActive(true);
        }
        

    }
    public void ShowFactionInfo() {
        if (this.faction == null) {
            return;
        }
        UIManager.Instance.ShowSmallInfo(this.faction.name);
    }
    public void HideSmallInfo() {
        if (this.faction == null) {
            return;
        }
        UIManager.Instance.HideSmallInfo();
    }
}
