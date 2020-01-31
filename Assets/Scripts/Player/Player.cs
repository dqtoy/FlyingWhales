using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;
using Inner_Maps;
using Traits;
using Random = UnityEngine.Random;
// ReSharper disable Unity.NoNullPropagation

public class Player : ILeader {

    private const int MAX_INTEL = 3;
    public const int MAX_MINIONS = 7;
    public readonly int MAX_INTERVENTION_ABILITIES = 4;

    public Faction playerFaction { get; private set; }
    public Settlement playerSettlement { get; private set; }
    public int mana { get; private set; }
    public List<Intel> allIntel { get; private set; }
    public List<Minion> minions { get; private set; }
    public List<SummonSlot> summonSlots { get; private set; }
    public List<ArtifactSlot> artifactSlots { get; private set; }
    private int currentCorruptionDuration { get; set; }
    private int currentCorruptionTick { get; set; }
    private bool isTileCurrentlyBeingCorrupted { get; set; }
    public HexTile currentTileBeingCorrupted { get; private set; }
    public Minion currentMinionLeader { get; private set; }
    public Settlement currentSettlementBeingInvaded { get; private set; }
    public CombatAbility currentActiveCombatAbility { get; private set; }
    public Intel currentActiveIntel { get; private set; }
    public int maxSummonSlots { get; private set; } //how many summons can the player have
    public int maxArtifactSlots { get; private set; } //how many artifacts can the player have
    public PlayerJobActionSlot[] interventionAbilitySlots { get; private set; }
    public UnsummonedMinionData[] minionsToSummon { get; private set; }
    public HexTile portalTile { get; private set; }

    public float constructionRatePercentageModifier { get; private set; }

    //Components
    public SeizeComponent seizeComponent { get; private set; }

    #region getters/setters
    public int id => -645;
    public string name => "Player";
    public RACE race => RACE.HUMANS;
    public GENDER gender => GENDER.MALE;
    public Region currentRegion => null;
   
    public Region homeRegion => null;
    #endregion

