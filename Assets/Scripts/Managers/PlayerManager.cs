using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance = null;

    public int totalLifestonesInWorld;
    public bool isChoosingStartingTile = false;
    public Player player = null;
    public Character playerCharacter;
    public INTERVENTION_ABILITY[] allInterventionAbilities;

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
        allInterventionAbilities = (INTERVENTION_ABILITY[]) System.Enum.GetValues(typeof(INTERVENTION_ABILITY));
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
        portal.SetIsBeingInspected(true);
        portal.SetHasBeenInspected(true);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(true);
        PlayerUI.Instance.UpdateUI();
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
        portal.SetIsBeingInspected(true);
        portal.SetHasBeenInspected(true);
        PlayerUI.Instance.UpdateUI();
    }


    public void PurchaseTile(HexTile tile) {
        AddTileToPlayerArea(tile);
    }
    public void AddTileToPlayerArea(HexTile tile) {
        player.playerArea.AddTile(tile);
        tile.SetCorruption(true);
        tile.StopCorruptionAnimation();
    }
    public void CreatePlayerLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        if(CanCreateLandmarkOnTile(landmarkType, location)) {
            player.AdjustCurrency(playerStructureTypes[landmarkType].currency, -playerStructureTypes[landmarkType].amount);
            BaseLandmark landmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(location, landmarkType);
            OnPlayerLandmarkCreated(landmark);
        }
    }

    private void OnPlayerLandmarkCreated(BaseLandmark newLandmark) {
        newLandmark.SetIsBeingInspected(true);
        newLandmark.SetHasBeenInspected(true);
        switch (newLandmark.specificLandmarkType) {
            //case LANDMARK_TYPE.SNATCHER_DEMONS_LAIR:
            //    //player.AdjustSnatchCredits(1);
            //    break;
            case LANDMARK_TYPE.DWELLINGS:
                //add 2 minion slots
                //player.AdjustMaxMinions(2);
                break;
            case LANDMARK_TYPE.IMP_KENNEL:
                //adds 1 Imp capacity
                //player.AdjustMaxImps(1);
                break;
            case LANDMARK_TYPE.RAMPART:
                //bonus 25% HP to all Defenders
                //for (int i = 0; i < player.playerArea.landmarks.Count; i++) {
                //    BaseLandmark currLandmark = player.playerArea.landmarks[i];
                //    currLandmark.AddDefenderBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //if (currLandmark.defenders != null) {
                    //    currLandmark.defenders.AddBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //}
                //}
                break;
            default:
                break;
        }
        //player.playerArea.DetermineExposedTiles();
        Messenger.Broadcast(Signals.PLAYER_LANDMARK_CREATED, newLandmark);
    }

    public void AdjustTotalLifestones(int amount) {
        totalLifestonesInWorld += amount;
        Debug.Log("Adjusted lifestones in world by " + amount + ". New total is " + totalLifestonesInWorld);
    }

    #region Utilities
    public bool CanCreateLandmarkOnTile(LANDMARK_TYPE type, HexTile tile) {
        CurrenyCost cost = playerStructureTypes[type];
        if(player.currencies[cost.currency] >= cost.amount) {
            return true;
        }
        return false;
    }
    public Sprite GetJobActionSprite(string actionName) {
        if (jobActionIcons.ContainsKey(actionName)) {
            return jobActionIcons[actionName];
        }
        return null;
    }
    #endregion

    #region Intervention Abilities
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
            case INTERVENTION_ABILITY.SHARE_INTEL:
                return new ShareIntel();
            case INTERVENTION_ABILITY.SPOOK:
                return new Spook();
            case INTERVENTION_ABILITY.ZAP:
                return new Zap();
        }
        return null;
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
