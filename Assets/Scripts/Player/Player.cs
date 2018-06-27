using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    private int _corruption;
    private Area _playerArea;

    #region getters/setters
    public Area playerArea {
        get { return _playerArea; }
    }
    #endregion

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
        BaseLandmark demonicPortal = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        SetPlayerArea(playerArea);
    }
    private void SetPlayerArea(Area area) {
        _playerArea = area;
    }
    #endregion
}
