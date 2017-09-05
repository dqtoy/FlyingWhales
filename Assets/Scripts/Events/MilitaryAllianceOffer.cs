using UnityEngine;
using System.Collections;

public class MilitaryAllianceOffer : GameEvent {

    private Kingdom _sourceKingdom;
    private Kingdom _targetKingdom;

    public MilitaryAllianceOffer(int startWeek, int startMonth, int startYear, Citizen startedBy,
        Kingdom sourceKingdom, Kingdom targetKingdom) : base(startWeek, startMonth, startYear, startedBy) {

        eventType = EVENT_TYPES.MILITARY_ALLIANCE_OFFER;

        _sourceKingdom = sourceKingdom;
        _targetKingdom = targetKingdom;
    }

    #region overrides
    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        Debug.Log("Military alliance officer from " + _sourceKingdom.name + " has arrived at " + _targetKingdom.name + "'s capital city " + _targetKingdom.capitalCity.name);
        DoneEvent();
    }
    #endregion
}
