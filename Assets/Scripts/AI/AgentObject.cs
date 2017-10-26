using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using Pathfinding;

[RequireComponent(typeof(PandaBehaviour))]
public class AgentObject : MonoBehaviour {

    [SerializeField] private GameAgent _agent;
    [SerializeField] private AIBehaviour _currentBehaviour;

    //Actions
    [SerializeField] private AgentAI _aiPath;
    [SerializeField] private Seeker _seeker;
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private ACTION_TYPE _currentAction;
    [SerializeField] private bool isPerformingAction;
    private int layerMask;
    [SerializeField] private List<GameAgent> _agentsInRange;
    [SerializeField] private List<GameAgent> _targetsInRange; //Agents in range that this agent wants to attack
    [SerializeField] private List<GameAgent> _alliesInRange; //Agents in range that is the same type as this
    [SerializeField] private List<GameAgent> _threatsInRange; //Agents in range that wants to attack this agent;
    [SerializeField] private List<GameAgent> _otherAttackersInRange;
    //private List<Agent> _entitiesAttackingThis;
    //private List<Agent> _threatsInRange;

    //Visuals
    [SerializeField] private SpriteRenderer visualSprite;
    [SerializeField] private UIProgressBar hpProgBar;
    [SerializeField] private Transform agentInfoPanel;
    [SerializeField] private UILabel agentActionLbl;

    #region getters/setters
    internal GameAgent agent {
        get { return _agent; }
    }
    internal AgentAI aiPath {
        get { return _aiPath; }
    }
    internal ACTION_TYPE currentAction {
        get { return _currentAction; }
    }
    internal AIBehaviour currentBehaviour {
        get { return _currentBehaviour; }
    }
    internal List<GameAgent> targetsInRange {
        get { return _targetsInRange; }
    }
    internal List<GameAgent> alliesInRange {
        get { return _alliesInRange; }
    }
    internal List<GameAgent> threatsInRange {
        get { return _threatsInRange; }
    }
    #endregion

    public void Initialize(GameAgent agent, int[] validTags) {
        _agent = agent;
        _aiPath.maxSpeed = agent.movementSpeed;
        _aiPath.endReachedDistance = agent.attackRange;
        sphereCollider.radius = agent.visibilityRange;
        isPerformingAction = false;
        name = agent.agentType.ToString() + UnityEngine.Random.Range(0, 5).ToString();
        visualSprite.gameObject.name = agent.agentType.ToString();
        layerMask = 1 << LayerMask.NameToLayer("Agent");
        _agentsInRange = new List<GameAgent>();
        _targetsInRange = new List<GameAgent>();
        _threatsInRange = new List<GameAgent>();
        _alliesInRange = new List<GameAgent>();
        _otherAttackersInRange = new List<GameAgent>();
        //_entitiesAttackingThis = new List<Entity>();
        //_threatsInRange = new List<Entity>();
        SetValidTags(validTags);
        if(agent.agentCategory == AGENT_CATEGORY.LIVING) {
            visualSprite.enabled = true;
            SetSpriteColor(agent.agentColor);
        } else {
            visualSprite.enabled = false;
        }
        if(agent.movementType == MOVE_TYPE.NONE) {
            _aiPath.canMove = false;
            _aiPath.canSearch = false;
        }
        Messenger.AddListener<GameAgent>("OnAgentDied", OnAgentDied);
    }

    #region Visual Functions
    internal void SetSpriteColor(Color color) {
        visualSprite.color = color;
    }
    #endregion

    #region Tag Functions
    internal void SetValidTags(int[] validTags) {
        _seeker.traversableTags = 0;

        if(System.Array.IndexOf(validTags, -1) != -1) {
            //Set all tags as traversable
            for (int i = 0; i < 32; i++) {
                AddValidTag(i);
            }
        } else {
            for (int i = 0; i < validTags.Length; i++) {
                AddValidTag(validTags[i]);
            }
        }
    }
    internal void AddValidTag(int validTag) {
        _seeker.traversableTags |= 1 << validTag;
    }
    internal void RemoveFromValidTags(int tagToRemove) {
        _seeker.traversableTags &= ~1 << tagToRemove;
    }
    #endregion

    #region Target Functions
    internal void OnAttacked(GameAgent attackedBy) {
        for (int i = 0; i < _alliesInRange.Count; i++) {
            _alliesInRange[i].agentObj.AddAgentAsNearbyAttacker(attackedBy);
        }
    }
    internal void AddAgentAsNearbyAttacker(GameAgent otherAgent) {
        if (!_otherAttackersInRange.Contains(otherAgent)) {
            _otherAttackersInRange.Add(otherAgent);
        }
    }
    internal void AddAgentInRange(GameAgent agent) {
        if (!_agentsInRange.Contains(agent)) {
            _agentsInRange.Add(agent);
        }
    }
    internal void RemoveAgentInRange(GameAgent agent) {
        _agentsInRange.Remove(agent);
    }
    internal void SetTarget(Transform target) {
        _aiPath.target = target;
    }
    internal void SetTarget(Vector3 target) {
        _seeker.StartPath(_aiPath.GetFeetPosition(), target);
    }
    #endregion

