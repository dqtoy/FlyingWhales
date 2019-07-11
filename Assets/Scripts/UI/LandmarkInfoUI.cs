using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

using UnityEngine.UI.Extensions;

public class LandmarkInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 60;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI landmarkNameLbl;
    [SerializeField] private TextMeshProUGUI landmarkTypeLbl;
    [SerializeField] private TextMeshProUGUI suppliesNameLbl;
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private Image healthProgressBar;
    [SerializeField] private GameObject defendersGO;
    [SerializeField] private GameObject charactersGO;
    [SerializeField] private GameObject logsGO;
    [SerializeField] private GameObject[] connectorsGO;

    //[Space(10)]
    //[Header("Info")]
    ////[SerializeField] private GameObject[] secrets;
    ////[SerializeField] private GameObject[] intel;
    ////[SerializeField] private GameObject[] encounters;
    //[SerializeField] private IntelItem[] intelItems;

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
    [Header("Investigation")]
    [SerializeField] private GameObject investigationGO;
    [SerializeField] private GameObject minionAssignmentGO;
    [SerializeField] private CharacterPortrait minionAssignmentPortrait;
    [SerializeField] private Button minionAssignmentConfirmButton;
    [SerializeField] private Button minionAssignmentRecallButton;
    [SerializeField] private TextMeshProUGUI minionAssignmentDescription;
    [SerializeField] private TweenPosition minionAssignmentTween;
    [SerializeField] private InvestigationMinionDraggableItem minionAssignmentDraggableItem;
    [SerializeField] private Toggle[] investigateToggles;

    [Space(10)]
    [Header("Attack/Raid")]
    [SerializeField] private GameObject minionAssignmentPartyGO;
    [SerializeField] private CharacterPortrait[] minionAssignmentPartyPortraits;
    [SerializeField] private Button minionAssignmentPartyConfirmButton;
    [SerializeField] private Button minionAssignmentPartyRecallButton;
    [SerializeField] private TextMeshProUGUI minionAssignmentPartyWinChance;
    [SerializeField] private TweenPosition minionAssignmentPartyTween;
    [SerializeField] private InvestigationMinionDraggableItem[] minionAssignmentPartyDraggableItem;


    private InvestigateButton _currentSelectedInvestigateButton;

    private LogHistoryItem[] logHistoryItems;
    
    internal BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    private BaseLandmark _activeLandmark;
    private Minion _assignedMinion;
    //private Minion[] _assignedParty;
    private float _currentWinChance;

    internal override void Initialize() {
        base.Initialize();
        characterItems = new List<LandmarkCharacterItem>();
        LoadLogItems();
        //Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
    }
    public override void OpenMenu() {
        base.OpenMenu();
        SetLandmarkBorderState(false);
        BaseLandmark previousLandmark = _activeLandmark;
        _activeLandmark = _data as BaseLandmark;
        UpdateLandmarkInfo();
        UpdateCharacters();
        ResetScrollPositions();
        if(previousLandmark != null) {
            if (previousLandmark.tileLocation.areaOfTile != null) {
                previousLandmark.tileLocation.areaOfTile.SetOutlineState(false);
            } else {
                SetLandmarkBorderState(false);
            }
        }
        if(_activeLandmark.tileLocation.areaOfTile != null) {
            _activeLandmark.tileLocation.areaOfTile.SetOutlineState(true);
        } else {
            SetLandmarkBorderState(true);
        }
    }
    public override void CloseMenu() {
        base.CloseMenu();
        SetLandmarkBorderState(false);
        if (_activeLandmark.tileLocation.areaOfTile != null) {
            _activeLandmark.tileLocation.areaOfTile.SetOutlineState(false);
        } else {
            SetLandmarkBorderState(false);
        }
        _activeLandmark = null;
    }
    public override void SetData(object data) {
        base.SetData(data);
    }

    public void UpdateLandmarkInfo() {
        if (_activeLandmark == null) {
            return;
        }
        UpdateBasicInfo();
        //UpdateAllHistoryInfo();
    }
    private void ShowIntelTriggeredUI() {
        defendersGO.SetActive(true);
        charactersGO.SetActive(true);
        logsGO.SetActive(true);
        for (int i = 0; i < connectorsGO.Length; i++) {
            connectorsGO[i].SetActive(true);
        }
    }
    private void HideIntelTriggeredUI() {
        defendersGO.SetActive(false);
        charactersGO.SetActive(false);
        logsGO.SetActive(false);
        for (int i = 0; i < connectorsGO.Length; i++) {
            connectorsGO[i].SetActive(false);
        }
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        //LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_activeLandmark.specificLandmarkType);
        if (_activeLandmark.tileLocation.areaOfTile != null) {
            landmarkNameLbl.text = _activeLandmark.tileLocation.areaOfTile.name;
        } else {
            landmarkNameLbl.text = _activeLandmark.landmarkName;
        }
        if (_activeLandmark.owner != null) {
            landmarkTypeLbl.text = Utilities.GetNormalizedRaceAdjective(_activeLandmark.owner.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(_activeLandmark.specificLandmarkType.ToString());
        } else {
            landmarkTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(_activeLandmark.specificLandmarkType.ToString());
        }
        
        if(_activeLandmark.tileLocation.areaOfTile != null) {
            suppliesNameLbl.text = _activeLandmark.tileLocation.areaOfTile.suppliesInBank.ToString();
        } else {
            suppliesNameLbl.text = "0";
        }

        if (_activeLandmark.owner == null) {
            factionEmblem.gameObject.SetActive(false);
        } else {
            factionEmblem.gameObject.SetActive(true);
            factionEmblem.SetFaction(_activeLandmark.owner);
        }
        UpdateHP();
    }
    private void UpdateHP() {
        //healthProgressBar.fillAmount = _activeLandmark.currDurability / (float)_activeLandmark.totalDurability;
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
    //    if (obj is BaseLandmark && _activeLandmark != null && (obj as BaseLandmark).id == _activeLandmark.id) {
    //        UpdateAllHistoryInfo();
    //    }
    //}
    //private void UpdateAllHistoryInfo() {
    //    List<Log> landmarkHistory = new List<Log>(_activeLandmark.history.OrderByDescending(x => x.id));
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
    #endregion

    #region Characters
    private void UpdateCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        characterItems.Clear();
        CheckScrollers();

        //if (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll) {
            //for (int i = 0; i < _activeLandmark.charactersAtLocation.Count; i++) {
                //Party currParty = _activeLandmark.charactersWithHomeOnLandmark[i].ownParty;
                //if (!_activeLandmark.IsDefenderOfLandmark(_activeLandmark.charactersAtLocation[i])) {
                    //CreateNewCharacterItem(_activeLandmark.charactersAtLocation[i].owner);
                //}
            //}
        //}
        //else {
        //    for (int i = 0; i < _activeLandmark.lastInspectedOfCharactersAtLocation.Count; i++) {
        //        LandmarkPartyData partyData = _activeLandmark.lastInspectedOfCharactersAtLocation[i];
        //        CreateNewCharacterItem(partyData);
        //    }
        //}
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
        item.SetCharacter(character,this);
        characterItems.Add(item);
        CheckScrollers();
        return item;
    }
    private void OnPartyEnteredLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) { //&& (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)
            CreateNewCharacterItem(party.owner);
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
    private void OnResidentAddedToLandmark(BaseLandmark landmark, Character character) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) { // && (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)
            CreateNewCharacterItem(character);
        }
    }
    private void OnResidentRemovedFromLandmark(BaseLandmark landmark, Character character) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) {
            LandmarkCharacterItem item = GetItem(character);
            if (item != null) {
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
        //List<Intel> intels = new List<Intel>(_activeLandmark.intels);
        //intels.AddRange(IntelManager.Instance.GetIntelConcerning(_activeLandmark.charactersAtLocation));
        //for (int i = 0; i < intelItems.Length; i++) {
        //    IntelItem currItem = intelItems[i];
        //    currItem.Reset();
        //    Intel currIntel = intels.ElementAtOrDefault(i);
        //    if (currIntel == null) {
        //        currItem.gameObject.SetActive(false);
        //    } else {
        //        currItem.SetIntel(currIntel);
        //        currItem.gameObject.SetActive(true);
        //    }
        //}
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
    private void SetLandmarkBorderState(bool state) {
        if (_activeLandmark != null) {
            _activeLandmark.tileLocation.SetBordersState(state);
        }
    }
    #endregion
}
