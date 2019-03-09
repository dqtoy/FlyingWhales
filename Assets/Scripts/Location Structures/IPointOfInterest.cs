﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointOfInterest {

    string name { get; }
    POINT_OF_INTEREST_TYPE poiType { get; }
    POI_STATE state { get; }
    LocationGridTile gridTileLocation { get; }
    List<INTERACTION_TYPE> poiGoapActions { get; }

    void SetGridTileLocation(LocationGridTile tile);
    List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions);
    LocationGridTile GetNearestUnoccupiedTileFromThis();

    void SetPOIState(POI_STATE state);
}
