using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIHoverHandler))]
public class FactionEmblem : MonoBehaviour{

    private Faction faction;

    [SerializeField] private Image emblemImage;
    [SerializeField] private bool forceShow;

    private void OnEnable() {
        Messenger.AddListener<Intel>(Signals.INTEL_ADDED, OnIntelObtained);
    }
    private void OnDisable() {
        Messenger.RemoveListener<Intel>(Signals.INTEL_ADDED, OnIntelObtained);
    }

    public void SetFaction(Faction faction) {
        this.faction = faction;
        UpdateEmblem();
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

    private void UpdateEmblem() {
        if (faction == null) {
            this.gameObject.SetActive(false);
        } else {
            if (forceShow || 
                (PlayerManager.Instance.player != null && PlayerManager.Instance.player.intels.Contains(faction.factionIntel))) {
                //player has intel for this faction
                emblemImage.sprite = faction.emblem;
                this.gameObject.SetActive(true);
            } else {
                //player does not have intel for this faction
                //emblemImage.sprite = faction.emblem;
                this.gameObject.SetActive(false);
            }
        }
    }

    private void OnIntelObtained(Intel intel) {
        if (intel is FactionIntel) {
            if (this.faction != null && (intel as FactionIntel).faction.id == this.faction.id) {
                UpdateEmblem();
            }
        }
    }
}
