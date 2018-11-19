﻿using UnityEngine;
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
    [SerializeField] private GameObject minionAssignmentPartyGO;
    [SerializeField] private CharacterPortrait minionAssignmentPortrait;
    [SerializeField] private CharacterPortrait[] minionAssignmentPartyPortraits;

    [SerializeField] private Button minionAssignmentConfirmButton;
    [SerializeField] private Button minionAssignmentRecallButton;
    [SerializeField] private Button minionAssignmentPartyConfirmButton;
    [SerializeField] private Button minionAssignmentPartyRecallButton;
    [SerializeField] private TextMeshProUGUI minionAssignmentDescription;
    [SerializeField] private TextMeshProUGUI minionAssignmentPartyWinChance;
    [SerializeField] private TweenPosition minionAssignmentTween;
    [SerializeField] private TweenPosition minionAssignmentPartyTween;
    [SerializeField] private InvestigationMinionDraggableItem minionAssignmentDraggableItem;
    [SerializeField] private InvestigationMinionDraggableItem[] minionAssignmentPartyDraggableItem;
    [SerializeField] private Toggle[] investigateToggles;
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
        Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_INSPECTED, OnLandmarkInspected);
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
        _activeLandmark = _data as BaseLandmark;
        UpdateHiddenUI();
        UpdateLandmarkInfo();
        UpdateCharacters();
        UpdateInvestigation();
        //if (_activeLandmark.specificLandmarkType != LANDMARK_TYPE.DEMONIC_PORTAL) {
        //    PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(_activeLandmark);
        //}
        ResetScrollPositions();
        //PlayerUI.Instance.UncollapseMinionHolder();
        //InteractionUI.Instance.OpenInteractionUI(_activeLandmark);
        _activeLandmark.tileLocation.SetBordersState(true);

    }
    public override void CloseMenu() {
        base.CloseMenu();
        if (_activeLandmark != null) {
            _activeLandmark.tileLocation.SetBordersState(false);
        }
        _activeLandmark = null;
        //PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        //PlayerUI.Instance.CollapseMinionHolder();
        //InteractionUI.Instance.HideInteractionUI();
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
    //private void UpdateBGs(bool state) {
    //    for (int i = 0; i < notInspectedBGs.Length; i++) {
    //        notInspectedBGs[i].SetActive(state);
    //    }
    //}
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
        landmarkTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(_activeLandmark.specificLandmarkType.ToString());
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
        item.SetParty(character, _activeLandmark);
        characterItems.Add(item);
        CheckScrollers();
        return item;
    }
    private void CreateNewCharacterItem(LandmarkPartyData partyData) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetParty(partyData.partyMembers[0], _activeLandmark);
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
                defenderSlots[i].SetParty(null, _activeLandmark, true);
            }
        } else {
            for (int i = 0; i < defenderSlots.Length; i++) {
                ICharacter defender = _activeLandmark.defenders.icharacters.ElementAtOrDefault(i);
                defenderSlots[i].SetParty(defender, _activeLandmark, true);
                defenderSlots[i].portrait.SetForceShowPortraitState(true);
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
    #endregion

    #region Investigation
    public void UpdateInvestigation() {
        if(_activeLandmark != null && _activeLandmark.landmarkInvestigation != null) {
            if (!investigationGO.activeSelf) {
                minionAssignmentTween.ResetToBeginning();
                minionAssignmentPartyTween.ResetToBeginning();
            }
            if (_activeLandmark.landmarkInvestigation.isActivated) {
                if (_activeLandmark.landmarkInvestigation.whatToDo == "explore") {
                    minionAssignmentGO.transform.localPosition = minionAssignmentTween.to;
                    minionAssignmentPartyGO.transform.localPosition = minionAssignmentPartyTween.from;
                    AssignMinionToInvestigate(_activeLandmark.landmarkInvestigation.assignedMinion);
                } else {
                    minionAssignmentGO.transform.localPosition = minionAssignmentTween.from;
                    minionAssignmentPartyGO.transform.localPosition = minionAssignmentPartyTween.to;
                    for (int i = 0; i < _assignedParty.Length; i++) {
                        if(i < _activeLandmark.landmarkInvestigation.assignedMinion.icharacter.currentParty.icharacters.Count) {
                            AssignPartyMinionToInvestigate(_activeLandmark.landmarkInvestigation.assignedMinion.icharacter.currentParty.icharacters[i].minion, i, false);
                        } else {
                            AssignPartyMinionToInvestigate(null, i, false);
                        }
                    }
                }
                for (int i = 0; i < investigateToggles.Length; i++) {
                    if (_activeLandmark.landmarkInvestigation.whatToDo == investigateToggles[i].GetComponent<InvestigateButton>().actionName) {
                        investigateToggles[i].isOn = true;
                    } else {
                        investigateToggles[i].isOn = false;
                    }
                }
                OnUpdateLandmarkInvestigationState();
            } else {
                minionAssignmentGO.transform.localPosition = minionAssignmentTween.from;
                minionAssignmentPartyGO.transform.localPosition = minionAssignmentPartyTween.from;
                //HideMinionAssignment();
                ResetMinionAssignment();
                ResetMinionAssignmentParty();

                for (int i = 0; i < investigateToggles.Length; i++) {
                    investigateToggles[i].isOn = false;
                }
            }
            ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
            investigationGO.SetActive(true);
        } else {
            investigationGO.SetActive(false);
        }
    }
    private void ChangeStateAllButtons(bool state) {
        //if (minionAssignmentConfirmButton.gameObject.activeSelf) {
        //    minionAssignmentConfirmButton.interactable = state;
        //}
        //if (minionAssignmentRecallButton.gameObject.activeSelf) {
        //    minionAssignmentRecallButton.interactable = state;
        //}
        for (int i = 0; i < investigateToggles.Length; i++) {
            investigateToggles[i].interactable = state;
        }
    }
    public void SetCurentSelectedInvestigateButton(InvestigateButton investigateButton) {
        _currentSelectedInvestigateButton = investigateButton;
        if(_currentSelectedInvestigateButton != null) {
            if(_currentSelectedInvestigateButton.actionName == "explore") {
                ShowMinionAssignment();
            } else {
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
        minionAssignmentPartyPortraits[index].gameObject.SetActive(false);
        minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
        minionAssignmentPartyRecallButton.gameObject.SetActive(false);
        for (int i = 0; i < _assignedParty.Length; i++) {
            if(_assignedParty[i] != null) {
                OnUpdateLandmarkInvestigationState();
            }
        }

    }
    public void ShowMinionAssignment() {
        if (_activeLandmark.landmarkInvestigation.isActivated) {
            AssignMinionToInvestigate(_activeLandmark.landmarkInvestigation.assignedMinion);
        } else {
            ResetMinionAssignment();
        }
        //minionAssignmentTween.ResetToBeginning();
        minionAssignmentTween.PlayForward();
        HideMinionAssignmentParty();
    }
    public void HideMinionAssignment() {
        ResetMinionAssignment();
        //minionAssignmentTween.ResetToBeginning();
        minionAssignmentTween.PlayReverse();
    }
    public void ShowMinionAssignmentParty() {
        if (_activeLandmark.landmarkInvestigation.isActivated) {
            AssignMinionToInvestigate(_activeLandmark.landmarkInvestigation.assignedMinion);
        } else {
            ResetMinionAssignmentParty();
        }
        minionAssignmentPartyTween.PlayForward();
        HideMinionAssignment();
    }
    public void HideMinionAssignmentParty() {
        ResetMinionAssignmentParty();
        minionAssignmentPartyTween.PlayReverse();
    }
    private void OnSetLandmarkInvestigationState(BaseLandmark landmark) {
        if(_activeLandmark == landmark) {
            OnUpdateLandmarkInvestigationState();
        }
    }
    public void OnUpdateLandmarkInvestigationState() {
        if (_activeLandmark.landmarkInvestigation.isActivated) {
            minionAssignmentConfirmButton.gameObject.SetActive(false);
            minionAssignmentRecallButton.gameObject.SetActive(true);
            minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
            minionAssignmentPartyRecallButton.gameObject.SetActive(true);
            minionAssignmentDraggableItem.SetDraggable(false);
            for (int i = 0; i < minionAssignmentPartyDraggableItem.Length; i++) {
                minionAssignmentPartyDraggableItem[i].SetDraggable(false);
            }
            //minionAssignmentRecallButton.interactable = !_activeLandmark.landmarkInvestigation.isMinionRecalled;
        } else {
            minionAssignmentConfirmButton.gameObject.SetActive(true);
            minionAssignmentRecallButton.gameObject.SetActive(false);
            minionAssignmentPartyConfirmButton.gameObject.SetActive(true);
            minionAssignmentPartyRecallButton.gameObject.SetActive(false);
            minionAssignmentDraggableItem.SetDraggable(true);
            for (int i = 0; i < minionAssignmentPartyDraggableItem.Length; i++) {
                minionAssignmentPartyDraggableItem[i].SetDraggable(true);
            }
        }
    }
    public void OnMinionDrop(Transform transform) {
        PlayerCharacterItem minionItem = transform.GetComponent<PlayerCharacterItem>();
        if(minionItem != null) {
            AssignMinionToInvestigate(minionItem.minion);
        }
    }
    public void OnPartyMinionDrop1(Transform transform) {
        PlayerCharacterItem minionItem = transform.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            AssignPartyMinionToInvestigate(minionItem.minion, 0);
        }
    }
    public void OnPartyMinionDrop2(Transform transform) {
        PlayerCharacterItem minionItem = transform.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            AssignPartyMinionToInvestigate(minionItem.minion, 1);
        }
    }
    public void OnPartyMinionDrop3(Transform transform) {
        PlayerCharacterItem minionItem = transform.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            AssignPartyMinionToInvestigate(minionItem.minion, 2);
        }
    }
    public void OnPartyMinionDrop4(Transform transform) {
        PlayerCharacterItem minionItem = transform.GetComponent<PlayerCharacterItem>();
        if (minionItem != null) {
            AssignPartyMinionToInvestigate(minionItem.minion, 3);
        }
    }
    public void AssignMinionToInvestigate(Minion minion) {
        _assignedMinion = minion;
        if (minion != null) {
            minionAssignmentPortrait.gameObject.SetActive(true);
            minionAssignmentPortrait.GeneratePortrait(minion.icharacter);
            minionAssignmentDescription.gameObject.SetActive(false);
            minionAssignmentRecallButton.gameObject.SetActive(false);
            minionAssignmentConfirmButton.gameObject.SetActive(true);
            minionAssignmentDraggableItem.SetDraggable(true);
            minionAssignmentConfirmButton.interactable = !_activeLandmark.landmarkInvestigation.isActivated;
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
            minionAssignmentPartyRecallButton.gameObject.SetActive(false);
            minionAssignmentPartyConfirmButton.gameObject.SetActive(true);
            minionAssignmentDraggableItem.SetDraggable(true);
            minionAssignmentConfirmButton.interactable = !_activeLandmark.landmarkInvestigation.isActivated;
        } else {
            _assignedParty[index] = null;
            ResetMinionAssignmentParty(index);
        }

        List<ICharacter> assignedCharacters = new List<ICharacter>();
        for (int i = 0; i < _assignedParty.Length; i++) {
            if(_assignedParty[i] != null) {
                assignedCharacters.Add(_assignedParty[i].icharacter);
            }
        }
        
        float chance = 0f;
        float enemyChance = 0f;
        if(_activeLandmark.defenders != null) {
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
        _activeLandmark.landmarkInvestigation.InvestigateLandmark(_currentSelectedInvestigateButton.actionName, _assignedMinion);
        OnUpdateLandmarkInvestigationState();
        ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickRecall() {
        _activeLandmark.landmarkInvestigation.RecallMinion();
        //OnUpdateLandmarkInvestigationState();
        ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickConfirmPartyInvestigation() {
        _activeLandmark.landmarkInvestigation.InvestigateLandmark(_currentSelectedInvestigateButton.actionName, _assignedParty);
        OnUpdateLandmarkInvestigationState();
        ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickPartyRecall() {
        _activeLandmark.landmarkInvestigation.RecallMinion();
        //OnUpdateLandmarkInvestigationState();
        ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    #endregion
}
