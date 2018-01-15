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

public class Quest {

    protected delegate void OnQuestAccepted();
    protected OnQuestAccepted onQuestAccepted;

    protected QuestCreator _createdBy;
    protected QUEST_TYPE _questType;
    protected int _daysBeforeDeadline;
    protected bool _isExpired;
    protected bool _isAccepted;
    protected bool _isDone;
    protected bool _isWaiting;
    protected Party _assignedParty;
    protected List<QuestFilter> _questFilters;
    protected QuestAction _currentAction;
    protected QUEST_RESULT _questResult;

    protected Queue<QuestAction> _questLine;

    #region getters/setters
    public QUEST_TYPE questType {
        get { return _questType; }
    }
    public bool isAccepted {
        get { return _isAccepted; }
    }
    public Party assignedParty {
        get { return _assignedParty; }
    }
    public QUEST_RESULT questResult {
        get { return _questResult; }
    }
    #endregion
    /*
     Create a new quest object.
     NOTE: Set daysBeforeDeadline to -1 if quest cannot expire.
         */
    public Quest(QuestCreator createdBy, int daysBeforeDeadline, QUEST_TYPE questType) {
        _createdBy = createdBy;
        _questType = questType;
        _daysBeforeDeadline = daysBeforeDeadline;
        if(daysBeforeDeadline != -1) {
            ScheduleDeadline();
        }
        _createdBy.AddNewQuest(this);
    }

    #region virtuals
    /*
     Accept this quest.
     Quests can only be accepted by characters that can be party leaders.
         */
    public virtual void AcceptQuest(ECS.Character partyLeader) {
        _isAccepted = true;

        if (partyLeader.party != null) {
            //if character already has a party, assign that party to this quest
            AssignPartyToQuest(partyLeader.party);
        } else {
            //Character that accepts this quest must now create a party
            CreateNewPartyForQuest(partyLeader);
        }

        if(onQuestAccepted != null) {
            onQuestAccepted();
        }
    }
    /*
     Add a new character as a party member of this quest.
         */
    public virtual void JoinQuest(ECS.Character member) {
        _assignedParty.AddPartyMember(member);
        member.SetCurrentQuest(this);
    }
    /*
     This is the action done, when the party assigned to this quest is full.
     Full meaning a number of characters have registered to join this quest,
     but they might not be with the party leader yet, if they are not, wait for them to arrive,
     otherwise, start the quest immediately.
         */
    protected virtual void OnPartyFull(Party party) {
        //When the party is full, check if all the characters have arrived at the party leaders location
        //if not wait for them to arrive before starting the quest.
        CheckPartyMembers();
    }
    protected virtual void EndQuest(QUEST_RESULT result) {
        if (!_isDone) {
            switch (result) {
                case QUEST_RESULT.SUCCESS:
                    QuestSuccess();
                    break;
                case QUEST_RESULT.FAIL:
                    QuestFail();
                    break;
                case QUEST_RESULT.CANCEL:
                    QuestCancel();
                    break;
                default:
                    break;
            }
        }
    }
    protected virtual void QuestSuccess() {
        _isDone = true;
        _questResult = QUEST_RESULT.SUCCESS;
        _createdBy.RemoveQuest(this);
        CheckIfDestroyAvatar();
        RetaskParty();
    }
    protected virtual void QuestFail() {
        _isDone = true;
        _questResult = QUEST_RESULT.FAIL;
        _createdBy.RemoveQuest(this);
        _currentAction.onQuestActionDone = null;
        _currentAction.ActionDone(QUEST_ACTION_RESULT.FAIL);
        CheckIfDestroyAvatar();
        RetaskParty();
    }
    protected virtual void QuestCancel() {
        _isDone = true;
        _questResult = QUEST_RESULT.CANCEL;
        _createdBy.RemoveQuest(this);
        _currentAction.onQuestActionDone = null;
        _currentAction.ActionDone(QUEST_ACTION_RESULT.CANCEL);
        CheckIfDestroyAvatar();
        RetaskParty();
    }
    /*
     Construct the list of quest actions that the party will perform.
         */
    protected virtual void ConstructQuestLine() { _questLine = new Queue<QuestAction>(); }
    #endregion

