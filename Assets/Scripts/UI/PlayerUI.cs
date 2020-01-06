using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq;
using System;
using Inner_Maps;
using Traits;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    [Header("Currencies")]
    [SerializeField] private TextMeshProUGUI manaLbl;

    [Header("Role Slots")]
    [SerializeField] private RectTransform roleSlotsParent;
    //[SerializeField] private RoleSlotItem[] roleSlots;
    [SerializeField] private GameObject roleSlotItemPrefab;
    [SerializeField] private GameObject actionBtnTooltipGO;
    [SerializeField] private TextMeshProUGUI actionBtnTooltipLbl;
    [SerializeField] private TextMeshProUGUI activeMinionTypeLbl;
    [SerializeField] private UI_InfiniteScroll roleSlotsInfiniteScroll;
    [SerializeField] private ScrollRect roleSlotsScrollRect;

    [Header("Attack")]
    public GameObject attackGridGO;
    [SerializeField] private Sprite attackGridIconSprite;
    [SerializeField] private Sprite defenseGridIconSprite;
    public AttackSlotItem attackSlot;
    [SerializeField] private DefenseSlotItem defenseSlot;
    public SlotItem[] attackGridSlots;

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
    [SerializeField] private Button generalConfirmationButton;
    [SerializeField] private TextMeshProUGUI generalConfirmationButtonText;
    public string previousMenu;

    [Header("Start Picker")]
    [SerializeField] private GameObject startingMinionPickerGO;
    [SerializeField] private MinionCard startingMinionCard1;
    [SerializeField] private MinionCard startingMinionCard2;
    //[SerializeField] private MinionCard startingMinionCard3;
    [SerializeField] private GameObject minionLeaderPickerParent;
    [SerializeField] private GameObject minionLeaderPickerPrefab;
    [SerializeField] private TextMeshProUGUI selectMinionLeaderText;
    private List<MinionLeaderPicker> minionLeaderPickers;
    private MinionLeaderPicker tempCurrentMinionLeaderPicker;
    [SerializeField] private Image[] startingAbilityIcons;
    [SerializeField] private TextMeshProUGUI[] startingAbilityLbls; //NOTE: This must always have the same length as startingAbilityIcons

    [Header("Intervention Abilities")]
    [SerializeField] private RectTransform activeMinionActionsParent;
    [SerializeField] private GameObject actionBtnPrefab;
    public UIHoverPosition roleSlotTooltipPos;

    [Header("Combat Abilities")]
    public GameObject combatAbilityGO;
    public GameObject combatAbilityButtonPrefab;
    public List<CombatAbilityButton> currentCombatAbilityButtons { get; private set; }

    [Header("Replace UI")]
    public ReplaceUI replaceUI;

    [Header("Level Up UI")]
    public LevelUpUI levelUpUI;

    [Header("New Ability UI")]
    public NewAbilityUI newAbilityUI;

    [Header("New Minion Ability UI")]
    public NewMinionAbilityUI newMinionAbilityUI;

    [Header("Research Intervention Ability UI")]
    public ResearchAbilityUI researchInterventionAbilityUI;

    [Header("Unleash Summon UI")]
    public UnleashSummonUI unleashSummonUI;

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
    public RectTransform deadHeader;
    private List<CharacterNameplateItem> killCountCharacterItems;
    private int unusedKillCountCharacterItems;
    private int aliveCount;
    private int allFilteredCharactersCount;

    [Header("Top Buttons")]
    [SerializeField] private Toggle portalToggle;
    [SerializeField] private Toggle spireToggle;
    [SerializeField] private Toggle anvilToggle;
    [SerializeField] private Toggle eyeToggle;
    [SerializeField] private Toggle needleToggle;
    [SerializeField] private Toggle fingerToggle;
    [SerializeField] private Toggle cryptToggle;
    [SerializeField] private Toggle kennelToggle;
    [SerializeField] private Toggle profaneToggle;
    [SerializeField] private ToggleGroup demonicToggleGroup;

    [Header("Seize Object")]
    [SerializeField] private Button unseizeButton;

    [Header("Custom Dropdown")]
    [SerializeField] private CustomDropdownList customDropdownList;
    private readonly List<string> spellsList = new List<string>() { "Tornado" };
    private readonly List<string> factionActionsList = new List<string>() { "Manage Cult", "Meddle" };

    public List<System.Action> pendingUIToShow { get; private set; }

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

    private PlayerJobActionButton[] interventionAbilityBtns;

    void Awake() {
        Instance = this;
        //minionItems = new List<PlayerCharacterItem>();
        //Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        //Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }
    public void UpdateUI() {
        if (PlayerManager.Instance.player == null) {
            return;
        }
        //if (InteriorMapManager.Instance.isAnAreaMapShowing) {
        //    UpdateStartInvasionButton();
        //}
        UpdateMana();
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
        pendingUIToShow = new List<Action>();

        //LoadRoleSlots();
        LoadAttackSlot();
        LoadInterventionAbilitySlots();
        UpdateInterventionAbilitySlots();
        //LoadKillCountCharacterItems();
        InitialUpdateKillCountCharacterItems();

        UpdateIntel();
        InitializeMemoriesMenu();
        SetCurrentlySelectedSummonSlot(null);
        SetCurrentlySelectedArtifactSlot(null);
        UpdateDemonicLandmarkToggleInteractables();
        //UpdateArtifactsInteraction();

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
        Messenger.AddListener<Intel>(Signals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
        Messenger.AddListener<Intel>(Signals.PLAYER_REMOVED_INTEL, OnIntelRemoved);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);

        //job action buttons
        Messenger.AddListener<PlayerJobAction>(Signals.PLAYER_LEARNED_INTERVENE_ABILITY, OnPlayerLearnedInterventionAbility);
        Messenger.AddListener<PlayerJobAction>(Signals.PLAYER_CONSUMED_INTERVENE_ABILITY, OnPlayerLearnedInterventionAbility);

        //summons
        Messenger.AddListener<Summon>(Signals.PLAYER_GAINED_SUMMON, OnGainNewSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_REMOVED_SUMMON, OnRemoveSummon);
        Messenger.AddListener<Summon>(Signals.PLAYER_PLACED_SUMMON, OnSummonUsed);
        Messenger.AddListener<SummonSlot>(Signals.PLAYER_GAINED_SUMMON_LEVEL, UpdateCurrentlySelectedSummonSlotLevel);
        Messenger.AddListener<SummonSlot>(Signals.PLAYER_LOST_SUMMON_SLOT, OnPlayerLostSummonSlot);
        Messenger.AddListener<SummonSlot>(Signals.PLAYER_GAINED_SUMMON_SLOT, OnPlayerGainedSummonSlot);

        //Artifacts
        Messenger.AddListener<Artifact>(Signals.PLAYER_GAINED_ARTIFACT, OnGainNewArtifact);
        Messenger.AddListener<Artifact>(Signals.PLAYER_REMOVED_ARTIFACT, OnRemoveArtifact);
        Messenger.AddListener<Artifact>(Signals.PLAYER_USED_ARTIFACT, OnUsedArtifact);
        Messenger.AddListener<ArtifactSlot>(Signals.PLAYER_GAINED_ARTIFACT_LEVEL, UpdateCurrentlySelectedArtifactSlotLevel);
        Messenger.AddListener<ArtifactSlot>(Signals.PLAYER_LOST_ARTIFACT_SLOT, OnPlayerLostArtifactSlot);
        Messenger.AddListener<ArtifactSlot>(Signals.PLAYER_GAINED_ARTIFACT_SLOT, OnPlayerGainedArtifactSlot);

        //Minion List
        Messenger.AddListener<Minion>(Signals.PLAYER_GAINED_MINION, OnGainedMinion);
        Messenger.AddListener<Minion>(Signals.PLAYER_LOST_MINION, OnLostMinion);

        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);

        //key presses
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);

        //currencies
        Messenger.AddListener(Signals.PLAYER_ADJUSTED_MANA, UpdateMana);

        //Landmarks
        Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_CREATED, OnLandmarkCreated);
        //Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_DESTROYED, OnLandmarkDestroyed);
        Messenger.AddListener<Region>(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, OnUpdateRegionUISignal);
    }

    public void InitializeAfterGameLoaded() {
        //Kill Count UI
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character>(Signals.CHARACTER_CREATED, AddedNewCharacter);
        Messenger.AddListener<Character>(Signals.CHARACTER_BECOMES_MINION_OR_SUMMON, CharacterBecomesMinionOrSummon);
        Messenger.AddListener<Character>(Signals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, CharacterBecomesNonMinionOrSummon);
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
    }

    #region Listeners
    private void OnAreaMapOpened(Area area) {
        UpdateSummonsInteraction();
        UpdateArtifactsInteraction();
        //UpdateStartInvasionButton();
        //startInvasionButton.gameObject.SetActive(true);
        //saveGameButton.gameObject.SetActive(false);

        //if (PlayerManager.Instance.player.currentAreaBeingInvaded == area) {
        //    ShowCombatAbilityUI();
        //}

        //Kill count UI
        //UpdateKillCountActiveState();
        //LoadKillCountCharacterItems(area);
        //UpdateKillCount();
    }
    private void OnAreaMapClosed(Area area) {
        UpdateSummonsInteraction();
        UpdateArtifactsInteraction();
        //startInvasionButton.gameObject.SetActive(false);
        //HideCombatAbilityUI();
        //saveGameButton.gameObject.SetActive(true);

        //UpdateKillCountActiveState();
    }
    private void OnKeyPressed(KeyCode pressedKey) {
        if (pressedKey == KeyCode.Escape) {
            if (PlayerManager.Instance.player.currentActivePlayerJobAction != null) {
                PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
                CursorManager.Instance.ClearLeftClickActions();
            } else if (isSummoning) {
                CancelSummon();
            } else if (isSummoningArtifact) {
                CancelSummonArtifact();
            } else {
                //only toggle options menu if doing nothing else
                UIManager.Instance.ToggleOptionsMenu();
            }
        } else if (pressedKey == KeyCode.Mouse0) {
            //left click
            //if (isSummoning) {
            //    TryPlaceSummon();
            //} else 
            if (isSummoningArtifact) {
                TryPlaceArtifact();
            }
        } else if (pressedKey == KeyCode.Mouse1) {
            //left click
            //if (isSummoning) {
            //    TryPlaceSummon();
            //} else 
            if (isSummoningArtifact) {
                CancelSummonArtifact();
            }
        }
    }
    private void OnCharacterDied(Character character) {
        //UpdateKillCount();
        //OrderKillSummaryItems();
        TransferCharacterFromActiveToInactive(character);
        //CheckIfAllCharactersWipedOut();
        UpdateKillCount();
    }
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            //UpdateKillCount();
            //OrderKillSummaryItems();
            TransferCharacterFromActiveToInactive(character);
            //CheckIfAllCharactersWipedOut();
            UpdateKillCount();
        }
    }
    private void OnCharacterLostTrait(Character character, Trait trait) {
        if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
            //UpdateKillCount();
            //OrderKillSummaryItems();
            TransferCharacterFromInactiveToActive(character);
            UpdateKillCount();
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        //UpdateKillCount();
        //OrderKillSummaryItems();

        //TODO: This causes inconsistencies since the character will have null faction once he/she is removed from the faction
        //TransferCharacterFromActiveToInactive(character);
        //CheckIfAllCharactersWipedOut();
    }
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        if (faction == FactionManager.Instance.neutralFaction) {
            TransferCharacterFromActiveToInactive(character);
        } else if (faction.isPlayerFaction /*|| faction == FactionManager.Instance.friendlyNeutralFaction*/) {
            OnCharacterBecomesMinionOrSummon(character);
        } else {
            TransferCharacterFromInactiveToActive(character);
        }
        UpdateKillCount();
        //CheckIfAllCharactersWipedOut();
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if (item != null) {
            item.SetObject(character);
        }
    }
    private void AddedNewCharacter(Character character) {
        OnAddNewCharacter(character);
    }
    private void CharacterBecomesMinionOrSummon(Character character) {
        //OnCharacterBecomesMinionOrSummon(character);
    }
    private void CharacterBecomesNonMinionOrSummon(Character character) {
        OnCharacterBecomesNonMinionOrSummon(character);
    }
    private void CheckIfAllCharactersWipedOut() {
        if (PlayerManager.Instance.player.currentAreaBeingInvaded != null) {
            return; //player has initiated invasion of settlement, all checking of win condition will be executed in PerTickInvasion() in Player script. TODO: Unify them!
        }
        bool stillHasResidents = false;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) { //Changed checking to faction members, because some characters may still consider the area as their home, but are no longer part of the faction
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter.IsAble() && currCharacter.isStillConsideredAlive && !(currCharacter is Summon) && currCharacter.faction.isMajorFriendlyNeutral) {
                stillHasResidents = true;
                break;
            }
        }
        if (!stillHasResidents) {
            //player has won
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
            Messenger.Broadcast(Signals.HIDE_MENUS);
            SuccessfulAreaCorruption();
        }
    }
    private void OnLandmarkCreated(BaseLandmark landmark) {
        UpdateDemonicLandmarkToggleInteractables();
    }
    private void OnLandmarkDestroyed(BaseLandmark landmark) {
        if (landmark.specificLandmarkType.IsPlayerLandmark()) {
            UpdateDemonicLandmarkToggleInteractables();
        }
    }
    private void OnUpdateRegionUISignal(Region region) {
        if (UIManager.Instance.regionInfoUI.isShowing) {
            UpdateClickedDemonicToggle();
        }
    }
    private void OnMenuOpened(UIMenu menu) {
        if (menu is LandmarkInfoUI) {
            UIManager.Instance.ShowMinionsMenu();
        } else if (menu is AreaInfoUI || menu is CharacterInfoUI || menu is TileObjectInfoUI || menu is RegionInfoUI) {
            HideKillSummary();
            if (menu is RegionInfoUI) {
                UpdateClickedDemonicToggle();
            }
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
        } else if (menu is RegionInfoUI) {
            UpdateClickedDemonicToggle();
        }
        //else if (menu is CharacterInfoUI || menu is TileObjectInfoUI) {
        //    HideActionButtons();
        //}
    }
    #endregion

    #region Currencies
    private void UpdateMana() {
        manaLbl.text = PlayerManager.Instance.player.mana.ToString();
    }
    #endregion

    #region Role Slots
    //int currentlyShowingSlotIndex = 0;
    //private void LoadRoleSlots() {
    //    roleSlots = new RoleSlotItem[PlayerManager.Instance.player.minions.Count];
    //    for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
    //        GameObject roleSlotGO = UIManager.Instance.InstantiateUIObject(roleSlotItemPrefab.name, roleSlotsParent);
    //        RoleSlotItem roleSlot = roleSlotGO.GetComponent<RoleSlotItem>();
    //        //roleSlot.SetSlotJob(keyValuePair.Key);
    //        roleSlot.Initialize();
    //        roleSlots[i] = roleSlot;
    //    }
    //    //foreach (KeyValuePair<JOB, PlayerJobData> keyValuePair in PlayerManager.Instance.player.roleSlots) {
    //    //    GameObject roleSlotGO = UIManager.Instance.InstantiateUIObject(roleSlotItemPrefab.name, roleSlotsParent);
    //    //    RoleSlotItem roleSlot = roleSlotGO.GetComponent<RoleSlotItem>();
    //    //    roleSlot.SetSlotJob(keyValuePair.Key);
    //    //    roleSlots[currIndex] = roleSlot;
    //    //    currIndex++;
    //    //}
    //    roleSlotsInfiniteScroll.Init();
    //    //LoadActionButtonsForActiveJob(roleSlots[currentlyeShowingSlotIndex]);
    //    UpdateRoleSlotScroll();
    //}
    //public void UpdateRoleSlots() {
    //    for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
    //        roleSlots[i].SetMinion(PlayerManager.Instance.player.minions[i]);
    //    }
    //    int minionCount = PlayerManager.Instance.player.GetCurrentMinionCount();
    //    if (currentlyShowingSlotIndex >= minionCount){
    //        currentlyShowingSlotIndex = minionCount - 1;
    //    }
    //    UpdateRoleSlotScroll();
    //}
    //public void ScrollNext() {
    //    currentlyShowingSlotIndex += 1;
    //    if (currentlyShowingSlotIndex == PlayerManager.Instance.player.GetCurrentMinionCount()) {
    //        currentlyShowingSlotIndex = 0;
    //    }
    //    UpdateRoleSlotScroll();
    //}
    //public void ScrollPrevious() {
    //    currentlyShowingSlotIndex -= 1;
    //    if (currentlyShowingSlotIndex < 0) {
    //        currentlyShowingSlotIndex = PlayerManager.Instance.player.GetCurrentMinionCount() - 1;
    //    }
    //    UpdateRoleSlotScroll();
    //}
    //public void ScrollRoleSlotTo(int index) {
    //    if (currentlyShowingSlotIndex == index) {
    //        return;
    //    }
    //    currentlyShowingSlotIndex = index;
    //    int minionCount = PlayerManager.Instance.player.GetCurrentMinionCount();
    //    if (currentlyShowingSlotIndex >= minionCount) {
    //        currentlyShowingSlotIndex = minionCount - 1;
    //    }
    //    UpdateRoleSlotScroll();
    //    PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
    //    CursorManager.Instance.ClearLeftClickActions(); //TODO: Change this to no clear all actions but just the ones concerened with the player abilities
    //}
    //private void UpdateRoleSlotScroll() {
    //    RoleSlotItem slotToShow = roleSlots[currentlyShowingSlotIndex];
    //    activeMinionTypeLbl.text = Utilities.NormalizeString(slotToShow.slotJob.ToString());
    //    Utilities.ScrolRectSnapTo(roleSlotsScrollRect, slotToShow.GetComponent<RectTransform>());
    //    LoadActionButtonsForActiveJob(slotToShow);
    //}

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
        //Utilities.DestroyChildren(activeMinionActionsParent);
        //Minion minion = active.minion;
        //if(minion != null) {
        //    //for (int i = 0; i < minion.interventionAbilities.Length; i++) {
        //    //    PlayerJobAction jobAction = minion.interventionAbilities[i];
        //    //    if(jobAction != null) {
        //    //        GameObject jobGO = UIManager.Instance.InstantiateUIObject(actionBtnPrefab.name, activeMinionActionsParent);
        //    //        PlayerJobActionButton actionBtn = jobGO.GetComponent<PlayerJobActionButton>();
        //    //        actionBtn.SetJobAction(jobAction, minion.character);
        //    //        actionBtn.SetClickAction(() => PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(jobAction));
        //    //    }
        //    //}
        //}

    }
    public PlayerJobActionButton GetPlayerJobActionButton(PlayerJobAction action) {
        PlayerJobActionButton[] buttons = Utilities.GetComponentsInDirectChildren<PlayerJobActionButton>(activeMinionActionsParent.gameObject);
        for (int i = 0; i < buttons.Length; i++) {
            PlayerJobActionButton currButton = buttons[i];
            if (currButton.actionSlot.ability == action) {
                return currButton;
            }
        }
        return null;
    }
    private void OnPlayerLearnedInterventionAbility(PlayerJobAction action) {
        UpdateInterventionAbilitySlots();
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
        if (combatGridAssignerIcon.sprite == attackGridIconSprite) {
            attackSlot.OnClickConfirm();
        } else {
            defenseSlot.OnClickConfirm();
        }
    }
    public void HideCombatGrid() {
        attackGridGO.SetActive(false);
    }
    private void OnDropOnAttackGrid(object obj, int index) {
        if (obj is Character) {
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
        //for (int i = 0; i < attackGridReference.slots.Length; i++) {
        //    attackGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
        //    attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        //}
    }
    private void SetDefenseGridCharactersFromPlayer() {
        //for (int i = 0; i < defenseGridReference.slots.Length; i++) {
        //    defenseGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
        //    attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        //}
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
    //public void OnClickStartInvasion() {
    //    //ShowCombatAbilityUI();
    //    UIManager.Instance.regionInfoUI.UpdateMainBtnState();
    //}
    //public void StopInvasion() {
    //    //startInvasionButton.interactable = true;
    //    HideCombatAbilityUI();
    //    UpdateMainBtnState();
    //}
    //private void UpdateStartInvasionButton() {
    //    startInvasionButton.interactable = InteriorMapManager.Instance.currentlyShowingArea.CanInvadeSettlement();
    //    startInvasionHoverHandler.gameObject.SetActive(!startInvasionButton.interactable);
    //}
    #endregion

    #region Miscellaneous
    public void AddPendingUI(System.Action pendingUIAction) {
        pendingUIToShow.Add(pendingUIAction);
    }
    public bool TryShowPendingUI() {
        if (pendingUIToShow.Count > 0) {
            System.Action pending = pendingUIToShow[0];
            pendingUIToShow.RemoveAt(0);
            pending.Invoke();
            return true;
        }
        return false;
    }
    public bool IsMajorUIShowing() {
        return levelUpUI.gameObject.activeInHierarchy || newAbilityUI.gameObject.activeInHierarchy || newMinionAbilityUI.gameObject.activeInHierarchy || replaceUI.gameObject.activeInHierarchy || generalConfirmationGO.activeInHierarchy || newMinionUIGO.activeInHierarchy;
    }
    #endregion

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
            if (intelItems[i].intel != null && intelItems[i].intel == intel) {
                return intelItems[i];
            }
        }
        return null;
    }
    #endregion

    #region Provoke
    public void OpenProvoke(Character actor, Character target) {
        provokeMenu.Open(actor, target);
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

        UnsummonedMinionData minion1Data = new UnsummonedMinionData();
        UnsummonedMinionData minion2Data = new UnsummonedMinionData();

        //string minionName1 = string.Empty;
        //string minionClassName1 = string.Empty;
        //COMBAT_ABILITY minionCombatAbilityType1 = COMBAT_ABILITY.FEAR_SPELL;

        //string minionName2 = string.Empty;
        //string minionClassName2 = string.Empty;
        //COMBAT_ABILITY minionCombatAbilityType2 = COMBAT_ABILITY.FEAR_SPELL;

        RandomizeTwoStartingMinions(ref minion1Data, ref minion2Data);

        startingMinionCard1.SetMinion(minion1Data);
        startingMinionCard2.SetMinion(minion2Data);
        //startingMinionCard3.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
        startingAbilities = new INTERVENTION_ABILITY[PlayerManager.Instance.player.MAX_INTERVENTION_ABILITIES];
        RandomizeStartingAbilities();
        startingMinionPickerGO.SetActive(true);
    }
    public void HideStartingMinionPicker() {
        startingMinionPickerGO.SetActive(false);
    }
    public void Reroll() {
        UnsummonedMinionData minion1Data = new UnsummonedMinionData();
        UnsummonedMinionData minion2Data = new UnsummonedMinionData();

        RandomizeTwoStartingMinions(ref minion1Data, ref minion2Data);

        startingMinionCard1.SetMinion(minion1Data);
        startingMinionCard2.SetMinion(minion2Data);
    }
    //public void Reroll2() {
    //    startingMinionCard2.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
    //}
    //public void Reroll3() {
    //    startingMinionCard3.SetMinion(PlayerManager.Instance.player.CreateNewMinionRandomClass(RACE.DEMON));
    //}
    public void OnClickStartGame() {
        HideStartingMinionPicker();

        Minion minion1 = PlayerManager.Instance.player.CreateNewMinion(startingMinionCard1.minionData.className, RACE.DEMON, false);
        Minion minion2 = PlayerManager.Instance.player.CreateNewMinion(startingMinionCard2.minionData.className, RACE.DEMON, false);

        minion1.character.SetName(startingMinionCard1.minionData.minionName);
        minion2.character.SetName(startingMinionCard2.minionData.minionName);

        minion1.SetCombatAbility(startingMinionCard1.minionData.combatAbility);
        minion2.SetCombatAbility(startingMinionCard2.minionData.combatAbility);

        minion1.SetRandomResearchInterventionAbilities(startingMinionCard1.minionData.interventionAbilitiesToResearch);
        minion2.SetRandomResearchInterventionAbilities(startingMinionCard2.minionData.interventionAbilitiesToResearch);

        PlayerManager.Instance.player.AddMinion(minion1);
        PlayerManager.Instance.player.AddMinion(minion2);
        //PlayerManager.Instance.player.AddMinion(startingMinionCard3.minion);
        PlayerManager.Instance.player.SetMinionLeader(minion1);
        for (int i = 0; i < startingAbilities.Length; i++) {
            PlayerManager.Instance.player.GainNewInterventionAbility(startingAbilities[i]);
        }
        startingAbilities = null;
        UIManager.Instance.SetSpeedTogglesState(true);
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.isMajorNonPlayer) {
                faction.CreateAndSetActiveQuest("Divine Intervention", faction.ownedRegions[0]);
                break;
            }
        }
        
        LANDMARK_TYPE[] landmarkTypes = Utilities.GetEnumValues<LANDMARK_TYPE>();
        for (int i = 0; i < landmarkTypes.Length; i++) {
            LANDMARK_TYPE type = landmarkTypes[i];
            if (type.IsPlayerLandmark() && type != LANDMARK_TYPE.THE_PORTAL && type != LANDMARK_TYPE.THE_SPIRE) {
                Region region =
                    Utilities.GetRandomElement(GridMap.Instance.allRegions.Where(x => x.coreTile.isCorrupted == false && x.owner == null).ToList());
                LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, region);
                BaseLandmark landmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(region.coreTile, type, false);
                landmark.OnFinishedBuilding();
            }
        }
        //PlayerManager.Instance.player.StartDivineIntervention();
        //PlayerManager.Instance.player.StartResearchNewInterventionAbility();
    }
    private void ShowSelectMinionLeader() {
        Utilities.DestroyChildren(minionLeaderPickerParent.transform);
        minionLeaderPickers.Clear();
        selectMinionLeaderText.gameObject.SetActive(true);
        tempCurrentMinionLeaderPicker = null;
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            Minion minion = PlayerManager.Instance.player.minions[i];
            if (minion != null) {
                GameObject go = GameObject.Instantiate(minionLeaderPickerPrefab, minionLeaderPickerParent.transform);
                MinionLeaderPicker minionLeaderPicker = go.GetComponent<MinionLeaderPicker>();
                minionLeaderPicker.SetMinion(minion);
                minionLeaderPickers.Add(minionLeaderPicker);
                if (PlayerManager.Instance.player.currentMinionLeader == null) {
                    if (tempCurrentMinionLeaderPicker == null) {
                        minionLeaderPicker.imgHighlight.gameObject.SetActive(true);
                        tempCurrentMinionLeaderPicker = minionLeaderPicker;
                    }
                } else if (minion == PlayerManager.Instance.player.currentMinionLeader) {
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
    private List<INTERVENTION_ABILITY> chosenAbilities;
    private void RandomizeStartingAbilities() {
        List<INTERVENTION_ABILITY> abilitiesPool = PlayerManager.Instance.allInterventionAbilities.ToList();
        chosenAbilities = new List<INTERVENTION_ABILITY>();

        while (chosenAbilities.Count != startingAbilityIcons.Length) {
            INTERVENTION_ABILITY randomAbility = abilitiesPool[UnityEngine.Random.Range(0, abilitiesPool.Count)];
            chosenAbilities.Add(randomAbility);
            abilitiesPool.Remove(randomAbility);
        }

        for (int i = 0; i < startingAbilityIcons.Length; i++) {
            INTERVENTION_ABILITY randomAbility = chosenAbilities[i];
            string abilityName = Utilities.NormalizeStringUpperCaseFirstLetters(randomAbility.ToString());
            startingAbilityIcons[i].sprite = PlayerManager.Instance.GetJobActionSprite(abilityName);
            startingAbilityLbls[i].text = abilityName;
            startingAbilities[i] = randomAbility;
        }
    }
    public void OnHoverStartingSpell(int index) {
        INTERVENTION_ABILITY spell = chosenAbilities[index];
        UIManager.Instance.ShowSmallInfo(PlayerManager.Instance.allInterventionAbilitiesData[spell].description, Utilities.NormalizeStringUpperCaseFirstLetters(spell.ToString()));
    }
    public void RerollAbilities() {
        RandomizeStartingAbilities();
    }
    public void RandomizeTwoStartingMinions(ref UnsummonedMinionData minion1Data, ref UnsummonedMinionData minion2Data) {

        string minionName1 = RandomNameGenerator.Instance.GenerateMinionName();
        string minionName2 = RandomNameGenerator.Instance.GenerateMinionName();

        COMBAT_ABILITY minionCombatAbilityType1 = PlayerManager.Instance.allCombatAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allCombatAbilities.Length)];
        COMBAT_ABILITY minionCombatAbilityType2 = PlayerManager.Instance.allCombatAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allCombatAbilities.Length)];

        List<string> filteredDeadlySinClasses = new List<string>();
        foreach (KeyValuePair<string, DeadlySin> kvp in CharacterManager.Instance.deadlySins) {
            if (kvp.Value.CanDoDeadlySinAction(DEADLY_SIN_ACTION.BUILDER) || kvp.Value.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INVADER)) {
                filteredDeadlySinClasses.Add(kvp.Key);
            }
        }

        int class1Index = UnityEngine.Random.Range(0, filteredDeadlySinClasses.Count);
        string minionClassName1 = filteredDeadlySinClasses[class1Index];
        filteredDeadlySinClasses.RemoveAt(class1Index);

        string minionClassName2 = string.Empty;
        if (CharacterManager.Instance.CanDoDeadlySinAction(minionClassName1, DEADLY_SIN_ACTION.INVADER)
            && CharacterManager.Instance.CanDoDeadlySinAction(minionClassName1, DEADLY_SIN_ACTION.BUILDER)) {
            minionClassName2 = CharacterManager.sevenDeadlySinsClassNames[UnityEngine.Random.Range(0, CharacterManager.sevenDeadlySinsClassNames.Length)];
        } else {
            if (CharacterManager.Instance.CanDoDeadlySinAction(minionClassName1, DEADLY_SIN_ACTION.INVADER)) {
                filteredDeadlySinClasses = filteredDeadlySinClasses.Where(x => CharacterManager.Instance.CanDoDeadlySinAction(x, DEADLY_SIN_ACTION.BUILDER)).ToList();
            } else if (CharacterManager.Instance.CanDoDeadlySinAction(minionClassName1, DEADLY_SIN_ACTION.BUILDER)) {
                filteredDeadlySinClasses = filteredDeadlySinClasses.Where(x => CharacterManager.Instance.CanDoDeadlySinAction(x, DEADLY_SIN_ACTION.INVADER)).ToList();
            }
            minionClassName2 = filteredDeadlySinClasses[UnityEngine.Random.Range(0, filteredDeadlySinClasses.Count)];
        }

        minion1Data = new UnsummonedMinionData() {
            minionName = minionName1,
            className = minionClassName1,
            combatAbility = minionCombatAbilityType1,
            interventionAbilitiesToResearch = CharacterManager.Instance.Get3RandomResearchInterventionAbilities(CharacterManager.Instance.GetDeadlySin(minionClassName1)),
        };

        minion2Data = new UnsummonedMinionData() {
            minionName = minionName2,
            className = minionClassName2,
            combatAbility = minionCombatAbilityType2,
            interventionAbilitiesToResearch = CharacterManager.Instance.Get3RandomResearchInterventionAbilities(CharacterManager.Instance.GetDeadlySin(minionClassName2)),
        };
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
        if (tempCurrentMinionLeaderPicker != null) {
            PlayerManager.Instance.player.SetMinionLeader(tempCurrentMinionLeaderPicker.minion);
        } else {
            //If story event, randomize minion leader, if not, keep current minion leader
            //if(PlayerManager.Instance.player.currentTileBeingCorrupted.landmarkOnTile.yieldType == LANDMARK_YIELD_TYPE.STORY_EVENT) {
            //    Minion minion = PlayerManager.Instance.player.GetRandomMinion();
            //    PlayerManager.Instance.player.SetMinionLeader(minion);
            //}
        }
        if (PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile != null) {
            InnerMapManager.Instance.TryShowAreaMap(PlayerManager.Instance.player.currentTileBeingCorrupted.areaOfTile);
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
    [Header("Summons")]
    [SerializeField] private Image currentSummonImg;
    [SerializeField] private GameObject currentSummonLvlGO;
    [SerializeField] private TextMeshProUGUI currentSummonLvlLbl;
    [SerializeField] private Button summonBtn;
    [SerializeField] private GameObject summonCover;
    [SerializeField] private UIHoverPosition summonTooltipPos;
    [SerializeField] private Button cycleSummonLeft;
    [SerializeField] private Button cycleSummonRight;
    [SerializeField] private GameObject summonSlotFrameGO;
    private bool isSummoning = false; //if the player has clicked the summon button and is targetting a tile to place the summon on.
    private SummonSlot currentlySelectedSummonSlot; //the summon type that is currently shown in the UI
    private void UpdateSummonsInteraction() {
        bool state = /*currentlySelectedSummonSlot != null && currentlySelectedSummonSlot.summon != null && !currentlySelectedSummonSlot.summon.hasBeenUsed &&*/ InnerMapManager.Instance.isAnAreaMapShowing;
        //summonCover.SetActive(!state);
        summonBtn.interactable = state;
    }
    private void OnPlayerGainedSummonSlot(SummonSlot slot) {
        UpdateSummonsInteraction();
        //if (currentlySelectedSummonSlot == null) {
        SetCurrentlySelectedSummonSlot(slot);
        //}
    }
    private void OnPlayerLostSummonSlot(SummonSlot slot) {
        UpdateSummonsInteraction();
        //if (currentlySelectedSummonSlot == slot) {
        SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.summonSlots.FirstOrDefault());
        //}
    }
    public void OnGainNewSummon(Summon newSummon) {
        UpdateSummonsInteraction();
        if (currentlySelectedSummonSlot == null || currentlySelectedSummonSlot.summon == null || currentlySelectedSummonSlot.summon == newSummon) {
            SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.GetSummonSlotBySummon(newSummon));
        }
        //ShowNewObjectInfo(newSummon);
        //AssignNewActionToLatestItem(newSummon);
    }
    public void OnRemoveSummon(Summon summon) {
        UpdateSummonsInteraction();
        //if (PlayerManager.Instance.player.GetTotalSummonsCount() == 0) { //the player has no more summons left
        SetCurrentlySelectedSummonSlot(currentlySelectedSummonSlot);
        //} else if (currentlySelectedSummonSlot.summon == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
        //    CycleSummons(1);
        //}
    }
    private void OnSummonUsed(Summon summon) {
        UpdateSummonsInteraction();
        //if (PlayerManager.Instance.player.GetTotalSummonsCount() == 0) { //the player has no more summons left
        SetCurrentlySelectedSummonSlot(currentlySelectedSummonSlot);
        //} else if (currentlySelectedSummonSlot.summon == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
        //    CycleSummons(1);
        //}
    }
    public void SetCurrentlySelectedSummonSlot(SummonSlot summonSlot) {
        currentlySelectedSummonSlot = summonSlot;
        if (currentlySelectedSummonSlot == null) {
            //no summon slot yet
            currentSummonImg.gameObject.SetActive(false);
            summonSlotFrameGO.SetActive(false);
            cycleSummonLeft.gameObject.SetActive(false);
            cycleSummonRight.gameObject.SetActive(false);
            //currentSummonLvlGO.SetActive(false);
        } else if (currentlySelectedSummonSlot.summon == null) {
            //summon slot has no summon
            summonSlotFrameGO.SetActive(true);
            currentSummonImg.gameObject.SetActive(false);
            //currentSummonLvlGO.SetActive(true);
            currentSummonLvlLbl.text = currentlySelectedSummonSlot.level.ToString();
        } else {
            //summon slot has summon
            summonSlotFrameGO.SetActive(true);
            currentSummonImg.gameObject.SetActive(true);
            currentSummonImg.sprite = CharacterManager.Instance.GetSummonSettings(currentlySelectedSummonSlot.summon.summonType).summonPortrait;
            //currentSummonLvlGO.SetActive(true);
            currentSummonLvlLbl.text = currentlySelectedSummonSlot.level.ToString();
        }
        if (currentlySelectedSummonSlot != null) {
            if (PlayerManager.Instance.player.summonSlots.Count == 1) {
                cycleSummonLeft.gameObject.SetActive(false);
                cycleSummonRight.gameObject.SetActive(false);
            } else {
                int index = PlayerManager.Instance.player.summonSlots.IndexOf(currentlySelectedSummonSlot);
                if (index == 0) {
                    cycleSummonLeft.gameObject.SetActive(false);
                    cycleSummonRight.gameObject.SetActive(true);
                } else if (index == PlayerManager.Instance.player.summonSlots.Count - 1) {
                    cycleSummonLeft.gameObject.SetActive(true);
                    cycleSummonRight.gameObject.SetActive(false);
                } else {
                    cycleSummonLeft.gameObject.SetActive(true);
                    cycleSummonRight.gameObject.SetActive(true);
                }
            }

        }

        UpdateSummonsInteraction();
    }
    public void UpdateCurrentlySelectedSummonSlotLevel(SummonSlot summonSlot) {
        if (currentlySelectedSummonSlot == summonSlot) {
            currentSummonLvlLbl.text = currentlySelectedSummonSlot.level.ToString();
        }
    }
    public void CycleSummons(int cycleDirection) {
        int currentSelectedSummonSlotIndex = PlayerManager.Instance.player.GetIndexForSummonSlot(currentlySelectedSummonSlot);
        int index = currentSelectedSummonSlotIndex;
        //int currentSummonCount = PlayerManager.Instance.player.summonSlots.Count;
        //while (true) {
        int next = index + cycleDirection;
        //if (next >= currentSummonCount) {
        //    next = 0;
        //} else if (next <= 0) {
        //    next = currentSummonCount - 1;
        //}
        //if (next < 0) {
        //    next = 0;
        //}
        //index = next;
        SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.summonSlots[next]);
    }
    public void ShowSummonTooltip() {
        if (currentlySelectedSummonSlot.summon != null) {
            string header = currentlySelectedSummonSlot.summon.summonType.SummonName() + " <i>(Click to summon.)</i>";
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
        unleashSummonUI.ShowUnleashSummonUI();
        //CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Target);
        //isSummoning = true;
    }
    public void TryPlaceSummon(Summon summon) {
        LocationGridTile mainEntrance = InnerMapManager.Instance.currentlyShowingMap.GetRandomUnoccupiedEdgeTile();
        //LocationGridTile tile = InteriorMapManager.Instance.GetTileFromMousePosition();
        Summon summonToPlace = summon;
        summonToPlace.CreateMarker();
        summonToPlace.marker.InitialPlaceMarkerAt(mainEntrance);
        //PlayerManager.Instance.player.RemoveSummon(summonToPlace);
        summonToPlace.OnPlaceSummon(mainEntrance);
        PlayerManager.Instance.player.RemoveSummon(summonToPlace);
        Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summonToPlace);
        summonToPlace.CenterOnCharacter();
        //isSummoning = false;
        //if (!UIManager.Instance.IsMouseOnUI()) {
        //    LocationGridTile tile = InteriorMapManager.Instance.GetTileFromMousePosition();
        //    Summon summonToPlace = currentlySelectedSummonSlot.summon;
        //    summonToPlace.CreateMarker();
        //    summonToPlace.marker.InitialPlaceMarkerAt(mainEntrance);
        //    //PlayerManager.Instance.player.RemoveSummon(summonToPlace);
        //    summonToPlace.OnPlaceSummon(mainEntrance);
        //    PlayerManager.Instance.player.RemoveSummon(summonToPlace);
        //    Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summonToPlace);
        //}
        //CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
    }
    public void TryPlaceSummon(Summon summon, LocationGridTile locationTile) {
        Summon summonToPlace = summon;
        summonToPlace.homeRegion.RemoveCharacterFromLocation(summonToPlace);
        summonToPlace.CreateMarker();
        summonToPlace.marker.InitialPlaceMarkerAt(locationTile);
        //PlayerManager.Instance.player.RemoveSummon(summonToPlace);
        summonToPlace.OnPlaceSummon(locationTile);
        Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summonToPlace);
    }

    private void CancelSummon() {
        isSummoning = false;
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
    }
    public void SetSummonCoverState(bool state) {
        summonCover.SetActive(state);
    }
    #endregion

    #region Artifacts
    [Header("Artifacts")]
    [SerializeField] private Image currentArtifactImg;
    [SerializeField] private GameObject currentArtifactLvlGO;
    [SerializeField] private TextMeshProUGUI currentArtifactLvlLbl;
    [SerializeField] private Button summonArtifactBtn;
    [SerializeField] private GameObject summonArtifactCover;
    [SerializeField] private UIHoverPosition summonArtifactTooltipPos;
    [SerializeField] private Button cycleArtifactLeft;
    [SerializeField] private Button cycleArtifactRight;
    [SerializeField] private GameObject artifactSlotFrameGO;
    private bool isSummoningArtifact = false; //if the player has clicked the summon artifact button and is targetting a tile to place the summon on.
    private ArtifactSlot currentlySelectedArtifactSlot; //the artifact that is currently shown in the UI
    private void UpdateArtifactsInteraction() {
        bool state = currentlySelectedArtifactSlot != null && currentlySelectedArtifactSlot.artifact != null && !currentlySelectedArtifactSlot.artifact.hasBeenUsed;
        summonArtifactBtn.interactable = state && InnerMapManager.Instance.isAnAreaMapShowing;
        summonArtifactCover.SetActive(!summonArtifactBtn.interactable);
    }
    private void OnPlayerGainedArtifactSlot(ArtifactSlot slot) {
        UpdateArtifactsInteraction();
        //if (currentlySelectedArtifactSlot == null) {
        SetCurrentlySelectedArtifactSlot(slot);
        //}
    }
    private void OnPlayerLostArtifactSlot(ArtifactSlot slot) {
        UpdateArtifactsInteraction();
        //if (currentlySelectedArtifactSlot == slot) {
        SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots.FirstOrDefault());
        //}
    }
    private void OnGainNewArtifact(Artifact newArtifact) {
        UpdateArtifactsInteraction();
        if (currentlySelectedArtifactSlot == null || currentlySelectedArtifactSlot.artifact == null || currentlySelectedArtifactSlot.artifact == newArtifact) {
            SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.GetArtifactSlotByArtifact(newArtifact));
        }
        //ShowNewObjectInfo(newArtifact);
        //AssignNewActionToLatestItem(newSummon);
    }
    private void OnRemoveArtifact(Artifact artifact) {
        UpdateArtifactsInteraction();
        //if (PlayerManager.Instance.player.GetTotalArtifactCount() == 0) { //the player has no more artifacts left
        SetCurrentlySelectedArtifactSlot(currentlySelectedArtifactSlot);
        //} else if (currentlySelectedArtifactSlot.artifact == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
        //    CycleArtifacts(1);
        //}
    }
    private void OnUsedArtifact(Artifact artifact) {
        UpdateArtifactsInteraction();
        //if (PlayerManager.Instance.player.GetTotalArtifactCount() == 0) { //the player has no more artifacts left
        SetCurrentlySelectedArtifactSlot(currentlySelectedArtifactSlot);
        //} else if (currentlySelectedArtifactSlot.artifact == null) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
        //    CycleArtifacts(1);
        //}
        //else if (artifact == currentlySelectedArtifactSlot
        //    && PlayerManager.Instance.player.GetAvailableArtifactsOfTypeCount(artifact.type) == 0) { //the current still has summons left but not of the type that was removed and that type is the players currently selected type
        //    CycleArtifacts(1);
        //}
    }
    public void SetCurrentlySelectedArtifactSlot(ArtifactSlot artifactSlot) {
        currentlySelectedArtifactSlot = artifactSlot;
        if (currentlySelectedArtifactSlot == null) {
            //player has no artifact slots yet
            currentArtifactImg.gameObject.SetActive(false);
            cycleArtifactLeft.gameObject.SetActive(false);
            cycleArtifactRight.gameObject.SetActive(false);
            //currentArtifactLvlGO.SetActive(false);
            artifactSlotFrameGO.SetActive(false);
        } else if (currentlySelectedArtifactSlot.artifact == null) {
            //artifact slot has no artifact
            artifactSlotFrameGO.SetActive(true);
            currentArtifactImg.gameObject.SetActive(false);
            //currentArtifactLvlGO.SetActive(true);
            currentArtifactLvlLbl.text = currentlySelectedArtifactSlot.level.ToString();
        } else {
            //player has at least 1 artifact slot and 1 artifact
            artifactSlotFrameGO.SetActive(true);
            currentArtifactImg.gameObject.SetActive(true);
            currentArtifactImg.sprite = CharacterManager.Instance.GetArtifactSettings(currentlySelectedArtifactSlot.artifact.type).artifactPortrait;
            //currentArtifactLvlGO.SetActive(true);
            currentArtifactLvlLbl.text = currentlySelectedArtifactSlot.level.ToString();
        }

        if (currentlySelectedArtifactSlot != null) {
            if (PlayerManager.Instance.player.artifactSlots.Count == 1) {
                cycleArtifactLeft.gameObject.SetActive(false);
                cycleArtifactRight.gameObject.SetActive(false);
            } else {
                int index = PlayerManager.Instance.player.artifactSlots.IndexOf(currentlySelectedArtifactSlot);
                if (index == 0) {
                    cycleArtifactLeft.gameObject.SetActive(false);
                    cycleArtifactRight.gameObject.SetActive(true);
                } else if (index == PlayerManager.Instance.player.artifactSlots.Count - 1) {
                    cycleArtifactLeft.gameObject.SetActive(true);
                    cycleArtifactRight.gameObject.SetActive(false);
                } else {
                    cycleArtifactLeft.gameObject.SetActive(true);
                    cycleArtifactRight.gameObject.SetActive(true);
                }
            }

        }
        UpdateArtifactsInteraction();
    }
    public void UpdateCurrentlySelectedArtifactSlotLevel(ArtifactSlot artifactSlot) {
        if (currentlySelectedArtifactSlot == artifactSlot) {
            currentArtifactLvlLbl.text = currentlySelectedArtifactSlot.level.ToString();
        }
    }
    public void CycleArtifacts(int cycleDirection) {
        int currentSelectedArtifactSlotIndex = PlayerManager.Instance.player.GetIndexForArtifactSlot(currentlySelectedArtifactSlot);
        int index = currentSelectedArtifactSlotIndex;
        //int currentArtifactCount = PlayerManager.Instance.player.GetTotalArtifactCount();
        //while (true) {
        //    int next = index + cycleDirection;
        //    if (next >= currentArtifactCount) {
        //        next = 0;
        //    } else if (next <= 0) {
        //        next = currentArtifactCount - 1;
        //    }
        //    if (next < 0) {
        //        next = 0;
        //    }
        //    index = next;
        //    if (PlayerManager.Instance.player.artifactSlots[index].artifact != null) {
        //        SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots[index]);
        //        break;
        //    } else if (index == currentSelectedArtifactSlotIndex) {//This means that artifact slots was already cycled through all of it and it can't find an artifact, end the loop when it happens
        //        SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots[currentSelectedArtifactSlotIndex]);
        //        break;
        //    }
        //}
        int next = index + cycleDirection;
        SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots[next]);
    }
    public void ShowArtifactTooltip() {
        if (currentlySelectedArtifactSlot.artifact != null) {
            string header = Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedArtifactSlot.artifact.type.ToString()) + " <i>(Click to place.)</i>";
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
            LocationGridTile tile = InnerMapManager.Instance.GetTileFromMousePosition();
            if (tile.objHere == null) {
                Artifact artifactToPlace = currentlySelectedArtifactSlot.artifact;
                if (tile.structure.AddPOI(artifactToPlace, tile)) {
                    artifactToPlace.SetIsSummonedByPlayer(true);
                    PlayerManager.Instance.player.RemoveArtifact(artifactToPlace);
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
    public void SetArtifactCoverState(bool state) {
        summonArtifactCover.SetActive(state);
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
        CharacterNameplateItem[] items = Utilities.GetComponentsInDirectChildren<CharacterNameplateItem>(killCountScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            item.transform.SetParent(killSummaryScrollView.content);
        }
    }
    public void BackToWorld() {
        Utilities.DestroyChildren(killSummaryScrollView.content);
        Area closedArea = InnerMapManager.Instance.HideAreaMap();
        SetCurrentlySelectedSummonSlot(PlayerManager.Instance.player.summonSlots.FirstOrDefault());
        SetCurrentlySelectedArtifactSlot(PlayerManager.Instance.player.artifactSlots.FirstOrDefault());
        successfulAreaCorruptionGO.SetActive(false);
        InnerMapManager.Instance.DestroyAreaMap(closedArea);

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
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            Minion currMinion = PlayerManager.Instance.player.minions[i];
            if (currMinion.assignedRegion != null && currMinion.assignedRegion.area != null && PlayerManager.Instance.player.currentAreaBeingInvaded == currMinion.assignedRegion.area) {
                GameObject go = Instantiate(combatAbilityButtonPrefab, combatAbilityGO.transform);
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
    [SerializeField] private Toggle killSummaryToggle;
    private void UpdateKillCountActiveState() {
        bool state = InnerMapManager.Instance.isAnAreaMapShowing;
        killCountGO.SetActive(state);
        killSummaryGO.SetActive(false);
    }
    private void LoadKillCountCharacterItems() {
        aliveCount = 0;
        allFilteredCharactersCount = 0;
        unusedKillCountCharacterItems = 0;
        killCountCharacterItems = new List<CharacterNameplateItem>();
        for (int i = 0; i < 20; i++) { //Initial number is 20
            CreateNewKillCountCharacterItem();
        }
    }
    private CharacterNameplateItem CreateNewKillCountCharacterItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(killCharacterItemPrefab.name, Vector3.zero, Quaternion.identity, killCountScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        go.SetActive(false);
        killCountCharacterItems.Add(item);
        unusedKillCountCharacterItems++;
        return item;
    }
    //This must only be called once during initialization
    private void InitialUpdateKillCountCharacterItems() {
        //CharacterNameplateItem[] items = Utilities.GetComponentsInDirectChildren<CharacterNameplateItem>(killCountScrollView.content.gameObject);
        //for (int i = 0; i < items.Length; i++) {
        //    ObjectPoolManager.Instance.DestroyObject(items[i].gameObject);
        //}
        LoadKillCountCharacterItems();
        List<CharacterNameplateItem> alive = new List<CharacterNameplateItem>();
        List<CharacterNameplateItem> dead = new List<CharacterNameplateItem>();
        List<Character> allCharacters = CharacterManager.Instance.allCharacters;
        int allCharactersCount = CharacterManager.Instance.allCharacters.Count;
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        //if (allCharactersCount < killCountCharacterItemsCount) {
        //    for (int i = allCharactersCount; i < killCountCharacterItemsCount; i++) {
        //        killCountCharacterItems[i].gameObject.SetActive(false);
        //    }
        //}
        string log = "Initial Kill Count UI";
        for (int i = 0; i < allCharactersCount; i++) {
            Character character = allCharacters[i];
            log += "\nCharacter: " + character.name;
            if (i >= killCountCharacterItemsCount) {
                CreateNewKillCountCharacterItem();
            }
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (!WillCharacterBeShownInKillCount(character)) {
                //Do not show minions and summons
                item.gameObject.SetActive(false);
                log += " - do not show";
                continue;
            }
            item.SetObject(character);
            item.SetAsButton();
            item.ClearAllOnClickActions();
            item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, false));
            item.gameObject.SetActive(true);
            allFilteredCharactersCount++;
            unusedKillCountCharacterItems--;
            if (/*character.isFactionless*/
                //|| character.faction == PlayerManager.Instance.player.playerFaction
                //|| character.faction == FactionManager.Instance.disguisedFaction
                /*||*/ !character.IsAble()) { //added checking for faction in cases that the character was raised from dead (Myk, if the concern here is only from raise dead, I changed the checker to returnedToLife to avoid conflicts with factions, otherwise you can return it to normal. -Chy)
                dead.Add(item);
                log += " - dead";
            } else {
                aliveCount++;
                alive.Add(item);
                log += " - alive";
            }
            //GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(killCharacterItemPrefab.name, Vector3.zero, Quaternion.identity, killCountScrollView.content);
            //CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
            //item.SetObject(character);
            //item.SetAsButton();
            //item.ClearAllOnClickActions();
            //item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, false));
        }
        Debug.Log(log);
        UpdateKillCount();

        aliveHeader.transform.SetAsFirstSibling();
        for (int i = 0; i < alive.Count; i++) {
            CharacterNameplateItem currItem = alive[i];
            currItem.transform.SetSiblingIndex(i + 1);
        }
        deadHeader.transform.SetSiblingIndex(alive.Count + 1);
        for (int i = 0; i < dead.Count; i++) {
            CharacterNameplateItem currItem = dead[i];
            currItem.transform.SetSiblingIndex(alive.Count + i + 2);
        }
        //OrderKillSummaryItems();
        //UpdateKillCount();
    }
    private void OnAddNewCharacter(Character character) {
        if (!WillCharacterBeShownInKillCount(character)) {
            //Do not show minions and summons
            return;
        }
        allFilteredCharactersCount++;
        CharacterNameplateItem item = null;
        if (unusedKillCountCharacterItems > 0) {
            item = GetUnusedCharacterNameplateItem();
        } else {
            item = CreateNewKillCountCharacterItem();
        }
        item.SetObject(character);
        item.SetAsButton();
        item.ClearAllOnClickActions();
        item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, false));
        item.gameObject.SetActive(true);
        unusedKillCountCharacterItems--;
        if (character.isFactionless
            //|| character.faction == PlayerManager.Instance.player.playerFaction
            //|| character.faction == FactionManager.Instance.disguisedFaction
            || !character.IsAble()) { //added checking for faction in cases that the character was raised from dead (Myk, if the concern here is only from raise dead, I changed the checker to returnedToLife to avoid conflicts with factions, otherwise you can return it to normal. -Chy)
            if (allFilteredCharactersCount == killCountCharacterItems.Count) {
                item.transform.SetAsLastSibling();
            } else {
                item.transform.SetSiblingIndex(allFilteredCharactersCount + 2);
            }
        } else {
            aliveCount++;
            item.transform.SetSiblingIndex(deadHeader.transform.GetSiblingIndex());
        }
        UpdateKillCount();
    }
    private void TransferCharacterFromActiveToInactive(Character character) {
        if (!WillCharacterBeShownInKillCount(character)) {
            return;
        }
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if (allFilteredCharactersCount == killCountCharacterItems.Count) {
            item.transform.SetAsLastSibling();
        } else {
            item.transform.SetSiblingIndex(allFilteredCharactersCount + 2);
        }
        aliveCount--;
        //UpdateKillCount();
    }
    private void TransferCharacterFromInactiveToActive(Character character) {
        if (!WillCharacterBeShownInKillCount(character)) {
            return;
        }
        CharacterNameplateItem item = GetInactiveCharacterNameplateItem(character);

        if (item != null) {
            int index = item.transform.GetSiblingIndex();
            int deadHeaderIndex = deadHeader.transform.GetSiblingIndex();
            if (index > deadHeaderIndex) {
                item.transform.SetSiblingIndex(deadHeaderIndex);
                aliveCount++;
            }
        }
        //UpdateKillCount();
    }
    private void OnCharacterBecomesMinionOrSummon(Character character) {
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if (item != null) {
            item.gameObject.SetActive(false);
            aliveCount--;
            allFilteredCharactersCount--;
            unusedKillCountCharacterItems++;
            //UpdateKillCount();
        }
    }
    private void OnCharacterBecomesNonMinionOrSummon(Character character) {
        OnAddNewCharacter(character);
    }
    private CharacterNameplateItem GetUnusedCharacterNameplateItem() {
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        for (int i = killCountCharacterItemsCount - 1; i >= 0; i--) {
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (!item.gameObject.activeSelf) {
                return item;
            }
        }
        return null;
    }
    private CharacterNameplateItem GetActiveCharacterNameplateItem(Character character) {
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        for (int i = 0; i < killCountCharacterItemsCount; i++) {
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (item.gameObject.activeSelf && item.character == character) {
                return item;
            }
        }
        return null;
    }
    private CharacterNameplateItem GetInactiveCharacterNameplateItem(Character character) {
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        for (int i = killCountCharacterItemsCount - 1; i >= 0; i--) {
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (item.gameObject.activeSelf && item.character == character) {
                return item;
            }
        }
        return null;
    }
    private void UpdateKillCount() {
        int aliveCount = 0;
        //TODO: Optimize this
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if (/*!character.isFactionless &&*/ character.IsAble() && WillCharacterBeShownInKillCount(character)) {
                aliveCount++;
            }
        }
        killCountLbl.text = aliveCount + "/" + allFilteredCharactersCount;
        if (aliveCount <= 0) {
            //player has won
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
            Messenger.Broadcast(Signals.HIDE_MENUS);
            SuccessfulAreaCorruption();
        }
    }
    //private void OrderKillSummaryItems() {
    //    CharacterNameplateItem[] items = Utilities.GetComponentsInDirectChildren<CharacterNameplateItem>(killCountScrollView.content.gameObject);
    //    List<CharacterNameplateItem> alive = new List<CharacterNameplateItem>();
    //    List<CharacterNameplateItem> dead = new List<CharacterNameplateItem>();
    //    for (int i = 0; i < items.Length; i++) {
    //        CharacterNameplateItem currItem = items[i];
    //        if (!currItem.character.IsAble() || !LandmarkManager.Instance.enemyOfPlayerArea.region.IsFactionHere(currItem.character.faction)) { //added checking for faction in cases that the character was raised from dead (Myk, if the concern here is only from raise dead, I changed the checker to returnedToLife to avoid conflicts with factions, otherwise you can return it to normal. -Chy)
    //            dead.Add(currItem);
    //        } else {
    //            alive.Add(currItem);
    //        }
    //    }
    //    aliveHeader.transform.SetAsFirstSibling();
    //    for (int i = 0; i < alive.Count; i++) {
    //        CharacterNameplateItem currItem = alive[i];
    //        currItem.transform.SetSiblingIndex(i + 1);
    //    }
    //    deadHeader.transform.SetSiblingIndex(alive.Count + 1);
    //    for (int i = 0; i < dead.Count; i++) {
    //        CharacterNameplateItem currItem = dead[i];
    //        currItem.transform.SetSiblingIndex(alive.Count + i + 2);
    //    }
    //    UpdateKillCount();
    //}
    public void ToggleKillSummary(bool isOn) {
        killSummaryGO.SetActive(isOn);
    }
    public void HideKillSummary() {
        if (killSummaryToggle.isOn) {
            killSummaryToggle.isOn = false;
        }
    }
    private bool WillCharacterBeShownInKillCount(Character character) {
        return character.isStillConsideredAlive;
        //if (character.minion != null || character is Summon || character.faction == PlayerManager.Instance.player.playerFaction
        //    /*|| character.faction == FactionManager.Instance.friendlyNeutralFaction*/) {
        //    //Do not show minions and summons
        //    return false;
        //}
        //return true;
    }
    #endregion

    #region Intervention Abilities
    private void LoadInterventionAbilitySlots() {
        interventionAbilityBtns = new PlayerJobActionButton[PlayerManager.Instance.player.MAX_INTERVENTION_ABILITIES];
        for (int i = 0; i < PlayerManager.Instance.player.MAX_INTERVENTION_ABILITIES; i++) {
            GameObject jobGO = UIManager.Instance.InstantiateUIObject(actionBtnPrefab.name, activeMinionActionsParent);
            PlayerJobActionButton actionBtn = jobGO.GetComponent<PlayerJobActionButton>();
            //actionBtn.gameObject.SetActive(false);
            interventionAbilityBtns[i] = actionBtn;
        }
    }
    private void UpdateInterventionAbilitySlots() {
        for (int i = 0; i < PlayerManager.Instance.player.interventionAbilitySlots.Length; i++) {
            PlayerJobActionSlot jobActionSlot = PlayerManager.Instance.player.interventionAbilitySlots[i];
            PlayerJobActionButton actionBtn = interventionAbilityBtns[i];
            actionBtn.SetJobAction(jobActionSlot);
            if (jobActionSlot.ability != null) {
                actionBtn.SetClickAction(() => PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(jobActionSlot.ability));
            } else {
                actionBtn.SetClickAction(null);
            }
        }
    }
    #endregion

    #region General Confirmation
    public void ShowGeneralConfirmation(string header, string body, string buttonText = "OK", System.Action onClickOK = null) {
        if (IsMajorUIShowing()) {
            AddPendingUI(() => ShowGeneralConfirmation(header, body, buttonText, onClickOK));
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        generalConfirmationTitleText.text = header.ToUpper();
        generalConfirmationBodyText.text = body;
        generalConfirmationButtonText.text = buttonText;
        generalConfirmationButton.onClick.RemoveAllListeners();
        generalConfirmationButton.onClick.AddListener(OnClickOKGeneralConfirmation);
        if (onClickOK != null) {
            generalConfirmationButton.onClick.AddListener(() => onClickOK.Invoke());
        }
        generalConfirmationGO.SetActive(true);
    }
    public void OnClickOKGeneralConfirmation() {
        generalConfirmationGO.SetActive(false);
        if (!TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
    }
    #endregion

    #region New Minion
    [Header("New Minion UI")]
    [SerializeField] private GameObject newMinionUIGO;
    [SerializeField] private MinionCard newMinionCard;
    public void ShowNewMinionUI(Minion minion) {
        if (IsMajorUIShowing()) {
            AddPendingUI(() => ShowNewMinionUI(minion));
            return;
        }
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        newMinionCard.SetMinion(minion);
        newMinionUIGO.SetActive(true);
    }
    public void HideNewMinionUI() {
        newMinionUIGO.SetActive(false);
        if (!TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
    }
    #endregion

    #region Minion List
    public bool isShowingMinionList { get { return minionListGO.activeSelf; } }
    [Header("Minion List")]
    [SerializeField] private TextMeshProUGUI minionCountLbl;
    [SerializeField] private GameObject minionItemPrefab;
    [SerializeField] private ScrollRect minionListScrollView;
    [SerializeField] private GameObject minionListGO;
    [SerializeField] private UIHoverPosition minionListCardTooltipPos;
    [SerializeField] private Toggle minionListToggle;
    private void UpdateMinionList() {
        Utilities.DestroyChildren(minionListScrollView.content);
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            Minion currMinion = PlayerManager.Instance.player.minions[i];
            CreateNewMinionItem(currMinion);
        }
        UpdateMinionCount();
    }
    private void CreateNewMinionItem(Minion minion) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(minionItemPrefab.name, Vector3.zero, Quaternion.identity, minionListScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        item.SetObject(minion.character);
        item.AddHoverEnterAction((character) => UIManager.Instance.ShowMinionCardTooltip(character.minion, minionListCardTooltipPos));
        item.AddHoverExitAction((character) => UIManager.Instance.HideMinionCardTooltip());
        item.SetAsDefaultBehaviour();
    }
    private void DeleteMinionItem(Minion minion) {
        CharacterNameplateItem item = GetMinionItem(minion);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        }
    }
    private CharacterNameplateItem GetMinionItem(Minion minion) {
        CharacterNameplateItem[] items = Utilities.GetComponentsInDirectChildren<CharacterNameplateItem>(minionListScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            if (item.character == minion.character) {
                return item;
            }
        }
        return null;
    }
    private void UpdateMinionCount() {
        minionCountLbl.text = PlayerManager.Instance.player.minions.Count.ToString();
    }
    private void OnGainedMinion(Minion minion) {
        CreateNewMinionItem(minion);
        UpdateMinionCount();
    }
    private void OnLostMinion(Minion minion) {
        DeleteMinionItem(minion);
        UpdateMinionCount();
    }
    public void ToggleMinionList(bool isOn) {
        minionListGO.SetActive(isOn);
    }
    public void HideMinionList() {
        if (minionListToggle.isOn) {
            minionListToggle.isOn = false;
        }
    }
    #endregion

    #region Demonic Landmark Toggles
    public void OnTogglePortal(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_PORTAL);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleSpire(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_SPIRE);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_SPIRE && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleAnvil(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_ANVIL);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_ANVIL && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleTheEye(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_EYE);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_EYE && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleTheNeedle(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_NEEDLES);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_NEEDLES && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleGoader(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.GOADER);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.GOADER && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleTheCrypt(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_CRYPT);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_CRYPT && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleTheKennel(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_KENNEL);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_KENNEL && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    public void OnToggleTheProfane(bool isOn) {
        if (isOn) {
            ShowDemonicLandmarkUI(LANDMARK_TYPE.THE_PROFANE);
        } else {
            if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE && !demonicToggleGroup.AnyTogglesOn()) {
                UIManager.Instance.regionInfoUI.CloseMenu();
            }
        }
    }
    private void ShowDemonicLandmarkUI(LANDMARK_TYPE demonicLandmark) {
        Region region = LandmarkManager.Instance.GetLandmarkOfType(demonicLandmark).tileLocation.region;
        UIManager.Instance.ShowRegionInfo(region, false);
    }
    private void UpdateDemonicLandmarkToggleInteractables() {
        List<LANDMARK_TYPE> existingTypes = LandmarkManager.Instance.GetAllLandmarks().Select(x => x.specificLandmarkType).ToList();
        portalToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_PORTAL);
        spireToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_SPIRE);
        anvilToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_ANVIL);
        eyeToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_EYE);
        needleToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_NEEDLES);
        fingerToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.GOADER);
        cryptToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_CRYPT);
        kennelToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_KENNEL);
        profaneToggle.interactable = existingTypes.Contains(LANDMARK_TYPE.THE_PROFANE);

        Toggle[] demonicToggles = Utilities.GetComponentsInDirectChildren<Toggle>(demonicToggleGroup.gameObject);
        for (int i = 0; i < demonicToggles.Length; i++) {
            Toggle t = demonicToggles[i];
            if (!t.interactable) {
                t.isOn = false;
            }
        }
    }
    private void UpdateClickedDemonicToggle() {
        if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType.IsPlayerLandmark()) {
            switch (UIManager.Instance.regionInfoUI.activeRegion.mainLandmark.specificLandmarkType) {
                case LANDMARK_TYPE.THE_PORTAL:
                    portalToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.THE_SPIRE:
                    spireToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.THE_ANVIL:
                    anvilToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.THE_EYE:
                    eyeToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.THE_NEEDLES:
                    needleToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.GOADER:
                    fingerToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.THE_CRYPT:
                    cryptToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.THE_KENNEL:
                    kennelToggle.isOn = true;
                    break;
                case LANDMARK_TYPE.THE_PROFANE:
                    profaneToggle.isOn = true;
                    break;

            }
        } else {
            portalToggle.isOn = false;
            spireToggle.isOn = false;
            anvilToggle.isOn = false;
            eyeToggle.isOn = false;
            needleToggle.isOn = false;
            fingerToggle.isOn = false;
            cryptToggle.isOn = false;
            kennelToggle.isOn = false;
            profaneToggle.isOn = false;
        }
    }
    #endregion

    #region Seize Object
    public void ShowSeizedObjectUI() {
        unseizeButton.gameObject.SetActive(true);
    }
    public void HideSeizedObjectUI() {
        unseizeButton.gameObject.SetActive(false);
    }
    //Not used right now, might be used in the future
    public void UpdateSeizedObjectUI() {
        unseizeButton.gameObject.SetActive(PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
    }
    public void OnClickSeizedObject() {
        PlayerManager.Instance.player.seizeComponent.PrepareToUnseize();
    }
    #endregion

    #region Spells
    public void OnToggleSpells(bool isOn) {
        if (isOn) {
            ShowSpells();
        } else {
            HideSpells();
        }
    }
    private void ShowSpells() {
        customDropdownList.ShowDropdown(spellsList, OnClickSpell);
    }
    private void HideSpells() {
        customDropdownList.HideDropdown();
    }
    private void OnClickSpell(string spellName) {
        if(PlayerManager.Instance.player.currentActivePlayerJobAction != null) {
            return;
        }
        INTERVENTION_ABILITY spell = (INTERVENTION_ABILITY) System.Enum.Parse(typeof(INTERVENTION_ABILITY), spellName.ToUpper().Replace(' ', '_'));
        //This is temporary only because we will eliminate instanced intervention abilities in the future, they will become singleton
        PlayerJobAction ability = PlayerManager.Instance.CreateNewInterventionAbility(spell);
        PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(ability);
    }
    #endregion

    #region Faction Actions
    public void OnToggleFactionActions(bool isOn) {
        if (isOn) {
            ShowFactionActions();
        } else {
            HideFactionActions();
        }
    }
    private void ShowFactionActions() {
        customDropdownList.ShowDropdown(factionActionsList, OnClickFactionAction);
    }
    private void HideFactionActions() {
        customDropdownList.HideDropdown();
    }
    private void OnClickFactionAction(string text) {
        //TODO
    }
    #endregion
}
