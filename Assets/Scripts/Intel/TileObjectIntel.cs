using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

public class TileObjectIntel : Intel {

    public IPointOfInterest obj { get; private set; }
    public LocationGridTile knownLocation { get; private set; }

    public TileObjectIntel(IPointOfInterest obj) : base(obj) {
        this.obj = obj;
        this.knownLocation = obj.gridTileLocation;
        Log log = new Log(GameManager.Instance.Today(), "Intel", this.GetType().ToString(), "main_log");
        log.AddToFillers(null, obj.name, LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(knownLocation.structure.location, knownLocation.structure.location.name, LOG_IDENTIFIER.LANDMARK_1);
        SetIntelLog(log);
    }
}

//public class SaveDataTileObjectIntel : SaveDataIntel {
//    public Vector3Save knownLocationID;
//    public int knownLocationAreaID;

//    public int objID;
//    public POINT_OF_INTEREST_TYPE objPOIType;
//    public TILE_OBJECT_TYPE objTileObjectType;

//    public override void Save(Intel intel) {
//        base.Save(intel);
//        TileObjectIntel derivedIntel = intel as TileObjectIntel;
//        if (derivedIntel.actor != null) {
//            actorID = derivedIntel.actor.id;
//        } else {
//            actorID = -1;
//        }

//        if (derivedIntel.target != null) {
//            objID = derivedIntel.target.id;
//            objPOIType = derivedIntel.target.poiType;
//            if (derivedIntel.target is TileObject) {
//                objTileObjectType = (derivedIntel.target as TileObject).tileObjectType;
//            }
//        } else {
//            objID = -1;
//        }
//    }

//    public override Intel Load() {
//        TileObjectIntel intel = base.Load() as TileObjectIntel;
//        intel.Load(this);
//        return intel;
//    }
//}