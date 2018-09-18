using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class SecretAffair : HiddenDesire {

	public SecretAffair(Character host) : base(HIDDEN_DESIRE.SECRET_AFFAIR, host) {
    }

    #region Overrides
    public override void Awaken() {
        base.Awaken();
        Character ladyOfTheLake = CharacterManager.Instance.GetCharacterByClass("Lady of the Lake");
        SecretMeeting secretMeetingEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.SECRET_MEETING) as SecretMeeting;
        secretMeetingEvent.Initialize(_host, ladyOfTheLake);
    }
    #endregion
}