    #region Agent Functions
    private void OnAgentDied(GameAgent otherAgent) {
        if(otherAgent == this.agent) {
            Messenger.RemoveListener<GameAgent>("OnAgentDied", OnAgentDied);
        } else {
            _agentsInRange.Remove(otherAgent);
            _targetsInRange.Remove(otherAgent);
            _alliesInRange.Remove(otherAgent);
            _threatsInRange.Remove(otherAgent);
        }
    }
    internal void SetIsPerformingAction(bool state) {
        isPerformingAction = state;
    }
    internal void SetCurrentBehaviour(AIBehaviour currentBehaviour) {
        if(currentBehaviour == null) {
            _currentBehaviour = null;
            _currentAction = ACTION_TYPE.NONE;
        } else {
            _currentBehaviour = currentBehaviour;
            _currentAction = currentBehaviour.actionType;
        }
        
    }
    #endregion

    #region AI
    internal void OnTargetReached() {
        _currentBehaviour.OnActionDone();
    }
    #endregion

    #region Behaviour Tree
    [Task]
    public void DetermineAction() {
        if(agent == null || agent.isDead || GameManager.Instance.isPaused) {
            Task.current.Fail();
            return;
        }
        //Collider[] objectsInRange = Physics.OverlapSphere(this.transform.position, _agent.visibilityRange, layerMask);
        _targetsInRange.Clear();
        _threatsInRange.Clear();
        _alliesInRange.Clear();
        if (_agentsInRange.Count > 0) {
            for (int i = 0; i < _agentsInRange.Count; i++) {
                GameAgent otherAgent = _agentsInRange[i];
                if (this.agent.allyTypes.Contains(otherAgent.agentType)) {
                    //other agent is considered as ally
                    //TODO: add checking for if other agent is from another kingdom, if so, determine if that guard is considered to be an ally
                    if (!_alliesInRange.Contains(otherAgent)) {
                        _alliesInRange.Add(otherAgent);
                    }
                } else {
                    //other agent is not an ally
                    //compute if it is a threat or a target, if threat value and initiative value is equal, randomize if the other agent should be considered a 
                    //threat or target. Also add initiative that is gained from surroundng allies
                    int threatValueOfOtherAgent = this.agent.GetThreatOfAgent(otherAgent);
                    int initiativeValueOfOtherAgent = this.agent.GetInitiativeFromAgent(otherAgent);
                    initiativeValueOfOtherAgent += _agent.GetTotalInitiativeFromAllies(_alliesInRange);
                    if (threatValueOfOtherAgent > initiativeValueOfOtherAgent) {
                        //other agent is a threat
                        _threatsInRange.Add(otherAgent);
                    } else if (initiativeValueOfOtherAgent > threatValueOfOtherAgent) {
                        //other agent is a target
                        _targetsInRange.Add(otherAgent);
                    }
                }
            }
        }

        //Determine whether other entites that have attacked you or allies are to be considered as threats or targets
        List<GameAgent> otherAgents = new List<GameAgent>();
        otherAgents.AddRange(_otherAttackersInRange);
        //otherEntites.AddRange(_threatsInRange);

        for (int i = 0; i < otherAgents.Count; i++) {
            GameAgent otherAgent = otherAgents[i];
            int threatOfEntity = _agent.GetThreatOfAgent(otherAgent);
            int initiativeFromEntity = _agent.GetInitiativeFromAgent(otherAgent);
            int initiativeFromAllies = _agent.GetTotalInitiativeFromAllies(_alliesInRange);
            int totalInitiative = initiativeFromEntity + initiativeFromAllies;
            if (totalInitiative > threatOfEntity) {
                //other entity is to be considered a target
                if (!_targetsInRange.Contains(otherAgent)) {
                    _targetsInRange.Add(otherAgent);
                }
            } else {
                //other entity is to be considered a threat
                if (!_threatsInRange.Contains(otherAgent)) {
                    _threatsInRange.Add(otherAgent);
                }
            }
        }

        AIBehaviour behaviourToPerform = _agent.DetermineAction(_threatsInRange, _targetsInRange, _alliesInRange, isPerformingAction);
        if (behaviourToPerform != null) {
            behaviourToPerform.DoAction();
            Task.current.Succeed();
        } else {
            if (!isPerformingAction) {
                _currentAction = ACTION_TYPE.NONE;
                _currentBehaviour = null;
            }
            Task.current.Fail();
        }
    }
    #endregion

    #region Monobehaviour Functions
    //protected override void OnDrawGizmos() {
    //    base.OnDrawGizmos();
    //    if (alwaysDrawGizmos) {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawWireSphere(Vector3.zero, fovOfEntity);
    //    }
    //}
    private void Update() {
        if(agent.movementType != MOVE_TYPE.NONE) {
            _aiPath.canMove = !GameManager.Instance.isPaused;
        }
        if(GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
            _aiPath.maxSpeed = agent.movementSpeed * 1f;
        } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
            _aiPath.maxSpeed = agent.movementSpeed * 2f;
        } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
            _aiPath.maxSpeed = agent.movementSpeed * 4f;
        }
        
        Vector3 pos = _aiPath.transform.localPosition;
        pos.y += 0.5f;
        pos.z = 0f;
        agentInfoPanel.transform.localPosition = pos;
        hpProgBar.value = (float)agent.currentHP / (float)agent.maxHP;
        if (currentAction == ACTION_TYPE.ATTACK) {
            agentActionLbl.text = "A";
        } else if (currentAction == ACTION_TYPE.FLEE) {
            agentActionLbl.text = "F";
        } else if (currentAction == ACTION_TYPE.RANDOM) {
            agentActionLbl.text = "W";
        } else if(currentAction == ACTION_TYPE.NONE) {
            agentActionLbl.text = "N";
        }

    }
    #endregion
}
