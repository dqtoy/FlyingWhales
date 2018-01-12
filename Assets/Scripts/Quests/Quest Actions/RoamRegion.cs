using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoamRegion : QuestAction {

    private Region _regionToRoam;

    private HexTile previousHexTile;
    private HexTile nextTile;

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

        //actionDoer._avatar.SetTarget()
        //actionDoer._avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone());

        if (actionDoer.currLocation.isOccupied && actionDoer.currLocation.landmarkOnTile.owner == actionDoer.faction) {
            //action doer is already at a home settlement
            ActionDone();
        } else {
            //Instantiate a new character avatar
            
        }

    }
    public override void ActionDone() {
        //Destroy ECS.Character Avatar
        actionDoer.DestroyAvatar();
        base.ActionDone();
    }
    #endregion

    private HexTile GetNextHexTile() {
        List<HexTile> possibleTiles = actionDoer.currLocation.allNeighbourRoads;
        if(previousHexTile != null) {
            possibleTiles.Remove(previousHexTile);
        }
        if(possibleTiles.Count > 0) {
            return possibleTiles[Random.Range(0, possibleTiles.Count)];
        }
        return previousHexTile;
    }
}
