using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointOfInterest {

    POINT_OF_INTEREST_TYPE poiType { get; }
    LocationGridTile gridTileLocation { get; }

    void SetGridTileLocation(LocationGridTile tile);
}
