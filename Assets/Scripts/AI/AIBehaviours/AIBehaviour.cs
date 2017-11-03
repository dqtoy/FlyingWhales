using UnityEngine;
using System.Collections;

public class AIBehaviour {

    private ACTION_TYPE _actionType;
    private GameAgent _agentPerformingAction;

    #region getters/setters
    internal ACTION_TYPE actionType {
        get { return _actionType; }
    }
    internal GameAgent agentPerformingAction {
        get { return _agentPerformingAction; }
    }
    #endregion

    public AIBehaviour(ACTION_TYPE actionType, GameAgent agentPerformingAction) {
        _actionType = actionType;
        _agentPerformingAction = agentPerformingAction;
    }

    /*
     * <summary>
     * This is the function to be called when an AI has chosen to perform this behaviour.
     * </summary>
     * */
    internal virtual void DoAction() {
        agentPerformingAction.agentObj.SetIsPerformingAction(true);
        agentPerformingAction.agentObj.SetCurrentBehaviour(this);
    }

    /*
     * <summary>
     * This is the function to be called when an AI has finished performing this behaviour.
     * </summary>
     * */
    internal virtual void OnActionDone() {
        agentPerformingAction.agentObj.SetIsPerformingAction(false);
        agentPerformingAction.agentObj.SetCurrentBehaviour(null);
    }

    internal virtual void CancelAction() {
        agentPerformingAction.agentObj.SetIsPerformingAction(false);
        agentPerformingAction.agentObj.SetCurrentBehaviour(null);
    }
}
