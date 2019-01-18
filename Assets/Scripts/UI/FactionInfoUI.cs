using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

using UnityEngine.UI.Extensions;
using EZObjectPools;

public class FactionInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 60;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private TextMeshProUGUI factionTypeLbl;
    [SerializeField] private GameObject charactersGO;
    [SerializeField] private GameObject logsGO;
    [SerializeField] private FactionEmblem emblem;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject landmarkCharacterPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private RectTransform leaderEmblem;
    private List<LandmarkCharacterItem> characterItems;

    [Space(10)]
    [Header("Areas")]
    [SerializeField] private ScrollRect areasScrollView;
    [SerializeField] private GameObject locationPortraitPrefab;
    private List<LocationPortrait> locationItems;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private Toggle logsToggle;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Relationships")]
    [SerializeField] private RectTransform relationshipsParent;
    [SerializeField] private GameObject relationshipPrefab;

    private LogHistoryItem[] logHistoryItems;

    internal Faction currentlyShowingFaction {
        get { return _data as Faction; }
    }

    public Faction activeFaction { get; private set; }

    internal override void Initialize() {
        base.Initialize();
        characterItems = new List<LandmarkCharacterItem>();
        locationItems = new List<LocationPortrait>();
        LoadLogItems();
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Faction, Area>(Signals.FACTION_OWNED_AREA_ADDED, OnFactionAreaAdded);
        Messenger.AddListener<Faction, Area>(Signals.FACTION_OWNED_AREA_REMOVED, OnFactionAreaRemoved);
        Messenger.AddListener<FactionRelationship>(Signals.FACTION_RELATIONSHIP_CHANGED, OnFactionRelationshipChanged);
        Messenger.AddListener<Faction>(Signals.FACTION_ACTIVE_CHANGED, OnFactionActiveChanged);
    }

    public override void OpenMenu() {
        Faction previousArea = activeFaction;
        activeFaction = _data as Faction;
        base.OpenMenu();
        UpdateFactionInfo();
        UpdateAllCharacters();
        UpdateAreas();
        UpdateAllRelationships();
        ResetScrollPositions();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        activeFaction = null;
    }

    public void UpdateFactionInfo() {
        if (activeFaction == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateAllHistoryInfo();
        //ResetScrollPositions();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        factionNameLbl.text = activeFaction.name;
        factionTypeLbl.text = Utilities.GetNormalizedSingularRace(activeFaction.raceType) + " Faction";
        emblem.SetFaction(activeFaction);
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
        if (obj is Area && activeFaction != null && (obj as Area).id == activeFaction.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> landmarkHistory = new List<Log>(activeFaction.history.OrderByDescending(x => x.id));
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
    private void UpdateAllCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        characterItems.Clear();

        for (int i = 0; i < activeFaction.characters.Count; i++) {
            Character currCharacter = activeFaction.characters[i];
            CreateNewCharacterItem(currCharacter);
        }
    }
    private LandmarkCharacterItem GetItem(Party party) {
        LandmarkCharacterItem[] items = Utilities.GetComponentsInDirectChildren<LandmarkCharacterItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            LandmarkCharacterItem item = items[i];
            if (item.character != null) {
                if (item.character.ownParty.id == party.id) {
                    return item;
                }
            }
        }
        return null;
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
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetCharacter(character, this);
        characterItems.Add(item);
        OrderCharacterItems();
        return item;
    }
    private void CreateNewCharacterItem(LandmarkPartyData partyData) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetCharacter(partyData.partyMembers[0], this);
        characterItems.Add(item);
        OrderCharacterItems();
    }
    private void OrderCharacterItems() {
        if (activeFaction.leader is Character) {
            LandmarkCharacterItem leaderItem = GetItem(activeFaction.leader as Character);
            leaderItem.transform.SetAsFirstSibling();
            leaderEmblem.gameObject.SetActive(true);
            leaderEmblem.SetParent(leaderItem.transform);
            leaderEmblem.anchoredPosition = new Vector2(-18.3f, -55f);
        } else {
            leaderEmblem.gameObject.SetActive(false);
        }
    }
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        if (isShowing && activeFaction.id == faction.id) {
            CreateNewCharacterItem(character);
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        if (isShowing && activeFaction != null && activeFaction.id == faction.id) {
            LandmarkCharacterItem item = GetItem(character);
            if (item != null) {
                characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item.gameObject);
                OrderCharacterItems();
            }
        }
    }
    #endregion

    #region Areas
    private void UpdateAreas() {
        Utilities.DestroyChildren(areasScrollView.content);
        locationItems.Clear();

        for (int i = 0; i < activeFaction.ownedAreas.Count; i++) {
            Area currArea = activeFaction.ownedAreas[i];
            CreateNewAreaItem(currArea);
        }
    }
    private void CreateNewAreaItem(Area area) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(locationPortraitPrefab.name, areasScrollView.content);
        LocationPortrait item = characterGO.GetComponent<LocationPortrait>();
        item.SetLocation(area);
        locationItems.Add(item);
    }
    private LocationPortrait GetLocationItem(Area area) {
        for (int i = 0; i < locationItems.Count; i++) {
            LocationPortrait locationPortrait = locationItems[i];
            if (locationPortrait.area.id == area.id) {
                return locationPortrait;
            }
        }
        return null;
    }
    private void DestroyLocationItem(Area area) {
        LocationPortrait item = GetLocationItem(area);
        if (item != null) {
            locationItems.Remove(item);
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        }
    }
    private void OnFactionAreaAdded(Faction faction, Area area) {
        if (isShowing && activeFaction.id == faction.id) {
            CreateNewAreaItem(area);
        }
    }
    private void OnFactionAreaRemoved(Faction faction, Area area) {
        if (isShowing && activeFaction.id == faction.id) {
            DestroyLocationItem(area);
        }
    }
    #endregion

    #region Relationships
    private void UpdateAllRelationships() {
        Utilities.DestroyChildren(relationshipsParent);

        foreach (KeyValuePair<Faction, FactionRelationship> keyValuePair in activeFaction.relationships) {
            if (keyValuePair.Key.isActive) {
                GameObject relGO = UIManager.Instance.InstantiateUIObject(relationshipPrefab.name, relationshipsParent);
                FactionRelationshipItem item = relGO.GetComponent<FactionRelationshipItem>();
                item.SetData(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
    private void OnFactionRelationshipChanged(FactionRelationship rel) {
        if (isShowing && (rel.faction1.id == activeFaction.id || rel.faction2.id == activeFaction.id)) {
            UpdateAllRelationships();
        }
    }
    private void OnFactionActiveChanged(Faction faction) {
        if (isShowing) {
            UpdateAllRelationships();
        }
    }
    #endregion

    #region Utilities
    public void OnClickCloseBtn() {
        CloseMenu();
    }
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        historyScrollView.verticalNormalizedPosition = 1;
        areasScrollView.verticalNormalizedPosition = 1;
    }
    private void OnInspectAll() {
        if (isShowing && activeFaction != null) {
            UpdateAllCharacters();
            //UpdateHiddenUI();
        }
    }
    #endregion
}
