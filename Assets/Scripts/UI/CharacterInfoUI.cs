using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;
using TMPro;
using UnityEngine.UI;

public class CharacterInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 20;
    public bool isWaitingForAttackTarget;
    public bool isWaitingForJoinBattleTarget;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI basicInfoLbl;
    [SerializeField] private TextMeshProUGUI generalInfoLbl;
    [SerializeField] private TextMeshProUGUI statInfoLbl;
    [SerializeField] private TextMeshProUGUI traitInfoLbl;
    [SerializeField] private TextMeshProUGUI equipmentInfoLbl;
    [SerializeField] private TextMeshProUGUI inventoryInfoLbl;
    [SerializeField] private TextMeshProUGUI relationshipsLbl;
    //[SerializeField] private ScrollRect equipmentScrollView;
    //[SerializeField] private ScrollRect inventoryScrollView;
    //[SerializeField] private ScrollRect relationshipsScrollView;

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
        Messenger.AddListener(Signals.UPDATE_UI, UpdateCharacterInfo);
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener<BaseLandmark>(Signals.PLAYER_LANDMARK_CREATED, OnPlayerLandmarkCreated);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        logHistoryItems = new LogHistoryItem[MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
            //newLogItem.name = "-1";
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }
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

    private void OnCharacterDied(ECS.Character deadCharacter) {
        if (isShowing && currentlyShowingCharacter != null && currentlyShowingCharacter.id == deadCharacter.id) {
            SetData(null);
            CloseMenu();
        }
    }

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
        UpdateGeneralInfo();
        UpdateStatInfo();
        UpdateTraitInfo();
        UpdateEquipmentInfo();
        UpdateInventoryInfo();
        UpdateRelationshipInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(currentlyShowingCharacter, IMAGE_SIZE.X256);
    }
    private void UpdateBasicInfo() {
        string text = string.Empty;
        text += "<b>Name: </b>" + currentlyShowingCharacter.name;
        text += "\n<b>Race: </b>" + Utilities.GetNormalizedSingularRace(currentlyShowingCharacter.raceSetting.race) + " " + Utilities.NormalizeString(currentlyShowingCharacter.gender.ToString());
        if (currentlyShowingCharacter.characterClass != null && currentlyShowingCharacter.characterClass.className != "Classless") {
            text += "\n<b>Class: </b> " + currentlyShowingCharacter.characterClass.className;
        }
        if (currentlyShowingCharacter.role != null) {
            text += "\n<b>Role: </b>" + currentlyShowingCharacter.role.roleType.ToString();
            if (currentlyShowingCharacter.role.job != null) {
                text += "\n<b>Job: </b>" + currentlyShowingCharacter.role.job.jobType.ToString();
            }
        }

        text += "\n<b>Faction: </b>" + (currentlyShowingCharacter.faction != null ? currentlyShowingCharacter.faction.urlName : "NONE");
        text += "\n<b>Home Area: </b>";
        if (currentlyShowingCharacter.home != null) {
            text += currentlyShowingCharacter.home.name;
        } else {
            text += "NONE";
        }
        basicInfoLbl.text = text;
    }
    public void UpdateGeneralInfo() {
        string text = string.Empty;
        text += "<b>Specific Location: </b>" + (currentlyShowingCharacter.party.specificLocation != null ? currentlyShowingCharacter.party.specificLocation.locationName : "NONE");
        text += "\n<b>Current Action: </b>";
        if (currentlyShowingCharacter.party.actionData.currentAction != null) {
            text += currentlyShowingCharacter.party.actionData.currentAction.actionData.actionName.ToString() + " ";
        } else {
            text += "NONE";
        }
        if (currentlyShowingCharacter.role != null) {
            text += "\n<b>Fullness: </b>" + currentlyShowingCharacter.role.fullness.ToString("F0") + ", <b>Energy: </b>" + currentlyShowingCharacter.role.energy.ToString("F0") + ", <b>Fun: </b>" + currentlyShowingCharacter.role.fun.ToString("F0");
            text += "\n<b>Sanity: </b>" + currentlyShowingCharacter.role.sanity.ToString("F0") + ", <b>Prestige: </b>" + currentlyShowingCharacter.role.prestige.ToString("F0") + ", <b>Safety: </b>" + currentlyShowingCharacter.role.safety.ToString("F0");
            text += "\n<b>Happiness: </b>" + currentlyShowingCharacter.role.happiness;
        }
        text += "\n<b>Computed Power: </b>" + currentlyShowingCharacter.computedPower;
        generalInfoLbl.text = text;

    }

    private void UpdateStatInfo() {
        string text = string.Empty;
        text += "<b>Lvl: </b>" + currentlyShowingCharacter.level.ToString() + "/" + CharacterManager.Instance.maxLevel.ToString();
        text += ", <b>Exp: </b>" + currentlyShowingCharacter.experience.ToString() + "/" + currentlyShowingCharacter.maxExperience.ToString();
        text += "\n<b>HP: </b>" + currentlyShowingCharacter.currentHP.ToString() + "/" + currentlyShowingCharacter.maxHP.ToString();
        text += ", <b>SP: </b>" + currentlyShowingCharacter.currentSP.ToString() + "/" + currentlyShowingCharacter.maxSP.ToString();
        text += "\n<b>Str: </b>" + currentlyShowingCharacter.strength.ToString();
        text += ", <b>Int: </b>" + currentlyShowingCharacter.intelligence.ToString();
        text += "\n<b>Agi: </b>" + currentlyShowingCharacter.agility.ToString();
        text += ", <b>Vit: </b>" + currentlyShowingCharacter.vitality.ToString();
        statInfoLbl.text = text;
    }
    private void UpdateTraitInfo() {
        string text = string.Empty;
        if (currentlyShowingCharacter.tags.Count > 0) { //currentlyShowingCharacter.traits.Count > 0 ||
            //for (int i = 0; i < currentlyShowingCharacter.traits.Count; i++) {
            //    Trait trait = currentlyShowingCharacter.traits[i];
            //    if (i > 0) {
            //        text += ", ";
            //    }
            //    text += trait.traitName;
            //}
            //if (currentlyShowingCharacter.traits.Count > 0) {
            //    text += ", ";
            //}
            for (int i = 0; i < currentlyShowingCharacter.tags.Count; i++) {
                CharacterTag tag = currentlyShowingCharacter.tags[i];
                if (i > 0) {
                    text += ", ";
                }
                text += tag.tagName;
            }
        } else {
            text += "\nNONE";
        }
        traitInfoLbl.text = text;
    }
    private void UpdateEquipmentInfo() {
        string text = string.Empty;
        if (currentlyShowingCharacter.equippedItems.Count > 0) {
            for (int i = 0; i < currentlyShowingCharacter.equippedItems.Count; i++) {
                Item item = currentlyShowingCharacter.equippedItems[i];
                if (i > 0) {
                    text += "\n";
                }
                text += item.itemName;
                if (item is Weapon) {
                    Weapon weapon = (Weapon)item;
                    if (weapon.bodyPartsAttached.Count > 0) {
                        text += " (";
                        for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
                            if (j > 0) {
                                text += ", ";
                            }
                            text += weapon.bodyPartsAttached[j].name;
                        }
                        text += ")";
                    }
                } else if (item is Armor) {
                    Armor armor = (Armor)item;
                    text += " (" + armor.bodyPartAttached.name + ")";
                }
            }
        } else {
            text += "NONE";
        }
        equipmentInfoLbl.text = text;
    }

    private void UpdateInventoryInfo() {
        string text = string.Empty;
        CharacterObj obj = currentlyShowingCharacter.party.characterObject as CharacterObj;
        foreach (RESOURCE resource in obj.resourceInventory.Keys) {
            text += resource.ToString() + ": " + obj.resourceInventory[resource];
            text += "\n";
        }
        inventoryInfoLbl.text = text;
    }

    private void UpdateRelationshipInfo() {
        string text = string.Empty;
        if (currentlyShowingCharacter.relationships.Count > 0) {
            bool isFirst = true;
            foreach (KeyValuePair<Character, Relationship> kvp in currentlyShowingCharacter.relationships) {
                if (!isFirst) {
                    text += "\n";
                } else {
                    isFirst = false;
                }
                text += kvp.Key.role.roleType.ToString() + " " + kvp.Key.urlName;
                if (kvp.Value.relationshipStatuses.Count > 0) {
                    text += "(";
                    for (int i = 0; i < kvp.Value.relationshipStatuses.Count; i++) {
                        if (i > 0) {
                            text += ",";
                        }
                        text += kvp.Value.relationshipStatuses[i].ToString();
                    }
                    text += ")";
                }
            }
        } else {
            text += "NONE";
        }

        relationshipsLbl.text = text;
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
        if (currentlyShowingCharacter.party.characterObject.currentState.stateName == "Imprisoned") {
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
        if(currentlyShowingCharacter.currentCombat != null) {
            currentlyShowingCharacter.currentCombat.CharacterDeath(currentlyShowingCharacter);
        }
        currentlyShowingCharacter.Death();
    }
    #endregion
}
