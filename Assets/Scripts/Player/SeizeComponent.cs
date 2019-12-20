using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class SeizeComponent {
    public IPointOfInterest seizedPOI { get; private set; }
    public bool isPreparingToBeUnseized { get; private set; }

    #region getters
    public bool hasSeizedPOI => seizedPOI != null;
    #endregion
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
            PlayerUI.Instance.ShowSeizedObjectUI();
        } else {
            PlayerUI.Instance.ShowGeneralConfirmation("ERROR", "Already have a seized object. You need to drop the currently seized object first.");
        }
    }
    public void PrepareToUnseize() {
        if (!isPreparingToBeUnseized) {
            isPreparingToBeUnseized = true;
            CursorManager.Instance.AddLeftClickAction(UnseizePOI);
        }
    }
    public void UnseizePOI() {
        if(seizedPOI == null) {
            Debug.LogError("Cannot unseize. Not holding seized object");
            return;
        }
        isPreparingToBeUnseized = false;
        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);

        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if(hoveredTile.objHere != null) {
            return;
        }
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        seizedPOI.OnUnseizePOI(hoveredTile);
        seizedPOI = null;
        PlayerUI.Instance.HideSeizedObjectUI();
    }
    private int GetManaCost(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return 50;
        }
        return 20;
    }
}
