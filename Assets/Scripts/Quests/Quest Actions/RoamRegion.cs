using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoamRegion : QuestAction {

    private Region _regionToRoam;
    private HexTile previousHexTile;

    #region overrides
    public override void InititalizeAction(Region target) {
        base.InititalizeAction(target);
        _regionToRoam = target;
    }
    public override void DoAction(ECS.Character actionDoer) {
        base.DoAction(actionDoer);
        if(actionDoer.avatar == null) {
            actionDoer.CreateNewAvatar();
        }
        if (previousHexTile == null) {
            previousHexTile = actionDoer.currLocation; //Expect this to be the settlement on the region
        }
        HexTile chosenTile = GetNextHexTile();
        //previousHexTile = chosenTile;
        actionDoer.avatar.SetTarget(chosenTile);
        actionDoer.avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone());
    }
    #endregion

    private HexTile GetNextHexTile() {
        //Check all the neighbouring roads of the characters current location, exclude roads that are not part of the region to explore
        List<HexTile> neighbouringRoads = new List<HexTile>(actionDoer.currLocation.allNeighbourRoads.Where(x => x.region.id == _regionToRoam.id));
        //exclude the previous tile from the neighbouring roads
        if(previousHexTile != null) {
            neighbouringRoads.Remove(previousHexTile);
        }

        if(neighbouringRoads.Count > 1) { 
            //if there is more than 1 neighbour road, this means that the character is at a crossroads, choose a random road
            HexTile chosenTile = neighbouringRoads[Random.Range(0, neighbouringRoads.Count)];
            previousHexTile = actionDoer.currLocation;
            return chosenTile;
        } else if(neighbouringRoads.Count == 1) { 
            //if there is exactly 1 neighbouring road, immediately choose that road
            HexTile chosenTile = neighbouringRoads[0];
            previousHexTile = actionDoer.currLocation;
            return chosenTile;
        } else {
            //if there are no neighbouring roads this means the character is at a dead end, return the previous tile, then set the characters current location as the previous tile
            HexTile chosenTile = previousHexTile;
            previousHexTile = actionDoer.currLocation;
            return chosenTile;
        }
    }
}
