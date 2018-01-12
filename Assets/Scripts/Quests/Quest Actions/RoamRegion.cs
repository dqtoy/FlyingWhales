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
        if(previousHexTile == null) {
            previousHexTile = actionDoer.currLocation;
        }
        HexTile chosenTile = GetNextHexTile();
        previousHexTile = chosenTile;
        actionDoer.avatar.SetTarget(chosenTile);
        actionDoer.avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone());
    }
    #endregion

    private HexTile GetNextHexTile() {
        List<HexTile> possibleTiles = new List<HexTile>(_actionDoer.currLocation.allNeighbourRoads.Where(x => x.region.id == _regionToRoam.id));
        if (previousHexTile != null) {
            possibleTiles.Remove(previousHexTile);
        }
        if(possibleTiles.Count > 0) {
            return possibleTiles[Random.Range(0, possibleTiles.Count)];
        }
        return previousHexTile;
    }
}
