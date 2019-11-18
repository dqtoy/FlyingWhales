using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Traits;

public class CharacterInfoUI : UIMenu {

    [Space(10)]
    [Header("Minion")]
    [SerializeField] private GameObject defaultTogglesGO;
    [SerializeField] private GameObject minionTogglesGO;
    [SerializeField] private Toggle skillToggle;
    [SerializeField] private Toggle minionTraitsToggle;
    [SerializeField] private Toggle spellsToggle;
    [SerializeField] private Toggle minionItemsToggle;
    [SerializeField] private TextMeshProUGUI minionSkillLbl;
    [SerializeField] private TextMeshProUGUI minionTraitsLbl;
    [SerializeField] private TextMeshProUGUI minionSpellsLbl;
    [SerializeField] private ScrollRect minionItemsScrollView;

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

    [Space(10)]
    [Header("Character")]
    [SerializeField] private GameObject attackButtonGO;
    [SerializeField] private Toggle attackBtnToggle;
    [SerializeField] private GameObject joinBattleButtonGO;
    [SerializeField] private Toggle joinBattleBtnToggle;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI attackLbl;
    [SerializeField] private TextMeshProUGUI speedLbl;

    [Space(10)]
    [Header("Traits")]
    [SerializeField] private ScrollRect statusTraitsScrollView;
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private ScrollRect normalTraitsScrollView;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private ScrollRect itemsScrollView;
    [SerializeField] private GameObject combatAttributePrefab;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)]
    [Header("Relationships")]
    [SerializeField] private ScrollRect relationshipTraitsScrollView;
    [SerializeField] private GameObject relationshipItemPrefab;

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
        Messenger.AddListener<Relatable, Relatable>(Signals.RELATIONSHIP_ADDED, OnRelationshipAdded);
        Messenger.AddListener<Relatable, RELATIONSHIP_TRAIT, Relatable>(Signals.RELATIONSHIP_REMOVED, OnRelationshipRemoved);
        inventoryItemContainers = Utilities.GetComponentsInDirectChildren<ItemContainer>(itemsScrollView.content.gameObject);

        normalTraitsEventLbl.SetOnClickAction(OnClickTrait);
        statusTraitsEventLbl.SetOnClickAction(OnClickTrait);

        InitializeLogsMenu();
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
        SetLogMenuState(false);
        //if (_activeCharacter.marker != null) {
        //    _activeCharacter.CenterOnCharacter();
        //}
        base.OpenMenu();
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            backButton.interactable = false;
        }
        if (UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.HideObjectPicker();
        }
        UpdateTabsToShow();
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

        if (_activeCharacter.minion != null) {
            UpdateMinionSkills();
            UpdateMinionTraits();
            UpdateMinionSpells();
        }
        
        //UpdateAllHistoryInfo();
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
    public void UpdateThoughtBubble() {
        if (_activeCharacter.minion != null) {
            plansLbl.text = string.Empty;
            return;
        }
        if (_activeCharacter.isDead) {
            plansLbl.text = _activeCharacter.name + " has died.";
            return;
        }
        if (_activeCharacter.minion != null) {
            if (_activeCharacter.minion.busyReasonLog != null) {
                plansLblLogItem.SetLog(_activeCharacter.minion.busyReasonLog);
                plansLbl.text = Utilities.LogReplacer(_activeCharacter.minion.busyReasonLog);
            } else {
                plansLbl.text = _activeCharacter.name + " is ready to do your bidding.";
            }
            return;
        }
        if (_activeCharacter.specificLocation.areaMap == null) {
            //area map has not yet been generated
            plansLbl.text = "Visit " + _activeCharacter.specificLocation.name + " to find out what " + _activeCharacter.name + " is doing.";
            return;
        }
        //Action
        if (_activeCharacter.currentActionNode != null) {
            Log currentLog = _activeCharacter.currentActionNode.GetCurrentLog();
            plansLblLogItem.SetLog(currentLog);
            plansLbl.text = Utilities.LogReplacer(currentLog);
            return;
        }

        //Disabler Thought
        if (_activeCharacter.doNotDisturb > 0) {
            Trait disablerTrait = _activeCharacter.traitContainer.GetAllTraitsOf(TRAIT_TYPE.DISABLER).FirstOrDefault();
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
        if (_activeCharacter.stateComponent.currentState != null) {
            plansLblLogItem.SetLog(_activeCharacter.stateComponent.currentState.thoughtBubbleLog);
            plansLbl.text = Utilities.LogReplacer(_activeCharacter.stateComponent.currentState.thoughtBubbleLog);
            return;
        }

        ////targetted by action
        //if (_activeCharacter.targettedByAction.Count > 0) {
        //    //character is targetted by an action
        //    Log targetLog = null;
        //    for (int i = 0; i < _activeCharacter.targettedByAction.Count; i++) {
        //        GoapAction action = _activeCharacter.targettedByAction[i];
        //        if (action.isPerformingActualAction && action.targetLog != null) {
        //            targetLog = action.targetLog;
        //            break;
        //        }
        //    }
        //    if (targetLog != null) {
        //        plansLbl.text = Utilities.LogReplacer(targetLog);
        //        return;
        //    }
        //}

        //State Job To Do
        //if (_activeCharacter.stateComponent.stateToDo != null) {
        //    plansLblLogItem.SetLog(_activeCharacter.stateComponent.stateToDo.thoughtBubbleLog);
        //    plansLbl.text = Utilities.LogReplacer(_activeCharacter.stateComponent.stateToDo.thoughtBubbleLog);
        //    return;
        //}

        //fleeing
        if (_activeCharacter.marker.hasFleePath) {
            plansLbl.text = _activeCharacter.name + " is fleeing.";
            return;
        }

        //Travelling
        if (_activeCharacter.currentParty.icon.isTravelling) {
            if (_activeCharacter.currentParty.owner.marker.destinationTile != null) {
                plansLbl.text = _activeCharacter.name + " is going to " + _activeCharacter.currentParty.owner.marker.destinationTile.structure.GetNameRelativeTo(_activeCharacter);
                return;
            }
        }

        //Default - Do nothing/Idle
        if (_activeCharacter.currentStructure != null) {
            plansLbl.text = _activeCharacter.name + " is in " + _activeCharacter.currentStructure.GetNameRelativeTo(_activeCharacter);
        }

        if (_activeCharacter.currentRegion != null || _activeCharacter.specificLocation != null) {
            Region region = _activeCharacter.currentRegion;
            if(region == null) {
                region = _activeCharacter.specificLocation.region;
            }
            plansLbl.text = _activeCharacter.name + " is in " + region.name;
        }
    }
    public bool IsCharacterInfoShowing(Character character) {
        return (isShowing && _activeCharacter == character);
    }
    #endregion

    #region Tabs
    /// <summary>
    /// Update the tabs to show based on the character that is being shown on the menu
    /// </summary>
    private void UpdateTabsToShow() {
        defaultTogglesGO.SetActive(activeCharacter.minion == null);
        minionTogglesGO.SetActive(activeCharacter.minion != null);
    }
    #endregion

    #region Minion
    private void UpdateMinionSkills() {
        minionSkillLbl.text = "<b><link=\"0\">" + activeCharacter.minion.combatAbility.name + "</link></b>";
    }
    private void UpdateMinionTraits() {
        minionTraitsLbl.text = string.Empty;
        for (int i = 0; i < activeCharacter.minion.deadlySin.assignments.Length; i++) {
            if (i > 0) {
                minionTraitsLbl.text += ", ";
            }
            minionTraitsLbl.text += "<b><link=\"" + i.ToString() + "\">" + Utilities.NormalizeStringUpperCaseFirstLetters(activeCharacter.minion.deadlySin.assignments[i].ToString()) + "</link></b>";
        }
    }
    private void UpdateMinionSpells() {
        if (activeCharacter.minion.interventionAbilitiesToResearch.Count > 0) {
            minionSpellsLbl.text = string.Empty;
            for (int i = 0; i < activeCharacter.minion.interventionAbilitiesToResearch.Count; i++) {
                if (i > 0) {
                    minionSpellsLbl.text += ", ";
                }
                minionSpellsLbl.text += "<b><link=\"" + i.ToString() + "\">" + Utilities.NormalizeStringUpperCaseFirstLetters(activeCharacter.minion.interventionAbilitiesToResearch[i].ToString()) + "</link></b>";
            }
        } else {
            minionSpellsLbl.text = "<i>Minions without the spell source trait do not have extractable spells.</i>";
        }
    }
    public void OnHoverActionAbility(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            DEADLY_SIN_ACTION action = activeCharacter.minion.deadlySin.assignments[index];
            
            UIManager.Instance.ShowSmallInfo(action.Description(), Utilities.NormalizeStringUpperCaseFirstLetters(action.ToString()));
        }
    }
    public void OnHoverExitAbility() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnHoverResearchSpell(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            INTERVENTION_ABILITY spell = activeCharacter.minion.interventionAbilitiesToResearch[index];
            UIManager.Instance.ShowSmallInfo(PlayerManager.Instance.allInterventionAbilitiesData[spell].description, Utilities.NormalizeStringUpperCaseFirstLetters(spell.ToString()));
        }
    }
    public void OnHoverExitSpell() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnHoverCombatAbility(object obj) {
        COMBAT_ABILITY action = activeCharacter.minion.combatAbility.type;
        UIManager.Instance.ShowSmallInfo(action.Description(), Utilities.NormalizeStringUpperCaseFirstLetters(action.ToString()));
    }
    public void OnHoverExitCombatAbility() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Stats
    private void UpdateStatInfo() {
        hpLbl.text = _activeCharacter.currentHP.ToString();
        attackLbl.text = _activeCharacter.attackPower.ToString();
        speedLbl.text = _activeCharacter.speed.ToString();
        if(characterPortrait.thisCharacter != null) {
            characterPortrait.UpdateLvl();
        }
    }
    #endregion

    #region Location
    private void UpdateLocationInfo() {
        if (_activeCharacter.currentRegion != null) {
            visitorLocationPortrait.SetLocation(_activeCharacter.currentRegion);
        } else {
            visitorLocationPortrait.SetLocation(_activeCharacter.specificLocation.region);
        }
        residentLocationPortrait.SetLocation(_activeCharacter.homeRegion);
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
        if (_activeCharacter.minion != null) {
            return;
        }
 
        statusTraitsLbl.text = string.Empty;
        normalTraitsLbl.text = string.Empty;
        
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
                if (!string.IsNullOrEmpty(statusTraitsLbl.text)) {
                    statusTraitsLbl.text += ", ";
                }
                statusTraitsLbl.text += "<b><color=" + color + "><link=" + '"' + i.ToString() + '"' + ">" + currTrait.name + "</link></color></b>";
            } else {
                string color = normalTextColor;
                if (currTrait.type == TRAIT_TYPE.BUFF) {
                    color = buffTextColor;
                } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                    color = flawTextColor;
                }
                if (!string.IsNullOrEmpty(normalTraitsLbl.text)) {
                    normalTraitsLbl.text += ", ";
                }
                normalTraitsLbl.text += "<b><color=" + color + "><link=" + '"' + i.ToString() + '"' + ">" + currTrait.name + "</link></color></b>";
            }
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
                    showCover: true, layer: 25, yesBtnText: "Trigger (" + trait.GetTriggerFlawManaCost(activeCharacter).ToString() + " Mana)",
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
        string reason = "You cannot trigger " + activeCharacter.name + "'s flaw because: ";
        List<string> reasons = trait.GetCannotTriggerFlawReasons(activeCharacter);
        for (int i = 0; i < reasons.Count; i++) {
            reason += "\n\t- " + reasons[i];
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
        //normalTraitsLbl.raycastTarget = true;
    }
    #endregion

    #region Buttons
    public void OnClickLogButton() {
        SetLogMenuState(!logParentGO.activeSelf);
    }
    public void OnCloseLog() {
        SetLogMenuState(false);
    }
    #endregion

    #region Items
    private void UpdateInventoryInfoFromSignal(SpecialToken token, Character character) {
        if (isShowing && _activeCharacter == character) {
            UpdateInventoryInfo();
        }
    }
    private void UpdateInventoryInfo() {
        //if (_activeCharacter.minion != null) {
        //    return;
        //}
        for (int i = 0; i < inventoryItemContainers.Length; i++) {
            ItemContainer currContainer = inventoryItemContainers[i];
            SpecialToken currInventoryItem = _activeCharacter.items.ElementAtOrDefault(i);
            currContainer.SetItem(currInventoryItem);
            if (activeCharacter.minion != null) {
                currContainer.transform.SetParent(minionItemsScrollView.content);
            } else {
                currContainer.transform.SetParent(itemsScrollView.content);
            }
        }
    }
    #endregion

    #region History
    private void UpdateHistory(object obj) {
        if (obj is Character && _activeCharacter != null && (obj as Character).id == _activeCharacter.id && _activeCharacter.minion == null) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        if (_activeCharacter.minion != null) {
            return;
        }
        //List<Log> characterHistory = new List<Log>(_activeCharacter.history.OrderByDescending(x => x.date.year).ThenByDescending(x => x.date.month).ThenByDescending(x => x.date.day).ThenByDescending(x => x.date.tick));
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if(i < _activeCharacter.history.Count) {
                Log currLog = _activeCharacter.history[_activeCharacter.history.Count - 1 - i];
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
            //if (currLog != null) {
                
            //} else {
            //}
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
    public void SetLogMenuState(bool state) {
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
        summary += "\nCurrent structure: " + activeCharacter.currentStructure?.ToString() ?? "None";
        summary += "\nPOI State: " + activeCharacter.state.ToString();
        summary += "\nDo Not Disturb: " + activeCharacter.doNotDisturb.ToString();
        summary += "\nDo Not Get Hungry: " + activeCharacter.doNotGetHungry.ToString();
        summary += "\nDo Not Get Tired: " + activeCharacter.doNotGetTired.ToString();
        summary += "\nDo Not Get Lonely: " + activeCharacter.doNotGetLonely.ToString();
        summary += "\nDo Not Recover HP: " + activeCharacter.doNotRecoverHP;
        summary += "\nFullness Time: " + (activeCharacter.fullnessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.fullnessForcedTick));
        summary += "\nTiredness Time: " + (activeCharacter.tirednessForcedTick == 0 ? "N/A" : GameManager.ConvertTickToTime(activeCharacter.tirednessForcedTick));
        summary += "\nRemaining Sleep Ticks: " + activeCharacter.currentSleepTicks;
        summary += "\nFood: " + activeCharacter.food;
        summary += "\nRole: " + activeCharacter.role.roleType.ToString();
        summary += "\nSexuality: " + activeCharacter.sexuality.ToString();
        summary += "\nMood: " + activeCharacter.moodValue.ToString() + "(" + activeCharacter.currentMoodType.ToString() + ")";
        summary += "\nHP: " + activeCharacter.currentHP.ToString() + "/" + activeCharacter.maxHP.ToString();
        summary += "\nIgnore Hostiles: " + activeCharacter.ignoreHostility.ToString();
        summary += "\nAttack Range: " + activeCharacter.characterClass.attackRange.ToString();
        summary += "\nAttack Speed: " + activeCharacter.attackSpeed.ToString();
        summary += "\nCurrent State: " + activeCharacter.stateComponent.currentState?.ToString() ?? "None";
        //summary += "\nState To Do: " + activeCharacter.stateComponent.stateToDo?.ToString() ?? "None";
        summary += "\nActions targetting this character: ";
        //if (activeCharacter.targettedByAction.Count > 0) {
        //    for (int i = 0; i < activeCharacter.targettedByAction.Count; i++) {
        //        summary += "\n" + activeCharacter.targettedByAction[i].goapName + " done by " + activeCharacter.targettedByAction[i].actor.name;
        //    }
        //} else {
        //    summary += "None";
        //}
        //summary += "\nActions advertised by this character: ";
        //if (activeCharacter.poiGoapActions.Count > 0) {
        //    for (int i = 0; i < activeCharacter.poiGoapActions.Count; i++) {
        //        summary += "|" + activeCharacter.poiGoapActions[i].ToString() + "|";
        //    }
        //} else {
        //    summary += "None";
        //}
        summary += "\n" + activeCharacter.GetNeedsSummary();
        summary += "\n\nAlter Egos: ";
        for (int i = 0; i < activeCharacter.alterEgos.Values.Count; i++) {
            summary += "\n" + activeCharacter.alterEgos.Values.ElementAt(i).GetAlterEgoSummary();
        }
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
    public void Death() {
        _activeCharacter.Death();
    }
    public void AssaultACharacter() {
        List<Character> characterPool = new List<Character>();
        for (int i = 0; i < _activeCharacter.specificLocation.charactersAtLocation.Count; i++) {
            Character character = _activeCharacter.specificLocation.charactersAtLocation[i];
            if (!character.isDead && !(character.currentParty.icon.isTravelling && character.currentParty.icon.travelLine != null)) {
                characterPool.Add(character);
            }
        }
        if(characterPool.Count > 0) {
            Character chosenCharacter = characterPool[UnityEngine.Random.Range(0, characterPool.Count)];
            _activeCharacter.CreateKnockoutJob(chosenCharacter);
        } else {
            Debug.LogError("No eligible characters to assault!");
        }
    }
    #endregion

    #region Relationships
    private void UpdateRelationships() {
        //CharacterRelationshipItem[] items = Utilities.GetComponentsInDirectChildren<CharacterRelationshipItem>(relationshipTraitsScrollView.content.gameObject);
        Utilities.DestroyChildren(relationshipTraitsScrollView.content);
        foreach (KeyValuePair<Relatable, IRelationshipData> keyValuePair in activeCharacter.relationshipContainer.relationships) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(relationshipItemPrefab.name, Vector3.zero, Quaternion.identity, relationshipTraitsScrollView.content);
            CharacterRelationshipItem item = go.GetComponent<CharacterRelationshipItem>();
            if (keyValuePair.Key is AlterEgoData) {
                item.Initialize(keyValuePair.Key as AlterEgoData, keyValuePair.Value);
            }
        }
    }
    private void OnRelationshipAdded(Relatable gainedBy, Relatable rel) {
        if (isShowing) {
            if (gainedBy == activeCharacter.currentAlterEgo || rel == activeCharacter.currentAlterEgo) {
                UpdateRelationships();
            }
        }
        
    }
    private void OnRelationshipRemoved(Relatable gainedBy, RELATIONSHIP_TRAIT trait, Relatable rel) {
        if (isShowing) {
            if (gainedBy == activeCharacter.currentAlterEgo || rel == activeCharacter.currentAlterEgo) {
                UpdateRelationships();
            }
        }
    }
    #endregion
}
