using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance = null;

    public int totalLifestonesInWorld;
    public bool isChoosingStartingTile = false;
    public Player player = null;
    public Character playerCharacter;

    [SerializeField] private Sprite[] _playerAreaFloorSprites;
    [SerializeField] private Sprite[] _playerAreaDefaultStructureSprites;

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
        Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Pick a starting tile", MESSAGE_BOX_MODE.MESSAGE_ONLY, false);
        isChoosingStartingTile = true;
        Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnChooseStartingTile);
        UIManager.Instance.SetTimeControlsState(false);
    }
    private void OnChooseStartingTile(HexTile tile) {
        if (tile.areaOfTile != null || tile.landmarkOnTile != null || !tile.isPassable) {
            Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "That is not a valid starting tile!", MESSAGE_BOX_MODE.MESSAGE_ONLY, false);
            return;
        }
        player = new Player();
        player.CreatePlayerFaction();
        player.CreatePlayerArea(tile);
        player.CreateInitialMinions();
        LandmarkManager.Instance.OwnArea(player.playerFaction, player.playerArea);
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
        player.CreatePlayerFaction();
        player.CreatePlayerArea(portal);
        player.CreateInitialMinions();
        LandmarkManager.Instance.OwnArea(player.playerFaction, player.playerArea);
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
        tile.ActivateMagicTransferToPlayer();
        for (int i = 0; i < tile.AllNeighbours.Count; i++) {
            HexTile neighbor = tile.AllNeighbours[i];
            if (!neighbor.isCorrupted) {
                if (neighbor.isPassable) {
                    if (neighbor.landmarkOnTile != null) {
                        //Flat tile with structure
                        if(neighbor.GetCorruptedNeighborsCount() >= 4) {
                            AddTileToPlayerArea(neighbor);
                        }
                    }
                } else {
                    //Non flat tile
                    if (neighbor.GetCorruptedNeighborsCount() >= 3) {
                        AddTileToPlayerArea(neighbor);
                    }
                }
            }
        }
    }

    public void CreatePlayerLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        BaseLandmark landmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(location, landmarkType);
        OnPlayerLandmarkCreated(landmark);
    }

    private void OnPlayerLandmarkCreated(BaseLandmark newLandmark) {
        switch (newLandmark.specificLandmarkType) {
            case LANDMARK_TYPE.SNATCHER_DEMONS_LAIR:
                player.AdjustSnatchCredits(1);
                break;
            default:
                break;
        }
        Messenger.Broadcast(Signals.PLAYER_LANDMARK_CREATED, newLandmark);
    }

    public void AdjustTotalLifestones(int amount) {
        totalLifestonesInWorld += amount;
        Debug.Log("Adjusted lifestones in world by " + amount + ". New total is " + totalLifestonesInWorld);
    }

    #region Snatch
    public bool CanSnatch() {
        if (player == null) {
            return false;
        }
        return player.snatchCredits > 0;
    }
    #endregion
}
