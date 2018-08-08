using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using ECS;

public class LandmarkInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 20;

    public bool isWaitingForAttackTarget;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI structureTypeLbl;
    [SerializeField] private Image structureIcon;
    [SerializeField] private Image areaIcon;
    [SerializeField] private Image factionIcon;
    [SerializeField] private Slider healthProgressBar;

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

    private LogHistoryItem[] logHistoryItems;

    internal BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    internal override void Initialize() {
        base.Initialize();
        SetWaitingForAttackState(false);
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
        Messenger.AddListener<NewParty, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, OnPartyEnteredLandmark);
        Messenger.AddListener<NewParty, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, OnPartyExitedLandmark);
        Messenger.AddListener<Item, BaseLandmark>(Signals.ITEM_PLACED_AT_LANDMARK, OnItemAddedToLandmark);
        Messenger.AddListener<Item, BaseLandmark>(Signals.ITEM_REMOVED_FROM_LANDMARK, OnItemRemovedFromLandmark);
    }
    public override void ShowMenu() {
        base.ShowMenu();
        UpdateBasicInfo();
        UpdateCharacters();
        UpdateItems();
        UpdateAllHistoryInfo();
        //ShowAttackButton();
    }
    //public override void SetData(object data) {
        //base.SetData(data);
        //UIManager.Instance.hexTileInfoUI.SetData((data as BaseLandmark).tileLocation);
        //if (isShowing) {
        //    UpdateLandmarkInfo();
        //}
    //}

    public void UpdateLandmarkInfo() {
        if (currentlyShowingLandmark == null) {
            return;
        }
        UpdateHP();
    }
    private void UpdateBasicInfo() {
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(currentlyShowingLandmark.specificLandmarkType);
        structureIcon.sprite = data.landmarkTypeIcon;
        structureTypeLbl.text = data.landmarkTypeString;
        if (currentlyShowingLandmark.tileLocation.areaOfTile == null) {
            areaIcon.gameObject.SetActive(false);
        } else {
            areaIcon.gameObject.SetActive(true);
        }
        if (currentlyShowingLandmark.owner == null) {
            factionIcon.gameObject.SetActive(false);
        } else {
            factionIcon.gameObject.SetActive(true);
        }
        healthProgressBar.minValue = 0f;
        healthProgressBar.maxValue = currentlyShowingLandmark.totalDurability;
        UpdateHP();
    }
    private void UpdateHP() {
        healthProgressBar.value = currentlyShowingLandmark.currDurability;
    }

    #region Log History
    private void UpdateHistory(object obj) {
        if (obj is BaseLandmark && currentlyShowingLandmark != null && (obj as BaseLandmark).id == currentlyShowingLandmark.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> landmarkHistory = new List<Log>(currentlyShowingLandmark.history.OrderByDescending(x => x.id));
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
        for (int i = 0; i < currentlyShowingLandmark.charactersAtLocation.Count; i++) {
            NewParty currParty = currentlyShowingLandmark.charactersAtLocation[i];
            CreateNewCharacterItem(currParty);
        }
    }
    private LandmarkCharacterItem GetItem(NewParty party) {
        LandmarkCharacterItem[] items = Utilities.GetComponentsInDirectChildren<LandmarkCharacterItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            LandmarkCharacterItem item = items[i];
            if (item.party.id == party.id) {
                return item;
            }
        }
        return null;
    }
    private void CreateNewCharacterItem(NewParty party) {
        GameObject characterGO = UIManager.Instance.InstantiateUIObject(landmarkCharacterPrefab.name, charactersScrollView.content);
        LandmarkCharacterItem item = characterGO.GetComponent<LandmarkCharacterItem>();
        item.SetParty(party, currentlyShowingLandmark);
    }
    private void OnPartyEnteredLandmark(NewParty party, BaseLandmark landmark) {
        if (isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark.id == landmark.id) {
            CreateNewCharacterItem(party);
        }
    }
    private void OnPartyExitedLandmark(NewParty party, BaseLandmark landmark) {
        if (isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark.id == landmark.id) {
            LandmarkCharacterItem item = GetItem(party);
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        }
    }
    #endregion
    
    #region Items
    private void UpdateItems() {
        for (int i = 0; i < itemContainers.Length; i++) {
            ItemContainer container = itemContainers[i];
            Item item = currentlyShowingLandmark.itemsInLandmark.ElementAtOrDefault(i);
            container.SetItem(item);
        }
    }
    private void OnItemAddedToLandmark(Item item, BaseLandmark landmark) {
        if (isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark.id == landmark.id) {
            UpdateItems();
        }
    }
    private void OnItemRemovedFromLandmark(Item item, BaseLandmark landmark) {
        if (isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark.id == landmark.id) {
            UpdateItems();
        }
    }
    #endregion

    public void OnClickCloseBtn(){
		HideMenu ();
	}
    public void CenterOnLandmark() {
        currentlyShowingLandmark.CenterOnLandmark();
    }

    #region Attack Landmark
    private void ShowAttackButton() {
        BaseLandmark landmark = currentlyShowingLandmark;
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
        if (currentlyShowingLandmark == null) {
            return;
        }
        if (obj.objectLocation == null) {
            return;
        }
        if (obj.objectLocation.id == currentlyShowingLandmark.id) {
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
            currentlyShowingLandmark.landmarkVisual.DrawPathTo(tile.landmarkOnTile);
        }
    }
    private void TileHoverOut(HexTile tile) {
        currentlyShowingLandmark.landmarkVisual.HidePathVisual();
    }
    private void OnAttackTargetSelected(BaseLandmark target) {
        Debug.Log(currentlyShowingLandmark.landmarkName + " will attack " + target.landmarkName);
        currentlyShowingLandmark.landmarkObj.AttackLandmark(target);
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
            currentlyShowingLandmark.landmarkVisual.HidePathVisual();
            Messenger.RemoveListener<HexTile>(Signals.TILE_HOVERED_OVER, TileHoverOver);
            Messenger.RemoveListener<HexTile>(Signals.TILE_HOVERED_OUT, TileHoverOut);
            Messenger.RemoveListener<HexTile>(Signals.TILE_RIGHT_CLICKED, TileRightClicked);
            Messenger.RemoveListener<BaseLandmark>(Signals.LANDMARK_ATTACK_TARGET_SELECTED, OnAttackTargetSelected);
        }
    }

    
}
