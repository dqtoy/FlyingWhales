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
    [SerializeField] private CharacterPortrait invader;

    [Header("Characters")]
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject characterItemPrefab;

    [Header("Events")]
    [SerializeField] private GameObject eventsListGO;
    [SerializeField] private ScrollRect eventsListScrollView;
    [SerializeField] private GameObject eventListItemPrefab;
    [SerializeField] private TextMeshProUGUI eventDesctiptionLbl;
    [SerializeField] private Button spawnEventBtn;

    [Header("Invasion")]
    [SerializeField] private GameObject invConfrimationGO;
    [SerializeField] private TextMeshProUGUI invDescriptionLbl;
    [SerializeField] private MinionPicker invMinionPicker;
    [SerializeField] private Button startInvBtn;

    [Header("Intervention")]
    [SerializeField] private Button interveneBtn;
    [SerializeField] private CharacterPortrait interferingCharacterPortrait;

    public Region activeRegion { get; private set; }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnWorldEventSpawned);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_FINISHED_NORMAL, OnWorldEventFinishedNormally);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_FAILED, OnWorldEventFailed);
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
        UpdateBasicInfo();
        UpdateRegionInfo();
        UpdateInvadeBtnState();
        //UpdateSpawnEventButton();
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
        worldObjLbl.text = "<b>World Object: </b>" + (activeRegion.worldObj?.worldObjectName ?? "None");
    }
    #endregion

    #region Characters
    private void OnCharacterEnteredRegion(Character character, Region region) {
        if (region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void OnCharacterExitedRegion(Character character, Region region) {
        if (region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void UpdateCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        for (int i = 0; i < activeRegion.charactersHere.Count; i++) {
            Character character = activeRegion.charactersHere[i];
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
            invader.GeneratePortrait(activeRegion.assignedMinion.character);
            invader.gameObject.SetActive(true);
            invader.SetClickButton(UnityEngine.EventSystems.PointerEventData.InputButton.Left);
        } else {
            invadeProgress.gameObject.SetActive(false);
            invader.gameObject.SetActive(false);
        }
    }
    public void OnClickInvade() {
        ShowInvasionConfirmation();
    }
    private Minion chosenMinionToInvade;
    private void ShowInvasionConfirmation() {
        invConfrimationGO.SetActive(true);
        invDescriptionLbl.text = "Choose a minion that will invade " + activeRegion.name + ". NOTE: That minion will be unavailable while the invasion is ongoing.";
        invMinionPicker.ShowMinionPicker(PlayerManager.Instance.player.minions, CanMinionInvade, ChooseMinionForInvasion);
        chosenMinionToInvade = null;
        UpdateStartInvasionBtn();
    }
    private bool CanMinionInvade(Minion minion) {
        return !minion.isAssigned && minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INVADE);
    }
    private void UpdateStartInvasionBtn() {
        startInvBtn.interactable = chosenMinionToInvade != null;
    }
    private void ChooseMinionForInvasion(Minion minion) {
        chosenMinionToInvade = minion;
        UpdateStartInvasionBtn();
    }
    public void StartInvasion() {
        activeRegion.StartInvasion(chosenMinionToInvade);
        UpdateInvadeBtnState();
        HideStartInvasionConfirmation();
    }
    public void HideStartInvasionConfirmation() {
        chosenMinionToInvade = null;
        invConfrimationGO.SetActive(false);
    }
    #endregion

    #region Events
    private void UpdateEventInfo() {
        if (activeRegion.activeEvent != null) {
            eventDesctiptionLbl.text = activeRegion.activeEvent.name + "\n" + activeRegion.activeEvent.description;
        } else {
            eventDesctiptionLbl.text = "No active event.";
        }
        //UpdateSpawnEventButton();
        UpdateInterveneButton();
    }
    private void OnWorldEventSpawned(Region region, WorldEvent we) {
        if (isShowing && activeRegion == region) {
            UpdateEventInfo();
        }
    }
    private void OnWorldEventFinishedNormally(Region region, WorldEvent we) {
        if (isShowing && activeRegion == region) {
            UpdateEventInfo();
        }
    }
    private void OnWorldEventFailed(Region region, WorldEvent we) {
        if (isShowing && activeRegion == region) {
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
        List<WorldEvent> spawnableEvents = StoryEventsManager.Instance.GetEventsThatCanSpawnAt(activeRegion);
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
        //activeRegion.SpawnEvent(we);
        //eventsListGO.SetActive(false);
    }
    private void UpdateSpawnEventButton() {
        spawnEventBtn.interactable = activeRegion.CanSpawnNewEvent();
        if (!spawnEventBtn.interactable) {
            eventsListGO.SetActive(false);
        }
    }
    private void UpdateInterveneButton() {
        interveneBtn.gameObject.SetActive(activeRegion.activeEvent != null);
        interferingCharacterPortrait.gameObject.SetActive(false);
        if (interveneBtn.gameObject.activeSelf) {
            interveneBtn.interactable = activeRegion.eventData.interferingCharacter == null && PlayerManager.Instance.player.HasMinionAssignedTo(LANDMARK_TYPE.THE_EYE);
            if (activeRegion.eventData.interferingCharacter != null) {
                interferingCharacterPortrait.gameObject.SetActive(true);
                interferingCharacterPortrait.GeneratePortrait(activeRegion.eventData.interferingCharacter);
            }
        }
    }
    public void OnClickInterfere() {
        List<Character> minions = PlayerManager.Instance.player.GetMinionsAssignedTo(LANDMARK_TYPE.THE_EYE);
        UIManager.Instance.ShowClickableObjectPicker(minions, OnClickMinion, title: "Choose minion to send out.", showCover: true, layer: 25);
    }
    private void OnClickMinion(Character character) {
        //show confirmation message
        UIManager.Instance.ShowYesNoConfirmation("Send minion to interfere.", "Are you sure you want to send " + character.name + " to interfere with the event happening at " + activeRegion.name + "?", () => SendOutMinionToInterfere(character), showCover: false, layer: 26);
    }
    private void SendOutMinionToInterfere(Character character) {
        character.minion.assignedRegion.SetAssignedMinion(null); //remove minion from assignment at previous region.
        character.minion.SetAssignedRegion(activeRegion); //only set assigned region to minion.
        activeRegion.eventData.SetInterferingCharacter(character);
        UIManager.Instance.HideObjectPicker();
        UpdateInterveneButton();
    }
    #endregion
}
