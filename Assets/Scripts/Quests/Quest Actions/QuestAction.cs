using UnityEngine;
using System.Collections;

public class QuestAction {

    public delegate void OnQuestActionDone();
    public OnQuestActionDone onQuestActionDone;

    protected Character actionDoer;

    #region virtuals
    public virtual void InititalizeAction(HexTile target) { }
    public virtual void InititalizeAction(Region target) { }
    public virtual void InititalizeAction(Character target) { }
    public virtual void InititalizeAction(Settlement target) { }
    public virtual void InititalizeAction(int days) { }

    public virtual void DoAction(Character partyLeader) { actionDoer = partyLeader; }
    public virtual void CancelAction() { }
    public virtual void ActionDone() {
        if(onQuestActionDone != null) {
            onQuestActionDone();
        }
    }
    #endregion

}
