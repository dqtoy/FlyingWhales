using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

public class Player : ILeader {

    private const int MAX_INTEL = 3;
    public const int MAX_MINIONS = 5;
    public const int MAX_THREAT = 100;
    private const int MAX_SUMMONS = 5;

    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int threat { get; private set; }

    //public Dictionary<JOB, PlayerJobData> roleSlots { get; private set; }
    public CombatGrid attackGrid { get; private set; }
    public CombatGrid defenseGrid { get; private set; }
    public List<Intel> allIntel { get; private set; }
    public Minion[] minions { get; private set; }
    public Dictionary<SUMMON_TYPE, List<Summon>> summons { get; private set; }

    //Unique ability of player
    public ShareIntel shareIntelAbility { get; private set; }

    public int currentCorruptionDuration { get; private set; }
    public int currentCorruptionTick { get; private set; }
    public bool isTileCurrentlyBeingCorrupted { get; private set; }
    public HexTile currentTileBeingCorrupted { get; private set; }

    #region getters/setters
    public int id {
        get { return -645; }
    }
    public string name {
        get { return "Player"; }
    }
    public RACE race {
        get { return RACE.HUMANS; }
    }
    public Area specificLocation {
        get { return playerArea; }
    }
    public Area homeArea {
        get { return playerArea; }
    }
    public List<Character> allOwnedCharacters {
        get { return minions.Select(x => x.character).ToList(); }
    }
    #endregion

    public bool hasSeenActionButtonsOnce = false;

    public Player() {
        playerArea = null;
        attackGrid = new CombatGrid();
        defenseGrid = new CombatGrid();
        attackGrid.Initialize();
        defenseGrid.Initialize();
        allIntel = new List<Intel>();
        minions = new Minion[MAX_MINIONS];
        summons = new Dictionary<SUMMON_TYPE, List<Summon>>();
        shareIntelAbility = new ShareIntel();
        //ConstructRoleSlots();
        AddListeners();
    }

