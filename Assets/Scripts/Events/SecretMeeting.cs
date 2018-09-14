using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class SecretMeeting : GameEvent {

    private Character _character1;
    private Character _character2;

    #region getters/setters
    public Character character1 {
        get { return _character1; }
    }
    public Character character2 {
        get { return _character2; }
    }
    #endregion

    public SecretMeeting(): base(GAME_EVENT.SECRET_MEETING) {
    }

    public void Initialize(Character character1, Character character2) {
        _character1 = character1;
        _character2 = character2;

        SetName("Secret Meeting between " + _character1.name + " and " + _character2.name);
        ScheduleMeeting();
    }
    private void ScheduleMeeting() {
        GameDate meetingDate = GetInitialMeetingDate();
        GameDate endDate = GameManager.Instance.Today();
        endDate.AddHours(GetEventDurationRoughEstimate());

        _character1.AddScheduledAction(new DateRange(meetingDate, endDate), this);
        _character2.AddScheduledAction(new DateRange(meetingDate, endDate), this);
        //Check if there is a special event on that day on any character, if there is, postpone this meeting by 1 day and recheck
    }
    private GameDate GetInitialMeetingDate() {
        int initialMeetingDay = 4; //Thursday
        int initialMeetingHours = 120; //8 PM
        GameDate meetingDate = GameManager.Instance.Today();
        meetingDate.SetHours(initialMeetingHours);
        int currentDay = GameManager.Instance.continuousDays % 7;
        if (currentDay < initialMeetingDay) {
            initialMeetingDay -= currentDay;
        } else if (currentDay > initialMeetingDay) {
            initialMeetingDay = 7 - (currentDay - initialMeetingDay);
        }
        meetingDate.AddDays(initialMeetingDay);
        return meetingDate;
    }

    public override int GetEventDurationRoughEstimate() {
        return 50;
    }
}
