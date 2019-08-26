using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Linq;

public class Player : ILeader {

    private const int MAX_INTEL = 3;
    public const int MAX_MINIONS = 5;
    public const int MAX_THREAT = 100;
    private const int MAX_SUMMONS = 3;
    private const int MAX_ARTIFACT = 3;
    public readonly int MAX_INTERVENTION_ABILITIES = 4;

    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int threat { get; private set; }

    //public Dictionary<JOB, PlayerJobData> roleSlots { get; private set; }
    public CombatGrid attackGrid { get; private set; }
    public CombatGrid defenseGrid { get; private set; }
    public List<Intel> allIntel { get; private set; }
    public List<Minion> minions { get; private set; }
    public SummonSlot[] summonSlots { get; private set; } //Summons that the player can still place. Does NOT include summons that have been placed. Individual summons are responsible for placeing themselves back after the player is done with a map.
    public ArtifactSlot[] artifactSlots { get; private set; }
    //Unique ability of player
    //public ShareIntel shareIntelAbility { get; private set; }
    public int currentCorruptionDuration { get; private set; }
    public int currentCorruptionTick { get; private set; }
    public bool isTileCurrentlyBeingCorrupted { get; private set; }
    public HexTile currentTileBeingCorrupted { get; private set; }
    public Minion currentMinionLeader { get; private set; }
    public Area currentAreaBeingInvaded { get; private set; }
    public CombatAbility currentActiveCombatAbility { get; private set; }
    public Intel currentActiveIntel { get; private set; }
    public int maxSummonSlots { get; private set; } //how many summons can the player have
    public int maxArtifactSlots { get; private set; } //how many artifacts can the player have
    public Faction currentTargetFaction { get; private set; } //the current faction that the player is targeting.
    public PlayerJobActionSlot[] interventionAbilitySlots { get; private set; }
    public Region invadingRegion { get; private set; }
    public int currentInterventionAbilityTimerTick { get; private set; }
    public int newInterventionAbilityTimerTicks { get; private set; }
    public int currentNewInterventionAbilityCycleIndex { get; private set; }

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
    public bool isInvadingRegion {
        get { return invadingRegion != null; }
    }
    #endregion

    public Player() {
        playerArea = null;
        attackGrid = new CombatGrid();
        defenseGrid = new CombatGrid();
        attackGrid.Initialize();
        defenseGrid.Initialize();
        allIntel = new List<Intel>();
        minions = new List<Minion>();
        summonSlots = new SummonSlot[MAX_SUMMONS];
        artifactSlots = new ArtifactSlot[MAX_ARTIFACT];
        interventionAbilitySlots = new PlayerJobActionSlot[MAX_INTERVENTION_ABILITIES];
        //shareIntelAbility = new ShareIntel();
        maxSummonSlots = 1;
        maxArtifactSlots = 1;
        //ConstructRoleSlots();
        InitializeNewInterventionAbilityCycle();
        ConstructAllInterventionAbilitySlots();
        ConstructAllSummonSlots();
        ConstructAllArtifactSlots();
        AddListeners();
    }
    public Player(SaveDataPlayer data) {
        attackGrid = new CombatGrid();
        defenseGrid = new CombatGrid();
        attackGrid.Initialize();
        defenseGrid.Initialize();
        allIntel = new List<Intel>();
        minions = new List<Minion>();
        summonSlots = new SummonSlot[MAX_SUMMONS];
        artifactSlots = new ArtifactSlot[MAX_ARTIFACT];
        interventionAbilitySlots = new PlayerJobActionSlot[MAX_INTERVENTION_ABILITIES];
        maxSummonSlots = data.maxSummonSlots;
        maxArtifactSlots = data.maxArtifactSlots;
        //threat = data.threat;
        InitializeNewInterventionAbilityCycle();
        ConstructAllInterventionAbilitySlots();
        ConstructAllSummonSlots();
        ConstructAllArtifactSlots();
        AddListeners();
    }

