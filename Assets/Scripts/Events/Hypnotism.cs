using UnityEngine;
using System.Collections;

public class Hypnotism : GameEvent {

    private Kingdom _sourceKingdom;
    private Kingdom _targetKingdom;

    private Witch _witch;

    public Hypnotism(int startWeek, int startMonth, int startYear, Citizen startedBy, Witch witch, Kingdom sourceKingdom, Kingdom targetKingdom) : base(startWeek, startMonth, startYear, startedBy) {
        name = "Hypnotism";
        eventType = EVENT_TYPES.HYPNOTISM;
        _sourceKingdom = sourceKingdom;
        _targetKingdom = targetKingdom;
        _witch = witch;

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Hypnotism", "event_title");
        newLogTitle.AddToFillers(_targetKingdom, _targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Hypnotism", "start");
        newLog.AddToFillers(startedBy, startedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        newLog.AddToFillers(_targetKingdom, _targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();
    }


    #region overrides
    internal override void DoneCitizenAction(Citizen citizen) {
        base.DoneCitizenAction(citizen);
        Log arrivedLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Hypnotism", "witch_arrived");
        arrivedLog.AddToFillers(_targetKingdom, _targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

        RelationshipKings rel = _targetKingdom.king.GetRelationshipWithCitizen(_sourceKingdom.king);
        if (Random.Range(0,100) < 60) {
            //Successful Hypnotize
            if(Random.Range(0,100) < 50) {
                //Positive Effects
                rel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.FRIEND, this);
                Log positive = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Hypnotism", "witch_successful_positive");
                positive.AddToFillers(_targetKingdom.king, _targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
                positive.AddToFillers(_sourceKingdom.king, _sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
            } else {
                //Negative Effects
                rel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.COLD, this);
                Log negative = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Hypnotism", "witch_successful_negative");
                negative.AddToFillers(_targetKingdom.king, _targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
                negative.AddToFillers(_sourceKingdom.king, _sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
            }
        } else {
            //Failed to Hypnotize
            rel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.ENEMY, this);
            Log negative = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Hypnotism", "witch_failed");
            negative.AddToFillers(_targetKingdom.king, _targetKingdom.king.name, LOG_IDENTIFIER.KING_2);
            negative.AddToFillers(_sourceKingdom.king, _sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);

            for (int i = 0; i < _sourceKingdom.discoveredKingdoms.Count; i++) {
                //If the witch fails, Kings that are aware of the source kings kingdom will react to his/her choice. +20 if also values INFLUENCE, otherwise, -20.
                Kingdom currKingdom = _sourceKingdom.discoveredKingdoms[i];
                if(currKingdom.id != _targetKingdom.id) {
                    //if (currKingdom.king.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
                    //    RelationshipKings otherRel = currKingdom.king.GetRelationshipWithCitizen(_sourceKingdom.king);
                    //    otherRel.AddEventModifier(20, "Hypnotism of " + _targetKingdom.king + " reaction", this);
                    //} else {
                        RelationshipKings otherRel = currKingdom.king.GetRelationshipWithCitizen(_sourceKingdom.king);
                        otherRel.AddEventModifier(-20, "Hypnotism of " + _targetKingdom.king + " reaction", this);
                    //}
                }
            }

            for (int i = 0; i < _sourceKingdom.cities.Count; i++) {
                Citizen currGovernor = _sourceKingdom.cities[i].governor;
                if (currGovernor.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
                    ((Governor)currGovernor.assignedRole).AddEventModifier(20, "Hypnotism of " + _targetKingdom.king + " reaction", this);
                } else {
                    ((Governor)currGovernor.assignedRole).AddEventModifier(-20, "Hypnotism of " + _targetKingdom.king + " reaction", this);
                }
            }

            if (_sourceKingdom.importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
                _sourceKingdom.AdjustUnrest(-10);
            } else {
                _sourceKingdom.AdjustUnrest(10);
            }
        }
        DoneEvent();
    }
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
    #endregion
}
