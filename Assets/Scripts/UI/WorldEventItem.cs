using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using Events.World_Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldEventItem : PooledObject {

    public Region region { get; private set; }
    public WorldEvent e { get; private set; }

    [SerializeField] private TextMeshProUGUI eventLbl;
    [SerializeField] private Button mainBtn;
    [SerializeField] private GameObject coverGO;

    private System.Action<WorldEventItem> onDestroyAction;

    public void Initialize(Region region, WorldEvent e, System.Action<WorldEventItem> onDestroyAction) {
        this.region = region;
        this.e = e;
        this.onDestroyAction = onDestroyAction;
        eventLbl.text = e.name + " at " + region.name;
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_DESPAWNED, OnWorldEventDespawned); //start listening for when this event despawns in this region
    }

    #region Listeners
    private void OnWorldEventDespawned(Region region, WorldEvent e) {
        if (this.region == region && this.e == e) {
            //destroy this object
            ObjectPoolManager.Instance.DestroyObject(this.gameObject);
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        region = null;
        e = null;
        Messenger.RemoveListener<Region, WorldEvent>(Signals.WORLD_EVENT_DESPAWNED, OnWorldEventDespawned); //stop listening
        onDestroyAction?.Invoke(this);
    }
    #endregion

    #region Mouse Actions
    public void OnClick() {
        if ((UIManager.Instance.regionInfoUI.activeRegion.mainLandmark as TheEye).isInCooldown) {
            PlayerUI.Instance.ShowGeneralConfirmation("Cannot interfere.", "The Eye is currently in cooldown.");
            return;
        }
        if (PlayerManager.Instance.player.mana < TheEye.interfereManaCost) {
            PlayerUI.Instance.ShowGeneralConfirmation("Cannot interfere.", "The Eye is currently in cooldown.");
            return;
        }

        //show interfere ui
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select minion to interfere with event.";
        UIManager.Instance.ShowClickableObjectPicker(characters, OnClickMinion, null, CanChooseMinion, title, layer: 25, showCover: true);
    }
    #endregion

    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SABOTEUR);
    }
    private void OnClickMinion(object c) {
        Character character = c as Character;
        UIManager.Instance.ShowYesNoConfirmation("Send minion to interfere.", "Are you sure you want to send " + character.name + " to interfere with the " + e.name + " event happening at " + region.name + "?", () => Interfere(character), showCover: false, layer: 26);
    }
    private void Interfere(Character character) {
        // (UIManager.Instance.regionInfoUI.activeRegion.mainLandmark as TheEye).StartInterference(region, character); //NOTE: This assumes that the Region Info UI is showing when the event item is clicked.
        // UIManager.Instance.HideObjectPicker();
    }
    public void SetMainButtonState(bool state) {
        mainBtn.interactable = state;
        coverGO.SetActive(!state);
    }
}
