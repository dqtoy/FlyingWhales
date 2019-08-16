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

    [Header("Role Slots")]
    [SerializeField] private RectTransform roleSlotsParent;
    [SerializeField] private RoleSlotItem[] roleSlots;
    [SerializeField] private GameObject roleSlotItemPrefab;
    [SerializeField] private GameObject actionBtnPrefab;
    [SerializeField] private GameObject actionBtnTooltipGO;
    [SerializeField] private TextMeshProUGUI actionBtnTooltipLbl;
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
    [SerializeField] private ScrollRect killSummaryScrollView;
    [SerializeField] private GameObject generalConfirmationGO;
    [SerializeField] private TextMeshProUGUI generalConfirmationTitleText;
    [SerializeField] private TextMeshProUGUI generalConfirmationBodyText;

    [Header("Start Picker")]
    [SerializeField] private GameObject startingMinionPickerGO;
    [SerializeField] private MinionCard startingMinionCard1;
    [SerializeField] private MinionCard startingMinionCard2;
    [SerializeField] private MinionCard startingMinionCard3;
    [SerializeField] private GameObject minionLeaderPickerParent;
    [SerializeField] private GameObject minionLeaderPickerPrefab;
    [SerializeField] private TextMeshProUGUI selectMinionLeaderText;
    private List<MinionLeaderPicker> minionLeaderPickers;
    private MinionLeaderPicker tempCurrentMinionLeaderPicker;
    [SerializeField] private Image[] startingAbilityIcons;
    [SerializeField] private TextMeshProUGUI[] startingAbilityLbls; //NOTE: This must always have the same length as startingAbilityIcons


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
    public StoryEventUI storyEventUI;

    [Header("Replace UI")]
    public ReplaceUI replaceUI;

    [Header("Level Up UI")]
    public LevelUpUI levelUpUI;

    [Header("New Ability UI")]
    public NewAbilityUI newAbilityUI;

    [Header("New Minion Ability UI")]
    public NewMinionAbilityUI newMinionAbilityUI;

    [Header("Skirmish UI")]
    public SkirmishUI skirmishUI;
    public CharacterPortrait skirmishEnemyPortrait;
    public TextMeshProUGUI skirmishEnemyText;
    public GameObject skirmishConfirmationGO;

    [Header("Saving/Loading")]
    public Button saveGameButton;

    [Header("Kill Count UI")]
    [SerializeField] private GameObject killCountGO;
    [SerializeField] private TextMeshProUGUI killCountLbl;
    [SerializeField] private GameObject killSummaryGO;
    [SerializeField] private GameObject killCharacterItemPrefab;
    [SerializeField] private ScrollRect killCountScrollView;
    [SerializeField] private RectTransform aliveHeader;
    [SerializeField] private RectTransform deadHeader;

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
        SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.summonSlots[0]);
        SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots[0]);
        //UpdateArtifactsInteraction();

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
        Messenger.AddListener<Minion, PlayerJobAction>(Signals.MINION_LEARNED_INTERVENE_ABILITY, OnMinionLearnedInterventionAbility);

        //summons
        Messenger.AddListener<Summon>(Signals.PLAYER_GAINED_SUMMON, OnGainNewSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_REMOVED_SUMMON, OnRemoveSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_PLACED_SUMMON, OnSummonUsed);
        Messenger.AddListener<SummonSlot>(Signals.PLAYER_GAINED_SUMMON_LEVEL, UpdateCurrentlySelectedSummonSlotLevel);

        //Artifacts
        Messenger.AddListener<Artifact>(Signals.PLAYER_GAINED_ARTIFACT, OnGainNewArtifact);
        Messenger.AddListener<Artifact>(Signals.PLAYER_REMOVED_ARTIFACT, OnRemoveArtifact);
        Messenger.AddListener<Artifact>(Signals.PLAYER_USED_ARTIFACT, OnUsedArtifact);
        Messenger.AddListener<ArtifactSlot>(Signals.PLAYER_GAINED_ARTIFACT_LEVEL, UpdateCurrentlySelectedArtifactSlotLevel);


        //Kill Count UI
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);

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
        saveGameButton.gameObject.SetActive(false);

        //Kill count UI
        UpdateKillCountActiveState();
        LoadKillCountCharacterItems(area);
        UpdateKillCount();
    }
    private void OnAreaMapClosed(Area area) {
        UpdateSummonsInteraction();
        UpdateArtifactsInteraction();
        startInvasionButton.gameObject.SetActive(false);
        saveGameButton.gameObject.SetActive(true);

        UpdateKillCountActiveState();
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
    private void OnCharacterDied(Character character) {
        if (InteriorMapManager.Instance.isAnAreaMapShowing) {
            UpdateKillCount();
            OrderKillSummaryItems();
        }
    }
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            UpdateKillCount();
            OrderKillSummaryItems();
        }
    }
    private void OnCharacterLostTrait(Character character, Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            UpdateKillCount();
            OrderKillSummaryItems();
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        UpdateKillCount();
        OrderKillSummaryItems();
    }
    #endregion

    #region Role Slots
    int currentlyShowingSlotIndex = 0;
    private void LoadRoleSlots() {
        roleSlots = new RoleSlotItem[PlayerManager.Instance.player.minions.Length];
        for (int i = 0; i < PlayerManager.Instance.player.minions.Length; i++) {
            GameObject roleSlotGO = UIManager.Instance.InstantiateUIObject(roleSlotItemPrefab.name, roleSlotsParent);
            RoleSlotItem roleSlot = roleSlotGO.GetComponent<RoleSlotItem>();
            //roleSlot.SetSlotJob(keyValuePair.Key);
            roleSlot.Initialize();
            roleSlots[i] = roleSlot;
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
        int minionCount = PlayerManager.Instance.player.GetCurrentMinionCount();
        if (currentlyShowingSlotIndex >= minionCount){
            currentlyShowingSlotIndex = minionCount - 1;
        }
        UpdateRoleSlotScroll();
    }
    public void ScrollNext() {
        currentlyShowingSlotIndex += 1;
        if (currentlyShowingSlotIndex == PlayerManager.Instance.player.GetCurrentMinionCount()) {
            currentlyShowingSlotIndex = 0;
        }
        UpdateRoleSlotScroll();
    }
    public void ScrollPrevious() {
        currentlyShowingSlotIndex -= 1;
        if (currentlyShowingSlotIndex < 0) {
            currentlyShowingSlotIndex = PlayerManager.Instance.player.GetCurrentMinionCount() - 1;
        }
        UpdateRoleSlotScroll();
    }
    public void ScrollRoleSlotTo(int index) {
        if (currentlyShowingSlotIndex == index) {
            return;
        }
        currentlyShowingSlotIndex = index;
        int minionCount = PlayerManager.Instance.player.GetCurrentMinionCount();
        if (currentlyShowingSlotIndex >= minionCount) {
            currentlyShowingSlotIndex = minionCount - 1;
        }
        UpdateRoleSlotScroll();
        PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
        CursorManager.Instance.ClearLeftClickActions(); //TODO: Change this to no clear all actions but just the ones concerened with the player abilities
    }
    private void UpdateRoleSlotScroll() {
        RoleSlotItem slotToShow = roleSlots[currentlyShowingSlotIndex];
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
        RoleSlotItem currActive = roleSlots[currentlyShowingSlotIndex];
        if (currActive.minion == minion) {
            LoadActionButtonsForActiveJob(currActive);
        }
    }
    #endregion

    #region Attack UI/Invasion
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
        } else if (menu is AreaInfoUI || menu is CharacterInfoUI || menu is TileObjectInfoUI) {
            HideKillSummary();
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
            if (intel != null) {
                currItem.SetClickAction(PlayerManager.Instance.player.SetCurrentActiveIntel);
            }
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
        //RectTransform rt = UIManager.Instance.playerNotifGO.transform as RectTransform;
        //Vector3 previousPos = rt.anchoredPosition;
        //if (!state) {
        //    rt.anchoredPosition = new Vector3(-640f, previousPos.y, previousPos.z);
        //} else {
        //    rt.anchoredPosition = new Vector3(-1150f, previousPos.y, previousPos.z);
        //}
    }
    public IntelItem GetIntelItemWithIntel(Intel intel) {
        for (int i = 0; i < intelItems.Length; i++) {
            if(intelItems[i].intel != null && intelItems[i].intel == intel) {
                return intelItems[i];
            }
        }
        return null;
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

    #region Start Picker
    private INTERVENTION_ABILITY[] startingAbilities;
    public void ShowStartingMinionPicker() {
        startingMinionCard1.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
        startingMinionCard2.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
        startingMinionCard3.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
        startingAbilities = new INTERVENTION_ABILITY[PlayerManager.Instance.player.MAX_INTERVENTION_ABILITIES];
        RandomizeStartingAbilities();
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
        HideStartingMinionPicker();
        PlayerManager.Instance.player.AddMinion(startingMinionCard1.minion);
        PlayerManager.Instance.player.AddMinion(startingMinionCard2.minion);
        PlayerManager.Instance.player.AddMinion(startingMinionCard3.minion);
        PlayerManager.Instance.player.SetMinionLeader(startingMinionCard1.minion);
        for (int i = 0; i < startingAbilities.Length; i++) {
            PlayerManager.Instance.player.GainNewInterventionAbility(startingAbilities[i]);
        }
        startingAbilities = null;
    }
    private void ShowSelectMinionLeader() {
        Utilities.DestroyChildren(minionLeaderPickerParent.transform);
        minionLeaderPickers.Clear();
        selectMinionLeaderText.gameObject.SetActive(true);
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
        tempCurrentMinionLeaderPicker.imgHighlight.gameObject.SetActive(false);
        leaderPicker.imgHighlight.gameObject.SetActive(true);
        tempCurrentMinionLeaderPicker = leaderPicker;
    }
    private void RandomizeStartingAbilities() {
        INTERVENTION_ABILITY[] abilities = PlayerManager.Instance.allInterventionAbilities;
        for (int i = 0; i < startingAbilityIcons.Length; i++) {
            INTERVENTION_ABILITY randomAbility = abilities[UnityEngine.Random.Range(0, abilities.Length)];
            string abilityName = Utilities.NormalizeStringUpperCaseFirstLetters(randomAbility.ToString());
            startingAbilityIcons[i].sprite = PlayerManager.Instance.GetJobActionSprite(abilityName);
            startingAbilityLbls[i].text = abilityName;
            startingAbilities[i] = randomAbility;
        }
    }
    public void RerollAbilities() {
        RandomizeStartingAbilities();
    }
    #endregion

    #region Corruption and Threat
    public void ShowCorruptTileConfirmation(HexTile tile) {
        if (tile.CanBeCorrupted() && !tile.isCorrupted) {//&& tile.elevationType != ELEVATION.WATER && !PlayerManager.Instance.player.isTileCurrentlyBeingCorrupted 
            PlayerManager.Instance.player.SetCurrentTileBeingCorrupted(tile);
            //if(tile.landmarkOnTile.yieldType == LANDMARK_YIELD_TYPE.SKIRMISH) {
            //    //tile.landmarkOnTile.GenerateSkirmishEnemy();
            //    skirmishEnemyPortrait.GeneratePortrait(tile.landmarkOnTile.skirmishEnemy);
            //    string text = tile.landmarkOnTile.skirmishEnemy.name;
            //    text += "\nLvl." + tile.landmarkOnTile.skirmishEnemy.level + " " + tile.landmarkOnTile.skirmishEnemy.raceClassName;
            //    skirmishEnemyText.text = text;
            //    skirmishConfirmationGO.SetActive(true);
            //    ShowSelectMinionLeader();
            //} else {
                tempCurrentMinionLeaderPicker = null;
                OnClickYesCorruption();
            //}
        }
    }
    public void HideCorruptTileConfirmation() {
        skirmishConfirmationGO.SetActive(false);
    }
    public void OnClickYesCorruption() {
        HideCorruptTileConfirmation();
        if(tempCurrentMinionLeaderPicker != null) {
            PlayerManager.Instance.player.SetMinionLeader(tempCurrentMinionLeaderPicker.minion);
        } else {
            //If story event, randomize minion leader, if not, keep current minion leader
            //if(PlayerManager.Instance.player.currentTileBeingCorrupted.landmarkOnTile.yieldType == LANDMARK_YIELD_TYPE.STORY_EVENT) {
            //    Minion minion = PlayerManager.Instance.player.GetRandomMinion();
            //    PlayerManager.Instance.player.SetMinionLeader(minion);
            //}
        }
        if (PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile != null) {
            GameManager.Instance.SetOnlyTickDays(false);
            InteriorMapManager.Instance.TryShowAreaMap(PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile);
        } else {
            //PlayerManager.Instance.player.currentTileBeingCorrupted.landmarkOnTile.ShowEventBasedOnYieldType();
            PlayerManager.Instance.player.InvadeATile();
        }
        //if (tempCurrentMinionLeaderPicker != null) {
        //    PlayerManager.Instance.player.SetMinionLeader(tempCurrentMinionLeaderPicker.minion);
        //    if (PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile == null) {
        //        StoryEvent e = PlayerManager.Instance.player.currentTileBeingCorrupted.GetRandomStoryEvent();
        //        if (e != null) {
        //            Debug.Log("Will show event " + e.name);
        //            storyEventUI.ShowEvent(e, true);
        //            //if (e.trigger == STORY_EVENT_TRIGGER.IMMEDIATE) {
        //            //    //show story event UI
        //            //    storyEventUI.ShowEvent(e, true);
        //            //} else if (e.trigger == STORY_EVENT_TRIGGER.MID) { //schedule show event UI based on trigger.
        //            //    int difference = Mathf.Abs(GameManager.Instance.Today().day - (GameManager.Instance.Today().day + PlayerManager.Instance.player.currentTileBeingCorrupted.corruptDuration));
        //            //    int day = UnityEngine.Random.Range(1, difference);
        //            //    GameDate dueDate = GameManager.Instance.Today().AddDays(day);
        //            //    SchedulingManager.Instance.AddEntry(dueDate, () => storyEventUI.ShowEvent(e, true), null);
        //            //} else if (e.trigger == STORY_EVENT_TRIGGER.END) {
        //            //    GameDate dueDate = GameManager.Instance.Today().AddDays(PlayerManager.Instance.player.currentTileBeingCorrupted.corruptDuration);
        //            //    SchedulingManager.Instance.AddEntry(dueDate, () => storyEventUI.ShowEvent(e, true), null);
        //            //}
        //        }
        //    }
        //}
    }
    public void OnClickNoCorruption() {
        HideCorruptTileConfirmation();
    }
    //public void UpdateThreatMeter() {
    //    threatMeter.value = PlayerManager.Instance.player.threat;
    //}
    #endregion

    #region Summons
    private SummonSlot currentlySelectedSummonSlot; //the summon type that is currently shown in the UI
    private void UpdateSummonsInteraction() {
        bool state = currentlySelectedSummonSlot.summon != null && !currentlySelectedSummonSlot.summon.hasBeenUsed;
        //summonCover.SetActive(!state);
        summonBtn.interactable = state && InteriorMapManager.Instance.isAnAreaMapShowing;
    }
    public void OnGainNewSummon(Summon newSummon) {
        UpdateSummonsInteraction();
        if (currentlySelectedSummonSlot.summon == null) {
            SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.GetSummonSlotBySummon(newSummon));
        }
        //ShowNewObjectInfo(newSummon);
        //AssignNewActionToLatestItem(newSummon);
    }
    public void OnRemoveSummon(Summon summon) {
        UpdateSummonsInteraction();
        if (PlayerManager.Instance.player.GetTotalSummonsCount() == 0) { //the player has no more summons left
            SetCurrentlySelectedSummonSlot(null);
        } else if (currentlySelectedSummonSlot.summon == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleSummons(1);
        }
    }
    private void OnSummonUsed(Summon summon) {
        UpdateSummonsInteraction();
        if (PlayerManager.Instance.player.GetTotalSummonsCount() == 0) { //the player has no more summons left
            SetCurrentlySelectedSummonSlot(null);
        } else if (currentlySelectedSummonSlot.summon == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleSummons(1);
        }
    }
    public void SetCurrentlySelectedSummonSlot(SummonSlot summonSlot) {
        if (summonSlot == null) {
            //If null, just set the first slot so that currentlySelectedArtifactSlot will not be null
            summonSlot = PlayerManager.Instance.player.summonSlots[0];
        }
        currentlySelectedSummonSlot = summonSlot;
        if (currentlySelectedSummonSlot.summon == null) {
            currentSummonImg.sprite = CharacterManager.Instance.GetSummonSettings(SUMMON_TYPE.None).summonPortrait;
        } else {
            currentSummonImg.sprite = CharacterManager.Instance.GetSummonSettings(currentlySelectedSummonSlot.summon.summonType).summonPortrait;
        }
        currentSummonCountLbl.text = currentlySelectedSummonSlot.level.ToString();
        UpdateSummonsInteraction();
    }
    public void UpdateCurrentlySelectedSummonSlotLevel(SummonSlot summonSlot) {
        if(currentlySelectedSummonSlot == summonSlot) {
            currentSummonCountLbl.text = currentlySelectedSummonSlot.level.ToString();
        }
    }
    public void CycleSummons(int cycleDirection) {
        int currentSelectedSummonSlotIndex = PlayerManager.Instance.player.GetIndexForSummonSlot(currentlySelectedSummonSlot);
        int index = currentSelectedSummonSlotIndex;
        int currentSummonCount = PlayerManager.Instance.player.GetTotalSummonsCount();
        while (true) {
            int next = index + cycleDirection;
            if (next >= currentSummonCount) {
                next = 0;
            } else if (next <= 0) {
                next = currentSummonCount - 1;
            }
            if(next < 0) {
                next = 0;
            }
            index = next;
            if (PlayerManager.Instance.player.summonSlots[index].summon != null) {
                SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.summonSlots[index]);
                break;
            } else if (index == currentSelectedSummonSlotIndex) {//This means that summon slots was already cycled through all of it and it can't find an summon, end the loop when it happens
                SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.summonSlots[currentSelectedSummonSlotIndex]);
                break;
            }
        }
    }
    public void ShowSummonTooltip() {
        if(currentlySelectedSummonSlot.summon != null) {
            string header = currentlySelectedSummonSlot.summon.summonType.SummonName();
            string message;
            switch (currentlySelectedSummonSlot.summon.summonType) {
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
                    message = "Summon a " + Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedSummonSlot.ToString());
                    break;
            }
            UIManager.Instance.ShowSmallInfo(message, summonTooltipPos, header);
        }
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
            Summon summonToPlace = currentlySelectedSummonSlot.summon;
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

    #region Artifacts
    private ArtifactSlot currentlySelectedArtifactSlot; //the artifact that is currently shown in the UI
    private void UpdateArtifactsInteraction() {
        bool state = currentlySelectedArtifactSlot.artifact != null && !currentlySelectedArtifactSlot.artifact.hasBeenUsed;
        //summonArtifactCover.SetActive(!state);
        summonArtifactBtn.interactable = state && InteriorMapManager.Instance.isAnAreaMapShowing;
    }
    private void OnGainNewArtifact(Artifact newArtifact) {
        UpdateArtifactsInteraction();
        if (currentlySelectedArtifactSlot.artifact == null) {
            SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.GetArtifactSlotByArtifact(newArtifact));
        }
        //ShowNewObjectInfo(newArtifact);
        //AssignNewActionToLatestItem(newSummon);
    }
    private void OnRemoveArtifact(Artifact artifact) {
        UpdateArtifactsInteraction();
        if (PlayerManager.Instance.player.GetTotalArtifactCount() == 0) { //the player has no more artifacts left
            SetCurrentlySelectedArtifactSlot(null);
        } else if (currentlySelectedArtifactSlot.artifact == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleArtifacts(1);
        }
    }
    private void OnUsedArtifact(Artifact artifact) {
        UpdateArtifactsInteraction();
        if (PlayerManager.Instance.player.GetTotalArtifactCount() == 0) { //the player has no more artifacts left
            SetCurrentlySelectedArtifactSlot(null);
        } else if (currentlySelectedArtifactSlot.artifact == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
            CycleArtifacts(1);
        }
        //else if (artifact == currentlySelectedArtifactSlot
        //    && PlayerManager.Instance.player.GetAvailableArtifactsOfTypeCount(artifact.type) == 0) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
        //    CycleArtifacts(1);
        //}
    }
    public void SetCurrentlySelectedArtifactSlot(ArtifactSlot artifactSlot) {
        if (artifactSlot == null) {
            //If null, just set the first slot so that currentlySelectedArtifactSlot will not be null
            artifactSlot = PlayerManager.Instance.player.artifactSlots[0];
        }
        currentlySelectedArtifactSlot = artifactSlot;
        if (currentlySelectedArtifactSlot.artifact == null) {
            currentArtifactImg.sprite = CharacterManager.Instance.GetArtifactSettings(ARTIFACT_TYPE.None).artifactPortrait;
        } else {
            currentArtifactImg.sprite = CharacterManager.Instance.GetArtifactSettings(currentlySelectedArtifactSlot.artifact.type).artifactPortrait;
            //currentArtifactCountLbl.text = PlayerManager.Instance.player.GetAvailableArtifactsOfTypeCount(currentlySelectedArtifactSlot.artifact.type).ToString();
        }
        currentArtifactCountLbl.text = currentlySelectedArtifactSlot.level.ToString();
        UpdateArtifactsInteraction();
    }
    public void UpdateCurrentlySelectedArtifactSlotLevel(ArtifactSlot artifactSlot) {
        if (currentlySelectedArtifactSlot == artifactSlot) {
            currentArtifactCountLbl.text = currentlySelectedArtifactSlot.level.ToString();
        }
    }
    public void CycleArtifacts(int cycleDirection) {
        int currentSelectedArtifactSlotIndex = PlayerManager.Instance.player.GetIndexForArtifactSlot(currentlySelectedArtifactSlot);
        int index = currentSelectedArtifactSlotIndex;
        int currentArtifactCount = PlayerManager.Instance.player.GetTotalArtifactCount();
        while (true) {
            int next = index + cycleDirection;
            if (next >= currentArtifactCount) {
                next = 0;
            } else if (next <= 0) {
                next = currentArtifactCount - 1;
            }
            if (next < 0) {
                next = 0;
            }
            index = next;
            if (PlayerManager.Instance.player.artifactSlots[index].artifact != null) {
                SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots[index]);
                break;
            } else if (index == currentSelectedArtifactSlotIndex) {//This means that artifact slots was already cycled through all of it and it can't find an artifact, end the loop when it happens
                SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots[currentSelectedArtifactSlotIndex]);
                break;
            }
        }
    }
    public void ShowArtifactTooltip() {
        if (currentlySelectedArtifactSlot.artifact != null) {
            string header = Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedArtifactSlot.artifact.type.ToString());
            string message = PlayerManager.Instance.player.GetArtifactDescription(currentlySelectedArtifactSlot.artifact.type);
            UIManager.Instance.ShowSmallInfo(message, summonArtifactTooltipPos, header);
        }
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
                Artifact artifactToPlace = currentlySelectedArtifactSlot.artifact;
                if (tile.structure.AddPOI(artifactToPlace, tile)) {
                    artifactToPlace.SetIsSummonedByPlayer(true);
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
        //Utilities.DestroyChildren(killSummaryScrollView.content);
        LoadKillSummaryCharacterItems();
    }
    private void LoadKillSummaryCharacterItems() {
        KillCountCharacterItem[] items = Utilities.GetComponentsInDirectChildren<KillCountCharacterItem>(killCountScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            KillCountCharacterItem item = items[i];
            item.transform.SetParent(killSummaryScrollView.content);
        }
    }
    public void BackToWorld() {
        Utilities.DestroyChildren(killSummaryScrollView.content);
        Area closedArea = InteriorMapManager.Instance.HideAreaMap();
        SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.summonSlots.FirstOrDefault());
        SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots.FirstOrDefault());
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

    #region Saving/Loading
    public void SaveGame() {
        SaveManager.Instance.SaveCurrentStateOfWorld();
    }
    #endregion

    #region Kill Count
    public bool isShowingKillSummary { get { return killCountGO.activeSelf; } }
    private void UpdateKillCountActiveState() {
        bool state = InteriorMapManager.Instance.isAnAreaMapShowing;
        killCountGO.SetActive(state);
        killSummaryGO.SetActive(false);
    }
    private void LoadKillCountCharacterItems(Area area) {
        KillCountCharacterItem[] items = Utilities.GetComponentsInDirectChildren<KillCountCharacterItem>(killCountScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(items[i].gameObject);
        }
        for (int i = 0; i < area.areaResidents.Count; i++) {
            Character character = area.areaResidents[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(killCharacterItemPrefab.name, Vector3.zero, Quaternion.identity, killCountScrollView.content);
            KillCountCharacterItem item = go.GetComponent<KillCountCharacterItem>();
            item.SetCharacter(character);
        }
        OrderKillSummaryItems();
    }
    private void UpdateKillCount() {
        killCountLbl.text = InteriorMapManager.Instance.currentlyShowingArea.areaResidents.Where(x => x.IsAble()).Count().ToString() + "/" + InteriorMapManager.Instance.currentlyShowingArea.citizenCount.ToString();
    }
    private void OrderKillSummaryItems() {
        KillCountCharacterItem[] items = Utilities.GetComponentsInDirectChildren<KillCountCharacterItem>(killCountScrollView.content.gameObject);
        List<KillCountCharacterItem> alive = new List<KillCountCharacterItem>();
        List<KillCountCharacterItem> dead = new List<KillCountCharacterItem>();
        for (int i = 0; i < items.Length; i++) {
            KillCountCharacterItem currItem = items[i];
            if (!currItem.character.IsAble() || currItem.character.faction != InteriorMapManager.Instance.currentlyShowingArea.owner) { //added checking for faction in cases that the character was raised from dead
                dead.Add(currItem);
            } else {
                alive.Add(currItem);
            }
        }
        aliveHeader.transform.SetAsFirstSibling();
        for (int i = 0; i < alive.Count; i++) {
            KillCountCharacterItem currItem = alive[i];
            currItem.transform.SetSiblingIndex(i + 1);
        }
        deadHeader.transform.SetSiblingIndex(alive.Count + 1);
        for (int i = 0; i < dead.Count; i++) {
            KillCountCharacterItem currItem = dead[i];
            currItem.transform.SetSiblingIndex(alive.Count + i + 2);
        }
    }
    public void ToggleKillSummary() {
        killSummaryGO.SetActive(!killSummaryGO.activeSelf);
    }
    public void HideKillSummary() {
        killSummaryGO.SetActive(false);
    }
    #endregion

    #region General Confirmation
    public void ShowGeneralConfirmation(string header, string body) {
        generalConfirmationTitleText.text = header.ToUpper();
        generalConfirmationBodyText.text = body;
        generalConfirmationGO.SetActive(true);
    }
    public void OnClickOKGeneralConfirmation() {
        generalConfirmationGO.SetActive(false);
    }
    #endregion
}

