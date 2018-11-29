using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using ECS;
using UnityEngine.UI.Extensions;

public class AreaInfoUI : UIMenu {

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
    [SerializeField] private GameObject defenderGroupPrefab;
    [SerializeField] private ScrollRect defendersScrollView;
    [SerializeField] private HorizontalScrollSnap defendersScrollSnap;

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
    
    internal Area currentlyShowingLandmark {
        get { return _data as Area; }
    }

    private Area _activeArea;
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
        //Messenger.AddListener<BaseLandmark, ICharacter>(Signals.LANDMARK_RESIDENT_ADDED, OnResidentAddedToLandmark);
        //Messenger.AddListener<BaseLandmark, ICharacter>(Signals.LANDMARK_RESIDENT_REMOVED, OnResidentRemovedFromLandmark);
        Messenger.AddListener<Intel>(Signals.INTEL_ADDED, OnIntelAdded);
        _assignedParty = new Minion[4];
    }
    public override void OpenMenu() {
        base.OpenMenu();
        Area previousArea = _activeArea;
        _activeArea = _data as Area;
        if(previousArea == null || (previousArea != null && previousArea != _activeArea)) {
            ResetMinionAssignment();
            ResetMinionAssignmentParty();
        }
        UpdateHiddenUI();
        UpdateDefenders();
        UpdateLandmarkInfo();
        UpdateCharacters();
        ResetScrollPositions();
        if(previousArea != null) {
            previousArea.SetOutlineState(false);
        }
        if (_activeArea != null) {
            _activeArea.SetOutlineState(true);
        }
    }
    public override void CloseMenu() {
        base.CloseMenu();
        GameObject[] objects;
        defendersScrollSnap.RemoveAllChildren(out objects);
        //Utilities.DestroyChildren(defendersScrollView.content);
        if (_activeArea != null) {
            _activeArea.SetOutlineState(false);
        }
        _activeArea = null;
        //PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        //PlayerUI.Instance.CollapseMinionHolder();
        //InteractionUI.Instance.HideInteractionUI();
    }
    public override void SetData(object data) {
        base.SetData(data);
    }

    public void UpdateLandmarkInfo() {
        if (_activeArea == null) {
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
        //UpdateDefenders();
        //UpdateItems();
        UpdateAllHistoryInfo();
    }
    private void UpdateHiddenUI() {
        if (_activeArea.locationIntel.isObtained || GameManager.Instance.inspectAll) {
            ShowLocationIntelUI();
        } else {
            HideLocationIntelUI();
        }
        //Add checker if defender intel is obtained
        ShowDefenderIntelUI();
    }
    private void OnIntelAdded(Intel intel) {
        if(_activeArea != null) {
            if (_activeArea.locationIntel == intel) {
                ShowLocationIntelUI();
            }
            //Add checker if defender intel is obtained
            ShowDefenderIntelUI();
        }

    }
    private void ShowLocationIntelUI() {
        charactersGO.SetActive(true);
        logsGO.SetActive(true);
        connectorsGO[1].SetActive(true);
        connectorsGO[2].SetActive(true);
    }
    private void HideLocationIntelUI() {
        charactersGO.SetActive(false);
        logsGO.SetActive(false);
        connectorsGO[1].SetActive(false);
        connectorsGO[2].SetActive(false);
    }
    private void ShowDefenderIntelUI() {
        defendersGO.SetActive(true);
        connectorsGO[0].SetActive(true);
    }
    private void HideDefenderIntelUI() {
        defendersGO.SetActive(false);
        connectorsGO[0].SetActive(false);
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_activeArea.coreTile.landmarkOnTile.specificLandmarkType);
        landmarkNameLbl.text = _activeArea.name;
        if (_activeArea.owner != null) {
            landmarkTypeLbl.text = Utilities.GetNormalizedSingularRace(_activeArea.owner.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(_activeArea.coreTile.landmarkOnTile.specificLandmarkType.ToString());
        } else {
            landmarkTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(_activeArea.coreTile.landmarkOnTile.specificLandmarkType.ToString());
        }

        suppliesNameLbl.text = _activeArea.suppliesInBank.ToString();


        if (_activeArea.owner == null) {
            factionEmblem.gameObject.SetActive(false);
        } else {
            factionEmblem.gameObject.SetActive(true);
            factionEmblem.SetFaction(_activeArea.owner);
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
        if (obj is BaseLandmark && _activeArea != null && (obj as BaseLandmark).tileLocation.areaOfTile.id == _activeArea.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> landmarkHistory = new List<Log>(_activeArea.history.OrderByDescending(x => x.id));
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

        for (int i = 0; i < _activeArea.areaResidents.Count; i++) {
            CreateNewCharacterItem(_activeArea.areaResidents[i]);
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
        item.SetCharacter(character, null);
        characterItems.Add(item);
        CheckScrollers();
        return item;
    }
    private void CreateNewCharacterItem(LandmarkPartyData partyData) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetCharacter(partyData.partyMembers[0], null);
    }
    private void OnPartyEnteredLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && _activeArea != null && _activeArea.id == landmark.tileLocation.areaOfTile.id) { //&& (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)
            CreateNewCharacterItem(party.owner);
        }
    }
    private void OnPartyExitedLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && _activeArea != null && _activeArea.id == landmark.tileLocation.areaOfTile.id) {
            LandmarkCharacterItem item = GetItem(party);
            if(item != null) {
                characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item.gameObject);
                CheckScrollers();
            }
        }
    }
    private void OnResidentAddedToLandmark(BaseLandmark landmark, ICharacter character) {
        if (isShowing && _activeArea != null && _activeArea.id == landmark.tileLocation.areaOfTile.id) { // && (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)
            CreateNewCharacterItem(character);
        }
    }
    private void OnResidentRemovedFromLandmark(BaseLandmark landmark, ICharacter character) {
        if (isShowing && _activeArea != null && _activeArea.id == landmark.tileLocation.areaOfTile.id) {
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
        for (int i = 0; i < _activeArea.defenderGroups.Count; i++) {
            DefenderGroup currGroup = _activeArea.defenderGroups[i];
            GameObject currGO = UIManager.Instance.InstantiateUIObject(defenderGroupPrefab.name, defendersScrollView.content);
            currGO.GetComponent<DefenderGroupItem>().SetDefender(currGroup);
        }
        defendersScrollSnap.InitialiseChildObjectsFromScene();
    }
    #endregion

    #region Utilities
    public void OnClickCloseBtn() {
        CloseMenu();
    }
    public void CenterOnCoreLandmark() {
        _activeArea.CenterOnCoreLandmark();
    }
    private void ResetScrollPositions() {
        charactersScrollView.verticalNormalizedPosition = 1;
        historyScrollView.verticalNormalizedPosition = 1;
    }
    private void OnInspectAll() {
        if (isShowing && _activeArea != null) {
            UpdateCharacters();
            UpdateHiddenUI();
        }
    }
    #endregion

    #region Investigation
    public void UpdateInvestigation(int indexToggleToBeActivated = 0) {
        if(_activeArea != null && _activeArea.areaInvestigation != null) {
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
                //BaseLandmark chosenToAttackLandmark = _activeArea.GetFirstAliveExposedTile();
                //if(chosenToAttackLandmark != _activeArea) {
                //    UIManager.Instance.ShowLandmarkInfo(chosenToAttackLandmark, 1);
                //}
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
        if (_activeArea.areaInvestigation.isExploring) {
            AssignMinionToInvestigate(_activeArea.areaInvestigation.assignedMinion);
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
        if (_activeArea.areaInvestigation.isAttacking) {
            for (int i = 0; i < _assignedParty.Length; i++) {
                if (i < _activeArea.areaInvestigation.assignedMinionAttack.icharacter.currentParty.icharacters.Count) {
                    AssignPartyMinionToInvestigate(_activeArea.areaInvestigation.assignedMinionAttack.icharacter.currentParty.icharacters[i].minion, i, false);
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
        if(_activeArea == null) {
            return;
        }
        if(whatToDo == "explore") {
            if (_activeArea.areaInvestigation.isExploring) {
                minionAssignmentConfirmButton.gameObject.SetActive(false);
                minionAssignmentRecallButton.gameObject.SetActive(true);
                minionAssignmentDraggableItem.SetDraggable(false);
                minionAssignmentConfirmButton.interactable = false;
                minionAssignmentRecallButton.interactable = true;
            } else {
                if (_activeArea.areaInvestigation.isMinionRecalledExplore) {
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
            if (_activeArea.areaInvestigation.isAttacking) {
                minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
                minionAssignmentPartyRecallButton.gameObject.SetActive(true);
                minionAssignmentPartyConfirmButton.interactable = false;
                minionAssignmentPartyRecallButton.interactable = true;
                for (int i = 0; i < minionAssignmentPartyDraggableItem.Length; i++) {
                    minionAssignmentPartyDraggableItem[i].SetDraggable(false);
                }
            } else {
                if (_activeArea.areaInvestigation.isMinionRecalledAttack) {
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
        PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
        if (minionItem != null) {
            if(_assignedMinion == minionItem.minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minionItem.minion, 0);
        }
    }
    public void OnPartyMinionDrop2(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
        if (minionItem != null) {
            if (_assignedMinion == minionItem.minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minionItem.minion, 1);
        }
    }
    public void OnPartyMinionDrop3(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
        if (minionItem != null) {
            if (_assignedMinion == minionItem.minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minionItem.minion, 2);
        }
    }
    public void OnPartyMinionDrop4(GameObject go) {
        PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
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
            minionAssignmentConfirmButton.interactable = !_activeArea.areaInvestigation.isAttacking;
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
        DefenderGroup defender = _activeArea.GetFirstDefenderGroup();
        if (defender != null) {
            CombatManager.Instance.GetCombatChanceOfTwoLists(assignedCharacters, defender.party.icharacters, out chance, out enemyChance);
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
        _activeArea.areaInvestigation.InvestigateLandmark(_assignedMinion);
        OnUpdateLandmarkInvestigationState("explore");
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickRecall() {
        _activeArea.areaInvestigation.RecallMinion("explore");
        //OnUpdateLandmarkInvestigationState();
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickConfirmPartyInvestigation() {
        _activeArea.areaInvestigation.AttackRaidLandmark(_currentSelectedInvestigateButton.actionName, _assignedParty, _activeArea.coreTile.landmarkOnTile);
        OnUpdateLandmarkInvestigationState("attack");
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickPartyRecall() {
        _activeArea.areaInvestigation.RecallMinion("attack");
        //OnUpdateLandmarkInvestigationState();
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    #endregion
}
