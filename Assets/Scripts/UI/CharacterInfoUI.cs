using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;
using UnityEngine.UI;
using System;

public class CharacterInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 300;
    private const int MAX_INVENTORY = 16;
    public bool isWaitingForAttackTarget;
    public bool isWaitingForJoinBattleTarget;

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private TextMeshProUGUI plansLbl;
    [SerializeField] private LogItem plansLblLogItem;
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private PartyEmblem partyEmblem;

    [Space(10)]
    [Header("Item And Location")]
    [SerializeField] private LocationPortrait visitorLocationPortrait;
    [SerializeField] private LocationPortrait residentLocationPortrait;
    [SerializeField] private ItemContainer itemContainer;

    [Space(10)]
    [Header("Relations")]
    [SerializeField] private GameObject relationsGO;
    [SerializeField] private ScrollRect relationsScrollView;
    [SerializeField] private GameObject relationshipItemPrefab;
    [SerializeField] private Color evenRelationshipColor;
    [SerializeField] private Color oddRelationshipColor;
    [SerializeField] private GameObject relationsMenuCover;

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
    [Header("Scheduling")]
    [SerializeField] private Dropdown monthDropdown;
    [SerializeField] private Dropdown dayDropdown;
    [SerializeField] private InputField yearField;
    [SerializeField] private InputField tickField;
    [SerializeField] private TextMeshProUGUI daysConversionLbl;
    [SerializeField] private Button scheduleManualBtn;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;

    [Space(10)]
    [Header("Combat Attributes")]
    [SerializeField] private Transform combatAttributeContentTransform;
    [SerializeField] private GameObject combatAttributePrefab;

    [Space(10)]
    [Header("Equipment")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image armorIcon;
    [SerializeField] private Image accessoryIcon;
    [SerializeField] private Image consumableIcon;

    private LogHistoryItem[] logHistoryItems;
    private ItemContainer[] inventoryItemContainers;

    private Character _activeCharacter;
    private Character _previousCharacter;

    //public Character currentlyShowingCharacter {
    //    get { return _data as Character; }
    //}
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

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        _activeCharacter = null;
        //UIManager.Instance.SetCoverState(false);
        //PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
        //PlayerUI.Instance.CollapseMinionHolder();
        //InteractionUI.Instance.HideInteractionUI();
    }
    public override void OpenMenu() {
        _previousCharacter = _activeCharacter;
        _activeCharacter = _data as Character;
        base.OpenMenu();
        UpdateCharacterInfo();
        //if (_activeCharacter.isBeingInspected || GameManager.Instance.inspectAll) {
            UpdateCombatAttributes();
        //} else {
        //    UpdateCombatAttributes(_activeCharacter.uiData);
        //}
        //if (_activeCharacter.isBeingInspected) {
        //    UpdateTagInfo(_activeCharacter.attributes);
        //} else {
        //    UpdateTagInfo(_activeCharacter.uiData.attributes);
        //}
        //UpdateTagInfo();
        //UpdateRelationshipInfo();
        //ShowAttackButton();
        //ShowReleaseButton();
        //CheckShowSnatchButton();
        //currentActionIcon.SetCharacter(_activeCharacter);
        //currentActionIcon.SetAction(_activeCharacter.currentParty.currentAction);
        //PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(_activeCharacter);
        //PlayerUI.Instance.UncollapseMinionHolder();
        //InteractionUI.Instance.OpenInteractionUI(_activeCharacter);
        historyScrollView.verticalNormalizedPosition = 1;
        //CheckIfMenuShouldBeHidden();
        //UIManager.Instance.SetCoverState(true);
    }
    //public override void ShowTooltip(GameObject objectHovered) {
    //    base.ShowTooltip(objectHovered);
    //    if(objectHovered == healthProgressBar.gameObject) {
    //        UIManager.Instance.ShowSmallInfo(_activeCharacter.currentHP + "/" + _activeCharacter.maxHP);
    //    } else if (objectHovered == manaProgressBar.gameObject) {
    //        UIManager.Instance.ShowSmallInfo(_activeCharacter.currentSP + "/" + _activeCharacter.maxSP);
    //    } 
    //    //else if (objectHovered == overallProgressBar.gameObject) {
    //    //    UIManager.Instance.ShowSmallInfo(_activeCharacter.role.happiness.ToString());
    //    //} else if (objectHovered == energyProgressBar.gameObject) {
    //    //    UIManager.Instance.ShowSmallInfo(_activeCharacter.role.energy.ToString());
    //    //} else if (objectHovered == fullnessProgressBar.gameObject) {
    //    //    UIManager.Instance.ShowSmallInfo(_activeCharacter.role.fullness.ToString());
    //    //} else if (objectHovered == funProgressBar.gameObject) {
    //    //    UIManager.Instance.ShowSmallInfo(_activeCharacter.role.fun.ToString());
    //    //} 
    //    //else if (objectHovered == prestigeProgressBar.gameObject) {
    //    //    UIManager.Instance.ShowSmallInfo(_activeCharacter.role.prestige.ToString());
    //    //} else if (objectHovered == sanityProgressBar.gameObject) {
    //    //    UIManager.Instance.ShowSmallInfo(_activeCharacter.role.sanity.ToString());
    //    //}
    //}
    #endregion

    public override void SetData(object data) {
        base.SetData(data);
        //if (isShowing) {
        //    UpdateCharacterInfo();
        //}
    }

    public void UpdateCharacterInfo() {
        if (_activeCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
        UpdateLocationInfo();
        UpdateItemInfo();
        UpdateAllHistoryInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeCharacter);
        //characterPortrait.SetBGState(false);
    }
    public void UpdateBasicInfo() {
        nameLbl.text = _activeCharacter.name;
        lvlClassLbl.text = _activeCharacter.raceClassName;
        //Disabler Thought
        if (_activeCharacter.doNotDisturb > 0) {
            Trait disablerTrait = _activeCharacter.GetTraitOf(TRAIT_TYPE.DISABLER);
            if (disablerTrait != null) {
                if (disablerTrait.thoughtText != null && disablerTrait.thoughtText != string.Empty) {
                    plansLbl.text = disablerTrait.thoughtText.Replace("[Character]", _activeCharacter.name);
                } else {
                    plansLbl.text = _activeCharacter.name + " has a disabler trait: " + disablerTrait.name + ".";
                }
                return;
            }
        }

        //Travel Thought
        if (!_activeCharacter.isDead && _activeCharacter.currentParty.icon.isTravelling) {
            plansLbl.text = _activeCharacter.name + " is travelling to " + _activeCharacter.currentParty.icon.targetLocation.name + ".";
            return;
        }

        //Planned Action
        if (_activeCharacter.currentAction != null) {
            if (_activeCharacter.currentAction.currentState == null) {
                plansLblLogItem.SetLog(_activeCharacter.currentAction.thoughtBubbleLog);
                plansLbl.text = Utilities.LogReplacer(_activeCharacter.currentAction.thoughtBubbleLog);
            } else {
                plansLblLogItem.SetLog(_activeCharacter.currentAction.currentState.descriptionLog);
                plansLbl.text = Utilities.LogReplacer(_activeCharacter.currentAction.currentState.descriptionLog);
            }
            //plansLbl.text = _activeCharacter.name + " does not have any immediate plans at the moment.";
            return;
        }

        //Default - Do nothing/Idle
        if (_activeCharacter.isAtHomeStructure) {
            plansLbl.text = _activeCharacter.name + " is at home.";
        } else {
            if (_activeCharacter.currentStructure.structureType == STRUCTURE_TYPE.DWELLING) {
                Dwelling dwelling = _activeCharacter.currentStructure as Dwelling;
                if (dwelling.residents.Count > 0) {
                    plansLbl.text = _activeCharacter.name + " is in " + dwelling.residents[0].name + "'s house.";
                } else {
                    plansLbl.text = _activeCharacter.name + " is in a dwelling.";
                }
            } else if (_activeCharacter.currentStructure.structureType == STRUCTURE_TYPE.WORK_AREA) {
                plansLbl.text = _activeCharacter.name + " is in " + _activeCharacter.currentStructure.location.name + ".";
            } else if (_activeCharacter.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                plansLbl.text = _activeCharacter.name + " is in the " + _activeCharacter.currentStructure.structureType.ToString().ToLower() + ".";
            } else if (_activeCharacter.currentStructure.structureType == STRUCTURE_TYPE.INN) {
                plansLbl.text = _activeCharacter.name + " is at the " + _activeCharacter.currentStructure.structureType.ToString().ToLower() + ".";
            } else if (_activeCharacter.currentStructure.structureType == STRUCTURE_TYPE.DUNGEON) {
                plansLbl.text = _activeCharacter.name + " is in a " + _activeCharacter.currentStructure.structureType.ToString().ToLower() + ".";
            } else if (_activeCharacter.currentStructure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
                plansLbl.text = _activeCharacter.name + " is in the " + _activeCharacter.currentStructure.structureType.ToString().ToLower() + ".";
            }
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

    #region Equipment
    private void UpdateEquipmentInfo() {
        if(_activeCharacter.equippedWeapon != null) {
            weaponIcon.gameObject.SetActive(true);
            weaponIcon.sprite = ItemManager.Instance.GetIconSprite(_activeCharacter.equippedWeapon.iconName);
        } else {
            weaponIcon.gameObject.SetActive(false);
        }

        if (_activeCharacter.equippedArmor != null) {
            armorIcon.gameObject.SetActive(true);
            armorIcon.sprite = ItemManager.Instance.GetIconSprite(_activeCharacter.equippedArmor.iconName);
        } else {
            armorIcon.gameObject.SetActive(false);
        }

        if (_activeCharacter.equippedAccessory != null) {
            accessoryIcon.gameObject.SetActive(true);
            accessoryIcon.sprite = ItemManager.Instance.GetIconSprite(_activeCharacter.equippedAccessory.iconName);
        } else {
            accessoryIcon.gameObject.SetActive(false);
        }

        if (_activeCharacter.equippedConsumable != null) {
            consumableIcon.gameObject.SetActive(true);
            consumableIcon.sprite = ItemManager.Instance.GetIconSprite(_activeCharacter.equippedConsumable.iconName);
        } else {
            consumableIcon.gameObject.SetActive(false);
        }
    }
    //private void UpdateEquipmentInfo(CharacterUIData uiData) {
    //    if (uiData.equippedWeapon != null) {
    //        weaponIcon.gameObject.SetActive(true);
    //        weaponIcon.sprite = ItemManager.Instance.GetIconSprite(uiData.equippedWeapon.iconName);
    //    } else {
    //        weaponIcon.gameObject.SetActive(false);
    //    }

    //    if (uiData.equippedArmor != null) {
    //        armorIcon.gameObject.SetActive(true);
    //        armorIcon.sprite = ItemManager.Instance.GetIconSprite(uiData.equippedArmor.iconName);
    //    } else {
    //        armorIcon.gameObject.SetActive(false);
    //    }

    //    if (uiData.equippedAccessory != null) {
    //        accessoryIcon.gameObject.SetActive(true);
    //        accessoryIcon.sprite = ItemManager.Instance.GetIconSprite(uiData.equippedAccessory.iconName);
    //    } else {
    //        accessoryIcon.gameObject.SetActive(false);
    //    }

    //    if (uiData.equippedConsumable != null) {
    //        consumableIcon.gameObject.SetActive(true);
    //        consumableIcon.sprite = ItemManager.Instance.GetIconSprite(uiData.equippedConsumable.iconName);
    //    } else {
    //        consumableIcon.gameObject.SetActive(false);
    //    }
    //}
    #endregion

    #region Combat Attributes
    private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(_activeCharacter == null || _activeCharacter != character) {
            return;
        }
        UpdateCombatAttributes();
    }
    private void UpdateCombatAttributes() {
        Utilities.DestroyChildren(combatAttributeContentTransform);
        for (int i = 0; i < _activeCharacter.traits.Count; i++) {
            CreateCombatAttributeGO(_activeCharacter.traits[i]);
        }
    }
    private void UpdateCombatAttributes(CharacterUIData uiData) {
        combatAttributeContentTransform.DestroyChildren();
        for (int i = 0; i < uiData.combatAttributes.Count; i++) {
            CreateCombatAttributeGO(uiData.combatAttributes[i]);
        }
    }
    private void CreateCombatAttributeGO(Trait combatAttribute) {
        GameObject go = GameObject.Instantiate(combatAttributePrefab, combatAttributeContentTransform);
        CombatAttributeItem combatAttributeItem = go.GetComponent<CombatAttributeItem>();
        combatAttributeItem.SetCombatAttribute(combatAttribute);
    }
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
    private void UpdateItemInfo() {
        //UpdateEquipmentInfo(_activeCharacter.equippedItems);
        //UpdateInventoryInfo(_activeCharacter.inventory);
        itemContainer.SetItem(_activeCharacter.tokenInInventory);
    }
    private void UpdateInventoryInfo(List<Item> inventory) {
        for (int i = 0; i < inventoryItemContainers.Length; i++) {
            ItemContainer currContainer = inventoryItemContainers[i];
            Item currInventoryItem = inventory.ElementAtOrDefault(i);
            //currContainer.SetItem(currInventoryItem);
        }
    }
    #endregion

    #region Relationships
    List<CharacterRelationshipItem> shownRelationships = new List<CharacterRelationshipItem>();
    private void UpdateRelationshipInfo(List<Relationship> relationships) {
        List<Relationship> relationshipsToShow = new List<Relationship>(relationships);
        List<CharacterRelationshipItem> relationshipsToRemove = new List<CharacterRelationshipItem>();
        for (int i = 0; i < shownRelationships.Count; i++) {
            CharacterRelationshipItem currRelItem = shownRelationships[i];
            if (relationshipsToShow.Contains(currRelItem.rel)) {
                relationshipsToShow.Remove(currRelItem.rel);
            } else {
                relationshipsToRemove.Add(currRelItem);
            }
        }

        for (int i = 0; i < relationshipsToRemove.Count; i++) {
            RemoveRelationship(relationshipsToRemove[i]);
        }

        //Utilities.DestroyChildren(relationsScrollView.content);
        for (int i = 0; i < relationshipsToShow.Count; i++) {
            AddRelationship(relationshipsToShow[i], i);
        }

        //int counter = 0;
        //foreach (KeyValuePair<Character, Relationship> kvp in _activeCharacter.relationships) {
        //    GameObject relItemGO = UIManager.Instance.InstantiateUIObject(relationshipItemPrefab.name, relationsScrollView.content);
        //    CharacterRelationshipItem relItem = relItemGO.GetComponent<CharacterRelationshipItem>();
        //    relItem.Initialize();
        //    if (Utilities.IsEven(counter)) {
        //        relItem.SetBGColor(evenRelationshipColor, oddRelationshipColor);
        //    } else {
        //        relItem.SetBGColor(oddRelationshipColor, evenRelationshipColor);
        //    }
        //    relItem.SetRelationship(kvp.Value);
        //    counter++;
        //}
    }
    private void AddRelationship(Relationship rel, int index) {
        GameObject relItemGO = UIManager.Instance.InstantiateUIObject(relationshipItemPrefab.name, relationsScrollView.content);
        CharacterRelationshipItem relItem = relItemGO.GetComponent<CharacterRelationshipItem>();
        Relationship currRel = rel;
        relItem.Initialize();
        if (Utilities.IsEven(index)) {
            relItem.SetBGColor(evenRelationshipColor, oddRelationshipColor);
        } else {
            relItem.SetBGColor(oddRelationshipColor, evenRelationshipColor);
        }
        relItem.SetRelationship(currRel);
        shownRelationships.Add(relItem);
    }
    private void RemoveRelationship(CharacterRelationshipItem item) {
        ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        shownRelationships.Remove(item);
    }
    #endregion

    #region History
    private void UpdateHistory(object obj) {
        if (obj is Character && _activeCharacter != null && (obj as Character).id == _activeCharacter.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> characterHistory = new List<Log>(_activeCharacter.history.OrderByDescending(x => x.id));
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

    #region Scheduling
    private void InitializeSchedulingMenu() {
        monthDropdown.ClearOptions();
        tickField.text = string.Empty;
        yearField.text = string.Empty;

        monthDropdown.AddOptions(new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"});
        //List<string> tickOptions = new List<string>();
        //for (int i = 1; i <= GameManager.hoursPerDay; i++) {
        //    tickOptions.Add(i.ToString());
        //}
        //tickField.AddOptions(tickOptions);
        UpdateDays(MONTH.JAN);
    }
    public void OnMonthChanged(int choice) {
        UpdateDays((MONTH)choice + 1);
    }
    private void UpdateDays(MONTH month) {
        dayDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 1; i <= GameManager.daysPerMonth; i++) {
            options.Add(i.ToString());
        }
        dayDropdown.AddOptions(options);
    }
    public void ValidateTicks(string value) {
        int tick = Int32.Parse(value);
        tick = Mathf.Clamp(tick, 1, GameManager.ticksPerDay);
        tickField.text = tick.ToString();
    }
    public void ValidateYear(string value) {
        int year = Int32.Parse(value);
        year = Mathf.Max(GameManager.Instance.year, year);
        yearField.text = year.ToString();
    }
    public void ScheduleManual() {
        int month = Int32.Parse(monthDropdown.options[monthDropdown.value].text);
        int day = Int32.Parse(dayDropdown.options[dayDropdown.value].text);
        int year = Int32.Parse(yearField.text);
        int hour = Int32.Parse(tickField.text);
        //TestEvent testEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.TEST_EVENT) as TestEvent;
        //testEvent.Initialize(new List<Character>() { _activeCharacter });
        //testEvent.ScheduleEvent(new GameDate(month, day, year, hour));
    }
    public void ScheduleAuto() {
        //TestEvent testEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.TEST_EVENT) as TestEvent;
        //testEvent.Initialize(new List<Character>() { _activeCharacter });
        //testEvent.ScheduleEvent();
    }
    //public void LogEventSchedule() {
    //    string text = _activeCharacter.name + "'s Event Schedule: \n";
    //    text += _activeCharacter.eventSchedule.GetEventScheduleSummary();
    //    Debug.Log(text);
    //}
    //private void Update() {
    //    int month;
    //    int day;
    //    int year;
    //    int hour;
    //    //daysConversionLbl.text = "Today is " + GameManager.Instance.Today().ToStringDate() + "Day Conversion: ";
    //    if (Int32.TryParse(monthDropdown.options[monthDropdown.value].text, out month) && 
    //        Int32.TryParse(dayDropdown.options[dayDropdown.value].text, out day) &&
    //        Int32.TryParse(yearField.text, out year) &&
    //        Int32.TryParse(tickField.text, out hour)) {
    //        GameDate date = new GameDate(month, day, year);
    //        daysConversionLbl.text = "Day Conversion: " + date.GetDayAndTicksString();
    //        if (date.IsBefore(GameManager.Instance.Today())) {
    //            //the specified schedule date is before today
    //            //do not allow
    //            scheduleManualBtn.interactable = false;
    //            daysConversionLbl.text += "(Invalid)";
    //        } else {
    //            if (!scheduleManualBtn.interactable) {
    //                scheduleManualBtn.interactable = true;
    //            }
    //        }
    //    } else {
    //        daysConversionLbl.text = "Day Conversion: Invalid";
    //        scheduleManualBtn.interactable = false;
    //    }
    //}
    #endregion

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
        _activeCharacter.LogAwarenessList();
    }
}
