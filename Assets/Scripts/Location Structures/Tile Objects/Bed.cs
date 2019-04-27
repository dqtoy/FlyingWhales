using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bed : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }
    private Character[] users; //array of characters, currently using the bed

    public Bed(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.SLEEP, INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_TYPE.NAP };
        Initialize(TILE_OBJECT_TYPE.BED);
        users = new Character[2];
    }

    public override string ToString() {
        return "Bed " + id.ToString();
    }

    public override void OnClickAction() {
        //base.OnClickAction();
        //cycle through characters in bed, and show the chosen characters ui
        if (UIManager.Instance.characterInfoUI.isShowing) {
            if (IsInThisBed(UIManager.Instance.characterInfoUI.activeCharacter)) {
                Character nextCharacter = GetNextCharacterInCycle(UIManager.Instance.characterInfoUI.activeCharacter);
                UIManager.Instance.ShowCharacterInfo(nextCharacter);
            } else {
                if (GetActiveUserCount() > 0) {
                    UIManager.Instance.ShowCharacterInfo(GetNextCharacterInCycle(null));
                }
            }
        } else {
            if (GetActiveUserCount() > 0) {
                UIManager.Instance.ShowCharacterInfo(GetNextCharacterInCycle(null));
            }
        }

    }
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (state == POI_STATE.ACTIVE) {
            if (GetActiveUserCount() > 0) {
                UpdateUsedBedAsset();
            } else {
                gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
            }
        }
    }
    public override void OnDoActionToObject(GoapAction action) {
        base.OnDoActionToObject(action);
        if (action.goapType == INTERACTION_TYPE.SLEEP || action.goapType == INTERACTION_TYPE.NAP) {
            AddUser(action.actor);
        }
    }
    public override void OnDoneActionToObject(GoapAction action) {
        base.OnDoneActionToObject(action);
        if (action.goapType == INTERACTION_TYPE.SLEEP || action.goapType == INTERACTION_TYPE.NAP) {
            RemoveUser(action.actor);
        }
    }
    public override void OnCancelActionTowardsObject(GoapAction action) {
        if (action.goapType == INTERACTION_TYPE.SLEEP || action.goapType == INTERACTION_TYPE.NAP) {
            RemoveUser(action.actor);
        }
    }
    public override bool IsAvailable() {
        for (int i = 0; i < users.Length; i++) {
            if (users[i] == null) {
                return true; //there is an available slot
            }
        }
        return false;
    }

    #region Users
    private void AddUser(Character character) {
        for (int i = 0; i < users.Length; i++) {
            if (users[i] == null) {
                users[i] = character;
                UpdateUsedBedAsset();
                if (!IsAvailable()) {
                    SetPOIState(POI_STATE.INACTIVE); //if all slots in the bed are occupied, set it as inactive
                }
                //disable the character's marker
                character.marker.SetActiveState(false);
                break;
            }
        }
    }
    private void RemoveUser(Character character) {
        for (int i = 0; i < users.Length; i++) {
            if (users[i] == character) {
                users[i] = null;
                if (IsAvailable()) {
                    SetPOIState(POI_STATE.ACTIVE); //if a slots in the bed is unoccupied, set it as active
                }
                //enable the character's marker
                character.marker.SetActiveState(true);
                break;
            }
        }
    }
    private int GetActiveUserCount() {
        int count = 0;
        for (int i = 0; i < users.Length; i++) {
            if (users[i] != null) {
                count++;
            }
        }
        return count;
    }
    private bool IsInThisBed(Character character) {
        for (int i = 0; i < users.Length; i++) {
            if (users[i] == character) {
                return true;
            }
        }
        return false;
    }
    private Character GetNextCharacterInCycle(Character startingPoint) {
        int startingIndex = 0;
        int currIndex = 0;
        if (startingPoint != null) {
            for (int i = 0; i < users.Length; i++) {
                Character currUser = users[i];
                if (currUser == startingPoint) {
                    startingIndex = i;
                    break;
                }
            }
            currIndex = startingIndex + 1;
        }
       
        
        while (true) {
            if (currIndex == users.Length) {
                currIndex = 0;
            }
            if (users[currIndex] != null) {
                return users[currIndex];
            }
            currIndex++;
        }

    }
    #endregion

    private void UpdateUsedBedAsset() {
        int userCount = GetActiveUserCount();
        if (userCount == 1) {
            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bed1SleepingVariant);
        } else if (userCount == 2) {
            gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this, gridTileLocation.parentAreaMap.bed2SleepingVariant);
        }
        //the asset will revert to no one sleeping once the bed is set to active again
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        base.SetGridTileLocation(tile);
    }
}
