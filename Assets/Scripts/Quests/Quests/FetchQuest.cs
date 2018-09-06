using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class FetchQuest : Quest {

    private BaseLandmark targetLandmark;
    private string neededItemName;
    private int neededQuantity;

    public FetchQuest(BaseLandmark targetLandmark, string neededItemName, int neededQuantity) : base(QUEST_TYPE.FETCH_ITEM) {
        this.targetLandmark = targetLandmark;
        this.neededItemName = neededItemName;
        this.neededQuantity = neededQuantity;
    }

    #region overrides
    protected override string GetQuestName() {
        return "Fetch " + neededItemName + " " + neededQuantity + " from " + targetLandmark.landmarkName;
    }
    public override QuestAction GetQuestAction(Character character) {
        QuestAction action = new QuestAction(targetLandmark.landmarkObj.currentState.GetAction(ACTION_TYPE.FETCH), targetLandmark.landmarkObj);
        return action;
    }
    #endregion
}