    public Player() {
        allIntel = new List<Intel>();
        minions = new List<Minion>();
        summonSlots = new List<SummonSlot>();
        artifactSlots = new List<ArtifactSlot>();
        interventionAbilitySlots = new PlayerJobActionSlot[MAX_INTERVENTION_ABILITIES];
        minionsToSummon = new UnsummonedMinionData[3];
        maxSummonSlots = 0;
        maxArtifactSlots = 0;
        AdjustMana(EditableValuesManager.Instance.startingMana);
        seizeComponent = new SeizeComponent();
        ConstructAllInterventionAbilitySlots();
        GenerateMinionsToSummon();
        AddListeners();
    }
    public Player(SaveDataPlayer data) {
        allIntel = new List<Intel>();
        minions = new List<Minion>();
        maxSummonSlots = data.maxSummonSlots;
        maxArtifactSlots = data.maxArtifactSlots;
        mana = data.mana;
        minionsToSummon = data.minionsToSummon;
        SetConstructionRatePercentageModifier(data.constructionRatePercentageModifier);
        summonSlots = new List<SummonSlot>();
        for (int i = 0; i < summonSlots.Count; i++) {
            summonSlots.Add(data.summonSlots[i].Load());
        }

        artifactSlots = new List<ArtifactSlot>();
        for (int i = 0; i < artifactSlots.Count; i++) {
            artifactSlots.Add(data.artifactSlots[i].Load());
        }

        interventionAbilitySlots = new PlayerJobActionSlot[data.interventionAbilitySlots.Count];
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            interventionAbilitySlots[i] = data.interventionAbilitySlots[i].Load();
        }
        seizeComponent = new SeizeComponent();
        AddListeners();
    }

    public void SetPortalTile(HexTile tile) {
        portalTile = tile;
    }
    
    #region Listeners
    private void AddListeners() {
        AddWinListener();
        //goap
        Messenger.AddListener<string, ActualGoapNode>(Signals.AFTER_ACTION_STATE_SET, OnAfterActionStateSet);
        Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<ILocation>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<ILocation>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);

        //minions
        Messenger.AddListener<Minion, BaseLandmark>(Signals.MINION_ASSIGNED_PLAYER_LANDMARK, OnMinionAssignedToPlayerLandmark);
        Messenger.AddListener<Minion, BaseLandmark>(Signals.MINION_UNASSIGNED_PLAYER_LANDMARK, OnMinionUnassignedFromPlayerLandmark);
    }
    #endregion

    #region ILeader
    public void LevelUp() { }
    #endregion

    #region Settlement
    public Settlement CreatePlayerSettlement(BaseLandmark portal) {
        Settlement settlement = LandmarkManager.Instance.CreateNewSettlement(portal.tileLocation.region, LOCATION_TYPE.DEMONIC_INTRUSION, 0, portal.tileLocation);
        settlement.LoadAdditionalData();
        settlement.SetName("Demonic Intrusion");
        SetPlayerArea(settlement);
        portal.tileLocation.InstantlyCorruptAllOwnedInnerMapTiles();
        return settlement;
    }
    public void LoadPlayerArea(Settlement settlement) {
        //Biomes.Instance.CorruptTileVisuals(settlement.coreTile);
        //settlement.coreTile.tileLocation.SetCorruption(true);
        SetPlayerArea(settlement);
        //_demonicPortal.tileLocation.ScheduleCorruption();
    }
    public void SetPlayerArea(Settlement settlement) {
        playerSettlement = settlement;
    }
    private void OnInnerMapOpened(ILocation area) {
        //for (int i = 0; i < minions.Count; i++) {
        //    minions[i].ResetCombatAbilityCD();
        //}
        //ResetInterventionAbilitiesCD();
        //currentTargetFaction = settlement.owner;
    }
    private void OnInnerMapClosed(ILocation area) {
        //currentTargetFaction = null;
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction faction = FactionManager.Instance.CreateNewFaction(true, "Player faction");
        faction.SetLeader(this);
        faction.SetEmblem(FactionManager.Instance.GetFactionEmblem(6));
        SetPlayerFaction(faction);
    }
    public void CreatePlayerFaction(SaveDataPlayer data) {
        Faction faction = FactionManager.Instance.GetFactionBasedOnID(data.playerFactionID);
        faction.SetLeader(this);
        SetPlayerFaction(faction);
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    //public void SetPlayerTargetFaction(Faction faction) {
    //    currentTargetFaction = faction;
    //}
    #endregion

    #region Minions
    public Minion CreateNewMinion(Character character, bool initialize = true) {
        Minion minion = new Minion(character, true);
        if (initialize) {
            InitializeMinion(minion);
        }
        return minion;
    }
    public Minion CreateNewMinion(SaveDataMinion data) {
        Minion minion = new Minion(data);
        InitializeMinion(data, minion);
        return minion;
    }
    // public Minion CreateNewMinion(RACE race) {
    //     Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, race, GENDER.MALE, playerFaction, playerSettlement, null), false);
    //     //minion.character.CreateMarker();
    //     InitializeMinion(minion);
    //     return minion;
    // }
    public Minion CreateNewMinion(string className, RACE race, bool initialize = true) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, race, GENDER.MALE, playerFaction, playerSettlement), false);
        if (initialize) {
            InitializeMinion(minion);
        }
        return minion;
    }
    public Minion CreateNewMinionRandomClass() {
        string className = CharacterManager.sevenDeadlySinsClassNames[UnityEngine.Random.Range(0, CharacterManager.sevenDeadlySinsClassNames.Length)];
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, RACE.DEMON, GENDER.MALE, playerFaction, playerSettlement), false);
        InitializeMinion(minion);
        return minion;
    }
    private void InitializeMinion(Minion minion) {
        minion.SetRandomResearchInterventionAbilities(CharacterManager.Instance.Get3RandomResearchInterventionAbilities(minion.deadlySin));
        minion.SetCombatAbility(PlayerManager.Instance.CreateNewCombatAbility(PlayerManager.Instance.allCombatAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allCombatAbilities.Length)]));
    }
    private void InitializeMinion(SaveDataMinion data, Minion minion) {
        //for (int i = 0; i < data.interventionAbilities.Count; i++) {
        //    data.interventionAbilities[i].Load(minion);
        //}
        data.combatAbility.Load(minion);
    }
    public void AddMinion(Minion minion, bool showNewMinionUI = false) {
        //int currentMinionCount = GetCurrentMinionCount();
        //if(currentMinionCount == minions.Count) {
        //    //Broadcast minion is full, must be received by a UI that will pop up and let the player whether it will replace or be discarded
        //    PlayerUI.Instance.replaceUI.ShowReplaceUI(minions.ToList(), minion, ReplaceMinion, RejectMinion);
        //} else {
        //    minion.SetIndexDefaultSort(currentMinionCount);
        //    minions[currentMinionCount] = minion;
        //    if (showNewMinionUI) {
        //        PlayerUI.Instance.ShowNewMinionUI(minion);
        //    }
        //    PlayerUI.Instance.UpdateRoleSlots();
        //}
        if(minions.Count < MAX_MINIONS) {
            if (!minions.Contains(minion)) {
                minions.Add(minion);
                if (showNewMinionUI) {
                    PlayerUI.Instance.ShowNewMinionUI(minion);
                }
                //PlayerUI.Instance.UpdateRoleSlots();
                Messenger.Broadcast(Signals.PLAYER_GAINED_MINION, minion);
            }
        } else {
            PlayerUI.Instance.replaceUI.ShowReplaceUI(minions, minion, ReplaceMinion, RejectMinion);
        }
    }
    public void RemoveMinion(Minion minion) {
        if (minions.Remove(minion)) {
            //RearrangeMinions();
            //PlayerUI.Instance.UpdateRoleSlots();
            if (currentMinionLeader == minion) {
                SetMinionLeader(null);
            }
            Messenger.Broadcast(Signals.PLAYER_LOST_MINION, minion);
        }
    }
    public int GetCurrentMinionCount() {
        //int count = 0;
        //for (int i = 0; i < minions.Count; i++) {
        //    if(minions[i] != null) {
        //        count++;
        //    }
        //}
        return minions.Count;
    }
    public void SetMinionLeader(Minion minion) {
        currentMinionLeader = minion;
    }
    private void ReplaceMinion(object objToReplace, object objToAdd) {
        Minion minionToBeReplaced = objToReplace as Minion;
        Minion minionToBeAdded = objToAdd as Minion;

        for (int i = 0; i < minions.Count; i++) {
            if(minions[i] == minionToBeReplaced) {
                minionToBeAdded.SetIndexDefaultSort(i);
                minions[i] = minionToBeAdded;
                if(currentMinionLeader == minionToBeReplaced) {
                    SetMinionLeader(minionToBeAdded);
                }
                break;
                //PlayerUI.Instance.UpdateRoleSlots();
            }
        }
    }
    private void RejectMinion(object obj) { }
    public bool HasMinionWithCombatAbility(COMBAT_ABILITY ability) {
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion != null && currMinion.combatAbility.type == ability) {
                return true;
            }
        }
        return false;
    }
    public Minion GetRandomMinion() {
        List<Minion> minionChoices = new List<Minion>();
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion != null) {
                minionChoices.Add(currMinion);
            }
        }
        return minionChoices[UnityEngine.Random.Range(0, minionChoices.Count)];
    }
    public void LevelUpAllMinions(int amount) {
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion != null) {
                currMinion.LevelUp(amount);
            }
        }
    }
    public void LevelUpAllMinions() {
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion != null) {
                currMinion.LevelUp();
            }
        }
    }
    public void GenerateMinionsToSummon() {
        List<string> choices = CharacterManager.sevenDeadlySinsClassNames.ToList();
        for (int i = 0; i < minionsToSummon.Length; i++) {
            int index = UnityEngine.Random.Range(0, choices.Count);
            UnsummonedMinionData minionData = new UnsummonedMinionData() {
                minionName = RandomNameGenerator.Instance.GenerateMinionName(),
                className = choices[index],
                combatAbility = PlayerManager.Instance.allCombatAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allCombatAbilities.Length)],
                interventionAbilitiesToResearch = CharacterManager.Instance.Get3RandomResearchInterventionAbilities(CharacterManager.Instance.GetDeadlySin(choices[index])),
            };
            choices.RemoveAt(index);
            minionsToSummon[i] = minionData;
        }
    }
    public bool HasMinionAssignedTo(LANDMARK_TYPE type) {
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion.isAssigned && currMinion.assignedRegion.mainLandmark.specificLandmarkType == type) {
                return true;
            }
        }
        return false;
    }
    public List<Character> GetMinionsAssignedTo(LANDMARK_TYPE type) {
        List<Character> validMinions = new List<Character>();
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion.isAssigned && currMinion.assignedRegion.mainLandmark.specificLandmarkType == type) {
                validMinions.Add(currMinion.character);
            }
        }
        return validMinions;
    }
    #endregion

    #region Win/Lose Conditions
    private void AddWinListener() {
        Messenger.AddListener<Faction>(Signals.FACTION_LEADER_DIED, OnFactionLeaderDied);
    }
    private void OnFactionLeaderDied(Faction faction) {
        List<Faction> allUndestroyedFactions = FactionManager.Instance.allFactions.Where(
            x => x.name != "Neutral" 
            && x.name != "Player faction" 
            && x.isActive && !x.isDestroyed).ToList();
        if (allUndestroyedFactions.Count == 0) {
            Debug.LogError("All factions are destroyed! Player won!");
        }        
    }
    #endregion

    #region Role Actions
    public PlayerSpell currentActivePlayerSpell { get; private set; }
    public void SetCurrentlyActivePlayerJobAction(PlayerSpell action) {
        PlayerSpell previousActiveAction = currentActivePlayerSpell;
        currentActivePlayerSpell = action;
        if (currentActivePlayerSpell == null) {
            Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnSpellCast);
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
            PlayerJobActionButton jobActionButton = PlayerUI.Instance.GetPlayerJobActionButton(previousActiveAction);
            jobActionButton?.UpdateInteractableState();
            jobActionButton?.SetSelectedIconState(false);
            // if (previousActiveAction != null) {
            //     previousActiveAction.HideRange(InnerMapManager.Instance.GetTileFromMousePosition());
            // }
        } else {
            PlayerJobActionButton jobActionButton = PlayerUI.Instance.GetPlayerJobActionButton(currentActivePlayerSpell);
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Cross);
            Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnSpellCast);
            jobActionButton?.SetSelectedIconState(true);
        }
    }
    private void OnSpellCast(KeyCode key) {
        if (key == KeyCode.Mouse0) {
            TryExecuteCurrentActiveAction();
            SetCurrentlyActivePlayerJobAction(null);
            Messenger.RemoveListener<KeyCode>(Signals.KEY_DOWN, OnSpellCast);
        }
    }

    private void TryExecuteCurrentActiveAction() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return; //clicked on UI;
        }
        for (int i = 0; i < currentActivePlayerSpell.targetTypes.Length; i++) {
            bool activatedAction = false;
            switch (currentActivePlayerSpell.targetTypes[i]) {
                case SPELL_TARGET.NONE:
                    break;
                case SPELL_TARGET.CHARACTER:
                    if (InnerMapManager.Instance.currentlyShowingMap != null && InnerMapManager.Instance.currentlyHoveredPoi is Character) {
                        if (currentActivePlayerSpell.CanPerformActionTowards(InnerMapManager.Instance.currentlyHoveredPoi)) {
                            currentActivePlayerSpell.ActivateAction(InnerMapManager.Instance.currentlyHoveredPoi);
                            activatedAction = true;
                        } else {
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.TILE_OBJECT:
                    if (InnerMapManager.Instance.currentlyHoveredPoi is TileObject) {
                        if (currentActivePlayerSpell.CanPerformActionTowards(InnerMapManager.Instance.currentlyHoveredPoi)) {
                            currentActivePlayerSpell.ActivateAction(InnerMapManager.Instance.currentlyHoveredPoi);
                            activatedAction = true;
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.TILE:
                    LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null) {
                        if (currentActivePlayerSpell.CanPerformActionTowards(hoveredTile)) {
                            currentActivePlayerSpell.ActivateAction(hoveredTile);
                            activatedAction = true;
                        } 
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                default:
                    break;
            }
            if (activatedAction) {
                break;
            }
        }
        
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region Intel
    public void AddIntel(Intel newIntel) {
        if (!allIntel.Contains(newIntel)) {
            //for (int i = 0; i < allIntel.Count; i++) {
            //    Intel currIntel = allIntel[i];
            //    if (currIntel.intelLog == newIntel.intelLog) {
            //        return;
            //    }
            //}
            allIntel.Add(newIntel);
            if (allIntel.Count > MAX_INTEL) {
                RemoveIntel(allIntel[0]);
            }
            Messenger.Broadcast(Signals.PLAYER_OBTAINED_INTEL, newIntel);
        }
    }
    public void RemoveIntel(Intel intel) {
        if (allIntel.Remove(intel)) {
            Messenger.Broadcast(Signals.PLAYER_REMOVED_INTEL, intel);
        }
    }
    public void LoadIntels(SaveDataPlayer data) {
        //for (int i = 0; i < data.allIntel.Count; i++) {
        //    AddIntel(data.allIntel[i].Load());
        //}
    }
    /// <summary>
    /// Listener for when a character has finished doing an action.
    /// </summary>
    /// <param name="character">The character that finished the action.</param>
    /// <param name="actionNode">The action that was finished.</param>
    //private void OnCharacterDidAction(Character character, GoapAction action) {
    //    for (int i = 0; i < action.currentState.arrangedLogs.Count; i++) {
    //        if(action.currentState.arrangedLogs[i].notifAction != null) {
    //            action.currentState.arrangedLogs[i].notifAction();
    //        } else {
    //            bool showPopup = false;
    //            if (action.showIntelNotification) {
    //                if (action.shouldIntelNotificationOnlyIfActorIsActive) {
    //                    showPopup = ShouldShowNotificationFrom(character, true);
    //                } else {
    //                    showPopup = ShouldShowNotificationFrom(character, action.currentState.descriptionLog);
    //                }
    //            }
    //            if (showPopup) {
    //                if (!action.isNotificationAnIntel) {
    //                    Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, action.currentState.descriptionLog);
    //                } else {
    //                    Messenger.Broadcast<Intel>(Signals.SHOW_INTEL_NOTIFICATION, InteractionManager.Instance.CreateNewIntel(action, character));
    //                }
    //            }
    //        }
    //    }
    //}
    /// <summary>
    /// Listener for when a character starts an action.
    /// Character will go to target location. <see cref="GoapAction.DoAction"/>
    /// </summary>
    /// <param name="character">The character that will do the action.</param>
    /// <param name="action">The action that will be performed.</param>
    private void OnCharacterDoingAction(Character character, ActualGoapNode actionNode) {
        bool showPopup = false;
        Log log = actionNode.GetCurrentLog();
        if (actionNode.action.showNotification && log != null) { //TODO: added checking if actor is already at target tile. So that travelling notification won't show if that is the case. && !actionNode.IsActorAtTargetTile() 
            // if (actionNode.action.shouldIntelNotificationOnlyIfActorIsActive) {
            //     showPopup = ShouldShowNotificationFrom(actionNode.actor, true);
            // } else {
            //     showPopup = ShouldShowNotificationFrom(actionNode.actor, log);
            // }
            showPopup = ShouldShowNotificationFrom(actionNode.actor, log);
        }
        if (showPopup) {
            Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, log);
        }
    }
    /// <summary>
    /// Listener for when an action's state is set. Always means that the character has
    /// started performing the action.
    /// </summary>
    /// <param name="action">The action that is being performed.</param>
    /// <param name="state">The state that the action is in.</param>
    private void OnAfterActionStateSet(string stateName, ActualGoapNode actionNode) {
        bool showPopup = false;
        Log log = actionNode.GetCurrentLog();
        if (actionNode.action.showNotification && actionNode.currentState.duration > 0 && log != null) { //added checking for duration because this notification should only show for actions that have durations.
            showPopup = ShouldShowNotificationFrom(actionNode.actor, log);
        }
        if (showPopup) {
            Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, log);
        }
    }
    public void SetCurrentActiveIntel(Intel intel) {
        if (currentActiveIntel == intel) {
            //Do not process when setting the same combat ability
            return;
        }
        Intel previousIntel = currentActiveIntel;
        currentActiveIntel = intel;
        if (currentActiveIntel == null) {
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(previousIntel);
            intelItem?.SetClickedState(false);
            //InteriorMapManager.Instance.UnhighlightTiles();
            //GameManager.Instance.SetPausedState(false);
        } else {
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(currentActiveIntel);
            //change the cursor
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Cross);
            CursorManager.Instance.AddLeftClickAction(TryExecuteCurrentActiveIntel);
            CursorManager.Instance.AddLeftClickAction(() => SetCurrentActiveIntel(null));
            intelItem?.SetClickedState(true);
            //GameManager.Instance.SetPausedState(true);
        }
    }
    private void TryExecuteCurrentActiveIntel() {
        string hoverText = string.Empty;
        if (minions.Count > 0 && CanShareIntel(InnerMapManager.Instance.currentlyHoveredPoi, ref hoverText)) {
            Character targetCharacter = InnerMapManager.Instance.currentlyHoveredPoi as Character;
            //if(currentActiveIntel is EventIntel) {
            //    if((currentActiveIntel as EventIntel).action == null) {
            //        //If intel has no action, do not execute intel, just remove it instead
            //        PlayerManager.Instance.player.RemoveIntel(currentActiveIntel);
            //        return;
            //    }
            //}
            UIManager.Instance.OpenShareIntelMenu(targetCharacter, minions[0].character, currentActiveIntel);
        }
    }
    public bool CanShareIntel(IPointOfInterest poi, ref string hoverText) {
        if(poi is Character) {
            Character character = poi as Character;
            hoverText = string.Empty;
            if(character.traitContainer.GetNormalTrait<Trait>("Blessed", "Catatonic") != null) {
                hoverText = "Blessed/Catatonic characters cannot be targetted.";
                return false;
            }
            if(!character.faction.isPlayerFaction && !Utilities.IsRaceBeast(character.race)) { //character.role.roleType != CHARACTER_ROLE.BEAST && character.role.roleType != CHARACTER_ROLE.PLAYER
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Player Notifications
    public bool ShouldShowNotificationFrom(Character character, bool onlyClickedCharacter = false) {
#if TRAILER_BUILD
        if (character.name == "Fiona" || character.name == "Jamie" || character.name == "Audrey") {
            return true;
        }
        return false;
#endif

        if (!onlyClickedCharacter && InnerMapCameraMove.Instance.gameObject.activeSelf) { //&& !character.isDead
            if ((UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) || (character.marker != null &&  InnerMapCameraMove.Instance.CanSee(character.marker.gameObject))) {
                return true;
            }
        } else if (onlyClickedCharacter && UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) {
            return true;
        }
        return false;
    }
    public bool ShouldShowNotificationFrom(Character character, Log log) {
#if TRAILER_BUILD
        if (character.name == "Fiona" || character.name == "Jamie" || character.name == "Audrey") {
            return true;
        }
        return false;
#endif
        if (ShouldShowNotificationFrom(character)) {
            return true;
        } else {
            return ShouldShowNotificationFrom(log.fillers.Where(x => x.obj is Character).Select(x => x.obj as Character).ToArray());
        }
    }
    private bool ShouldShowNotificationFrom(Character[] characters) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i])) {
                return true;
            }
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Character[] characters, Log log) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i], log)) {
                return true;
            }
        }
        return false;
    }
    public bool ShowNotificationFrom(Character character, Log log) {
        if (ShouldShowNotificationFrom(character, log)) {
            ShowNotification(log);
            return true;
        }
        return false;
    }
    public void ShowNotificationFrom(Log log, params Character[] characters) {
        if (ShouldShowNotificationFrom(characters, log)) {
            ShowNotification(log);
        }
    }
    public void ShowNotificationFrom(Log log, Character character, bool onlyClickedCharacter) {
        if (ShouldShowNotificationFrom(character, onlyClickedCharacter)) {
            ShowNotification(log);
        }
    }
    public void ShowNotification(Log log) {
        Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, log);
    }
    #endregion

    #region Tile Corruption
    public void InvadeATile() {
        //currentCorruptionDuration = currentTileBeingCorrupted.corruptDuration;
        //if(currentCorruptionDuration == 0) {
        //    Debug.LogError("Cannot corrupt a tile with 0 corruption duration");
        //} else {
        //    GameManager.Instance.SetOnlyTickDays(true);
        //    currentTileBeingCorrupted.StartCorruptionAnimation();
        //    currentCorruptionTick = 0;
        //    Messenger.AddListener(Signals.DAY_STARTED, CorruptTilePerTick);
        //    UIManager.Instance.Unpause();
        //    isTileCurrentlyBeingCorrupted = true;
        //}
        currentTileBeingCorrupted.region.InvadeActions();
        //TODO:
        // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, currentTileBeingCorrupted.region);
        //PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
    }
    private void CorruptTilePerTick() {
        currentCorruptionTick ++;
        if(currentCorruptionTick >= currentCorruptionDuration) {
            TileIsCorrupted();
        }
    }
    private void TileIsCorrupted() {
        isTileCurrentlyBeingCorrupted = false;
        Messenger.RemoveListener(Signals.DAY_STARTED, CorruptTilePerTick);
        UIManager.Instance.Pause();
        //TODO:
        // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, currentTileBeingCorrupted.region);
        //PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
    }
    #endregion

    #region Settlement Corruption
    private Settlement AreaIsCorrupted() {
        //TODO:
        // isTileCurrentlyBeingCorrupted = false;
        // GameManager.Instance.SetPausedState(true);
        // Settlement corruptedSettlement = currentTileBeingCorrupted.settlementOfTile;
        // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, RACE.DEMON, currentTileBeingCorrupted.region);
        // //PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
        // return corruptedSettlement;
        return null;
    }
    #endregion

    #region Summons
    //private void ConstructAllSummonSlots() {
    //    for (int i = 0; i < summonSlots.Length; i++) {
    //        if (summonSlots[i] == null) {
    //            summonSlots[i] = new SummonSlot();
    //        }
    //    }
    //}
    private void GainSummonSlot(bool showUI = true) {
        SummonSlot newSlot = new SummonSlot();
        summonSlots.Add(newSlot);
        //if (showUI) {
        //    PlayerUI.Instance.ShowGeneralConfirmation("New Summon Slot", "You gained a new summon slot!");
        //}
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "You gained a summon slot!", null);
        Messenger.Broadcast<SummonSlot>(Signals.PLAYER_GAINED_SUMMON_SLOT, newSlot);
    }
    private void LoseSummonSlot() {
        SummonSlot unusedSlot;
        if (TryGetUnusedSummonSlot(out unusedSlot)) {
            //lose the unused slot.
            LoseSummonSlot(unusedSlot, true);
        } else {
            //no unused slot, show UI to pick slot to be discarded.
            UIManager.Instance.ShowClickableObjectPicker(summonSlots, ShowDiscardSummonConfirmation, title: "Discard a summon slot.", showCover: true, layer: 25, closable: false);
        }
    }
    private void LoseSummonSlot(SummonSlot slot, bool showUI = false) {
        if (summonSlots.Remove(slot)) {
            //PlayerUI.Instance.ShowGeneralConfirmation("Lost Summon Slot", "You lost a summon slot!");
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "You lost a summon slot!", null);
            if (slot.summon != null) {
                ClearSummonData(slot.summon);
            }
            Messenger.Broadcast(Signals.PLAYER_LOST_SUMMON_SLOT, slot);
        }
    }
    private void ShowDiscardSummonConfirmation(object s) {
        SummonSlot slot = s as SummonSlot;
        UIManager.Instance.ShowYesNoConfirmation("Discard summon slot", "Are you sure you want to discard your " + slot.summon.summonType.SummonName() + " summon?", () => OnClickYesDiscardSummon(slot), layer: 26);
    }
    private void OnClickYesDiscardSummon(SummonSlot slot) {
        LoseSummonSlot(slot);
        UIManager.Instance.HideObjectPicker();
    }
    public void GainSummon(SUMMON_TYPE type, int level = 1, bool showNewSummonUI = false) {
        Faction faction = playerFaction;
        if (type == SUMMON_TYPE.Incubus || type == SUMMON_TYPE.Succubus) {
            faction = FactionManager.Instance.neutralFaction;
        }
        Summon newSummon = CharacterManager.Instance.CreateNewSummon(type, faction, playerSettlement);
        newSummon.SetLevel(level);
        GainSummon(newSummon, showNewSummonUI);
    }
    public void GainSummon(Summon summon, bool showNewSummonUI = false) {
        if (maxSummonSlots == 0) {
            //no summon slots yet
            PlayerUI.Instance.ShowGeneralConfirmation("New Summon", "You gained a new summon but do not yet have a summon slot! " + summon.summonType.SummonName() + " will be discarded.");
            RejectSummon(summon);
        } else if (GetTotalSummonsCount() < maxSummonSlots) {
            AddSummon(summon, showNewSummonUI);
        } else {
            Debug.LogWarning("Max summons has been reached!");
            PlayerUI.Instance.replaceUI.ShowReplaceUI(GetAllSummons(), summon, ReplaceSummon, RejectSummon);
        }
    }
    public bool HasSpaceForNewSummon() {
        return GetTotalSummonsCount() < maxSummonSlots; //if the total summons count is less than the summon slots
    }
    private void ReplaceSummon(object summonToReplace, object summonToAdd) {
        Summon replace = summonToReplace as Summon;
        Summon add = summonToAdd as Summon;
        RemoveSummon(replace);
        AddSummon(add);
    }
    private void RejectSummon(object rejectedSummon) {
        ClearSummonData(rejectedSummon as Summon);
    }
    /// <summary>
    /// Get total number of summons that the player has, regardless of them having been used or not.
    /// </summary>
    /// <returns></returns>
    public int GetTotalSummonsCount() {
        int count = 0;
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i].summon != null) {
                count++;
            }
        }
        return count;
    }
    private void AddSummon(Summon newSummon, bool showNewSummonUI = false) {
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i].summon == null) {
                summonSlots[i].SetSummon(newSummon);
                playerSettlement.AddResident(newSummon, ignoreCapacity:true);
                Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON, newSummon);
                if (showNewSummonUI) {
                    PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(currentMinionLeader, newSummon);
                }
                break;
            }
        }
    }
    /// <summary>
    /// Remove summon from the players list of available summons.
    /// NOTE: Summons will be placed back on the list when the player is done with a map.
    /// </summary>
    /// <param name="summon">The summon to be removed.</param>
    public void RemoveSummon(Summon summon) {
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i].summon == summon) {
                summonSlots[i].summon = null;
                Messenger.Broadcast(Signals.PLAYER_REMOVED_SUMMON, summon);
                break;
            }
        }
    }
    public void RemoveSummon(SUMMON_TYPE summon) {
        Summon chosenSummon = GetAvailableSummonOfType(summon);
        if(chosenSummon != null) {
            RemoveSummon(chosenSummon);
        }
    }
    public string GetSummonDescription(SUMMON_TYPE currentlySelectedSummon) {
        switch (currentlySelectedSummon) {
            case SUMMON_TYPE.Wolf:
                return "Summon a wolf to run amok.";
            case SUMMON_TYPE.Skeleton:
                return "Summon a skeleton that will abduct a random character.";
            case SUMMON_TYPE.Golem:
                return "Summon a stone golem that can sustain alot of hits.";
            case SUMMON_TYPE.Succubus:
                return "Summon a succubus that will seduce a male character and eliminate him.";
            case SUMMON_TYPE.Incubus:
                return "Summon a succubus that will seduce a female character and eliminate her.";
            case SUMMON_TYPE.ThiefSummon:
                return "Summon a thief that will steal items from the settlements warehouse.";
            default:
                return "Summon a " + Utilities.NormalizeStringUpperCaseFirstLetters(currentlySelectedSummon.ToString());
        }
    }
    private void ClearSummonData(Summon summon) {
        PlayerManager.Instance.player.playerFaction.LeaveFaction(summon);
        PlayerManager.Instance.player.playerSettlement.region.RemoveCharacterFromLocation(summon);
        PlayerManager.Instance.player.playerSettlement.region.RemoveResident(summon);
        CharacterManager.Instance.RemoveCharacter(summon);
    }
    public Summon GetAvailableSummonOfType(SUMMON_TYPE type) {
        List<SummonSlot> choices = summonSlots.Where(x => x.summon != null && !x.summon.hasBeenUsed && x.summon.summonType == type).ToList();
        return choices[Random.Range(0, choices.Count)].summon;
    }
    public bool HasSummonOfType(SUMMON_TYPE summonType) {
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i].summon != null && summonSlots[i].summon.summonType == summonType) {
                return true;
            }
        }
        return false;
    }
    public bool HasAnySummon(params string[] summonName) {
        SUMMON_TYPE type;
        for (int i = 0; i < summonName.Length; i++) {
            string currName = summonName[i];
            if (System.Enum.TryParse(currName, out type)) {
                return HasSummonOfType(type);
            }
        }
        return false;
    }
    public List<Summon> GetAllSummons() {
        List<Summon> all = new List<Summon>();
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i].summon != null) {
                all.Add(summonSlots[i].summon);
            }
        }
        return all;
    }
    public Summon GetRandomSummon() {
        List<Summon> all = GetAllSummons();
        return all[UnityEngine.Random.Range(0, all.Count)];
    }
    private void ResetSummons() {
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i].summon != null) {
                summonSlots[i].summon.Reset();
            }
        }
    }
    public void IncreaseSummonSlot() {
        maxSummonSlots += 1;
        maxSummonSlots = Mathf.Max(maxSummonSlots, 0);
        //validate if adjusted max summons can accomodate current summons
        if (summonSlots.Count < maxSummonSlots) {
            //add new summon slot
            GainSummonSlot();
        }
    }
    public void DecreaseSummonSlot() {
        maxSummonSlots -= 1;
        maxSummonSlots = Mathf.Max(maxSummonSlots, 0);
        //validate if adjusted max summons can accomodate current summons
        if (summonSlots.Count > maxSummonSlots) {
            //remove summon slot
            LoseSummonSlot();
        }
    }
    public SummonSlot GetSummonSlotBySummon(Summon summon) {
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i].summon == summon) {
                return summonSlots[i];
            }
        }
        return null;
    }
    public int GetIndexForSummonSlot(SummonSlot slot) {
        for (int i = 0; i < summonSlots.Count; i++) {
            if (summonSlots[i] == slot) {
                return i;
            }
        }
        return 0;
    }
    public bool AreAllSummonSlotsMaxLevel() {
        for (int i = 0; i < maxSummonSlots; i++) {
            if (summonSlots[i].level < PlayerManager.MAX_LEVEL_SUMMON) {
                return false;
            }
        }
        return true;
    }
    private bool TryGetUnusedSummonSlot(out SummonSlot unusedSlot) {
        for (int i = 0; i < summonSlots.Count; i++) {
            SummonSlot currSlot = summonSlots[i];
            if (currSlot.summon == null) {
                unusedSlot = currSlot;
                return true;
            }
        }
        unusedSlot = null; //no unused slot
        return false;
    }
    #endregion

    #region Artifacts
    //private void ConstructAllArtifactSlots() {
    //    for (int i = 0; i < artifactSlots.Length; i++) {
    //        if(artifactSlots[i] == null) {
    //            artifactSlots[i] = new ArtifactSlot();
    //        }
    //    }
    //}
    private void GainArtifactSlot(bool showUI = true) {
        ArtifactSlot newSlot = new ArtifactSlot();
        artifactSlots.Add(newSlot);
        //if (showUI) {
        //    PlayerUI.Instance.ShowGeneralConfirmation("New Artifact Slot", "You gained a new artifact slot!");
        //}
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "You gained a new artifact slot!", null);
        Messenger.Broadcast<ArtifactSlot>(Signals.PLAYER_GAINED_ARTIFACT_SLOT, newSlot);
    }
    private void LoseArtifactSlot() {
        ArtifactSlot unusedSlot;
        if (TryGetUnusedArtifactSlot(out unusedSlot)) {
            //lose the unused slot.
            LoseArtifactSlot(unusedSlot, true);
        } else {
            //no unused slot, show UI to pick slot to be discarded.
            UIManager.Instance.ShowClickableObjectPicker(artifactSlots, ShowDiscardArtifactConfirmation, title: "Discard an artifact slot.", showCover: true, layer: 25, closable: false);
        }
    }
    private void LoseArtifactSlot(ArtifactSlot slot, bool showUI = false) {
        if (artifactSlots.Remove(slot)) {
            PlayerUI.Instance.ShowGeneralConfirmation("Lost Artifact Slot", "You lost an artifact slot!");
            Messenger.Broadcast(Signals.PLAYER_LOST_ARTIFACT_SLOT, slot);
        }
    }
    private void ShowDiscardArtifactConfirmation(object s) {
        ArtifactSlot slot = s as ArtifactSlot;
        UIManager.Instance.ShowYesNoConfirmation("Discard artifact slot", "Are you sure you want to discard your " + slot.artifact.name + "?", () => OnClickYesDiscardArtifact(slot), layer: 26);
    }
    private void OnClickYesDiscardArtifact(ArtifactSlot slot) {
        LoseArtifactSlot(slot);
        UIManager.Instance.HideObjectPicker();
    }
    public void GainArtifact(ARTIFACT_TYPE type, bool showNewArtifactUI = false) {
        Artifact newArtifact = PlayerManager.Instance.CreateNewArtifact(type);
        GainArtifact(newArtifact, showNewArtifactUI);
    }
    public void GainArtifact(Artifact artifact, bool showNewArtifactUI = false) {
        if (maxArtifactSlots == 0) {
            //no artifact slots yet.
            PlayerUI.Instance.ShowGeneralConfirmation("New Artifact", "You gained a new artifact but do not yet have an artifact slot! " + artifact.name + " will be discarded.");
        } else if (GetTotalArtifactCount() < maxArtifactSlots) {
            AddArtifact(artifact, showNewArtifactUI);
        } else {
            Debug.LogWarning("Max artifacts has been reached!");
            PlayerUI.Instance.replaceUI.ShowReplaceUI(GetAllArtifacts(), artifact, ReplaceArtifact, RejectArtifact);
        }
    }
    public bool HasSpaceForNewArtifact() {
        return GetTotalArtifactCount() < maxArtifactSlots;
    }
    private void ReplaceArtifact(object objToReplace, object objToAdd) {
        Artifact replace = objToReplace as Artifact;
        Artifact add = objToAdd as Artifact;
        RemoveArtifact(replace);
        AddArtifact(add);
    }
    private void RejectArtifact(object rejectedObj) { }
    public void LoseArtifact(ARTIFACT_TYPE type) {
        if (GetAvailableArtifactsOfTypeCount(type) > 0) {
            Artifact artifact = GetArtifactOfType(type);
            RemoveArtifact(artifact);
        } else {
            Debug.LogWarning("Cannot lose artifact " + type.ToString() + " because player has none.");
        }
    }
    private void AddArtifact(Artifact newArtifact, bool showNewArtifactUI = false) {
        for (int i = 0; i < artifactSlots.Count; i++) {
            if (artifactSlots[i].artifact == null) {
                artifactSlots[i].SetArtifact(newArtifact);
                Messenger.Broadcast<Artifact>(Signals.PLAYER_GAINED_ARTIFACT, newArtifact);
                if (showNewArtifactUI) {
                    PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(currentMinionLeader, newArtifact);
                }
                break;
            }
        }
    }
    public void RemoveArtifact(Artifact removedArtifact) {
        for (int i = 0; i < artifactSlots.Count; i++) {
            if (artifactSlots[i].artifact == removedArtifact) {
                artifactSlots[i].artifact = null;
                Messenger.Broadcast<Artifact>(Signals.PLAYER_REMOVED_ARTIFACT, removedArtifact);
                break;
            }
        }
    }
    public int GetTotalArtifactCount() {
        int count = 0;
        for (int i = 0; i < artifactSlots.Count; i++) {
            if (artifactSlots[i].artifact != null) {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// Get number of artifacts that have not been used.
    /// </summary>
    public int GetTotalAvailableArtifactCount() {
        int count = 0;
        for (int i = 0; i < artifactSlots.Count; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && !currArtifact.hasBeenUsed) {
                count++;
            }
        }
        return count;
    }
    public int GetAvailableArtifactsOfTypeCount(ARTIFACT_TYPE type) {
        int count = 0;
        for (int i = 0; i < artifactSlots.Count; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && currArtifact.type == type && !currArtifact.hasBeenUsed) {
                count++;
            }
        }
        return count;
    }
    public bool TryGetAvailableArtifactOfType(ARTIFACT_TYPE type, out Artifact artifact) {
        for (int i = 0; i < artifactSlots.Count; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && currArtifact.type == type && !currArtifact.hasBeenUsed) {
                artifact = currArtifact;
                return true;
            }
        }
        artifact = null;
        return false;
    }
    public bool HasArtifact(string artifactName) {
        //ARTIFACT_TYPE type = (ARTIFACT_TYPE)System.Enum.Parse(typeof(ARTIFACT_TYPE), artifactName);
        for (int i = 0; i < artifactSlots.Count; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && currArtifact.name == artifactName) {
                return true;
            }
        }
        return false;
    }
    private Artifact GetArtifactOfType(ARTIFACT_TYPE type) {
        for (int i = 0; i < artifactSlots.Count; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact.type == type) {
                return currArtifact;
            }
        }
        return null;
    }
    private List<Artifact> GetAllArtifacts() {
        List<Artifact> all = new List<Artifact>();
        for (int i = 0; i < artifactSlots.Count; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null) {
                all.Add(currArtifact);
            }
        }
        return all;
    }
    public string GetArtifactDescription(ARTIFACT_TYPE type) {
        switch (type) {
            case ARTIFACT_TYPE.Necronomicon:
                return "Raises all dead characters in the settlement to attack residents.";
            case ARTIFACT_TYPE.Chaos_Orb_Artifact:
                return "Characters that inspect the Chaos Orb may be permanently berserked.";
            case ARTIFACT_TYPE.Hermes_Statue:
                return "Characters that inspect this will be teleported to a different settlement. If no other settlement exists, this will be useless.";
            case ARTIFACT_TYPE.Ankh_Of_Anubis:
                return "All characters that moves through here may slowly sink and perish. Higher agility means higher chance of escaping. Sand pit has a limited duration upon placing the artifact.";
            case ARTIFACT_TYPE.Miasma_Emitter:
                return "Characters will avoid the settlement. If any character gets caught within, they will gain Poisoned status effect. Any objects inside the radius are disabled.";
            default:
                return "Summon a " + Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
        }
    }
    public Artifact GetRandomArtifact() {
        List<Artifact> choices = GetAllArtifacts();
        return choices[UnityEngine.Random.Range(0, choices.Count)];
    }
    private void ResetArtifacts() {
        for (int i = 0; i < artifactSlots.Count; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null) {
                currArtifact.Reset();
            }
        }
    }
    public void IncreaseArtifactSlot() {
        maxArtifactSlots += 1;
        maxArtifactSlots = Mathf.Max(maxArtifactSlots, 0);
        //validate if adjusted max artifacts can accomodate current summons
        if (artifactSlots.Count < maxArtifactSlots) {
            //add new artifact slot
            GainArtifactSlot();
        }
    }
    public void DecreaseArtifactSlot() {
        maxArtifactSlots -= 1;
        maxArtifactSlots = Mathf.Max(maxArtifactSlots, 0);
        //validate if adjusted max artifacts can accomodate current summons
        if (artifactSlots.Count > maxArtifactSlots) {
            //remove artifact slot
            LoseArtifactSlot();
        }
    }
    public ArtifactSlot GetArtifactSlotByArtifact(Artifact artifact) {
        for (int i = 0; i < artifactSlots.Count; i++) {
            if(artifactSlots[i].artifact == artifact) {
                return artifactSlots[i];
            }
        }
        return null;
    }
    public int GetIndexForArtifactSlot(ArtifactSlot slot) {
        for (int i = 0; i < artifactSlots.Count; i++) {
            if (artifactSlots[i] == slot) {
                return i;
            }
        }
        return 0;
    }
    public bool AreAllArtifactSlotsMaxLevel() {
        for (int i = 0; i < maxArtifactSlots; i++) {
            if (artifactSlots[i].level < PlayerManager.MAX_LEVEL_ARTIFACT) {
                return false;
            }
        }
        return true;
    }
    private bool TryGetUnusedArtifactSlot(out ArtifactSlot unusedSlot) {
        for (int i = 0; i < artifactSlots.Count; i++) {
            ArtifactSlot currSlot = artifactSlots[i];
            if (currSlot.artifact == null) {
                unusedSlot = currSlot;
                return true;
            }
        }
        unusedSlot = null; //no unused slot
        return false;
    }
    #endregion

    #region Invasion
    public void StartInvasion(Settlement settlement) {
        List<LocationGridTile> entrances = new List<LocationGridTile>();
        List<Minion> currentMinions = new List<Minion>();
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion.assignedRegion == null) { //only include minions that are not currently invading another landmark
                currMinion.character.CreateMarker();
                currMinion.character.marker.SetActiveState(false);
                currentMinions.Add(currMinion);
            }
        }

        if(currentMinions.Count > 0) {
            LocationGridTile mainEntrance = settlement.innerMap.GetRandomUnoccupiedEdgeTile();
            entrances.Add(mainEntrance);
            //int neededEntrances = currentMinions.Count - 1;

            for (int i = 0; i < entrances.Count; i++) {
                if (entrances.Count == currentMinions.Count) {
                    break;
                }
                for (int j = 0; j < entrances[i].neighbourList.Count; j++) {
                    LocationGridTile newEntrance = entrances[i].neighbourList[j];
                    //if (newEntrance.objHere == null && newEntrance.charactersHere.Count == 0 && newEntrance.structure != null) {
                    if (newEntrance.IsAtEdgeOfWalkableMap() && !entrances.Contains(newEntrance)) {
                        entrances.Add(newEntrance);
                        if (entrances.Count == currentMinions.Count) {
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < entrances.Count; i++) {
                currentMinions[i].character.marker.InitialPlaceMarkerAt(entrances[i]);
            }
            for (int i = 0; i < currentMinions.Count; i++) {
                Minion currMinion = currentMinions[i];
                if (!currMinion.character.marker.gameObject.activeInHierarchy) {
                    throw new System.Exception(currMinion.character.name + " was not placed!");
                }
                currMinion.StartInvasionProtocol(settlement);
            }
            //PlayerUI.Instance.startInvasionButton.interactable = false;
            currentSettlementBeingInvaded = settlement;
            currentMinions[0].character.CenterOnCharacter();
            Messenger.AddListener(Signals.TICK_ENDED, PerTickInvasion);
        }
        //else {
        //    Debug.LogError("Can't invade! No more minions!");
        //}
    }
    private void PerTickInvasion() {
        bool stillHasMinions = false;
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion.assignedRegion != currentSettlementBeingInvaded.region) {
                continue; //do not include minions that are not invading the main settlement.
            }
            if(currMinion.character.currentHP > 0 && !currMinion.character.isDead && currMinion.character.canPerform) {
                stillHasMinions = true;
                break;
            }
        }
        if (!stillHasMinions) {
            StopInvasion(false);
            return;
        }

        bool stillHasResidents = false;
        for (int i = 0; i < currentSettlementBeingInvaded.region.residents.Count; i++) { //Changed checking to faction members, because some characters may still consider the settlement as their home, but are no longer part of the faction
            Character currCharacter = currentSettlementBeingInvaded.region.residents[i];
            if (currCharacter.currentSettlement == currentSettlementBeingInvaded && currCharacter.IsAble()) {
                stillHasResidents = true;
                break;
            }
        }
        //for (int i = 0; i < currentSettlementBeingInvaded.areaResidents.Count; i++) {
        //    Character currCharacter = currentSettlementBeingInvaded.areaResidents[i];
        //    if (currCharacter.IsAble() && currCharacter.specificLocation == currentSettlementBeingInvaded) {
        //        stillHasResidents = true;
        //        break;
        //    }
        //}
        if (!stillHasResidents) {
            StopInvasion(true);
            return;
        }
    }
    private void StopInvasion(bool playerWon) {
        //TODO:
        // Messenger.RemoveListener(Signals.TICK_ENDED, PerTickInvasion);
        // if (playerWon) {
        //     PlayerUI.Instance.SuccessfulAreaCorruption();
        //     Settlement corruptedSettlement = AreaIsCorrupted();
        //     ResetThreat();
        //     for (int i = 0; i < corruptedSettlement.charactersAtLocation.Count; i++) {
        //         corruptedSettlement.charactersAtLocation[i].combatComponent.ClearAvoidInRange(false);
        //         corruptedSettlement.charactersAtLocation[i].combatComponent.ClearHostilesInRange(false);
        //         corruptedSettlement.charactersAtLocation[i].marker.ClearPOIsInVisionRange();
        //         corruptedSettlement.charactersAtLocation[i].marker.ClearTerrifyingObjects();
        //     }
        //     Messenger.Broadcast(Signals.SUCCESS_INVASION_AREA, corruptedSettlement);
        //     //ResetSummons();
        //     //ResetArtifacts();
        //     //LevelUpAllMinions();
        // } else {
        //     HexTile tile = currentSettlementBeingInvaded.coreTile;
        //     UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "You failed to invade " + currentSettlementBeingInvaded.name + ".", () => UIManager.Instance.ShowRegionInfo(tile.region));
        // }
        // for (int i = 0; i < minions.Count; i++) {
        //     Minion currMinion = minions[i];
        //     currMinion.StopInvasionProtocol(currentSettlementBeingInvaded);
        // }
        // currentSettlementBeingInvaded = null;
        // UIManager.Instance.regionInfoUI.StopSettlementInvasion();
    }
    //public void SetInvadingRegion(Region region) {
    //    invadingRegion = region;
    //}
    public void SetConstructionRatePercentageModifier(float amount) {
        constructionRatePercentageModifier = amount;
    }
    //public void AdjustInvasionRatePercentageModifier(float amount) {
    //    invasionRatePercentageModifier += amount;
    //}
    #endregion

    #region Combat Ability
    public void SetCurrentActiveCombatAbility(CombatAbility ability) {
        if(currentActiveCombatAbility == ability) {
            //Do not process when setting the same combat ability
            return;
        }
        CombatAbility previousAbility = currentActiveCombatAbility;
        currentActiveCombatAbility = ability;
        if (currentActiveCombatAbility == null) {
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
            CombatAbilityButton abilityButton = PlayerUI.Instance.GetCombatAbilityButton(previousAbility);
            abilityButton?.UpdateInteractableState();
            InnerMapManager.Instance.UnhighlightTiles();
            CursorManager.Instance.ClearLeftClickActions();
            CursorManager.Instance.ClearRightClickActions();
            //GameManager.Instance.SetPausedState(false);
        } else {
            CombatAbilityButton abilityButton = PlayerUI.Instance.GetCombatAbilityButton(ability);
            //change the cursor
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Cross);
            CursorManager.Instance.AddLeftClickAction(TryExecuteCurrentActiveCombatAbility);
            CursorManager.Instance.AddLeftClickAction(() => SetCurrentActiveCombatAbility(null));
            CursorManager.Instance.AddRightClickAction(() => SetCurrentActiveCombatAbility(null));
            abilityButton?.UpdateInteractableState();
            //GameManager.Instance.SetPausedState(true);
        }
    }
    private void TryExecuteCurrentActiveCombatAbility() {
        //string summary = "Mouse was clicked. Will try to execute " + currentActiveCombatAbility.name;
        if (currentActiveCombatAbility.abilityRadius == 0) {
            if (currentActiveCombatAbility.CanTarget(InnerMapManager.Instance.currentlyHoveredPoi)) {
                currentActiveCombatAbility.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
            }
        } else {
            List<LocationGridTile> highlightedTiles = InnerMapManager.Instance.currentlyHighlightedTiles;
            if (highlightedTiles != null) {
                List<IPointOfInterest> poisInHighlightedTiles = new List<IPointOfInterest>();
                for (int i = 0; i < InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation.Count; i++) {
                    Character currCharacter = InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation[i];
                    if (highlightedTiles.Contains(currCharacter.gridTileLocation)) {
                        poisInHighlightedTiles.Add(currCharacter);
                    }
                }
                for (int i = 0; i < highlightedTiles.Count; i++) {
                    if(highlightedTiles[i].objHere != null) {
                        poisInHighlightedTiles.Add(highlightedTiles[i].objHere);
                    }
                }
                currentActiveCombatAbility.ActivateAbility(poisInHighlightedTiles);
            }
        }
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region Intervention Ability
    private void ConstructAllInterventionAbilitySlots() {
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i] == null) {
                interventionAbilitySlots[i] = new PlayerJobActionSlot();
            }
        }
    }
    public void GainNewInterventionAbility(SPELL_TYPE ability, bool showNewAbilityUI = false) {
        PlayerSpell playerSpell = PlayerManager.Instance.CreateNewInterventionAbility(ability);
        GainNewInterventionAbility(playerSpell, showNewAbilityUI);
    }
    public void GainNewInterventionAbility(PlayerSpell ability, bool showNewAbilityUI = false) {
        if (!HasEmptyInterventionSlot()) {
            PlayerUI.Instance.replaceUI.ShowReplaceUI(GetAllInterventionAbilities(), ability, OnReplaceInterventionAbility, OnRejectInterventionAbility);
        } else {
            for (int i = 0; i < interventionAbilitySlots.Length; i++) {
                if (interventionAbilitySlots[i].ability == null) {
                    interventionAbilitySlots[i].SetAbility(ability);
                    Messenger.Broadcast(Signals.PLAYER_LEARNED_INTERVENE_ABILITY, ability);
                    if (showNewAbilityUI) {
                        PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(null, ability);
                    }
                    break;
                }
            }
        }
    }
    public void ConsumeAbility(PlayerSpell ability) {
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].ability == ability) {
                interventionAbilitySlots[i].SetAbility(null);
                Messenger.Broadcast(Signals.PLAYER_CONSUMED_INTERVENE_ABILITY, ability);
                break;
            }
        }
    }
    private void OnReplaceInterventionAbility(object objToReplace, object objToAdd) {
        PlayerSpell replace = objToReplace as PlayerSpell;
        PlayerSpell add = objToAdd as PlayerSpell;
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].ability == replace) {
                interventionAbilitySlots[i].SetAbility(add);
                Messenger.Broadcast(Signals.PLAYER_LEARNED_INTERVENE_ABILITY, add);
                break;
            }
        }
    }
    private void OnRejectInterventionAbility(object rejectedObj) { }
    private int GetInterventionAbilityCount() {
        int count = 0;
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].ability != null) {
                count++;
            }
        }
        return count;
    }
    public bool HasEmptyInterventionSlot() {
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].ability == null) {
                return true;
            }
        }
        return false;
    }
    public List<PlayerSpell> GetAllInterventionAbilities() {
        List<PlayerSpell> abilities = new List<PlayerSpell>();
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].ability != null) {
                abilities.Add(interventionAbilitySlots[i].ability);
            }
        }
        return abilities;
    }
    public void ResetInterventionAbilitiesCD() {
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].ability != null) {
                interventionAbilitySlots[i].ability.InstantCooldown();
            }
        }
    }
    public bool AreAllInterventionSlotsMaxLevel() {
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].level < PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY) {
                return false;
            }
        }
        return true;
    }
    //public void LoadResearchNewInterventionAbility(SaveDataPlayer data) {
    //    if(data.interventionAbilityToResearch != INTERVENTION_ABILITY.NONE) {
    //        currentNewInterventionAbilityCycleIndex = data.currentNewInterventionAbilityCycleIndex;
    //        currentInterventionAbilityTimerTick = data.currentInterventionAbilityTimerTick;
    //        NewCycleForNewInterventionAbility(data.interventionAbilityToResearch);
    //    } else {
    //        StartResearchNewInterventionAbility();
    //    }
    //}
    //private void InitializeNewInterventionAbilityCycle() {
    //    currentNewInterventionAbilityCycleIndex = -1;
    //    newInterventionAbilityTimerTicks = GameManager.Instance.GetTicksBasedOnHour(8);
    //    interventionAbilityToResearch = INTERVENTION_ABILITY.NONE;
    //}
    //public void StartResearchNewInterventionAbility() {
    //    currentInterventionAbilityTimerTick = 0;
    //    currentNewInterventionAbilityCycleIndex++;
    //    if (currentNewInterventionAbilityCycleIndex > 3) {
    //        currentNewInterventionAbilityCycleIndex = 0;
    //    }

    //    int tier = GetTierBasedOnCycle();
    //    List<INTERVENTION_ABILITY> abilities = PlayerManager.Instance.GetAbilitiesByTier(tier);

    //    int index1 = UnityEngine.Random.Range(0, abilities.Count);
    //    INTERVENTION_ABILITY ability1 = abilities[index1];
    //    abilities.RemoveAt(index1);

    //    if (abilities.Count <= 0) {
    //        abilities = PlayerManager.Instance.GetAbilitiesByTier(tier);
    //    }
    //    int index2 = UnityEngine.Random.Range(0, abilities.Count);
    //    INTERVENTION_ABILITY ability2 = abilities[index2];
    //    abilities.RemoveAt(index2);

    //    if (abilities.Count <= 0) {
    //        abilities = PlayerManager.Instance.GetAbilitiesByTier(tier);
    //    }
    //    int index3 = UnityEngine.Random.Range(0, abilities.Count);
    //    INTERVENTION_ABILITY ability3 = abilities[index3];

    //    PlayerUI.Instance.researchInterventionAbilityUI.SetAbility1(ability1);
    //    PlayerUI.Instance.researchInterventionAbilityUI.SetAbility2(ability2);
    //    PlayerUI.Instance.researchInterventionAbilityUI.SetAbility3(ability3);
    //    PlayerUI.Instance.researchInterventionAbilityUI.ShowResearchUI();
    //}
    //private void PerTickInterventionAbility() {
    //    currentInterventionAbilityTimerTick++;
    //    if (currentInterventionAbilityTimerTick >= newInterventionAbilityTimerTicks) {
    //        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInterventionAbility);
    //        GainNewInterventionAbility(interventionAbilityToResearch, true);
    //        StartResearchNewInterventionAbility();
    //    }
    //}
    //public void NewCycleForNewInterventionAbility(INTERVENTION_ABILITY interventionAbilityToResearch) {
    //    if (!isNotFirstResearch) {
    //        isNotFirstResearch = true;
    //    }
    //    this.interventionAbilityToResearch = interventionAbilityToResearch;
    //    TimerHubUI.Instance.AddItem("Research for " + Utilities.NormalizeStringUpperCaseFirstLetters(interventionAbilityToResearch.ToString()), newInterventionAbilityTimerTicks - currentInterventionAbilityTimerTick, null);
    //    Messenger.AddListener(Signals.TICK_STARTED, PerTickInterventionAbility);
    //}
    //private int GetTierBasedOnCycle() {
    //    //Tier Cycle - 3, 3, 2, 1
    //    if (currentNewInterventionAbilityCycleIndex == 0) return 3;
    //    else if (currentNewInterventionAbilityCycleIndex == 1) return 3;
    //    else if (currentNewInterventionAbilityCycleIndex == 2) return 2;
    //    else if (currentNewInterventionAbilityCycleIndex == 3) return 1;
    //    return 3;
    //}
    #endregion

    #region The Eye
    private void OnMinionAssignedToPlayerLandmark(Minion minion, BaseLandmark landmark) { }
    private void OnMinionUnassignedFromPlayerLandmark(Minion minion, BaseLandmark landmark) { }
    #endregion

    #region Mana
    public void AdjustMana(int amount) {
        mana += amount;
        mana = Mathf.Clamp(mana, 0, EditableValuesManager.Instance.maximumMana);
        Messenger.Broadcast(Signals.PLAYER_ADJUSTED_MANA);
    }
    public int GetManaCostForInterventionAbility(SPELL_TYPE ability) {
        int tier = PlayerManager.Instance.GetSpellTier(ability);
        return PlayerManager.Instance.GetManaCostForSpell(tier);
    }
    #endregion
}

[System.Serializable]
public struct DemonicLandmarkBuildingData {
    public LANDMARK_TYPE landmarkType;
    public string landmarkName;
    public int buildDuration;
    public int currentDuration;
}

[System.Serializable]
public struct DemonicLandmarkInvasionData {
    public bool beingInvaded;
    public int currentDuration;
}