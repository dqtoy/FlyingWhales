using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectIntel : Intel {

    public IPointOfInterest obj { get; private set; }
    public LocationGridTile knownLocation { get; private set; }

    public TileObjectIntel(IPointOfInterest obj) {
        this.obj = obj;
        this.knownLocation = obj.gridTileLocation;
        Log log = new Log(GameManager.Instance.Today(), "Intel", this.GetType().ToString(), "main_log");
        log.AddToFillers(null, obj.name, LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(knownLocation.structure.location, knownLocation.structure.location.name, LOG_IDENTIFIER.LANDMARK_1);
        SetIntelLog(log);
    }


}
