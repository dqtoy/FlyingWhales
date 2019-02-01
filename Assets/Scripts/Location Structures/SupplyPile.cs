using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyPile : IPointOfInterest {

    public LocationStructure location { get; private set; }

    public int suppliesInPile { get; private set; }

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
            return Random.Range(1, suppliesInPile);
        }
    }

    public int GetAndReduceSuppliesObtained(Area reciever) {
        int suppliesObtained = GetSuppliesObtained();
        TransferSuppliesTo(reciever, suppliesObtained);
        return suppliesObtained;
    }

    public void SetSuppliesInPile(int amount) {
        suppliesInPile = amount;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void AdjustSuppliesInPile(int adjustment) {
        suppliesInPile += adjustment;
        suppliesInPile = Mathf.Max(0, suppliesInPile);
    }

    public void TransferSuppliesTo(Area reciever, int amount) {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) { //if supplies come from warehouse, reduce amount
            location.location.AdjustSuppliesInBank(-amount);
            reciever.AdjustSuppliesInBank(amount);
        } else if (location.structureType == STRUCTURE_TYPE.DUNGEON) { //if supplies come from dungeon, do not reduce amount in area
            reciever.AdjustSuppliesInBank(amount);
        }
    }

    public bool HasSupply() {
        if (location.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            return suppliesInPile > 0;
        }
        return true;
    }

    public override string ToString() {
        return "Supply Pile";
    }
}