    #region Listeners
    private void AddListeners() {
        AddWinListener();
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, OnTileRemovedFromPlayerArea);
        Messenger.AddListener(Signals.TICK_STARTED, PerTickPlayer);

        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);

        //goap
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnCharacterDidAction);
        Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
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
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION, 0);
        playerArea.LoadAdditionalData();
        LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        Biomes.Instance.CorruptTileVisuals(chosenCoreTile);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        //_demonicPortal.tileLocation.ScheduleCorruption();
        //OnTileAddedToPlayerArea(playerArea, chosenCoreTile);
    }
    public void CreatePlayerArea(BaseLandmark portal) {
        Area playerArea = LandmarkManager.Instance.CreateNewArea(portal.tileLocation, AREA_TYPE.DEMONIC_INTRUSION, 0);
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
    public void SetPlayerArea(Area area) {
        playerArea = area;
        //area.SetSuppliesInBank(_currencies[CURRENCY.SUPPLY]);
        //area.StopSupplyLine();
    }
    private void OnTileRemovedFromPlayerArea(Area affectedArea, HexTile removedTile) {
        if (playerArea != null && affectedArea.id == playerArea.id) {
            Biomes.Instance.UpdateTileVisuals(removedTile);
        }
    }
    private void OnAreaMapOpened(Area area) {
        for (int i = 0; i < minions.Count; i++) {
            minions[i].ResetCombatAbilityCD();
        }
        ResetInterventionAbilitiesCD();
        //currentTargetFaction = area.owner;
    }
    private void OnAreaMapClosed(Area area) {
        //currentTargetFaction = null;
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction playerFaction = FactionManager.Instance.CreateNewFaction(true, "Player faction");
        playerFaction.SetLeader(this);
        playerFaction.SetEmblem(FactionManager.Instance.GetFactionEmblem(6));
        SetPlayerFaction(playerFaction);
    }
    public void CreatePlayerFaction(SaveDataPlayer data) {
        Faction playerFaction = FactionManager.Instance.GetFactionBasedOnID(data.playerFactionID);
        playerFaction.SetLeader(this);
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
    public Minion CreateNewMinion(SaveDataMinion data) {
        Minion minion = new Minion(data);
        InitializeMinion(data, minion);
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
    public Minion CreateNewMinionRandomClass() {
        string className = CharacterManager.sevenDeadlySinsClassNames[UnityEngine.Random.Range(0, CharacterManager.sevenDeadlySinsClassNames.Length)];
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, RACE.DEMON, GENDER.MALE, playerFaction, playerArea), false);
        InitializeMinion(minion);
        return minion;
    }
    public void InitializeMinion(Minion minion) {
        //minion.SetLevel(30);
        //minion.SetUnlockedInterventionSlots(3);
        //minion.GainNewInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(PlayerManager.Instance.allInterventionAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allInterventionAbilities.Length)]));
        //minion.GainNewInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(PlayerManager.Instance.allInterventionAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allInterventionAbilities.Length)]));
        minion.SetCombatAbility(PlayerManager.Instance.CreateNewCombatAbility(PlayerManager.Instance.allCombatAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allCombatAbilities.Length)]));
        //TODO: Add one positive and one negative trait
    }
    public void InitializeMinion(SaveDataMinion data, Minion minion) {
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
        if (!minions.Contains(minion)) {
            minions.Add(minion);
            if (showNewMinionUI) {
                PlayerUI.Instance.ShowNewMinionUI(minion);
            }
            //PlayerUI.Instance.UpdateRoleSlots();
            Messenger.Broadcast(Signals.PLAYER_GAINED_MINION, minion);
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
    //public void RearrangeMinions() {
    //    List<int> minionIndexesThatAreNotNull = new List<int>();
    //    for (int i = 0; i < minions.Count; i++) {
    //        if(minions[i] != null) {
    //            minionIndexesThatAreNotNull.Add(i);
    //        }
    //    }
    //    for (int i = 0; i < minions.Count; i++) {
    //        if(i < minionIndexesThatAreNotNull.Count) {
    //            minions[i] = minions[minionIndexesThatAreNotNull[i]];
    //        } else {
    //            minions[i] = null;
    //        }
    //    }
    //    //Update UI
    //}
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
            if (previousActiveAction != null) {
                previousActiveAction.HideRange(InteriorMapManager.Instance.GetTileFromMousePosition());
            }
        } else {
            PlayerJobActionButton jobActionButton = PlayerUI.Instance.GetPlayerJobActionButton(currentActivePlayerJobAction);
            //change the cursor
            CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Cross);
            CursorManager.Instance.AddLeftClickAction(TryExecuteCurrentActiveAction);
            CursorManager.Instance.AddLeftClickAction(() => SetCurrentlyActivePlayerJobAction(null));
            jobActionButton?.SetSelectedIconState(true);
        }
    }
    private void TryExecuteCurrentActiveAction() {
        //string summary = "Mouse was clicked. Will try to execute " + currentActivePlayerJobAction.name;
        LocationGridTile hoveredTile;
        for (int i = 0; i < currentActivePlayerJobAction.targetTypes.Length; i++) {
            bool activatedAction = false;
            switch (currentActivePlayerJobAction.targetTypes[i]) {
                case JOB_ACTION_TARGET.NONE:
                    //summary += "\nNo Target!";
                    break;
                case JOB_ACTION_TARGET.CHARACTER:
                    if (InteriorMapManager.Instance.currentlyShowingMap != null && InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter != null) {
                        //summary += " targetting " + InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter.name;
                        if (currentActivePlayerJobAction.CanPerformActionTowards(InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter)) {
                            //summary += "\nActivated action!";
                            currentActivePlayerJobAction.ActivateAction(InteriorMapManager.Instance.currentlyShowingMap.hoveredCharacter);
                            activatedAction = true;
                        } else {
                            //summary += "\nDid not activate action! Did not meet requirements";
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    } else {
                        //summary += "\nThere is no hovered character!";
                    }
                    break;
                case JOB_ACTION_TARGET.TILE_OBJECT:
                    hoveredTile = InteriorMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null && hoveredTile.objHere != null) {
                        //summary += " targetting " + hoveredTile.objHere.name;
                        if (currentActivePlayerJobAction.CanPerformActionTowards(hoveredTile.objHere)) {
                            //summary += "\nActivated action!";
                            currentActivePlayerJobAction.ActivateAction(hoveredTile.objHere);
                            activatedAction = true;
                        } else {
                            //summary += "\nDid not activate action! Did not meet requirements";
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    } else {
                        //summary += "\nThere is no hovered tile object!";
                    }
                    break;
                case JOB_ACTION_TARGET.TILE:
                    hoveredTile = InteriorMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null) {
                        //summary += " targetting " + hoveredTile.ToString();
                        if (currentActivePlayerJobAction.CanPerformActionTowards(hoveredTile)) {
                            //summary += "\nActivated action!";
                            currentActivePlayerJobAction.ActivateAction(hoveredTile);
                            activatedAction = true;
                        } else {
                            //summary += "\nDid not activate action! Did not meet requirements";
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    } else {
                        //summary += "\nThere is no hovered tile object!";
                    }
                    break;
                default:
                    //summary += "\nNo casing for target type: " + currentActivePlayerJobAction.targetTypes.ToString();
                    break;
            }
            if (activatedAction) {
                break;
            }
        }
        
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    //public string GetInterventionAbilityDescription(PlayerJobAction action) {
    //    if (action is ShareIntel) {
    //        return "The Diplomat will reach out to a character and share a piece of information with them.";
    //    } else if (action is RileUp) {
    //        return "The Instigator will rile up a character and goad him into attacking people in a specified location. This action only works for beasts.";
    //    } else if (action is Provoke) {
    //        return "The Instigator will provoke a character into attacking one of his/her enemies. This is more likely to succeed if he/she is in a bad mood.";
    //    } else if (action is Destroy) {
    //        return "Remove this object from the world.";
    //    } else if (action is Disable) {
    //        return "Prevent characters from using this object for 4 hours.";
    //    } else if (action is AccessMemories) {
    //        return "Access the memories of a character.";
    //    } else if (action is Abduct) {
    //        return "The Instigator will goad a character into abducting a specified character. This action only works on goblins and skeletons.";
    //    } else if (action is Zap) {
    //        return "Temporarily prevents a character from moving for 30 minutes.";
    //    } else if (action is Jolt) {
    //        return "Temporarily speeds up the movement of a character.";
    //    } else if (action is Spook) {
    //        return "Temporarily forces a character to flee from all other nearby characters.";
    //    } else if (action is Enrage) {
    //        return "Temporarily enrages a character.";
    //    } else if (action is CorruptLycanthropy) {
    //        return "Inflict a character with Lycanthropy, which gives a character a chance to transform into a wild wolf whenever he/she sleeps.";
    //    } else if (action is CorruptKleptomaniac) {
    //        return "Inflict a character with Kleptomania, which will make that character enjoy stealing other people's items.";
    //    } else if (action is CorruptVampiric) {
    //        return "Inflict a character with Vampirism, which will make that character need blood for sustenance.";
    //    } else if (action is CorruptUnfaithful) {
    //        return "Make a character prone to have affairs.";
    //    } else if (action is RaiseDead) {
    //        return "Return a character to life.";
    //    } else {
    //        return action.GetType().ToString();
    //    }
    //}
    #endregion

    #region Utilities
    //private void OnCharacterDied(Character character) {
    //    JOB job = GetCharactersCurrentJob(character);
    //    if (job != JOB.NONE) {
    //        UnassignCharacterFromJob(job);
    //    }
    //}
    private void PerTickPlayer() {
        PerTickInterventionAbility();
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
        if (CanShareIntel(InteriorMapManager.Instance.currentlyHoveredPOI)) {
            Character targetCharacter = InteriorMapManager.Instance.currentlyHoveredPOI as Character;
            UIManager.Instance.OpenShareIntelMenu(targetCharacter, currentMinionLeader.character, currentActiveIntel);
        }
    }
    public bool CanShareIntel(IPointOfInterest poi) {
        if(poi is Character) {
            Character character = poi as Character;
            if(character.faction != PlayerManager.Instance.player.playerFaction && character.role.roleType != CHARACTER_ROLE.BEAST && character.role.roleType != CHARACTER_ROLE.PLAYER) {
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

        if (!onlyClickedCharacter && AreaMapCameraMove.Instance.gameObject.activeSelf) { //&& !character.isDead
            if ((UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == character.id) || (character.marker != null &&  AreaMapCameraMove.Instance.CanSee(character.marker.gameObject))) {
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
        //PlayerUI.Instance.UpdateThreatMeter();
        if(threat >= MAX_THREAT) {
            PlayerUI.Instance.GameOver("Your threat reached the whole world. You are now exposed. You lost!");
        }
    }
    public void SetThreat(int amount) {
        threat = amount;
        threat = Mathf.Clamp(threat, 0, MAX_THREAT);
        //PlayerUI.Instance.UpdateThreatMeter();
        if (threat >= MAX_THREAT) {
            PlayerUI.Instance.GameOver("Your threat reached the whole world. You are now exposed. You lost!");
        }
    }
    public void ResetThreat() {
        threat = 0;
        //PlayerUI.Instance.UpdateThreatMeter();
    }
    #endregion

    #region Tile Corruption
    public void SetCurrentTileBeingCorrupted(HexTile tile) {
        currentTileBeingCorrupted = tile;
    }
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
        if(currentTileBeingCorrupted.landmarkOnTile != null) {
            currentTileBeingCorrupted.landmarkOnTile.InvadeThisLandmark();
        }
        PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
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
        UIManager.Instance.Pause();
        PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
    }
    #endregion

    #region Area Corruption
    private Area AreaIsCorrupted() {
        isTileCurrentlyBeingCorrupted = false;
        GameManager.Instance.SetPausedState(true);
        Area corruptedArea = currentTileBeingCorrupted.areaOfTile;
        PlayerManager.Instance.AddTileToPlayerArea(currentTileBeingCorrupted);
        return corruptedArea;
    }
    #endregion

    #region Summons
    private void ConstructAllSummonSlots() {
        for (int i = 0; i < summonSlots.Length; i++) {
            if (summonSlots[i] == null) {
                summonSlots[i] = new SummonSlot();
            }
        }
    }
    public void GainSummon(SUMMON_TYPE type, int level = 1, bool showNewSummonUI = false) {
        Summon newSummon = CharacterManager.Instance.CreateNewSummon(type, playerFaction, playerArea);
        newSummon.SetLevel(level);
        GainSummon(newSummon, showNewSummonUI);
    }
    public void GainSummon(Summon summon, bool showNewSummonUI = false) {
        if (GetTotalSummonsCount() < maxSummonSlots) {
            AddSummon(summon, showNewSummonUI);
        } else {
            Debug.LogWarning("Max summons has been reached!");
            PlayerUI.Instance.replaceUI.ShowReplaceUI(GetAllSummons(), summon, ReplaceSummon, RejectSummon);
        }
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
        for (int i = 0; i < summonSlots.Length; i++) {
            if (summonSlots[i].summon != null) {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// Get number of summons that have not been used yet.
    /// </summary>
    public int GetTotalAvailableSummonsCount() {
        int count = 0;
        for (int i = 0; i < summonSlots.Length; i++) {
            if (summonSlots[i].summon != null && !summonSlots[i].summon.hasBeenUsed) {
                count++;
            }
        }
        return count;
    }
    /// <summary>
    /// Get the number of summons of a specific type that the player has not yet used.
    /// </summary>
    /// <param name="type">The type of summon.</param>
    /// <returns>Integer</returns>
    //public int GetAvailableSummonsOfTypeCount(SUMMON_TYPE type) {
    //    if (!summonSlots.ContainsKey(type)) {
    //        return 0;
    //    }
    //    int count = 0;
    //    for (int i = 0; i < summonSlots[type].Count; i++) {
    //        Summon currSummon = summonSlots[type][i];
    //        if (currSummon.summonType == type && !currSummon.hasBeenUsed) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    private void AddSummon(Summon newSummon, bool showNewSummonUI = false) {
        for (int i = 0; i < summonSlots.Length; i++) {
            if (summonSlots[i].summon == null) {
                summonSlots[i].SetSummon(newSummon);
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
        for (int i = 0; i < summonSlots.Length; i++) {
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
        PlayerManager.Instance.player.playerFaction.RemoveCharacter(summon);
        PlayerManager.Instance.player.playerArea.RemoveCharacterFromLocation(summon);
        PlayerManager.Instance.player.playerArea.RemoveResident(summon);
        CharacterManager.Instance.RemoveCharacter(summon);
    }
    public Summon GetAvailableSummonOfType(SUMMON_TYPE type) {
        List<SummonSlot> choices = summonSlots.Where(x => x.summon != null && !x.summon.hasBeenUsed && x.summon.summonType == type).ToList();
        return choices[Random.Range(0, choices.Count)].summon;
    }
    public bool HasSummonOfType(SUMMON_TYPE summonType) {
        for (int i = 0; i < summonSlots.Length; i++) {
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
        for (int i = 0; i < summonSlots.Length; i++) {
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
        for (int i = 0; i < summonSlots.Length; i++) {
            if (summonSlots[i].summon != null) {
                summonSlots[i].summon.Reset();
            }
        }
    }
    public void AdjustSummonSlot(int adjustment) {
        maxSummonSlots += adjustment;
        maxSummonSlots = Mathf.Clamp(maxSummonSlots, 0, MAX_SUMMONS);
        //TODO: validate if adjusted max summons can accomodate current summons
    }
    public SummonSlot GetSummonSlotBySummon(Summon summon) {
        for (int i = 0; i < summonSlots.Length; i++) {
            if (summonSlots[i].summon == summon) {
                return summonSlots[i];
            }
        }
        return null;
    }
    public int GetIndexForSummonSlot(SummonSlot slot) {
        for (int i = 0; i < summonSlots.Length; i++) {
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
    public void UnlockASummonSlotOrUpgradeExisting() {
        PlayerUI.Instance.levelUpUI.ShowLevelUpUI(null, "summon_slot");
        //if (AreAllSummonSlotsMaxLevel()) {
        //    AdjustSummonSlot(1);
        //    PlayerUI.Instance.ShowGeneralConfirmation("Congratulations!", "You gained 1 Summon Slot.");
        //} else {
        //    int chance = UnityEngine.Random.Range(0, 2);
        //    if (chance == 0) {
        //        //Unlock slot
        //        AdjustSummonSlot(1);
        //        PlayerUI.Instance.ShowGeneralConfirmation("Congratulations!", "You gained 1 Summon Slot.");
        //    } else {
        //        //Upgrade slot
        //        PlayerUI.Instance.levelUpUI.ShowLevelUpUI(null, "summon_slot");
        //    }
        //}
    }
    #endregion

    #region Artifacts
    private void ConstructAllArtifactSlots() {
        for (int i = 0; i < artifactSlots.Length; i++) {
            if(artifactSlots[i] == null) {
                artifactSlots[i] = new ArtifactSlot();
            }
        }
    }
    public void GainArtifact(ARTIFACT_TYPE type, bool showNewArtifactUI = false) {
        Artifact newArtifact = PlayerManager.Instance.CreateNewArtifact(type);
        GainArtifact(newArtifact, showNewArtifactUI);
    }
    public void GainArtifact(Artifact artifact, bool showNewArtifactUI = false) {
        if (GetTotalArtifactCount() < maxArtifactSlots) {
            AddArtifact(artifact, showNewArtifactUI);
        } else {
            Debug.LogWarning("Max artifacts has been reached!");
            PlayerUI.Instance.replaceUI.ShowReplaceUI(GetAllArtifacts(), artifact, ReplaceArtifact, RejectArtifact);
        }
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
        for (int i = 0; i < artifactSlots.Length; i++) {
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
        for (int i = 0; i < artifactSlots.Length; i++) {
            if (artifactSlots[i].artifact == removedArtifact) {
                artifactSlots[i].artifact = null;
                Messenger.Broadcast<Artifact>(Signals.PLAYER_REMOVED_ARTIFACT, removedArtifact);
                break;
            }
        }
    }
    public int GetTotalArtifactCount() {
        int count = 0;
        for (int i = 0; i < artifactSlots.Length; i++) {
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
        for (int i = 0; i < artifactSlots.Length; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && !currArtifact.hasBeenUsed) {
                count++;
            }
        }
        return count;
    }
    public int GetAvailableArtifactsOfTypeCount(ARTIFACT_TYPE type) {
        int count = 0;
        for (int i = 0; i < artifactSlots.Length; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && currArtifact.type == type && !currArtifact.hasBeenUsed) {
                count++;
            }
        }
        return count;
    }
    public bool TryGetAvailableArtifactOfType(ARTIFACT_TYPE type, out Artifact artifact) {
        for (int i = 0; i < artifactSlots.Length; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && currArtifact.type == type && !currArtifact.hasBeenUsed) {
                artifact = currArtifact;
                return true;
            }
        }
        artifact = null;
        return false;
    }
    public bool TryGetAvailableArtifactSlotOfType(ARTIFACT_TYPE type, out ArtifactSlot artifactSlot) {
        for (int i = 0; i < artifactSlots.Length; i++) {
            ArtifactSlot currArtifactSlot = artifactSlots[i];
            if (currArtifactSlot.artifact != null && currArtifactSlot.artifact.type == type && !currArtifactSlot.artifact.hasBeenUsed) {
                artifactSlot = currArtifactSlot;
                return true;
            }
        }
        artifactSlot = null;
        return false;
    }
    public bool HasArtifact(string artifactName) {
        ARTIFACT_TYPE type = (ARTIFACT_TYPE)System.Enum.Parse(typeof(ARTIFACT_TYPE), artifactName);
        for (int i = 0; i < artifactSlots.Length; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null && currArtifact.type.ToString() == artifactName) {
                return true;
            }
        }
        return false;
    }
    private Artifact GetArtifactOfType(ARTIFACT_TYPE type) {
        for (int i = 0; i < artifactSlots.Length; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact.type == type) {
                return currArtifact;
            }
        }
        return null;
    }
    private List<Artifact> GetAllArtifacts() {
        List<Artifact> all = new List<Artifact>();
        for (int i = 0; i < artifactSlots.Length; i++) {
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
                return "Raises all dead characters in the area to attack residents.";
            case ARTIFACT_TYPE.Chaos_Orb:
                return "Characters that inspect the Chaos Orb may be permanently berserked.";
            case ARTIFACT_TYPE.Hermes_Statue:
                return "Characters that inspect this will be teleported to a different settlement. If no other settlement exists, this will be useless.";
            case ARTIFACT_TYPE.Ankh_Of_Anubis:
                return "All characters that moves through here may slowly sink and perish. Higher agility means higher chance of escaping. Sand pit has a limited duration upon placing the artifact.";
            case ARTIFACT_TYPE.Miasma_Emitter:
                return "Characters will avoid the area. If any character gets caught within, they will gain Poisoned status effect. Any objects inside the radius are disabled.";
            default:
                return "Summon a " + Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
        }
    }
    public Artifact GetRandomArtifact() {
        List<Artifact> choices = GetAllArtifacts();
        return choices[UnityEngine.Random.Range(0, choices.Count)];
    }
    private void ResetArtifacts() {
        for (int i = 0; i < artifactSlots.Length; i++) {
            Artifact currArtifact = artifactSlots[i].artifact;
            if (currArtifact != null) {
                currArtifact.Reset();
            }
        }
    }
    public void AdjustArtifactSlot(int adjustment) {
        maxArtifactSlots += adjustment;
        maxArtifactSlots = Mathf.Clamp(maxArtifactSlots, 0, MAX_ARTIFACT);
        //TODO: validate if adjusted max artifacts can accomodate current summons
    }

    public ArtifactSlot GetArtifactSlotByArtifact(Artifact artifact) {
        for (int i = 0; i < artifactSlots.Length; i++) {
            if(artifactSlots[i].artifact == artifact) {
                return artifactSlots[i];
            }
        }
        return null;
    }
    public int GetIndexForArtifactSlot(ArtifactSlot slot) {
        for (int i = 0; i < artifactSlots.Length; i++) {
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
    public void UnlockAnArtifactSlotOrUpgradeExisting() {
        PlayerUI.Instance.levelUpUI.ShowLevelUpUI(null, "artifact_slot");
        //if (AreAllArtifactSlotsMaxLevel()) {
        //    AdjustArtifactSlot(1);
        //    PlayerUI.Instance.ShowGeneralConfirmation("Congratulations!", "You gained 1 Artifact Slot.");
        //} else {
        //    int chance = UnityEngine.Random.Range(0, 2);
        //    if (chance == 0) {
        //        //Unlock slot
        //        AdjustArtifactSlot(1);
        //        PlayerUI.Instance.ShowGeneralConfirmation("Congratulations!", "You gained 1 Artifact Slot.");
        //    } else {
        //        //Upgrade slot
        //        PlayerUI.Instance.levelUpUI.ShowLevelUpUI(null, "artifact_slot");
        //    }
        //}
    }
    #endregion

    #region Invasion
    public void StartInvasion(Area area) {
        currentTargetFaction = area.owner;
        List<LocationGridTile> entrances = new List<LocationGridTile>();
        List<Minion> currentMinions = new List<Minion>();
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if (currMinion != null) {
                currMinion.character.CreateMarker();
                currMinion.character.marker.SetActiveState(false);
                currentMinions.Add(currMinion);
            }
        }

        if(currentMinions.Count > 0) {
            LocationGridTile mainEntrance = area.GetRandomUnoccupiedEdgeTile();
            entrances.Add(mainEntrance);
            int neededEntrances = currentMinions.Count - 1;

            for (int i = 0; i < entrances.Count; i++) {
                for (int j = 0; j < entrances[i].neighbourList.Count; j++) {
                    LocationGridTile newEntrance = entrances[i].neighbourList[j];
                    //if (newEntrance.objHere == null && newEntrance.charactersHere.Count == 0 && newEntrance.structure != null) {
                    if (newEntrance.IsAtEdgeOfWalkableMap() && !entrances.Contains(newEntrance)) {
                        entrances.Add(newEntrance);
                        if (entrances.Count >= currentMinions.Count) {
                            break;
                        }
                    }
                }
                if (entrances.Count >= currentMinions.Count) {
                    break;
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
                currMinion.StartInvasionProtocol();
            }
            PlayerUI.Instance.startInvasionButton.interactable = false;
            currentAreaBeingInvaded = area;
            Messenger.AddListener(Signals.TICK_ENDED, PerTickInvasion);
        } else {
            Debug.LogError("Can't invade! No more minions!");
        }
    }
    private void PerTickInvasion() {
        bool stillHasMinions = false;
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            if(currMinion.character.currentHP > 0) {
                stillHasMinions = true;
                break;
            }
        }
        if (!stillHasMinions) {
            StopInvasion(false);
            return;
        }

        bool stillHasResidents = false;
        for (int i = 0; i < currentTargetFaction.characters.Count; i++) { //Changed checking to faction members, because some characters may still consider the area as their home, but are no longer part of the faction
            Character currCharacter = currentTargetFaction.characters[i];
            if (currCharacter.IsAble() && currCharacter.specificLocation == currentAreaBeingInvaded) {
                stillHasResidents = true;
                break;
            }
        }
        //for (int i = 0; i < currentAreaBeingInvaded.areaResidents.Count; i++) {
        //    Character currCharacter = currentAreaBeingInvaded.areaResidents[i];
        //    if (currCharacter.IsAble() && currCharacter.specificLocation == currentAreaBeingInvaded) {
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
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickInvasion);
        PlayerUI.Instance.StopInvasion();
        if (playerWon) {
            PlayerUI.Instance.SuccessfulAreaCorruption();
            Area corruptedArea = AreaIsCorrupted();
            ResetThreat();
            for (int i = 0; i < corruptedArea.charactersAtLocation.Count; i++) {
                corruptedArea.charactersAtLocation[i].marker.ClearAvoidInRange(false);
                corruptedArea.charactersAtLocation[i].marker.ClearHostilesInRange(false);
                corruptedArea.charactersAtLocation[i].marker.ClearPOIsInVisionRange();
                corruptedArea.charactersAtLocation[i].marker.ClearTerrifyingObjects();
            }
            Messenger.Broadcast(Signals.SUCCESS_INVASION_AREA, corruptedArea);
            ResetSummons();
            ResetArtifacts();
            LevelUpAllMinions();
        } else {
            string gameOverText = "Your minions were wiped out. This settlement is not as weak as you think. You should reconsider your strategy next time.";
            PlayerUI.Instance.GameOver(gameOverText);
        }
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            currMinion.StopInvasionProtocol();
        }
    }
    public void SetInvadingRegion(Region region) {
        invadingRegion = region;
    }
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
            InteriorMapManager.Instance.UnhighlightTiles();
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
            if (currentActiveCombatAbility.CanTarget(InteriorMapManager.Instance.currentlyHoveredPOI)) {
                currentActiveCombatAbility.ActivateAbility(InteriorMapManager.Instance.currentlyHoveredPOI);
            }
        } else {
            List<LocationGridTile> highlightedTiles = InteriorMapManager.Instance.currentlyHighlightedTiles;
            if (highlightedTiles != null) {
                List<IPointOfInterest> poisInHighlightedTiles = new List<IPointOfInterest>();
                for (int i = 0; i < InteriorMapManager.Instance.currentlyShowingArea.charactersAtLocation.Count; i++) {
                    Character currCharacter = InteriorMapManager.Instance.currentlyShowingArea.charactersAtLocation[i];
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
    public void GainNewInterventionAbility(INTERVENTION_ABILITY ability, bool showNewAbilityUI = false) {
        PlayerJobAction playerJobAction = PlayerManager.Instance.CreateNewInterventionAbility(ability);
        GainNewInterventionAbility(playerJobAction, showNewAbilityUI);
    }
    public void GainNewInterventionAbility(PlayerJobAction ability, bool showNewAbilityUI = false) {
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
    public void ConsumeAbility(PlayerJobAction ability) {
        for (int i = 0; i < interventionAbilitySlots.Length; i++) {
            if (interventionAbilitySlots[i].ability == ability) {
                interventionAbilitySlots[i].SetAbility(null);
                Messenger.Broadcast(Signals.PLAYER_CONSUMED_INTERVENE_ABILITY, ability);
                break;
            }
        }
    }
    private void OnReplaceInterventionAbility(object objToReplace, object objToAdd) {
        PlayerJobAction replace = objToReplace as PlayerJobAction;
        PlayerJobAction add = objToAdd as PlayerJobAction;
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
    public List<PlayerJobAction> GetAllInterventionAbilities() {
        List<PlayerJobAction> abilities = new List<PlayerJobAction>();
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
    private void InitializeNewInterventionAbilityCycle() {
        currentNewInterventionAbilityCycleIndex = -1;
        newInterventionAbilityTimerTicks = GameManager.Instance.GetTicksBasedOnHour(8);
        NewCycleForNewInterventionAbility();
    }
    private void PerTickInterventionAbility() {
        currentInterventionAbilityTimerTick++;
        if (currentInterventionAbilityTimerTick >= newInterventionAbilityTimerTicks) {
            int tier = GetTierBasedOnCycle();
            INTERVENTION_ABILITY newAbility = PlayerManager.Instance.GetRandomAbilityByTier(tier);
            if(newAbility != INTERVENTION_ABILITY.ABDUCT) {
                GainNewInterventionAbility(newAbility, true);
            }
            NewCycleForNewInterventionAbility();
        }
    }
    public void NewCycleForNewInterventionAbility() {
        currentInterventionAbilityTimerTick = 0;
        currentNewInterventionAbilityCycleIndex++;
        if (currentNewInterventionAbilityCycleIndex > 3) {
            currentNewInterventionAbilityCycleIndex = 0;
        }
        TimerHubUI.Instance.AddItem("Unlock New Intervention Ability", newInterventionAbilityTimerTicks, null);
        //Messenger.Broadcast(Signals.SHOW_TIMER_HUB_ITEM, "Unlock New Intervention Ability", newInterventionAbilityTimerTicks);
    }
    private int GetTierBasedOnCycle() {
        //Tier Cycle - 3, 3, 2, 1
        if (currentNewInterventionAbilityCycleIndex == 0) return 3;
        else if (currentNewInterventionAbilityCycleIndex == 1) return 3;
        else if (currentNewInterventionAbilityCycleIndex == 2) return 2;
        else if (currentNewInterventionAbilityCycleIndex == 3) return 1;
        return 3;
    }
    #endregion
}