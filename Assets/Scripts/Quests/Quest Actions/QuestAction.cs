using UnityEngine;
using System.Collections;

public class QuestAction {

    public delegate void OnQuestActionDone();
    public OnQuestActionDone onQuestActionDone;

    public delegate void OnQuestDoAction();
    public OnQuestDoAction onQuestDoAction;

    protected ECS.Character _actionDoer;
    protected Quest _quest;
    protected QUEST_ACTION_RESULT _result;
    protected bool _isDone;

    #region getters/setters
    public ECS.Character actionDoer {
        get { return _actionDoer; }
    }
    #endregion

    public QuestAction(Quest quest) {
        _quest = quest;
    }

    #region virtuals
    public virtual void InititalizeAction(HexTile target) { }
    public virtual void InititalizeAction(Region target) { }
    public virtual void InititalizeAction(ECS.Character target) { }
    public virtual void InititalizeAction(Settlement target) { }
    public virtual void InititalizeAction(int days) { }

    public virtual void DoAction(ECS.Character partyLeader) {
        _actionDoer = partyLeader;
        if (onQuestDoAction != null) {
            onQuestDoAction();
        }
    }
    public virtual void ActionDone(QUEST_ACTION_RESULT result) {
        _isDone = true;
        _result = result;
        if(onQuestActionDone != null) {
            onQuestActionDone();
        }
    }
    #endregion

}
