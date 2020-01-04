using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using TMPro;
using UnityEngine.UI;
using Traits;
using UnityEngine.Serialization;

public class CharacterInfoUI : UIMenu {
    
    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private CharacterPortrait characterPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI lvlClassLbl;
    [SerializeField] private TextMeshProUGUI plansLbl;
    [SerializeField] private LogItem plansLblLogItem;
    
    [Space(10)]
    [Header("Location")]
    [SerializeField] private TextMeshProUGUI factionLbl;
    [SerializeField] private EventLabel factionEventLbl;
    [SerializeField] private TextMeshProUGUI currentLocationLbl;
    [SerializeField] private EventLabel currentLocationEventLbl;
    [SerializeField] private TextMeshProUGUI homeRegionLbl;
    [SerializeField] private EventLabel homeRegionEventLbl;
    [SerializeField] private TextMeshProUGUI houseLbl;
    [SerializeField] private EventLabel houseEventLbl;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logParentGO;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    private LogHistoryItem[] logHistoryItems;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;

    [Space(10)]
    [Header("Traits")]
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private TextMeshProUGUI itemsLbl;
    
    [Space(10)]
    [Header("Relationships")]
    [SerializeField] private EventLabel relationshipNamesEventLbl;
    [SerializeField] private TextMeshProUGUI relationshipTypesLbl;
    [SerializeField] private TextMeshProUGUI relationshipNamesLbl;
    [SerializeField] private TextMeshProUGUI relationshipValuesLbl;
    
    private Character _activeCharacter;
    private Character _previousCharacter;

