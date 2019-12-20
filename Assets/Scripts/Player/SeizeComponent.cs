using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class SeizeComponent {
    public IPointOfInterest seizedPOI { get; private set; }


    public void SeizePOI(IPointOfInterest poi) {
        int manaCost = GetManaCost(poi);
        if (PlayerManager.Instance.player.mana < manaCost) {
            PlayerUI.Instance.ShowGeneralConfirmation("ERROR", "Not enough mana! You need " + manaCost + " mana to seize this object.");
            return;
        }
        if (seizedPOI == null) {
            if(poi.isBeingCarriedBy != null) {
                poi.isBeingCarriedBy.ownParty.RemoveCarriedPOI();
            }
            if (poi.gridTileLocation != null) {
                poi.OnSeizePOI();
                //if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //} else {
                //    poi.gridTileLocation.structure.RemovePOI(poi);
                //}
            } else {
                Debug.LogError("Cannot seize. " + poi.name + " has no tile");
                return;
            }
            seizedPOI = poi;
            PlayerManager.Instance.player.AdjustMana(-manaCost);
        } else {
            PlayerUI.Instance.ShowGeneralConfirmation("ERROR", "Already have a seized object. You need to drop the currently seized object first.");
        }
    }
    public void UnseizePOI(IPointOfInterest poi, LocationGridTile tileLocation) {
        if (poi.gridTileLocation == null) {
            poi.OnUnseizePOI(tileLocation);
        } else {
            Debug.LogError("Cannot unseize. " + poi.name + " has a tile");
            return;
        }
        seizedPOI = null;
    }
    private int GetManaCost(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return 50;
        }
        return 20;
    }
}
