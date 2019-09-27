using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheEyeUI : MonoBehaviour {

    [Header("General")]
    public Button assignBtn;
    public TextMeshProUGUI assignBtnLbl;

    [Header("Minion")]
    public TextMeshProUGUI minionName;
    public CharacterPortrait minionPortrait;
    public Button selectMinionBtn;

    [Header("World Events")]
    [SerializeField] private ScrollRect worldEventScrollView;
    [SerializeField] private GameObject worldEventPrefab;

    public Minion chosenMinion { get; private set; }

    private TheEye theEye;

    List<WorldEventNameplate> activeItems = new List<WorldEventNameplate>();

    #region General
    public void Initialize() {
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnEventSpawned);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_DESPAWNED, OnEventDespawned);
        Messenger.AddListener(Signals.PLAYER_ADJUSTED_MANA, UpdateWorldEventButtonStates);
    }
    public void ShowTheEyeUI(TheEye theEye) {
        this.theEye = theEye;
        gameObject.SetActive(true);
        UpdateWorldEventButtonStates();
    }
    public void HideTheEyeUI() {
        gameObject.SetActive(false);
    }
    private void UpdateWorldEventButtonStates() {
        if (!this.gameObject.activeSelf) {
            return;
        }
        bool state = PlayerManager.Instance.player.mana >= TheEye.interfereManaCost && !theEye.isInCooldown;
        for (int i = 0; i < activeItems.Count; i++) {
            activeItems[i].SetMainButtonState(state);
        }
    }
    #endregion

    #region Listeners
    //List<WorldEventItem> activeItems = new List<WorldEventItem>();
    private void OnEventSpawned(Region region, WorldEvent e) {
        //create world event item
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(worldEventPrefab.name, Vector3.zero, Quaternion.identity, worldEventScrollView.content);
        //WorldEventItem item = go.GetComponent<WorldEventItem>();
        WorldEventNameplate item = go.GetComponent<WorldEventNameplate>();
        item.Initialize(region, e);
        item.SetClickAction(OnClickNameplate);
        activeItems.Add(item);
    }
    private void OnEventDespawned(Region region, WorldEvent e) {
        for (int i = 0; i < activeItems.Count; i++) {
            WorldEventNameplate worldEventNameplate = activeItems[i];
            if (worldEventNameplate.worldEvent == e) {
                activeItems.RemoveAt(i);
                ObjectPoolManager.Instance.DestroyObject(worldEventNameplate.gameObject);
                break;
            }
        }
    }
    //private void OnDestroyWorldEventItem(WorldEventItem item) {
    //    activeItems.Remove(item);
    //}
    #endregion

    #region World Event Nameplate
    private Region region;
    private WorldEvent e;
    private void OnClickNameplate(Region region, WorldEvent worldEvent) {
        if ((UIManager.Instance.regionInfoUI.activeRegion.mainLandmark as TheEye).isInCooldown) {
            PlayerUI.Instance.ShowGeneralConfirmation("Cannot interfere.", "The Eye is currently in cooldown.");
            return;
        }
        if (PlayerManager.Instance.player.mana < TheEye.interfereManaCost) {
            PlayerUI.Instance.ShowGeneralConfirmation("Cannot interfere.", "The Eye is currently in cooldown.");
            return;
        }

        this.region = region;
        e = worldEvent;
        //show interfere ui
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select minion to interfere with event.";
        UIManager.Instance.ShowClickableObjectPicker(characters, OnClickMinion, null, CanChooseMinion, title, layer: 25, showCover: true);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.SABOTEUR);
    }
    private void OnClickMinion(Character character) {
        UIManager.Instance.ShowYesNoConfirmation("Send minion to interfere.", "Are you sure you want to send " + character.name + " to interfere with the " + e.name + " event happening at " + region.name + "?", () => Interfere(character), showCover: false, layer: 26);
    }
    private void Interfere(Character character) {
        (UIManager.Instance.regionInfoUI.activeRegion.mainLandmark as TheEye).StartInterference(region, character); //NOTE: This assumes that the Region Info UI is showing when the event item is clicked.
        UIManager.Instance.HideObjectPicker();
    }
    #endregion
}