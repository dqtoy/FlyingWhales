using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using ECS;

public class LandmarkInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 60;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI landmarkNameLbl;
    [SerializeField] private TextMeshProUGUI suppliesNameLbl;
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private Slider healthProgressBar;

    [Space(10)]
    [Header("Info")]
    //[SerializeField] private GameObject[] secrets;
    //[SerializeField] private GameObject[] intel;
    //[SerializeField] private GameObject[] encounters;
    [SerializeField] private IntelItem[] intelItems;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject landmarkCharacterPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject scrollLeftArrowGO;
    [SerializeField] private GameObject scrollRightArrowGO;
    private List<LandmarkCharacterItem> characterItems;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Defenders")]
    [SerializeField] private LandmarkCharacterItem[] defenderSlots;

    [Space(10)]
    [Header("Others")]
    [SerializeField] private GameObject[] notInspectedBGs;

    private LogHistoryItem[] logHistoryItems;
    

    internal BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    private BaseLandmark _activeLandmark;

    internal override void Initialize() {
        base.Initialize();
        characterItems = new List<LandmarkCharacterItem>();
        healthProgressBar.minValue = 0f;
        LoadLogItems();
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_INSPECTED, OnLandmarkInspected);
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, OnPartyExitedLandmark);
    }
    public override void OpenMenu() {
        base.OpenMenu();
        _activeLandmark = _data as BaseLandmark;
        UpdateLandmarkInfo();
        UpdateCharacters();
        if (_activeLandmark.specificLandmarkType != LANDMARK_TYPE.DEMONIC_PORTAL) {
            PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(_activeLandmark);
        }
        ResetScrollPositions();
        PlayerUI.Instance.UncollapseMinionHolder();
        InteractionUI.Instance.OpenInteractionUI(_activeLandmark);
    }
    public override void CloseMenu() {
        base.CloseMenu();
        _activeLandmark = null;
        PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        PlayerUI.Instance.CollapseMinionHolder();
        InteractionUI.Instance.HideInteractionUI();
    }

    public void UpdateLandmarkInfo() {
        if (_activeLandmark == null) {
            return;
        }
        UpdateBasicInfo();
        //if (!_activeLandmark.isBeingInspected) {
        //    UpdateBGs(true);
        //} else {
        //    UpdateBGs(false);
        //}
        UpdateInfo();
        //UpdateCharacters();
        UpdateDefenders();
        //UpdateItems();
        UpdateAllHistoryInfo();
    }
    private void UpdateBGs(bool state) {
        for (int i = 0; i < notInspectedBGs.Length; i++) {
            notInspectedBGs[i].SetActive(state);
        }
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_activeLandmark.specificLandmarkType);
        landmarkNameLbl.text = _activeLandmark.landmarkName;
        suppliesNameLbl.text = _activeLandmark.tileLocation.areaOfTile.suppliesInBank.ToString();

        if (_activeLandmark.owner == null) {
            factionEmblem.gameObject.SetActive(false);
        } else {
            factionEmblem.gameObject.SetActive(true);
            factionEmblem.SetFaction(_activeLandmark.owner);
        }
        UpdateHP();
    }
    private void UpdateHP() {
        healthProgressBar.maxValue = _activeLandmark.totalDurability;
        healthProgressBar.value = _activeLandmark.currDurability;
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
        if (obj is BaseLandmark && _activeLandmark != null && (obj as BaseLandmark).id == _activeLandmark.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> landmarkHistory = new List<Log>(_activeLandmark.history.OrderByDescending(x => x.id));
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
    #endregion

    #region Characters
    private void UpdateCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        characterItems.Clear();
        CheckScrollers();
        if (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll) {
            for (int i = 0; i < _activeLandmark.charactersAtLocation.Count; i++) {
                Party currParty = _activeLandmark.charactersAtLocation[i];
                CreateNewCharacterItem(currParty);
            }
        } else {
            for (int i = 0; i < _activeLandmark.lastInspectedOfCharactersAtLocation.Count; i++) {
                LandmarkPartyData partyData = _activeLandmark.lastInspectedOfCharactersAtLocation[i];
                CreateNewCharacterItem(partyData);
            }
        }
    }
    private LandmarkCharacterItem GetItem(Party party) {
        LandmarkCharacterItem[] items = Utilities.GetComponentsInDirectChildren<LandmarkCharacterItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            LandmarkCharacterItem item = items[i];
            if (item.party != null) {
                if (item.party.id == party.id) {
                    return item;
                }
            }
        }
        return null;
    }
    private LandmarkCharacterItem CreateNewCharacterItem(Party party) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetParty(party, _activeLandmark);
        characterItems.Add(item);
        CheckScrollers();
        return item;
    }
    private void CreateNewCharacterItem(LandmarkPartyData partyData) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetParty(partyData.partyMembers[0].currentParty, _activeLandmark);
    }
    private void OnPartyEnteredLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id && (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)) {
            CreateNewCharacterItem(party);
        }
    }
    private void OnPartyExitedLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) {
            LandmarkCharacterItem item = GetItem(party);
            if(item != null) {
                characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item.gameObject);
                CheckScrollers();
            }
        }
    }
    public void ScrollCharactersLeft() {
        charactersScrollView.horizontalNormalizedPosition -= Time.deltaTime;
    }
    public void ScrollCharactersRight() {
        charactersScrollView.horizontalNormalizedPosition += Time.deltaTime;
    }
    private void CheckScrollers() {
        if (characterItems.Count > 5) {
            scrollLeftArrowGO.SetActive(true);
            scrollRightArrowGO.SetActive(true);
        } else {
            scrollLeftArrowGO.SetActive(false);
            scrollRightArrowGO.SetActive(false);
        }
    }
    #endregion

    #region Info
    private void UpdateInfo() {
        List<Intel> intels = new List<Intel>(_activeLandmark.intels);
        intels.AddRange(IntelManager.Instance.GetIntelConcerning(_activeLandmark.charactersAtLocation));
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.Reset();
            Intel currIntel = intels.ElementAtOrDefault(i);
            if (currIntel == null) {
                currItem.gameObject.SetActive(false);
            } else {
                currItem.SetIntel(currIntel);
                currItem.gameObject.SetActive(true);
            }
        }
        //for (int i = 0; i < secrets.Length; i++) {
        //    if(i < _activeLandmark.secrets.Count) {
        //        secrets[i].SetActive(true);
        //    } else {
        //        secrets[i].SetActive(false);
        //    }
        //}
        //for (int i = 0; i < intel.Length; i++) {
        //    if (i < _activeLandmark.intels.Count) {
        //        intel[i].SetActive(true);
        //    } else {
        //        intel[i].SetActive(false);
        //    }
        //}
        //for (int i = 0; i < encounters.Length; i++) {
        //    if (i < _activeLandmark.encounters.Count) {
        //        encounters[i].SetActive(true);
        //    } else {
        //        encounters[i].SetActive(false);
        //    }
        //}
    }
    #endregion

    #region Defenders
    private void UpdateDefenders() {
        for (int i = 0; i < _activeLandmark.defenders.Length; i++) {
            Party defender = _activeLandmark.defenders[i];
            defenderSlots[i].SetParty(defender, _activeLandmark);
        }
    }
    #endregion

    #region Utilities
    public void OnClickCloseBtn() {
        CloseMenu();
    }
    public void CenterOnLandmark() {
        _activeLandmark.CenterOnLandmark();
    }
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        historyScrollView.verticalNormalizedPosition = 1;
    }
    private void OnLandmarkInspected(BaseLandmark landmark) {
        if (_activeLandmark != null && _activeLandmark.id == landmark.id) {
            UpdateCharacters();
        }
    }
    private void OnInspectAll() {
        if (isShowing && _activeLandmark != null) {
            UpdateCharacters();
        }
    }
    #endregion
}
