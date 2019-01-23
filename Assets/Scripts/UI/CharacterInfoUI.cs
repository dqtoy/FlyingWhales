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
    [SerializeField] private FactionEmblem factionEmblem;
    [SerializeField] private PartyEmblem partyEmblem;
    //[SerializeField] private ActionIconCharacterInfoUI currentActionIcon;
    //[SerializeField] private GameObject actionIconPrefab;
    //[SerializeField] private string actionIconPrefabName;

    //[Space(10)]
    //[Header("Stats")]
    //[SerializeField] private Slider healthProgressBar;
    //[SerializeField] private Slider manaProgressBar;
    //[SerializeField] private TextMeshProUGUI strengthLbl;
    //[SerializeField] private TextMeshProUGUI agilityLbl;
    //[SerializeField] private TextMeshProUGUI intelligenceLbl;
    //[SerializeField] private TextMeshProUGUI vitalityLbl;
    //[SerializeField] private ScrollRect tagsScrollView;
    //[SerializeField] private GameObject characterTagPrefab;

    [Space(10)]
    [Header("Item And Location")]
    [SerializeField] private LocationPortrait visitorLocationPortrait;
    [SerializeField] private LocationPortrait residentLocationPortrait;
    [SerializeField] private ItemContainer itemContainer;

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
    [SerializeField] private GameObject itemsMenuCover;

    [Space(10)]
    [Header("Relations")]
    [SerializeField] private GameObject relationsGO;
    [SerializeField] private ScrollRect relationsScrollView;
    [SerializeField] private GameObject relationshipItemPrefab;
    [SerializeField] private Color evenRelationshipColor;
    [SerializeField] private Color oddRelationshipColor;
    [SerializeField] private GameObject relationsMenuCover;

    //[Space(10)]
    //[Header("Info")]
    //[SerializeField] private SecretItem[] secretItems;
    //[SerializeField] private TokenItem[] tokenItems;
    //[SerializeField] private HiddenDesireItem hiddenDesireItem;
    //[SerializeField] private GameObject infoMenuCover;

    //[Space(10)]
    //[Header("Content")]
    //[SerializeField] private TextMeshProUGUI statInfoLbl;
    //[SerializeField] private TextMeshProUGUI traitInfoLbl;
    //[SerializeField] private TextMeshProUGUI equipmentInfoLbl;
    //[SerializeField] private TextMeshProUGUI inventoryInfoLbl;
    //[SerializeField] private TextMeshProUGUI relationshipsLbl;

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
        Messenger.AddListener<Character>(Signals.TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character>(Signals.TRAIT_REMOVED, UpdateTraitsFromSignal);

        //Messenger.AddListener<ActionQueueItem, Character>(Signals.ACTION_ADDED_TO_QUEUE, OnActionAddedToQueue);
        //Messenger.AddListener<ActionQueueItem, Character>(Signals.ACTION_REMOVED_FROM_QUEUE, OnActionRemovedFromQueue);
        //Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_TAKEN, OnActionTaken);

        //Messenger.AddListener<Character, Attribute>(Signals.ATTRIBUTE_ADDED, OnCharacterAttributeAdded);
        //Messenger.AddListener<Character, Attribute>(Signals.ATTRIBUTE_REMOVED, OnCharacterAttributeRemoved);
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        //affiliations.Initialize();
        //currentActionIcon.Initialize();
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        InitializeLogsMenu();
        //InititalizeInventoryMenu();
        //InitializeSchedulingMenu();
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
    private void InitializeInventoryMenu() {
        inventoryItemContainers = Utilities.GetComponentsInDirectChildren<ItemContainer>(inventoryItemContainersParent.gameObject);
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

    private void SetCoversState(bool state) {
        //infoMenuCover.SetActive(state);
        itemsMenuCover.SetActive(state);
        relationsMenuCover.SetActive(state);
        //logsMenuCover.SetActive(state);
    }

    //private void OnCharacterDied(Character deadCharacter) {
    //    if (isShowing && _activeCharacter != null && _activeCharacter.id == deadCharacter.id) {
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
        if (_activeCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
        UpdateLocationInfo();
        UpdateItemInfo();
        UpdateAllHistoryInfo();

        //if (GameManager.Instance.inspectAll) {
        //SetCoversState(false);
        //} else {
        //    //if the character has never been inspected
        //    //activate all the tab menu covers
        //    //else, disable all the tab menu covers
        //    SetCoversState(!_activeCharacter.hasBeenInspected);
        //}


        //UpdateInfoMenu();

        //if (_activeCharacter.isBeingInspected || GameManager.Instance.inspectAll) {
        //UpdateStatInfo();
        //UpdateEquipmentInfo();
        //UpdateItemsInfo();
        //UpdateTagInfo(_activeCharacter.attributes);
        //UpdateRelationshipInfo(_activeCharacter.relationships.Values.ToList());
        //} else {
        //    //UpdateStatInfo(_activeCharacter.uiData);
        //    UpdateEquipmentInfo(_activeCharacter.uiData);
        //    //UpdateItemsInfo(_activeCharacter.uiData);
        //    //UpdateTagInfo(_activeCharacter.uiData.attributes);
        //    //UpdateRelationshipInfo(_activeCharacter.uiData.relationships);
        //}

        //UpdateGeneralInfo();
        //UpdateMoodInfo();

        //UpdateActionQueue();
        //UpdateEquipmentInfo();
        //UpdateInventoryInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeCharacter);
        //characterPortrait.SetBGState(false);
    }
    private void UpdateBasicInfo() {
        nameLbl.text = _activeCharacter.name;
//#if UNITY_EDITOR
//        nameLbl.text += " (" + _activeCharacter.currentInteractionTick.ToString() + ")";
//#endif
        lvlClassLbl.text = _activeCharacter.raceClassName;

        //if (_activeCharacter.schedule != null) {
        //    phaseLbl.text = _activeCharacter.schedule.currentPhase.ToString();
        //    phaseLbl.gameObject.SetActive(true);
        //} else {
        //    phaseLbl.gameObject.SetActive(false);
        //}
        //if (_activeCharacter.IsInParty()) { //if was added to prevent showing the emblem if the character is only in a party with 1 character
        //    partyEmblem.SetParty(_activeCharacter.currentParty);
        //} else {
        //    partyEmblem.SetParty(null);
        //}
                //factionEmblem.SetFaction(_activeCharacter.faction);
        //affiliations.SetCharacter(_activeCharacter);
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
        visitorLocationPortrait.SetLocation(_activeCharacter.specificLocation.tileLocation.areaOfTile);
        residentLocationPortrait.SetLocation(_activeCharacter.homeLandmark.tileLocation.areaOfTile);
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
    private void UpdateTraitsFromSignal(Character character) {
        if(_activeCharacter == null || _activeCharacter != character) {
            return;
        }
        UpdateCombatAttributes();
    }
    private void UpdateCombatAttributes() {
        combatAttributeContentTransform.DestroyChildren();
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
        logParentGO.SetActive(true);
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
    private void UpdateItemsInfo(CharacterUIData uiData) {
        //UpdateEquipmentInfo(uiData.equippedItems);
        UpdateInventoryInfo(uiData.inventory);
    }
    //private void UpdateEquipmentInfo(List<Item> equipment) {
    //    headArmorContainer.SetItem(null);
    //    chestArmorContainer.SetItem(null);
    //    legArmorContainer.SetItem(null);
    //    leftFootArmorContainer.SetItem(null);
    //    rightFootArmorContainer.SetItem(null);
        ////Equipment
        //IBodyPart head = _activeCharacter.GetBodyPart("Head");
        //if (head != null) {
        //    Item headArmor = head.GetArmor();
        //    if (headArmor != null) {
        //        headArmorContainer.SetItem(headArmor);
        //    } else {
        //        headArmorContainer.SetItem(null);
        //    }
        //}

        //IBodyPart torso = _activeCharacter.GetBodyPart("Torso");
        //if (torso != null) {
        //    Item torsoArmor = torso.GetArmor();
        //    if (torsoArmor != null) {
        //        chestArmorContainer.SetItem(torsoArmor);
        //    } else {
        //        chestArmorContainer.SetItem(null);
        //    }
        //}

        //IBodyPart leftHand = _activeCharacter.GetBodyPart("Left Hand");
        //if (leftHand != null) {
        //    Item leftHandWeapon = leftHand.GetWeapon();
        //    if (leftHandWeapon != null) {
        //        leftHandContainer.SetItem(leftHandWeapon);
        //    } else {
        //        leftHandContainer.SetItem(null);
        //    }
        //}

        //IBodyPart rightHand = _activeCharacter.GetBodyPart("Right Hand");
        //if (rightHand != null) {
        //    Item rightHandWeapon = rightHand.GetWeapon();
        //    if (rightHandWeapon != null) {
        //        rightHandContainer.SetItem(rightHandWeapon);
        //    } else {
        //        rightHandContainer.SetItem(null);
        //    }
        //}

        //IBodyPart hips = _activeCharacter.GetBodyPart("Hip");
        //if (hips != null) {
        //    Item hipArmor = hips.GetArmor();
        //    if (hipArmor != null) {
        //        legArmorContainer.SetItem(hipArmor);
        //    } else {
        //        legArmorContainer.SetItem(null);
        //    }
        //}

        //IBodyPart leftFoot = _activeCharacter.GetBodyPart("Left Foot");
        //if (leftFoot != null) {
        //    Item footArmor = leftFoot.GetArmor();
        //    if (footArmor != null) {
        //        leftFootArmorContainer.SetItem(footArmor);
        //    } else {
        //        leftFootArmorContainer.SetItem(null);
        //    }
        //}

        //IBodyPart rightFoot = _activeCharacter.GetBodyPart("Right Foot");
        //if (leftFoot != null) {
        //    Item footArmor = rightFoot.GetArmor();
        //    if (footArmor != null) {
        //        rightFootArmorContainer.SetItem(footArmor);
        //    } else {
        //        rightFootArmorContainer.SetItem(null);
        //    }
        //}
    //}
    private void UpdateInventoryInfo(List<Item> inventory) {
        for (int i = 0; i < inventoryItemContainers.Length; i++) {
            ItemContainer currContainer = inventoryItemContainers[i];
            Item currInventoryItem = inventory.ElementAtOrDefault(i);
            //currContainer.SetItem(currInventoryItem);
        }
    }
    #endregion

    //private void UpdateMoodInfo() {
    //    overallProgressBar.value = _activeCharacter.role.happiness;
    //    energyProgressBar.value = _activeCharacter.role.energy;
    //    fullnessProgressBar.value = _activeCharacter.role.fullness;
    //    funProgressBar.value = _activeCharacter.role.fun;
    //    //prestigeProgressBar.value = _activeCharacter.role.prestige;
    //    //sanityProgressBar.value = _activeCharacter.role.sanity;
    //}

    //private void UpdateEquipmentInfo() {
    //    string text = string.Empty;
    //    if (_activeCharacter.equippedItems.Count > 0) {
    //        for (int i = 0; i < _activeCharacter.equippedItems.Count; i++) {
    //            Item item = _activeCharacter.equippedItems[i];
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
    //    CharacterObj obj = _activeCharacter.party.characterObject as CharacterObj;
    //    foreach (RESOURCE resource in obj.resourceInventory.Keys) {
    //        text += resource.ToString() + ": " + obj.resourceInventory[resource];
    //        text += "\n";
    //    }
    //    inventoryInfoLbl.text = text;
    //}

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

    #region Character Tags
    private List<CharacterAttributeIcon> shownAttributes = new List<CharacterAttributeIcon>();
    private void UpdateTagInfo(List<CharacterAttribute> attributes) {
        List<CharacterAttribute> attributesToShow = new List<CharacterAttribute>(attributes);
        //Utilities.DestroyChildren(tagsScrollView.content);
        List<CharacterAttributeIcon> iconsToRemove = new List<CharacterAttributeIcon>();
        for (int i = 0; i < shownAttributes.Count; i++) {
            CharacterAttributeIcon shownIcon = shownAttributes[i];
            if (attributesToShow.Contains(shownIcon.attribute)) {
                //remove the tag from the attributes list so it doesn't get loaded again later
                attributesToShow.Remove(shownIcon.attribute);
            } else {
                //destroy the tag
                iconsToRemove.Add(shownIcon);
            }
        }

        for (int i = 0; i < iconsToRemove.Count; i++) {
            CharacterAttributeIcon icon = iconsToRemove[i];
            RemoveIcon(icon);
        }

        for (int i = 0; i < attributesToShow.Count; i++) { //show the remaining attributes
            CharacterAttribute currTag = attributesToShow[i];
            AddTag(currTag);
        }
    }
    private void AddTag(CharacterAttribute tag) {
        //GameObject tagGO = UIManager.Instance.InstantiateUIObject(characterTagPrefab.name, tagsScrollView.content);
        //CharacterAttributeIcon icon = tagGO.GetComponent<CharacterAttributeIcon>();
        //icon.SetTag(tag);
        //shownAttributes.Add(icon);
    }
    private void RemoveTag(CharacterAttribute tag) {
        //CharacterAttributeIcon[] icons = Utilities.GetComponentsInDirectChildren<CharacterAttributeIcon>(tagsScrollView.content.gameObject);
        //for (int i = 0; i < icons.Length; i++) {
        //    CharacterAttributeIcon icon = icons[i];
        //    if (icon.attribute == tag) {
        //        RemoveIcon(icon);
        //        break;
        //    }
        //}
    }
    private void RemoveIcon(CharacterAttributeIcon icon) {
        ObjectPoolManager.Instance.DestroyObject(icon.gameObject);
        shownAttributes.Remove(icon);
    }
    private void OnCharacterAttributeAdded(Character affectedCharacter, CharacterAttribute tag) {
        if (_activeCharacter != null && _activeCharacter.id == affectedCharacter.id && _activeCharacter.isBeingInspected) {
            AddTag(tag);
        }
    }
    private void OnCharacterAttributeRemoved(Character affectedCharacter, CharacterAttribute tag) {
        if (_activeCharacter != null && _activeCharacter.id == affectedCharacter.id && _activeCharacter.isBeingInspected) {
            RemoveTag(tag);
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

    #region Release Character
    public void ShowReleaseButton() {
        if (_activeCharacter.party != null && _activeCharacter.party.characterObject.currentState.stateName == "Imprisoned") {
            releaseBtnGO.SetActive(true);
        } else {
            releaseBtnGO.SetActive(false);
        }
    }
    public void ReleaseCharacter() {
        CharacterAction action = _activeCharacter.party.characterObject.currentState.GetAction(ACTION_TYPE.RELEASE);
        ReleaseAction releaseAction = action as ReleaseAction;
        releaseAction.ReleaseCharacter(_activeCharacter.party.characterObject);
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

    private void CheckIfMenuShouldBeHidden() {
        if (UIManager.Instance.partyinfoUI.isShowing) {
            logParentGO.SetActive(false);
        } else {
            logParentGO.SetActive(true);
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
}
