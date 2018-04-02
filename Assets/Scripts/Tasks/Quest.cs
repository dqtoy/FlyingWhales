/*
 This is the base class for quests.
 - Quests are created by either the internal manager or military manager.
 - Once a quest is created, it has an x amount of days before it expires, expire meaning that no more characters can accept it and it is discarded.
 - Each quest can have quest filters, that filter the characters that can accept it.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ECS;

public class Quest {

    protected int _id;
    protected bool _isDone;
    protected TaskCreator _createdBy;
    protected QUEST_TYPE _questType;
	protected string _questName;

    protected List<ACTION_ALIGNMENT> _alignment;
    protected List<ECS.Character> _acceptedCharacters;
    protected List<QuestPhase> _phases;
    protected TaskFilter[] _filters;
	protected int _maxRegistration;

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public bool isDone {
        get { return _isDone; }
    }
    public TaskCreator createdBy {
        get { return _createdBy; }
    }
    public string questName {
        get { return _questName; }
    }
    public QUEST_TYPE questType {
        get { return _questType; }
    }
    public List<QuestPhase> phases {
        get { return _phases; }
    }
    public List<ECS.Character> acceptedCharacters {
        get { return _acceptedCharacters; }
    }
	public int maxRegistration {
		get { return _maxRegistration; }
	}
    #endregion

    public Quest(TaskCreator createdBy, QUEST_TYPE questType) {
        _createdBy = createdBy;
        _questType = questType;
		_questName = Utilities.NormalizeStringUpperCaseFirstLetters (_questType.ToString ());
        _alignment = new List<ACTION_ALIGNMENT>();
        _acceptedCharacters = new List<Character>();
        _phases = new List<QuestPhase>();
		SetMaxRegistration (3);
    }

    #region virtuals
    protected virtual string GetQuestName() {
        return _questName;
    }
    /*
     Can a character accept this quest? 
     This is determined by checking this quest's alignment, and verifying
     whether a character's role allows that alignment.
         */
    public virtual bool CanAcceptQuest(ECS.Character character) {
        if (character.currentQuest != null) {
            return false; //the character already has a current quest!
        }
		if(_acceptedCharacters.Count >= _maxRegistration){
			return false;
		}
        if (_acceptedCharacters.Contains(character)) {
            return false; //the character has already accepted this quest!
        }
        if(character.role == null) {
            return false; //if the character doesn't have a role, it cannot accept quests
        } else { 
            for (int i = 0; i < _alignment.Count; i++) {
                ACTION_ALIGNMENT currAlignment = _alignment[i];
                if (!character.role.allowedQuestAlignments.Contains(currAlignment)) {
                    //the character does not allow an alignment that this quest requires, it cannot do this quest!
                    return false;
                }
            }
        }
        //check filters
        if (_filters != null) {
            for (int i = 0; i < _filters.Length; i++) {
                if (!_filters[i].MeetsRequirements(character)) {
                    return false;
                }
            }
        }
        return true;
    }
    /*
     Return the weight that a character will accept this quest
         */
    public virtual int GetAcceptQuestWeight() {
        return 0;
    }
    public virtual void AcceptQuest(ECS.Character accepter) {
        accepter.SetCurrentQuest(this);
        _acceptedCharacters.Add(accepter);
        LogQuestAccept(accepter);
        UIManager.Instance.UpdateQuestsSummary();
    }
    public virtual void QuestTaskDone(TASK_ACTION_RESULT result) {
        if (result == TASK_ACTION_RESULT.SUCCESS) {
            //AdvancePhase();
        }
    }
    public virtual void EndQuest(TASK_STATUS result, ECS.Character endedBy) {
        if (!_isDone) {
            switch (result) {
                case TASK_STATUS.SUCCESS:
                    QuestSuccess(endedBy);
                    break;
                case TASK_STATUS.FAIL:
                    QuestFail(endedBy);
                    break;
                case TASK_STATUS.CANCEL:
                    QuestCancel(endedBy);
                    break;
                default:
                    break;
            }
			UIManager.Instance.UpdateQuestsSummary();
        }
    }
    protected virtual void QuestSuccess(ECS.Character endedBy) {
        _isDone = true;
        QuestManager.Instance.RemoveQuestFromAvailableQuests(this);
        UnregisterAcceptedCharacters();
		if(endedBy != null){
			endedBy.DetermineAction();
		}
    }
    protected virtual void QuestFail(ECS.Character endedBy) {
        
    }
    protected virtual void QuestCancel(ECS.Character endedBy) {
        _isDone = true;
        QuestManager.Instance.RemoveQuestFromAvailableQuests(this);
        UnregisterAcceptedCharacters();
    }
    #endregion

    public void ForceCancelQuest() {
        EndQuest(TASK_STATUS.CANCEL, null);
    }
    private void UnregisterAcceptedCharacters() {
        //Set quest of those that accepted this quest to null, make sure that their current quest is still this one
        for (int i = 0; i < _acceptedCharacters.Count; i++) {
            ECS.Character currCharacter = _acceptedCharacters[i];
            if (currCharacter.currentQuest.id != this.id) {
                throw new Exception(currCharacter.name + "'s quest is no longer set to this quest!");
            }
            currCharacter.SetCurrentQuest(null);
        }
    }
    private void LogQuestAccept(ECS.Character acceptedBy) {
        Log acceptQuestLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "accept_quest");
        acceptQuestLog.AddToFillers(acceptedBy, acceptedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        acceptQuestLog.AddToFillers(null, GetQuestName(), LOG_IDENTIFIER.QUEST_NAME);
        acceptedBy.AddHistory(acceptQuestLog);
        if (acceptedBy.specificLocation is BaseLandmark) {
            (acceptedBy.specificLocation as BaseLandmark).AddHistory(acceptQuestLog);
        }
    }

	#region Utilities
	public void SetMaxRegistration(int amount){
		_maxRegistration = amount;
	}
	#endregion
}

