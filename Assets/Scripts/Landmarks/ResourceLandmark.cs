using UnityEngine;
using System.Collections;

public class ResourceLandmark : BaseLandmark {

    private MATERIAL _materialOnLandmark;
    private Materials _materialData;

    #region getters/setters
    public MATERIAL materialOnLandmark {
        get { return _materialOnLandmark; }
    }
    #endregion

    public ResourceLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
        _materialOnLandmark = Utilities.ConvertLandmarkTypeToMaterial(specificLandmarkType);
        _materialData = MaterialManager.Instance.materialsLookup[_materialOnLandmark];
    }

    #region Ownership
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        //Create structures on location
        location.CreateStructureOnTile(faction, _materialData.structure.structureType);
    }
    #endregion
}
