using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class PartyEvent : GameEvent {

    private BaseLandmark location;

    public PartyEvent() : base(GAME_EVENT.PARTY_EVENT) {
        //this.location = location;
    }

    public void Initialize() {
        //End this event a day after today
        GameDate endDate = GameManager.Instance.Today();
        endDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(endDate, () => EndEvent());
    }

    #region overrides
    public override EventAction GetNextEventAction(Character character) {
        //return null;
        return new EventAction(character.GetMiscAction(ACTION_TYPE.PARTY), location.landmarkObj, null);
    }
    public override EventAction PeekNextEventAction(Character character) {
        return null;
    }
    public override int GetEventDurationRoughEstimateInTicks() {
        return GameManager.hoursPerDay;
    }
    public override void EndEvent() {
        base.EndEvent();
        //remove this event from the advertised events at it's location
        location.RemoveAdvertisedEvent(this);
    }
    public override bool MeetsRequirements(Character character) {
        if (character.GetAttribute(ATTRIBUTE.GREGARIOUS) == null) {
            return false;
        }
        return base.MeetsRequirements(character);
    }
    #endregion

    public void SetLocation(BaseLandmark landmark) {
        this.location = landmark;
    }
}
