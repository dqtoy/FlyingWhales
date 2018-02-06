using UnityEngine;
using System.Collections;

public class ResourceLandmark : BaseLandmark {

    private MATERIAL _materialOnLandmark;

    #region getters/setters
    public MATERIAL materialOnLandmark {
        get { return _materialOnLandmark; }
    }
    #endregion

    public ResourceLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
        _materialOnLandmark = Utilities.GetMaterialForLandmarkType(specificLandmarkType);
    }

    #region Ownership
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        //Create structures on location
        location.CreateStructureOnTile(faction, Utilities.GetStructureTypeForMaterial(_materialOnLandmark));
    }
    #endregion
}