namespace OldQuest{
    public class Quest : CharacterTask {

        protected delegate void OnQuestAccepted();
        protected OnQuestAccepted onQuestAccepted;

        protected int _id;
        protected QUEST_TYPE _questType;
		protected string _questName;
        //protected int _daysBeforeDeadline;
        protected bool _isExpired;
        protected bool _isAccepted;
        protected bool _isWaiting;
        protected Party _assignedParty;
        protected List<TaskFilter> _questFilters;
        protected TaskAction _currentAction;
        protected int _activeDuration;

        protected Queue<TaskAction> _questLine;
        protected Faction _targetFaction; //This is only supposed to have a value when this quest is harmful, put the faction that will be harmed by this quest
        protected Settlement _postedAt; //Where this quest was posted.

        #region getters/setters
        public int id {
            get { return _id; }
        }
        public string urlName {
			get { return "[url=" + this._id.ToString() + "_quest]" + _questName + "[/url]"; }
        }
		public string questName {
			get { return _questName; }
		}
        public TaskCreator createdBy {
            get { return _createdBy; }
        }
        public QUEST_TYPE questType {
            get { return _questType; }
        }
        public bool isExpired {
            get { return _isExpired; }
        }
        public bool isAccepted {
            get { return _isAccepted; }
        }
        public bool isWaiting {
            get { return _isWaiting; }
        }
        public Party assignedParty {
            get { return _assignedParty; }
        }
        public TaskAction currentAction {
            get { return _currentAction; }
        }
        public int activeDuration {
            get { return _activeDuration; }
        }
        public Faction targetFaction {
            get { return _targetFaction; }
        }
        public Settlement postedAt {
            get { return _postedAt; }
        }
        #endregion
        /*
         Create a new quest object.
         NOTE: Set daysBeforeDeadline to -1 if quest cannot expire.
             */
		public Quest(TaskCreator createdBy, QUEST_TYPE questType, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.QUEST, stance) {
            _id = Utilities.SetID(this);
            _questType = questType;
			_questName = Utilities.NormalizeStringUpperCaseFirstLetters (_questType.ToString ());
            //_daysBeforeDeadline = daysBeforeDeadline;
            _activeDuration = 0;
            _questFilters = new List<TaskFilter>();
            _taskLogs = new List<string>();
            //if(daysBeforeDeadline != -1) {
            //    ScheduleDeadline();
            //}
            //_createdBy.AddNewQuest(this);
            if (!(createdBy is ECS.Character)) {
                FactionManager.Instance.AddQuest(this);
            }
        }

