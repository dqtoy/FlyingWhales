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
    [SerializeField] private GameObject landmarkCharacterPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private RectTransform visitorsMain;
    [SerializeField] private RectTransform residentsMain;
    [SerializeField] private RectTransform visitorsEmblem;
    [SerializeField] private RectTransform residentsEmblem;
    private List<LandmarkCharacterItem> characterItems;

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

    private CombatGrid combatGrid;

    private LogHistoryItem[] logHistoryItems;
    private ItemContainer[] itemContainers;

    internal Area currentlyShowingLandmark {
        get { return _data as Area; }
    }

    public Area activeArea { get; private set; }
    private float _currentWinChance;

    internal override void Initialize() {
        base.Initialize();
        characterItems = new List<LandmarkCharacterItem>();
        combatGrid = new CombatGrid();
        combatGrid.Initialize();
        LoadLogItems();
        itemContainers = Utilities.GetComponentsInDirectChildren<ItemContainer>(itemsParent.gameObject);
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, OnPartyExitedLandmark);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Area>(Signals.AREA_SUPPLIES_CHANGED, OnAreaSuppliesSet);
        Messenger.AddListener<Area>(Signals.AREA_OWNER_CHANGED, OnAreaOwnerChanged);
        //Messenger.AddListener<Area, Character>(Signals.AREA_RESIDENT_ADDED, OnAreaResidentAdded);
        //Messenger.AddListener<Area, Character>(Signals.AREA_RESIDENT_REMOVED, OnAreaResidentRemoved);
        Messenger.AddListener<Character>(Signals.CHARACTER_LEVEL_CHANGED, OnCharacterLevelChanged);
        Messenger.AddListener<Character, Area, Area>(Signals.CHARACTER_MIGRATED_HOME, OnCharacterMigratedHome);
    }

    public override void OpenMenu() {
        Area previousArea = activeArea;
        activeArea = _data as Area;
        base.OpenMenu();
        
        UpdateAreaInfo();
        UpdateCharacters();
        ResetScrollPositions();
        
        if (previousArea != null) {
            previousArea.SetOutlineState(false);
        }
        if (activeArea != null) {
            activeArea.SetOutlineState(true);
        }
        //UIManager.Instance.SetCoverState(true);
        if(activeArea.owner != PlayerManager.Instance.player.playerFaction) {
            PlayerUI.Instance.attackSlot.ShowAttackButton();
        }
    }
    public override void CloseMenu() {
        //Utilities.DestroyChildren(charactersScrollView.content);
        base.CloseMenu();
        if (activeArea != null) {
            activeArea.SetOutlineState(false);
        }
        activeArea = null;
        //UIManager.Instance.SetCoverState(false);
        PlayerUI.Instance.attackSlot.HideAttackButton();
    }
    public override void SetData(object data) {
        base.SetData(data);
    }

    public void UpdateAreaInfo() {
        if (activeArea == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateItems();
        UpdateAllHistoryInfo();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(activeArea.coreTile.landmarkOnTile.specificLandmarkType);
        landmarkNameLbl.text = activeArea.name + " (" + activeArea.areaResidents.Count + "/" + activeArea.residentCapacity + ")";
        landmarkTypeLbl.text = activeArea.GetAreaTypeString();
        UpdateSupplies();
        locationPortrait.SetLocation(activeArea);
        ////portrait
        //if (activeArea.locationPortrait != null) {
        //    areaPortrait.gameObject.SetActive(false);
        //    areaPortrait.sprite = activeArea.locationPortrait;
        //} else {
        //    areaPortrait.gameObject.SetActive(false);
        //}
    }
    private void OnAreaSuppliesSet(Area area) {
        if (this.isShowing && activeArea.id == area.id) {
            UpdateSupplies();
        }
    }
    private void UpdateSupplies() {
        suppliesNameLbl.text = activeArea.suppliesInBank.ToString();
    }
    private void OnAreaOwnerChanged(Area area) {
        if (this.isShowing && activeArea.id == area.id) {
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
    private void UpdateHistory(object obj) {
        if (obj is Area && activeArea != null && (obj as Area).id == activeArea.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> landmarkHistory = new List<Log>(activeArea.history.OrderByDescending(x => x.id));
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            Log currLog = landmarkHistory.ElementAtOrDefault(i);
            if (currLog != null) {
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
                if (Utilities.IsEven(i)) {
                    currItem.SetLogColor(evenLogColor);
                } else {
                    currItem.SetLogColor(oddLogColor);
                }
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
        //if (this.gameObject.activeInHierarchy) {
        //    StartCoroutine(UIManager.Instance.RepositionTable(logHistoryTable));
        //    StartCoroutine(UIManager.Instance.RepositionScrollView(historyScrollView));
        //}
    }
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
        Utilities.DestroyChildren(charactersScrollView.content);
        characterItems.Clear();

        List<Character> charactersToShow = new List<Character>(activeArea.charactersAtLocation);
        for (int i = 0; i < activeArea.charactersAtLocation.Count; i++) {
            Character character = activeArea.charactersAtLocation[i];
            for (int j = 0; j < character.ownParty.characters.Count; j++) {
                Character currCharacter = character.ownParty.characters[j];
                charactersToShow.Add(currCharacter);
            }
        }

        for (int i = 0; i < activeArea.areaResidents.Count; i++) {
            Character resident = activeArea.areaResidents[i];
            if (!charactersToShow.Contains(resident)) {
                charactersToShow.Add(resident);
            }
        }

        for (int i = 0; i < charactersToShow.Count; i++) {
            Character currCharacter = charactersToShow[i];
            CreateNewCharacterItem(currCharacter);
        }
    }
    private LandmarkCharacterItem GetItem(Character character) {
        LandmarkCharacterItem[] items = Utilities.GetComponentsInDirectChildren<LandmarkCharacterItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            LandmarkCharacterItem item = items[i];
            if (item.character != null) {
                if (item.character.id == character.id) {
                    return item;
                }
            }
        }
        return null;
    }
    private LandmarkCharacterItem CreateNewCharacterItem(Character character) {
        if (AlreadyHasItem(character)) {
            return null;
        }
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetCharacter(character, this);
        characterItems.Add(item);
        OrderCharacterItems();
        return item;
    }
    private bool AlreadyHasItem(Character character) {
        for (int i = 0; i < characterItems.Count; i++) {
            LandmarkCharacterItem item = characterItems[i];
            if (item.character.id == character.id) {
                return true;
            }
        }
        return false;
    }
    private void OnPartyEnteredLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && activeArea != null && activeArea.id == landmark.tileLocation.areaOfTile.id) {
            for (int i = 0; i < party.characters.Count; i++) {
                Character currCharacter = party.characters[i];
                CreateNewCharacterItem(currCharacter);
            }
        }
    }
    private void OnPartyExitedLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && activeArea != null && activeArea.id == landmark.tileLocation.areaOfTile.id) {
            for (int i = 0; i < party.characters.Count; i++) {
                Character currCharacter = party.characters[i];
                if (!activeArea.areaResidents.Contains(currCharacter)) {
                    DestroyItemOfCharacter(currCharacter);
                }
            }
            OrderCharacterItems();
        }
    }
    private void DestroyItemOfCharacter(Character character) {
        LandmarkCharacterItem item = GetItem(character);
        if (item != null) {
            characterItems.Remove(item);
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
            OrderCharacterItems();
        }
    }
    public void OrderCharacterItems() {
        visitorsEmblem.SetParent(this.transform);
        residentsEmblem.SetParent(this.transform);
        List<LandmarkCharacterItem> nonTravellingVisitors = new List<LandmarkCharacterItem>();
        List<LandmarkCharacterItem> travellingVisitors = new List<LandmarkCharacterItem>();

        List<LandmarkCharacterItem> travellingResidents = new List<LandmarkCharacterItem>();
        List<LandmarkCharacterItem> nonTravellingResidents = new List<LandmarkCharacterItem>();
        for (int i = 0; i < characterItems.Count; i++) {
            LandmarkCharacterItem currItem = characterItems[i];
            if (currItem.character.homeArea != null && activeArea.id == currItem.character.homeArea.id) {
                if (currItem.character.currentParty.specificLocation.tileLocation.areaOfTile.id != activeArea.id || currItem.character.currentParty.icon.isTravelling) { //character is away from home
                    travellingResidents.Add(currItem);
                } else {
                    nonTravellingResidents.Add(currItem);
                }
            } else {
                if (currItem.character.currentParty.icon.isTravelling) {
                    travellingVisitors.Add(currItem);
                } else {
                    nonTravellingVisitors.Add(currItem);
                }
            }
        }

        List<LandmarkCharacterItem> orderedVisitors = new List<LandmarkCharacterItem>(nonTravellingVisitors.OrderByDescending(x => x.character.level));
        orderedVisitors.AddRange(travellingVisitors.OrderByDescending(x => x.character.level));
        List<LandmarkCharacterItem> orderedResidents = new List<LandmarkCharacterItem>(nonTravellingResidents.OrderByDescending(x => x.character.level));
        orderedResidents.AddRange(travellingResidents.OrderByDescending(x => x.character.level));

        List<LandmarkCharacterItem> orderedItems = new List<LandmarkCharacterItem>();
        orderedItems.AddRange(orderedVisitors);
        orderedItems.AddRange(orderedResidents);

        LandmarkCharacterItem firstResident = orderedResidents.FirstOrDefault();
        LandmarkCharacterItem firstVisitor = orderedVisitors.FirstOrDefault();

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
            LandmarkCharacterItem currItem = orderedItems[i];
            currItem.transform.SetSiblingIndex(i);
        }
    }
    private void OnCharacterDied(Character character) {
        if (this.isShowing) {
            DestroyItemOfCharacter(character);
        }
    }
    private void OnCharacterMigratedHome(Character character, Area previousHome, Area newHome) {
        if (this.isShowing) {
            if ((previousHome != null && previousHome.id == activeArea.id) || newHome.id == activeArea.id) {
                UpdateCharacters();
            }
        }
    }
    private void OnCharacterLevelChanged(Character character) {
        if (this.isShowing) {
            if (GetItem(character) != null) {
                OrderCharacterItems();
            }
        }
    }
    #endregion

    #region Items
    private void UpdateItems() {
        for (int i = 0; i < itemContainers.Length; i++) {
            ItemContainer currContainer = itemContainers[i];
            SpecialToken currToken = activeArea.possibleSpecialTokenSpawns.ElementAtOrDefault(i);
            currContainer.SetItem(currToken);
        }
    }
    #endregion

    #region Utilities
    public void OnClickCloseBtn() {
        CloseMenu();
    }
    public void CenterOnCoreLandmark() {
        activeArea.CenterOnCoreLandmark();
    }
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        historyScrollView.verticalNormalizedPosition = 1;
    }
    private void OnInspectAll() {
        if (isShowing && activeArea != null) {
            UpdateCharacters();
            //UpdateHiddenUI();
        }
    }
    #endregion

    #region For Testing
    public void ShowSpecialTokensAtLocation() {
        string summary = "Items at " + activeArea.name + ": ";
        if (activeArea.possibleSpecialTokenSpawns.Count > 0) {
            for (int i = 0; i < activeArea.possibleSpecialTokenSpawns.Count; i++) {
                SpecialToken currToken = activeArea.possibleSpecialTokenSpawns[i];
                summary += "\n" + currToken.name;
                if (currToken is SecretScroll) {
                    summary += ": " + (currToken as SecretScroll).grantedClass;
                }
                summary +=  " owned by " + currToken.ownerName;
            }
        } else {
            summary += "None";
        }
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideSpecialTokensAtLocation() {
        UIManager.Instance.HideSmallInfo();
    }
    public void ClearOutFaction() {
        if (activeArea.owner != null) {
            if (activeArea.owner.ownedAreas.Count <= 1) {
                Debug.Log(activeArea.owner.name + " only has 1 area left! Not allowing clear out this areas faction...");
                return;
            }

            List<Character> charactersToMove = new List<Character>();
            for (int i = 0; i < activeArea.areaResidents.Count; i++) {
                Character currResident = activeArea.areaResidents[i];
                if (currResident.faction.id == activeArea.owner.id) {
                    charactersToMove.Add(currResident);
                }
            }
            //DefenderGroup defender = activeArea.GetFirstDefenderGroup();
            //if (defender != null && defender.party != null) {
            //    List<Character> defenders = new List<Character>(defender.party.characters);
            //    for (int i = 0; i < defenders.Count; i++) {
            //        Character currDefender = defenders[i];
            //        if (currDefender.faction.id == activeArea.owner.id) {
            //            defender.RemoveCharacterFromGroup(currDefender);
            //            charactersToMove.Add(currDefender);
            //        }
            //    }
            //}
            List<Area> choices = new List<Area>(activeArea.owner.ownedAreas);
            choices.Remove(activeArea);
            Area moveLocation = choices[Random.Range(0, choices.Count)];
            for (int i = 0; i < charactersToMove.Count; i++) {
                Character currCharacter = charactersToMove[i];
                currCharacter.homeLandmark.RemoveCharacterHomeOnLandmark(currCharacter);
                moveLocation.coreTile.landmarkOnTile.AddCharacterHomeOnLandmark(currCharacter, true);
            }

            LandmarkManager.Instance.UnownArea(activeArea);
            FactionManager.Instance.neutralFaction.AddToOwnedAreas(activeArea);
            OpenMenu();
        }
    }
    #endregion
}
