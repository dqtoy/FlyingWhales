using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[RequireComponent(typeof(UIHoverHandler))]
public class FactionEmblem : MonoBehaviour, IPointerClickHandler{

    private Faction faction;

    [SerializeField] private Image emblemImage;
    [SerializeField] private bool forceShow;

    private void OnEnable() {
        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnTokenObtained);
    }
    private void OnDisable() {
        Messenger.RemoveListener<Token>(Signals.TOKEN_ADDED, OnTokenObtained);
    }

    public void SetFaction(Faction faction) {
        this.faction = faction;
        UpdateEmblem();
    }
    public void ShowFactionInfo() {
        if (this.faction == null) {
            return;
        }
        string text = this.faction.name + "\nRelationship Summary:";
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in faction.relationships) {
            text += "\n" + kvp.Key.name + " - " + kvp.Value.relationshipStatus.ToString();
        }
        UIManager.Instance.ShowSmallInfo(text);
    }
    public void HideSmallInfo() {
        if (this.faction == null) {
            return;
        }
        UIManager.Instance.HideSmallInfo();
    }

    private void UpdateEmblem() {
        if (faction == null || (FactionManager.Instance.neutralFaction != null && faction.id == FactionManager.Instance.neutralFaction.id)) {
            this.gameObject.SetActive(false);   
        } else {
            this.gameObject.SetActive(true);
            emblemImage.sprite = faction.emblem.GetSpriteForSize(emblemImage);
            //if (forceShow || 
            //    (PlayerManager.Instance.player != null && PlayerManager.Instance.player.tokens.Contains(faction.factionToken))) {
            //    //player has intel for this faction
            //    emblemImage.sprite = faction.emblem;
            //    this.gameObject.SetActive(true);
            //} else {
            //    //player does not have intel for this faction
            //    //emblemImage.sprite = faction.emblem;
            //    this.gameObject.SetActive(false);
            //}
        }
    }

    private void OnTokenObtained(Token token) {
        if (token is FactionToken) {
            if (this.faction != null && (token as FactionToken).faction.id == this.faction.id) {
                UpdateEmblem();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (faction != null) {
            UIManager.Instance.ShowFactionInfo(faction);
        }
    }
}
