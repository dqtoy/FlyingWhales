using UnityEngine;
using System.Collections;

public class GoToLocation : QuestAction {

    private HexTile targetLocation;

    public GoToLocation(Quest quest) : base(quest) {
    }

    #region overrides
    public override void InititalizeAction(HexTile target) {
        base.InititalizeAction(target);
        targetLocation = target;
    }
    public override void ActionDone(QUEST_ACTION_RESULT result) {
        //Destroy ECS.Character Avatar
        //_actionDoer.DestroyAvatar();
        base.ActionDone(result);
    }
    #endregion

    /*
     This will check if the character is already at target location,
     if it is, action is done, otherwise, make the character move
     to the target location.
         */
    internal void Generic() {
        if (actionDoer.currLocation.id == targetLocation.id) {
            //action doer is already at the target location
            ActionDone(QUEST_ACTION_RESULT.SUCCESS);
        } else {
            if (actionDoer.avatar == null) {
                //Instantiate a new character avatar
                actionDoer.CreateNewAvatar();
            }
            actionDoer.avatar.SetTarget(targetLocation);
            actionDoer.avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => ActionDone(QUEST_ACTION_RESULT.SUCCESS));
        }
    }

	internal void Expand() {
		if (actionDoer.currLocation.id == targetLocation.id) {
			//action doer is already at the target location
			ActionDone(QUEST_ACTION_RESULT.SUCCESS);
		} else {
			if (actionDoer.avatar == null) {
				//Instantiate a new character avatar
				actionDoer.CreateNewAvatar();
			}
			actionDoer.avatar.SetTarget (targetLocation);
			actionDoer.avatar.StartPath(PATHFINDING_MODE.MAJOR_ROADS, () => ActionDone(QUEST_ACTION_RESULT.SUCCESS));
		}
	}
}
