using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance = null;

    public const int MAX_LEVEL_SUMMON = 3;
    public const int MAX_LEVEL_ARTIFACT = 3;
    public const int MAX_LEVEL_COMBAT_ABILITY = 3;
    public const int MAX_LEVEL_INTERVENTION_ABILITY = 3;

    public int totalLifestonesInWorld;
    public bool isChoosingStartingTile = false;
    public Player player = null;
    public Character playerCharacter;
    public INTERVENTION_ABILITY[] allInterventionAbilities;
    public COMBAT_ABILITY[] allCombatAbilities;

    [SerializeField] private Sprite[] _playerAreaFloorSprites;
    [SerializeField] private Sprite[] _playerAreaDefaultStructureSprites;

    public Dictionary<LANDMARK_TYPE, CurrenyCost> playerStructureTypes = new Dictionary<LANDMARK_TYPE, CurrenyCost>() {
        { LANDMARK_TYPE.DEMONIC_PORTAL, new CurrenyCost{ amount = 0, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.CORRUPTION_NODE, new CurrenyCost{ amount = 50, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.TRAINING_ARENA, new CurrenyCost{ amount = 100, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.RITUAL_CIRCLE, new CurrenyCost{ amount = 200, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.PENANCE_TEMPLE, new CurrenyCost{ amount = 100, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.MANA_EXTRACTOR, new CurrenyCost{ amount = 100, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.RAMPART, new CurrenyCost{ amount = 0, currency = CURRENCY.SUPPLY } },
    };

    [Header("Job Action Icons")]
    [SerializeField] private StringSpriteDictionary jobActionIcons;

    [Header("Combat Ability Icons")]
    [SerializeField] private StringSpriteDictionary combatAbilityIcons;

    #region getters/setters
    public Sprite[] playerAreaFloorSprites {
        get { return _playerAreaFloorSprites; }
    }
    public Sprite[] playerAreaDefaultStructureSprites {
        get { return _playerAreaDefaultStructureSprites; }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }
    public void Initialize() {
        //allInterventionAbilities = new INTERVENTION_ABILITY[] { INTERVENTION_ABILITY.INFLICT_CANNIBALISM };
        allInterventionAbilities = new INTERVENTION_ABILITY[] { INTERVENTION_ABILITY.ZAP, INTERVENTION_ABILITY.RAISE_DEAD, INTERVENTION_ABILITY.INFLICT_CANNIBALISM
            , INTERVENTION_ABILITY.CLOAK_OF_INVISIBILITY, INTERVENTION_ABILITY.INFLICT_LYCANTHROPY, INTERVENTION_ABILITY.INFLICT_VAMPIRISM, INTERVENTION_ABILITY.INFLICT_KLEPTOMANIA
            , INTERVENTION_ABILITY.INFLICT_UNFAITHFULNESS, INTERVENTION_ABILITY.JOLT, INTERVENTION_ABILITY.ENRAGE, INTERVENTION_ABILITY.PROVOKE };
        //allInterventionAbilities = (INTERVENTION_ABILITY[]) System.Enum.GetValues(typeof(INTERVENTION_ABILITY));
        allCombatAbilities = (COMBAT_ABILITY[]) System.Enum.GetValues(typeof(COMBAT_ABILITY));
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
        if (tile.areaOfTile != null || tile.landmarkOnTile != null || !tile.isPassable) {
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
        PlayerUI.Instance.InitializeThreatMeter();
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
        PlayerUI.Instance.InitializeThreatMeter();
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
        LandmarkManager.Instance.OwnArea(player.playerFaction, RACE.DEMON, player.playerArea);
        PlayerUI.Instance.UpdateUI();
        PlayerUI.Instance.InitializeThreatMeter();
    }


    public void PurchaseTile(HexTile tile) {
        AddTileToPlayerArea(tile);
    }
    public void AddTileToPlayerArea(HexTile tile) {
        player.playerArea.AddTile(tile);
        tile.SetCorruption(true);
        tile.StopCorruptionAnimation();
    }
    public void AdjustTotalLifestones(int amount) {
        totalLifestonesInWorld += amount;
        Debug.Log("Adjusted lifestones in world by " + amount + ". New total is " + totalLifestonesInWorld);
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
    private object CreateNewArtifactClassFromType(ARTIFACT_TYPE artifactType) {
        var typeName = artifactType.ToString();
        return System.Activator.CreateInstance(System.Type.GetType(typeName));
    }
    #endregion

    //#region Minion
    //[ContextMenu("Create And Add New Minion")]
    //public void CreateMinionForTesting() {
    //    Minion minion = CreateNewMinion("Pride");
    //    player.AddMinion(minion);
    //}
    //public Minion CreateNewMinion(string className, int level = 1) {
    //    Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, RACE.HUMANS, GENDER.MALE,
    //        player.playerFaction, player.playerArea), false);
    //    minion.SetLevel(level);
    //    return minion;
    //}
    //#endregion
}
