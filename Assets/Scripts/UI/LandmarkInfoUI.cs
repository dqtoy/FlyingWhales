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
    [Header("Defenders")]
    [SerializeField] private LandmarkCharacterItem[] defenderSlots;

    //[Space(10)]
    //[Header("Others")]
    //[SerializeField] private GameObject[] notInspectedBGs;

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
    private Minion[] _assignedParty;
    private float _currentWinChance;

    internal override void Initialize() {
        base.Initialize();
        characterItems = new List<LandmarkCharacterItem>();
        LoadLogItems();
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        //Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_INSPECTED, OnLandmarkInspected);
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, OnPartyExitedLandmark);
        Messenger.AddListener<BaseLandmark, ICharacter>(Signals.LANDMARK_RESIDENT_ADDED, OnResidentAddedToLandmark);
        Messenger.AddListener<BaseLandmark, ICharacter>(Signals.LANDMARK_RESIDENT_REMOVED, OnResidentRemovedFromLandmark);
        Messenger.AddListener<Intel>(Signals.INTEL_ADDED, OnIntelAdded);
        _assignedParty = new Minion[4];
    }
    public override void OpenMenu() {
        base.OpenMenu();
        SetLandmarkBorderState(false);
        BaseLandmark previousLandmark = _activeLandmark;
        _activeLandmark = _data as BaseLandmark;
        if(previousLandmark == null || (previousLandmark != null && previousLandmark.tileLocation.areaOfTile != _activeLandmark.tileLocation.areaOfTile)) {
            ResetMinionAssignment();
            ResetMinionAssignmentParty();
        }
        UpdateHiddenUI();
        UpdateLandmarkInfo();
        UpdateCharacters();
        //UpdateInvestigation();
        //if (_activeLandmark.specificLandmarkType != LANDMARK_TYPE.DEMONIC_PORTAL) {
        //    PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(_activeLandmark);
        //}
        ResetScrollPositions();
        //PlayerUI.Instance.UncollapseMinionHolder();
        //InteractionUI.Instance.OpenInteractionUI(_activeLandmark);
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
        //PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        //PlayerUI.Instance.CollapseMinionHolder();
        //InteractionUI.Instance.HideInteractionUI();
    }
    public override void SetData(object data) {
        base.SetData(data);
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
        //UpdateInfo();
        //UpdateCharacters();
        UpdateDefenders();
        //UpdateItems();
        UpdateAllHistoryInfo();
    }
    private void UpdateHiddenUI() {
        if (_activeLandmark.tileLocation.areaOfTile.locationIntel.isObtained || GameManager.Instance.inspectAll) {
            ShowIntelTriggeredUI();
        } else {
            HideIntelTriggeredUI();
        }
    }
    private void OnIntelAdded(Intel intel) {
        if(_activeLandmark != null && _activeLandmark.tileLocation.areaOfTile.locationIntel == intel) {
            ShowIntelTriggeredUI();
        }
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
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_activeLandmark.specificLandmarkType);
        if (_activeLandmark.tileLocation.areaOfTile != null) {
            landmarkNameLbl.text = _activeLandmark.tileLocation.areaOfTile.name;
        } else {
            landmarkNameLbl.text = _activeLandmark.landmarkName;
        }
        if (_activeLandmark.owner != null) {
            landmarkTypeLbl.text = Utilities.GetNormalizedSingularRace(_activeLandmark.owner.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(_activeLandmark.specificLandmarkType.ToString());
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
        healthProgressBar.fillAmount = _activeLandmark.currDurability / (float)_activeLandmark.totalDurability;
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

        //if (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll) {
            for (int i = 0; i < _activeLandmark.charactersAtLocation.Count; i++) {
                //Party currParty = _activeLandmark.charactersWithHomeOnLandmark[i].ownParty;
                if (!_activeLandmark.IsDefenderOfLandmark(_activeLandmark.charactersAtLocation[i])) {
                    CreateNewCharacterItem(_activeLandmark.charactersAtLocation[i].owner);
                }
            }
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

    #region Defenders
    private void UpdateDefenders() {
        if (_activeLandmark.defenders == null) {
            for (int i = 0; i < defenderSlots.Length; i++) {
                LandmarkCharacterItem currSlot = defenderSlots[i];
                currSlot.SetCharacter(null, _activeLandmark, true);
                currSlot.slotItem.dropZone.SetEnabledState(false);
                currSlot.slotItem.draggable.SetDraggable(false);
            }
        } else {
            for (int i = 0; i < defenderSlots.Length; i++) {
                LandmarkCharacterItem currSlot = defenderSlots[i];
                ICharacter defender = _activeLandmark.defenders.icharacters.ElementAtOrDefault(i);
                currSlot.SetCharacter(defender, _activeLandmark, true);
                currSlot.slotItem.dropZone.SetEnabledState(false);
                currSlot.slotItem.draggable.SetDraggable(false);
                //defenderSlots[i].portrait.SetForceShowPortraitState(true);
            }
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
            UpdateHiddenUI();
        }
    }
    private void SetLandmarkBorderState(bool state) {
        if (_activeLandmark != null) {
            _activeLandmark.tileLocation.SetBordersState(state);
        }
    }
    #endregion

    #region Investigation
    public void UpdateInvestigation(int indexToggleToBeActivated = 0) {
        if(_activeLandmark != null && _activeLandmark.tileLocation.areaOfTile.areaInvestigation != null) {
            if (!investigationGO.activeSelf) {
                minionAssignmentTween.ResetToBeginning();
                minionAssignmentPartyTween.ResetToBeginning();
            }
            investigateToggles[indexToggleToBeActivated].isOn = false;
            investigateToggles[indexToggleToBeActivated].isOn = true;
            investigationGO.SetActive(true);
        } else {
            investigationGO.SetActive(false);
        }
    }
    public void SetCurentSelectedInvestigateButton(InvestigateButton investigateButton) {
        _currentSelectedInvestigateButton = investigateButton;
        if(_currentSelectedInvestigateButton != null) {
            if(_currentSelectedInvestigateButton.actionName == "explore") {
                ShowMinionAssignment();
            } else {
                BaseLandmark chosenToAttackLandmark = _activeLandmark.tileLocation.areaOfTile.GetFirstAliveExposedTile();
                if(chosenToAttackLandmark != _activeLandmark) {
                    UIManager.Instance.ShowLandmarkInfo(chosenToAttackLandmark, 1);
                }
                ShowMinionAssignmentParty();
            }
        } else {
            HideMinionAssignment();
            HideMinionAssignmentParty();
        }
    }
    private void ResetMinionAssignment() {
        _assignedMinion = null;
        minionAssignmentPortrait.gameObject.SetActive(false);
        minionAssignmentDescription.gameObject.SetActive(true);
        minionAssignmentConfirmButton.gameObject.SetActive(false);
        minionAssignmentRecallButton.gameObject.SetActive(false);
    }
    private void ResetMinionAssignmentParty() {
        for (int i = 0; i < minionAssignmentPartyPortraits.Length; i++) {
            _assignedParty[i] = null;
            minionAssignmentPartyPortraits[i].gameObject.SetActive(false);
        }
        minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
        minionAssignmentPartyRecallButton.gameObject.SetActive(false);
        _currentWinChance = 0f;
        minionAssignmentPartyWinChance.text = _currentWinChance.ToString("F2") + "%";
    }
    private void ResetMinionAssignmentParty(int index) {
        _assignedParty[index] = null;
        minionAssignmentPartyPortraits[index].gameObject.SetActive(false);
        minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
        minionAssignmentPartyRecallButton.gameObject.SetActive(false);
        for (int i = 0; i < _assignedParty.Length; i++) {
            if(_assignedParty[i] != null) {
                OnUpdateLandmarkInvestigationState("attack");
                break;
            }
        }

    }
    public void ShowMinionAssignment() {
        if (_activeLandmark.tileLocation.areaOfTile.areaInvestigation.isExploring) {
            AssignMinionToInvestigate(_activeLandmark.tileLocation.areaOfTile.areaInvestigation.assignedMinion);
        } else {
            AssignMinionToInvestigate(_assignedMinion);
        }

        minionAssignmentTween.PlayForward();
        HideMinionAssignmentParty();
    }
    public void HideMinionAssignment() {
        //ResetMinionAssignment();
        //minionAssignmentTween.ResetToBeginning();
        minionAssignmentTween.PlayReverse();
    }
    public void ShowMinionAssignmentParty() {
        if (_activeLandmark.tileLocation.areaOfTile.areaInvestigation.isAttacking) {
            for (int i = 0; i < _assignedParty.Length; i++) {
                if (i < _activeLandmark.tileLocation.areaOfTile.areaInvestigation.assignedMinionAttack.icharacter.currentParty.icharacters.Count) {
                    AssignPartyMinionToInvestigate(_activeLandmark.tileLocation.areaOfTile.areaInvestigation.assignedMinionAttack.icharacter.currentParty.icharacters[i].minion, i, false);
                } else {
                    AssignPartyMinionToInvestigate(null, i, false);
                }
            }
        } else {
            for (int i = 0; i < _assignedParty.Length; i++) {
                AssignPartyMinionToInvestigate(_assignedParty[i], i, false);
            }
        }
        minionAssignmentPartyTween.PlayForward();
        HideMinionAssignment();
    }
    public void HideMinionAssignmentParty() {
        //ResetMinionAssignmentParty();
        minionAssignmentPartyTween.PlayReverse();
    }
    public void OnUpdateLandmarkInvestigationState(string whatToDo) {
        if(_activeLandmark == null) {
            return;
        }
        if(whatToDo == "explore") {
            if (_activeLandmark.tileLocation.areaOfTile.areaInvestigation.isExploring) {
                minionAssignmentConfirmButton.gameObject.SetActive(false);
                minionAssignmentRecallButton.gameObject.SetActive(true);
                minionAssignmentDraggableItem.SetDraggable(false);
                minionAssignmentConfirmButton.interactable = false;
                minionAssignmentRecallButton.interactable = true;
            } else {
                if (_activeLandmark.tileLocation.areaOfTile.areaInvestigation.isMinionRecalledExplore) {
                    minionAssignmentConfirmButton.gameObject.SetActive(false);
                    minionAssignmentRecallButton.gameObject.SetActive(true);
                    minionAssignmentDraggableItem.SetDraggable(false);
                    minionAssignmentConfirmButton.interactable = true;
                    minionAssignmentRecallButton.interactable = false;
                } else {
                    minionAssignmentConfirmButton.gameObject.SetActive(true);
                    minionAssignmentRecallButton.gameObject.SetActive(false);
                    minionAssignmentDraggableItem.SetDraggable(true);
                    minionAssignmentConfirmButton.interactable = true;
                    minionAssignmentRecallButton.interactable = false;
                }
            }
        } else {
            if (_activeLandmark.tileLocation.areaOfTile.areaInvestigation.isAttacking) {
                minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
                minionAssignmentPartyRecallButton.gameObject.SetActive(true);
                minionAssignmentPartyConfirmButton.interactable = false;
                minionAssignmentPartyRecallButton.interactable = true;
                for (int i = 0; i < minionAssignmentPartyDraggableItem.Length; i++) {
                    minionAssignmentPartyDraggableItem[i].SetDraggable(false);
                }
            } else {
                if (_activeLandmark.tileLocation.areaOfTile.areaInvestigation.isMinionRecalledAttack) {
                    minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
                    minionAssignmentPartyRecallButton.gameObject.SetActive(true);
                    minionAssignmentPartyConfirmButton.interactable = true;
                    minionAssignmentPartyRecallButton.interactable = false;
                    for (int i = 0; i < minionAssignmentPartyDraggableItem.Length; i++) {
                        minionAssignmentPartyDraggableItem[i].SetDraggable(false);
                    }
                } else {
                    minionAssignmentPartyConfirmButton.gameObject.SetActive(true);
                    minionAssignmentPartyRecallButton.gameObject.SetActive(false);
                    minionAssignmentPartyConfirmButton.interactable = true;
                    minionAssignmentPartyRecallButton.interactable = false;
                    for (int i = 0; i < minionAssignmentPartyDraggableItem.Length; i++) {
                        minionAssignmentPartyDraggableItem[i].SetDraggable(true);
                    }
                }
            }
        }
    }
    public void OnMinionDrop(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
        if(minionItem != null) {
            for (int i = 0; i < _assignedParty.Length; i++) {
                if(_assignedParty[i] == minionItem.minion) {
                    AssignPartyMinionToInvestigate(null, i, false);
                    break;
                }
            }
            AssignMinionToInvestigate(minionItem.minion);
        }
    }
    public void OnPartyMinionDrop1(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            if(_assignedMinion == minionItem.minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minionItem.minion, 0);
        }
    }
    public void OnPartyMinionDrop2(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            if (_assignedMinion == minionItem.minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minionItem.minion, 1);
        }
    }
    public void OnPartyMinionDrop3(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            if (_assignedMinion == minionItem.minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minionItem.minion, 2);
        }
    }
    public void OnPartyMinionDrop4(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            if (_assignedMinion == minionItem.minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minionItem.minion, 3);
        }
    }
    public void AssignMinionToInvestigate(Minion minion) {
        _assignedMinion = minion;
        if (minion != null) {
            minionAssignmentPortrait.gameObject.SetActive(true);
            minionAssignmentPortrait.GeneratePortrait(minion.icharacter);
            //minionAssignmentConfirmButton.interactable = !_activeLandmark.landmarkInvestigation.isExploring;
            minionAssignmentDescription.gameObject.SetActive(false);

            OnUpdateLandmarkInvestigationState("explore");
            //minionAssignmentRecallButton.gameObject.SetActive(false);
            //minionAssignmentConfirmButton.gameObject.SetActive(true);
            //minionAssignmentDraggableItem.SetDraggable(true);
        } else {
            ResetMinionAssignment();
        }
    }
    public void AssignPartyMinionToInvestigate(Minion minion, int index, bool checkDuplicate = true) {
        if (minion != null) {
            if (checkDuplicate) {
                for (int i = 0; i < _assignedParty.Length; i++) {
                    if (_assignedParty[i] != null && _assignedParty[i] == minion) {
                        return;
                    }
                }
            }
            _assignedParty[index] = minion;
            minionAssignmentPartyPortraits[index].gameObject.SetActive(true);
            minionAssignmentPartyPortraits[index].GeneratePortrait(minion.icharacter, 85);
            //minionAssignmentPartyRecallButton.gameObject.SetActive(false);
            //minionAssignmentPartyConfirmButton.gameObject.SetActive(true);
            //minionAssignmentDraggableItem.SetDraggable(true);
            minionAssignmentConfirmButton.interactable = !_activeLandmark.tileLocation.areaOfTile.areaInvestigation.isAttacking;
            OnUpdateLandmarkInvestigationState("attack");
        } else {
            ResetMinionAssignmentParty(index);
        }

        List<ICharacter> assignedCharacters = new List<ICharacter>();
        for (int i = 0; i < _assignedParty.Length; i++) {
            if (_assignedParty[i] != null) {
                assignedCharacters.Add(_assignedParty[i].icharacter);
            }
        }

        float chance = 0f;
        float enemyChance = 0f;
        if (_activeLandmark.defenders != null) {
            CombatManager.Instance.GetCombatChanceOfTwoLists(assignedCharacters, _activeLandmark.defenders.icharacters, out chance, out enemyChance);
        } else {
            CombatManager.Instance.GetCombatChanceOfTwoLists(assignedCharacters, null, out chance, out enemyChance);
        }
        SetWinChance(chance);
    }
    private void SetWinChance(float chance) {
        iTween.ValueTo(this.gameObject, iTween.Hash("from", _currentWinChance, "to", chance, "time", 0.3f, "onupdate", "OnUpdateWinChance"));
    }
    private void OnUpdateWinChance(float value) {
        _currentWinChance = value;
        minionAssignmentPartyWinChance.text = _currentWinChance.ToString("F2") + "%";
    }
    public void OnClickConfirmInvestigation() {
        _activeLandmark.tileLocation.areaOfTile.areaInvestigation.InvestigateLandmark(_assignedMinion);
        OnUpdateLandmarkInvestigationState("explore");
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickRecall() {
        _activeLandmark.tileLocation.areaOfTile.areaInvestigation.RecallMinion("explore");
        //OnUpdateLandmarkInvestigationState();
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickConfirmPartyInvestigation() {
        _activeLandmark.tileLocation.areaOfTile.areaInvestigation.AttackRaidLandmark(_currentSelectedInvestigateButton.actionName, _assignedParty, _activeLandmark);
        OnUpdateLandmarkInvestigationState("attack");
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickPartyRecall() {
        _activeLandmark.tileLocation.areaOfTile.areaInvestigation.RecallMinion("attack");
        //OnUpdateLandmarkInvestigationState();
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    #endregion
}
