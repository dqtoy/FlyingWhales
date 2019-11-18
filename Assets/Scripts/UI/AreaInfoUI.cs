using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

using UnityEngine.UI.Extensions;
using EZObjectPools;

public class AreaInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 60;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI landmarkNameLbl;
    [SerializeField] private TextMeshProUGUI landmarkTypeLbl;
    [SerializeField] private TextMeshProUGUI suppliesNameLbl;
    [SerializeField] private GameObject charactersGO;
    [SerializeField] private GameObject logsGO;
    [SerializeField] private LocationPortrait locationPortrait;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private RectTransform visitorsMain;
    [SerializeField] private RectTransform residentsMain;
    [SerializeField] private RectTransform visitorsEmblem;
    [SerializeField] private RectTransform residentsEmblem;
    private List<CharacterNameplateItem> characterItems;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private RectTransform itemsParent;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private Toggle logsToggle;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Content Management")]
    public GameObject normalContentGO;
    public PlayerBuildLandmarkUI playerBuildLandmarkUI;
    public PlayerResearchUI playerResearchUI;
    public TheProfaneUI playerDelayDivineInterventionUI;
    public PlayerSummonMinionUI playerSummonMinionUI;
    [SerializeField] private GameObject cryptGO;
    [SerializeField] private GameObject kennelGO;
    [SerializeField] private PlayerUpgradeUI playerUpgradeUI;
    [SerializeField] private TheEyeUI theEyeUI;


    private CombatGrid combatGrid;

    private LogHistoryItem[] logHistoryItems;
    private ItemContainer[] itemContainers;

    //internal Area currentlyShowingLandmark {
    //    get { return _data as Area; }
    //}

    public HexTile activeTile { get; private set; }
    public Area activeArea {
        get {
            if(activeTile.region.owner == PlayerManager.Instance.player.playerFaction) {
                return PlayerManager.Instance.player.playerArea;
            }
            return activeTile.region.area;
        }
    }
    private float _currentWinChance;

    private bool hideOnShowAreaMap = true;

    internal override void Initialize() {
        base.Initialize();
        characterItems = new List<CharacterNameplateItem>();
        combatGrid = new CombatGrid();
        combatGrid.Initialize();
        LoadLogItems();
        itemContainers = Utilities.GetComponentsInDirectChildren<ItemContainer>(itemsParent.gameObject);
        //Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<Area, Character>(Signals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
        Messenger.AddListener<Area, Character>(Signals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Area>(Signals.AREA_SUPPLIES_CHANGED, OnAreaSuppliesSet);
        Messenger.AddListener<Area>(Signals.AREA_OWNER_CHANGED, OnAreaOwnerChanged);
        //Messenger.AddListener<Area, Character>(Signals.AREA_RESIDENT_ADDED, OnAreaResidentAdded);
        //Messenger.AddListener<Area, Character>(Signals.AREA_RESIDENT_REMOVED, OnAreaResidentRemoved);
        Messenger.AddListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        Messenger.AddListener<Character, Region, Region>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
        Messenger.AddListener<Area, SpecialToken>(Signals.ITEM_ADDED_TO_AREA, OnItemAddedToArea);
        Messenger.AddListener<Area, SpecialToken>(Signals.ITEM_REMOVED_FROM_AREA, OnItemRemovedFromArea);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        //Messenger.AddListener<Region>(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, ShowAppropriateContentOnSignal);
    }

    public override void OpenMenu() {
        HexTile previousTile = activeTile;
        activeTile = _data as HexTile;
        base.OpenMenu();

        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        UpdateAreaInfo();
        //ShowAppropriateContent();
        ShowNormalContent();
        ResetScrollPositions();


        if (previousTile != null) {
            previousTile.SetOutlineState(false);
        }
        if (activeTile != null) {
            activeTile.SetOutlineState(true);
        }
        //UIManager.Instance.SetCoverState(true);
        //if(activeArea.owner != PlayerManager.Instance.player.playerFaction) {
        //    PlayerUI.Instance.attackSlot.ShowAttackButton();
        //}

        //if (InteriorMapManager.Instance.isAnAreaMapShowing && activeArea.areaMap != null) {
        //    bool instantCenter = InteriorMapManager.Instance.currentlyShowingArea != activeArea;
        //    InteriorMapManager.Instance.HideAreaMap();
        //    hideOnShowAreaMap = false;
        //    InteriorMapManager.Instance.ShowAreaMap(activeArea, true, instantCenter);
        //    hideOnShowAreaMap = true;
        //}
    }
    public override void CloseMenu() {
        //Utilities.DestroyChildren(charactersScrollView.content);
        base.CloseMenu();
        if (activeTile != null) {
            activeTile.SetOutlineState(false);
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        activeTile = null;
        //UIManager.Instance.SetCoverState(false);
        PlayerUI.Instance.attackSlot.HideAttackButton();
    }
    public override void SetData(object data) {
        base.SetData(data);
    }

    public void UpdateAreaInfo() {
        if (activeTile == null) {
            return;
        }
        UpdateBasicInfo();
        //UpdateAllHistoryInfo();
        //UpdateAppropriateContentPerUpdateUI();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        landmarkNameLbl.text = activeTile.region.name;
        //if (activeTile.landmarkOnTile.specificLandmarkType.IsPlayerLandmark() || activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.NONE) { //TODO: Unify names!
        //    //locationPortrait.SetLocation(activeTile.landmarkOnTile);
        //} else {
        //    landmarkNameLbl.text = activeArea.name;
        //    //locationPortrait.SetLocation(activeTile.areaOfTile);
        //}
        locationPortrait.SetLocation(activeTile.region);
        //landmarkTypeLbl.text = activeArea.GetAreaTypeString();
        landmarkTypeLbl.text = activeTile.landmarkOnTile.specificLandmarkType.LandmarkToString();
        UpdateSupplies();
    }
    private void OnAreaSuppliesSet(Area area) {
        if (this.isShowing && activeTile.id == area.id) {
            UpdateSupplies();
        }
    }
    private void UpdateSupplies() {
        suppliesNameLbl.text = activeArea.suppliesInBank.ToString();
    }
    private void OnAreaOwnerChanged(Area area) {
        if (this.isShowing && activeTile.id == area.id) {
            UpdateBasicInfo();
        }
    }
    #endregion

    #region Log History
    private void LoadLogItems() {
        logHistoryItems = new LogHistoryItem[MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }
    }
    //private void UpdateHistory(object obj) {
    //    if (obj is Area && activeTile != null && (obj as Area).id == activeTile.id) {
    //        UpdateAllHistoryInfo();
    //    }
    //}
    //private void UpdateAllHistoryInfo() {
    //    List<Log> landmarkHistory = new List<Log>(activeTile.areaOfTile.history.OrderByDescending(x => x.id));
    //    for (int i = 0; i < logHistoryItems.Length; i++) {
    //        LogHistoryItem currItem = logHistoryItems[i];
    //        Log currLog = landmarkHistory.ElementAtOrDefault(i);
    //        if (currLog != null) {
    //            currItem.gameObject.SetActive(true);
    //            currItem.SetLog(currLog);
    //            if (Utilities.IsEven(i)) {
    //                currItem.SetLogColor(evenLogColor);
    //            } else {
    //                currItem.SetLogColor(oddLogColor);
    //            }
    //        } else {
    //            currItem.gameObject.SetActive(false);
    //        }
    //    }
    //    //if (this.gameObject.activeInHierarchy) {
    //    //    StartCoroutine(UIManager.Instance.RepositionTable(logHistoryTable));
    //    //    StartCoroutine(UIManager.Instance.RepositionScrollView(historyScrollView));
    //    //}
    //}
    private bool IsLogAlreadyShown(Log log) {
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if (currItem.log != null) {
                if (currItem.log.id == log.id) {
                    return true;
                }
            }
        }
        return false;
    }
    public void ToggleLogsMenu(bool state) {
        logsToggle.isOn = state;
        logsGO.SetActive(state);
        if (state) {
            for (int i = 0; i < logHistoryItems.Length; i++) {
                LogHistoryItem currItem = logHistoryItems[i];
                currItem.EnvelopContentExecute();
            }
        }
    }
    #endregion

    #region Characters
    private void UpdateCharacters() {
        if (!normalContentGO.activeSelf) { return; }
        Utilities.DestroyChildren(charactersScrollView.content);
        characterItems.Clear();

        List<Character> charactersToShow = new List<Character>();
        for (int i = 0; i < activeTile.region.charactersAtLocation.Count; i++) {
            Character character = activeTile.region.charactersAtLocation[i];
            for (int j = 0; j < character.ownParty.characters.Count; j++) {
                Character currCharacter = character.ownParty.characters[j];
                charactersToShow.Add(currCharacter);
            }
        }

        for (int i = 0; i < activeArea.region.residents.Count; i++) {
            Character resident = activeArea.region.residents[i];
            if (!charactersToShow.Contains(resident)) {
                charactersToShow.Add(resident);
            }
        }

        for (int i = 0; i < charactersToShow.Count; i++) {
            Character currCharacter = charactersToShow[i];
            CreateNewCharacterItem(currCharacter);
        }
    }
    private CharacterNameplateItem GetItem(Character character) {
        CharacterNameplateItem[] items = Utilities.GetComponentsInDirectChildren<CharacterNameplateItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            if (item.character != null) {
                if (item.character.id == character.id) {
                    return item;
                }
            }
        }
        return null;
    }
    private CharacterNameplateItem CreateNewCharacterItem(Character character) {
        if (AlreadyHasItem(character)) {
            return null;
        }
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
        CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
        item.SetObject(character);
        item.SetAsDefaultBehaviour();
        characterItems.Add(item);
        OrderCharacterItems();
        return item;
    }
    private bool AlreadyHasItem(Character character) {
        for (int i = 0; i < characterItems.Count; i++) {
            CharacterNameplateItem item = characterItems[i];
            if (item.character.id == character.id) {
                return true;
            }
        }
        return false;
    }
    private void OnCharacterEnteredArea(Area area, Character character) {
        if (!normalContentGO.activeSelf) { return; }
        if (isShowing && activeTile != null && activeArea == area) {
            //for (int i = 0; i < character.characters.Count; i++) {
            //    Character currCharacter = character.characters[i];
                CreateNewCharacterItem(character);
            //}
        }
    }
    private void OnCharacterExitedArea(Area area, Character character) {
        if (!normalContentGO.activeSelf) { return; }
        if (isShowing && activeTile != null && activeArea == area) {
            //for (int i = 0; i < character.characters.Count; i++) {
            //    Character currCharacter = character.characters[i];
                if (!activeArea.region.residents.Contains(character)) {
                    DestroyItemOfCharacter(character);
                }
            //}
            OrderCharacterItems();
        }
    }
    private void DestroyItemOfCharacter(Character character) {
        CharacterNameplateItem item = GetItem(character);
        if (item != null) {
            characterItems.Remove(item);
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
            OrderCharacterItems();
        }
    }
    public void OrderCharacterItems() {
        visitorsEmblem.SetParent(this.transform);
        residentsEmblem.SetParent(this.transform);
        List<CharacterNameplateItem> visitors = new List<CharacterNameplateItem>();
        //List<LandmarkCharacterItem> travellingVisitors = new List<LandmarkCharacterItem>();

        List<CharacterNameplateItem> residents = new List<CharacterNameplateItem>();
        //List<LandmarkCharacterItem> nonTravellingResidents = new List<LandmarkCharacterItem>();
        for (int i = 0; i < characterItems.Count; i++) {
            CharacterNameplateItem currItem = characterItems[i];
            if (currItem.character.homeArea != null && activeTile.id == currItem.character.homeArea.id) {
                residents.Add(currItem);
            } else {
                visitors.Add(currItem);
            }
        }

        List<CharacterNameplateItem> orderedVisitors = new List<CharacterNameplateItem>(visitors.OrderByDescending(x => x.character.level));
        //orderedVisitors.AddRange(travellingVisitors.OrderByDescending(x => x.character.level));
        List<CharacterNameplateItem> orderedResidents = new List<CharacterNameplateItem>(residents.OrderByDescending(x => x.character.level));
        //orderedResidents.AddRange(residents.OrderByDescending(x => x.character.level));

        List<CharacterNameplateItem> orderedItems = new List<CharacterNameplateItem>();
        orderedItems.AddRange(orderedVisitors);
        orderedItems.AddRange(orderedResidents);

        CharacterNameplateItem firstResident = orderedResidents.FirstOrDefault();
        CharacterNameplateItem firstVisitor = orderedVisitors.FirstOrDefault();

        visitorsEmblem.gameObject.SetActive(firstVisitor != null);
        residentsEmblem.gameObject.SetActive(firstResident != null);
        if (firstVisitor != null) {
            visitorsEmblem.SetParent(firstVisitor.transform);
            visitorsEmblem.anchoredPosition = new Vector2(-18.3f, -55f);
        } else {
            visitorsEmblem.SetParent(charactersScrollView.transform);
        }

        if (firstResident != null) {
            residentsEmblem.SetParent(firstResident.transform);
            residentsEmblem.anchoredPosition = new Vector2(-18.3f, -55f);
        } else {
            residentsEmblem.SetParent(charactersScrollView.transform);
        }


        for (int i = 0; i < orderedItems.Count; i++) {
            CharacterNameplateItem currItem = orderedItems[i];
            currItem.transform.SetSiblingIndex(i);
        }
    }
    private void OnCharacterDied(Character character) {
        if (!normalContentGO.activeSelf) { return; }
        if (this.isShowing) {
            DestroyItemOfCharacter(character);
        }
    }
    private void OnCharacterMigratedHome(Character character, Region previousHome, Region newHome) {
        if (this.isShowing) {
            if ((previousHome != null && previousHome.coreTile.id == activeTile.id) || newHome.coreTile.id == activeTile.id) {
                UpdateCharacters();
            }
        }
    }
    private void OnCharacterLevelChanged(Character character) {
        if (!normalContentGO.activeSelf) { return; }
        if (this.isShowing) {
            if (GetItem(character) != null) {
                OrderCharacterItems();
            }
        }
    }
    #endregion

    #region Items
    private void OnItemAddedToArea(Area area, SpecialToken token) {
        if (this.isShowing && activeTile.id == area.id) {
            UpdateItems();
        }
    }
    private void OnItemRemovedFromArea(Area area, SpecialToken token) {
        if (this.isShowing && activeTile.id == area.id) {
            UpdateItems();
        }
    }
    private void UpdateItems() {
        if (!normalContentGO.activeSelf) { return; }
        for (int i = 0; i < itemContainers.Length; i++) {
            ItemContainer currContainer = itemContainers[i];
            SpecialToken currToken = activeArea.itemsInArea.ElementAtOrDefault(i);
            currContainer.SetItem(currToken);
        }
    }
    #endregion

    #region Utilities
    public void OnClickCloseBtn() {
        CloseMenu();
    }
    public void CenterOnCoreTile() {
        activeTile.region.CenterCameraOnRegion();
    }
    public void CenterOnTile() {
        activeTile.CenterCameraHere();
    }
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        historyScrollView.verticalNormalizedPosition = 1;
    }
    private void OnInspectAll() {
        if (isShowing && activeTile != null) {
            UpdateCharacters();
            //UpdateHiddenUI();
        }
    }
    #endregion

    #region For Testing
    public void ShowLocationInfo() {
        string summary = "Location Job Queue: ";
        if (activeArea.availableJobs.Count > 0) {
            for (int i = 0; i < activeArea.availableJobs.Count; i++) {
                JobQueueItem jqi = activeArea.availableJobs[i];
                if (jqi is GoapPlanJob) {
                    GoapPlanJob gpj = jqi as GoapPlanJob;
                    summary += "\n" + gpj.name + " Targetting " + gpj.targetPOI?.name ?? "None";
                } else {
                    summary += "\n" + jqi.name;
                }
                summary += "\n\tAssigned Character: " + jqi.assignedCharacter?.name ?? "None";

            }
        } else {
            summary += "\nNone";
        }
        summary += "\nActive Quest: ";
        if(activeTile.region.owner != null && activeTile.region.owner.activeQuest != null) {
            summary += activeTile.region.owner.activeQuest.name;
        } else {
            summary += "None";
        }

        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideLocationInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Area Map
    public void ShowAreaMap() {
        PlayerUI.Instance.ShowCorruptTileConfirmation(activeTile);
    }
    private void OnAreaMapOpened(Area area) {
        if (hideOnShowAreaMap) {
            CloseMenu(); //hide area menu when inner map is shown
        }
    }
    private void OnAreaMapClosed(Area area) {
        
    }
    #endregion

    #region Share Intel
    private void OnOpenShareIntelMenu() {
        backButton.interactable = false;
    }
    private void OnCloseShareIntelMenu() {
        backButton.interactable = UIManager.Instance.GetLastUIMenuHistory() != null;
    }
    #endregion

    //#region Content Manager
    //private void ShowAppropriateContentOnSignal(Region region) {
    //    if(region.coreTile == activeTile) {
    //        ShowAppropriateContent();
    //    }
    //}
    //private void ShowAppropriateContent() {
    //    HideNormalContent();
    //    HidePlayerBuildLandmarkUI();
    //    HidePlayerResearchUI();
    //    HideCryptUI();
    //    HideKennelUI();
    //    HidePlayerDelayDivineInterventionUI();
    //    HidePlayerUpgradeUI();
    //    HidePlayerSummonMinionUI();
    //    HideTheEyeUI();

    //    if (activeTile.isCorrupted) {
    //        //The things you can do to corrupted landmarks
    //        if (activeTile.landmarkOnTile != null) {
    //            if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.NONE) {
    //                ShowPlayerBuildLandmarkUI();
    //                return;
    //            } else if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_SPIRE) {
    //                ShowPlayerResearchUI();
    //                return;
    //            } else if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_CRYPT) {
    //                ShowCryptUI();
    //                return;
    //            } else if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_KENNEL) {
    //                ShowKennelUI();
    //                return;
    //            } else if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE) {
    //                ShowPlayerDelayDivineInterventionUI();
    //                return;
    //            } else if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_ANVIL) {
    //                ShowPlayerUpgradeUI();
    //                return;
    //            } else if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL) {
    //                ShowPlayerSummonMinionUI();
    //                return;
    //            } else if (activeTile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.THE_EYE) {
    //                ShowTheEyeUI();
    //                return;
    //            }
    //        }
    //    }
    //    ShowNormalContent();
    //}
    //private void UpdateAppropritateContent() {
    //    if (normalContentGO.activeSelf) {
    //        UpdateNormalContent();
    //    }
    //}
    ////This update is only called on UpdateAreaInfo which happens per broadcast of UPDATE_UI which happens per tick
    ////So only put calls here if you want them to be called per tick
    //private void UpdateAppropriateContentPerUpdateUI() {
    //    if (playerBuildLandmarkUI.gameObject.activeSelf) {
    //        UpdatePlayerBuildLandmarkUI();
    //    } else if (playerResearchUI.gameObject.activeSelf) {
    //        UpdatePlayerResearchUI();
    //    } else if(normalContentGO.activeSelf) {
    //        //UpdateItems();
    //    } else if (playerDelayDivineInterventionUI.gameObject.activeSelf) {
    //        UpdatePlayerDelayDivineInterventionUI();
    //    } else if (playerUpgradeUI.gameObject.activeSelf) {
    //        UpdatePlayerUpgradeUI();
    //    } else if (playerSummonMinionUI.gameObject.activeSelf) {
    //        UpdatePlayerSummonMinionUI();
    //    }
    //}
    //#endregion

    #region Normal Content
    private void ShowNormalContent() {
        normalContentGO.SetActive(true);
        UpdateNormalContent();
    }
    private void HideNormalContent() {
        normalContentGO.SetActive(false);
    }
    private void UpdateNormalContent() {
        UpdateCharacters();
        UpdateItems();
    }
    #endregion

    //#region Player Build Landmark Content
    //private void ShowPlayerBuildLandmarkUI() {
    //    playerBuildLandmarkUI.ShowPlayerBuildLandmarkUI(activeTile);
    //}
    //private void HidePlayerBuildLandmarkUI() {
    //    playerBuildLandmarkUI.HidePlayerBuildLandmarkUI();
    //}
    //private void UpdatePlayerBuildLandmarkUI() {
    //    playerBuildLandmarkUI.UpdatePlayerBuildLandmarkUI();
    //}
    //#endregion

    //#region Player Research Content
    //private void ShowPlayerResearchUI() {
    //    playerResearchUI.ShowPlayerResearchUI(activeTile.landmarkOnTile as TheSpire);
    //}
    //private void HidePlayerResearchUI() {
    //    playerResearchUI.HidePlayerResearchUI();
    //}
    //private void UpdatePlayerResearchUI() {
    //    playerResearchUI.UpdatePlayerResearchUI();
    //}
    //#endregion

    //#region Player Delay Divine Intervention Content
    //private void ShowPlayerDelayDivineInterventionUI() {
    //    playerDelayDivineInterventionUI.ShowPlayerDelayDivineInterventionUI(activeTile.landmarkOnTile as TheProfane);
    //}
    //private void HidePlayerDelayDivineInterventionUI() {
    //    playerDelayDivineInterventionUI.HidePlayerDelayDivineInterventionUI();
    //}
    //private void UpdatePlayerDelayDivineInterventionUI() {
    //    playerDelayDivineInterventionUI.UpdatePlayerDelayDivineInterventionUI();
    //}
    //#endregion

    //#region Player Summon Minion Content
    //private void ShowPlayerSummonMinionUI() {
    //    playerSummonMinionUI.ShowPlayerSummonMinionUI(activeTile.landmarkOnTile as ThePortal);
    //}
    //private void HidePlayerSummonMinionUI() {
    //    playerSummonMinionUI.HidePlayerSummonMinionUI();
    //}
    //private void UpdatePlayerSummonMinionUI() {
    //    playerSummonMinionUI.UpdatePlayerSummonMinionUI();
    //}
    //#endregion

    //#region Crypt UI
    //private void ShowCryptUI() {
    //    cryptGO.SetActive(true);
    //}
    //private void HideCryptUI() {
    //    cryptGO.SetActive(false);
    //}
    //#endregion

    //#region Kennel UI
    //private void ShowKennelUI() {
    //    cryptGO.SetActive(true);
    //}
    //private void HideKennelUI() {
    //    cryptGO.SetActive(false);
    //}
    //#endregion

    //#region Player Upgrade Content
    //private void ShowPlayerUpgradeUI() {
    //    playerUpgradeUI.ShowPlayerUpgradeUI(activeTile.landmarkOnTile as TheAnvil);
    //}
    //private void HidePlayerUpgradeUI() {
    //    playerUpgradeUI.HidePlayerResearchUI();
    //}
    //private void UpdatePlayerUpgradeUI() {
    //    playerUpgradeUI.UpdatePlayerUpgradeUI();
    //}
    //public void OnPlayerUpgradeDone() {
    //    if (playerUpgradeUI.gameObject.activeSelf) {
    //        playerUpgradeUI.OnUpgradeDone();
    //    }
    //}
    //#endregion

    //#region The Eye
    //private void ShowTheEyeUI() {
    //    theEyeUI.ShowTheEyeUI(activeTile.landmarkOnTile as TheEye);
    //}
    //private void HideTheEyeUI() {
    //    theEyeUI.HideTheEyeUI();
    //}
    //#endregion
}
