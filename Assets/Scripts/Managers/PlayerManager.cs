using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance = null;

    public const int MAX_LEVEL_SUMMON = 3;
    public const int MAX_LEVEL_ARTIFACT = 3;
    public const int MAX_LEVEL_COMBAT_ABILITY = 3;
    public const int MAX_LEVEL_INTERVENTION_ABILITY = 3;
    public const int DIVINE_INTERVENTION_DURATION = 4320;

    public bool isChoosingStartingTile = false;
    public Player player = null;
    public INTERVENTION_ABILITY[] allInterventionAbilities;
    public Dictionary<INTERVENTION_ABILITY, PlayerJobActionData> allInterventionAbilitiesData;
    public COMBAT_ABILITY[] allCombatAbilities;
    public LANDMARK_TYPE[] allLandmarksThatCanBeBuilt;

    [SerializeField] private Sprite[] _playerAreaFloorSprites;
    [SerializeField] private LandmarkStructureSprite[] _playerAreaDefaultStructureSprites;

    [Header("Job Action Icons")]
    [SerializeField] private StringSpriteDictionary jobActionIcons;

    [Header("Combat Ability Icons")]
    [SerializeField] private StringSpriteDictionary combatAbilityIcons;

    [Header("Intervention Ability Tiers")]
    [SerializeField] private InterventionAbilityTierDictionary interventionAbilityTiers;

    #region getters/setters
    public Sprite[] playerAreaFloorSprites {
        get { return _playerAreaFloorSprites; }
    }
    public LandmarkStructureSprite[] playerAreaDefaultStructureSprites {
        get { return _playerAreaDefaultStructureSprites; }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }
    public void Initialize() {
        // , INTERVENTION_ABILITY.CLOAK_OF_INVISIBILITY
        allInterventionAbilities = new INTERVENTION_ABILITY[] { INTERVENTION_ABILITY.ZAP, INTERVENTION_ABILITY.RAISE_DEAD, INTERVENTION_ABILITY.CANNIBALISM
            , INTERVENTION_ABILITY.LYCANTHROPY, INTERVENTION_ABILITY.VAMPIRISM, INTERVENTION_ABILITY.KLEPTOMANIA
            , INTERVENTION_ABILITY.UNFAITHFULNESS, INTERVENTION_ABILITY.ENRAGE, INTERVENTION_ABILITY.PROVOKE, INTERVENTION_ABILITY.EXPLOSION
            , INTERVENTION_ABILITY.IGNITE, INTERVENTION_ABILITY.LURE, INTERVENTION_ABILITY.CURSED_OBJECT, INTERVENTION_ABILITY.SPOIL, INTERVENTION_ABILITY.ALCOHOLIC
            , INTERVENTION_ABILITY.LULLABY, INTERVENTION_ABILITY.AGORAPHOBIA, INTERVENTION_ABILITY.PARALYSIS, INTERVENTION_ABILITY.RELEASE, INTERVENTION_ABILITY.ZOMBIE_VIRUS
            , INTERVENTION_ABILITY.PESTILENCE, INTERVENTION_ABILITY.PSYCHOPATHY, INTERVENTION_ABILITY.TORNADO }; //INTERVENTION_ABILITY.JOLT, , INTERVENTION_ABILITY.CLOAK_OF_INVISIBILITY//
        allCombatAbilities = (COMBAT_ABILITY[]) System.Enum.GetValues(typeof(COMBAT_ABILITY));

        allInterventionAbilitiesData = new Dictionary<INTERVENTION_ABILITY, PlayerJobActionData>();
        for (int i = 0; i < allInterventionAbilities.Length; i++) {
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(allInterventionAbilities[i].ToString()) + "Data";
            allInterventionAbilitiesData.Add(allInterventionAbilities[i], System.Activator.CreateInstance(System.Type.GetType(typeName)) as PlayerJobActionData);
        }

        allLandmarksThatCanBeBuilt = new LANDMARK_TYPE[] { LANDMARK_TYPE.THE_ANVIL, LANDMARK_TYPE.THE_EYE , LANDMARK_TYPE.THE_KENNEL, LANDMARK_TYPE.THE_CRYPT, LANDMARK_TYPE.THE_SPIRE, LANDMARK_TYPE.THE_NEEDLES, LANDMARK_TYPE.THE_PROFANE, LANDMARK_TYPE.THE_PIT, LANDMARK_TYPE.THE_FINGERS };
        //Unit Selection
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressedDown);
    }

    public void LoadStartingTile() {
        BaseLandmark portal = LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.THE_PORTAL);
        OnLoadStartingTile(portal);
    }
    private void OnLoadStartingTile(BaseLandmark portal) {
        player = new Player();
        PlayerUI.Instance.Initialize();
        player.CreatePlayerFaction();
        Area existingPlayerArea = LandmarkManager.Instance.GetAreaByName("Portal");
        if (existingPlayerArea == null) {
            player.CreatePlayerArea(portal);
        } else {
            player.LoadPlayerArea(existingPlayerArea);
        }
        //player.SetMaxMinions(9);
        //player.CreateInitialMinions();
        //player.PreAssignJobSlots();
        LandmarkManager.Instance.OwnRegion(player.playerFaction, RACE.DEMON, portal.tileLocation.region);
        player.SetPlayerTargetFaction(LandmarkManager.Instance.enemyOfPlayerArea.owner);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetSpeedTogglesState(true);
        PlayerUI.Instance.UpdateUI();
        //PlayerUI.Instance.InitializeThreatMeter();
    }
    public void InitializePlayer(BaseLandmark portal) {
        player = new Player();
        PlayerUI.Instance.Initialize();
        player.CreatePlayerFaction();
        Area existingPlayerArea = LandmarkManager.Instance.GetAreaByName("Portal");
        if (existingPlayerArea == null) {
            player.CreatePlayerArea(portal);
        } else {
            player.LoadPlayerArea(existingPlayerArea);
        }
        //for (int i = 0; i < portal.tileLocation.region.tiles.Count; i++) {
        //    HexTile regionTile = portal.tileLocation.region.tiles[i];
        //    //player.playerArea.AddTile(regionTile);
        //    regionTile.SetCorruption(true);
        //}
        LandmarkManager.Instance.OwnRegion(player.playerFaction, RACE.DEMON, portal.tileLocation.region);
        player.SetPlayerTargetFaction(LandmarkManager.Instance.enemyOfPlayerArea.owner);
        PlayerUI.Instance.UpdateUI();

        //Add an adjacent region to the player at the start of the game.
        //Ref: https://trello.com/c/cQKzEx06/2699-one-additional-empty-region-owned-by-the-player-at-the-start-of-game
        List<Region> choices = portal.tileLocation.region.connections;
        Region chosenRegion = choices[Random.Range(0, choices.Count)];
        LandmarkManager.Instance.OwnRegion(player.playerFaction, RACE.DEMON, chosenRegion);
        //Pre-build a Spire in the second initial empty corrupted region and ensure that it does not have a Hallowed Ground trait.
        chosenRegion.RemoveAllFeatures();
        BaseLandmark newLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenRegion.coreTile, LANDMARK_TYPE.THE_SPIRE, false);

    }
    public void InitializePlayer(SaveDataPlayer data) {
        player = new Player(data);
        PlayerUI.Instance.Initialize();
        player.CreatePlayerFaction(data);
        Area existingPlayerArea = LandmarkManager.Instance.GetAreaByID(data.playerAreaID);
        player.SetPlayerArea(existingPlayerArea);
        //PlayerUI.Instance.UpdateUI();
        //PlayerUI.Instance.InitializeThreatMeter();
        //PlayerUI.Instance.UpdateThreatMeter();

        for (int i = 0; i < data.minions.Count; i++) {
            data.minions[i].Load(player);
        }
        //for (int i = 0; i < data.summonSlots.Count; i++) {
        //    Summon summon = CharacterManager.Instance.GetCharacterByID(data.summonIDs[i]) as Summon;
        //    player.GainSummon(summon);
        //}
        //for (int i = 0; i < data.artifacts.Count; i++) {
        //    data.artifacts[i].Load(player);
        //}
        //for (int i = 0; i < data.interventionAbilities.Count; i++) {
        //    data.interventionAbilities[i].Load(player);
        //}
        for (int i = 0; i < player.minions.Count; i++) {
            if(player.minions[i].character.id == data.currentMinionLeaderID) {
                player.SetMinionLeader(player.minions[i]);
            }
        }
        player.SetPlayerTargetFaction(LandmarkManager.Instance.enemyOfPlayerArea.owner);
    }
    //public void AddTileToPlayerArea(HexTile tile) {
    //    player.playerArea.AddTile(tile);
    //    tile.SetCorruption(true);
    //    for (int i = 0; i < tile.region.tiles.Count; i++) {
    //        HexTile regionTile = tile.region.tiles[i];
    //        player.playerArea.AddTile(regionTile);
    //        regionTile.SetCorruption(true);
    //    }
    //    //tile.StopCorruptionAnimation();
    //}

    #region Utilities
    public Sprite GetJobActionSprite(string actionName) {
        if (jobActionIcons.ContainsKey(actionName)) {
            return jobActionIcons[actionName];
        }
        return null;
    }
    public Sprite GetCombatAbilitySprite(string abilityName) {
        if (combatAbilityIcons.ContainsKey(abilityName)) {
            return combatAbilityIcons[abilityName];
        }
        return null;
    }
    #endregion

    #region Intervention Ability
    public PlayerJobAction CreateNewInterventionAbility(INTERVENTION_ABILITY abilityType) {
        switch (abilityType) {
            case INTERVENTION_ABILITY.ABDUCT:
                return new Abduct();
            case INTERVENTION_ABILITY.ACCESS_MEMORIES:
                return new AccessMemories();
            case INTERVENTION_ABILITY.DESTROY:
                return new Destroy();
            case INTERVENTION_ABILITY.DISABLE:
                return new Disable();
            case INTERVENTION_ABILITY.ENRAGE:
                return new Enrage();
            case INTERVENTION_ABILITY.KLEPTOMANIA:
                return new Kleptomania();
            case INTERVENTION_ABILITY.LYCANTHROPY:
                return new Lycanthropy();
            case INTERVENTION_ABILITY.UNFAITHFULNESS:
                return new Unfaithfulness();
            case INTERVENTION_ABILITY.VAMPIRISM:
                return new Vampirism();
            case INTERVENTION_ABILITY.JOLT:
                return new Jolt();
            case INTERVENTION_ABILITY.PROVOKE:
                return new Provoke();
            case INTERVENTION_ABILITY.RAISE_DEAD:
                return new RaiseDead();
            case INTERVENTION_ABILITY.RILE_UP:
                return new RileUp();
            //case INTERVENTION_ABILITY.SHARE_INTEL:
            //    return new ShareIntel();
            case INTERVENTION_ABILITY.SPOOK:
                return new Spook();
            case INTERVENTION_ABILITY.ZAP:
                return new Zap();
            case INTERVENTION_ABILITY.CANNIBALISM:
                return new Cannibalism();
            case INTERVENTION_ABILITY.CLOAK_OF_INVISIBILITY:
                return new CloakOfInvisibility();
            case INTERVENTION_ABILITY.LURE:
                return new Lure();
            case INTERVENTION_ABILITY.EXPLOSION:
                return new Explosion();
            case INTERVENTION_ABILITY.IGNITE:
                return new Ignite();
            case INTERVENTION_ABILITY.CURSED_OBJECT:
                return new CursedObject();
            case INTERVENTION_ABILITY.SPOIL:
                return new Spoil();
            case INTERVENTION_ABILITY.ALCOHOLIC:
                return new Alcoholic();
            case INTERVENTION_ABILITY.LULLABY:
                return new Lullaby();
            case INTERVENTION_ABILITY.PESTILENCE:
                return new Pestilence();
            case INTERVENTION_ABILITY.AGORAPHOBIA:
                return new Agoraphobia();
            case INTERVENTION_ABILITY.PARALYSIS:
                return new Paralysis();
            case INTERVENTION_ABILITY.RELEASE:
                return new Release();
            case INTERVENTION_ABILITY.ZOMBIE_VIRUS:
                return new ZombieVirus();
            case INTERVENTION_ABILITY.PSYCHOPATHY:
                return new Psychopathy();
            case INTERVENTION_ABILITY.TORNADO:
                return new Tornado();
        }
        return null;
    }
    public List<INTERVENTION_ABILITY> GetInterventionAbilitiesWithTag(ABILITY_TAG tag) {
        List<INTERVENTION_ABILITY> valid = new List<INTERVENTION_ABILITY>();
        INTERVENTION_ABILITY[] abilities = allInterventionAbilities;
        for (int i = 0; i < abilities.Length; i++) {
            INTERVENTION_ABILITY currAbility = abilities[i];
            List<ABILITY_TAG> tags = currAbility.GetAbilityTags();
            if (tags.Contains(tag)) {
                valid.Add(currAbility);
            }
        }
        return valid;
    }
    public int GetInterventionAbilityTier(INTERVENTION_ABILITY abilityType) {
        if (interventionAbilityTiers.ContainsKey(abilityType)) {
            return interventionAbilityTiers[abilityType];
        }
        return 3;
    }
    public INTERVENTION_ABILITY GetRandomAbilityByTier(int tier) {
        List<INTERVENTION_ABILITY> abilityTiers = new List<INTERVENTION_ABILITY>();
        for (int i = 0; i < allInterventionAbilities.Length; i++) {
            INTERVENTION_ABILITY ability = allInterventionAbilities[i];
            if (GetInterventionAbilityTier(ability) == tier) {
                abilityTiers.Add(ability);
            }
        }
        if (abilityTiers.Count > 0) {
            return abilityTiers[UnityEngine.Random.Range(0, abilityTiers.Count)];
        }
        return INTERVENTION_ABILITY.ABDUCT;
    }
    public List<INTERVENTION_ABILITY> GetAbilitiesByTier(int tier) {
        List<INTERVENTION_ABILITY> abilityTiers = new List<INTERVENTION_ABILITY>();
        for (int i = 0; i < allInterventionAbilities.Length; i++) {
            INTERVENTION_ABILITY ability = allInterventionAbilities[i];
            if (GetInterventionAbilityTier(ability) == tier) {
                abilityTiers.Add(ability);
            }
        }
        return abilityTiers;
    }
    public List<INTERVENTION_ABILITY> GetAllInterventionAbilityByCategory(INTERVENTION_ABILITY_CATEGORY category) {
        List<INTERVENTION_ABILITY> abilities = new List<INTERVENTION_ABILITY>();
        for (int i = 0; i < allInterventionAbilities.Length; i++) {
            INTERVENTION_ABILITY ability = allInterventionAbilities[i];
            if (allInterventionAbilitiesData[ability].category == category) {
                abilities.Add(ability);
            }
        }
        return abilities;
    }
    #endregion

    #region Combat Ability
    public CombatAbility CreateNewCombatAbility(COMBAT_ABILITY abilityType) {
        switch (abilityType) {
            case COMBAT_ABILITY.SINGLE_HEAL:
                return new SingleHeal();
            case COMBAT_ABILITY.FLAMESTRIKE:
                return new Flamestrike();
            case COMBAT_ABILITY.FEAR_SPELL:
                return new FearSpellAbility();
            case COMBAT_ABILITY.SACRIFICE:
                return new Sacrifice();
            case COMBAT_ABILITY.TAUNT:
                return new Taunt();
        }
        return null;
    }
    #endregion

    #region Artifacts
    public Artifact CreateNewArtifact(ARTIFACT_TYPE artifactType) {
        Artifact newArtifact = CreateNewArtifactClassFromType(artifactType) as Artifact;
        return newArtifact;
    }
    //public Artifact CreateNewArtifact(SaveDataArtifactSlot data) {
    //    Artifact newArtifact = CreateNewArtifactClassFromType(data);
    //    newArtifact.SetLevel(data.level);
    //    return newArtifact;
    //}
    public Artifact CreateNewArtifact(SaveDataArtifact data) {
        Artifact newArtifact = CreateNewArtifactClassFromType(data) as Artifact;
        return newArtifact;
    }
    private object CreateNewArtifactClassFromType(ARTIFACT_TYPE artifactType) {
        var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(artifactType.ToString());
        return System.Activator.CreateInstance(System.Type.GetType(typeName));
    }
    private object CreateNewArtifactClassFromType(SaveDataArtifact data) {
        var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(data.artifactType.ToString());
        return System.Activator.CreateInstance(System.Type.GetType(typeName), data);
    }
    //private Artifact CreateNewArtifactClassFromType(SaveDataArtifactSlot data) {
    //    switch (data.type) {
    //        case ARTIFACT_TYPE.Ankh_Of_Anubis:
    //            return new Ankh_Of_Anubis(data);
    //        case ARTIFACT_TYPE.Chaos_Orb:
    //            return new Chaos_Orb(data);
    //        case ARTIFACT_TYPE.Hermes_Statue:
    //            return new Hermes_Statue(data);
    //        case ARTIFACT_TYPE.Miasma_Emitter:
    //            return new Miasma_Emitter(data);
    //        case ARTIFACT_TYPE.Necronomicon:
    //            return new Necronomicon(data);
    //    }
    //    return null;
    //}
    #endregion

    #region Unit Selection
    private List<Character> selectedUnits = new List<Character>();
    public void SelectUnit(Character character) {
        if (!selectedUnits.Contains(character)) {
            selectedUnits.Add(character);
        }
    }
    public void DeselectUnit(Character character) {
        if (selectedUnits.Remove(character)) {

        }
    }
    public void DeselectAllUnits() {
        Character[] units = selectedUnits.ToArray();
        for (int i = 0; i < units.Length; i++) {
            DeselectUnit(units[i]);
        }
    }
    private void OnMenuOpened(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            DeselectAllUnits();
            CharacterInfoUI infoUI = menu as CharacterInfoUI;
            SelectUnit(infoUI.activeCharacter);
            //if (infoUI.activeCharacter.CanBeInstructedByPlayer()) {
            //    SelectUnit(infoUI.activeCharacter);
            //}
        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            DeselectAllUnits();
        }
    }
    private void OnKeyPressedDown(KeyCode keyCode) {
        if (selectedUnits.Count > 0) {
            if (keyCode == KeyCode.Mouse1) {
                //right click
                for (int i = 0; i < selectedUnits.Count; i++) {
                    Character character = selectedUnits[i];
                    if (!character.CanBeInstructedByPlayer()) {
                        continue;
                    }
                    IPointOfInterest hoveredPOI = InteriorMapManager.Instance.currentlyHoveredPOI;
                    character.StopCurrentAction(false, "Stopped by the player");
                    if (character.stateComponent.currentState != null) {
                        character.stateComponent.currentState.OnExitThisState();
                    }
                    character.marker.ClearHostilesInRange();
                    character.marker.ClearAvoidInRange();
                    character.SetIsFollowingPlayerInstruction(false); //need to reset before giving commands
                    if (hoveredPOI is Character) {
                        Character target = hoveredPOI as Character;
                        if (character.IsHostileWith(target) && character.IsCombatReady()) {
                            character.marker.AddHostileInRange(target);
                            CombatState cs = character.stateComponent.currentState as CombatState;
                            if (cs != null) {
                                cs.SetForcedTarget(target);
                            } else {
                                throw new System.Exception(character.name + " was instructed to attack " + target.name + " but did not enter combat state!");
                            }
                        } else {
                            Debug.Log(character.name + " is not combat ready or is not hostile with " + target.name + ". Ignoring command.");
                        }
                    } else {
                        character.marker.GoTo(InteriorMapManager.Instance.currentlyShowingMap.worldUICanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition), () => OnFinishInstructionFromPlayer(character));
                    }
                    character.SetIsFollowingPlayerInstruction(true);
                }
            } else if (keyCode == KeyCode.Mouse0) {
                DeselectAllUnits();
            }
        }
    }
    private void OnFinishInstructionFromPlayer(Character character) {
        character.SetIsFollowingPlayerInstruction(false);
    }
    #endregion

    #region Special Objects
    public SpecialObject CreateNewSpecialObject(string typeName) {
        return System.Activator.CreateInstance(System.Type.GetType(typeName)) as SpecialObject;
    }
    #endregion
}
