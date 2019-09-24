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

    #region General
    public void Initialize() {
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnEventSpawned);
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
    List<WorldEventItem> activeItems = new List<WorldEventItem>();
    private void OnEventSpawned(Region region, WorldEvent e) {
        //create world event item
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(worldEventPrefab.name, Vector3.zero, Quaternion.identity, worldEventScrollView.content);
        WorldEventItem item = go.GetComponent<WorldEventItem>();
        item.Initialize(region, e, OnDestroyWorldEventItem);
        activeItems.Add(item);
    }
    private void OnDestroyWorldEventItem(WorldEventItem item) {
        activeItems.Remove(item);
    }
    #endregion
}