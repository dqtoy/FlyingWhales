using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System.Linq;

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

    private Dictionary<Character, bool> isDone;

    public SecretMeeting(): base(GAME_EVENT.SECRET_MEETING) {
    }
    public void Initialize(Character character1, Character character2) {
        _character1 = character1;
        _character2 = character2;

        eventActions = new Dictionary<Character, Queue<EventAction>>();
        isDone = new Dictionary<Character, bool>();

        eventActions.Add(_character1, new Queue<EventAction>());
        eventActions.Add(_character2, new Queue<EventAction>());

        isDone.Add(_character1, false);
        isDone.Add(_character2, false);

        GameDate meetingDate = GetInitialMeetingDate();
        ScheduleMeeting(meetingDate);
    }
    private void ScheduleMeeting(GameDate meetingDate) {
        isDone[_character1] = false; //reset is done
        isDone[_character2] = false; //reset is done

        List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks().Where(x => x.specificLandmarkType == LANDMARK_TYPE.IRON_MINES).ToList();
        BaseLandmark chosenMeetup = allLandmarks[Utilities.rng.Next(0, allLandmarks.Count)];
        WaitingInteractionAction char1WaitAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.WAITING) as WaitingInteractionAction;
        WaitingInteractionAction char2WaitAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.WAITING) as WaitingInteractionAction;
        char1WaitAction.SetWaitedCharacter(_character2);
        char2WaitAction.SetWaitedCharacter(_character1);
        //char1WaitAction.SetOnWaitedCharacterArrivedAction(_character1.ownParty.icharacterObject.currentState.GetAction(ACTION_TYPE.IDLE));
        //char2WaitAction.SetOnWaitedCharacterArrivedAction(_character2.ownParty.icharacterObject.currentState.GetAction(ACTION_TYPE.IDLE));

        GameDate waitDeadline = meetingDate;
        waitDeadline.AddHours(20); //both of the characters will wait for 20 ticks from the meeting date
        char1WaitAction.SetWaitUntil(waitDeadline);
        char2WaitAction.SetWaitUntil(waitDeadline);

        CharacterAction harvestAction = chosenMeetup.landmarkObj.currentState.GetAction(ACTION_TYPE.HARVEST);
        eventActions[_character1].Enqueue(new EventAction(harvestAction, chosenMeetup.landmarkObj, harvestAction.actionData.duration)); //wait at meetup for 20 ticks
        eventActions[_character2].Enqueue(new EventAction(harvestAction, chosenMeetup.landmarkObj, harvestAction.actionData.duration));

        
        allLandmarks = LandmarkManager.Instance.GetAllLandmarks().Where(x => x.specificLandmarkType == LANDMARK_TYPE.LUMBERYARD).ToList();
        BaseLandmark otherLandmark = allLandmarks[Utilities.rng.Next(0, allLandmarks.Count)];

        CharacterAction woodCuttingAction = otherLandmark.landmarkObj.currentState.GetAction(ACTION_TYPE.WOODCUTTING);

        eventActions[_character1].Enqueue(new EventAction(woodCuttingAction, otherLandmark.landmarkObj, woodCuttingAction.actionData.duration)); //once met up, idle at meetup for 30 ticks
        eventActions[_character2].Enqueue(new EventAction(woodCuttingAction, otherLandmark.landmarkObj, woodCuttingAction.actionData.duration));

        SetName("Secret Meeting between " + _character1.name + " and " + _character2.name + " at " + chosenMeetup.locationName);

        GameDate endDate = meetingDate;
        endDate.AddHours(GetEventDurationRoughEstimate());

        _character1.AddScheduledEvent(new DateRange(meetingDate, endDate), this);
        _character2.AddScheduledEvent(new DateRange(meetingDate, endDate), this);
    }
    private GameDate GetInitialMeetingDate() {
        int initialMeetingDay = 1; //4//Thursday
        int initialMeetingHours = 50; //120//8 PM
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
        int longestDuartion = 0;
        foreach (KeyValuePair<Character, Queue<EventAction>> kvp in eventActions) {
            int currCharactersDuration = 0;
            for (int i = 0; i < kvp.Value.Count; i++) {
                EventAction currentAction = kvp.Value.ElementAt(i);
                currCharactersDuration += currentAction.duration;
            }
            if (longestDuartion < currCharactersDuration) {
                longestDuartion = currCharactersDuration;
            }
        }
        return longestDuartion;
    }

    public override void EndEventForCharacter(Character character) {
        base.EndEventForCharacter(character);
        isDone[character] = true;
        bool areAllCharactersDone = true;
        //check if all characters are done with event
        foreach (KeyValuePair<Character, bool> kvp in isDone) {
            if (!kvp.Value) {
                //a character is not yet done!
                areAllCharactersDone = false;
                break;
            }
        }
        if (areAllCharactersDone) {
            //Reschedule the meeting 7 days from now
            GameDate nextMeeting = GameManager.Instance.Today();
            nextMeeting.AddDays(7);
            ScheduleMeeting(nextMeeting);
        }
    }
}