    public bool CanAcceptQuest(ECS.Character character) {
        for (int i = 0; i < _questFilters.Count; i++) {
            QuestFilter currFilter = _questFilters[i];
            if (!currFilter.MeetsRequirements(character)) {
                return false;
            }
        }
        return true;
    }

    #region Deadline
    private void ScheduleDeadline() {
        GameDate deadline = GameManager.Instance.Today();
        deadline.AddDays(_daysBeforeDeadline);
        SchedulingManager.Instance.AddEntry(deadline, () => QuestExpired());
    }
    private void QuestExpired() {
        //Quest has reached the expiry date
        if (_isAccepted) {
            if (!_isWaiting) {//Check first if all the party members have arrived
                //Quest has already been accepted, procceed with the next step regardless of party members
                StartQuestLine();
            }
        } else {
            //Quest has not been accepted, remove quest from quest log
            _createdBy.RemoveQuest(this);
        }
    }
    protected void ScheduleQuestEnd(int days, QUEST_RESULT result) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(days);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndQuest(result));
    }
    #endregion

    #region Quest Line
    public void StartQuestLine() {
        if(_assignedParty != null) {
            _assignedParty.SetOpenStatus(false); //set party to not accept party members
        }
        ConstructQuestLine();
        PerformNextQuestAction();
    }
    internal void PerformNextQuestAction() {
        _currentAction = _questLine.Dequeue();
        _currentAction.DoAction(_assignedParty.partyLeader);
    }
    internal void RepeatCurrentAction() {
        if(_currentAction != null) {
            _currentAction.DoAction(_assignedParty.partyLeader);
        }
    }
    #endregion

    #region Party
    /*
     Create a new party for this quest.
     The created party will automatically be assigned to this quest.
         */
    private Party CreateNewPartyForQuest(ECS.Character partyLeader) {
        Party newParty = new Party(partyLeader);
        newParty.onPartyFull = OnPartyFull;
        AssignPartyToQuest(newParty);
        return newParty;
    }
    /*
     Assign a party to this quest.
         */
    internal void AssignPartyToQuest(Party party) {
        _assignedParty = party;
        party.SetCurrentQuest(this);
        if (_assignedParty.isFull) {
            //Party is already full, check party members
            CheckPartyMembers();
        } else {
            _assignedParty.SetOpenStatus(true); //Set party as open to members
            _assignedParty.onPartyFull = OnPartyFull;
        }
    }
    private void RetaskParty() {
        //Check which party members will leave
        List<ECS.Character> charactersToLeave = new List<ECS.Character>();
        for (int i = 0; i < _assignedParty.partyMembers.Count; i++) {
            ECS.Character currMember = _assignedParty.partyMembers[i];
            if (currMember != _assignedParty.partyLeader) {
                WeightedDictionary<PARTY_ACTION> partyActionWeights = _assignedParty.GetPartyActionWeightsForCharacter(currMember);
                if(partyActionWeights.PickRandomElementGivenWeights() == PARTY_ACTION.LEAVE) {
                    charactersToLeave.Add(currMember);
                }
            }
        }

        for (int i = 0; i < charactersToLeave.Count; i++) {
            ECS.Character characterToLeave = charactersToLeave[i];
            _assignedParty.RemovePartyMember(characterToLeave);
            characterToLeave.DetermineAction(); //Make the character that left the party determine a next action
        }

        _assignedParty.SetCurrentQuest(null);
        _assignedParty.onPartyFull = null;
        _assignedParty.DetermineNextAction();
    }
    internal void CheckPartyMembers() {
        if (_assignedParty.AreAllPartyMembersPresent()) {
            _isWaiting = false;
            StartQuestLine();
        } else {
            _isWaiting = true;
        }
    }
    #endregion

    private void CheckIfDestroyAvatar() {
        if (_assignedParty.partyLeader.currLocation.isOccupied) {
            //Destroy Avatar
            _assignedParty.partyLeader.DestroyAvatar();
        }
    }
}
