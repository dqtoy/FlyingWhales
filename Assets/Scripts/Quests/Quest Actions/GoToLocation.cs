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
        if(actionDoer.currLocation.isOccupied && actionDoer.currLocation.landmarkOnTile.owner == actionDoer.faction) {
            //action doer is already at a home settlement
            ActionDone();
        } else {
            //Instantiate a new character avatar
            GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterAvatar", actionDoer.currLocation.transform.position, Quaternion.identity);
            CharacterAvatar avatar = avatarGO.GetComponent<CharacterAvatar>();
            avatar.Init(actionDoer);
            avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone());
        }
        
    }
    public override void ActionDone() {
        //Destroy ECS.Character Avatar
        actionDoer.DestroyAvatar();
        base.ActionDone();
    }
    #endregion
}
