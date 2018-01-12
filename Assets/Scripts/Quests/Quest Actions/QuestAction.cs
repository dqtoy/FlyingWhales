using UnityEngine;
using System.Collections;

public class QuestAction {

    public delegate void OnQuestActionDone();
    public OnQuestActionDone onQuestActionDone;

    protected ECS.Character _actionDoer;

    #region getters/setters
    public ECS.Character actionDoer {
        get { return _actionDoer; }
    }
    #endregion

    #region virtuals
    public virtual void InititalizeAction(HexTile target) { }
    public virtual void InititalizeAction(Region target) { }
    public virtual void InititalizeAction(ECS.Character target) { }
    public virtual void InititalizeAction(Settlement target) { }
    public virtual void InititalizeAction(int days) { }

    public virtual void DoAction(ECS.Character partyLeader) { _actionDoer = partyLeader; }
    public virtual void CancelAction() { }
    public virtual void ActionDone() {
        if(onQuestActionDone != null) {
            onQuestActionDone();
        }
    }
    #endregion

}
