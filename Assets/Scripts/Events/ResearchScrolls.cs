using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchScrolls : GameEvent {

    private Character researcher;

    public ResearchScrolls() : base(GAME_EVENT.RESEARCH_SCROLLS) {
    }

    public override void Initialize(List<Character> characters) {
        base.Initialize(characters);
        researcher = characters[0];

        GameDate startDate = GameManager.Instance.Today();
        startDate.AddDays(1);

        //this is ususally called when the researcher obtains a new scroll
        //schedule research a day after obtaining a new scroll
        CharacterAction researchAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RESEARCH);
        researchAction.SetDuration(10);
        eventActions[researcher].Enqueue(new EventAction(researchAction, researcher.homeLandmark.landmarkObj, researchAction.actionData.duration));

        GameDate endDate = startDate;
        endDate.AddHours(GetEventDurationRoughEstimateInTicks());

        researcher.AddScheduledEvent(new DateRange(startDate, endDate), this);

    }
}
