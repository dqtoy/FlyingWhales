using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEvent : GameEvent {

    private Character character;

    public TestEvent() : base(GAME_EVENT.TEST_EVENT) {

    }

    public override void Initialize(List<Character> characters) {
        base.Initialize(characters);
        this.character = characters[0];

        //eventActions = new Dictionary<Character, Queue<EventAction>>();
        //eventActions.Add(character, new Queue<EventAction>());
    }

    public void ScheduleEvent(GameDate startDate) {
        SetupEventActions();
        GameDate endDate = startDate;
        endDate.AddDays(GetEventDurationRoughEstimateInTicks());
        character.AddScheduledEvent(new DateRange(startDate, endDate), this);
    }

    public void ScheduleEvent() {
        SetupEventActions();
        character.AddScheduledEvent(this);
    }

    public override void EndEventForCharacter(Character character) {
        base.EndEventForCharacter(character);
        EndEvent();
    }

    private void SetupEventActions() {
        List<BaseLandmark> landmarks = LandmarkManager.Instance.GetAllLandmarks();
        BaseLandmark firstLandmark = landmarks[Utilities.rng.Next(0, landmarks.Count)];
        CharacterAction restAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.REST);
        restAction.SetDuration(30);

        BaseLandmark otherLandmark = landmarks[Utilities.rng.Next(0, landmarks.Count)];
        CharacterAction eatAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.EAT);
        eatAction.SetDuration(15);

        eventActions[character].Enqueue(new EventAction(restAction, firstLandmark.landmarkObj, this, restAction.actionData.duration));
        eventActions[character].Enqueue(new EventAction(eatAction, otherLandmark.landmarkObj, this, eatAction.actionData.duration));
    }

}
