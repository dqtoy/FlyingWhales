using UnityEngine;
using System.Collections;

public class MilitaryAllianceOffer : GameEvent {

    private Kingdom _sourceKingdom;
    private Kingdom _targetKingdom;

    private KingdomRelationship _sourceRel;
    private KingdomRelationship _targetRel;

    public MilitaryAllianceOffer(int startWeek, int startMonth, int startYear, Citizen startedBy,
        Kingdom sourceKingdom, Kingdom targetKingdom) : base(startWeek, startMonth, startYear, startedBy) {

        eventType = EVENT_TYPES.MILITARY_ALLIANCE_OFFER;
        name = "Military Alliance Offer";

        _sourceKingdom = sourceKingdom;
        _targetKingdom = targetKingdom;

        _sourceRel = _sourceKingdom.GetRelationshipWithKingdom(_targetKingdom);
        _targetRel = _targetKingdom.GetRelationshipWithKingdom(_sourceKingdom);

        _sourceRel.currentActiveMilitaryAllianceOffer = this;
        _targetRel.currentActiveMilitaryAllianceOffer = this;
    }

    #region overrides
    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        Debug.Log("Military alliance officer from " + _sourceKingdom.name + " has arrived at " + _targetKingdom.name + "'s capital city " + _targetKingdom.capitalCity.name);
        KingdomRelationship targetKingdomRelWithSource = _targetKingdom.GetRelationshipWithKingdom(_sourceKingdom);
        if(targetKingdomRelWithSource.totalLike > 0) {
            //If the receiver has a positive opinion of the sender and he doesnt consider the sender as his Main Threat, he will accept.
			if(_targetRel.totalLike >= 0 && (_targetKingdom.mainThreat == null || _targetKingdom.mainThreat != _sourceKingdom)) {
                //Accept
//                _sourceKingdom.AddMilitaryAlliance(_targetKingdom);
//                _targetKingdom.AddMilitaryAlliance(_sourceKingdom);
				_sourceRel.ChangeMilitaryAlliance (true);
                Debug.Log(_targetKingdom.name + " has accepted a military alliance offer from " + _sourceKingdom.name);
            } else {
                //Decline
                _targetKingdom.UpdateCurrentMilitaryAllianceRejectionDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
                Debug.Log(_targetKingdom.name + " has declined a military alliance offer from " + _sourceKingdom.name);
            }
        } else {
            //Decline
            _targetKingdom.UpdateCurrentMilitaryAllianceRejectionDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
            Debug.Log(_targetKingdom.name + " has declined a military alliance offer from " + _sourceKingdom.name);
        }
        DoneEvent();
    }

    internal override void DoneEvent() {
        base.DoneEvent();
        _sourceRel.currentActiveMilitaryAllianceOffer = null;
        _targetRel.currentActiveMilitaryAllianceOffer = null;
    }
    #endregion
}
