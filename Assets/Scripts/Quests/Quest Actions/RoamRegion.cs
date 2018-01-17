using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoamRegion : QuestAction {

    private Region _regionToRoam;
    private HexTile previousHexTile;

    public RoamRegion(Quest quest) : base(quest) {
    }

    #region overrides
    public override void InititalizeAction(Region target) {
        base.InititalizeAction(target);
        _regionToRoam = target;
    }
    #endregion

    private HexTile GetNextHexTile() {
        //Check all the neighbouring roads of the characters current location
        List<HexTile> neighbouringRoads = new List<HexTile>();
        for (int i = 0; i < actionDoer.currLocation.allNeighbourRoads.Count; i++) {
            HexTile currRoadTile = actionDoer.currLocation.allNeighbourRoads[i];
            if (currRoadTile.region.id != _regionToRoam.id) {
                continue; //exclude roads that are not part of the region to explore
            }
            if(currRoadTile.roadType == ROAD_TYPE.MAJOR) {
                if(currRoadTile.landmarkOnTile == null) {
                    continue;
                }
            }
            if(previousHexTile != null && currRoadTile.id == previousHexTile.id) {
                continue; //exclude the previous tile from the neighbouring roads
            }
            neighbouringRoads.Add(currRoadTile);
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

    internal void ExploreRegion() {
        if (actionDoer.avatar == null) {
            actionDoer.CreateNewAvatar();
        }
        if (previousHexTile == null) {
            previousHexTile = actionDoer.currLocation; //Expect this to be the settlement on the region
        }
        HexTile chosenTile = GetNextHexTile();
        //previousHexTile = chosenTile;
        actionDoer.avatar.SetTarget(chosenTile);
        actionDoer.avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone(QUEST_ACTION_RESULT.SUCCESS));
    }
}
