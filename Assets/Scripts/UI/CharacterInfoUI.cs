using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;
using UnityEngine.UI;
using System;

public class CharacterInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 300;
    public bool isWaitingForAttackTarget;
    public bool isWaitingForJoinBattleTarget;

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private TextMeshProUGUI plansLbl;
    [SerializeField] private TextMeshProUGUI supplyLbl;
    [SerializeField] private LogItem plansLblLogItem;
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private PartyEmblem partyEmblem;

    [Space(10)]
    [Header("Location")]
    [SerializeField] private LocationPortrait visitorLocationPortrait;
    [SerializeField] private LocationPortrait residentLocationPortrait;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logParentGO;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;
    [SerializeField] private GameObject logsMenuCover;

    [Space(10)]
    [Header("Character")]
    [SerializeField] private GameObject attackButtonGO;
    [SerializeField] private Toggle attackBtnToggle;
    [SerializeField] private GameObject joinBattleButtonGO;
    [SerializeField] private Toggle joinBattleBtnToggle;
    [SerializeField] private GameObject releaseBtnGO;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;

    [Space(10)]
    [Header("Traits")]
    [SerializeField] private ScrollRect statusTraitsScrollView;
    [SerializeField] private ScrollRect normalTraitsScrollView;
    [SerializeField] private ScrollRect relationshipTraitsScrollView;
    [SerializeField] private ScrollRect itemsScrollView;
    [SerializeField] private GameObject combatAttributePrefab;

    [Space(10)]
    [Header("Memories")]
    [SerializeField] private GameObject memoriesGO;
    [SerializeField] private GameObject memoryItemPrefab;
    [SerializeField] private ScrollRect memoriesScrollView;
    public MemoryItem[] memoryItems { get; private set; }

    private CombatAttributeItem[] statusTraitContainers;
    private CombatAttributeItem[] normalTraitContainers;
    private CombatAttributeItem[] relationshipTraitContainers;
    private ItemContainer[] inventoryItemContainers;

    private LogHistoryItem[] logHistoryItems;

    private Character _activeCharacter;
    private Character _previousCharacter;

    public Character activeCharacter {
        get { return _activeCharacter; }
    }
    public Character previousCharacter {
        get { return _previousCharacter; }
    }

    internal override void Initialize() {
        base.Initialize();
        isWaitingForAttackTarget = false;
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener<BaseLandmark>(Signals.PLAYER_LANDMARK_CREATED, OnPlayerLandmarkCreated);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Character>(Signals.CHARACTER_TRACKED, OnCharacterTracked);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<SpecialToken, Character>(Signals.CHARACTER_OBTAINED_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<SpecialToken, Character>(Signals.CHARACTER_LOST_ITEM, UpdateInventoryInfoFromSignal);

        statusTraitContainers = Utilities.GetComponentsInDirectChildren<CombatAttributeItem>(statusTraitsScrollView.content.gameObject);
        normalTraitContainers = Utilities.GetComponentsInDirectChildren<CombatAttributeItem>(normalTraitsScrollView.content.gameObject);
        relationshipTraitContainers = Utilities.GetComponentsInDirectChildren<CombatAttributeItem>(relationshipTraitsScrollView.content.gameObject);
        inventoryItemContainers = Utilities.GetComponentsInDirectChildren<ItemContainer>(itemsScrollView.content.gameObject);

        //InitializeMemoryUI();

        InitializeLogsMenu();
    }
    private void InitializeLogsMenu() {
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

    private void OnCharacterDied(Character character) {
        if (this.isShowing && activeCharacter.id == character.id) {
            AreaMapCameraMove.Instance.CenterCameraOn(null);
        }
    }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        _activeCharacter = null;
        AreaMapCameraMove.Instance.CenterCameraOn(null);
        //UIManager.Instance.SetCoverState(false);
        //PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        //PlayerUI.Instance.CollapseMinionHolder();
        //InteractionUI.Instance.HideInteractionUI();
    }
    public override void OpenMenu() {
        _previousCharacter = _activeCharacter;
        _activeCharacter = _data as Character;
        _activeCharacter.CenterOnCharacter(false);
        base.OpenMenu();
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        UIManager.Instance.HideObjectPicker();
        UpdateCharacterInfo();
        UpdateTraits();
        UpdateInventoryInfo();
        ResetAllScrollPositions();
    }
    public override void SetData(object data) {
        base.SetData(data);
        //if (isShowing) {
        //    UpdateCharacterInfo();
        //}
    }
    #endregion

    public void ResetAllScrollPositions() {
        historyScrollView.verticalNormalizedPosition = 1;
        relationshipTraitsScrollView.verticalNormalizedPosition = 1;
        statusTraitsScrollView.verticalNormalizedPosition = 1;
        normalTraitsScrollView.verticalNormalizedPosition = 1;
    }

    public void UpdateCharacterInfo() {
        if (_activeCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
        UpdateLocationInfo();
        UpdateAllHistoryInfo();
        //UpdateMemories();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeCharacter);
        //characterPortrait.SetBGState(false);
    }
    public void UpdateBasicInfo() {
        nameLbl.text = _activeCharacter.name;
        lvlClassLbl.text = _activeCharacter.raceClassName; //+ " (" + _activeCharacter.currentMoodType.ToString() + ")"; // + " " + _activeCharacter.role.name
        supplyLbl.text = _activeCharacter.supply.ToString();
        UpdateThoughtBubble();
    }

    private void UpdateThoughtBubble() {
        if (_activeCharacter.isDead) {
            plansLbl.text = _activeCharacter.name + " has died.";
            return;
        }
        //Disabler Thought
        if (_activeCharacter.doNotDisturb > 0) {
            Trait disablerTrait = _activeCharacter.GetTraitOf(TRAIT_TYPE.DISABLER);
            if (disablerTrait != null) {
                if (disablerTrait.thoughtText != null && disablerTrait.thoughtText != string.Empty) {
                    plansLbl.text = disablerTrait.thoughtText.Replace("[Character]", _activeCharacter.name);
                    return;
                } 
                //else {
                //    plansLbl.text = _activeCharacter.name + " has a disabler trait: " + disablerTrait.name + ".";
                //}
                //return;
            }
        }

        //Character State
        if(_activeCharacter.stateComponent.currentState != null) {
            plansLblLogItem.SetLog(_activeCharacter.stateComponent.currentState.thoughtBubbleLog);
            plansLbl.text = Utilities.LogReplacer(_activeCharacter.stateComponent.currentState.thoughtBubbleLog);
            return;
        }

        //targetted by action
        if (_activeCharacter.targettedByAction.Count > 0) {
            //character is targetted by an action
            Log targetLog = null;
            for (int i = 0; i < _activeCharacter.targettedByAction.Count; i++) {
                GoapAction action = _activeCharacter.targettedByAction[i];
                if (action.isPerformingActualAction && action.targetLog != null) {
                    targetLog = action.targetLog;
                    break;
                }
            }
            if (targetLog != null) {
                plansLbl.text = Utilities.LogReplacer(targetLog);
                return;
            }
        }

        //Action
        if (_activeCharacter.currentAction != null && !_activeCharacter.currentAction.isStopped) {
            if (_activeCharacter.currentParty.icon.isTravelling) {
                //character is travelling
                plansLblLogItem.SetLog(_activeCharacter.currentAction.thoughtBubbleMovingLog);
                plansLbl.text = Utilities.LogReplacer(_activeCharacter.currentAction.thoughtBubbleMovingLog);
            } else {
                //character is not travelling
                if (_activeCharacter.currentAction.isDone) {
                    //action is already done
                    plansLblLogItem.SetLog(_activeCharacter.currentAction.currentState.descriptionLog);
                    plansLbl.text = Utilities.LogReplacer(_activeCharacter.currentAction.currentState.descriptionLog);
                } else {
                    //action is not yet done
                    if (activeCharacter.currentAction.currentState == null) {
                        //if the actions' current state is null, Use moving log
                        plansLblLogItem.SetLog(_activeCharacter.currentAction.thoughtBubbleMovingLog);
                        plansLbl.text = Utilities.LogReplacer(_activeCharacter.currentAction.thoughtBubbleMovingLog);
                    } else {
                        //if the actions current state has a duration
                        plansLblLogItem.SetLog(_activeCharacter.currentAction.thoughtBubbleLog);
                        plansLbl.text = Utilities.LogReplacer(_activeCharacter.currentAction.thoughtBubbleLog);
                    }
                }
            }
            return;
        }

        //State Job To Do
        if (_activeCharacter.stateComponent.stateToDo != null) {
            plansLblLogItem.SetLog(_activeCharacter.stateComponent.stateToDo.thoughtBubbleLog);
            plansLbl.text = Utilities.LogReplacer(_activeCharacter.stateComponent.stateToDo.thoughtBubbleLog);
            return;
        }



        ////Travel Thought
        //if (!_activeCharacter.isDead && _activeCharacter.currentParty.icon.isTravelling) {
        //    //&& _activeCharacter.currentParty.icon.travelLine != null
        //    if (_activeCharacter.currentAction != null) {
        //        //use moving log of current action
        //        plansLblLogItem.SetLog(_activeCharacter.currentAction.thoughtBubbleMovingLog);
        //        plansLbl.text = Utilities.LogReplacer(_activeCharacter.currentAction.thoughtBubbleMovingLog);
        //    } else {
        //        plansLbl.text = _activeCharacter.name + " is travelling.";
        //    }
        //    return;
        //} 

        //Travelling
        if (_activeCharacter.currentParty.icon.isTravelling) {
            if(_activeCharacter.currentParty.owner.marker.destinationTile != null) {
                plansLbl.text = _activeCharacter.name + " is going to " + _activeCharacter.currentParty.owner.marker.destinationTile.structure.GetNameRelativeTo(_activeCharacter);
                return;
            }
        }

        //Default - Do nothing/Idle
        if (_activeCharacter.currentStructure != null) {
            plansLbl.text =  _activeCharacter.name + " is in " + _activeCharacter.currentStructure.GetNameRelativeTo(_activeCharacter);
        }
    }

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = _activeCharacter.maxHP.ToString();
        attackLbl.text = _activeCharacter.attackPower.ToString();
        speedLbl.text = _activeCharacter.speed.ToString();
        if(characterPortrait.thisCharacter != null) {
            characterPortrait.UpdateLvl();
        }
    }
    #endregion

    #region Location
    private void UpdateLocationInfo() {
        visitorLocationPortrait.SetLocation(_activeCharacter.specificLocation);
        residentLocationPortrait.SetLocation(_activeCharacter.homeArea);
    }
    #endregion

    #region Combat Attributes
    private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(_activeCharacter == null || _activeCharacter != character) {
            return;
        }
        UpdateTraits();
        UpdateThoughtBubble();
    }
    private void UpdateTraits() {
        //Utilities.DestroyChildren(statusTraitsScrollView.content);
        //Utilities.DestroyChildren(normalTraitsScrollView.content);
        //Utilities.DestroyChildren(relationshipTraitsScrollView.content);

        int lastStatusIndex = 0;
        int lastNormalIndex = 0;
        int lastRelationshipIndex = 0;

        for (int i = 0; i < _activeCharacter.traits.Count; i++) {
            Trait currTrait = _activeCharacter.traits[i];
            if (currTrait.type == TRAIT_TYPE.ABILITY || currTrait.type == TRAIT_TYPE.ATTACK || currTrait.type == TRAIT_TYPE.COMBAT_POSITION
                || currTrait.name == "Herbivore" || currTrait.name == "Carnivore") {
                continue; //hide combat traits
            }
            if (currTrait is RelationshipTrait) {
                //CreateTraitGO(currTrait, relationshipTraitsScrollView.content);
                if (lastRelationshipIndex < relationshipTraitContainers.Length) {
                    relationshipTraitContainers[lastRelationshipIndex].SetCombatAttribute(currTrait);
                    lastRelationshipIndex++;
                }
            } else if (currTrait.type == TRAIT_TYPE.STATUS || currTrait.type == TRAIT_TYPE.DISABLER || currTrait.type == TRAIT_TYPE.ENCHANTMENT) {
                //CreateTraitGO(currTrait, statusTraitsScrollView.content);
                if (lastStatusIndex < statusTraitContainers.Length) {
                    statusTraitContainers[lastStatusIndex].SetCombatAttribute(currTrait);
                    lastStatusIndex++;
                }
            } else {
                //CreateTraitGO(currTrait, normalTraitsScrollView.content);
                if (lastNormalIndex < normalTraitContainers.Length) {
                    normalTraitContainers[lastNormalIndex].SetCombatAttribute(currTrait);
                    lastNormalIndex++;
                }
            }
        }

        if (lastRelationshipIndex < relationshipTraitContainers.Length) {
            for (int i = lastRelationshipIndex; i < relationshipTraitContainers.Length; i++) {
                relationshipTraitContainers[i].gameObject.SetActive(false);
            }
        }
        if (lastStatusIndex < statusTraitContainers.Length) {
            for (int i = lastStatusIndex; i < statusTraitContainers.Length; i++) {
                statusTraitContainers[i].gameObject.SetActive(false);
            }
        }
        if (lastNormalIndex < normalTraitContainers.Length) {
            for (int i = lastNormalIndex; i < normalTraitContainers.Length; i++) {
                normalTraitContainers[i].gameObject.SetActive(false);
            }
        }
    }
    //private void CreateTraitGO(Trait combatAttribute, RectTransform parent) {
    //    GameObject go = GameObject.Instantiate(combatAttributePrefab, parent);
    //    CombatAttributeItem combatAttributeItem = go.GetComponent<CombatAttributeItem>();
    //    combatAttributeItem.SetCombatAttribute(combatAttribute);
    //}
    #endregion

    #region Buttons
    public void OnClickLogButton() {
        logParentGO.SetActive(!logParentGO.activeSelf);
    }
    public void OnCloseLog() {
        logParentGO.SetActive(false);
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(SpecialToken token, Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateInventoryInfo();
        }
    }
    private void UpdateInventoryInfo() {
        for (int i = 0; i < inventoryItemContainers.Length; i++) {
            ItemContainer currContainer = inventoryItemContainers[i];
            SpecialToken currInventoryItem = _activeCharacter.items.ElementAtOrDefault(i);
            currContainer.SetItem(currInventoryItem);
        }
    }
    #endregion

    #region History
    private void UpdateHistory(object obj) {
        if (obj is Character && _activeCharacter != null && (obj as Character).id == _activeCharacter.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> characterHistory = new List<Log>(_activeCharacter.history.OrderByDescending(x => x.date.year).ThenByDescending(x => x.date.month).ThenByDescending(x => x.date.day).ThenByDescending(x => x.date.tick));
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            Log currLog = characterHistory.ElementAtOrDefault(i);
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

    public bool IsCharacterInfoShowing(Character character) {
        return (isShowing && _activeCharacter == character);
    }

    #region Attack Character
    private void ShowAttackButton() {
        SetActiveAttackButtonGO(true);
        SetAttackButtonState(false);
    }
    public void ToggleAttack() {
        isWaitingForAttackTarget = !isWaitingForAttackTarget;
        if (isWaitingForAttackTarget) {
            SetJoinBattleButtonState(false);
        }
    }
    public void SetAttackButtonState(bool state) {
        attackBtnToggle.isOn = state;
        //isWaitingForAttackTarget = state;
        if (isWaitingForAttackTarget) {
            SetJoinBattleButtonState(false);
        }
    }
    public void SetActiveAttackButtonGO(bool state) {
        attackButtonGO.SetActive(state);
    }
    #endregion

    #region Join Battle Character
    private void ShowJoinBattleButton() {
        SetActiveJoinBattleButtonGO(true);
        SetJoinBattleButtonState(false);
    }
    public void ToggleJoinBattle() {
        isWaitingForJoinBattleTarget = !isWaitingForJoinBattleTarget;
        if (isWaitingForJoinBattleTarget) {
            SetAttackButtonState(false);
        }
    }
    public void SetJoinBattleButtonState(bool state) {
        isWaitingForJoinBattleTarget = state;
        joinBattleBtnToggle.isOn = state;
        if (isWaitingForJoinBattleTarget) {
            SetAttackButtonState(false);
        }
    }
    public void SetActiveJoinBattleButtonGO(bool state) {
        joinBattleButtonGO.SetActive(state);
    }
    #endregion

    private void OnPlayerLandmarkCreated(BaseLandmark createdLandmark) {
        //if (createdLandmark.specificLandmarkType == LANDMARK_TYPE.SNATCHER_DEMONS_LAIR) {
        //    CheckShowSnatchButton();
        //}
    }

    #region Level Up
    public void LevelUpCharacter() {
        _activeCharacter.LevelUp();
        UpdateStatInfo();
    }
    public void LevelDownCharacter() {
        _activeCharacter.LevelUp(-1);
        UpdateStatInfo();
    }
    #endregion

    #region Death
    public void DieCharacter() {
        //if(_activeCharacter.party.currentCombat != null) {
        //    _activeCharacter.party.currentCombat.CharacterDeath(_activeCharacter, null);
        //}
        _activeCharacter.Death();
    }
    #endregion

    //#region Memories
    //private void InitializeMemoryUI() {
    //    memoryItems = new MemoryItem[CharacterManager.CHARACTER_MAX_MEMORY];
    //    for (int i = 0; i < CharacterManager.CHARACTER_MAX_MEMORY; i++) {
    //        GameObject go = GameObject.Instantiate(memoryItemPrefab, memoriesScrollView.content);
    //        memoryItems[i] = go.GetComponent<MemoryItem>();
    //    }
    //}
    //private void UpdateMemories() {
    //    if (memoriesGO.activeSelf) {
    //        for (int i = 0; i < memoryItems.Length; i++) {
    //            if(i < _activeCharacter.memories.memoryList.Count) {
    //                memoryItems[i].SetMemory(_activeCharacter.memories.memoryList[(_activeCharacter.memories.memoryList.Count - 1) - i]);
    //            } else {
    //                memoryItems[i].SetMemory(null);
    //            }
    //        }
    //    }
    //}
    //public void OpenMemories() {
    //    memoriesGO.SetActive(true);
    //    UpdateMemories();
    //}
    //#endregion

    #region Listeners
    private void OnCharacterTracked(Character character) {
        if (isShowing && activeCharacter.id == character.id) {
            UpdateBasicInfo();
        }
    }
    private void OnMenuOpened(UIMenu openedMenu) {
        //if (this.isShowing) {
        //    if (openedMenu is PartyInfoUI) {
        //        CheckIfMenuShouldBeHidden();
        //    }
        //}
    }
    private void OnMenuClosed(UIMenu closedMenu) {
        //if (this.isShowing) {
        //    if (closedMenu is PartyInfoUI) {
        //        CheckIfMenuShouldBeHidden();
        //    }
        //}
    }
    private void OnOpenShareIntelMenu() {
        backButton.interactable = false;
    }
    private void OnCloseShareIntelMenu() {
        backButton.interactable = UIManager.Instance.GetLastUIMenuHistory() != null;
    }
    #endregion
    private void CheckIfMenuShouldBeHidden() {
        if (UIManager.Instance.partyinfoUI.isShowing) {
            logParentGO.SetActive(false);
        } else {
            logParentGO.SetActive(true);
        }
    }
   

    public void ShowCharacterTestingInfo() {
        string summary = "Home structure: " + activeCharacter.homeStructure?.ToString() ?? "None";
        summary += "\nCurrent structure: " + activeCharacter.currentStructure?.ToString() ?? "None";
        summary += "\nRole: " + activeCharacter.role.roleType.ToString();
        summary += "\nSexuality: " + activeCharacter.sexuality.ToString();
        summary += "\nCurrent State: " + activeCharacter.stateComponent.currentState?.ToString() ?? "None";
        summary += "\nActions targetting this character: ";
        if (activeCharacter.targettedByAction.Count > 0) {
            for (int i = 0; i < activeCharacter.targettedByAction.Count; i++) {
                summary += "\n" + activeCharacter.targettedByAction[i].goapName + " done by " + activeCharacter.targettedByAction[i].actor.name;
            }
        } else {
            summary += "None";
        }
        summary += "\n" + activeCharacter.GetNeedsSummary();
        UIManager.Instance.ShowSmallInfo(summary);
    }

    public void HideCharacterTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    public void DropACharacter() {
        _activeCharacter.DropACharacter();
    }
    public void LogAwareness() {
        _activeCharacter.marker.LogPOIsInVisionRange();
        _activeCharacter.LogAwarenessList();
    }
}
