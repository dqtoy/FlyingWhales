using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private AffiliationsObject affiliations;
    [SerializeField] private ScrollRect actionQueueScrollView;
    [SerializeField] private GameObject actionIconPrefab;

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

    [Space(10)]
    [Header("Mood")]
    [SerializeField] private GameObject moodMenuGO;
    [SerializeField] private Slider overallProgressBar;
    [SerializeField] private Slider energyProgressBar;
    [SerializeField] private Slider fullnessProgressBar;
    [SerializeField] private Slider funProgressBar;
    [SerializeField] private Slider prestigeProgressBar;
    [SerializeField] private Slider sanityProgressBar;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private GameObject itemsMenuGO;
    [SerializeField] private ItemContainer headArmorContainer;
    [SerializeField] private ItemContainer leftArmArmorContainer;
    [SerializeField] private ItemContainer rightArmArmorContainer;
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
        affiliations.Initialize();
        //Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        InititalizeLogsMenu();
        InititalizeInventoryMenu();
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
    public override void HideMenu() {
        _activeCharacter = null;
        base.HideMenu();
    }
    public override void ShowMenu() {
        base.ShowMenu();
        _activeCharacter = (Character)_data;
        UpdateCharacterInfo();
        UpdateAllHistoryInfo();
        ShowAttackButton();
        ShowReleaseButton();
        CheckShowSnatchButton();
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
        if (currentlyShowingCharacter == null || (currentlyShowingCharacter != null && currentlyShowingCharacter.isDead)) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        //UpdateGeneralInfo();
        UpdateStatInfo();
        UpdateTagInfo();
        UpdateMoodInfo();
        //UpdateEquipmentInfo();
        //UpdateInventoryInfo();
        UpdateRelationshipInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(currentlyShowingCharacter, IMAGE_SIZE.X256);
    }
    private void UpdateBasicInfo() {
        nameLbl.text = currentlyShowingCharacter.name;
        lvlClassLbl.text = "Lvl." + currentlyShowingCharacter.level.ToString() + " " + currentlyShowingCharacter.characterClass.className;
        affiliations.SetCharacter(currentlyShowingCharacter);
    }
    //public void UpdateGeneralInfo() {
    //    string text = string.Empty;
    //    if (currentlyShowingCharacter.party == null) {
    //        text += "<b>Specific Location: </b>NONE";
    //    } else {
    //        text += "<b>Specific Location: </b>" + (currentlyShowingCharacter.party.specificLocation != null ? currentlyShowingCharacter.party.specificLocation.locationName : "NONE");
    //    }
        
    //    text += "\n<b>Current Action: </b>";
    //    if (currentlyShowingCharacter.party.actionData.currentAction != null) {
    //        text += currentlyShowingCharacter.party.actionData.currentAction.actionData.actionName.ToString() + " ";
    //    } else {
    //        text += "NONE";
    //    }
    //    text += "\n<b>Taken Quests: </b>";
    //    if (currentlyShowingCharacter.questData.Count > 0) {
    //        for (int i = 0; i < currentlyShowingCharacter.questData.Count; i++) {
    //            CharacterQuestData data = currentlyShowingCharacter.questData[i];
    //            text += "\n" + data.parentQuest.name;
    //        }
    //    } else {
    //        text += "NONE";
    //    }

    //    if (currentlyShowingCharacter.role != null) {
    //        text += "\n<b>Fullness: </b>" + currentlyShowingCharacter.role.fullness.ToString("F0") + ", <b>Energy: </b>" + currentlyShowingCharacter.role.energy.ToString("F0") + ", <b>Fun: </b>" + currentlyShowingCharacter.role.fun.ToString("F0");
    //        text += "\n<b>Sanity: </b>" + currentlyShowingCharacter.role.sanity.ToString("F0") + ", <b>Prestige: </b>" + currentlyShowingCharacter.role.prestige.ToString("F0") + ", <b>Safety: </b>" + currentlyShowingCharacter.role.safety.ToString("F0");
    //        text += "\n<b>Happiness: </b>" + currentlyShowingCharacter.role.happiness;
    //    }
    //    text += "\n<b>Computed Power: </b>" + currentlyShowingCharacter.computedPower;
    //    text += "\n<b>P Final Attack: </b>" + currentlyShowingCharacter.pFinalAttack;
    //    text += "\n<b>M Final Attack: </b>" + currentlyShowingCharacter.mFinalAttack;
    //    text += "\n<b>Mental Points: </b>" + currentlyShowingCharacter.mentalPoints;
    //    text += "\n<b>Physical Points: </b>" + currentlyShowingCharacter.physicalPoints;
    //    text += "\n<b>Current Party: </b>" + currentlyShowingCharacter.currentParty.name;
    //    for (int i = 0; i < currentlyShowingCharacter.currentParty.icharacters.Count; i++) {
    //        text += "\n - " + currentlyShowingCharacter.currentParty.icharacters[i].name;
    //    }
    //    text += "\n<b>Own Party: </b>" + currentlyShowingCharacter.ownParty.name;
    //    for (int i = 0; i < currentlyShowingCharacter.ownParty.icharacters.Count; i++) {
    //        text += "\n - " + currentlyShowingCharacter.ownParty.icharacters[i].name;
    //    }
    //    text += "\n<b>Squad: </b>";
    //    if (currentlyShowingCharacter.squad != null) {
    //        text += currentlyShowingCharacter.squad.name + " (PP: " + currentlyShowingCharacter.squad.potentialPower + ")";
    //        for (int i = 0; i < currentlyShowingCharacter.squad.squadMembers.Count; i++) {
    //            ICharacter currChar = currentlyShowingCharacter.squad.squadMembers[i];
    //            if (currentlyShowingCharacter.squad.squadLeader.id == currChar.id) {
    //                text += "\n - " + currChar.name + " (LEADER)";
    //            } else {
    //                text += "\n - " + currChar.name;
    //            }
    //        }
    //        List<Quest> quests = currentlyShowingCharacter.squad.GetSquadQuests();
    //        text += "\n<b>Squad Quests: </b>";
    //        if (quests.Count > 0) {
    //            for (int i = 0; i < quests.Count; i++) {
    //                Quest currQuest = quests[i];
    //                text += "\n - " + currQuest.name + "(" + currQuest.groupType.ToString() + ")";
    //            }
    //        } else {
    //            text += "NONE";
    //        }
            
    //    } else {
    //        text += "NONE";
    //    }
    //    generalInfoLbl.text = text;

    //}

    private void UpdateStatInfo() {
        healthProgressBar.value = currentlyShowingCharacter.currentHP / currentlyShowingCharacter.maxHP;
        manaProgressBar.value = currentlyShowingCharacter.currentSP / currentlyShowingCharacter.maxSP;
        strengthLbl.text = currentlyShowingCharacter.strength.ToString();
        agilityLbl.text = currentlyShowingCharacter.agility.ToString();
        intelligenceLbl.text = currentlyShowingCharacter.intelligence.ToString();
        vitalityLbl.text = currentlyShowingCharacter.vitality.ToString();
    }
    private void UpdateTagInfo() {
        Utilities.DestroyChildren(tagsScrollView.content);
        for (int i = 0; i < currentlyShowingCharacter.tags.Count; i++) {
            CharacterTag currTag = currentlyShowingCharacter.tags[i];
            GameObject tagGO = UIManager.Instance.InstantiateUIObject(characterTagPrefab.name, tagsScrollView.content);
            tagGO.GetComponent<CharacterTagIcon>().SetTag(currTag.tagType);
        }
    }
    private void UpdateMoodInfo() {
        overallProgressBar.value = currentlyShowingCharacter.role.happiness;
        energyProgressBar.value = currentlyShowingCharacter.role.energy;
        fullnessProgressBar.value = currentlyShowingCharacter.role.fullness;
        funProgressBar.value = currentlyShowingCharacter.role.fun;
        prestigeProgressBar.value = currentlyShowingCharacter.role.prestige;
        sanityProgressBar.value = currentlyShowingCharacter.role.sanity;
    }
    private void UpdateItemsInfo() {
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

        IBodyPart torso = currentlyShowingCharacter.GetBodyPart("Head");
        if (torso != null) {
            Item torsoArmor = torso.GetArmor();
            if (torsoArmor != null) {
                headArmorContainer.SetItem(torsoArmor);
            } else {
                headArmorContainer.SetItem(null);
            }
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
        //string text = string.Empty;
        //if (currentlyShowingCharacter.relationships.Count > 0) {
        //    bool isFirst = true;
        //    foreach (KeyValuePair<Character, Relationship> kvp in currentlyShowingCharacter.relationships) {
        //        if (!isFirst) {
        //            text += "\n";
        //        } else {
        //            isFirst = false;
        //        }
        //        text += kvp.Key.role.roleType.ToString() + " " + kvp.Key.urlName;
        //        if (kvp.Value.relationshipStatuses.Count > 0) {
        //            text += "(";
        //            for (int i = 0; i < kvp.Value.relationshipStatuses.Count; i++) {
        //                if (i > 0) {
        //                    text += ",";
        //                }
        //                text += kvp.Value.relationshipStatuses[i].ToString();
        //            }
        //            text += ")";
        //        }
        //    }
        //} else {
        //    text += "NONE";
        //}

        //relationshipsLbl.text = text;
    }

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
            currentlyShowingCharacter.party.currentCombat.CharacterDeath(currentlyShowingCharacter);
        }
        currentlyShowingCharacter.Death();
    }
    #endregion
}
