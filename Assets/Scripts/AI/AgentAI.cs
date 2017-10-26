using UnityEngine;
using System.Collections;
using Panda;
using Pathfinding;

//[RequireComponent(typeof(Seeker))]
//[RequireComponent(typeof(SimpleSmoothModifier))]
public class AgentAI : AIPath {

    [SerializeField] private AgentObject _agentObj;

    public override void OnPathComplete(Path _p) {
        if (_p.error) {
            if(_agentObj.currentBehaviour != null) {
                _agentObj.currentBehaviour.CancelAction();
            }
            return;
        }
        base.OnPathComplete(_p);
    }
    public override void OnTargetReached() {
        _agentObj.OnTargetReached(); ;
    }

    private void OnTriggerEnter(Collider other) {
        //Debug.Log(name + " on trigger enter!: " + other.name);
        GameAgent otherAgent = other.transform.parent.parent.GetComponent<AgentObject>().agent;
        if(otherAgent != _agentObj.agent) {
            _agentObj.AddAgentInRange(otherAgent);
        }
    }
    private void OnTriggerExit(Collider other) {
        //Debug.Log(name + " on trigger exit!: " + other.name);
        GameAgent otherAgent = other.transform.parent.parent.GetComponent<AgentObject>().agent;
        _agentObj.RemoveAgentInRange(otherAgent);
    }
}
