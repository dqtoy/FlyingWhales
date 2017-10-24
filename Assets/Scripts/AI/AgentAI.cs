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
            return;
        }
        base.OnPathComplete(_p);
    }
    public override void OnTargetReached() {
        _agentObj.OnTargetReached(); ;
    }
}
