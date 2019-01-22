using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class SecretMeeting : GameEvent {

    private Character _generalMax;
    private Character _ladyOfTheLake;


    #region getters/setters
    public Character generalMax {
        get { return _generalMax; }
    }
    public Character ladyOfTheLake {
        get { return _ladyOfTheLake; }
    }
    #endregion

    private Dictionary<Character, bool> isDoneDict;

    public SecretMeeting(): base(GAME_EVENT.SECRET_MEETING) {
    }
    public override void Initialize(List<Character> characters) {
        base.Initialize(characters);
        _generalMax = characters[0];
        _ladyOfTheLake = characters[1];
        
        isDoneDict = new Dictionary<Character, bool>();

        isDoneDict.Add(_generalMax, false);
        isDoneDict.Add(_ladyOfTheLake, false);

        GameDate meetingDate = GetInitialMeetingDate();
        ScheduleMeeting(meetingDate);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void ScheduleMeeting(GameDate meetingDate) {
        isDoneDict[_generalMax] = false; //reset is done
        isDoneDict[_ladyOfTheLake] = false; //reset is done

        //List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks().Where(x => x.specificLandmarkType == LANDMARK_TYPE.IRON_MINES).ToList();
        BaseLandmark chosenMeetup = LandmarkManager.Instance.GetLandmarkByName("Haundiville");
        if (chosenMeetup == null || chosenMeetup.landmarkObj.isRuined) {
            //if the initial meetup landmark is ruined, set meetup at lady of the lake's home instead
            chosenMeetup = _ladyOfTheLake.homeLandmark;
        }

        WaitingInteractionAction char1WaitAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.WAITING) as WaitingInteractionAction;
        WaitingInteractionAction char2WaitAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.WAITING) as WaitingInteractionAction;
        char1WaitAction.SetWaitedCharacter(_ladyOfTheLake);
        char2WaitAction.SetWaitedCharacter(_generalMax);

        char1WaitAction.SetOnEndAction(() => OnWaitingActionDone());
        char2WaitAction.SetOnEndAction(() => OnWaitingActionDone());

        EatAction char1EatAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.EAT) as EatAction;
        EatAction char2EatAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.EAT) as EatAction;
        char1EatAction.SetDuration(72);
        char2EatAction.SetDuration(72);

        GameDate waitDeadline = meetingDate;
        waitDeadline.AddDays(20); //both of the characters will wait for 20 ticks from the meeting date
        char1WaitAction.SetWaitUntil(waitDeadline);
        char2WaitAction.SetWaitUntil(waitDeadline);

        eventActions[_generalMax].Enqueue(new EventAction(char1WaitAction, chosenMeetup.landmarkObj, this, 20)); //wait at meetup for 20 ticks
        eventActions[_ladyOfTheLake].Enqueue(new EventAction(char2WaitAction, chosenMeetup.landmarkObj, this, 20));

        eventActions[_generalMax].Enqueue(new EventAction(char1EatAction, chosenMeetup.landmarkObj, this, char1EatAction.actionData.duration)); //once met up, idle at meetup for 30 ticks
        eventActions[_ladyOfTheLake].Enqueue(new EventAction(char2EatAction, chosenMeetup.landmarkObj, this, char2EatAction.actionData.duration));

        SetName("Secret Meeting between " + _generalMax.name + " and " + _ladyOfTheLake.name + " at " + chosenMeetup.locationName);

        GameDate endDate = meetingDate;
        endDate.AddDays(GetEventDurationRoughEstimateInTicks());

        //_generalMax.AddScheduledEvent(new DateRange(meetingDate, endDate), this);
        //_ladyOfTheLake.AddScheduledEvent(new DateRange(meetingDate, endDate), this);
    }
    private GameDate GetInitialMeetingDate() {
        //int initialMeetingDay = 1; //4//Thursday
        //int initialMeetingHours = 50; //120//8 PM
        //GameDate meetingDate = GameManager.Instance.Today();
        //meetingDate.SetHours(initialMeetingHours);
        //int currentDayInWeek = GameManager.Instance.continuousDays % 7;
        //if (currentDayInWeek < initialMeetingDay) {
        //    initialMeetingDay -= currentDayInWeek;
        //} else if (currentDayInWeek > initialMeetingDay) {
        //    initialMeetingDay = 7 - (currentDayInWeek - initialMeetingDay);
        //}
        //meetingDate.AddDays(initialMeetingDay);
        GameDate meetingDate = GameManager.Instance.Today();
        meetingDate.AddDays(72);
        return meetingDate;
    }
    public override void EndEventForCharacter(Character character) {
        base.EndEventForCharacter(character);
        isDoneDict[character] = true;
        bool areAllCharactersDone = true;
        //check if all characters are done with event
        foreach (KeyValuePair<Character, bool> kvp in isDoneDict) {
            if (!kvp.Value) {
                //a character is not yet done!
                areAllCharactersDone = false;
                break;
            }
        }
        if (areAllCharactersDone) {
            //Reschedule the meeting 7 days from now
            GameDate nextMeeting = GameManager.Instance.Today();
            nextMeeting.AddMonths(7);
            ScheduleMeeting(nextMeeting);
        }
    }
    public override void EndEvent() {
        base.EndEvent();
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }

    private void OnWaitingActionDone() {
        if (this.isDone) {
            return; //the event is already done (possibly from both of them dying, ignore this)
        }
        //this is for actions once the waiting action of a character is done 
        //this is ususally called, when the character has finished waiting and the other person has not arrived)
        //(or the character has arrived and the character stopped waiting)
        if (!generalMax.isDead && !ladyOfTheLake.isDead) {
            SetCharactersAsLovers();
        } else {
            //check if both character's are still alive, if not this means that at least one character was not able to attend the meeting, 
            //End the event for now, TBD if they should reschedule
            base.EndEventForCharacter(generalMax);
            base.EndEventForCharacter(ladyOfTheLake);
            EndEvent();
        }


    }
    private void SetCharactersAsLovers() {
        if (generalMax.isDead || ladyOfTheLake.isDead) {
            return;
        }
        Relationship char1Rel;
        Relationship char2Rel;
        //if (!generalMax.HasRelationshipWith(ladyOfTheLake)) {
        //    //create relationship
        //    char1Rel = CharacterManager.Instance.CreateNewRelationshipTowards(generalMax, ladyOfTheLake);
        //} else {
        //    char1Rel = _generalMax.GetRelationshipWith(ladyOfTheLake);
        //}
        //if (!ladyOfTheLake.HasRelationshipWith(generalMax)) {
        //    //create relationship
        //    char2Rel = CharacterManager.Instance.CreateNewRelationshipTowards(ladyOfTheLake, generalMax);
        //} else {
        //    char2Rel = _ladyOfTheLake.GetRelationshipWith(generalMax);
        //}

        //Add Relationship status if none yet
        //if (!char1Rel.HasStatus(CHARACTER_RELATIONSHIP.LOVER)) {
        //    char1Rel.AddRelationshipStatus(CHARACTER_RELATIONSHIP.LOVER);
        //}
        //if (!char2Rel.HasStatus(CHARACTER_RELATIONSHIP.LOVER)) {
        //    char2Rel.AddRelationshipStatus(CHARACTER_RELATIONSHIP.LOVER);
        //}
    }
    private void OnCharacterDied(Character character) {
        if (generalMax.id == character.id || ladyOfTheLake.id == character.id) {
            ////if either of the characters died, end the event
            //base.EndEventForCharacter(generalMax);
            //base.EndEventForCharacter(ladyOfTheLake);
            //EndEvent();
            if (!generalMax.isDead) {
                if (generalMax.party.actionData.isCurrentActionFromEvent && generalMax.party.actionData.currentAction.actionType != ACTION_TYPE.WAITING && generalMax.party.actionData.eventAssociatedWithAction.id == this.id) {
                    //character1's current action is from this event, end it
                    generalMax.party.actionData.currentAction.EndAction(generalMax.party, generalMax.party.actionData.currentTargetObject);
                }
            } else {
                //if dead
                base.EndEventForCharacter(generalMax);
            }

            if (!ladyOfTheLake.isDead) {
                if (ladyOfTheLake.party.actionData.isCurrentActionFromEvent && ladyOfTheLake.party.actionData.currentAction.actionType != ACTION_TYPE.WAITING && ladyOfTheLake.party.actionData.eventAssociatedWithAction.id == this.id) {
                    //character1's current action is from this event, end it
                    ladyOfTheLake.party.actionData.currentAction.EndAction(ladyOfTheLake.party, ladyOfTheLake.party.actionData.currentTargetObject);
                }
                if (generalMax.isDead) {
                    //check if lady of the lake is at same location as general max's death
                    if (generalMax.specificLocation.tileLocation.id == ladyOfTheLake.specificLocation.tileLocation.id) {
                        //if yes, give intel to her immediately
                        //remove any reactions to general max's death (Explanation: Because General Max gave some last words to lady of the lake
                        //ladyOfTheLake.RemoveIntelReaction(1);
                        //PlayerManager.Instance.player.GiveIntelToCharacter(IntelManager.Instance.GetIntel(1), ladyOfTheLake);
                    }
                }
            } else {
                //if dead
                base.EndEventForCharacter(ladyOfTheLake);
            }
        }
    }
}
