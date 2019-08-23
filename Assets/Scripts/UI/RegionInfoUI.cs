using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegionInfoUI : UIMenu {

    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI regionNameLbl;
    [SerializeField] private TextMeshProUGUI regionTypeLbl;
    [SerializeField] private LocationPortrait locationPortrait;

    [Header("Main")]
    [SerializeField] private TextMeshProUGUI worldObjLbl;
    [SerializeField] private Button invadeBtn;
    [SerializeField] private Image invadeProgress;

    [Header("Characters")]
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject characterItemPrefab;

    [Header("Events")]
    [SerializeField] private GameObject eventsListGO;
    [SerializeField] private ScrollRect eventsListScrollView;
    [SerializeField] private GameObject eventListItemPrefab;
    [SerializeField] private TextMeshProUGUI eventDesctiptionLbl;
    [SerializeField] private Button spawnEventBtn;

    public Region activeRegion { get; private set; }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, BaseLandmark>(Signals.CHARACTER_ENTERED_LANDMARK, OnCharacterEnteredLandmark);
        Messenger.AddListener<Character, BaseLandmark>(Signals.CHARACTER_EXITED_LANDMARK, OnCharacterExitedLandmark);
        Messenger.AddListener<BaseLandmark, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnWorldEventSpawned);
        Messenger.AddListener<BaseLandmark, WorldEvent>(Signals.WORLD_EVENT_FINISHED_NORMAL, OnWorldEventFinishedNormally);
    }

    public override void OpenMenu() {
        base.OpenMenu();
        Region previousRegion = activeRegion;
        if (previousRegion != null) {
            previousRegion.ShowTransparentBorder();
        }
        activeRegion = _data as Region;
        UpdateBasicInfo();
        UpdateRegionInfo();
        UpdateCharacters();
        UpdateInvadeBtnState();
        UpdateEventInfo();
        eventsListGO.SetActive(false);
        activeRegion.CenterCameraOnRegion();
        activeRegion.ShowSolidBorder();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        activeRegion.ShowTransparentBorder();
        activeRegion = null;
    }

    public void UpdateInfo() {
        UpdateRegionInfo();
        UpdateInvadeBtnState();
        UpdateSpawnEventButton();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        locationPortrait.SetLocation(activeRegion.mainLandmark);
        regionNameLbl.text = activeRegion.name;
        regionTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(activeRegion.mainLandmark.specificLandmarkType.ToString());
    }
    #endregion

    #region Main
    private void UpdateRegionInfo() {
        worldObjLbl.text = "<b>World Object: </b>" + (activeRegion.mainLandmark.worldObj?.worldObjectName ?? "None");
    }
    #endregion

    #region Characters
    private void OnCharacterEnteredLandmark(Character character, BaseLandmark landmark) {
        if (landmark.tileLocation.region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void OnCharacterExitedLandmark(Character character, BaseLandmark landmark) {
        if (landmark.tileLocation.region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void UpdateCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        for (int i = 0; i < activeRegion.mainLandmark.charactersHere.Count; i++) {
            Character character = activeRegion.mainLandmark.charactersHere[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, charactersScrollView.content);
            LandmarkCharacterItem item = go.GetComponent<LandmarkCharacterItem>();
            item.SetCharacter(character, this);
        }
    }
    #endregion

    #region Invade
    private void UpdateInvadeBtnState() {
        invadeBtn.interactable = activeRegion.CanBeInvaded();
        if (activeRegion == PlayerManager.Instance.player.invadingRegion) {
            invadeProgress.gameObject.SetActive(true);
            invadeProgress.fillAmount = ((float)activeRegion.ticksInInvasion / (float)activeRegion.mainLandmark.invasionTicks);
        } else {
            invadeProgress.gameObject.SetActive(false);
        }
    }
    public void OnClickInvade() {
        activeRegion.StartInvasion();
        UpdateInvadeBtnState();
    }
    #endregion

    #region Events
    private void UpdateEventInfo() {
        if (activeRegion.mainLandmark.activeEvent != null) {
            eventDesctiptionLbl.text = activeRegion.mainLandmark.activeEvent.name + "\n" + activeRegion.mainLandmark.activeEvent.description;
        } else {
            eventDesctiptionLbl.text = "No active event.";
        }
        UpdateSpawnEventButton();
    }
    private void OnWorldEventSpawned(BaseLandmark landmark, WorldEvent we) {
        if (isShowing && activeRegion.mainLandmark == landmark) {
            UpdateEventInfo();
        }
    }
    private void OnWorldEventFinishedNormally(BaseLandmark landmark, WorldEvent we) {
        if (isShowing && activeRegion.mainLandmark == landmark) {
            UpdateEventInfo();
        }
    }
    public void OnClickSpawnEvent() {
        if (eventsListGO.activeInHierarchy) {
            eventsListGO.SetActive(false);
        } else {
            ShowEventsThatCanBeSpawned();
        }
    }
    private void ShowEventsThatCanBeSpawned() {
        Utilities.DestroyChildren(eventsListScrollView.content);
        List<WorldEvent> spawnableEvents = StoryEventsManager.Instance.GetEventsThatCanSpawnAt(activeRegion.mainLandmark);
        for (int i = 0; i < spawnableEvents.Count; i++) {
            WorldEvent currEvent = spawnableEvents[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(eventListItemPrefab.name, Vector3.zero, Quaternion.identity, eventsListScrollView.content);
            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
            Button button = go.GetComponent<Button>();
            text.text = currEvent.eventType.ToString();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SpawnEvent(currEvent));
        }
        eventsListGO.SetActive(true);
    }
    private void SpawnEvent(WorldEvent we) {
        activeRegion.mainLandmark.SpawnEvent(we);
        eventsListGO.SetActive(false);
    }
    private void UpdateSpawnEventButton() {
        spawnEventBtn.interactable = activeRegion.mainLandmark.CanSpawnNewEvent();
        if (!spawnEventBtn.interactable) {
            eventsListGO.SetActive(false);
        }
    }
    #endregion
}