    public Character activeCharacter => _activeCharacter;
    public Character previousCharacter => _previousCharacter;
    private string normalTextColor = "#CEB67C";
    private string buffTextColor = "#39FF14";
    private string flawTextColor = "#FF073A";

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<SpecialToken, Character>(Signals.CHARACTER_OBTAINED_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<SpecialToken, Character>(Signals.CHARACTER_LOST_ITEM, UpdateInventoryInfoFromSignal);
        Messenger.AddListener<Character>(Signals.CHARACTER_SWITCHED_ALTER_EGO, OnCharacterChangedAlterEgo);
        //Messenger.AddListener<Relatable, Relatable>(Signals.RELATIONSHIP_ADDED, OnRelationshipAdded);
        //Messenger.AddListener<Relatable, RELATIONSHIP_TRAIT, Relatable>(Signals.RELATIONSHIP_REMOVED, OnRelationshipRemoved);
        Messenger.AddListener<Character, Character>(Signals.OPINION_ADDED, OnOpinionAdded);
        Messenger.AddListener<Character, Character>(Signals.OPINION_REMOVED, OnOpinionRemoved);

        normalTraitsEventLbl.SetOnClickAction(OnClickTrait);
        statusTraitsEventLbl.SetOnClickAction(OnClickTrait);
        relationshipNamesEventLbl.SetOnClickAction(OnClickCharacter);
        
        factionEventLbl.SetOnClickAction(OnClickFaction);
        currentLocationEventLbl.SetOnClickAction(OnClickCurrentLocation);
        homeRegionEventLbl.SetOnClickAction(OnClickHomeLocation);
        houseEventLbl.SetOnClickAction(OnClickHomeStructure);

        InitializeLogsMenu();
    }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        if (_activeCharacter != null && _activeCharacter.marker != null && AreaMapCameraMove.Instance.target == _activeCharacter.marker.gameObject.transform) {
            AreaMapCameraMove.Instance.CenterCameraOn(null);    
        }
        _activeCharacter = null;
    }
    public override void OpenMenu() {
        _previousCharacter = _activeCharacter;
        _activeCharacter = _data as Character;
        base.OpenMenu();
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        UpdateCharacterInfo();
        UpdateTraits();
        UpdateRelationships();
        UpdateInventoryInfo();
        UpdateAllHistoryInfo();
        ResetAllScrollPositions();
    }
    public override void SetData(object data) {
        base.SetData(data);
        //if (isShowing) {
        //    UpdateCharacterInfo();
        //}
    }
    #endregion

    #region Utilities
    private void InitializeLogsMenu() {
        logHistoryItems = new LogHistoryItem[CharacterManager.MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < CharacterManager.MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }
    }
    private void ResetAllScrollPositions() {
        historyScrollView.verticalNormalizedPosition = 1;
    }
    public void UpdateCharacterInfo() {
        if (_activeCharacter == null) {
            return;
        }
        UpdatePortrait();
        UpdateBasicInfo();
        UpdateStatInfo();
        UpdateLocationInfo();
    }
    private void UpdatePortrait() {
        characterPortrait.GeneratePortrait(_activeCharacter);
    }
    public void UpdateBasicInfo() {
        nameLbl.text = _activeCharacter.name;
        lvlClassLbl.text = _activeCharacter.raceClassName;
        UpdateThoughtBubble();
    }
    public void UpdateThoughtBubble() {
        Log log;
        plansLbl.text = activeCharacter.GetThoughtBubble(out log);
        if (log != null) {
            plansLblLogItem.SetLog(log);
        }
    }
    #endregion

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = $"{_activeCharacter.currentHP.ToString()}/{_activeCharacter.maxHP.ToString()}";
        attackLbl.text = $"{_activeCharacter.attackPower.ToString()}";
        speedLbl.text = $"{_activeCharacter.speed.ToString()}";
        if(characterPortrait.character != null) {
            characterPortrait.UpdateLvl();
        }
    }
    #endregion

    #region Location
    private void UpdateLocationInfo() {
        factionLbl.text = _activeCharacter.faction != null ? $"<link=\"faction\">{_activeCharacter.faction.name}</link>" : "Factionless";
        currentLocationLbl.text = $"<link=\"currLocation\">{_activeCharacter.currentRegion.name}</link>";
        homeRegionLbl.text = _activeCharacter.homeRegion != null ? $"<link=\"home\">{_activeCharacter.homeRegion.name}</link>" : "Homeless";
        houseLbl.text = _activeCharacter.homeStructure != null ? $"<link=\"house\">{_activeCharacter.homeStructure.name}</link>" : "Homeless";
    }
    private void OnClickFaction(object obj) {
        UIManager.Instance.ShowFactionInfo(activeCharacter.faction);
    }
    private void OnClickCurrentLocation(object obj) {
        UIManager.Instance.ShowRegionInfo(activeCharacter.currentRegion);
    }
    private void OnClickHomeLocation(object obj) {
        UIManager.Instance.ShowRegionInfo(activeCharacter.homeRegion);
    }
    private void OnClickHomeStructure(object obj) {
        if (activeCharacter.homeRegion.area != null) {
            if (InnerMapManager.Instance.isAnAreaMapShowing && InnerMapManager.Instance.currentlyShowingMap != activeCharacter.homeRegion.area.areaMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (activeCharacter.homeRegion.area.areaMap.isShowing == false) {
                InnerMapManager.Instance.ShowAreaMap(activeCharacter.homeRegion.area);
            }
            AreaMapCameraMove.Instance.CenterCameraOn(activeCharacter.homeStructure.structureObj.gameObject);
        } else {
            UIManager.Instance.ShowRegionInfo(activeCharacter.homeRegion);
        }
        
    }
    #endregion

    #region Traits
    private void UpdateTraitsFromSignal(Character character, Trait trait) {
        if(_activeCharacter == null || _activeCharacter != character) {
            return;
        }
        UpdateTraits();
        UpdateThoughtBubble();
    }
    private void UpdateTraits() {
        // if (_activeCharacter.minion != null) {
        //     return;
        // }

        string statusTraits = string.Empty;
        string normalTraits = string.Empty;

        for (int i = 0; i < _activeCharacter.traitContainer.allTraits.Count; i++) {
            Trait currTrait = _activeCharacter.traitContainer.allTraits[i];
            if (currTrait.isHidden) {
                continue; //skip
            }
            if (currTrait.type == TRAIT_TYPE.ABILITY || currTrait.type == TRAIT_TYPE.ATTACK || currTrait.type == TRAIT_TYPE.COMBAT_POSITION
                || currTrait.name == "Herbivore" || currTrait.name == "Carnivore") {
                continue; //hide combat traits
            }
            if (currTrait.type == TRAIT_TYPE.STATUS || currTrait.type == TRAIT_TYPE.DISABLER || currTrait.type == TRAIT_TYPE.ENCHANTMENT || currTrait.type == TRAIT_TYPE.EMOTION) {
                string color = normalTextColor;
                if (currTrait.type == TRAIT_TYPE.BUFF) {
                    color = buffTextColor;
                } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                    color = flawTextColor;
                }
                if (!string.IsNullOrEmpty(statusTraits)) {
                    statusTraits = $"{statusTraits}, ";
                }
                statusTraits = $"{statusTraits}<b><color={color}><link=\"{i}\">{currTrait.name}</link></color></b>";
            } else {
                string color = normalTextColor;
                if (currTrait.type == TRAIT_TYPE.BUFF) {
                    color = buffTextColor;
                } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                    color = flawTextColor;
                }
                if (!string.IsNullOrEmpty(normalTraits)) {
                    normalTraits = $"{normalTraits}, ";
                }
                normalTraits = $"{normalTraits}<b><color={color}><link=\"{i}\">{currTrait.name}</link></color></b>";
            }
        }

        statusTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(statusTraits) == false) {
            //character has status traits
            statusTraitsLbl.text = statusTraits; 
        }
        normalTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(normalTraits) == false) {
            //character has normal traits
            normalTraitsLbl.text = normalTraits;
        }
    }
    public void OnHoverTrait(object obj) {
        if (obj is string) {
            string text = (string) obj;
            int index = int.Parse(text);
            Trait trait = activeCharacter.traitContainer.allTraits[index];
            UIManager.Instance.ShowSmallInfo(trait.description);
        }

    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnClickTrait(object obj) {
        if (TraitManager.Instance.CanStillTriggerFlaws(activeCharacter)) {
            if (obj is string) {
                string text = (string)obj;
                int index = int.Parse(text);
                Trait trait = activeCharacter.traitContainer.allTraits[index];
                string traitDescription = trait.description;
                if (trait.canBeTriggered) {
                    traitDescription += "\n" + trait.GetRequirementDescription(activeCharacter) +
                    "\n\n<b>Effect</b>: " + trait.GetTriggerFlawEffectDescription(activeCharacter, "flaw_effect");
                }

                StartCoroutine(HoverOutTraitAfterClick());//Quick fix because tooltips do not disappear. Issue with hover out action in label not being called when other collider goes over it.
                UIManager.Instance.ShowYesNoConfirmation(trait.name, traitDescription,
                    onClickYesAction: () => OnClickTriggerFlaw(trait),
                    showCover: true, layer: 25, yesBtnText:
                    $"Trigger ({trait.GetTriggerFlawManaCost(activeCharacter).ToString()} Mana)",
                    yesBtnInteractable: trait.CanFlawBeTriggered(activeCharacter),
                    pauseAndResume: true,
                    noBtnActive: false,
                    yesBtnActive: trait.canBeTriggered,
                    yesBtnInactiveHoverAction: () => ShowCannotTriggerFlawReason(trait),
                    yesBtnInactiveHoverExitAction: UIManager.Instance.HideSmallInfo
                );
                normalTraitsEventLbl.ResetHighlightValues();
            }
        } else {
            StartCoroutine(HoverOutTraitAfterClick());//Quick fix because tooltips do not disappear. Issue with hover out action in label not being called when other collider goes over it.
            PlayerUI.Instance.ShowGeneralConfirmation("Invalid", "This character's flaws can no longer be triggered.");
            normalTraitsEventLbl.ResetHighlightValues();
        }
    }
    private IEnumerator HoverOutTraitAfterClick() {
        yield return new WaitForEndOfFrame();
        OnHoverOutTrait();
    }
    private void ShowCannotTriggerFlawReason(Trait trait) {
        string reason = $"You cannot trigger {activeCharacter.name}'s flaw because: ";
        List<string> reasons = trait.GetCannotTriggerFlawReasons(activeCharacter);
        for (int i = 0; i < reasons.Count; i++) {
            reason = $"{reason}\n\t- {reasons[i]}";
        }
        UIManager.Instance.ShowSmallInfo(reason);
    }
    private void OnClickTriggerFlaw(Trait trait) {
        string logKey = trait.TriggerFlaw(activeCharacter);
        if (logKey != "flaw_effect") {
            UIManager.Instance.ShowYesNoConfirmation(
                trait.name,
                trait.GetTriggerFlawEffectDescription(activeCharacter, logKey),
                showCover: true,
                layer: 25,
                yesBtnText: "OK",
                pauseAndResume: true,
                noBtnActive: false
            );
        }
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(SpecialToken token, Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateInventoryInfo();
        }
    }
    private void UpdateInventoryInfo() {
        itemsLbl.text = string.Empty;
        for (int i = 0; i < _activeCharacter.items.Count; i++) {
            SpecialToken currInventoryItem = _activeCharacter.items[i];
            itemsLbl.text = $"{itemsLbl.text} {currInventoryItem.name}";
            if (i < _activeCharacter.items.Count - 1) {
                itemsLbl.text = $"{itemsLbl.text}, ";
            }
        }
    }
    #endregion

    #region History
    private void UpdateHistory(object obj) {
        var character = obj as Character;
        if (character != null && _activeCharacter != null && character.id == _activeCharacter.id && _activeCharacter.minion == null) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        if (_activeCharacter.minion != null) {
            return;
        }
        //List<Log> characterHistory = new List<Log>(_activeCharacter.history.OrderByDescending(x => x.date.year).ThenByDescending(x => x.date.month).ThenByDescending(x => x.date.day).ThenByDescending(x => x.date.tick));
        int historyCount = _activeCharacter.history.Count;
        int historyLastIndex = historyCount - 1;
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if(i < historyCount) {
                Log currLog = _activeCharacter.history[historyLastIndex - i];
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
    }
    private void SetLogMenuState(bool state) {
        logParentGO.SetActive(state);
    }
    #endregion   

    #region Listeners
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
    private void OnCharacterChangedAlterEgo(Character character) {
        if (isShowing && activeCharacter == character) {
            UpdateCharacterInfo();
            UpdateTraits();
        }
    }
    private void OnCharacterDied(Character character) {
        if (this.isShowing && activeCharacter.id == character.id) {
            AreaMapCameraMove.Instance.CenterCameraOn(null);
        }
    }
    #endregion

    #region For Testing
    public void ShowCharacterTestingInfo() {
        string summary = "Home structure: " + activeCharacter.homeStructure?.ToString() ?? "None";
        summary = $"{summary}{("\nCurrent structure: " + activeCharacter.currentStructure?.ToString() ?? "None")}";
        summary = $"{summary}{("\nPOI State: " + activeCharacter.state.ToString())}";
        summary = $"{summary}{("\nDo Not Disturb: " + activeCharacter.doNotDisturb)}";
        summary = $"{summary}{("\nDo Not Get Hungry: " + activeCharacter.needsComponent.doNotGetHungry.ToString())}";
        summary = $"{summary}{("\nDo Not Get Tired: " + activeCharacter.needsComponent.doNotGetTired.ToString())}";
        summary = $"{summary}{("\nDo Not Get Lonely: " + activeCharacter.needsComponent.doNotGetLonely.ToString())}";
        summary = $"{summary}{("\nDo Not Recover HP: " + activeCharacter.doNotRecoverHP.ToString())}";
        summary = $"{summary}{("\nFullness: " + activeCharacter.needsComponent.fullness + "/" + CharacterNeedsComponent.FULLNESS_DEFAULT)}";
        summary = $"{summary}{("\nTiredness: " + activeCharacter.needsComponent.tiredness + "/" + CharacterNeedsComponent.TIREDNESS_DEFAULT)}";
        summary = $"{summary}{("\nHappiness: " + activeCharacter.needsComponent.happiness + "/" + CharacterNeedsComponent.HAPPINESS_DEFAULT)}";
        summary = $"{summary}{("\nFullness Time: " + (activeCharacter.needsComponent.fullnessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.needsComponent.fullnessForcedTick)))}";
        summary = $"{summary}{("\nTiredness Time: " + (activeCharacter.needsComponent.tirednessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.needsComponent.tirednessForcedTick)))}";
        summary = $"{summary}{("\nRemaining Sleep Ticks: " + activeCharacter.needsComponent.currentSleepTicks.ToString())}";
        summary = $"{summary}{("\nFood: " + activeCharacter.food.ToString())}";
        summary = $"{summary}{("\nRole: " + activeCharacter.role.roleType.ToString())}";
        summary = $"{summary}{("\nSexuality: " + activeCharacter.sexuality.ToString())}";
        summary = $"{summary}{("\nMood: " + activeCharacter.moodValue.ToString() + "(" + activeCharacter.currentMoodType.ToString() + ")")}";
        summary = $"{summary}{("\nHP: " + activeCharacter.currentHP.ToString() + "/" + activeCharacter.maxHP.ToString())}";
        summary = $"{summary}{("\nIgnore Hostiles: " + activeCharacter.ignoreHostility.ToString())}";
        summary = $"{summary}{("\nAttack Range: " + activeCharacter.characterClass.attackRange.ToString())}";
        summary = $"{summary}{("\nAttack Speed: " + activeCharacter.attackSpeed.ToString())}";
        if (activeCharacter.stateComponent.currentState != null) {
            summary = $"{summary}{"\nCurrent State: " + activeCharacter.stateComponent.currentState.ToString()}";
            summary = $"{summary}{"\n\tDuration in state: " + activeCharacter.stateComponent.currentState.currentDuration.ToString() + "/" + activeCharacter.stateComponent.currentState.duration.ToString()}";
        }
        
        summary = $"{summary}\nActions targeting this character: ";
        
        summary += "\nPersonal Job Queue: ";
        if (activeCharacter.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < activeCharacter.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem poi = activeCharacter.jobQueue.jobsInQueue[i];
                summary += poi + ", ";
            }
        } else {
            summary += "None";
        }
        
        // summary += "\n" + activeCharacter.needsComponent.GetNeedsSummary();
        // summary += "\n\nAlter Egos: ";
        // for (int i = 0; i < activeCharacter.alterEgos.Values.Count; i++) {
        //     summary += "\n" + activeCharacter.alterEgos.Values.ElementAt(i).GetAlterEgoSummary();
        // }
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideCharacterTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Relationships
    private void UpdateRelationships() {
        relationshipTypesLbl.text = string.Empty;
        relationshipNamesLbl.text = string.Empty;
        relationshipValuesLbl.text = string.Empty;
        for (int i = 0; i < _activeCharacter.opinionComponent.opinions.Keys.Count; i++) {
            Character target = _activeCharacter.opinionComponent.opinions.Keys.ElementAt(i);
            IRelationshipData relData = _activeCharacter.relationshipContainer.GetRelationshipDataWith(target);
            RELATIONSHIP_TYPE relType = RELATIONSHIP_TYPE.NONE;
            if (relData != null) {
                relType = relData.GetFirstMajorRelationship();    
            }
            if (relType == RELATIONSHIP_TYPE.NONE) {
                relationshipTypesLbl.text += "Acquaintance\n";
            } else {
                relationshipTypesLbl.text += Utilities.NormalizeString(relType.ToString()) + "\n";    
            }
            int opinionOfOther = 0;
            if (target.opinionComponent.HasOpinion(activeCharacter)) {
                opinionOfOther = target.opinionComponent.GetTotalOpinion(activeCharacter);
            }
            relationshipNamesLbl.text += $"<link=\"{i}\">{target.name}</link>\n";
            relationshipValuesLbl.text += $"<link=\"{i}\"><color=\"{ OpinionColor(activeCharacter.opinionComponent.GetTotalOpinion(target)) }\"> " +
                                          $"{GetOpinionText(activeCharacter.opinionComponent.GetTotalOpinion(target))}</color> " +
                                          $"<color=\"{OpinionColor(opinionOfOther)}\">({GetOpinionText(opinionOfOther)})</color></link>\n";
        }
    }
    public void OnHoverRelationshipValue(object obj) {
        if (obj is string) {
            string text = (string)obj;
            int index = int.Parse(text);
            Character target = _activeCharacter.opinionComponent.opinions.Keys.ElementAtOrDefault(index);
            if (target != null) {
                ShowRelationshipData(target);
            }
        }
    }
    private void OnOpinionAdded(Character owner, Character target) {
        if (isShowing && owner == activeCharacter) {
            UpdateRelationships();
        }
    }
    private void OnOpinionRemoved(Character owner, Character target) {
        if (isShowing && owner == activeCharacter) {
            UpdateRelationships();
        }
    }
    private void ShowRelationshipData(Character target) {
        int opinionOfOther = target.opinionComponent.GetTotalOpinion(activeCharacter);
        string summary = target.name;
        summary += "\n---------------------";
        Dictionary<string, int> opinions = activeCharacter.opinionComponent.GetOpinion(target);
        foreach (KeyValuePair<string, int> kvp in opinions) {
            summary += "\n" + kvp.Key + ": " + "<color=" + OpinionColor(kvp.Value) + ">" + GetOpinionText(kvp.Value) + "</color>";
        }
        summary += "\n---------------------";
        summary += "\nTotal: <color=" + OpinionColor(opinionOfOther) + ">" + GetOpinionText(activeCharacter.opinionComponent.GetTotalOpinion(target)) + "</color> <color=" + OpinionColor(opinionOfOther) + ">(" + GetOpinionText(opinionOfOther) + ")</color>";
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideRelationshipData() {
        UIManager.Instance.HideSmallInfo();
    }
    private string OpinionColor(int number) {
        if(number < 0) {
            return "red";
        }
        return "green";
    }
    private string GetOpinionText(int number) {
        if (number < 0) {
            return "" + number;
        }
        return "+" + number;
    }
    private void OnClickCharacter(object obj) {
        if (obj is string) {
            string text = (string)obj;
            int index = int.Parse(text);
            Character target = _activeCharacter.opinionComponent.opinions.Keys.ElementAtOrDefault(index);
            if (target != null) {
                UIManager.Instance.ShowCharacterInfo(target,true);    
            }
        }
    }
    #endregion

    #region Actions
    protected override void LoadActions() {
        Utilities.DestroyChildren(actionsTransform);
        ActionItem item = AddNewAction("Seize", null, () => PlayerManager.Instance.player.seizeComponent.SeizePOI(activeCharacter));
        bool isInteractable = PlayerManager.Instance.player.seizeComponent.seizedPOI == null && activeCharacter.minion == null && !(activeCharacter is Summon) && activeCharacter.traitContainer.GetNormalTrait<Trait>("Leader", "Blessed") == null;
        item.SetInteractable(isInteractable);
    }
    #endregion
}
