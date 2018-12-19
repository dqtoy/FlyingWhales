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
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private Image healthProgressBar;
    [SerializeField] private Image areaCenterImage;
    [SerializeField] private GameObject defendersGO;
    [SerializeField] private GameObject charactersGO;
    [SerializeField] private GameObject logsGO;
    [SerializeField] private GameObject[] connectorsGO;
    [SerializeField] private Sprite[] areaCenterSprites;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private Toggle charactersMenuToggle;
    [SerializeField] private GameObject landmarkCharacterPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    private List<LandmarkCharacterItem> characterItems;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private Toggle logsMenuToggle;
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
    [SerializeField] private TextMeshProUGUI defenderPageLbl;

    [Space(10)]
    [Header("Investigation")]
    [SerializeField] private GameObject investigationGO;
    [SerializeField] private GameObject minionAssignmentGO;
    //[SerializeField] private CharacterPortrait minionAssignmentPortrait;
    [SerializeField] private Button minionAssignmentConfirmButton;
    [SerializeField] private Button minionAssignmentRecallButton;
    [SerializeField] private TextMeshProUGUI minionAssignmentDescription;
    //[SerializeField] private InvestigationMinionDraggableItem minionAssignmentDraggableItem;
    [SerializeField] private Toggle[] investigateToggles;
    [SerializeField] private EnvelopContentUnityUI minionAssignmentDescriptionEnvelop;
    [SerializeField] private SlotItem minionAssignmentSlot;
    [SerializeField] private SlotItem supportingTokenSlot;
    [SerializeField] private Button consumeSuppotingTokenBtn;

    [Space(10)]
    [Header("Token Collector")]
    [SerializeField] private GameObject tokenCollectorGO;
    //[SerializeField] private CharacterPortrait tokenCollectorPortrait;
    [SerializeField] private Button tokenCollectorConfirmBtn;
    [SerializeField] private Button tokenCollectorRecallBtn;
    //[SerializeField] private InvestigationMinionDraggableItem tokenCollectorDraggableItem;
    [SerializeField] private SlotItem tokenCollectorSlot;

    [Space(10)]
    [Header("Attack/Raid")]
    [SerializeField] private GameObject minionAssignmentPartyGO;
    [SerializeField] private SlotItem[] minionAssignmentPartySlots;
    [SerializeField] private Button minionAssignmentPartyConfirmButton;
    [SerializeField] private Button minionAssignmentPartyRecallButton;
    [SerializeField] private TextMeshProUGUI minionAssignmentPartyWinChance;
    //[SerializeField] private InvestigationMinionDraggableItem[] minionAssignmentPartyDraggableItem;


    private InvestigateButton _currentSelectedInvestigateButton;

    private LogHistoryItem[] logHistoryItems;
    
    internal Area currentlyShowingLandmark {
        get { return _data as Area; }
    }

    public Area activeArea { get; private set; }
    private Minion _assignedMinion;
    private Minion[] _assignedParty;
    private float _currentWinChance;
    private Minion _assignedTokenCollectorMinion;

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
        //Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnTokenAdded);
        Messenger.AddListener<Area>(Signals.AREA_TOKEN_COLLECTION_CHANGED, OnTokenCollectionStateChanged);
        Messenger.AddListener<Minion, Area>(Signals.MINION_STARTS_INVESTIGATING_AREA, OnMinionInvestigateArea);
        Messenger.AddListener<Area>(Signals.AREA_SUPPLIES_CHANGED, OnAreaSuppliesSet);
        _assignedParty = new Minion[4];

        //Minion Investigator slot
        minionAssignmentSlot.SetNeededType(typeof(Minion));
        minionAssignmentSlot.SetOtherValidation(IsObjectValidForInvestigation);
        minionAssignmentSlot.SetItemDroppedCallback(OnMinionDrop);
        minionAssignmentSlot.SetItemDroppedOutCallback(OnInvestigatorMinionDraggedOut);

        //attack/raid
        for (int i = 0; i < minionAssignmentPartySlots.Length; i++) {
            SlotItem currSlot = minionAssignmentPartySlots[i];
            currSlot.SetNeededType(typeof(Minion));
            currSlot.SetSlotIndex(i);
            currSlot.SetItemDroppedCallback(OnPartyMinionDrop);
            currSlot.SetItemDroppedOutCallback(OnPartyMinionDroppedOut);
        }

        //Token Collector Slot
        tokenCollectorSlot.SetNeededType(typeof(Minion));
        tokenCollectorSlot.SetOtherValidation(IsObjectValidForTokenCollector);
        tokenCollectorSlot.SetItemDroppedCallback(OnTokenCollectorMinionDrop);
        tokenCollectorSlot.SetItemDroppedOutCallback(OnTokenCollectorDragOut);

        //Supporting Token Slot
        supportingTokenSlot.SetNeededType(typeof(Token));
        supportingTokenSlot.SetOtherValidation(IsObjectValidForSupportToken);
        supportingTokenSlot.SetItemDroppedCallback(OnSupportTokenDropped);
        supportingTokenSlot.SetItemDroppedOutCallback(OnSupportTokenDraggedOut);
    }

    #region Slot Checkers
    private bool IsObjectValidForTokenCollector(object obj) {
        if (obj is Minion) {
            Minion minion = obj as Minion;
            return activeArea.areaInvestigation.CanCollectTokensHere(minion);
        }
        return false;
    }
    private bool IsObjectValidForInvestigation(object obj) {
        if (obj is Minion) {
            Minion minion = obj as Minion;
            return minion.character.job.jobType != JOB.EXPLORER && minion.character.job.jobType != JOB.SPY;
        }
        return false;
    }
    private bool IsObjectValidForSupportToken(object obj) {
        if (obj is Token && activeArea.areaInvestigation.assignedMinion != null) {
            Token token = obj as Token;
            return activeArea.areaInvestigation.assignedMinion.character.job.CanTokenBeAttached(token);
        }
        return false;
    }
    #endregion

    public override void OpenMenu() {
        base.OpenMenu();
        Area previousArea = activeArea;
        activeArea = _data as Area;
        if(previousArea == null || (previousArea != null && previousArea != activeArea)) {
            ResetMinionAssignment();
            ResetMinionAssignmentParty();
        }
        UpdateHiddenUI();
        UpdateDefenders();
        UpdateAreaInfo();
        UpdateCharacters();
        ResetScrollPositions();
        UpdateTokenCollectorData();
        LoadSupportTokenData();
        if (previousArea != null) {
            previousArea.SetOutlineState(false);
        }
        if (activeArea != null) {
            activeArea.SetOutlineState(true);
        }
        UIManager.Instance.SetCoverState(true);
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
    }
    public override void CloseMenu() {
        base.CloseMenu();
        //GameObject[] objects;
        //defendersScrollSnap.RemoveAllChildren(out objects);
        //for (int i = 0; i < objects.Length; i++) {
        //    ObjectPoolManager.Instance.DestroyObject(objects[i]);
        //}
        //Utilities.DestroyChildren(defendersScrollView.content);
        if (activeArea != null) {
            activeArea.SetOutlineState(false);
        }
        activeArea = null;
        UIManager.Instance.SetCoverState(false);
        UIManager.Instance.Unpause();
        UIManager.Instance.SetSpeedTogglesState(true);
        //PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        //PlayerUI.Instance.CollapseMinionHolder();
        //InteractionUI.Instance.HideInteractionUI();
    }
    public override void SetData(object data) {
        base.SetData(data);
    }

    public void UpdateAreaInfo() {
        if (activeArea == null) {
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
        if (activeArea.areaInvestigation.isActivelyCollectingToken || GameManager.Instance.inspectAll) {
            ShowLocationTokenUI();
            //ShowDefenderTokenUI();
        } else {
            HideLocationTokenUI();
            //HideDefenderTokenUI();
        }
        //if (_activeArea.defenderIntel.isObtained || GameManager.Instance.inspectAll) {
        //    ShowDefenderIntelUI();
        //} else {
        //    HideDefenderIntelUI();
        //}
        
    }
    //private void OnTokenAdded(Token token) {
    //    if(activeArea != null) {
    //        if (activeArea.locationToken == token) {
    //            ShowLocationTokenUI();
    //        } else if (activeArea.defenderToken == token) {
    //            ShowDefenderTokenUI();
    //        }
    //    }
    //}
    private void OnTokenCollectionStateChanged(Area area) {
        if (this.isShowing && activeArea.id == area.id) {
            UpdateHiddenUI();
        }
    }
    private void ShowLocationTokenUI() {
        //charactersGO.SetActive(true);
        //logsGO.SetActive(true);
        charactersMenuToggle.interactable = true;
        logsMenuToggle.interactable = true;
        //connectorsGO[1].SetActive(true);
        //connectorsGO[2].SetActive(true);
    }
    private void HideLocationTokenUI() {
        charactersGO.SetActive(false);
        logsGO.SetActive(false);
        charactersMenuToggle.isOn = false;
        logsMenuToggle.isOn = false;
        charactersMenuToggle.interactable = false;
        logsMenuToggle.interactable = false;
        //connectorsGO[1].SetActive(false);
        //connectorsGO[2].SetActive(false);
    }
    private void ShowDefenderTokenUI() {
        defendersGO.SetActive(true);
        //connectorsGO[0].SetActive(true);
    }
    private void HideDefenderTokenUI() {
        defendersGO.SetActive(false);
        //connectorsGO[0].SetActive(false);
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(activeArea.coreTile.landmarkOnTile.specificLandmarkType);
        landmarkNameLbl.text = activeArea.name;
        if (activeArea.race.race != RACE.NONE) {
            if (activeArea.tiles.Count > 1) {
                landmarkTypeLbl.text = Utilities.GetNormalizedSingularRace(activeArea.race.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(activeArea.GetBaseAreaType().ToString());
            } else {
                landmarkTypeLbl.text = Utilities.GetNormalizedSingularRace(activeArea.race.race) + " " + Utilities.NormalizeStringUpperCaseFirstLetters(activeArea.coreTile.landmarkOnTile.specificLandmarkType.ToString());
            }
            
        } else {
            landmarkTypeLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(activeArea.coreTile.landmarkOnTile.specificLandmarkType.ToString());
        }
        areaCenterImage.sprite = GetAreaCenterSprite(activeArea.name);
        UpdateSupplies();


        if (activeArea.owner == null) {
            factionEmblem.gameObject.SetActive(false);
        } else {
            factionEmblem.gameObject.SetActive(true);
            factionEmblem.SetFaction(activeArea.owner);
        }
    }
    private void OnAreaSuppliesSet(Area area) {
        if (this.isShowing && activeArea.id == area.id) {
            UpdateSupplies();
        }
    }
    private void UpdateSupplies() {
        suppliesNameLbl.text = activeArea.suppliesInBank.ToString();
    }
    private Sprite GetAreaCenterSprite(string name) {
        for (int i = 0; i < areaCenterSprites.Length; i++) {
            if(areaCenterSprites[i].name.ToLower() == name.ToLower()) {
                return areaCenterSprites[i];
            }
        }
        return null;
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
        //CheckScrollers();

        for (int i = 0; i < activeArea.charactersAtLocation.Count; i++) {
            CreateNewCharacterItem(activeArea.charactersAtLocation[i]);
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
        item.SetCharacter(character, null);
        //item.slotItem.draggable.SetDraggable(false);
        //item.slotItem.dropZone.SetEnabledState(false);
        characterItems.Add(item);
        //CheckScrollers();
        return item;
    }
    private void CreateNewCharacterItem(LandmarkPartyData partyData) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetCharacter(partyData.partyMembers[0], null);
    }
    private void OnPartyEnteredLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && activeArea != null && activeArea.id == landmark.tileLocation.areaOfTile.id) { //&& (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)
            CreateNewCharacterItem(party.owner);
        }
    }
    private void OnPartyExitedLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && activeArea != null && activeArea.id == landmark.tileLocation.areaOfTile.id) {
            LandmarkCharacterItem item = GetItem(party);
            if(item != null) {
                characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item.gameObject);
                //CheckScrollers();
            }
        }
    }
    private void OnResidentAddedToLandmark(BaseLandmark landmark, Character character) {
        if (isShowing && activeArea != null && activeArea.id == landmark.tileLocation.areaOfTile.id) { // && (_activeLandmark.isBeingInspected || GameManager.Instance.inspectAll)
            CreateNewCharacterItem(character);
        }
    }
    private void OnResidentRemovedFromLandmark(BaseLandmark landmark, Character character) {
        if (isShowing && activeArea != null && activeArea.id == landmark.tileLocation.areaOfTile.id) {
            LandmarkCharacterItem item = GetItem(character);
            if (item != null) {
                characterItems.Remove(item);
                ObjectPoolManager.Instance.DestroyObject(item.gameObject);
                //CheckScrollers();
            }
        }
    }
    public void ScrollCharactersLeft() {
        charactersScrollView.horizontalNormalizedPosition -= Time.deltaTime;
    }
    public void ScrollCharactersRight() {
        charactersScrollView.horizontalNormalizedPosition += Time.deltaTime;
    }
    #endregion

    #region Defenders
    private void UpdateDefenders() {
        defendersScrollSnap.enabled = false;
        Utilities.DestroyChildren(defendersScrollView.content);
        defendersScrollSnap.ChildObjects = new GameObject[0];
        for (int i = 0; i < activeArea.defenderGroups.Count; i++) {
            DefenderGroup currGroup = activeArea.defenderGroups[i];
            GameObject currGO = UIManager.Instance.InstantiateUIObject(defenderGroupPrefab.name, defendersScrollView.content);
            currGO.GetComponent<DefenderGroupItem>().SetDefender(currGroup);
        }
        //defendersScrollSnap.InitialiseChildObjectsFromScene();
        defendersScrollSnap.enabled = true;
        ShowDefenderTokenUI();
    }
    public void UpdateDefenderPage(int newPage) {
        defenderPageLbl.text = (newPage + 1).ToString() + "/" + defendersScrollSnap.ChildObjects.Length.ToString();
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
            UpdateHiddenUI();
        }
    }
    #endregion

    #region Investigation
    public void UpdateInvestigation(int indexToggleToBeActivated = 0) {
        if(activeArea != null && activeArea.areaInvestigation != null) {
            //if (!investigationGO.activeSelf) {
            //    minionAssignmentTween.ResetToBeginning();
            //    minionAssignmentPartyTween.ResetToBeginning();
            //}
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
    public void ResetMinionAssignment() {
        _assignedMinion = null;
        minionAssignmentSlot.ClearSlot(true);
        //minionAssignmentPortrait.gameObject.SetActive(false);
        //minionAssignmentDescription.gameObject.SetActive(true);
        minionAssignmentSlot.dropZone.SetEnabledState(true);
        minionAssignmentConfirmButton.gameObject.SetActive(false);
        minionAssignmentRecallButton.gameObject.SetActive(false);
        UpdateMinionTooltipDescription(_assignedMinion);
    }
    public void ResetMinionAssignmentParty() {
        for (int i = 0; i < minionAssignmentPartySlots.Length; i++) {
            _assignedParty[i] = null;
            //minionAssignmentPartySlots[i].gameObject.SetActive(false);
            minionAssignmentPartySlots[i].ClearSlot(true);
        }
        minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
        minionAssignmentPartyRecallButton.gameObject.SetActive(false);
        _currentWinChance = 0f;
        minionAssignmentPartyWinChance.text = _currentWinChance.ToString("F2") + "%";
    }
    private void ResetMinionAssignmentParty(int index) {
        _assignedParty[index] = null;
        minionAssignmentPartySlots[index].ClearSlot(true);
        //minionAssignmentPartySlots[index].gameObject.SetActive(false);
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
        if (activeArea.areaInvestigation.isExploring) {
            AssignMinionToInvestigate(activeArea.areaInvestigation.assignedMinion);
        } else {
            AssignMinionToInvestigate(_assignedMinion);
        }

        //minionAssignmentTween.PlayForward();
        minionAssignmentGO.SetActive(true);
        HideMinionAssignmentParty();
    }
    public void HideMinionAssignment() {
        //ResetMinionAssignment();
        //minionAssignmentTween.ResetToBeginning();
        //minionAssignmentTween.PlayReverse();
        minionAssignmentGO.SetActive(false);
    }
    public void ShowMinionAssignmentParty() {
        if (activeArea.areaInvestigation.isAttacking) {
            for (int i = 0; i < _assignedParty.Length; i++) {
                if (i < activeArea.areaInvestigation.assignedMinionAttack.character.currentParty.characters.Count) {
                    AssignPartyMinionToInvestigate(activeArea.areaInvestigation.assignedMinionAttack.character.currentParty.characters[i].minion, i, false);
                } else {
                    AssignPartyMinionToInvestigate(null, i, false);
                }
            }
        } else {
            for (int i = 0; i < _assignedParty.Length; i++) {
                AssignPartyMinionToInvestigate(_assignedParty[i], i, false);
            }
        }
        //minionAssignmentPartyTween.PlayForward();
        minionAssignmentPartyGO.SetActive(true);
        HideMinionAssignment();
    }
    public void HideMinionAssignmentParty() {
        //ResetMinionAssignmentParty();
        //minionAssignmentPartyTween.PlayReverse();
        minionAssignmentPartyGO.SetActive(false);
    }
    public void OnUpdateLandmarkInvestigationState(string whatToDo) {
        if(activeArea == null) {
            return;
        }
        if(whatToDo == "explore") {
            if (activeArea.areaInvestigation.isExploring) {
                minionAssignmentConfirmButton.gameObject.SetActive(false);
                minionAssignmentRecallButton.gameObject.SetActive(true);
                minionAssignmentSlot.draggable.SetDraggable(false);
                minionAssignmentSlot.dropZone.SetEnabledState(false);
                //minionAssignmentDraggableItem.SetDraggable(false);
                minionAssignmentConfirmButton.interactable = false;
                minionAssignmentRecallButton.interactable = true;
            } else {
                if (activeArea.areaInvestigation.isMinionRecalledExplore) {
                    minionAssignmentConfirmButton.gameObject.SetActive(false);
                    minionAssignmentRecallButton.gameObject.SetActive(true);
                    minionAssignmentSlot.draggable.SetDraggable(false);
                    minionAssignmentSlot.dropZone.SetEnabledState(false);
                    //minionAssignmentDraggableItem.SetDraggable(false);
                    minionAssignmentConfirmButton.interactable = true;
                    minionAssignmentRecallButton.interactable = false;
                } else {
                    minionAssignmentConfirmButton.gameObject.SetActive(true);
                    minionAssignmentRecallButton.gameObject.SetActive(false);
                    minionAssignmentSlot.draggable.SetDraggable(true);
                    //minionAssignmentDraggableItem.SetDraggable(true);
                    minionAssignmentConfirmButton.interactable = true;
                    minionAssignmentRecallButton.interactable = false;
                }
            }
        } else {
            if (activeArea.areaInvestigation.isAttacking) {
                minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
                minionAssignmentPartyRecallButton.gameObject.SetActive(true);
                minionAssignmentPartyConfirmButton.interactable = false;
                minionAssignmentPartyRecallButton.interactable = true;
                for (int i = 0; i < minionAssignmentPartySlots.Length; i++) {
                    minionAssignmentPartySlots[i].draggable.SetDraggable(false);
                }
            } else {
                if (activeArea.areaInvestigation.isMinionRecalledAttack) {
                    minionAssignmentPartyConfirmButton.gameObject.SetActive(false);
                    minionAssignmentPartyRecallButton.gameObject.SetActive(true);
                    minionAssignmentPartyConfirmButton.interactable = true;
                    minionAssignmentPartyRecallButton.interactable = false;
                    for (int i = 0; i < minionAssignmentPartySlots.Length; i++) {
                        minionAssignmentPartySlots[i].draggable.SetDraggable(false);
                    }
                } else {
                    minionAssignmentPartyConfirmButton.gameObject.SetActive(true);
                    minionAssignmentPartyRecallButton.gameObject.SetActive(false);
                    minionAssignmentPartyConfirmButton.interactable = true;
                    minionAssignmentPartyRecallButton.interactable = false;
                    for (int i = 0; i < minionAssignmentPartySlots.Length; i++) {
                        minionAssignmentPartySlots[i].draggable.SetDraggable(true);
                    }
                }
            }
        }
    }
    public void OnMinionDrop(object obj, int index) {
        //PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
        //if(minionItem != null) {
        if (obj is Minion) {
            Minion minion = obj as Minion;
            for (int i = 0; i < _assignedParty.Length; i++) {
                if (_assignedParty[i] == minion) {
                    AssignPartyMinionToInvestigate(null, i, false);
                    break;
                }
            }
            AssignMinionToInvestigate(minion);
        }
        
        //}
    }
    public void OnPartyMinionDrop(object obj, int index) {
        //PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
        if (obj is Minion) {
            Minion minion = obj as Minion;
            if(_assignedMinion == minion) {
                AssignMinionToInvestigate(null);
            }
            AssignPartyMinionToInvestigate(minion, index);
        }
    }
    public void OnPartyMinionDroppedOut(object obj, int index) {
        AssignPartyMinionToInvestigate(null, index);
    }
    //public void OnPartyMinionDrop2(object obj, int index) {
    //    PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
    //    if (minionItem != null) {
    //        if (_assignedMinion == minionItem.minion) {
    //            AssignMinionToInvestigate(null);
    //        }
    //        AssignPartyMinionToInvestigate(minionItem.minion, 1);
    //    }
    //}
    //public void OnPartyMinionDrop3(object obj, int index) {
    //    PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
    //    if (minionItem != null) {
    //        if (_assignedMinion == minionItem.minion) {
    //            AssignMinionToInvestigate(null);
    //        }
    //        AssignPartyMinionToInvestigate(minionItem.minion, 2);
    //    }
    //}
    //public void OnPartyMinionDrop4(object obj, int index) {
    //    PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
    //    if (minionItem != null) {
    //        if (_assignedMinion == minionItem.minion) {
    //            AssignMinionToInvestigate(null);
    //        }
    //        AssignPartyMinionToInvestigate(minionItem.minion, 3);
    //    }
    //}
    public void AssignMinionToInvestigate(Minion minion) {
        _assignedMinion = minion;
        if (minion != null) {
            //minionAssignmentPortrait.gameObject.SetActive(true);
            //minionAssignmentPortrait.GeneratePortrait(minion.character);
            minionAssignmentSlot.PlaceObject(minion);
            //minionAssignmentConfirmButton.interactable = !_activeLandmark.landmarkInvestigation.isExploring;
            //minionAssignmentDescription.gameObject.SetActive(false);
            OnUpdateLandmarkInvestigationState("explore");
            UpdateMinionTooltipDescription(minion);
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
            minionAssignmentPartySlots[index].PlaceObject(minion);
            //minionAssignmentPartySlots[index].gameObject.SetActive(true);
            //minionAssignmentPartySlots[index].GeneratePortrait(minion.character);
            //minionAssignmentPartyRecallButton.gameObject.SetActive(false);
            //minionAssignmentPartyConfirmButton.gameObject.SetActive(true);
            //minionAssignmentDraggableItem.SetDraggable(true);
            minionAssignmentConfirmButton.interactable = !activeArea.areaInvestigation.isAttacking;
            OnUpdateLandmarkInvestigationState("attack");
        } else {
            ResetMinionAssignmentParty(index);
        }

        List<Character> assignedCharacters = new List<Character>();
        for (int i = 0; i < _assignedParty.Length; i++) {
            if (_assignedParty[i] != null) {
                assignedCharacters.Add(_assignedParty[i].character);
            }
        }

        float chance = 0f;
        float enemyChance = 0f;
        DefenderGroup defender = activeArea.GetFirstDefenderGroup();
        if (defender != null) {
            CombatManager.Instance.GetCombatChanceOfTwoLists(assignedCharacters, defender.party.characters, out chance, out enemyChance);
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
        minionAssignmentPartyWinChance.text = "Send up to four units to attack.\nWin Chance: " + _currentWinChance.ToString("F2") + "%";
    }
    public void OnClickConfirmInvestigation() {
        activeArea.areaInvestigation.InvestigateLandmark(_assignedMinion);
        OnUpdateLandmarkInvestigationState("explore");
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickRecall() {
        activeArea.areaInvestigation.RecallMinion("explore");
        LoadSupportTokenData();
        //OnUpdateLandmarkInvestigationState();
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickConfirmPartyInvestigation() {
        activeArea.areaInvestigation.AttackRaidLandmark(_currentSelectedInvestigateButton.actionName, _assignedParty, activeArea.coreTile.landmarkOnTile);
        OnUpdateLandmarkInvestigationState("attack");
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    public void OnClickPartyRecall() {
        activeArea.areaInvestigation.RecallMinion("attack");
        //OnUpdateLandmarkInvestigationState();
        //ChangeStateAllButtons(!_activeLandmark.landmarkInvestigation.isActivated);
    }
    private void UpdateMinionTooltipDescription(Minion minion) {
        if (minion != null) {
            string jobStr = string.Empty;
            if (activeArea.areaInvestigation.assignedMinion == null || activeArea.areaInvestigation.assignedMinion.character.ownParty.icon.isTravelling) {
                switch (minion.character.job.jobType) {
                    case JOB.INSTIGATOR:
                        jobStr = "will <b>sow discord</b> in";
                        break;
                    case JOB.EXPLORER:
                        jobStr = "will <b>explore</b>";
                        break;
                    case JOB.DIPLOMAT:
                        jobStr = "will <b>improve relations with characters</b> in";
                        break;
                    case JOB.RECRUITER:
                        jobStr = "will <b>recruit new minions</b> in";
                        break;
                    case JOB.RAIDER:
                        jobStr = "will <b>raid</b>";
                        break;
                    case JOB.SPY:
                        jobStr = "will <b>obtain token</b> about";
                        break;
                    case JOB.DISSUADER:
                        jobStr = "will <b>discourage activities</b> in";
                        break;
                    default:
                        jobStr = "will <b>do nothing</b> in";
                        break;
                }
            } else {
                switch (minion.character.job.jobType) {
                    case JOB.INSTIGATOR:
                        jobStr = "is <b>sowing discord</b> in";
                        break;
                    case JOB.EXPLORER:
                        jobStr = "is <b>exploring</b>";
                        break;
                    case JOB.DIPLOMAT:
                        jobStr = "is <b>improving relations with characters</b> in";
                        break;
                    case JOB.RECRUITER:
                        jobStr = "is <b>recruiting new minions</b> in";
                        break;
                    case JOB.RAIDER:
                        jobStr = "is <b>raiding</b>";
                        break;
                    case JOB.SPY:
                        jobStr = "is <b>obtaining token</b> about";
                        break;
                    case JOB.DISSUADER:
                        jobStr = "is <b>discouraging activities</b> in";
                        break;
                    default:
                        jobStr = "is <b>doing nothing</b> in";
                        break;
                }
            }
            
            minionAssignmentDescription.text = "This minion " + jobStr + " this area";
        } else {
            minionAssignmentDescription.text = "Drag a minion from the list here.";
        }
        minionAssignmentDescriptionEnvelop.Execute();
    }
    private void OnMinionInvestigateArea(Minion minion, Area area) {
        if (this.isShowing && activeArea != null && activeArea.id == area.id) {
            UpdateMinionTooltipDescription(minion);
        }
    }
    private void OnInvestigatorMinionDraggedOut(object obj, int index) {
        AssignMinionToInvestigate(null);
    }
    #endregion

    #region Token Collector
    public void OnTokenCollectorMinionDrop(object obj, int index) {
        if (obj is Minion) {
            AssignMinionToTokenCollection(obj as Minion);
        }
        //PlayerCharacterItem minionItem = go.GetComponent<DragObject>().parentItem as PlayerCharacterItem;
        //if (minionItem != null) {
        //    AssignMinionToTokenCollection(minionItem.minion);
        //}
    }
    public void OnTokenCollectorDragOut(object obj, int index) {
        AssignMinionToTokenCollection(null);
    }
    public void AssignMinionToTokenCollection(Minion minion) {
        _assignedTokenCollectorMinion = minion;
        if (minion != null) {
            tokenCollectorSlot.PlaceObject(minion);
            //tokenCollectorPortrait.gameObject.SetActive(true);
            //tokenCollectorPortrait.GeneratePortrait(minion.character);
            UpdateTokenCollectorInteractables();
        } else {
            ResetTokenCollectorAssignment();
        }
    }
    public void ResetTokenCollectorAssignment() {
        _assignedTokenCollectorMinion = null;
        tokenCollectorSlot.ClearSlot(true);
        tokenCollectorSlot.dropZone.SetEnabledState(true);
        //tokenCollectorPortrait.gameObject.SetActive(false);
        UpdateTokenCollectorInteractables();
    }
    public void OnClickConfirmTokenCollection() {
        activeArea.areaInvestigation.AssignTokenCollector(_assignedTokenCollectorMinion);
        tokenCollectorSlot.dropZone.SetEnabledState(false);
        UpdateTokenCollectorData();
    }
    public void OnClickTokenCollectorRecall() {
        activeArea.areaInvestigation.RecallMinion("collect");
    }
    private void UpdateTokenCollectorData() {
        if (activeArea.areaInvestigation.tokenCollector != null) {
            AssignMinionToTokenCollection(activeArea.areaInvestigation.tokenCollector);
            tokenCollectorSlot.PlaceObject(activeArea.areaInvestigation.tokenCollector);
            //tokenCollectorPortrait.gameObject.SetActive(true);
            //tokenCollectorPortrait.GeneratePortrait(activeArea.areaInvestigation.tokenCollector.character);
        } else {
            //tokenCollectorPortrait.gameObject.SetActive(false);
            //tokenCollectorSlot.ClearSlot(true);
            ResetTokenCollectorAssignment();
        }
        UpdateTokenCollectorInteractables();
    }
    private void UpdateTokenCollectorInteractables() {
        if (activeArea.areaInvestigation.tokenCollector != null) { //if there is already an assigned minion as the token collector
            //only show the recall button
            tokenCollectorRecallBtn.gameObject.SetActive(true);
            tokenCollectorConfirmBtn.gameObject.SetActive(false);
            //set portrait as undraggable
            tokenCollectorSlot.draggable.SetDraggable(false);
        } else { //else
            //disable recall button
            tokenCollectorRecallBtn.gameObject.SetActive(false);
            tokenCollectorConfirmBtn.gameObject.SetActive(true);
            //set portrait as draggable
            tokenCollectorSlot.draggable.SetDraggable(true);
            if (_assignedTokenCollectorMinion != null) { //check if there is an assigned token collector minion (not yet locked in)
                if (activeArea.areaInvestigation.CanCollectTokensHere(_assignedTokenCollectorMinion)) { //check if that assigned minion can collect tokens in this area
                    //if yes, enable confirm btn
                    tokenCollectorConfirmBtn.interactable = true;
                } else {
                    //if no, disable confirm btn
                    tokenCollectorConfirmBtn.interactable = false;
                }
            } else { //else if there is no assigned token collector yet
                //disable confirm btn
                tokenCollectorConfirmBtn.interactable = false;
            }
        }
    }
    #endregion

    #region Token Assignment
    private Token assignedSupportToken;
    private void LoadSupportTokenData() {
        if (activeArea.areaInvestigation.assignedMinion != null 
            && activeArea.areaInvestigation.assignedMinion.character.job.attachedToken != null) {
            assignedSupportToken = activeArea.areaInvestigation.assignedMinion.character.job.attachedToken;
            supportingTokenSlot.PlaceObject(assignedSupportToken);
        } else {
            ResetSupportTokenSlot();
        }
        UpdateConsumeButtonState();
    }
    private void OnSupportTokenDropped(object obj, int index) {
        assignedSupportToken = obj as Token;
        UpdateConsumeButtonState();
    }
    private void OnSupportTokenDraggedOut(object obj, int index) {
        ResetSupportTokenSlot();
    }
    private void ResetSupportTokenSlot() {
        assignedSupportToken = null;
        supportingTokenSlot.ClearSlot(true);
        supportingTokenSlot.dropZone.SetEnabledState(true);
        supportingTokenSlot.draggable.SetDraggable(true);
        UpdateConsumeButtonState();
    }
    public void OnClickConsumeSupportToken() {
        activeArea.areaInvestigation.assignedMinion.character.job.SetToken(assignedSupportToken);
        assignedSupportToken.PlayerConsumeToken();
        supportingTokenSlot.dropZone.SetEnabledState(false);
        supportingTokenSlot.draggable.SetDraggable(false);
        UpdateConsumeButtonState();
    }
    private void UpdateConsumeButtonState() {
        if (assignedSupportToken == null) {
            consumeSuppotingTokenBtn.gameObject.SetActive(false);
        } else {
            if (activeArea.areaInvestigation.assignedMinion == null) {
                consumeSuppotingTokenBtn.gameObject.SetActive(false);
            } else {
                if (activeArea.areaInvestigation.assignedMinion.character.job.attachedToken != null) {
                    consumeSuppotingTokenBtn.gameObject.SetActive(false);
                } else {
                    consumeSuppotingTokenBtn.gameObject.SetActive(true);
                }
            }
            //consumeSuppotingTokenBtn.gameObject.SetActive(activeArea.areaInvestigation.isExploring);
        }
    }
    #endregion
}
