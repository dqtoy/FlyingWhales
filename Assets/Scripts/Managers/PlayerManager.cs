using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance = null;

    public const int MAX_LEVEL_SUMMON = 3;
    public const int MAX_LEVEL_ARTIFACT = 3;
    public const int MAX_LEVEL_COMBAT_ABILITY = 3;
    public const int MAX_LEVEL_INTERVENTION_ABILITY = 3;

    public bool isChoosingStartingTile = false;
    public Player player = null;
    public INTERVENTION_ABILITY[] allInterventionAbilities;
    public COMBAT_ABILITY[] allCombatAbilities;

    [SerializeField] private Sprite[] _playerAreaFloorSprites;
    [SerializeField] private LandmarkStructureSprite[] _playerAreaDefaultStructureSprites;

    [Header("Job Action Icons")]
    [SerializeField] private StringSpriteDictionary jobActionIcons;

    [Header("Combat Ability Icons")]
    [SerializeField] private StringSpriteDictionary combatAbilityIcons;

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
        //allInterventionAbilities = new INTERVENTION_ABILITY[] { INTERVENTION_ABILITY.EXPLOSION, INTERVENTION_ABILITY.IGNITE };
        allInterventionAbilities = new INTERVENTION_ABILITY[] { INTERVENTION_ABILITY.ZAP, INTERVENTION_ABILITY.RAISE_DEAD, INTERVENTION_ABILITY.INFLICT_CANNIBALISM
            , INTERVENTION_ABILITY.CLOAK_OF_INVISIBILITY, INTERVENTION_ABILITY.INFLICT_LYCANTHROPY, INTERVENTION_ABILITY.INFLICT_VAMPIRISM, INTERVENTION_ABILITY.INFLICT_KLEPTOMANIA
            , INTERVENTION_ABILITY.INFLICT_UNFAITHFULNESS, INTERVENTION_ABILITY.JOLT, INTERVENTION_ABILITY.ENRAGE, INTERVENTION_ABILITY.PROVOKE, INTERVENTION_ABILITY.EXPLOSION
            , INTERVENTION_ABILITY.IGNITE, INTERVENTION_ABILITY.LURE,};
        //allInterventionAbilities = (INTERVENTION_ABILITY[]) System.Enum.GetValues(typeof(INTERVENTION_ABILITY));
        allCombatAbilities = (COMBAT_ABILITY[]) System.Enum.GetValues(typeof(COMBAT_ABILITY));

        //Unit Selection
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressedDown);
    }

    public void LoadStartingTile() {
        BaseLandmark portal = LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.DEMONIC_PORTAL);
        if (portal == null) {
            //choose a starting tile
            ChooseStartingTile();
        } else {
            OnLoadStartingTile(portal);
        }
    }
    public void ChooseStartingTile() {
        Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Pick a starting tile", false);
        isChoosingStartingTile = true;
        Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnChooseStartingTile);
        UIManager.Instance.SetTimeControlsState(false);
    }
    private void OnChooseStartingTile(HexTile tile) {
        if (tile.areaOfTile != null || tile.landmarkOnTile != null) {
            Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "That is not a valid starting tile!", false);
            return;
        }
        player = new Player();
        PlayerUI.Instance.Initialize();
        player.CreatePlayerFaction();
        player.CreatePlayerArea(tile);
        //player.SetMaxMinions(9);
        //player.CreateInitialMinions();
        //player.PreAssignJobSlots();
        LandmarkManager.Instance.OwnArea(player.playerFaction, RACE.DEMON, player.playerArea);
        Messenger.RemoveListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnChooseStartingTile);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        GameManager.Instance.StartProgression();
        isChoosingStartingTile = false;
        UIManager.Instance.SetTimeControlsState(true);
        PlayerUI.Instance.UpdateUI();
        //PlayerUI.Instance.InitializeThreatMeter();
        //LandmarkManager.Instance.CreateNewArea(tile, AREA_TYPE.DEMONIC_INTRUSION);
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
        LandmarkManager.Instance.OwnArea(player.playerFaction, RACE.DEMON, player.playerArea);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(true);
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
        for (int i = 0; i < portal.sameRowTiles.Count; i++) {
            HexTile sameRowTile = portal.sameRowTiles[i];
            player.playerArea.AddTile(sameRowTile);
            sameRowTile.SetCorruption(true);
        }
        LandmarkManager.Instance.OwnArea(player.playerFaction, RACE.DEMON, player.playerArea);
        PlayerUI.Instance.UpdateUI();
        //PlayerUI.Instance.InitializeThreatMeter();
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
        for (int i = 0; i < data.summonIDs.Count; i++) {
            Summon summon = CharacterManager.Instance.GetCharacterByID(data.summonIDs[i]) as Summon;
            player.AddASummon(summon);
        }
        for (int i = 0; i < data.artifacts.Count; i++) {
            data.artifacts[i].Load(player);
        }

        for (int i = 0; i < player.minions.Length; i++) {
            if(player.minions[i] != null && player.minions[i].character.id == data.currentMinionLeaderID) {
                player.SetMinionLeader(player.minions[i]);
            }
        }
    }

    public void PurchaseTile(HexTile tile) {
        AddTileToPlayerArea(tile);
    }
    public void AddTileToPlayerArea(HexTile tile) {
        player.playerArea.AddTile(tile);
        tile.SetCorruption(true);
        for (int i = 0; i < tile.landmarkOnTile.sameRowTiles.Count; i++) {
            HexTile sameRowTile = tile.landmarkOnTile.sameRowTiles[i];
            player.playerArea.AddTile(sameRowTile);
            sameRowTile.SetCorruption(true);
        }
        //tile.StopCorruptionAnimation();
    }

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
            case INTERVENTION_ABILITY.INFLICT_KLEPTOMANIA:
                return new CorruptKleptomaniac();
            case INTERVENTION_ABILITY.INFLICT_LYCANTHROPY:
                return new CorruptLycanthropy();
            case INTERVENTION_ABILITY.INFLICT_UNFAITHFULNESS:
                return new CorruptUnfaithful();
            case INTERVENTION_ABILITY.INFLICT_VAMPIRISM:
                return new CorruptVampiric();
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
            case INTERVENTION_ABILITY.INFLICT_CANNIBALISM:
                return new InflictCannibalism();
            case INTERVENTION_ABILITY.CLOAK_OF_INVISIBILITY:
                return new CloakOfInvisibility();
            case INTERVENTION_ABILITY.LURE:
                return new Lure();
            case INTERVENTION_ABILITY.EXPLOSION:
                return new Explosion();
            case INTERVENTION_ABILITY.IGNITE:
                return new Ignite();
        }
        return null;
    }
    public List<INTERVENTION_ABILITY> GetInterventionAbilitiesWithTag(ABILITY_TAG tag) {
        List<INTERVENTION_ABILITY> valid = new List<INTERVENTION_ABILITY>();
        INTERVENTION_ABILITY[] abilities = Utilities.GetEnumValues<INTERVENTION_ABILITY>();
        for (int i = 0; i < abilities.Length; i++) {
            INTERVENTION_ABILITY currAbility = abilities[i];
            List<ABILITY_TAG> tags = currAbility.GetAbilityTags();
            if (tags.Contains(tag)) {
                valid.Add(currAbility);
            }
        }
        return valid;
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
    public Artifact CreateNewArtifact(SaveDataArtifact data) {
        Artifact newArtifact = CreateNewArtifactClassFromType(data);
        newArtifact.SetLevel(data.level);
        return newArtifact;
    }
    private object CreateNewArtifactClassFromType(ARTIFACT_TYPE artifactType) {
        var typeName = artifactType.ToString();
        return System.Activator.CreateInstance(System.Type.GetType(typeName));
    }

    private Artifact CreateNewArtifactClassFromType(SaveDataArtifact data) {
        switch (data.type) {
            case ARTIFACT_TYPE.Ankh_Of_Anubis:
                return new Ankh_Of_Anubis(data);
            case ARTIFACT_TYPE.Chaos_Orb:
                return new Chaos_Orb(data);
            case ARTIFACT_TYPE.Hermes_Statue:
                return new Hermes_Statue(data);
            case ARTIFACT_TYPE.Miasma_Emitter:
                return new Miasma_Emitter(data);
            case ARTIFACT_TYPE.Necronomicon:
                return new Necronomicon(data);
        }
        return null;
    }
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
                    character.StopCurrentAction(false);
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
}