        #region virtuals
        /*
         This is called when a quest is posted on the board.
         This will perform the actions needed by the quest, once it is posted,
         such as reserve citizens, setup data, etc.
             */
        public virtual void OnQuestPosted() { }
        /*
         Accept this quest.
         Quests can only be accepted by characters that can be party leaders.
             */
        protected virtual void AcceptQuest(ECS.Character partyLeader) {
            Debug.Log(partyLeader.name + " accepts quest " + questType.ToString() + " on " + Utilities.GetDateString(GameManager.Instance.Today()));
            _isAccepted = true;
            AddNewLog(partyLeader.name + " accepted this quest.");
            if (partyLeader.party != null) {
                //if character already has a party, assign that party to this quest
                AssignPartyToQuest(partyLeader.party);
            } else {
                //Character that accepts this quest must now create a party
                CreateNewPartyForQuest(partyLeader);
            }
            //UnScheduleDeadline();
            SchedulePartyExpiration();
            if (onTaskInfoChanged != null) {
                onTaskInfoChanged();
            }
            if (onQuestAccepted != null) {
                onQuestAccepted();
            }
        }
        protected virtual void EndQuest(TASK_STATUS result) {
            if (_assignedParty.isInCombat) {
                _assignedParty.SetCurrentFunction(() => EndTask(result));
                return;
            }
            if (!_isDone) {
                _taskStatus = result;
                if (_currentAction != null) {
                    _currentAction.onTaskActionDone = null;
                }
                switch (result) {
                    case TASK_STATUS.SUCCESS:
                        QuestSuccess();
                        break;
                    case TASK_STATUS.FAIL:
                        QuestFail();
                        break;
                    case TASK_STATUS.CANCEL:
                        QuestCancel();
                        break;
                    default:
                        break;
                }
                CheckForInternationalIncident();
            }
        }
        protected virtual void QuestSuccess() {
            _isDone = true;
            _createdBy.RemoveQuest(this);
            //if (_postedAt != null) {
            //    _postedAt.RemoveQuestFromBoard(this);//Remove quest from quest board
            //}
            if (_currentAction != null) {
                _currentAction.ActionDone(TASK_ACTION_RESULT.SUCCESS);
            }
            //RetaskParty(_assignedParty.OnReachNonHostileSettlementAfterQuest);
            GiveRewards();
            //_assignedParty.OnQuestEnd();
        }
        protected virtual void QuestFail() {
            _isAccepted = false;
            _createdBy.RemoveQuest(this);
            //if (_postedAt != null) {
            //    _postedAt.RemoveQuestFromBoard(this);//Remove quest from quest board
            //}
            if (_currentAction != null) {
                _currentAction.ActionDone(TASK_ACTION_RESULT.FAIL);
            }
            //_assignedParty.OnQuestEnd();
            //RetaskParty(_assignedParty.OnReachNonHostileSettlementAfterQuest);
        }
        protected virtual void QuestCancel() {
            _isAccepted = false;
            if (_currentAction != null) {
                _currentAction.ActionDone(TASK_ACTION_RESULT.CANCEL);
            }
            //RetaskParty(_assignedParty.partyLeader.OnReachNonHostileSettlementAfterQuest);
            //_assignedParty.OnQuestEnd();
            ResetQuestValues();
        }
        //Some variables in a specific quest must be reset so if other party will get the quest it will not have any values
        protected virtual void ResetQuestValues() {
            _isAccepted = false;
            _isWaiting = false;
            _isExpired = false;
            _assignedParty = null;
            _currentAction = null;
            _questLine.Clear();
        }
        /*
         Construct the list of quest actions that the party will perform.
             */
        protected virtual void ConstructQuestLine() { _questLine = new Queue<TaskAction>(); }
        internal virtual void Result(bool isSuccess) { }
        internal virtual HexTile GetQuestTargetLocation() {
            return null;
        }
        internal virtual void GiveRewards() {
            if (_assignedParty != null) {
                QuestTypeSetup qts = FactionManager.Instance.GetQuestTypeSetup(this.questType);
                if (qts != null) {
                    QuestReward questReward = qts.questRewards;
                    //Give rewards to the characters
                    for (int i = 0; i < _assignedParty.partyMembers.Count; i++) {
                        ECS.Character currMember = _assignedParty.partyMembers[i];
                        if (_assignedParty.IsCharacterLeaderOfParty(currMember)) {
                            //CurrMember is Party Leader
                            currMember.AdjustGold(questReward.leaderGoldReward);
                            currMember.AdjustGold(questReward.leaderPrestigeReward);
                        } else {
                            //CurrMember is a member
                            currMember.AdjustGold(questReward.membersGoldReward);
                            currMember.AdjustGold(questReward.membersPrestigeReward);
                        }
                    }
                }
            }
        }
        public virtual bool CanAcceptQuest(ECS.Character character) {
            if (_isAccepted) {
                return false;
            }
            for (int i = 0; i < _questFilters.Count; i++) {
                TaskFilter currFilter = _questFilters[i];
                if (!currFilter.MeetsRequirements(character)) {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region overrides
        public override void PerformTask() {
			if(!CanPerformTask()){
				return;
			}
            base.PerformTask();
            AcceptQuest(_assignedCharacter);
        }
        public override void EndTask(TASK_STATUS taskResult) {
            EndQuest(taskResult);
        }
        public override void TaskSuccess() {
            QuestSuccess();
        }
        public override void TaskCancel() {
            QuestCancel();
        }
        public override void TaskFail() {
            QuestFail();
        }
        #endregion

        #region Deadline
        public void SchedulePartyExpiration() {
            GameDate deadline = GameManager.Instance.Today();
            deadline.AddDays(3);
            SchedulingManager.Instance.AddEntry(deadline, () => QuestExpired());
        }
        private void QuestExpired() {
            Debug.Log(this.questType.ToString() + " has expired on " + Utilities.GetDateString(GameManager.Instance.Today()));
            _isExpired = true;
            //OldQuest.Quest has reached the expiry date
            if (_isAccepted) {
                //OldQuest.Quest has already been accepted, and all registered party members have arrived
                StartQuestLine();
            }
        }
        protected void ScheduleQuestEnd(int days, TASK_STATUS result) {
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddDays(days);
            SchedulingManager.Instance.AddEntry(dueDate, () => EndQuest(result));
        }
        protected void ScheduleQuestAction(int days, Action action) {
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddDays(days);
            SchedulingManager.Instance.AddEntry(dueDate, () => action());
        }
        #endregion

        #region OldQuest.Quest Line
        public void StartQuestLine() {
            if (_assignedParty != null) {
                _assignedParty.SetOpenStatus(false); //set party to not accept party members
            }
            Debug.Log("Start " + this.questType.ToString() + " OldQuest.Quest!");
            ConstructQuestLine();
            PerformNextQuestAction();
            if (onTaskInfoChanged != null) {
                onTaskInfoChanged();
            }
        }
        internal void PerformNextQuestAction() {
            if (_isDone) {
                return;
            }
            _currentAction = _questLine.Dequeue();
            if (_assignedParty == null) {
                if (_createdBy is ECS.Character) {
                    _currentAction.DoAction((ECS.Character)_createdBy);
                }
            } else {
                _currentAction.DoAction(_assignedParty.partyLeader);
            }

        }
        internal void RepeatCurrentAction() {
            if (_currentAction != null) {
                _currentAction.DoAction(_assignedParty.partyLeader);
            }
        }
        /*
         Turn in this quest, This will end this quest and give the rewards to
         the characters if any.
             */
        public void TurnInQuest(TASK_STATUS taskResult) {
            EndTask(taskResult);
        }
        #endregion

        #region Party
        /*
         Create a new party for this quest.
         The created party will automatically be assigned to this quest.
             */
        internal Party CreateNewPartyForQuest(ECS.Character partyLeader) {
            Party newParty = new Party(partyLeader);
            AssignPartyToQuest(newParty);

            return newParty;
        }
        /*
         Assign a party to this quest.
             */
        internal void AssignPartyToQuest(Party party) {
            _assignedParty = party;
            //_assignedParty.SetCurrentTask(this);
            AddNewLog("Party " + party.name + " is now assigned to this quest.");
            if (_assignedParty.partyLeader.avatar == null) {
                _assignedParty.partyLeader.CreateNewAvatar();//Characters that have accepted a OldQuest.Quest should have icon already even if they are still forming party in the city
//                _assignedParty.SetAvatar(_assignedParty.partyLeader.avatar);
            }
            if (_assignedParty.isFull) {
                //Party is already full, Start the quest
                StartQuestLine();
            } else {
                _assignedParty.SetOpenStatus(true); //Set party as open to members
                                                    //_assignedParty.InviteCharactersOnLocation(CHARACTER_ROLE.ADVENTURER, _assignedParty.specificLocation);
            }
        }
        /*
         When a party member has joined the party,
         check if the party is full, if it is, start the quest
             */
        internal void OnPartyMemberJoined() {
            if (_assignedParty.isFull) {
                StartQuestLine();
            }
        }
        /*
         Make the assigned party go back to the settlement that
         gave the quest.
             */
        internal void GoBackToQuestGiver(TASK_STATUS taskResult) {
            _assignedParty.GoBackToQuestGiver(taskResult);
        }
        internal void SetWaitingStatus(bool isWaiting) {
            _isWaiting = isWaiting;
        }
        #endregion

        #region International Incidents
        protected void CheckForInternationalIncident() {
            //Check first if this quest is targetting any faction, and if it is harmful
            if (_targetFaction != null && FactionManager.Instance.IsQuestHarmful(this.questType)) {
                Faction currLocationOwner = _assignedParty.currLocation.region.owner;
                if (currLocationOwner != null && currLocationOwner.id != _assignedParty.partyLeader.faction.id) { //the party is at a region not owned by his/her faction
                    if (currLocationOwner.id == _targetFaction.id) {//the party is at a region owned by his/her target faction
                        if (_assignedParty.partyLeader.faction != null) {
                            FactionManager.Instance.InternationalIncidentOccured(_targetFaction, _assignedParty.partyLeader.faction, INTERNATIONAL_INCIDENT_TYPE.HARMFUL_QUEST, this);
                        }
                    }
                }
            }
        }
        #endregion

        #region OldQuest.Utilities
        public void SetSettlement(Settlement postedAt) {
            _postedAt = postedAt;
        }
        #endregion
    }
}