    #region Listeners
    private void AddListeners() {
        AddWinListener();
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, OnTileRemovedFromPlayerArea);
        Messenger.AddListener(Signals.TICK_STARTED, EverydayAction);
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);

        //goap
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnCharacterDidAction);
        Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
    }
    private void EverydayAction() {
        //DepleteThreatLevel();
    }
    private void OnKeyPressed(KeyCode pressedKey) {
        if (pressedKey == KeyCode.Escape) {
            if (currentActivePlayerJobAction != null) {
                SetCurrentlyActivePlayerJobAction(null);
                CursorManager.Instance.ClearLeftClickActions();
            }
        }
    }
    #endregion

    #region ILeader
    public void LevelUp() {
        //Not applicable
    }
    #endregion

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        chosenCoreTile.SetCorruption(true);
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        Biomes.Instance.CorruptTileVisuals(chosenCoreTile);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        //_demonicPortal.tileLocation.ScheduleCorruption();
        //OnTileAddedToPlayerArea(playerArea, chosenCoreTile);
    }
    public void CreatePlayerArea(BaseLandmark portal) {
        Area playerArea = LandmarkManager.Instance.CreateNewArea(portal.tileLocation, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        Biomes.Instance.CorruptTileVisuals(portal.tileLocation);
        portal.tileLocation.SetCorruption(true);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        //_demonicPortal.tileLocation.ScheduleCorruption();
    }
    public void LoadPlayerArea(Area area) {
        Biomes.Instance.CorruptTileVisuals(area.coreTile);
        area.coreTile.tileLocation.SetCorruption(true);
        SetPlayerArea(area);
        //_demonicPortal.tileLocation.ScheduleCorruption();

    }
    private void SetPlayerArea(Area area) {
        playerArea = area;
        //area.SetSuppliesInBank(_currencies[CURRENCY.SUPPLY]);
        //area.StopSupplyLine();
    }
    private void OnTileRemovedFromPlayerArea(Area affectedArea, HexTile removedTile) {
        if (playerArea != null && affectedArea.id == playerArea.id) {
            Biomes.Instance.UpdateTileVisuals(removedTile);
        }
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction playerFaction = FactionManager.Instance.CreateNewFaction(true);
        playerFaction.SetLeader(this);
        playerFaction.SetEmblem(FactionManager.Instance.GetFactionEmblem(6));
        SetPlayerFaction(playerFaction);
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    #endregion

    #region Minions
    public Minion CreateNewMinion(Character character) {
        Minion minion = new Minion(character, true);
        InitializeMinion(minion);
        return minion;
    }
    public Minion CreateNewMinion(RACE race) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, race, GENDER.MALE, playerFaction, playerArea, null), false);
        //minion.character.CreateMarker();
        InitializeMinion(minion);
        return minion;
    }
    public Minion CreateNewMinion(string className, RACE race) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, race, GENDER.MALE, playerFaction, playerArea), false);
        InitializeMinion(minion);
        return minion;
    }
    public Minion CreateNewMinionRandomClass(RACE race) {
        string className = CharacterManager.sevenDeadlySinsClassNames[UnityEngine.Random.Range(0, CharacterManager.sevenDeadlySinsClassNames.Length)];
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, race, GENDER.MALE, playerFaction, playerArea), false);
        InitializeMinion(minion);
        return minion;
    }
    public void InitializeMinion(Minion minion) {
        minion.SetUnlockedInterventionSlots(3);
        minion.AddInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(PlayerManager.Instance.allInterventionAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allInterventionAbilities.Length)]));
        minion.AddInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(PlayerManager.Instance.allInterventionAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allInterventionAbilities.Length)]));
        minion.SetCombatAbility(new CombatAbility()); //TODO: variations
        //TODO: Add one positive and one negative trait
    }
    public void AddMinion(Minion minion) {
        int currentMinionCount = GetCurrentMinionCount();
        if(currentMinionCount == minions.Length) {
            //Broadcast minion is full, must be received by a UI that will pop up and let the player whether it will replace or be discarded
        } else {
            minion.SetIndexDefaultSort(currentMinionCount);
            minions[currentMinionCount] = minion;
            PlayerUI.Instance.UpdateRoleSlots();
        }
    }
    public void RemoveMinion(Minion minion) {
        bool hasRemoved = false;
        for (int i = 0; i < minions.Length; i++) {
            if (minions[i] != null && minions[i] == minion) {
                minions[i] = null;
                hasRemoved = true;
                break;
            }
        }
        if (hasRemoved) {
            RearrangeMinions();
            PlayerUI.Instance.UpdateRoleSlots();
        }
    }
    public int GetCurrentMinionCount() {
        int count = 0;
        for (int i = 0; i < minions.Length; i++) {
            if(minions[i] != null) {
                count++;
            }
        }
        return count;
    }
    public void RearrangeMinions() {
        List<int> minionIndexesThatAreNotNull = new List<int>();
        for (int i = 0; i < minions.Length; i++) {
            if(minions[i] != null) {
                minionIndexesThatAreNotNull.Add(i);
            }
        }
        for (int i = 0; i < minions.Length; i++) {
            if(i < minionIndexesThatAreNotNull.Count) {
                minions[i] = minions[minionIndexesThatAreNotNull[i]];
            } else {
                minions[i] = null;
            }
        }
        //Update UI
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

    //#region Role Slots
    //public void ConstructRoleSlots() {
    //    roleSlots = new Dictionary<JOB, PlayerJobData>();
    //    roleSlots.Add(JOB.SPY, new PlayerJobData(JOB.SPY));
    //    roleSlots.Add(JOB.SEDUCER, new PlayerJobData(JOB.SEDUCER));
    //    roleSlots.Add(JOB.DIPLOMAT, new PlayerJobData(JOB.DIPLOMAT));
    //    roleSlots.Add(JOB.INSTIGATOR, new PlayerJobData(JOB.INSTIGATOR));
    //    roleSlots.Add(JOB.DEBILITATOR, new PlayerJobData(JOB.DEBILITATOR));
    //}
    //public List<JOB> GetValidJobForCharacter(Character character) {
    //    List<JOB> validJobs = new List<JOB>();
    //    if (character.minion != null) {
    //        switch (character.characterClass.className) {
    //            case "Envy":
    //                validJobs.Add(JOB.SPY);
    //                validJobs.Add(JOB.SEDUCER);
    //                break;
    //            case "Lust":
    //                validJobs.Add(JOB.DIPLOMAT);
    //                validJobs.Add(JOB.SEDUCER);
    //                break;
    //            case "Pride":
    //                validJobs.Add(JOB.DIPLOMAT);
    //                validJobs.Add(JOB.INSTIGATOR);
    //                break;
    //            case "Greed":
    //                validJobs.Add(JOB.SPY);
    //                validJobs.Add(JOB.INSTIGATOR);
    //                break;
    //            case "Guttony":
    //                validJobs.Add(JOB.SPY);
    //                validJobs.Add(JOB.SEDUCER);
    //                break;
    //            case "Wrath":
    //                validJobs.Add(JOB.INSTIGATOR);
    //                validJobs.Add(JOB.DEBILITATOR);
    //                break;
    //            case "Sloth":
    //                validJobs.Add(JOB.DEBILITATOR);
    //                validJobs.Add(JOB.DIPLOMAT);
    //                break;
    //        }
    //    } else {
    //        switch (character.race) {
    //            case RACE.HUMANS:
    //                validJobs.Add(JOB.DIPLOMAT);
    //                validJobs.Add(JOB.SEDUCER);
    //                break;
    //            case RACE.ELVES:
    //                validJobs.Add(JOB.SPY);
    //                validJobs.Add(JOB.DIPLOMAT);
    //                break;
    //            case RACE.GOBLIN:
    //                validJobs.Add(JOB.INSTIGATOR);
    //                validJobs.Add(JOB.SEDUCER);
    //                break;
    //            case RACE.FAERY:
    //                validJobs.Add(JOB.SPY);
    //                validJobs.Add(JOB.DEBILITATOR);
    //                break;
    //            case RACE.SKELETON:
    //                validJobs.Add(JOB.DEBILITATOR);
    //                validJobs.Add(JOB.INSTIGATOR);
    //                break;
    //        }
    //    }
    //    return validJobs;
    //}
    //public bool CanAssignCharacterToJob(JOB job, Character character) {
    //    List<JOB> jobs = GetValidJobForCharacter(character);
    //    return jobs.Contains(job);
    //}
    //public bool CanAssignCharacterToAttack(Character character) {
    //    return GetCharactersCurrentJob(character) == JOB.NONE && !defenseGrid.IsCharacterInGrid(character);
    //}
    //public bool CanAssignCharacterToDefend(Character character) {
    //    return GetCharactersCurrentJob(character) == JOB.NONE && !attackGrid.IsCharacterInGrid(character);
    //}
    //public void AssignCharacterToJob(JOB job, Character character) {
    //    if (!roleSlots.ContainsKey(job)) {
    //        Debug.LogWarning("There is something trying to assign a character to " + job.ToString() + " but the player doesn't have a slot for it.");
    //        return;
    //    }
    //    if (roleSlots[job].assignedCharacter != null) {
    //        UnassignCharacterFromJob(job);
    //    }
    //    JOB charactersCurrentJob = GetCharactersCurrentJob(character);
    //    if (charactersCurrentJob != JOB.NONE) {
    //        UnassignCharacterFromJob(charactersCurrentJob);
    //    }

    //    roleSlots[job].AssignCharacter(character);
    //    //Messenger.Broadcast(Signals.MINION_ASSIGNED_TO_JOB, job, character);
    //}
    //public void UnassignCharacterFromJob(JOB job) {
    //    if (!roleSlots.ContainsKey(job)) {
    //        Debug.LogWarning("There is something trying to unassign a character from " + job.ToString() + " but the player doesn't have a slot for it.");
    //        return;
    //    }
    //    if (roleSlots[job] == null) {
    //        return; //ignore command
    //    }
    //    Character character = roleSlots[job].assignedCharacter;
    //    roleSlots[job].AssignCharacter(null);
    //    //Messenger.Broadcast(Signals.MINION_UNASSIGNED_FROM_JOB, job, character);
    //}
    //public void AssignAttackGrid(CombatGrid grid) {
    //    attackGrid = grid;
    //}
    //public void AssignDefenseGrid(CombatGrid grid) {
    //    defenseGrid = grid;
    //}
    //public JOB GetCharactersCurrentJob(Character character) {
    //    foreach (KeyValuePair<JOB, PlayerJobData> keyValuePair in roleSlots) {
    //        if (keyValuePair.Value.assignedCharacter != null && keyValuePair.Value.assignedCharacter.id == character.id) {
    //            return keyValuePair.Key;
    //        }
    //    }
    //    return JOB.NONE;
    //}
    //public bool HasCharacterAssignedToJob(JOB job) {
    //    return roleSlots[job].assignedCharacter != null;
    //}
    ////private List<Character> GetValidCharactersForJob(JOB job) {
    ////    List<Character> valid = new List<Character>();
    ////    for (int i = 0; i < minions.Count; i++) {
    ////        Character currMinion = minions[i].character;
    ////        if (CanAssignCharacterToJob(job, currMinion) && GetCharactersCurrentJob(currMinion) == JOB.NONE) {
    ////            valid.Add(currMinion);
    ////        }
    ////    }
    ////    return valid;
    ////}
    ////public void PreAssignJobSlots() {
    ////    foreach (KeyValuePair<JOB, PlayerJobData> kvp in roleSlots) {
    ////        List<Character> validCharacters = GetValidCharactersForJob(kvp.Key);
    ////        if (validCharacters.Count > 0) {
    ////            Character chosenCharacter = validCharacters[UnityEngine.Random.Range(0, validCharacters.Count)];
    ////            AssignCharacterToJob(kvp.Key, chosenCharacter);
    ////        } else {
    ////            Debug.LogWarning("Could not pre assign any character to job: " + kvp.Key.ToString());
    ////        }
    ////    }
    ////}
    //#endregion

    #region Role Actions
    //public List<PlayerJobAction> GetJobActionsThatCanTarget(JOB job, IPointOfInterest target) {
    //    List<PlayerJobAction> actions = new List<PlayerJobAction>();
    //    if (HasCharacterAssignedToJob(job)) {
    //        for (int i = 0; i < roleSlots[job].jobActions.Count; i++) {
    //            PlayerJobAction currAction = roleSlots[job].jobActions[i];
    //            if (currAction.CanTarget(target)) {
    //                actions.Add(currAction);
    //            }
    //        }
    //    }
    //    return actions;
    //}
    public PlayerJobAction currentActivePlayerJobAction { get; private set; }
    public void SetCurrentlyActivePlayerJobAction(PlayerJobAction action) {
        PlayerJobAction previousActiveAction = currentActivePlayerJobAction;
        currentActivePlayerJobAction = action;
        if (currentActivePlayerJobAction == null) {
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
            PlayerJobActionButton jobActionButton = PlayerUI.Instance.GetPlayerJobActionButton(previousActiveAction);
            jobActionButton?.UpdateInteractableState();
            jobActionButton?.SetSelectedIconState(false);
            //CursorManager.Instance.SetElectricEffectState(false);
        } else {
            PlayerJobActionButton jobActionButton = PlayerUI.Instance.GetPlayerJobActionButton(currentActivePlayerJobAction);
            //change the cursor
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Cross);
            CursorManager.Instance.AddLeftClickAction(TryExecuteCurrentActiveAction);
            CursorManager.Instance.AddLeftClickAction(() => SetCurrentlyActivePlayerJobAction(null));
            jobActionButton?.SetSelectedIconState(true);
            //if (action is Zap) {
            //    CursorManager.Instance.SetElectricEffectState(true);
            //}
        }
        
    }
    private void TryExecuteCurrentActiveAction() {
        string summary = "Mouse was clicked. Will try to execute " + currentActivePlayerJobAction.name;
        if (InteriorMapManager.Instance.currentlyShowingMap != null && InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter != null) {
            summary += " targetting " + InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter.name;
            if (currentActivePlayerJobAction.CanPerformActionTowards(currentActivePlayerJobAction.minion.character, InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter)) {
                summary += "\nActivated action!";
                currentActivePlayerJobAction.ActivateAction(currentActivePlayerJobAction.minion.character, InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter);
            } else {
                summary += "\nDid not activate action! Did not meet requirements";
            }
            UIManager.Instance.SetTempDisableShowInfoUI(true);
        } else {
            LocationGridTile hoveredTile = InteriorMapManager.Instance.GetTileFromMousePosition();
            if (hoveredTile != null && hoveredTile.objHere != null) {
                summary += " targetting " + hoveredTile.objHere.name;
                if (currentActivePlayerJobAction.CanPerformActionTowards(currentActivePlayerJobAction.minion.character, hoveredTile.objHere)) {
                    summary += "\nActivated action!";
                    currentActivePlayerJobAction.ActivateAction(currentActivePlayerJobAction.minion.character, hoveredTile.objHere);
                } else {
                    summary += "\nDid not activate action! Did not meet requirements";
                }
                UIManager.Instance.SetTempDisableShowInfoUI(true);
            } else {
                summary += "\nNo Target!";
            }
        }
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region Utilities
    //private void OnCharacterDied(Character character) {
    //    JOB job = GetCharactersCurrentJob(character);
    //    if (job != JOB.NONE) {
    //        UnassignCharacterFromJob(job);
    //    }
    //}
    public void SeenActionButtonsOnce() {
        if (!hasSeenActionButtonsOnce) {
            hasSeenActionButtonsOnce = true;
            Messenger.Broadcast(Signals.HAS_SEEN_ACTION_BUTTONS);
        }
    }
    #endregion

    #region Intel
    public void AddIntel(Intel newIntel) {
        if (!allIntel.Contains(newIntel)) {
            for (int i = 0; i < allIntel.Count; i++) {
                Intel currIntel = allIntel[i];
                if (currIntel.intelLog == newIntel.intelLog) {
                    return;
                }
            }
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
    /// <summary>
    /// Listener for when a character has finished doing an action.
    /// </summary>
    /// <param name="character">The character that finished the action.</param>
    /// <param name="action">The action that was finished.</param>
    private void OnCharacterDidAction(Character character, GoapAction action) {
        for (int i = 0; i < action.currentState.arrangedLogs.Count; i++) {
            if(action.currentState.arrangedLogs[i].notifAction != null) {
                action.currentState.arrangedLogs[i].notifAction();
            } else {
                bool showPopup = false;
                if (action.showIntelNotification) {
                    if (action.shouldIntelNotificationOnlyIfActorIsActive) {
                        showPopup = ShouldShowNotificationFrom(character, true);
                    } else {
                        showPopup = ShouldShowNotificationFrom(character, action.currentState.descriptionLog);
                    }
                }
                if (showPopup) {
                    if (!action.isNotificationAnIntel) {
                        Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, action.currentState.descriptionLog);
                    } else {
                        Messenger.Broadcast<Intel>(Signals.SHOW_INTEL_NOTIFICATION, InteractionManager.Instance.CreateNewIntel(action, character));
                    }
                }
            }
        }
    }
    /// <summary>
    /// Listener for when a character starts an action.
    /// Character will go to target location. <see cref="GoapAction.DoAction"/>
    /// </summary>
    /// <param name="character">The character that will do the action.</param>
    /// <param name="action">The action that will be performed.</param>
    private void OnCharacterDoingAction(Character character, GoapAction action) {
        bool showPopup = false;
        Log log = action.GetCurrentLog();
        if (action.showIntelNotification && !action.IsActorAtTargetTile() && log != null) { //added checking if actor is already at target tile. So that travelling notification won't show if that is the case.
            if (action.shouldIntelNotificationOnlyIfActorIsActive) {
                showPopup = ShouldShowNotificationFrom(action.actor, true);
            } else {
                showPopup = ShouldShowNotificationFrom(action.actor, log);
            }
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
    private void OnActionStateSet(GoapAction action, GoapActionState state) {
        bool showPopup = false;
        Log log = action.GetCurrentLog();
        if (action.showIntelNotification && state.duration > 0 && log != null) { //added checking for duration because this notification should only show for actions that have durations.
            if (action.shouldIntelNotificationOnlyIfActorIsActive) {
                showPopup = ShouldShowNotificationFrom(action.actor, true);
            } else {
                showPopup = ShouldShowNotificationFrom(action.actor, log);
            }
        }
        if (showPopup) {
            Messenger.Broadcast<Log>(Signals.SHOW_PLAYER_NOTIFICATION, log);
        }
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

        if (!onlyClickedCharacter && !character.isDead && AreaMapCameraMove.Instance.gameObject.activeSelf) {
            if((UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) || AreaMapCameraMove.Instance.CanSee(character.marker.gameObject)) {
                return true;
            }
        } else if (onlyClickedCharacter && UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) {
            return true;
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Character character, Log log) {
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

    #region Threat
    public void AdjustThreat(int amount) {
        threat += amount;
        threat = Mathf.Clamp(threat, 0, MAX_THREAT);
        PlayerUI.Instance.UpdateThreatMeter();
    }
    #endregion

    #region Tile Corruption
    public void SetCurrentTileBeingCorrupted(HexTile tile) {
        currentTileBeingCorrupted = tile;
    }
    public void CorruptATile() {
        currentCorruptionDuration = currentTileBeingCorrupted.corruptDuration;
        if(currentCorruptionDuration == 0) {
            Debug.LogError("Cannot corrupt a tile with 0 corruption duration");
        } else {
            GameManager.Instance.SetOnlyTickDays(true);
            currentTileBeingCorrupted.StartCorruptionAnimation();
            currentCorruptionTick = 0;
            Messenger.AddListener(Signals.DAY_STARTED, CorruptTilePerTick);
            GameManager.Instance.SetPausedState(false);
            isTileCurrentlyBeingCorrupted = true;
        }
    }
    private void CorruptTilePerTick() {
        currentCorruptionTick ++;
        AdjustThreat(1);
        if(currentCorruptionTick >= currentCorruptionDuration) {
            TileIsCorrupted();
        }
    }
    private void TileIsCorrupted() {
        isTileCurrentlyBeingCorrupted = false;
        Messenger.RemoveListener(Signals.DAY_STARTED, CorruptTilePerTick);
        GameManager.Instance.SetPausedState(true);
        PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
    }
    #endregion

    #region Summons
    public void GainSummon(SUMMON_TYPE type) {
        if (GetTotalSummonsCount() < MAX_SUMMONS) {
            Summon newSummon = CharacterManager.Instance.CreateNewSummon(type, GetRoleForSummon(type), GetRaceForSummon(type), Utilities.GetRandomGender(), playerFaction, playerArea);
            AddSummon(newSummon);
        } else {
            Debug.LogWarning("Max summons has been reached!");
        }
    }
    public int GetTotalSummonsCount() {
        int count = 0;
        foreach (KeyValuePair<SUMMON_TYPE, List<Summon>> kvp in summons) {
            count += summons[kvp.Key].Count;
        }
        
        return count;
    }
    public int GetSummonsOfTypeCount(SUMMON_TYPE type) {
        if (!summons.ContainsKey(type)) {
            return 0;
        }
        return summons[type].Count;
    }
    private void AddSummon(Summon newSummon) {
        if (!summons.ContainsKey(newSummon.summonType)) {
            summons.Add(newSummon.summonType, new List<Summon>());
        }
        if (!summons[newSummon.summonType].Contains(newSummon)) {
            summons[newSummon.summonType].Add(newSummon);
            Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON, newSummon);
        }
    }
    private void RemoveSummon(Summon summon) {
        if (summons[summon.summonType].Remove(summon)) {
            if (summons[summon.summonType].Count == 0) {
                summons.Remove(summon.summonType);
            }
            Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON, summon);
        }
    }
    private CharacterRole GetRoleForSummon(SUMMON_TYPE summonType) {
        switch (summonType) {
            default:
                return CharacterRole.MINION;
        }
    }
    private RACE GetRaceForSummon(SUMMON_TYPE summonType) {
        switch (summonType) {
            case SUMMON_TYPE.WOLF:
                return RACE.WOLF;
            case SUMMON_TYPE.SKELETON:
                return RACE.SKELETON;
            case SUMMON_TYPE.GOLEM:
                return RACE.DEMON;
            case SUMMON_TYPE.SUCCUBUS:
                return RACE.DEMON;
            case SUMMON_TYPE.INCUBUS:
                return RACE.DEMON;
            case SUMMON_TYPE.THIEF:
                return RACE.DEMON;
            default:
                return RACE.DEMON;
        }
    }
    /// <summary>
    /// Get a list of all summon types that the player has.
    /// </summary>
    /// <returns>List of summon types.</returns>
    public List<SUMMON_TYPE> GetAvailableSummonTypes() {
        return summons.Keys.ToList();
    }
    #endregion
}

