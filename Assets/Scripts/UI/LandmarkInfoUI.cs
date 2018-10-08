using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using ECS;

public class LandmarkInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 60;

    public bool isWaitingForAttackTarget;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI structureTypeLbl;
    //[SerializeField] private Image structureIcon;
    //[SerializeField] private Image areaIcon;
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private Slider healthProgressBar;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private GameObject[] secrets;
    [SerializeField] private GameObject[] intel;
    [SerializeField] private GameObject[] encounters;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject charactersGO;
    [SerializeField] private GameObject landmarkCharacterPrefab;
    [SerializeField] private ScrollRect charactersScrollView;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private GameObject itemsGO;
    [SerializeField] private RectTransform itemsParent;
    private ItemContainer[] itemContainers;

    [Space(10)]
    [Header("Events")]
    [SerializeField] private GameObject eventsGO;
    [SerializeField] private ScrollRect eventsScrollView;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Settlement")]
    [SerializeField] private GameObject attackButtonGO;
    [SerializeField] private Toggle attackBtnToggle;

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
        SetWaitingForAttackState(false);
        healthProgressBar.minValue = 0f;
        //Messenger.AddListener("UpdateUI", UpdateLandmarkInfo);
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);

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

        //populate item containers
        itemContainers = itemsParent.GetComponentsInChildren<ItemContainer>();
        Messenger.AddListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        Messenger.AddListener<Party, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, OnPartyExitedLandmark);
        Messenger.AddListener<Item, BaseLandmark>(Signals.ITEM_PLACED_AT_LANDMARK, OnItemAddedToLandmark);
        Messenger.AddListener<Item, BaseLandmark>(Signals.ITEM_REMOVED_FROM_LANDMARK, OnItemRemovedFromLandmark);
    }
    public override void OpenMenu() {
        base.OpenMenu();
        _activeLandmark = _data as BaseLandmark;
        UpdateLandmarkInfo();
        //ShowAttackButton();
        if(_activeLandmark.specificLandmarkType != LANDMARK_TYPE.DEMONIC_PORTAL) {
            PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(_activeLandmark);
        }
        charactersScrollView.verticalNormalizedPosition = 1;
        historyScrollView.verticalNormalizedPosition = 1;
        PlayerUI.Instance.UncollapseMinionHolder();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        _activeLandmark = null;
        PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        PlayerUI.Instance.CollapseMinionHolder();
    }
    //public override void SetData(object data) {
    //base.SetData(data);
    //UIManager.Instance.hexTileInfoUI.SetData((data as BaseLandmark).tileLocation);
    //if (isShowing) {
    //    UpdateLandmarkInfo();
    //}
    //}

    public void UpdateLandmarkInfo() {
        if (_activeLandmark == null) {
            return;
        }
        UpdateBasicInfo();
        if (!_activeLandmark.isBeingInspected) {
            UpdateBGs(true);
        } else {
            UpdateBGs(false);
        }
        UpdateInfo();
        UpdateCharacters();
        UpdateItems();
        UpdateAllHistoryInfo();
    }
    private void UpdateBGs(bool state) {
        for (int i = 0; i < notInspectedBGs.Length; i++) {
            notInspectedBGs[i].SetActive(state);
        }
    }
    private void UpdateBasicInfo() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_activeLandmark.specificLandmarkType);
        //structureIcon.sprite = data.landmarkTypeIcon;
        structureTypeLbl.text = data.landmarkTypeString + "(" + _activeLandmark.locationName + ")";
        //if (currentlyShowingLandmark.tileLocation.areaOfTile == null) {
        //    areaIcon.gameObject.SetActive(false);
        //} else {
        //    areaIcon.gameObject.SetActive(true);
        //}
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

    #region Log History
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
            if(item.party != null) {
                if (item.party.id == party.id) {
                    return item;
                }
            }
        }
        return null;
    }
    private void CreateNewCharacterItem(Party party) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetParty(party, _activeLandmark);
    }
    private void CreateNewCharacterItem(LandmarkPartyData partyData) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetPartyData(partyData);
    }
    private void OnPartyEnteredLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id && _activeLandmark.isBeingInspected) {
            CreateNewCharacterItem(party);
        }
    }
    private void OnPartyExitedLandmark(Party party, BaseLandmark landmark) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) {
            LandmarkCharacterItem item = GetItem(party);
            if(item != null) {
                ObjectPoolManager.Instance.DestroyObject(item.gameObject);
            }
        }
    }
    #endregion
    
    #region Items
    private void UpdateItems() {
        if (GameManager.Instance.inspectAll) {
            for (int i = 0; i < itemContainers.Length; i++) {
                ItemContainer container = itemContainers[i];
                Item item = null;
                if (i < _activeLandmark.itemsInLandmark.Count) {
                    item = _activeLandmark.itemsInLandmark[i];
                }
                container.SetItem(item, true);
            }
            return;
        }
        if (!_activeLandmark.isBeingInspected && _activeLandmark.hasBeenInspected) {
            for (int i = 0; i < itemContainers.Length; i++) {
                ItemContainer container = itemContainers[i];
                Item item = null;
                if (i < _activeLandmark.lastInspectedItemsInLandmark.Count) {
                    item = _activeLandmark.lastInspectedItemsInLandmark[i];
                }
                container.SetItem(item, _activeLandmark.hasBeenInspected);
            }
            return;
        }
        for (int i = 0; i < itemContainers.Length; i++) {
            ItemContainer container = itemContainers[i];
            Item item = null;
            if (i < _activeLandmark.itemsInLandmark.Count) {
                item = _activeLandmark.itemsInLandmark[i];
            }
            container.SetItem(item, _activeLandmark.hasBeenInspected);
        }
    }
    private void OnItemAddedToLandmark(Item item, BaseLandmark landmark) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) {
            UpdateItems();
        }
    }
    private void OnItemRemovedFromLandmark(Item item, BaseLandmark landmark) {
        if (isShowing && _activeLandmark != null && _activeLandmark.id == landmark.id) {
            UpdateItems();
        }
    }
    #endregion

    #region Info
    private void UpdateInfo() {
        for (int i = 0; i < secrets.Length; i++) {
            if(i < _activeLandmark.secrets.Count) {
                secrets[i].SetActive(true);
            } else {
                secrets[i].SetActive(false);
            }
        }
        for (int i = 0; i < intel.Length; i++) {
            if (i < _activeLandmark.intels.Count) {
                intel[i].SetActive(true);
            } else {
                intel[i].SetActive(false);
            }
        }
        for (int i = 0; i < encounters.Length; i++) {
            if (i < _activeLandmark.encounters.Count) {
                encounters[i].SetActive(true);
            } else {
                encounters[i].SetActive(false);
            }
        }
    }
    #endregion

    public void OnClickCloseBtn(){
		CloseMenu ();
	}
    public void CenterOnLandmark() {
        _activeLandmark.CenterOnLandmark();
    }

    #region Attack Landmark
    private void ShowAttackButton() {
        BaseLandmark landmark = _activeLandmark;
        if (!landmark.isAttackingAnotherLandmark) {
            if ((landmark.landmarkObj.specificObjectType == LANDMARK_TYPE.GARRISON || landmark.landmarkObj.specificObjectType == LANDMARK_TYPE.DEMONIC_PORTAL) && landmark.landmarkObj.currentState.stateName == "Ready") {
                attackButtonGO.SetActive(true);
                attackBtnToggle.isOn = false;
                SetWaitingForAttackState(false);
            } else {
                attackButtonGO.SetActive(false);
            }
        } else {
            attackButtonGO.SetActive(false);
        }
        //SetAttackButtonState(false);
    }
    //public void ToggleAttack() {
    //    SetWaitingForAttackState(!isWaitingForAttackTarget);
    //}
    public void SetWaitingForAttackState(bool state) {
        //attackBtnToggle.isOn = state;
    }
    public void OnSetAttackState(bool state) {
        isWaitingForAttackTarget = state;
        if (isWaitingForAttackTarget) {
            GameManager.Instance.SetCursorToTarget();
            OnStartWaitingForAttack();
        } else {
            GameManager.Instance.SetCursorToDefault();
            OnEndWaitingForAttack();
        }
    }
    //private void NotWaitingForAttackState() {
    //    attackBtnToggle.isOn = false;
    //    isWaitingForAttackTarget = false;
    //    GameManager.Instance.SetCursorToDefault();
    //}
    public void SetActiveAttackButtonGO(bool state) {
        attackButtonGO.SetActive(state);
        if (state) {
            SetWaitingForAttackState(false);
        }
    }
    #endregion

    private void OnStructureChangedState(StructureObj obj, ObjectState newState) {
        if (_activeLandmark == null) {
            return;
        }
        if (obj.objectLocation == null) {
            return;
        }
        if (obj.objectLocation.id == _activeLandmark.id) {
            //if (newState.stateName.Equals("Ready")) {
            //    SetActiveAttackButtonGO(true);
            //} else {
            //    SetActiveAttackButtonGO(false);
            //}
        }
    }

    private void OnStartWaitingForAttack() {
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, TileHoverOver);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, TileHoverOut);
        Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, TileRightClicked);
        Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_ATTACK_TARGET_SELECTED, OnAttackTargetSelected);
    }
    private void TileHoverOver(HexTile tile) {
        if (tile.landmarkOnTile != null) {
            _activeLandmark.landmarkVisual.DrawPathTo(tile.landmarkOnTile);
        }
    }
    private void TileHoverOut(HexTile tile) {
        _activeLandmark.landmarkVisual.HidePathVisual();
    }
    private void OnAttackTargetSelected(BaseLandmark target) {
        Debug.Log(_activeLandmark.landmarkName + " will attack " + target.landmarkName);
        _activeLandmark.landmarkObj.AttackLandmark(target);
        SetWaitingForAttackState(false);
        SetActiveAttackButtonGO(false);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        //OnEndWaitingForAttack();
        //currentlyShowingLandmark.landmarkVisual.HidePathVisual();
        //SetWaitingForAttackState(false);
        //NotWaitingForAttackState();
    }
    private void TileRightClicked(HexTile tile) {
        SetWaitingForAttackState(false);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        //NotWaitingForAttackState();
    }
    private void OnEndWaitingForAttack() {
        if (this.gameObject.activeSelf) {
            _activeLandmark.landmarkVisual.HidePathVisual();
            Messenger.RemoveListener<HexTile>(Signals.TILE_HOVERED_OVER, TileHoverOver);
            Messenger.RemoveListener<HexTile>(Signals.TILE_HOVERED_OUT, TileHoverOut);
            Messenger.RemoveListener<HexTile>(Signals.TILE_RIGHT_CLICKED, TileRightClicked);
            Messenger.RemoveListener<BaseLandmark>(Signals.LANDMARK_ATTACK_TARGET_SELECTED, OnAttackTargetSelected);
        }
    }

    
}
