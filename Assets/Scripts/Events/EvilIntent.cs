using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EvilIntent : GameEvent {

    private Citizen _sourceKing;
    private Citizen _targetKing;

    private Citizen _kidnappedCitizen;

    public EvilIntent(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen sourceKing, Citizen targetKing) : base(startWeek, startMonth, startYear, startedBy) {
        eventType = EVENT_TYPES.EVIL_INTENT;

        _sourceKing = sourceKing;
        _targetKing = targetKing;

        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();
    }

    #region Overrides
    internal override void PerformAction() {
        base.PerformAction();
    }
    #endregion

    /*
     * Determine action of first king when he/she is chosen
     * for evil intent.
     * */
    private void DetermineFirstAction() {
        if (_sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE) || _sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.DOMINATION)) {
            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = _sourceKing.importantCharacterValues
                .FirstOrDefault(x => x.Key == CHARACTER_VALUE.PEACE || x.Key == CHARACTER_VALUE.DOMINATION);
            AdjustGovernorsLoyalty(_sourceKing, priotiyValue.Key);
            if (priotiyValue.Key == CHARACTER_VALUE.PEACE) {
                ChooseToResist();
            } else {
                ChooseToFeed();
            }
            
        } else {
            AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.DOMINATION);
            ChooseToFeed();
            
        }
    }

    private void ChooseToResist() {
        durationInDays = 5;
        remainingDays = durationInDays;
        EventManager.Instance.onWeekEnd.AddListener(Resist);
    }

    private void Resist() {
        durationInDays -= 1;
        if(durationInDays <= 0) {
            EventManager.Instance.onWeekEnd.RemoveListener(Resist);
            if(Random.Range(0, 100) < 60) {
                //Success
                for (int i = 0; i < _sourceKing.relationshipKings.Count; i++) {
                    Citizen otherKing = _sourceKing.relationshipKings[i].king;
                    RelationshipKings otherKingRel = otherKing.GetRelationshipWithCitizen(_sourceKing);
                    otherKingRel.AddEventModifier(20, "Evil Intent Reaction", this);
                }
                DoneEvent();
            } else {
                //Fail
                _sourceKing.Death(DEATH_REASONS.EVIL_INTENT);
                DoneEvent();
            }
        }
    }

    private void ChooseToFeed() {
        int chance = Random.Range(0, 2);
        if(chance == 0) {
            StartRansomPlot();
        } else {
            War warEventBetweenKingdoms = KingdomManager.Instance.GetWarBetweenKingdoms(_sourceKing.city.kingdom, _targetKing.city.kingdom);
            if(warEventBetweenKingdoms == null) {
                warEventBetweenKingdoms = new War(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, _sourceKing, _sourceKing.city.kingdom, _targetKing.city.kingdom);
            }
            warEventBetweenKingdoms.CreateInvasionPlan(_sourceKing.city.kingdom, this, WAR_TRIGGER.EVIL_INTENT);
        }
    }

    private void StartRansomPlot() {
        durationInDays = 30;
        remainingDays = durationInDays;
        EventManager.Instance.onWeekEnd.AddListener(ProcessRansomPlot);
    }

    private void ProcessRansomPlot() {
        remainingDays -= 1;
        if(remainingDays <= 0) {
            //Successful Kidnapping
            EventManager.Instance.onWeekEnd.RemoveListener(ProcessRansomPlot);
            Kidnap();
        } else {
            //Chance To Find out
            if(Random.Range(0,100) < 2) {
                //Target King finds out about the ransom plot
                for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
                    if(KingdomManager.Instance.allKingdoms[i].id != _sourceKing.city.kingdom.id) {
                        Citizen otherKing = KingdomManager.Instance.allKingdoms[i].king;
                        RelationshipKings otherKingRel = otherKing.GetRelationshipWithCitizen(_sourceKing);
                        otherKingRel.AddEventModifier(-20, "Evil Intent Reaction", this);
                    }
                }
                EventManager.Instance.onWeekEnd.RemoveListener(ProcessRansomPlot);
                DoneEvent();
            }
        }
    }

    private void Kidnap() {
        List<Citizen> kidnapChoices = _targetKing.GetRelatives();
        Citizen citizenToKidnap = kidnapChoices[Random.Range(0, kidnapChoices.Count)];
        if(citizenToKidnap != null) {
            citizenToKidnap.city.RemoveCitizenFromCity(citizenToKidnap);
            _kidnappedCitizen = citizenToKidnap;
            _targetKing.GetRelationshipWithCitizen(_sourceKing).ChangeRelationshipStatus(RELATIONSHIP_STATUS.ENEMY, this);
            AskForRansom();
        } else {
            Debug.LogError("CANNOT KIDNAP ANYONE!");
            DoneEvent();
        }
    }

    private void AskForRansom() {
        durationInDays = 5;
        remainingDays = durationInDays;
        EventManager.Instance.onWeekEnd.AddListener(ProcessRansomDecision);
    }

    private void ProcessRansomDecision() {
        remainingDays -= 1;
        if(remainingDays <= 0) {
            EventManager.Instance.onWeekEnd.RemoveListener(ProcessRansomDecision);
            if(_targetKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.STRENGTH) || _targetKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIFE)) {
                KeyValuePair<CHARACTER_VALUE, int> priotiyValue = _targetKing.importantCharacterValues
                .FirstOrDefault(x => x.Key == CHARACTER_VALUE.STRENGTH || x.Key == CHARACTER_VALUE.LIFE);

                if(priotiyValue.Key == CHARACTER_VALUE.STRENGTH) {
                    KillHostage();
                    DoneEvent();
                } else {

                }
                AdjustGovernorsLoyalty(_targetKing, priotiyValue.Key);
                AdjustOtherKingsRel(_targetKing, priotiyValue.Key);
            } else {
                AdjustGovernorsLoyalty(_targetKing, CHARACTER_VALUE.LIFE);
                AdjustOtherKingsRel(_targetKing, CHARACTER_VALUE.LIFE);
            }
           
        }
    }

    private void KillHostage() {
        _kidnappedCitizen.Death(DEATH_REASONS.EVIL_INTENT);
    }

    private void PayRansom() {
        List<City> citiesToChooseFrom = _targetKing.city.kingdom.cities.Where(x => x.id != _targetKing.city.id).ToList();
        if(citiesToChooseFrom.Count > 0) {
            City cityToGive = citiesToChooseFrom[Random.Range(0, citiesToChooseFrom.Count)];
            KingdomManager.Instance.TransferCitiesToOtherKingdom(_targetKing.city.kingdom, _sourceKing.city.kingdom, new List<City>() { cityToGive });
            if(_sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.HONOR) || _sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.DOMINATION)) {
                KeyValuePair<CHARACTER_VALUE, int> priotiyValue = _targetKing.importantCharacterValues
                .FirstOrDefault(x => x.Key == CHARACTER_VALUE.HONOR || x.Key == CHARACTER_VALUE.DOMINATION);
                if(priotiyValue.Key == CHARACTER_VALUE.HONOR) {
                    ReturnHostage();
                } else {
                    KillHostage();
                }
                AdjustGovernorsLoyalty(_sourceKing, priotiyValue.Key);
                AdjustOtherKingsRel(_sourceKing, priotiyValue.Key);
            } else {
                CHARACTER_VALUE chosenValue = CHARACTER_VALUE.HONOR;
                if(Random.Range(0,2) == 1) {
                    chosenValue = CHARACTER_VALUE.DOMINATION;
                }

                if (chosenValue == CHARACTER_VALUE.HONOR) {
                    ReturnHostage();
                } else {
                    KillHostage();
                }
                AdjustGovernorsLoyalty(_sourceKing, chosenValue);
                AdjustOtherKingsRel(_sourceKing, chosenValue);
            }
        } else {
            //Cannot give any city, kill hostage
            KillHostage();
        }
        DoneEvent();
    }

    private void ReturnHostage() {
        _targetKing.city.AddCitizenToCity(_kidnappedCitizen);
    }

    private void AdjustGovernorsLoyalty(Citizen king, CHARACTER_VALUE chosenValue) {
        for (int i = 0; i < king.city.kingdom.cities.Count; i++) {
            Governor currGovernor = (Governor)king.city.kingdom.cities[i].governor.assignedRole;
            if (currGovernor.citizen.importantCharacterValues.ContainsKey(chosenValue)) {
                currGovernor.AddEventModifier(20, "Evil Intent Opinion", this);
            } else {
                currGovernor.AddEventModifier(-20, "Evil Intent Opinion", this);
            }
        }
    }

    private void AdjustOtherKingsRel(Citizen king, CHARACTER_VALUE chosenValue) {
        for (int i = 0; i < king.city.kingdom.discoveredKingdoms.Count; i++) {
            Citizen otherKing = king.city.kingdom.discoveredKingdoms[i].king;
            RelationshipKings otherKingRel = otherKing.GetRelationshipWithCitizen(king);
            if (otherKing.importantCharacterValues.ContainsKey(chosenValue)) {
                otherKingRel.AddEventModifier(20, "Evil Intent Opinion", this);
            } else {
                otherKingRel.AddEventModifier(-20, "Evil Intent Opinion", this);
            }
        }
    }
}
