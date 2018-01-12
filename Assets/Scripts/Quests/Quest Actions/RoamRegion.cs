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
    public override void DoAction(Character actionDoer) {
        base.DoAction(actionDoer);
        if(actionDoer._avatar == null) {
            actionDoer.CreateNewAvatar();
        }
        HexTile chosenTile = GetNextHexTile();
        previousHexTile = chosenTile;
        actionDoer._avatar.SetTarget(chosenTile);
        actionDoer._avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone());
    }
    #endregion

    private HexTile GetNextHexTile() {
        List<HexTile> possibleTiles = _actionDoer.currLocation.allNeighbourRoads.Where(x => x.region.id == _regionToRoam.id).ToList();
        if(previousHexTile != null) {
            possibleTiles.Remove(previousHexTile);
        }
        if(possibleTiles.Count > 0) {
            return possibleTiles[Random.Range(0, possibleTiles.Count)];
        }
        return previousHexTile;
    }
}
