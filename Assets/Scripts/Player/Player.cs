using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    private int _corruption;
    private Area _playerArea;

    public Player() {
        _corruption = 0;
        _playerArea = null;
    }

    #region Corruption
    public void AdjustCorruption(int adjustment) {
        _corruption += adjustment;
    }
    #endregion

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        SetPlayerArea(playerArea);
    }
    private void SetPlayerArea(Area area) {
        _playerArea = area;
    }
    #endregion
}
