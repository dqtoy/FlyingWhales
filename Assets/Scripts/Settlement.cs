/*
 This will replace the city script.
 */
using UnityEngine;
using System.Collections;

public class Settlement : BaseLandmark {

    public Settlement(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {

    }

    #region Ownership
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        if (location.isHabitable) {
            //Create structures on location
            location.region.HighlightRegionTiles(faction.factionColor, 69f / 255f);
            location.CreateStructureOnTile(faction, STRUCTURE_TYPE.CITY);
            location.emptyCityGO.SetActive(false);
        }
    }
    #endregion
}
