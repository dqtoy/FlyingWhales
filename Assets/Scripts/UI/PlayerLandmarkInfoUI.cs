using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using ECS;

public class PlayerLandmarkInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 60;

    [Space(10)]
    [Header("Content")]
    [SerializeField]
    private TextMeshProUGUI landmarkNameLbl;
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
    [SerializeField]
    private GameObject landmarkCharacterPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject scrollLeftArrowGO;
    [SerializeField] private GameObject scrollRightArrowGO;
    private List<LandmarkCharacterItem> characterItems;

    [Space(10)]
    [Header("Logs")]
    [SerializeField]
    private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Defenders")]
    [SerializeField]
    private LandmarkCharacterItem[] defenderSlots;

    [Space(10)]
    [Header("Investigation")]
    [SerializeField]
    private GameObject investigationGO;
    [SerializeField] private GameObject minionAssignmentGO;
    [SerializeField] private CharacterPortrait minionAssignmentPortrait;
    [SerializeField] private Button minionAssignmentConfirmButton;
    [SerializeField] private Button minionAssignmentRecallButton;
    [SerializeField] private TextMeshProUGUI minionAssignmentDescription;
    [SerializeField] private InvestigationMinionDraggableItem minionAssignmentDraggableItem;

    [Space(10)]
    [Header("Others")]
    [SerializeField] private TextMeshProUGUI ritualCircleTraitText;

    private LogHistoryItem[] logHistoryItems;


    internal BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    private BaseLandmark _activeLandmark;
    private Minion _assignedMinion;

    internal override void Initialize() {
        base.Initialize();
        characterItems = new List<LandmarkCharacterItem>();
        LoadLogItems();
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        //Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_INSPECTED, OnLandmarkInspected);
        Messenger.AddListener<BaseLandmark, ICharacter>(Signals.LANDMARK_RESIDENT_ADDED, OnResidentAddedToLandmark);
        Messenger.AddListener<BaseLandmark, ICharacter>(Signals.LANDMARK_RESIDENT_REMOVED, OnResidentRemovedFromLandmark);
        Messenger.AddListener<RitualCircle>(Signals.UPDATE_RITUAL_CIRCLE_TRAIT, OnUpdateRitualCircleTrait);
    }
    public override void OpenMenu() {
        base.OpenMenu();
        SetLandmarkBorderState(false);
        BaseLandmark previousLandmark = _activeLandmark;
        _activeLandmark = _data as BaseLandmark;
        //UpdateHiddenUI();
        UpdateLandmarkInfo();
        UpdateCharacters();
        UpdateInvestigation();
        ResetScrollPositions();
        if (_activeLandmark.tileLocation.areaOfTile != null) {
            _activeLandmark.tileLocation.areaOfTile.SetOutlineState(true);
            //if (!_activeLandmark.tileLocation.areaOfTile.isHighlighted) {
            //}
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
    public void UpdateLandmarkInfo() {
        if (_activeLandmark == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateDefenders();
        UpdateAllHistoryInfo();
    }
    //private void UpdateHiddenUI() {
    //    ShowIntelTriggeredUI();
    //}
    //private void ShowIntelTriggeredUI() {
    //    defendersGO.SetActive(true);
    //    charactersGO.SetActive(true);
    //    logsGO.SetActive(true);
    //    for (int i = 0; i < connectorsGO.Length; i++) {
    //        connectorsGO[i].SetActive(true);
    //    }
    //}
    //private void HideIntelTriggeredUI() {
    //    defendersGO.SetActive(false);
    //    charactersGO.SetActive(false);
    //    logsGO.SetActive(false);
    //    for (int i = 0; i < connectorsGO.Length; i++) {
    //        connectorsGO[i].SetActive(false);
    //    }
    //}

    #region Basic Info
    private void UpdateBasicInfo() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_activeLandmark.specificLandmarkType);
        if (_activeLandmark.tileLocation.areaOfTile != null) {
            landmarkNameLbl.text = _activeLandmark.tileLocation.areaOfTile.name;
        } else {
            landmarkNameLbl.text = _activeLandmark.landmarkName;
        }
        if (_activeLandmark.owner != null) {
            landmarkTypeLbl.text = Utilities.GetNormalizedSingularRace(_activeLandmark.owner.raceType) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(_activeLandmark.specificLandmarkType.ToString());
        } else {
            landmarkTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(_activeLandmark.specificLandmarkType.ToString());
        }

        if (_activeLandmark.tileLocation.areaOfTile != null) {
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
        healthProgressBar.fillAmount = _activeLandmark.currDurability / (float) _activeLandmark.totalDurability;
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
        for (int i = 0; i < _activeLandmark.charactersAtLocation.Count; i++) {
            //if (!_activeLandmark.IsDefenderOfLandmark(_activeLandmark.charactersAtLocation[i])) {
                CreateNewCharacterItem(_activeLandmark.charactersAtLocation[i].owner);
            //}
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
    private LandmarkCharacterItem GetItem(ICharacter character) {
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
    private LandmarkCharacterItem CreateNewCharacterItem(ICharacter character) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetCharacter(character, _activeLandmark);
        characterItems.Add(item);
        CheckScrollers();
        return item;
    }
    private void CreateNewCharacterItem(LandmarkPartyData partyData) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetCharacter(partyData.partyMembers[0], _activeLandmark);
    }
    private void OnResidentAddedToLandmark(BaseLandmark landmark, ICharacter character) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) { // && (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)
            CreateNewCharacterItem(character);
        }
    }
    private void OnResidentRemovedFromLandmark(BaseLandmark landmark, ICharacter character) {
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

    #region Defenders
    private void UpdateDefenders() {
        for (int i = 0; i < defenderSlots.Length; i++) {
            LandmarkCharacterItem currSlot = defenderSlots[i];
            currSlot.SetCharacter(null, _activeLandmark, true);
            currSlot.slotItem.dropZone.SetEnabledState(false);
            currSlot.slotItem.draggable.SetDraggable(false);
            //currSlot.slotItem.SetNeededType(typeof(IUnit));
        }
        //if (_activeLandmark.defenders == null) {
        //    for (int i = 0; i < defenderSlots.Length; i++) {
        //        LandmarkCharacterItem currSlot = defenderSlots[i];
        //        currSlot.SetCharacter(null, _activeLandmark, true);
        //        currSlot.slotItem.dropZone.SetEnabledState(false);
        //        currSlot.slotItem.draggable.SetDraggable(false);
        //        //currSlot.slotItem.SetNeededType(typeof(IUnit));
        //    }
        //} else {
        //    for (int i = 0; i < defenderSlots.Length; i++) {
        //        LandmarkCharacterItem currSlot = defenderSlots[i];
        //        ICharacter defender = _activeLandmark.defenders.icharacters.ElementAtOrDefault(i);
        //        currSlot.SetCharacter(defender, _activeLandmark, true);
        //        currSlot.slotItem.dropZone.SetEnabledState(false);
        //        currSlot.slotItem.draggable.SetDraggable(false);
        //        //currSlot.slotItem.SetNeededType(typeof(IUnit));
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
    private void SetLandmarkBorderState(bool state) {
        if (_activeLandmark != null) {
            _activeLandmark.tileLocation.SetBordersState(state);
        }
    }
    #endregion

    #region Investigation
    public void UpdateInvestigation() {
        if (_activeLandmark != null && _activeLandmark.landmarkObj.needsMinionAssignment) {
            if (_activeLandmark.landmarkObj.assignedCharacter != null) {
                AssignMinionToInvestigate(_activeLandmark.landmarkObj.assignedCharacter.minion);
            } else {
                ResetMinionAssignment();
            }
            ShowHideRitualCircleTraitText();
            investigationGO.SetActive(true);
        } else {
            investigationGO.SetActive(false);
        }
    }
    private void ResetMinionAssignment() {
        _assignedMinion = null;
        minionAssignmentPortrait.gameObject.SetActive(false);
        minionAssignmentDescription.gameObject.SetActive(true);
        minionAssignmentConfirmButton.gameObject.SetActive(false);
        minionAssignmentRecallButton.gameObject.SetActive(false);
    }
    public void OnUpdateLandmarkInvestigationState() {
        if (_activeLandmark == null) {
            return;
        }
        if (_activeLandmark.landmarkObj.assignedCharacter != null) {
            minionAssignmentConfirmButton.gameObject.SetActive(true);
            //minionAssignmentConfirmButton.gameObject.SetActive(false);
            //minionAssignmentRecallButton.gameObject.SetActive(true);
            minionAssignmentDraggableItem.SetDraggable(false);
            minionAssignmentConfirmButton.interactable = false;
            //minionAssignmentRecallButton.interactable = true;
        } else {
            if (_assignedMinion != null) {
                minionAssignmentConfirmButton.gameObject.SetActive(true);
                //minionAssignmentConfirmButton.gameObject.SetActive(false);
                //minionAssignmentRecallButton.gameObject.SetActive(true);
                minionAssignmentDraggableItem.SetDraggable(true);
                minionAssignmentConfirmButton.interactable = true;
                //minionAssignmentRecallButton.interactable = false;
            } else {
                ResetMinionAssignment();
            }
        }
    }
    public void OnMinionDrop(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            AssignMinionToInvestigate(minionItem.minion);
        }
    }
    public void AssignMinionToInvestigate(Minion minion) {
        _assignedMinion = minion;
        if (minion != null) {
            minionAssignmentPortrait.gameObject.SetActive(true);
            minionAssignmentPortrait.GeneratePortrait(minion.icharacter);
            minionAssignmentDescription.gameObject.SetActive(false);
            OnUpdateLandmarkInvestigationState();
        } else {
            ResetMinionAssignment();
        }
    }
    public void OnClickConfirmInvestigation() {
        _activeLandmark.landmarkObj.SetAssignedCharacter(_assignedMinion.icharacter as Character);
        OnUpdateLandmarkInvestigationState();
    }
    //public void OnClickRecall() {
    //    _activeLandmark.tileLocation.areaOfTile.areaInvestigation.RecallMinion("explore");
    //}
    private void ShowHideRitualCircleTraitText() {
        if (_activeLandmark.landmarkObj.specificObjectType == LANDMARK_TYPE.RITUAL_CIRCLE) {
            RitualCircle ritualCircle = _activeLandmark.landmarkObj as RitualCircle;
            ritualCircleTraitText.text = ritualCircle.traitForTheDay;
            ritualCircleTraitText.gameObject.SetActive(true);
        } else {
            ritualCircleTraitText.gameObject.SetActive(false);
        }
    }
    private void OnUpdateRitualCircleTrait(RitualCircle ritualCircle) {
        if (_activeLandmark != null && _activeLandmark.landmarkObj == ritualCircle) {
            ritualCircleTraitText.text = ritualCircle.traitForTheDay;
        }
    }
    #endregion
}
