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
    [SerializeField] private TextMeshProUGUI descriptionLbl;
    [SerializeField] private TextMeshProUGUI worldObjLbl;
    [SerializeField] private ToggleGroup tabsToggleGroup;
    [SerializeField] private Toggle overviewTabToggle;
    [SerializeField] private Toggle eventsTabToggle;
    [SerializeField] private GameObject overviewGO;

    [Header("Characters")]
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject characterItemPrefab;

    [Header("Events")]
    [SerializeField] private GameObject eventsListGO;
    [SerializeField] private ScrollRect eventsListScrollView;
    [SerializeField] private GameObject eventListItemPrefab;
    [SerializeField] private TextMeshProUGUI eventDesctiptionLbl;
    [SerializeField] private Button spawnEventBtn;
    [SerializeField] private GameObject worldEventNameplatePrefab;
    [SerializeField] private ScrollRect worldEventsScrollView;


    [Header("Invasion")]
    [SerializeField] private GameObject invConfrimationGO;
    [SerializeField] private TextMeshProUGUI invConfrimationTitleLbl;
    [SerializeField] private TextMeshProUGUI invDescriptionLbl;
    [SerializeField] private MinionPicker invMinionPicker;
    [SerializeField] private Button confirmInvasionBtn;
    [SerializeField] private Button invadeBtn;
    [SerializeField] private Image invadeProgress;
    [SerializeField] private CharacterPortrait invader;

    [Header("Intervention")]
    [SerializeField] private Button interveneBtn;
    [SerializeField] private CharacterPortrait interferingCharacterPortrait;

    [Header("Demonic Landmark")]
    [SerializeField] private PlayerBuildLandmarkUI playerBuildLandmarkUI;
    [SerializeField] private PlayerResearchUI playerResearchUI;
    [SerializeField] private TheProfaneUI theProfaneUI;
    [SerializeField] private PlayerSummonMinionUI playerSummonMinionUI;
    [SerializeField] private PlayerUpgradeUI playerUpgradeUI;
    [SerializeField] private TheEyeUI theEyeUI;

    public Region activeRegion { get; private set; }
    protected List<WorldEventNameplate> activeWorldEventNameplates = new List<WorldEventNameplate>();

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnWorldEventSpawned);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_DESPAWNED, OnWorldEventDespawned);
        //Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_FINISHED_NORMAL, OnWorldEventFinishedNormally);
        //Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_FAILED, OnWorldEventFailed);
        Messenger.AddListener<Region>(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, ShowAppropriateContentOnSignal);
        theEyeUI.Initialize();
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
        //UpdateDemonicLandmarkToggleState();
        ShowAppropriateContentOnOpen();
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
        UpdateAppropriateContentPerUpdateUI();
        //UpdateSpawnEventButton();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        locationPortrait.SetLocation(activeRegion);
        regionNameLbl.text = activeRegion.name;
        regionTypeLbl.text = activeRegion.mainLandmark.specificLandmarkType.LandmarkToString();
    }
    #endregion

    #region Main
    private void UpdateRegionInfo() {
        descriptionLbl.text = activeRegion.description;
        descriptionLbl.text += "\n\n<b>Features: </b>";

        if (activeRegion.features.Count == 0) {
            descriptionLbl.text += "None";
        } else {
            for (int i = 0; i < activeRegion.features.Count; i++) {
                RegionFeature feature = activeRegion.features[i];
                if (i != 0) {
                    descriptionLbl.text += ", ";
                }
                descriptionLbl.text += "<link=\"" + i + "\">" + feature.name + "</link>";
            }
        }
    }
    public void OnHoverFeature(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            UIManager.Instance.ShowSmallInfo(activeRegion.features[index].description);
        }
    }
    public void OnHoverExitFeature() {
        UIManager.Instance.HideSmallInfo();
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
        for (int i = 0; i < activeRegion.charactersAtLocation.Count; i++) {
            Character character = activeRegion.charactersAtLocation[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, charactersScrollView.content);
            CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
            item.SetObject(character);
            item.SetAsDefaultBehaviour();
        }
    }
    #endregion

    #region Invade
    private void UpdateInvadeBtnState() {
        if (activeRegion.coreTile.isCorrupted) {
            invadeBtn.gameObject.SetActive(false);
            invadeProgress.gameObject.SetActive(false);
            invader.gameObject.SetActive(false);
        } else {
            invadeBtn.gameObject.SetActive(true);
            invadeBtn.interactable = activeRegion.CanBeInvaded();
            if (activeRegion.demonicInvasionData.beingInvaded) {
                invadeProgress.gameObject.SetActive(true);
                invadeProgress.fillAmount = ((float)activeRegion.demonicInvasionData.currentDuration / (float)activeRegion.mainLandmark.invasionTicks);
                invader.GeneratePortrait(activeRegion.assignedMinion.character);
                invader.gameObject.SetActive(true);
                invader.SetClickButton(UnityEngine.EventSystems.PointerEventData.InputButton.Left);
            } else {
                invadeProgress.gameObject.SetActive(false);
                invader.gameObject.SetActive(false);
            }
        }
    }
    public void OnClickInvade() {
        ShowInvasionConfirmation();
    }
    private Minion chosenMinionToInvade;
    private void ShowInvasionConfirmation() {
        invConfrimationGO.SetActive(true);
        invConfrimationTitleLbl.text = "Invasion (" + ((int)activeRegion.mainLandmark.invasionTicks / (int)GameManager.ticksPerHour).ToString() + " hours)";
        invDescriptionLbl.text = "Choose a minion that will invade " + activeRegion.name + ". NOTE: That minion will be unavailable while the invasion is ongoing.";
        invMinionPicker.ShowMinionPicker(PlayerManager.Instance.player.minions, CanMinionInvade, ChooseMinionForInvasion);
        chosenMinionToInvade = null;
        UpdateStartInvasionBtn();
    }
    private bool CanMinionInvade(Minion minion) {
        return !minion.isAssigned && minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INVADER);
    }
    private void UpdateStartInvasionBtn() {
        confirmInvasionBtn.interactable = chosenMinionToInvade != null;
    }
    private void ChooseMinionForInvasion(Character character, bool isOn) {
        if (isOn) {
            chosenMinionToInvade = character.minion;
            UpdateStartInvasionBtn();
        }
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
        if(worldEventsScrollView.content.childCount > 0) {
            Utilities.DestroyChildren(worldEventsScrollView.content);
        }
        if (activeRegion.activeEvent != null) {
            GenerateWorldEventNameplate(activeRegion.activeEvent);
        }
        //if (activeRegion.activeEvent != null) {
        //    eventDesctiptionLbl.text = activeRegion.activeEvent.name + "\n" + activeRegion.activeEvent.description;
        //} 
        //else {
        //    eventDesctiptionLbl.text = "No active event.";
        //}
        //UpdateSpawnEventButton();
        //UpdateInterveneButton();
    }
    private void OnWorldEventSpawned(Region region, WorldEvent we) {
        if (isShowing && activeRegion == region) {
            GenerateWorldEventNameplate(we);
        }
    }
    private void OnWorldEventDespawned(Region region, WorldEvent we) {
        if (isShowing && activeRegion == region) {
            RemoveWorldEventNameplate(we);
        }
    }
    private void GenerateWorldEventNameplate(WorldEvent worldEvent) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(worldEventNameplatePrefab.name, Vector3.zero, Quaternion.identity, worldEventsScrollView.content);
        WorldEventNameplate item = go.GetComponent<WorldEventNameplate>();
        item.Initialize(worldEvent);
        activeWorldEventNameplates.Add(item);
    }
    private void RemoveWorldEventNameplate(WorldEvent worldEvent) {
        for (int i = 0; i < activeWorldEventNameplates.Count; i++) {
            WorldEventNameplate worldEventNameplate = activeWorldEventNameplates[i];
            if (worldEventNameplate.worldEvent == worldEvent) {
                activeWorldEventNameplates.RemoveAt(i);
                ObjectPoolManager.Instance.DestroyObject(worldEventNameplate.gameObject);
                break;
            }
        }
    }
    //private void OnWorldEventFinishedNormally(Region region, WorldEvent we) {
    //    if (isShowing && activeRegion == region) {
    //        UpdateEventInfo();
    //    }
    //}
    //private void OnWorldEventFailed(Region region, WorldEvent we) {
    //    if (isShowing && activeRegion == region) {
    //        UpdateEventInfo();
    //    }
    //}
    public void OnClickSpawnEvent() {
        if (eventsListGO.activeInHierarchy) {
            eventsListGO.SetActive(false);
        } else {
            ShowEventsThatCanBeSpawned();
        }
    }
    private void ShowEventsThatCanBeSpawned() {
        //Utilities.DestroyChildren(eventsListScrollView.content);
        //List<WorldEvent> spawnableEvents = StoryEventsManager.Instance.GetEventsThatCanSpawnAt(activeRegion);
        //for (int i = 0; i < spawnableEvents.Count; i++) {
        //    WorldEvent currEvent = spawnableEvents[i];
        //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(eventListItemPrefab.name, Vector3.zero, Quaternion.identity, eventsListScrollView.content);
        //    TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
        //    Button button = go.GetComponent<Button>();
        //    text.text = currEvent.eventType.ToString();
        //    button.onClick.RemoveAllListeners();
        //    button.onClick.AddListener(() => SpawnEvent(currEvent));
        //}
        //eventsListGO.SetActive(true);
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
        interveneBtn.gameObject.SetActive(false);
        //interveneBtn.gameObject.SetActive(activeRegion.activeEvent != null);
        //interferingCharacterPortrait.gameObject.SetActive(false);
        //if (interveneBtn.gameObject.activeSelf) {
        //    interveneBtn.interactable = activeRegion.eventData.interferingCharacter == null && PlayerManager.Instance.player.HasMinionAssignedTo(LANDMARK_TYPE.THE_EYE);
        //    if (activeRegion.eventData.interferingCharacter != null) {
        //        interferingCharacterPortrait.gameObject.SetActive(true);
        //        interferingCharacterPortrait.GeneratePortrait(activeRegion.eventData.interferingCharacter);
        //    }
        //}
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
        activeRegion.eventData.SetInterferingCharacter(character);
        character.minion.assignedRegion.SetAssignedMinion(null); //remove minion from assignment at previous region.
        character.minion.SetAssignedRegion(activeRegion); //only set assigned region to minion.
        UIManager.Instance.HideObjectPicker();
        UpdateInterveneButton();
    }
    #endregion

    #region Demonic Landmarks
    private void ShowAppropriateContentOnSignal(Region region) {
        if (region == activeRegion && overviewTabToggle.isOn) {
            //UpdateDemonicLandmarkToggleState();
            OnDemonicToggleStateChanged(overviewTabToggle.isOn);
        }
    }
    private void ShowAppropriateContentOnOpen() {
        if (overviewTabToggle.isOn) {
            //UpdateDemonicLandmarkToggleState();
            OnDemonicToggleStateChanged(overviewTabToggle.isOn);
        }
    }
    private void UpdateDemonicLandmarkToggleState() {
        //only activate demonic tab if the main landmark of this region is a player landmark and is not the kennel or crypt (Since both of those do not have their own special UI)
        //overviewTabToggle.gameObject.SetActive((activeRegion.mainLandmark.specificLandmarkType.IsPlayerLandmark() && 
        //    activeRegion.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_CRYPT && activeRegion.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_KENNEL) || activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE);

        if (overviewTabToggle.gameObject.activeSelf) {
            overviewTabToggle.group = tabsToggleGroup;
        } else {
            if (overviewTabToggle.isOn) {
                overviewTabToggle.isOn = false;
                eventsTabToggle.isOn = true;
            }
            overviewTabToggle.group = null;
        }
    }
    private void UpdateAppropriateContentPerUpdateUI() {
        if (playerBuildLandmarkUI.gameObject.activeSelf) {
            UpdatePlayerBuildLandmarkUI();
        } else if (playerResearchUI.gameObject.activeSelf) {
            UpdatePlayerResearchUI();
        } else if (theProfaneUI.gameObject.activeSelf) {
            UpdateTheProfaneUI();
        } else if (playerUpgradeUI.gameObject.activeSelf) {
            UpdatePlayerUpgradeUI();
        } else if (playerSummonMinionUI.gameObject.activeSelf) {
            UpdatePlayerSummonMinionUI();
        } else if (needlesUI.gameObject.activeSelf) {
            UpdateTheNeedlesUI();
        } else if (theEyeUI.gameObject.activeSelf) {
            UpdateTheEyeUI();
        }
    }
    public void OnDemonicToggleStateChanged(bool isOn) {
        overviewGO.SetActive(isOn);
        if (isOn) {
            HidePlayerBuildLandmarkUI();
            HidePlayerResearchUI();
            HideTheProfaneUI();
            HidePlayerUpgradeUI();
            HidePlayerSummonMinionUI();
            HideTheEyeUI();
            HideTheNeedlesUI();
            //activate the neeeded UI for the tab
            if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
                ShowPlayerBuildLandmarkUI();
            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_SPIRE) {
                ShowPlayerResearchUI();
            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE) {
                ShowTheProfaneUI();
            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_ANVIL) {
                ShowPlayerUpgradeUI();
            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL) {
                ShowPlayerSummonMinionUI();
            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_EYE) {
                ShowTheEyeUI();
            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_NEEDLES) {
                ShowTheNeedlesUI();
            }
        } else {
            //deactivate the UI for the tab
            HidePlayerBuildLandmarkUI();
            HidePlayerResearchUI();
            HideTheProfaneUI();
            HidePlayerUpgradeUI();
            HidePlayerSummonMinionUI();
            HideTheEyeUI();
            HideTheNeedlesUI();
        }
    }

    #endregion

    #region Player Build Landmark Content
    private void ShowPlayerBuildLandmarkUI() {
        playerBuildLandmarkUI.ShowPlayerBuildLandmarkUI(activeRegion.coreTile);
    }
    private void HidePlayerBuildLandmarkUI() {
        playerBuildLandmarkUI.HidePlayerBuildLandmarkUI();
    }
    private void UpdatePlayerBuildLandmarkUI() {
        playerBuildLandmarkUI.UpdatePlayerBuildLandmarkUI();
    }
    #endregion

    #region Player Research Content
    private void ShowPlayerResearchUI() {
        playerResearchUI.ShowPlayerResearchUI(activeRegion.mainLandmark as TheSpire);
    }
    private void HidePlayerResearchUI() {
        playerResearchUI.HidePlayerResearchUI();
    }
    private void UpdatePlayerResearchUI() {
        playerResearchUI.UpdatePlayerResearchUI();
    }
    #endregion

    #region Player Delay Divine Intervention Content
    private void ShowTheProfaneUI() {
        theProfaneUI.ShowTheProfaneUI(activeRegion.mainLandmark as TheProfane);
    }
    private void HideTheProfaneUI() {
        theProfaneUI.Hide();
    }
    private void UpdateTheProfaneUI() {
        theProfaneUI.UpdateTheProfaneUI();
    }
    #endregion

    #region Player Summon Minion Content
    private void ShowPlayerSummonMinionUI() {
        playerSummonMinionUI.ShowPlayerSummonMinionUI(activeRegion.mainLandmark as ThePortal);
    }
    private void HidePlayerSummonMinionUI() {
        playerSummonMinionUI.HidePlayerSummonMinionUI();
    }
    private void UpdatePlayerSummonMinionUI() {
        playerSummonMinionUI.UpdatePlayerSummonMinionUI();
    }
    #endregion

    #region Player Upgrade Content
    private void ShowPlayerUpgradeUI() {
        playerUpgradeUI.ShowPlayerUpgradeUI(activeRegion.mainLandmark as TheAnvil);
    }
    private void HidePlayerUpgradeUI() {
        playerUpgradeUI.HidePlayerResearchUI();
    }
    private void UpdatePlayerUpgradeUI() {
        playerUpgradeUI.UpdatePlayerUpgradeUI();
    }
    public void OnPlayerUpgradeDone() {
        if (playerUpgradeUI.gameObject.activeSelf) {
            playerUpgradeUI.OnUpgradeDone();
        }
    }
    #endregion

    #region The Eye
    private void ShowTheEyeUI() {
        theEyeUI.ShowTheEyeUI(activeRegion.mainLandmark as TheEye);
    }
    private void HideTheEyeUI() {
        theEyeUI.HideTheEyeUI();
    }
    private void UpdateTheEyeUI() {
        theEyeUI.UpdateTheEyeUI();
    }
    #endregion

    #region The Needles
    [SerializeField] private TheNeedlesUI needlesUI;
    private void ShowTheNeedlesUI() {
        needlesUI.ShowTheNeedlesUI(activeRegion.mainLandmark as TheNeedles);
    }
    private void HideTheNeedlesUI() {
        needlesUI.HideTheNeedlesUI();
    }
    private void UpdateTheNeedlesUI() {
        needlesUI.UpdateUI();
    }
    #endregion
}
