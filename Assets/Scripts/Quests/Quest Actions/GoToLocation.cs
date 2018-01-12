using UnityEngine;
using System.Collections;

public class GoToLocation : QuestAction {

    private HexTile targetLocation;

    #region overrides
    public override void InititalizeAction(HexTile target) {
        base.InititalizeAction(target);
        targetLocation = target;
    }
    public override void DoAction(ECS.Character actionDoer) {
        base.DoAction(actionDoer);
        if(actionDoer.currLocation.isHabitable && actionDoer.currLocation.isOccupied && actionDoer.currLocation.landmarkOnTile.owner == actionDoer.faction) {
            //action doer is already at a home settlement
            ActionDone();
        } else {
            if(actionDoer.avatar == null) {
                //Instantiate a new character avatar
                actionDoer.CreateNewAvatar();
            }
            actionDoer.avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone());
        }
        
    }
    public override void ActionDone() {
        //Destroy ECS.Character Avatar
        //_actionDoer.DestroyAvatar();
        base.ActionDone();
    }
    #endregion
}
