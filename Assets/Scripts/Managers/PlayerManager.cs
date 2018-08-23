using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance = null;
    public bool isChoosingStartingTile = false;

    public Player player = null;
    public Character playerCharacter;

    [SerializeField] private Sprite[] _playerAreaFloorSprites;

    #region getters/setters
    public Sprite[] playerAreaFloorSprites {
        get { return _playerAreaFloorSprites; }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {

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
        LandmarkManager.Instance.OwnArea(player.playerFaction, player.playerArea);
        playerCharacter = tile.landmarkOnTile.CreateNewCharacter(RACE.HUMANS, CHARACTER_ROLE.PLAYER, "Warrior");
        playerCharacter.party.actionData.SetCannotPerformAction(true);
        playerCharacter.party.RemoveListeners();
        playerCharacter.UnsubscribeSignals();
        Messenger.RemoveListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnChooseStartingTile);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        GameManager.Instance.StartProgression();
        isChoosingStartingTile = false;
        UIManager.Instance.SetTimeControlsState(true);
        //LandmarkManager.Instance.CreateNewArea(tile, AREA_TYPE.DEMONIC_INTRUSION);
    }

    public void AddTileToPlayerArea(HexTile tile) {
        player.playerArea.AddTile(tile);
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

    #region Snatch
    public bool CanSnatch() {
        if (player == null) {
            return false;
        }
        return player.snatchCredits > 0;
    }
    #endregion
}
