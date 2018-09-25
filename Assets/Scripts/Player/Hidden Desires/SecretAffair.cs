using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class SecretAffair : HiddenDesire {

    private Character affairWith;

	public SecretAffair(Character host) : base(HIDDEN_DESIRE.SECRET_AFFAIR, host) {

    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        affairWith = CharacterManager.Instance.GetCharacterByClass("Lady");
        _description = "Has an intense attraction towards " + affairWith.name + ".";
    }
    public override void Awaken() {
        base.Awaken();
        GameEvent secretMeetingEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.SECRET_MEETING);
        secretMeetingEvent.Initialize(new List<Character>() { _host, affairWith });
    }
    #endregion
}
