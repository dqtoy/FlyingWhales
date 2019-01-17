using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;


public class MonsterInfoUI : UIMenu {
    private const int MAX_HISTORY_LOGS = 60;
    private const int MAX_INVENTORY = 16;

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private FactionEmblem factionEmblem;
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
    [SerializeField] private ScrollRect tagsScrollView;
    [SerializeField] private GameObject characterTagPrefab;
    [SerializeField] private GameObject statsMenuCover;


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
    //[SerializeField] private TokenItem[] intelItems;
    //[SerializeField] private HiddenDesireItem hiddenDesireItem;
    //[SerializeField] private GameObject infoMenuCover;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;
    [SerializeField] private GameObject logsMenuCover;

    private LogHistoryItem[] logHistoryItems;
    private ItemContainer[] inventoryItemContainers;

    internal Monster currentlyShowingMonster {
        get { return _data as Monster; }
    }

    private Monster _activeMonster;
    //internal override void Initialize() {
    //    base.Initialize();
    //    //Messenger.AddListener(Signals.UPDATE_UI, UpdateMonsterInfo);
    //}

    #region Overrides
    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener<Token>(Signals.TOKEN_ADDED, OnIntelAdded);

        currentActionIcon.Initialize();
        InitializeLogsMenu();
        InitializeInventoryMenu();
        InitializeInfoMenu();
    }
    public override void OpenMenu() {
        base.OpenMenu();
        _activeMonster = _data as Monster;
        UpdatePortrait();
        UpdateMonsterInfo();
        //currentActionIcon.SetCharacter(_activeMonster);
        currentActionIcon.SetAction(_activeMonster.currentParty.currentAction);
        //PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(_activeMonster);
        historyScrollView.verticalNormalizedPosition = 1;
    }
    public override void CloseMenu() {
        base.CloseMenu();
        _activeMonster = null;
        //PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdateMonsterInfo();
        }
    }
    public override void ShowTooltip(GameObject objectHovered) {
        base.ShowTooltip(objectHovered);
        if (objectHovered == healthProgressBar.gameObject) {
            UIManager.Instance.ShowSmallInfo(_activeMonster.currentHP + "/" + _activeMonster.maxHP);
        } else if (objectHovered == manaProgressBar.gameObject) {
            UIManager.Instance.ShowSmallInfo(_activeMonster.currentSP + "/" + _activeMonster.maxSP);
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

    public void UpdateMonsterInfo() {
        if (_activeMonster == null) {
            return;
        }
        if (GameManager.Instance.inspectAll) {
            SetCoversState(false);
        } else {
            //if the character has never been inspected
            //activate all the tab menu covers
            //else, disable all the tab menu covers
            SetCoversState(!_activeMonster.hasBeenInspected);
        }
        UpdateBasicInfo();
        if (!_activeMonster.hasBeenInspected && !GameManager.Instance.inspectAll) {
            //if the character has never been inspected
            //clear all menus and do not load any info
            ClearAllTabMenus();
            return;
        }
        UpdateInfoMenu();
        if (_activeMonster.isBeingInspected || GameManager.Instance.inspectAll) {
            UpdateStatsInfo();
            UpdateItemsInfo();
            UpdateTagInfo(_activeMonster.attributes);
            if(_activeMonster.relationships != null) {
                UpdateRelationshipInfo(_activeMonster.relationships.Values.ToList());
            }
        } else {
            UpdateStatsInfo(_activeMonster.uiData);
            UpdateItemsInfo(_activeMonster.uiData);
            UpdateTagInfo(_activeMonster.uiData.attributes);
            UpdateRelationshipInfo(_activeMonster.uiData.relationships);
        }
        UpdateAllHistoryInfo();
        //UpdateOtherInfo();
        //Item drop info
    }
    private void SetCoversState(bool state) {
        //infoMenuCover.SetActive(state);
        statsMenuCover.SetActive(state);
        itemsMenuCover.SetActive(state);
        relationsMenuCover.SetActive(state);
        logsMenuCover.SetActive(state);
    }
    private void ClearAllTabMenus() {
        //stats
        healthProgressBar.value = 0f;
        manaProgressBar.value = 0f;
        strengthLbl.text = "-";
        agilityLbl.text = "-";
        intelligenceLbl.text = "-";
        vitalityLbl.text = "-";

        //items
        for (int i = 0; i < inventoryItemContainers.Length; i++) {
            ItemContainer currContainer = inventoryItemContainers[i];
            currContainer.SetItem(null);
        }
        headArmorContainer.SetItem(null);
        leftHandContainer.SetItem(null);
        rightHandContainer.SetItem(null);
        chestArmorContainer.SetItem(null);
        legArmorContainer.SetItem(null);
        leftFootArmorContainer.SetItem(null);
        rightFootArmorContainer.SetItem(null);

        //tags
        Utilities.DestroyChildren(tagsScrollView.content);

        //relationships
        Utilities.DestroyChildren(relationsScrollView.content);

        //logs
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            currItem.gameObject.SetActive(false);
        }

        ////secrets
        //for (int i = 0; i < secretItems.Length; i++) {
        //    SecretItem currItem = secretItems[i];
        //    currItem.gameObject.SetActive(false);
        //}

        //intel
        //for (int i = 0; i < intelItems.Length; i++) {
        //    TokenItem currItem = intelItems[i];
        //    currItem.gameObject.SetActive(false);
        //}

        //hidden desire
        //hiddenDesireItem.gameObject.SetActive(false);
    }
    private void UpdateBasicInfo() {
        nameLbl.text = _activeMonster.name;
        if (_activeMonster.isBeingInspected || GameManager.Instance.inspectAll) {
            nameLbl.text += " (Info State: Updated)";
            lvlClassLbl.text = "Lvl." + _activeMonster.level.ToString();
        } else {
            if (_activeMonster.hasBeenInspected) {
                nameLbl.text += " (Info State: Old)";
                lvlClassLbl.text = "Lvl." + _activeMonster.uiData.level.ToString();
            } else {
                lvlClassLbl.text = "???";
            }
        }
        factionEmblem.SetFaction(_activeMonster.faction);
        //affiliations.SetCharacter(_activeCharacter);
    }
    private void UpdatePortrait() {
        //characterPortrait.GeneratePortrait(_activeMonster);
        //characterPortrait.SetBGState(false);
    }

    #region Stats
    private void UpdateStatsInfo() {
        healthProgressBar.value = (float) _activeMonster.currentHP / (float) _activeMonster.maxHP;
        manaProgressBar.value = (float) _activeMonster.currentSP / (float) _activeMonster.maxSP;
        //strengthLbl.text = _activeMonster.strength.ToString();
        //agilityLbl.text = _activeMonster.agility.ToString();
        //intelligenceLbl.text = _activeMonster.intelligence.ToString();
        //vitalityLbl.text = _activeMonster.vitality.ToString();
        //expDropLbl.text = _activeMonster.experienceDrop.ToString();
    }
    private void UpdateStatsInfo(CharacterUIData uiData) {
        healthProgressBar.value = uiData.healthValue;
        manaProgressBar.value = uiData.manaValue;
        //strengthLbl.text = uiData.strength.ToString();
        //agilityLbl.text = uiData.agility.ToString();
        //intelligenceLbl.text = uiData.intelligence.ToString();
        //vitalityLbl.text = uiData.vitality.ToString();
    }
    #endregion

    #region Items
    private void UpdateItemsInfo() {
        //UpdateEquipmentInfo(_activeMonster.equippedItems);
        UpdateInventoryInfo(_activeMonster.inventory);
    }
    private void UpdateItemsInfo(CharacterUIData uiData) {
        //UpdateEquipmentInfo(uiData.equippedItems);
        UpdateInventoryInfo(uiData.inventory);
    }
    private void UpdateEquipmentInfo(List<Item> equipment) {
        headArmorContainer.SetItem(null);
        chestArmorContainer.SetItem(null);
        legArmorContainer.SetItem(null);
        leftFootArmorContainer.SetItem(null);
        rightFootArmorContainer.SetItem(null);
    }
    private void UpdateInventoryInfo(List<Item> inventory) {
        if(inventory == null) {
            return;
        }
        for (int i = 0; i < inventoryItemContainers.Length; i++) {
            ItemContainer currContainer = inventoryItemContainers[i];
            Item currInventoryItem = inventory.ElementAtOrDefault(i);
            //currContainer.SetItem(currInventoryItem);
        }
    }
    #endregion
    //private void UpdateOtherInfo() {
    //    string text = string.Empty;
    //    text += "\n<b>P Final Attack:</b> " + _activeMonster.pFinalAttack;
    //    text += "\n<b>M Final Attack:</b> " + _activeMonster.pFinalAttack;
    //    text += "\n<b>Def:</b> " + _activeMonster.def;
    //    text += "\n<b>Crit Chance:</b> " + _activeMonster.critChance + "%";
    //    text += "\n<b>Dodge Chance:</b> " + _activeMonster.dodgeChance + "%";
    //    text += "\n<b>Computed Power:</b> " + _activeMonster.computedPower;
    //    text += "\n<b>Is Sleeping:</b> " + _activeMonster.isSleeping;

    //    otherInfoLbl.text = text;
    //}

    #region Info
    private void InitializeInfoMenu() {
        //for (int i = 0; i < secretItems.Length; i++) {
        //    SecretItem currItem = secretItems[i];
        //    currItem.Initialize();
        //}
    }
    private void UpdateInfoMenu() {
        //for (int i = 0; i < secretItems.Length; i++) {
        //    secretItems[i].gameObject.SetActive(false);
        //}
        //for (int i = 0; i < intelItems.Length; i++) {
        //    intelItems[i].gameObject.SetActive(false);
        //}
        //hiddenDesireItem.gameObject.SetActive(false);
        //for (int i = 0; i < secretItems.Length; i++) {
        //    SecretItem currItem = secretItems[i];
        //    Secret currSecret = _activeMonster.secrets.ElementAtOrDefault(i);
        //    if (currSecret == null) {
        //        currItem.gameObject.SetActive(false);
        //    } else {
        //        currItem.SetSecret(currSecret, _activeMonster);
        //        currItem.gameObject.SetActive(true);
        //    }
        //}
        //List<Intel> intel = IntelManager.Instance.GetIntelConcerning(_activeMonster);
        //for (int i = 0; i < intelItems.Length; i++) {
        //    IntelItem currItem = intelItems[i];
        //    Intel currIntel = intel.ElementAtOrDefault(i);
        //    if (currIntel == null) {
        //        currItem.gameObject.SetActive(false);
        //    } else {
        //        currItem.SetIntel(currIntel);
        //        currItem.gameObject.SetActive(true);
        //    }
        //}
        //if (_activeCharacter.hiddenDesire == null) {
        //    hiddenDesireItem.gameObject.SetActive(false);
        //} else {
        //    hiddenDesireItem.SetHiddenDesire(_activeCharacter.hiddenDesire, _activeCharacter);
        //    hiddenDesireItem.gameObject.SetActive(true);
        //}
    }
    private void OnIntelAdded(Token intel) {
        if (_activeMonster == null) {
            return;
        }
        UpdateInfoMenu();
    }
    #endregion

    #region Attributes
    private List<CharacterAttributeIcon> shownAttributes = new List<CharacterAttributeIcon>();
    private void UpdateTagInfo(List<CharacterAttribute> attributes) {
        if(attributes == null) {
            return;
        }
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
        GameObject tagGO = UIManager.Instance.InstantiateUIObject(characterTagPrefab.name, tagsScrollView.content);
        CharacterAttributeIcon icon = tagGO.GetComponent<CharacterAttributeIcon>();
        icon.SetTag(tag);
        shownAttributes.Add(icon);
    }
    private void RemoveTag(CharacterAttribute tag) {
        CharacterAttributeIcon[] icons = Utilities.GetComponentsInDirectChildren<CharacterAttributeIcon>(tagsScrollView.content.gameObject);
        for (int i = 0; i < icons.Length; i++) {
            CharacterAttributeIcon icon = icons[i];
            if (icon.attribute == tag) {
                RemoveIcon(icon);
                break;
            }
        }
    }
    private void RemoveIcon(CharacterAttributeIcon icon) {
        ObjectPoolManager.Instance.DestroyObject(icon.gameObject);
        shownAttributes.Remove(icon);
    }
    private void OnCharacterAttributeAdded(Character affectedCharacter, CharacterAttribute tag) {
        if (_activeMonster != null && _activeMonster.id == affectedCharacter.id && _activeMonster.isBeingInspected) {
            AddTag(tag);
        }
    }
    private void OnCharacterAttributeRemoved(Character affectedCharacter, CharacterAttribute tag) {
        if (_activeMonster != null && _activeMonster.id == affectedCharacter.id && _activeMonster.isBeingInspected) {
            RemoveTag(tag);
        }
    }
    #endregion

    #region History
    private void UpdateHistory(object obj) {
        if (obj is Monster && _activeMonster != null && (obj as Monster).id == _activeMonster.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        if(_activeMonster.history == null) {
            return;
        }
        List<Log> characterHistory = new List<Log>(_activeMonster.history.OrderByDescending(x => x.id));
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

    #region Relationships
    List<CharacterRelationshipItem> shownRelationships = new List<CharacterRelationshipItem>();
    private void UpdateRelationshipInfo(List<Relationship> relationships) {
        if(relationships == null) {
            return;
        }
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
}
