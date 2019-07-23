using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq;
using System;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    [Header("Currency")]
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI suppliesText;
    public TextMeshProUGUI impsText;

    [Header("Role Slots")]
    [SerializeField] private RectTransform roleSlotsParent;
    [SerializeField] private RoleSlotItem[] roleSlots;
    [SerializeField] private GameObject roleSlotItemPrefab;
    [SerializeField] private GameObject actionBtnPrefab;
    [SerializeField] private GameObject actionBtnTooltipGO;
    [SerializeField] private TextMeshProUGUI actionBtnTooltipLbl;
    [SerializeField] private GameObject actionBtnPointer;
    [SerializeField] private TextMeshProUGUI activeMinionTypeLbl;
    [SerializeField] private RectTransform activeMinionActionsParent;
    [SerializeField] private UI_InfiniteScroll roleSlotsInfiniteScroll;
    [SerializeField] private ScrollRect roleSlotsScrollRect;
    public UIHoverPosition roleSlotTooltipPos;

    [Header("Attack")]
    public GameObject attackGridGO;
    [SerializeField] private Sprite attackGridIconSprite;
    [SerializeField] private Sprite defenseGridIconSprite;
    public AttackSlotItem attackSlot;
    [SerializeField] private DefenseSlotItem defenseSlot;
    public SlotItem[] attackGridSlots;
    public Button startInvasionButton;

    [Header("Intel")]
    [SerializeField] private GameObject intelContainer;
    [SerializeField] private IntelItem[] intelItems;
    [SerializeField] private Toggle intelToggle;

    [Header("Provoke")]
    [SerializeField] private ProvokeMenu provokeMenu;

    [Header("Memories")]
    [SerializeField] private GameObject logItemPrefab;
    [SerializeField] private RectTransform memoriesParent;
    [SerializeField] private GameObject memoriesGO;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;
    private LogHistoryItem[] logHistoryItems;

    [Header("Miscellaneous")]
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private Image combatGridAssignerIcon;
    [SerializeField] private GameObject gameOverGO;
    [SerializeField] private TextMeshProUGUI gameOverDescriptionText;
    [SerializeField] private GameObject successfulAreaCorruptionGO;

    [Header("Minions")]
    [SerializeField] private GameObject startingMinionPickerGO;
    [SerializeField] private MinionCard startingMinionCard1;
    [SerializeField] private MinionCard startingMinionCard2;
    [SerializeField] private MinionCard startingMinionCard3;
    [SerializeField] private GameObject minionLeaderPickerParent;
    [SerializeField] private GameObject minionLeaderPickerPrefab;
    private List<MinionLeaderPicker> minionLeaderPickers;
    private MinionLeaderPicker tempCurrentMinionLeaderPicker;

    [Header("Corruption and Threat")]
    [SerializeField] private GameObject corruptTileConfirmationGO;
    [SerializeField] private TextMeshProUGUI corruptTileConfirmationLbl;
    [SerializeField] private Slider threatMeter;

    [Header("Summons")]
    [SerializeField] private Image currentSummonImg;
    [SerializeField] private TextMeshProUGUI currentSummonCountLbl;
    [SerializeField] private Button summonBtn;
    [SerializeField] private GameObject summonCover;
    [SerializeField] private UIHoverPosition summonTooltipPos;
    private bool isSummoning = false; //if the player has clicked the summon button and is targetting a tile to place the summon on.

    [Header("Artifacts")]
    [SerializeField] private Image currentArtifactImg;
    [SerializeField] private TextMeshProUGUI currentArtifactCountLbl;
    [SerializeField] private Button summonArtifactBtn;
    [SerializeField] private GameObject summonArtifactCover;
    [SerializeField] private UIHoverPosition summonArtifactTooltipPos;
    private bool isSummoningArtifact = false; //if the player has clicked the summon artifact button and is targetting a tile to place the summon on.

    [Header("Combat Abilities")]
    public GameObject combatAbilityGO;
    public GameObject combatAbilityButtonPrefab;
    public List<CombatAbilityButton> currentCombatAbilityButtons { get; private set; }

    [Header("Story Events")]
    [SerializeField] private StoryEventUI storyEventUI;

    [Header("Replace UI")]
    public ReplaceUI replaceUI;

    [Header("Level Up UI")]
    public LevelUpUI levelUpUI;
    //[Header("Actions")]
    //[SerializeField] private int maxActionPages;
    //[SerializeField] private GameObject actionPagePrefab;
    //[SerializeField] private RectTransform actionPageParent;
    //private ActionsPage[] actionPages;
    //private int currentActionPage;

    private bool _isScrollingUp;
    private bool _isScrollingDown;
    public CombatGrid attackGridReference { get; private set; }
    public CombatGrid defenseGridReference { get; private set; }

    void Awake() {
        Instance = this;
        //minionItems = new List<PlayerCharacterItem>();
        //Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        //Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }
    //void Start() {
    //    Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
    //}
    public void UpdateUI() {
        if (PlayerManager.Instance.player == null) {
            return;
        }
        //manaText.text = PlayerManager.Instance.player.currencies[CURRENCY.MANA].ToString();
        //redMagicText.text = "" + PlayerManager.Instance.player.redMagic;
        //greenMagicText.text = "" + PlayerManager.Instance.player.greenMagic;
        //suppliesText.text = PlayerManager.Instance.player.currencies[CURRENCY.SUPPLY].ToString();
        //impsText.text = "Imps: " + PlayerManager.Instance.player.currencies[CURRENCY.IMP].ToString() + "/" + PlayerManager.Instance.player.maxImps.ToString();
        //threatFiller.fillAmount = (float) PlayerManager.Instance.player.threatLevel / 100f;
    }

    public void Initialize() {
        //attack/raid
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetNeededType(typeof(Character));
            currSlot.SetSlotIndex(i);
        }
        minionLeaderPickers = new List<MinionLeaderPicker>();
        currentCombatAbilityButtons = new List<CombatAbilityButton>();

        storyEventUI.Initialize();

        LoadRoleSlots();
        LoadAttackSlot();

        UpdateIntel();
        InitializeMemoriesMenu();
        UpdateSummonsInteraction();
        UpdateArtifactsInteraction();

        //actions
        //actionPages = new ActionsPage[maxActionPages];
        //for (int i = 0; i < maxActionPages; i++) {
        //    GameObject pageGO = GameObject.Instantiate(actionPagePrefab, actionPageParent);
        //    ActionsPage page = pageGO.GetComponent<ActionsPage>();
        //    page.Initialize();
        //    actionPages[i] = page;
        //}

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
        Messenger.AddListener<Intel>(Signals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
        Messenger.AddListener<Intel>(Signals.PLAYER_REMOVED_INTEL, OnIntelRemoved);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);

        //job action buttons
        Messenger.AddListener(Signals.HAS_SEEN_ACTION_BUTTONS, OnSeenActionButtons);
        Messenger.AddListener<Minion, PlayerJobAction>(Signals.MINION_LEARNED_INTERVENE_ABILITY, OnMinionLearnedInterventionAbility);

        //summons
        Messenger.AddListener<Summon>(Signals.PLAYER_GAINED_SUMMON, OnGainNewSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_REMOVED_SUMMON, OnRemoveSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_PLACED_SUMMON, OnSummonUsed);

        //Artifacts
        Messenger.AddListener<Artifact>(Signals.PLAYER_GAINED_ARTIFACT, OnGainNewArtifact);
        Messenger.AddListener<Artifact>(Signals.PLAYER_REMOVED_ARTIFACT, OnRemoveArtifact);
        Messenger.AddListener<Artifact>(Signals.PLAYER_USED_ARTIFACT, OnUsedArtifact);

        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);

        //key presses
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);
    }

    #region Listeners
    private void OnAreaMapOpened(Area area) {
        UpdateSummonsInteraction();
        UpdateArtifactsInteraction();
        startInvasionButton.gameObject.SetActive(true);
        
    }
    private void OnAreaMapClosed(Area area) {
        UpdateSummonsInteraction();
        UpdateArtifactsInteraction();
        startInvasionButton.gameObject.SetActive(false);
    }
    private void OnKeyPressed(KeyCode pressedKey) {
        if (pressedKey == KeyCode.Escape) {
            if (PlayerManager.Instance.player.currentActivePlayerJobAction != null) {
                PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
                CursorManager.Instance.ClearLeftClickActions();
            }
            if (isSummoning) {
                CancelSummon();
            } else if (isSummoningArtifact) {
                CancelSummonArtifact();
            }
        } else if (pressedKey == KeyCode.Mouse0) {
            //left click
            if (isSummoning) {
                TryPlaceSummon();
            } else if (isSummoningArtifact) {
                TryPlaceArtifact();
            }
        }
    }
    #endregion

    #region Role Slots
    private void OnSeenActionButtons() {
        actionBtnPointer.SetActive(!PlayerManager.Instance.player.hasSeenActionButtonsOnce);
    }
    int currentlyeShowingSlotIndex = 0;
    private void LoadRoleSlots() {
        int currIndex = 0;
        roleSlots = new RoleSlotItem[PlayerManager.Instance.player.minions.Length];
        for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
            GameObject roleSlotGO = UIManager.Instance.InstantiateUIObject(roleSlotItemPrefab.name, roleSlotsParent);
            RoleSlotItem roleSlot = roleSlotGO.GetComponent<RoleSlotItem>();
            //roleSlot.SetSlotJob(keyValuePair.Key);
            roleSlot.Initialize();
            roleSlots[currIndex] = roleSlot;
            currIndex++;
        }
        //foreach (KeyValuePair<JOB, PlayerJobData> keyValuePair in PlayerManager.Instance.player.roleSlots) {
        //    GameObject roleSlotGO = UIManager.Instance.InstantiateUIObject(roleSlotItemPrefab.name, roleSlotsParent);
        //    RoleSlotItem roleSlot = roleSlotGO.GetComponent<RoleSlotItem>();
        //    roleSlot.SetSlotJob(keyValuePair.Key);
        //    roleSlots[currIndex] = roleSlot;
        //    currIndex++;
        //}
        roleSlotsInfiniteScroll.Init();
        //LoadActionButtonsForActiveJob(roleSlots[currentlyeShowingSlotIndex]);
        UpdateRoleSlotScroll();
    }
    public void UpdateRoleSlots() {
        for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
            roleSlots[i].SetMinion(PlayerManager.Instance.player.minions[i]);
        }
        UpdateRoleSlotScroll();
    }
    public void ScrollNext() {
        currentlyeShowingSlotIndex += 1;
        if (currentlyeShowingSlotIndex == roleSlots.Length) {
            currentlyeShowingSlotIndex = 0;
        }
        UpdateRoleSlotScroll();
    }
    public void ScrollPrevious() {
        currentlyeShowingSlotIndex -= 1;
        if (currentlyeShowingSlotIndex < 0) {
            currentlyeShowingSlotIndex = roleSlots.Length - 1;
        }
        UpdateRoleSlotScroll();
    }
    public void ScrollRoleSlotTo(int index) {
        if (currentlyeShowingSlotIndex == index) {
            return;
        }
        currentlyeShowingSlotIndex = index;
        UpdateRoleSlotScroll();
        PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
        CursorManager.Instance.ClearLeftClickActions(); //TODO: Change this to no clear all actions but just the ones concerened with the player abilities
    }
    private void UpdateRoleSlotScroll() {
        RoleSlotItem slotToShow = roleSlots[currentlyeShowingSlotIndex];
        activeMinionTypeLbl.text = Utilities.NormalizeString(slotToShow.slotJob.ToString());
        Utilities.ScrolRectSnapTo(roleSlotsScrollRect, slotToShow.GetComponent<RectTransform>());
        LoadActionButtonsForActiveJob(slotToShow);
    }

    //private void ShowActionButtonsFor(IPointOfInterest poi) {
    //    if (UIManager.Instance.IsShareIntelMenuOpen()) {
    //        return;
    //    }
    //    Utilities.DestroyChildren(jobActionsParent);
    //    for (int i = 0; i < roleSlots.Length; i++) {
    //        RoleSlotItem item = roleSlots[i];
    //        if (PlayerManager.Instance.player.roleSlots[item.slotJob].assignedCharacter != null) {
    //            item.ShowActionButtons(poi, jobActionsParent);
    //        }
    //    }
    //    jobActionsParent.gameObject.SetActive(true);
    //    actionBtnPointer.SetActive(!PlayerManager.Instance.player.hasSeenActionButtonsOnce);
    //}
    private void HideActionButtons() {
        actionBtnPointer.SetActive(false);
    }
    public void ShowActionBtnTooltip(string message, string header) {
        string m = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            m = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
        }
        m += "<line-height=70%><size=16>" + message;

        m = m.Replace("\\n", "\n");

        actionBtnTooltipLbl.text = m;
        actionBtnTooltipGO.gameObject.SetActive(true);
    }
    public void HideActionBtnTooltip() {
        actionBtnTooltipGO.gameObject.SetActive(false);
    }
    //public void OnStartChangeRoleSlotPage() {
    //    Utilities.DestroyChildren(activeMinionActionsParent);
    //}
    //public void OnChangeRoleSlotPage(int page) {
    //    RoleSlotItem slot = roleSlots[page];
    //    activeMinionTypeLbl.text = Utilities.NormalizeString(slot.slotJob.ToString());
    //    LoadActionButtonsForActiveJob(slot);
    //}
    private void LoadActionButtonsForActiveJob(RoleSlotItem active) {
        Utilities.DestroyChildren(activeMinionActionsParent);
        Minion minion = active.minion;
        if(minion != null) {
            for (int i = 0; i < minion.interventionAbilities.Length; i++) {
                PlayerJobAction jobAction = minion.interventionAbilities[i];
                if(jobAction != null) {
                    GameObject jobGO = UIManager.Instance.InstantiateUIObject(actionBtnPrefab.name, activeMinionActionsParent);
                    PlayerJobActionButton actionBtn = jobGO.GetComponent<PlayerJobActionButton>();
                    actionBtn.SetJobAction(jobAction, minion.character);
                    actionBtn.SetClickAction(() => PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(jobAction));
                }
            }
        }

    }
    public PlayerJobActionButton GetPlayerJobActionButton(PlayerJobAction action) {
        PlayerJobActionButton[] buttons = Utilities.GetComponentsInDirectChildren<PlayerJobActionButton>(activeMinionActionsParent.gameObject);
        for (int i = 0; i < buttons.Length; i++) {
            PlayerJobActionButton currButton = buttons[i];
            if (currButton.action == action) {
                return currButton;
            }
        }
        return null;
    }
    private void OnMinionLearnedInterventionAbility(Minion minion, PlayerJobAction action) {
        RoleSlotItem currActive = roleSlots[currentlyeShowingSlotIndex];
        if (currActive.minion == minion) {
            LoadActionButtonsForActiveJob(currActive);
        }
    }
    #endregion

    #region Attack UI
    private void LoadAttackSlot() {
        attackGridReference = new CombatGrid();
        defenseGridReference = new CombatGrid();
        attackGridReference.Initialize();
        defenseGridReference.Initialize();
        attackSlot.UpdateVisuals();
        defenseSlot.UpdateVisuals();
    }
    public void ShowAttackGrid() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetItemDroppedCallback(OnDropOnAttackGrid);
            currSlot.SetItemDroppedOutCallback(OnDroppedOutFromAttackGrid);
        }
        attackGridGO.SetActive(true);
        combatGridAssignerIcon.sprite = attackGridIconSprite;
        SetAttackGridCharactersFromPlayer();
    }
    public void ShowDefenseGrid() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetItemDroppedCallback(OnDropOnDefenseGrid);
            currSlot.SetItemDroppedOutCallback(OnDroppedOutFromDefenseGrid);
        }
        attackGridGO.SetActive(true);
        combatGridAssignerIcon.sprite = defenseGridIconSprite;
        SetDefenseGridCharactersFromPlayer();
    }
    public void OnClickConfirmCombatGrid() {
        if(combatGridAssignerIcon.sprite == attackGridIconSprite) {
            attackSlot.OnClickConfirm();
        } else {
            defenseSlot.OnClickConfirm();
        }
    }
    public void HideCombatGrid() {
        attackGridGO.SetActive(false);
    }
    private void OnDropOnAttackGrid(object obj, int index) {
        if(obj is Character) {
            Character character = obj as Character;
            if (attackGridReference.IsCharacterInGrid(character)) {
                attackGridSlots[index].PlaceObject(attackGridReference.slots[index].character);
                return;
            }
            attackGridReference.AssignCharacterToGrid(character, index, true);
            UpdateAttackGridSlots();
        }
    }
    private void OnDropOnDefenseGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            if (defenseGridReference.IsCharacterInGrid(character)) {
                attackGridSlots[index].PlaceObject(defenseGridReference.slots[index].character);
                return;
            }
            defenseGridReference.AssignCharacterToGrid(character, index, true);
            UpdateDefenseGridSlots();
        }
    }
    private void OnDroppedOutFromAttackGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            attackGridReference.RemoveCharacterFromGrid(character);
            UpdateAttackGridSlots();
        }
    }
    private void OnDroppedOutFromDefenseGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            defenseGridReference.RemoveCharacterFromGrid(character);
            UpdateDefenseGridSlots();
        }
    }
    private bool IsObjectValidForAttack(object obj, SlotItem slotItem) {
        if (obj is Character) {
            Character character = obj as Character;
            if (character.characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
                if (attackGridSlots[0] == slotItem || attackGridSlots[1] == slotItem) {
                    return true;
                }
            } else {
                if (attackGridSlots[2] == slotItem || attackGridSlots[3] == slotItem) {
                    return true;
                }
            }
        }
        return false;
    }
    private void SetAttackGridCharactersFromPlayer() {
        for (int i = 0; i < attackGridReference.slots.Length; i++) {
            attackGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void SetDefenseGridCharactersFromPlayer() {
        for (int i = 0; i < defenseGridReference.slots.Length; i++) {
            defenseGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void UpdateAttackGridSlots() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void UpdateDefenseGridSlots() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            attackGridSlots[i].PlaceObject(defenseGridReference.slots[i].character);
        }
    }
    public void OnClickStartInvasion() {
        PlayerManager.Instance.player.StartInvasion(InteriorMapManager.Instance.currentlyShowingArea);
        ShowCombatAbilityUI();
    }
    public void StopInvasion() {
        startInvasionButton.interactable = true;
        HideCombatAbilityUI();
    }
    #endregion

    #region Miscellaneous
    public void SetBottomMenuTogglesState(bool isOn) {
        intelToggle.isOn = isOn;
    }
    #endregion


    public string previousMenu;
    private void OnMenuOpened(UIMenu menu) {
        if (menu is LandmarkInfoUI) {
            UIManager.Instance.ShowMinionsMenu();
        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is LandmarkInfoUI) {
            if (string.IsNullOrEmpty(previousMenu)) {
                UIManager.Instance.HideRightMenus();
            } else if (previousMenu.Equals("minion")) {
                UIManager.Instance.ShowMinionsMenu();
            } else if (previousMenu.Equals("character")) {
                UIManager.Instance.ShowCharacterTokenMenu();
            } else if (previousMenu.Equals("location")) {
                UIManager.Instance.ShowLocationTokenMenu();
            } else if (previousMenu.Equals("faction")) {
                UIManager.Instance.ShowFactionTokenMenu();
            }
        } 
        //else if (menu is CharacterInfoUI || menu is TileObjectInfoUI) {
        //    HideActionButtons();
        //}
    }

    #region Intel
    private void OnIntelObtained(Intel intel) {
        UpdateIntel();
    }
    private void OnIntelRemoved(Intel intel) {
        UpdateIntel();
    }
    private void UpdateIntel() {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            Intel intel = PlayerManager.Instance.player.allIntel.ElementAtOrDefault(i);
            currItem.SetIntel(intel);
        }
    }
    public void SetIntelMenuState(bool state) {
        if (intelToggle.isOn == state) {
            return; //ignore change
        }
        intelToggle.isOn = state;
        if (!intelToggle.isOn) {
            OnCloseIntelMenu();
        }
    }
    private void OnCloseIntelMenu() {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.ClearClickActions();
        }
    }
    public void SetIntelItemClickActions(IntelItem.OnClickAction clickAction) {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.SetClickAction(clickAction);
        }
    }
    public void AddIntelItemOtherClickActions(System.Action clickAction) {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.AddOtherClickAction(clickAction);
        }
    }
    private void OnOpenShareIntelMenu() {
        intelToggle.isOn = false;
        intelToggle.interactable = false;
        //for (int i = 0; i < roleSlots.Length; i++) {
        //    RoleSlotItem rsi = roleSlots[i];
        //    rsi.HideActionButtons();
        //    rsi.OverrideDraggableState(false);
        //}
        //assignBtn.interactable = false;

        //if (UIManager.Instance.characterInfoUI.isShowing || UIManager.Instance.tileObjectInfoUI.isShowing) {
        //    HideActionButtons();
        //}
    }
    private void OnCloseShareIntelMenu() {
        intelToggle.interactable = true;
        //for (int i = 0; i < roleSlots.Length; i++) {
        //    RoleSlotItem rsi = roleSlots[i];
        //    //rsi.UpdateActionButtons();
        //    rsi.OverrideDraggableState(true);
        //}
        //assignBtn.interactable = true;
        //if (UIManager.Instance.characterInfoUI.isShowing) {
        //    ShowActionButtonsFor(UIManager.Instance.characterInfoUI.activeCharacter);
        //}else if (UIManager.Instance.tileObjectInfoUI.isShowing) {
        //    ShowActionButtonsFor(UIManager.Instance.tileObjectInfoUI.activeTileObject);
        //}
    }
    public void ShowPlayerIntels(bool state) {
        intelContainer.SetActive(state);
        RectTransform rt = UIManager.Instance.playerNotifGO.transform as RectTransform;
        Vector3 previousPos = rt.anchoredPosition;
        if (!state) {
            rt.anchoredPosition = new Vector3(-640f, previousPos.y, previousPos.z);
        } else {
            rt.anchoredPosition = new Vector3(-1150f, previousPos.y, previousPos.z);
        }
    }
    #endregion

    #region Provoke
    public void OpenProvoke(Character minion, Character target) {
        provokeMenu.Open(target, minion);
    }
    #endregion

    #region Memories
    public void ShowMemories(Character character) {
        memoriesGO.SetActive(true);
        List<Log> characterHistory = new List<Log>(character.history.OrderByDescending(x => x.date.year).ThenByDescending(x => x.date.month).ThenByDescending(x => x.date.day).ThenByDescending(x => x.date.tick));
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
    private void InitializeMemoriesMenu() {
        logHistoryItems = new LogHistoryItem[CharacterManager.MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < CharacterManager.MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logItemPrefab.name, Vector3.zero, Quaternion.identity, memoriesParent);
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }
    }
    #endregion

    #region Minions
    public void ShowStartingMinionPicker() {
        startingMinionCard1.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
        startingMinionCard2.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
        startingMinionCard3.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));

        startingMinionPickerGO.SetActive(true);
    }
    public void HideStartingMinionPicker() {
        startingMinionPickerGO.SetActive(false);
    }
    public void Reroll1() {
        startingMinionCard1.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
    }
    public void Reroll2() {
        startingMinionCard2.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
    }
    public void Reroll3() {
        startingMinionCard3.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
    }
    public void OnClickStartGame() {
        //TODO: add minions to minion list
        HideStartingMinionPicker();
        PlayerManager.Instance.player.AddMinion(startingMinionCard1.minion);
        PlayerManager.Instance.player.AddMinion(startingMinionCard2.minion);
        PlayerManager.Instance.player.AddMinion(startingMinionCard3.minion);
        PlayerManager.Instance.player.GainSummon(SUMMON_TYPE.Wolf);
        PlayerManager.Instance.player.GainSummon(SUMMON_TYPE.Skeleton);
        PlayerManager.Instance.player.GainSummon(SUMMON_TYPE.Golem);
        PlayerManager.Instance.player.GainSummon(SUMMON_TYPE.Succubus);
        PlayerManager.Instance.player.GainSummon(SUMMON_TYPE.Incubus);
        PlayerManager.Instance.player.GainSummon(SUMMON_TYPE.ThiefSummon);
        PlayerManager.Instance.player.GainArtifact(ARTIFACT_TYPE.Necronomicon);
        PlayerManager.Instance.player.GainArtifact(ARTIFACT_TYPE.Chaos_Orb);
        PlayerManager.Instance.player.GainArtifact(ARTIFACT_TYPE.Hermes_Statue);
        PlayerManager.Instance.player.GainArtifact(ARTIFACT_TYPE.Ankh_Of_Anubis);
        PlayerManager.Instance.player.GainArtifact(ARTIFACT_TYPE.Miasma_Emitter);
        PlayerManager.Instance.player.SetMinionLeader(startingMinionCard1.minion);
    }
    private void ShowSelectMinionLeader() {
        Utilities.DestroyChildren(minionLeaderPickerParent.transform);
        minionLeaderPickers.Clear();
        tempCurrentMinionLeaderPicker = null;
        for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
            Minion minion = PlayerManager.Instance.player.minions[i];
            if(minion != null) {
                GameObject go = GameObject.Instantiate(minionLeaderPickerPrefab, minionLeaderPickerParent.transform);
                MinionLeaderPicker minionLeaderPicker = go.GetComponent<MinionLeaderPicker>();
                minionLeaderPicker.SetMinion(minion);
                minionLeaderPickers.Add(minionLeaderPicker);
                if (PlayerManager.Instance.player.currentMinionLeader == null) {
                    if (tempCurrentMinionLeaderPicker == null) {
                        minionLeaderPicker.imgHighlight.gameObject.SetActive(true);
                        tempCurrentMinionLeaderPicker = minionLeaderPicker;
                    }
                } else if(minion == PlayerManager.Instance.player.currentMinionLeader) {
                    minionLeaderPicker.imgHighlight.gameObject.SetActive(true);
                    tempCurrentMinionLeaderPicker = minionLeaderPicker;
                }
            }
        }
    }
    public void TemporarySetMinionLeader(MinionLeaderPicker leaderPicker) {
        leaderPicker.imgHighlight.gameObject.SetActive(true);
        tempCurrentMinionLeaderPicker.imgHighlight.gameObject.SetActive(false);
        tempCurrentMinionLeaderPicker = leaderPicker;
    }
    #endregion

    #region Corruption and Threat
    public void InitializeThreatMeter() {
        threatMeter.minValue = 0f;
        threatMeter.maxValue = Player.MAX_THREAT;
        threatMeter.value = 0f;
    }
    public void ShowCorruptTileConfirmation(HexTile tile) {
        if (tile.CanBeCorrupted() && tile.elevationType != ELEVATION.WATER && !PlayerManager.Instance.player.isTileCurrentlyBeingCorrupted && !tile.isCorrupted) { //
            PlayerManager.Instance.player.SetCurrentTileBeingCorrupted(tile);
            if (tile.areaOfTile != null) {
                corruptTileConfirmationLbl.text = "To corrupt this area, you must defeat all residents within. Once you proceeed there is no going back. Do you wish to take on this settlement?";
            } else {
                corruptTileConfirmationLbl.text = "Corrupt this Area?";
            }
            corruptTileConfirmationGO.SetActive(true);
            ShowSelectMinionLeader();
        }
    }
    public void HideCorruptTileConfirmation() {
        corruptTileConfirmationGO.SetActive(false);
    }
    public void OnClickYesCorruption() {
        HideCorruptTileConfirmation();
        if (PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile != null) {
            GameManager.Instance.SetOnlyTickDays(false);
            InteriorMapManager.Instance.TryShowAreaMap(PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile);
        } else {
            PlayerManager.Instance.player.CorruptATile();
        }
        if (tempCurrentMinionLeaderPicker != null) {
            PlayerManager.Instance.player.SetMinionLeader(tempCurrentMinionLeaderPicker.minion);
            if (PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile == null) {
                StoryEvent e = PlayerManager.Instance.player.currentTileBeingCorrupted.GetRandomStoryEvent();
                if (e != null) {
                    Debug.Log("Will show event " + e.name);
                    if (e.trigger == STORY_EVENT_TRIGGER.IMMEDIATE) {
                        //show story event UI
                        storyEventUI.ShowEvent(e, true);
                    } else if (e.trigger == STORY_EVENT_TRIGGER.MID) { //schedule show event UI based on trigger.
                        int difference = Mathf.Abs(GameManager.Instance.Today().day - (GameManager.Instance.Today().day + PlayerManager.Instance.player.currentTileBeingCorrupted.corruptDuration));
                        int day = UnityEngine.Random.Range(1, difference);
                        GameDate dueDate = GameManager.Instance.Today().AddDays(day);
                        SchedulingManager.Instance.AddEntry(dueDate, () => storyEventUI.ShowEvent(e, true), null);
                    } else if (e.trigger == STORY_EVENT_TRIGGER.END) {
                        GameDate dueDate = GameManager.Instance.Today().AddDays(PlayerManager.Instance.player.currentTileBeingCorrupted.corruptDuration);
                        SchedulingManager.Instance.AddEntry(dueDate, () => storyEventUI.ShowEvent(e, true), null);
                    }
                }
            }
        }
    }
    public void OnClickNoCorruption() {
        HideCorruptTileConfirmation();
    }
    public void UpdateThreatMeter() {
        threatMeter.value = PlayerManager.Instance.player.threat;
    }
    #endregion

    #region Summons
    private SUMMON_TYPE currentlySelectedSummon; //the summon type that is currently shown in the UI
    private void UpdateSummonsInteraction() {
        bool state = PlayerManager.Instance.player.GetTotalAvailableSummonsCount() == 0;
        summonCover.SetActive(state);
        if (currentlySelectedSummon == SUMMON_TYPE.None) {
            summonBtn.interactable = !state && InteriorMapManager.Instance.isAnAreaMapShowing;
        } else {
            summonBtn.interactable = !state && InteriorMapManager.Instance.isAnAreaMapShowing && PlayerManager.Instance.player.GetAvailableSummonsOfTypeCount(currentlySelectedSummon) > 0;
        }
       
    }
    public void OnGainNewSummon(Summon newSummon) {
        UpdateSummonsInteraction();
        if (currentlySelectedSummon == SUMMON_TYPE.None) {
            SetCurrentlySelectedSummon(newSummon.summonType);
        }
        //AssignNewActionToLatestItem(newSummon);
    }
    public void OnRemoveSummon(Summon summon) {
        UpdateSummonsInteraction();
        if (PlayerManager.Instance.player.GetTotalAvailableSummonsCount() == 0) { //the player has no more summons left
            SetCurrentlySelectedSummon(SUMMON_TYPE.None);
        } else if (summon.summonType == currentlySelectedSummon 
            && PlayerManager.Instance.player.GetAvailableSummonsOfTypeCount(summon.summonType) == 0) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleSummons(1);
        }
    }
    private void OnSummonUsed(Summon summon) {
        UpdateSummonsInteraction();
        if (PlayerManager.Instance.player.GetTotalAvailableSummonsCount() == 0) { //the player has no more summons left
            SetCurrentlySelectedSummon(SUMMON_TYPE.None);
        } else if (summon.summonType == currentlySelectedSummon
            && PlayerManager.Instance.player.GetAvailableSummonsOfTypeCount(summon.summonType) == 0) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleSummons(1);
        }
    }
    public void SetCurrentlySelectedSummon(SUMMON_TYPE type) {
        currentlySelectedSummon = type;
        currentSummonImg.sprite = CharacterManager.Instance.GetSummonSettings(type).summonPortrait;
        currentSummonCountLbl.text = PlayerManager.Instance.player.GetAvailableSummonsOfTypeCount(type).ToString();
        UpdateSummonsInteraction();
    }
    public void CycleSummons(int cycleDirection) {
        SUMMON_TYPE[] types = Utilities.GetEnumValues<SUMMON_TYPE>();
        int index = Array.IndexOf(types, currentlySelectedSummon);
        while (true) {
            int next = index + cycleDirection;
            if (next >= types.Length) {
                next = 0;
            } else if (next <= 0) {
                next = types.Length - 1;
            }
            index = next;
            SUMMON_TYPE type = types[index];
            if (PlayerManager.Instance.player.GetAvailableSummonsOfTypeCount(type) > 0) { //Player has a summon of that type, select that.
                SetCurrentlySelectedSummon(type);
                break;
            }
        }
    }
    public void ShowSummonTooltip() {
        string header = currentlySelectedSummon.SummonName();
        string message;
        switch (currentlySelectedSummon) {
            case SUMMON_TYPE.Wolf:
                message = "Summon a wolf to run amok.";
                break;
            case SUMMON_TYPE.Skeleton:
                message = "Summon a skeleton that will abduct a random character.";
                break;
            case SUMMON_TYPE.Golem:
                message = "Summon a stone golem that can sustain alot of hits.";
                break;
            case SUMMON_TYPE.Succubus:
                message = "Summon a succubus that will seduce a male character and eliminate him.";
                break;
            case SUMMON_TYPE.Incubus:
                message = "Summon a succubus that will seduce a female character and eliminate her.";
                break;
            case SUMMON_TYPE.ThiefSummon:
                message = "Summon a thief that will steal items from the settlements warehouse.";
                break;
            default:
                message = "Summon a " + Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedSummon.ToString());
                break;
        }
        UIManager.Instance.ShowSmallInfo(message, summonTooltipPos, header);
    }
    public void HideSummonTooltip() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnClickSummon() {
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Target);
        isSummoning = true;
    }
    private void TryPlaceSummon() {
        isSummoning = false;
        if (!UIManager.Instance.IsMouseOnUI()) {
            LocationGridTile tile = InteriorMapManager.Instance.GetTileFromMousePosition();
            Summon summonToPlace = PlayerManager.Instance.player.GetAvailableSummonOfType(currentlySelectedSummon);
            summonToPlace.CreateMarker();
            summonToPlace.marker.InitialPlaceMarkerAt(tile);
            //PlayerManager.Instance.player.RemoveSummon(summonToPlace);
            summonToPlace.OnPlaceSummon(tile);
            Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summonToPlace);
        }
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
    }
    private void CancelSummon() {
        isSummoning = false;
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
    }
    #endregion

    #region Lose Condition
    public void GameOver(string descriptionText) {
        gameOverDescriptionText.text = descriptionText;
        gameOverGO.SetActive(true);
    }
    public void BackToMainMenu() {
        LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
    #endregion

    #region Area Corruption
    public void SuccessfulAreaCorruption() {
        successfulAreaCorruptionGO.SetActive(true);
    }
    public void BackToWorld() {
        Area closedArea = InteriorMapManager.Instance.HideAreaMap();
        if (PlayerManager.Instance.player.summons.Count > 0) {
            SetCurrentlySelectedSummon(PlayerManager.Instance.player.summons.Keys.FirstOrDefault());
        } else {
            SetCurrentlySelectedSummon(SUMMON_TYPE.None);
        }
        SetCurrentlySelectedArtifact(PlayerManager.Instance.player.artifacts.FirstOrDefault());
        successfulAreaCorruptionGO.SetActive(false);
        InteriorMapManager.Instance.DestroyAreaMap(closedArea);

        if (LandmarkManager.Instance.AreAllNonPlayerAreasCorrupted()) {
            GameOver("You have conquered all settlements! This world is now yours! Congratulations!");
        }
    }
    #endregion

    #region Combat Ability
    public void ShowCombatAbilityUI() {
        PopulateCombatAbilities();
        combatAbilityGO.SetActive(true);
    }
    public void HideCombatAbilityUI() {
        combatAbilityGO.SetActive(false);
    }
    private void PopulateCombatAbilities() {
        currentCombatAbilityButtons.Clear();
        Utilities.DestroyChildren(combatAbilityGO.transform);
        for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
            Minion currMinion = PlayerManager.Instance.player.minions[i];
            if(currMinion != null) {
                GameObject go = GameObject.Instantiate(combatAbilityButtonPrefab, combatAbilityGO.transform);
                CombatAbilityButton abilityButton = go.GetComponent<CombatAbilityButton>();
                abilityButton.SetCombatAbility(currMinion.combatAbility);
                currentCombatAbilityButtons.Add(abilityButton);
            }
        }
    }
    public CombatAbilityButton GetCombatAbilityButton(CombatAbility ability) {
        for (int i = 0; i < currentCombatAbilityButtons.Count; i++) {
            if (currentCombatAbilityButtons[i].ability == ability) {
                return currentCombatAbilityButtons[i];
            }
        }
        return null;
    }
    #endregion

    #region Artifacts
    private Artifact currentlySelectedArtifact; //the artifact that is currently shown in the UI
    private void UpdateArtifactsInteraction() {
        bool state = PlayerManager.Instance.player.GetTotalAvailableArtifactCount() == 0;
        summonArtifactCover.SetActive(state);
        summonArtifactBtn.interactable = !state && InteriorMapManager.Instance.isAnAreaMapShowing;
    }
    private void OnGainNewArtifact(Artifact newArtifact) {
        UpdateArtifactsInteraction();
        if (currentlySelectedArtifact == null) {
            SetCurrentlySelectedArtifact(newArtifact);
        }
        //AssignNewActionToLatestItem(newSummon);
    }
    private void OnRemoveArtifact(Artifact artifact) {
        UpdateArtifactsInteraction();
        if (PlayerManager.Instance.player.GetTotalAvailableArtifactCount() == 0) { //the player has no more artifacts left
            SetCurrentlySelectedArtifact(null);
        } else if (artifact == currentlySelectedArtifact
            && PlayerManager.Instance.player.GetAvailableArtifactsOfTypeCount(artifact.type) == 0) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleArtifacts(1);
        }
    }
    private void OnUsedArtifact(Artifact artifact) {
        UpdateArtifactsInteraction();
        if (PlayerManager.Instance.player.GetTotalAvailableArtifactCount() == 0) { //the player has no more artifacts left
            SetCurrentlySelectedArtifact(null);
        } else if (artifact == currentlySelectedArtifact
            && PlayerManager.Instance.player.GetAvailableArtifactsOfTypeCount(artifact.type) == 0) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleArtifacts(1);
        }
    }
    public void SetCurrentlySelectedArtifact(Artifact artifact) {
        currentlySelectedArtifact = artifact;
        if (currentlySelectedArtifact == null) {
            currentArtifactImg.sprite = CharacterManager.Instance.GetArtifactSettings(ARTIFACT_TYPE.None).artifactPortrait;
            currentArtifactCountLbl.text = "0";
        } else {
            currentArtifactImg.sprite = CharacterManager.Instance.GetArtifactSettings(artifact.type).artifactPortrait;
            currentArtifactCountLbl.text = PlayerManager.Instance.player.GetAvailableArtifactsOfTypeCount(artifact.type).ToString();
        }
        UpdateArtifactsInteraction();
    }
    public void CycleArtifacts(int cycleDirection) {
        ARTIFACT_TYPE[] types = Utilities.GetEnumValues<ARTIFACT_TYPE>();
        int index = Array.IndexOf(types, currentlySelectedArtifact?.type ?? ARTIFACT_TYPE.None);
        while (true) {
            int next = index + cycleDirection;
            if (next >= types.Length) {
                next = 0;
            } else if (next <= 0) {
                next = types.Length - 1;
            }
            index = next;
            ARTIFACT_TYPE type = types[index];
            Artifact artifact;
            if (PlayerManager.Instance.player.TryGetAvailableArtifactOfType(type, out artifact)) { //Player has a summon of that type, select that.
                SetCurrentlySelectedArtifact(artifact);
                break;
            }
        }
    }
    public void ShowArtifactTooltip() {
        string header = Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedArtifact.type.ToString());
        string message = PlayerManager.Instance.player.GetArtifactDescription(currentlySelectedArtifact.type);
        UIManager.Instance.ShowSmallInfo(message, summonArtifactTooltipPos, header);
    }
    public void HideArtifactTooltip() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnClickSummonArtifact() {
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Target);
        isSummoningArtifact = true;
    }
    private void TryPlaceArtifact() {
        isSummoningArtifact = false;
        if (!UIManager.Instance.IsMouseOnUI()) {
            LocationGridTile tile = InteriorMapManager.Instance.GetTileFromMousePosition();
            if (tile.objHere == null) {
                Artifact artifactToPlace = currentlySelectedArtifact;
                if (tile.structure.AddPOI(artifactToPlace, tile)) {
                    Messenger.Broadcast(Signals.PLAYER_USED_ARTIFACT, artifactToPlace);
                }
                //PlayerManager.Instance.player.RemoveArtifact(artifactToPlace);
            } else {
                Debug.LogWarning("There is already an object placed there. Not placing artifact");
            }
            //summonToPlace.CreateMarker();
            //summonToPlace.marker.InitialPlaceMarkerAt(tile);
            //PlayerManager.Instance.player.RemoveSummon(summonToPlace);
            //summonToPlace.OnPlaceSummon(tile);
            //Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summonToPlace);
        }
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
    }
    private void CancelSummonArtifact() {
        isSummoningArtifact = false;
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
    }
    #endregion

    //#region Actions
    //private void AssignNewActionToLatestItem(object obj) {
    //    ActionItem assignedTo = null;
    //    for (int i = 0; i < actionPages.Length; i++) {
    //        ActionsPage currPage = actionPages[i];
    //        ActionItem availableItem = currPage.GetUnoccupiedActionItem();
    //        if (availableItem != null) {
    //            availableItem.SetAction(obj);
    //            assignedTo = availableItem;
    //            break;
    //        }
    //    }
    //    if (assignedTo == null) {
    //        Debug.LogWarning("Could not assign new action " + obj.ToString() + " to an action item, because all pages are occupied!");
    //    }
    //}
    //#endregion

}

