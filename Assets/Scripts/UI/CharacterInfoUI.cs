using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;
using TMPro;
using UnityEngine.UI;
using System;

public class CharacterInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 20;
    private const int MAX_INVENTORY = 16;
    public bool isWaitingForAttackTarget;
    public bool isWaitingForJoinBattleTarget;

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private TextMeshProUGUI phaseLbl;
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private SquadEmblem squadEmblem;
    [SerializeField] private ActionIcon currentActionIcon;
    [SerializeField] private GameObject actionIconPrefab;
    [SerializeField] private string actionIconPrefabName;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private Slider healthProgressBar;
    [SerializeField] private Slider manaProgressBar;
    [SerializeField] private TextMeshProUGUI strengthLbl;
    [SerializeField] private TextMeshProUGUI agilityLbl;
    [SerializeField] private TextMeshProUGUI intelligenceLbl;
    [SerializeField] private TextMeshProUGUI vitalityLbl;

    [Space(10)]
    [Header("Tags")]
    [SerializeField] private ScrollRect tagsScrollView;
    [SerializeField] private GameObject characterTagPrefab;

    //[Space(10)]
    //[Header("Mood")]
    //[SerializeField] private GameObject moodMenuGO;
    //[SerializeField] private TextMeshProUGUI moodTabLbl;
    //[SerializeField] private Slider overallProgressBar;
    //[SerializeField] private Slider energyProgressBar;
    //[SerializeField] private Slider fullnessProgressBar;
    //[SerializeField] private Slider funProgressBar;
    //[SerializeField] private Slider prestigeProgressBar;
    //[SerializeField] private Slider sanityProgressBar;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private GameObject itemsMenuGO;
    [SerializeField] private ItemContainer headArmorContainer;
    [SerializeField] private ItemContainer leftHandContainer;
    [SerializeField] private ItemContainer rightHandContainer;
    [SerializeField] private ItemContainer chestArmorContainer;
    [SerializeField] private ItemContainer legArmorContainer;
    [SerializeField] private ItemContainer leftFootArmorContainer;
    [SerializeField] private ItemContainer rightFootArmorContainer;
    [SerializeField] private RectTransform inventoryItemContainersParent;

    [Space(10)]
    [Header("Relations")]
    [SerializeField] private GameObject relationsGO;
    [SerializeField] private ScrollRect relationsScrollView;
    [SerializeField] private GameObject relationshipItemPrefab;
    [SerializeField] private Color evenRelationshipColor;
    [SerializeField] private Color oddRelationshipColor;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private SecretItem[] secretItems;
    [SerializeField] private IntelItem[] intelItems;
    [SerializeField] private HiddenDesireItem hiddenDesireItem;

    //[Space(10)]
    //[Header("Content")]
    //[SerializeField] private TextMeshProUGUI statInfoLbl;
    //[SerializeField] private TextMeshProUGUI traitInfoLbl;
    //[SerializeField] private TextMeshProUGUI equipmentInfoLbl;
    //[SerializeField] private TextMeshProUGUI inventoryInfoLbl;
    //[SerializeField] private TextMeshProUGUI relationshipsLbl;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Character")]
    [SerializeField] private GameObject attackButtonGO;
    [SerializeField] private Toggle attackBtnToggle;
    [SerializeField] private GameObject joinBattleButtonGO;
    [SerializeField] private Toggle joinBattleBtnToggle;
    [SerializeField] private Button snatchBtn;
    [SerializeField] private GameObject releaseBtnGO;

    [Space(10)]
    [Header("Scheduling")]
    [SerializeField] private Dropdown monthDropdown;
    [SerializeField] private Dropdown dayDropdown;
    [SerializeField] private InputField yearField;
    [SerializeField] private InputField tickField;
    [SerializeField] private TextMeshProUGUI daysConversionLbl;
    [SerializeField] private Button scheduleManualBtn;


    private LogHistoryItem[] logHistoryItems;
    private ItemContainer[] inventoryItemContainers;

    private Character _activeCharacter;

    internal Character currentlyShowingCharacter {
        get { return _data as Character; }
    }

    internal Character activeCharacter {
        get { return _activeCharacter; }
    }

    internal override void Initialize() {
        base.Initialize();
        isWaitingForAttackTarget = false;
        //Messenger.AddListener(Signals.UPDATE_UI, UpdateCharacterInfo);
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener<BaseLandmark>(Signals.PLAYER_LANDMARK_CREATED, OnPlayerLandmarkCreated);
        //Messenger.AddListener<ActionQueueItem, Character>(Signals.ACTION_ADDED_TO_QUEUE, OnActionAddedToQueue);
        //Messenger.AddListener<ActionQueueItem, Character>(Signals.ACTION_REMOVED_FROM_QUEUE, OnActionRemovedFromQueue);
        //Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);
        Messenger.AddListener<Character, Attribute>(Signals.ATTRIBUTE_ADDED, OnCharacterAttributeAdded);
        Messenger.AddListener<Character, Attribute>(Signals.ATTRIBUTE_REMOVED, OnCharacterAttributeRemoved);
        //affiliations.Initialize();
        currentActionIcon.Initialize();
        //Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        InititalizeLogsMenu();
        InititalizeInventoryMenu();
        InitializeSchedulingMenu();
    }
    private void InititalizeLogsMenu() {
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
    private void InititalizeInventoryMenu() {
        inventoryItemContainers = Utilities.GetComponentsInDirectChildren<ItemContainer>(inventoryItemContainersParent.gameObject);
    }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        _activeCharacter = null;
        PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
    }
    public override void OpenMenu() {
        base.OpenMenu();
        _activeCharacter = (Character)_data;
        UpdateCharacterInfo();
        UpdateTagInfo();
        UpdateRelationshipInfo();
        ShowAttackButton();
        ShowReleaseButton();
        CheckShowSnatchButton();
        currentActionIcon.SetCharacter(currentlyShowingCharacter);
        currentActionIcon.SetAction(currentlyShowingCharacter.currentParty.currentAction);
        PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(currentlyShowingCharacter);
    }
    public override void ShowTooltip(GameObject objectHovered) {
        base.ShowTooltip(objectHovered);
        if(objectHovered == healthProgressBar.gameObject) {
            UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.currentHP + "/" + currentlyShowingCharacter.maxHP);
        } else if (objectHovered == manaProgressBar.gameObject) {
            UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.currentSP + "/" + currentlyShowingCharacter.maxSP);
        } 
        //else if (objectHovered == overallProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.happiness.ToString());
        //} else if (objectHovered == energyProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.energy.ToString());
        //} else if (objectHovered == fullnessProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.fullness.ToString());
        //} else if (objectHovered == funProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.fun.ToString());
        //} 
        //else if (objectHovered == prestigeProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.prestige.ToString());
        //} else if (objectHovered == sanityProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.sanity.ToString());
        //}
    }
    #endregion

    //private void OnCharacterDied(ECS.Character deadCharacter) {
    //    if (isShowing && currentlyShowingCharacter != null && currentlyShowingCharacter.id == deadCharacter.id) {
    //        SetData(null);
    //        CloseMenu();
    //    }
    //}

    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdateCharacterInfo();
        }
    }

    public void UpdateCharacterInfo() {
        if (currentlyShowingCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        //UpdateGeneralInfo();
        UpdateInfoMenu();
        UpdateStatInfo();
        //UpdateMoodInfo();
        UpdateItemsInfo();
        //UpdateActionQueue();
        //UpdateEquipmentInfo();
        //UpdateInventoryInfo();
        UpdateAllHistoryInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(currentlyShowingCharacter, 100, true);
        characterPortrait.SetBGState(false);
    }
    private void UpdateBasicInfo() {
        nameLbl.text = currentlyShowingCharacter.name;
        if (currentlyShowingCharacter.isBeingInspected) {
            lvlClassLbl.text = "Lvl." + currentlyShowingCharacter.level.ToString() + " " + currentlyShowingCharacter.characterClass.className;
        } else {
            if (!currentlyShowingCharacter.hasBeenInspected) {
                lvlClassLbl.text = "???";
            }
        }
        phaseLbl.text = currentlyShowingCharacter.dailySchedule.currentPhase.phaseName;
        squadEmblem.SetSquad(currentlyShowingCharacter.squad);
        factionEmblem.SetFaction(currentlyShowingCharacter.faction);
        //affiliations.SetCharacter(currentlyShowingCharacter);
    }
    //private void UpdateActionQueue() {
    //    Utilities.DestroyChildren(actionQueueScrollView.content);
    //    for (int i = 0; i < currentlyShowingCharacter.actionQueue.Count; i++) {
    //        ActionQueueItem queueItem = currentlyShowingCharacter.actionQueue.GetBasedOnIndex(i);
    //        GameObject actionItemGO = UIManager.Instance.InstantiateUIObject(actionIconPrefab.name, actionQueueScrollView.content);
    //        ActionIcon actionItem = actionItemGO.GetComponent<ActionIcon>();
    //        actionItem.Initialize();
    //        actionItem.SetCharacter(currentlyShowingCharacter);
    //        actionItem.SetAction(queueItem.action);
    //        actionItem.SetAlpha(128f/255f);
    //    }
    //}

    private void UpdateStatInfo() {
        healthProgressBar.value = (float)currentlyShowingCharacter.currentHP / (float) currentlyShowingCharacter.maxHP;
        manaProgressBar.value = (float) currentlyShowingCharacter.currentSP / (float) currentlyShowingCharacter.maxSP;
        strengthLbl.text = currentlyShowingCharacter.strength.ToString();
        agilityLbl.text = currentlyShowingCharacter.agility.ToString();
        intelligenceLbl.text = currentlyShowingCharacter.intelligence.ToString();
        vitalityLbl.text = currentlyShowingCharacter.vitality.ToString();
    }
    //private void UpdateMoodInfo() {
    //    overallProgressBar.value = currentlyShowingCharacter.role.happiness;
    //    energyProgressBar.value = currentlyShowingCharacter.role.energy;
    //    fullnessProgressBar.value = currentlyShowingCharacter.role.fullness;
    //    funProgressBar.value = currentlyShowingCharacter.role.fun;
    //    //prestigeProgressBar.value = currentlyShowingCharacter.role.prestige;
    //    //sanityProgressBar.value = currentlyShowingCharacter.role.sanity;
    //}
    private void UpdateItemsInfo() {
        UpdateEquipmentInfo();
        UpdateInventoryInfo();
    }
    private void UpdateEquipmentInfo() {
        //Equipment
        IBodyPart head = currentlyShowingCharacter.GetBodyPart("Head");
        if (head != null) {
            Item headArmor = head.GetArmor();
            if (headArmor != null) {
                headArmorContainer.SetItem(headArmor);
            } else {
                headArmorContainer.SetItem(null);
            }
        }

        IBodyPart torso = currentlyShowingCharacter.GetBodyPart("Torso");
        if (torso != null) {
            Item torsoArmor = torso.GetArmor();
            if (torsoArmor != null) {
                chestArmorContainer.SetItem(torsoArmor);
            } else {
                chestArmorContainer.SetItem(null);
            }
        }

        IBodyPart leftHand = currentlyShowingCharacter.GetBodyPart("Left Hand");
        if (leftHand != null) {
            Item leftHandWeapon = leftHand.GetWeapon();
            if (leftHandWeapon != null) {
                leftHandContainer.SetItem(leftHandWeapon);
            } else {
                leftHandContainer.SetItem(null);
            }
        }

        IBodyPart rightHand = currentlyShowingCharacter.GetBodyPart("Right Hand");
        if (rightHand != null) {
            Item rightHandWeapon = rightHand.GetWeapon();
            if (rightHandWeapon != null) {
                rightHandContainer.SetItem(rightHandWeapon);
            } else {
                rightHandContainer.SetItem(null);
            }
        }

        IBodyPart hips = currentlyShowingCharacter.GetBodyPart("Hip");
        if (hips != null) {
            Item hipArmor = hips.GetArmor();
            if (hipArmor != null) {
                legArmorContainer.SetItem(hipArmor);
            } else {
                legArmorContainer.SetItem(null);
            }
        }

        IBodyPart leftFoot = currentlyShowingCharacter.GetBodyPart("Left Foot");
        if (leftFoot != null) {
            Item footArmor = leftFoot.GetArmor();
            if (footArmor != null) {
                leftFootArmorContainer.SetItem(footArmor);
            } else {
                leftFootArmorContainer.SetItem(null);
            }
        }

        IBodyPart rightFoot = currentlyShowingCharacter.GetBodyPart("Right Foot");
        if (leftFoot != null) {
            Item footArmor = rightFoot.GetArmor();
            if (footArmor != null) {
                rightFootArmorContainer.SetItem(footArmor);
            } else {
                rightFootArmorContainer.SetItem(null);
            }
        }
    }
    private void UpdateInventoryInfo() {
        for (int i = 0; i < inventoryItemContainers.Length; i++) {
            ItemContainer currContainer = inventoryItemContainers[i];
            Item currInventoryItem = currentlyShowingCharacter.inventory.ElementAtOrDefault(i);
            currContainer.SetItem(currInventoryItem);
        }
    }
    //private void UpdateEquipmentInfo() {
    //    string text = string.Empty;
    //    if (currentlyShowingCharacter.equippedItems.Count > 0) {
    //        for (int i = 0; i < currentlyShowingCharacter.equippedItems.Count; i++) {
    //            Item item = currentlyShowingCharacter.equippedItems[i];
    //            if (i > 0) {
    //                text += "\n";
    //            }
    //            text += item.itemName;
    //            if (item is Weapon) {
    //                Weapon weapon = (Weapon)item;
    //                if (weapon.bodyPartsAttached.Count > 0) {
    //                    text += " (";
    //                    for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
    //                        if (j > 0) {
    //                            text += ", ";
    //                        }
    //                        text += weapon.bodyPartsAttached[j].name;
    //                    }
    //                    text += ")";
    //                }
    //            } else if (item is Armor) {
    //                Armor armor = (Armor)item;
    //                text += " (" + armor.bodyPartAttached.name + ")";
    //            }
    //        }
    //    } else {
    //        text += "NONE";
    //    }
    //    equipmentInfoLbl.text = text;
    //}

    //private void UpdateInventoryInfo() {
    //    string text = string.Empty;
    //    CharacterObj obj = currentlyShowingCharacter.party.characterObject as CharacterObj;
    //    foreach (RESOURCE resource in obj.resourceInventory.Keys) {
    //        text += resource.ToString() + ": " + obj.resourceInventory[resource];
    //        text += "\n";
    //    }
    //    inventoryInfoLbl.text = text;
    //}

    private void UpdateRelationshipInfo() {
        Utilities.DestroyChildren(relationsScrollView.content);
        int counter = 0;
        foreach (KeyValuePair<Character, Relationship> kvp in currentlyShowingCharacter.relationships) {
            GameObject relItemGO = UIManager.Instance.InstantiateUIObject(relationshipItemPrefab.name, relationsScrollView.content);
            CharacterRelationshipItem relItem = relItemGO.GetComponent<CharacterRelationshipItem>();
            relItem.Initialize();
            if (Utilities.IsEven(counter)) {
                relItem.SetBGColor(evenRelationshipColor, oddRelationshipColor);
            } else {
                relItem.SetBGColor(oddRelationshipColor, evenRelationshipColor);
            }
            relItem.SetRelationship(kvp.Value);
            counter++;
        }
    }

    #region Character Tags
    private void UpdateTagInfo() {
        Utilities.DestroyChildren(tagsScrollView.content);
        for (int i = 0; i < currentlyShowingCharacter.attributes.Count; i++) {
            Attribute currTag = currentlyShowingCharacter.attributes[i];
            AddTag(currTag);
        }
    }
    private void AddTag(Attribute tag) {
        GameObject tagGO = UIManager.Instance.InstantiateUIObject(characterTagPrefab.name, tagsScrollView.content);
        tagGO.GetComponent<CharacterAttributeIcon>().SetTag(tag);
    }
    private void RemoveTag(Attribute tag) {
        CharacterAttributeIcon[] icons = Utilities.GetComponentsInDirectChildren<CharacterAttributeIcon>(tagsScrollView.content.gameObject);
        for (int i = 0; i < icons.Length; i++) {
            CharacterAttributeIcon icon = icons[i];
            if (icon.attribute == tag) {
                ObjectPoolManager.Instance.DestroyObject(icon.gameObject);
                break;
            }
        }
    }
    private void OnCharacterAttributeAdded(Character affectedCharacter, Attribute tag) {
        if (currentlyShowingCharacter != null && currentlyShowingCharacter.id == affectedCharacter.id) {
            AddTag(tag);
        }
    }
    private void OnCharacterAttributeRemoved(Character affectedCharacter, Attribute tag) {
        if (currentlyShowingCharacter != null && currentlyShowingCharacter.id == affectedCharacter.id) {
            RemoveTag(tag);
        }
    }
    #endregion

    #region Utilities
    //public void UpdateMoodColor(bool isOn) {
    //    if (isOn) {
    //        moodTabLbl.color = oddLogColor;
    //    } else {
    //        moodTabLbl.color = Color.white;
    //    }
    //}
    //public void UpdateItemsColor(bool isOn) {
    //    if (isOn) {
    //        itemsTabLbl.color = oddLogColor;
    //    } else {
    //        itemsTabLbl.color = Color.white;
    //    }
    //}
    //public void UpdateRelationsColor(bool isOn) {
    //    if (isOn) {
    //        relationsTabLbl.color = oddLogColor;
    //    } else {
    //        relationsTabLbl.color = Color.white;
    //    }
    //}
    //public void UpdateLogsColor(bool isOn) {
    //    if (isOn) {
    //        logsTabLbl.color = oddLogColor;
    //    } else {
    //        logsTabLbl.color = Color.white;
    //    }
    //}
    #endregion

    #region Action Queue
    //private void OnActionAddedToQueue(ActionQueueItem actionAdded, Character character) {
    //    if (currentlyShowingCharacter != null && currentlyShowingCharacter.id == character.id) {
    //        GameObject actionItemGO = UIManager.Instance.InstantiateUIObject(actionIconPrefabName, actionQueueScrollView.content);
    //        ActionIcon actionItem = actionItemGO.GetComponent<ActionIcon>();
    //        actionItem.Initialize();
    //        actionItem.SetCharacter(currentlyShowingCharacter);
    //        actionItem.SetAction(actionAdded.action);
    //    }
    //}
    //private void OnActionRemovedFromQueue(ActionQueueItem actionAdded, Character character) {
    //    if (currentlyShowingCharacter != null && currentlyShowingCharacter.id == character.id) {
    //        ActionIcon icon = GetActionIcon(actionAdded.action);
    //        ObjectPoolManager.Instance.DestroyObject(icon.gameObject);
    //    }
    //}
    //private ActionIcon GetActionIcon(CharacterAction action) {
    //    ActionIcon[] icons = Utilities.GetComponentsInDirectChildren<ActionIcon>(actionQueueScrollView.content.gameObject);
    //    for (int i = 0; i < icons.Length; i++) {
    //        ActionIcon currIcon = icons[i];
    //        if (currIcon.action == action) {
    //            return currIcon;
    //        }
    //    }
    //    return null;
    //}
    //private void OnActionTaken(CharacterAction takenAction, CharacterParty party) {
    //    if (currentlyShowingCharacter != null && currentlyShowingCharacter.currentParty.id == party.id) {
    //        currentActionIcon.SetAction(takenAction);
    //    }
    //}
    #endregion

    #region History
    private void UpdateHistory(object obj) {
        if (obj is Character && currentlyShowingCharacter != null && (obj as Character).id == currentlyShowingCharacter.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> characterHistory = new List<Log>(currentlyShowingCharacter.history.OrderByDescending(x => x.id));
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
        return (isShowing && currentlyShowingCharacter == character);
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

    #region Release Character
    public void ShowReleaseButton() {
        if (currentlyShowingCharacter.party != null && currentlyShowingCharacter.party.characterObject.currentState.stateName == "Imprisoned") {
            releaseBtnGO.SetActive(true);
        } else {
            releaseBtnGO.SetActive(false);
        }
    }
    public void ReleaseCharacter() {
        CharacterAction action = currentlyShowingCharacter.party.characterObject.currentState.GetAction(ACTION_TYPE.RELEASE);
        ReleaseAction releaseAction = action as ReleaseAction;
        releaseAction.ReleaseCharacter(currentlyShowingCharacter.party.characterObject);
    }
    #endregion

    private void OnPlayerLandmarkCreated(BaseLandmark createdLandmark) {
        if (createdLandmark.specificLandmarkType == LANDMARK_TYPE.SNATCHER_DEMONS_LAIR) {
            CheckShowSnatchButton();
        }
    }

    #region Snatch
    public void CheckShowSnatchButton() {
        if (!PlayerManager.Instance.CanSnatch() || PlayerManager.Instance.player.IsCharacterSnatched(currentlyShowingCharacter)) {
            snatchBtn.gameObject.SetActive(false);
        } else {
            snatchBtn.gameObject.SetActive(true);
        }
    }
    public void Snatch() {
        PlayerManager.Instance.player.SnatchCharacter(currentlyShowingCharacter);
        CheckShowSnatchButton();
    }
    #endregion

    #region Level Up
    public void LevelUpCharacter() {
        currentlyShowingCharacter.LevelUp();
    }
    #endregion

    #region Death
    public void DieCharacter() {
        if(currentlyShowingCharacter.party.currentCombat != null) {
            currentlyShowingCharacter.party.currentCombat.CharacterDeath(currentlyShowingCharacter, null);
        }
        currentlyShowingCharacter.Death();
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
        for (int i = 1; i <= GameManager.daysInMonth[(int)month]; i++) {
            options.Add(i.ToString());
        }
        dayDropdown.AddOptions(options);
    }
    public void ValidateHour(string value) {
        int hour = Int32.Parse(value);
        hour = Mathf.Clamp(hour, 1, GameManager.hoursPerDay);
        tickField.text = hour.ToString();
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
        TestEvent testEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.TEST_EVENT) as TestEvent;
        testEvent.Initialize(new List<Character>() { currentlyShowingCharacter });
        testEvent.ScheduleEvent(new GameDate(month, day, year, hour));
    }
    public void ScheduleAuto() {
        TestEvent testEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.TEST_EVENT) as TestEvent;
        testEvent.Initialize(new List<Character>() { currentlyShowingCharacter });
        testEvent.ScheduleEvent();
    }
    public void LogEventSchedule() {
        string text = currentlyShowingCharacter.name + "'s Event Schedule: \n";
        text += currentlyShowingCharacter.eventSchedule.GetEventScheduleSummary();
        Debug.Log(text);
    }
    private void Update() {
        int month;
        int day;
        int year;
        int hour;
        //daysConversionLbl.text = "Today is " + GameManager.Instance.Today().ToStringDate() + "Day Conversion: ";
        if (Int32.TryParse(monthDropdown.options[monthDropdown.value].text, out month) && 
            Int32.TryParse(dayDropdown.options[dayDropdown.value].text, out day) &&
            Int32.TryParse(yearField.text, out year) &&
            Int32.TryParse(tickField.text, out hour)) {
            GameDate date = new GameDate(month, day, year, hour);
            daysConversionLbl.text = "Day Conversion: " + date.GetDayAndTicksString();
            if (date.IsBefore(GameManager.Instance.Today())) {
                //the specified schedule date is before today
                //do not allow
                scheduleManualBtn.interactable = false;
                daysConversionLbl.text += "(Invalid)";
            } else {
                if (!scheduleManualBtn.interactable) {
                    scheduleManualBtn.interactable = true;
                }
            }
        } else {
            daysConversionLbl.text = "Day Conversion: Invalid";
            scheduleManualBtn.interactable = false;
        }
    }
    #endregion

    #region Info
    private void UpdateInfoMenu() {
        for (int i = 0; i < secretItems.Length; i++) {
            SecretItem currItem = secretItems[i];
            Secret currSecret = currentlyShowingCharacter.secrets.ElementAtOrDefault(i);
            if (currSecret == null) {
                currItem.gameObject.SetActive(false);
            } else {
                currItem.SetSecret(currSecret);
                currItem.gameObject.SetActive(true);
            }
        }
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.gameObject.SetActive(false);
            //Intel currSecret = currentlyShowingCharacter.intelReactions.ElementAtOrDefault(i);
            //if (currSecret == null) {
            //    currItem.gameObject.SetActive(false);
            //} else {
            //    currItem.SetSecret(currSecret);
            //    currItem.gameObject.SetActive(true);
            //}
        }
        if (currentlyShowingCharacter.hiddenDesire == null) {
            hiddenDesireItem.gameObject.SetActive(false);
        } else {
            hiddenDesireItem.SetHiddenDesire(currentlyShowingCharacter.hiddenDesire);
            hiddenDesireItem.gameObject.SetActive(true);
        }
    }
    #endregion
}
