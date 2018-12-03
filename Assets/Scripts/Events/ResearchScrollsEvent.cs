using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchScrollsEvent : GameEvent {

    private Character researcher;

    public ResearchScrollsEvent() : base(GAME_EVENT.RESEARCH_SCROLLS) {
    }

    public override void Initialize(List<Character> characters) {
        base.Initialize(characters);
        researcher = characters[0];

        GameDate startDate = GameManager.Instance.Today();
        startDate.AddMonths(1); //schedule research a day from today

        //this is ususally called when the researcher obtains a new scroll
        //schedule research a day after obtaining a new scroll
        CharacterAction researchAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RESEARCH);
        researchAction.SetDuration(1);
        eventActions[researcher].Enqueue(new EventAction(researchAction, researcher.homeLandmark.landmarkObj, this, researchAction.actionData.duration));

        GameDate endDate = startDate;
        endDate.AddDays(GetEventDurationRoughEstimateInTicks());

        researcher.AddScheduledEvent(new DateRange(startDate, endDate), this);

    }
}
