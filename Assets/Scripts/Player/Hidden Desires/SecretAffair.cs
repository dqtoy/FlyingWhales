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
        Character ladyOfTheLake = CharacterManager.Instance.GetCharacterByClass("Lady");
        GameEvent secretMeetingEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.SECRET_MEETING);
        secretMeetingEvent.Initialize(new List<Character>() { _host, ladyOfTheLake });
    }
    #endregion
}
