using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance = null;

    public int totalLifestonesInWorld;
    public bool isChoosingStartingTile = false;
    public Player player = null;
    public Character playerCharacter;

    [SerializeField] private Sprite[] _playerAreaFloorSprites;
    [SerializeField] private Sprite[] _playerAreaDefaultStructureSprites;
    [SerializeField] private Sprite _supplySprite, _manaSprite, _impSprite;

    public Dictionary<LANDMARK_TYPE, CurrenyCost> playerStructureTypes = new Dictionary<LANDMARK_TYPE, CurrenyCost>() {
        { LANDMARK_TYPE.DEMONIC_PORTAL, new CurrenyCost{ amount = 0, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.CORRUPTION_NODE, new CurrenyCost{ amount = 50, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.TRAINING_ARENA, new CurrenyCost{ amount = 100, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.RITUAL_CIRCLE, new CurrenyCost{ amount = 200, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.PENANCE_TEMPLE, new CurrenyCost{ amount = 100, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.MANA_EXTRACTOR, new CurrenyCost{ amount = 100, currency = CURRENCY.SUPPLY } },
        { LANDMARK_TYPE.RAMPART, new CurrenyCost{ amount = 0, currency = CURRENCY.SUPPLY } },
    };

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
        player.CreateInitialMinions();
        player.PreAssignJobSlots();
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
        player.CreateInitialMinions();
        player.PreAssignJobSlots();
        LandmarkManager.Instance.OwnArea(player.playerFaction, RACE.DEMON, player.playerArea);
        portal.SetIsBeingInspected(true);
        portal.SetHasBeenInspected(true);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(true);
        PlayerUI.Instance.UpdateUI();
    }

    public void PurchaseTile(HexTile tile) {
        if(player.lifestones > 0) {
            player.AdjustLifestone(-1);
            AddTileToPlayerArea(tile);
        }
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

    public Sprite GetSpriteByCurrency(CURRENCY currency) {
        if(currency == CURRENCY.IMP) {
            return _impSprite;
        }else if (currency == CURRENCY.MANA) {
            return _manaSprite;
        }else if (currency == CURRENCY.SUPPLY) {
            return _supplySprite;
        }
        return null;
    }

    #region Utilities
    public bool CanCreateLandmarkOnTile(LANDMARK_TYPE type, HexTile tile) {
        CurrenyCost cost = playerStructureTypes[type];
        if(player.currencies[cost.currency] >= cost.amount) {
            return true;
        }
        return false;
    }
    #endregion

    #region Minion
    [ContextMenu("Create And Add New Minion")]
    public void CreateMinionForTesting() {
        Minion minion = CreateNewMinion("Pride");
        player.AddMinion(minion);
    }
    public Minion CreateNewMinion(string className, int level = 1) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter(CharacterRole.MINION, className, RACE.HUMANS, GENDER.MALE,
            player.playerFaction, player.playerArea, false), false);
        minion.SetLevel(level);
        return minion;
    }
    #endregion

    #region Attack
    public void AttackLandmark(Area area) {
        DefenderGroup defender = area.GetDefenseGroup();
        if (defender != null) {
            //Combat combat = _assignedMinionAttack.character.currentParty.CreateCombatWith(defender.party);
            //combat.Fight(() => AttackCombatResult(combat));
            CombatManager.Instance.newCombat.StartNewCombat();
            CombatManager.Instance.newCombat.AddCharacters(player.attackGrid, SIDES.A);
            CombatManager.Instance.newCombat.AddCharacters(defender.party.characters, SIDES.B);
            CombatManager.Instance.newCombat.AddEndCombatActions(() => AttackCombatResult(area));
            UIManager.Instance.combatUI.OpenCombatUI(true);
        } else {
            Debug.LogWarning("No defense in area, auto win for the player!");
            area.Death();
        }
    }
    private void AttackCombatResult(Area area) {
        for (int i = 0; i < player.attackGrid.slots.Length; i++) {
            player.attackGrid.slots[i].OccupySlot(CombatManager.Instance.newCombat.leftSide.slots[i].character);
        }
        PlayerUI.Instance.attackSlot.UpdateVisuals();
        if (CombatManager.Instance.newCombat.winningSide == SIDES.A) {
            area.Death();
        }
    }
    #endregion
}
