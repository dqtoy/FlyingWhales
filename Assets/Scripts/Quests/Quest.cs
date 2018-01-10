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
    protected QuestCreator _createdBy;
    protected int _daysBeforeDeadline;
    protected bool _isExpired;
    protected bool _isAccepted;
    protected bool _isDone;
    protected Party _assignedParty;
    protected int _maxPartyMembers;
    protected List<QuestFilter> _questFilters;
    protected QuestAction _currentAction;

    protected Queue<QuestAction> _questLine;

    public Quest(QuestCreator createdBy, int daysBeforeDeadline, int maxPartyMembers) {
        _createdBy = createdBy;
        _daysBeforeDeadline = daysBeforeDeadline;
        _maxPartyMembers = maxPartyMembers;
        ScheduleDeadline();
    }

    #region virtuals
    /*
     Accept this quest.
     Quests can only be accepted by characters that can be party leaders.
         */
    public virtual void AcceptQuest(Character partyLeader) {
        _isAccepted = true;
        _isExpired = false;
        //Character that accepts this quest must now create a party
        Party newParty = new Party(partyLeader, _maxPartyMembers);
        newParty.onPartyFull += OnPartyFull;
        _assignedParty = newParty;
    }
    /*
     Add a new character as a party member of this quest.
         */
    public virtual void JoinQuest(Character member) {
        _assignedParty.AddPartyMember(member);
    }
    protected virtual void OnPartyFull(Party party) {
        //When the party is full, proceed with the next step
        StartQuestLine();
    }
    protected virtual void EndQuest(QUEST_RESULT result) {
        if (!_isDone) {
            _isDone = true;
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
    protected virtual void QuestSuccess() { }
    protected virtual void QuestFail() { }
    protected virtual void QuestCancel() { }
    /*
     Construct the list of quest actions that the party will perform.
         */
    protected virtual void ConstructQuestLine() { _questLine = new Queue<QuestAction>(); }
    #endregion

    #region Deadline
    private void ScheduleDeadline() {
        GameDate deadline = GameManager.Instance.Today();
        deadline.AddDays(_daysBeforeDeadline);
        SchedulingManager.Instance.AddEntry(deadline, () => QuestExpired());
    }
    private void QuestExpired() {
        //Quest has reached the expiry date
        if (_isAccepted) {
            //Quest has already been accepted, procceed with the next step regardless of party members
            StartQuestLine();
        } else {
            //Quest has not been accepted, remove quest from quest log
        }
    }
    protected void ScheduleQuestEnd(int days, QUEST_RESULT result) {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(days);
        SchedulingManager.Instance.AddEntry(dueDate, () => EndQuest(result));
    }
    #endregion

    #region Quest Line
    private void StartQuestLine() {
        ConstructQuestLine();
        PerformNextQuestAction();
    }
    internal void PerformNextQuestAction() {
        _currentAction = _questLine.Dequeue();
        _currentAction.DoAction();
    }
    #endregion
}
