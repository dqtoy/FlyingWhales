using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyPile : IPointOfInterest {

    public LocationStructure location { get; private set; }

    #region getters/setters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.SUPLY_PILE; }
    }
    #endregion

    public SupplyPile(LocationStructure location) {
        this.location = location;
    }

    public int GetSuppliesObtained() {
        if (location.structureType == STRUCTURE_TYPE.DUNGEON) {
            return Random.Range(location.location.dungeonSupplyRangeMin, location.location.dungeonSupplyRangeMax + 1);
        } else {
            return Random.Range(1, location.location.suppliesInBank);
        }
        
    }

    public override string ToString() {
        return "Supply Pile";
    }
}
