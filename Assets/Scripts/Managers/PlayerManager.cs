using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance = null;
    public bool isChoosingStartingTile = false;


    public Player player = null;

    private void Awake() {
        Instance = this;
    }

    public void ChooseStartingTile() {
        Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Pick a starting tile", false);
        isChoosingStartingTile = true;
        Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnChooseStartingTile);
    }

    private void OnChooseStartingTile(HexTile tile) {
        if (tile.areaOfTile != null || tile.landmarkOnTile != null || !tile.isPassable) {
            Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "That is not a valid starting tile!", false);
            return;
        }
        player = new Player();
        player.CreatePlayerArea(tile);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        GameManager.Instance.StartProgression();
        isChoosingStartingTile = false;
        //LandmarkManager.Instance.CreateNewArea(tile, AREA_TYPE.DEMONIC_INTRUSION);
    }
}
