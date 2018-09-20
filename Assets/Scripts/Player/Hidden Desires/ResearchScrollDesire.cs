using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class ResearchScrollDesire : HiddenDesire {
    public ResearchScrollDesire(Character host) : base(HIDDEN_DESIRE.RESEARCH_SCROLL, host) {
    }

    #region Overrides
    public override void Awaken() {
        base.Awaken();
        //when awakened, create a new quest to obtain scrolls,
        //also activate a listener for when the character obtains a new scroll, and schedule a research scrolls event
        //Character ladyOfTheLake = CharacterManager.Instance.GetCharacterByClass("Lady");
        //GameEvent secretMeetingEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.SECRET_MEETING);
        //secretMeetingEvent.Initialize(new List<Character>() { _host, ladyOfTheLake });
    }
    #endregion
}
