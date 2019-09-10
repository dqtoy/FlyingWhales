using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows 1 additional Artifact slot. If destroyed and there is an occupant Artifact, player must choose which Artifact to discard.
/// Ref: https://trello.com/c/EaUHzTaK/2690-the-crypt
/// </summary>
public class TheCrypt : BaseLandmark {
    public TheCrypt(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }
    public TheCrypt(HexTile location, SaveDataLandmark data) : base(location, data) { }

    #region Overrides
    public override void OnFinishedBuilding() {
        base.OnFinishedBuilding();
        PlayerManager.Instance.player.IncreaseArtifactSlot();
    }
    public override void DestroyLandmark() {
        base.DestroyLandmark();
        PlayerManager.Instance.player.DecreaseArtifactSlot();
    }
    #endregion
}
