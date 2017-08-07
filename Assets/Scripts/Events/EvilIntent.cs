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

		this._warTrigger = WAR_TRIGGER.EVIL_INTENT;

        Log newLogTitle = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "event_title");
        newLogTitle.AddToFillers(targetKing, targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        Log newLogStart = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "start");
        newLogStart.AddToFillers(sourceKing, sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        newLogStart.AddToFillers(targetKing, targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        EventManager.Instance.AddEventToDictionary(this);
        EventIsCreated();

        EventManager.Instance.onWeekEnd.AddListener(CheckEventValidity);

        DetermineFirstAction();
    }

    /*
     * Check if the event is still valid, if not,
     * cancel the event.
     * */
    private void CheckEventValidity() {
        //Either of the kings are dead
        if(_sourceKing.isDead || _targetKing.isDead) {
            CancelEvent();
        }
        //source king is no longer king
        if(_sourceKing.city.kingdom.king.id != _sourceKing.id) {
            CancelEvent();
        }
        //target king is no longer king
        if (_targetKing.city.kingdom.king.id != _targetKing.id) {
            CancelEvent();
        }
    }

    /*
     * Determine action of first king when he/she is chosen
     * for evil intent.
     * */
    private void DetermineFirstAction() {
        if (_sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.PEACE) || _sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.DOMINATION)) {
            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = _sourceKing.importantCharacterValues
                .FirstOrDefault(x => x.Key == CHARACTER_VALUE.PEACE || x.Key == CHARACTER_VALUE.DOMINATION);
            
            if (priotiyValue.Key == CHARACTER_VALUE.PEACE) {
                AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.PEACE, CHARACTER_VALUE.DOMINATION);
                AdjustKingdomUnrest(_sourceKing.city.kingdom, CHARACTER_VALUE.PEACE, CHARACTER_VALUE.DOMINATION);
                ChooseToResist();
            } else {
                AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.PEACE);
                AdjustKingdomUnrest(_sourceKing.city.kingdom, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.PEACE);
                ChooseToFeed();
            }
        } else {
            AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.PEACE);
            AdjustKingdomUnrest(_sourceKing.city.kingdom, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.PEACE);
            ChooseToFeed();
            
        }
    }

    private void ChooseToResist() {
        Log resistLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "first_action_decision_resist");
        resistLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

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
                Log resistSuccessLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "resist_success");
                resistSuccessLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                resistSuccessLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                for (int i = 0; i < _sourceKing.relationshipKings.Count; i++) {
                    Citizen otherKing = _sourceKing.relationshipKings[i].king;
                    RelationshipKings otherKingRel = otherKing.GetRelationshipWithCitizen(_sourceKing);
                    otherKingRel.AddEventModifier(20, "Evil Intent Reaction", this);
                }
                DoneEvent();
            } else {
                //Fail
                Log resistFailLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "resist_fail");
                resistFailLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                resistFailLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                _sourceKing.Death(DEATH_REASONS.EVIL_INTENT);
                DoneEvent();
            }
        }
    }

    private void ChooseToFeed() {
        Log feedLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "first_action_decision_feed");
        feedLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        int chance = Random.Range(0, 2);
        if(chance == 0) {
            Log kidnapLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "feed_action_kidnap");
            kidnapLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            kidnapLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            StartRansomPlot();
        } else {
            Log invasionLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "feed_action_invasion");
            invasionLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            invasionLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            War warEventBetweenKingdoms = KingdomManager.Instance.GetWarBetweenKingdoms(_sourceKing.city.kingdom, _targetKing.city.kingdom);
            if(warEventBetweenKingdoms == null) {
				warEventBetweenKingdoms = new War(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, _sourceKing, _sourceKing.city.kingdom, _targetKing.city.kingdom, this._warTrigger);
            }
            warEventBetweenKingdoms.CreateInvasionPlan(_sourceKing.city.kingdom, this);
            DoneEvent();
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
            Log successKidnapLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "kidnap_plot_success");
            successKidnapLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            successKidnapLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            EventManager.Instance.onWeekEnd.RemoveListener(ProcessRansomPlot);
            Kidnap();
        } else {
            //Chance To Find out
            if(Random.Range(0,100) < 2) {
                //Target King finds out about the ransom plot
                Log failKidnapLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "kidnap_plot_fail");
                failKidnapLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                failKidnapLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

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
        Log ransomLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "ransom");
        ransomLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        ransomLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

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
                    Log ransomKillLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "ransom_decision_kill");
                    ransomKillLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    ransomKillLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

                    AdjustKingdomUnrest(_targetKing.city.kingdom, CHARACTER_VALUE.STRENGTH, CHARACTER_VALUE.LIFE);
                    AdjustGovernorsLoyalty(_targetKing, CHARACTER_VALUE.STRENGTH, CHARACTER_VALUE.LIFE);
                    AdjustOtherKingsRel(_targetKing, CHARACTER_VALUE.STRENGTH, CHARACTER_VALUE.LIFE);

                    KillHostage();
                    DoneEvent();
                } else {
                    AdjustKingdomUnrest(_targetKing.city.kingdom, CHARACTER_VALUE.LIFE, CHARACTER_VALUE.STRENGTH);
                    AdjustGovernorsLoyalty(_targetKing, CHARACTER_VALUE.LIFE, CHARACTER_VALUE.STRENGTH);
                    AdjustOtherKingsRel(_targetKing, CHARACTER_VALUE.LIFE, CHARACTER_VALUE.STRENGTH);
                    PayRansom();
                }
                
            } else {
                PayRansom();
                AdjustKingdomUnrest(_targetKing.city.kingdom, CHARACTER_VALUE.LIFE, CHARACTER_VALUE.STRENGTH);
                AdjustGovernorsLoyalty(_targetKing, CHARACTER_VALUE.LIFE, CHARACTER_VALUE.STRENGTH);
                AdjustOtherKingsRel(_targetKing, CHARACTER_VALUE.LIFE, CHARACTER_VALUE.STRENGTH);
            }
           
        }
    }

    private void KillHostage() {
        _kidnappedCitizen.Death(DEATH_REASONS.EVIL_INTENT);
    }

    private void PayRansom() {
        Log ransomPayLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "ransom_decision_pay");
        ransomPayLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        List<City> citiesToChooseFrom = _targetKing.city.kingdom.cities.Where(x => x.id != _targetKing.city.id).ToList();
        if(citiesToChooseFrom.Count > 0) {
            City cityToGive = citiesToChooseFrom[Random.Range(0, citiesToChooseFrom.Count)];
            KingdomManager.Instance.TransferCitiesToOtherKingdom(_targetKing.city.kingdom, _sourceKing.city.kingdom, new List<City>() { cityToGive });
            if(_sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.HONOR) || _sourceKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.DOMINATION)) {
                KeyValuePair<CHARACTER_VALUE, int> priotiyValue = _targetKing.importantCharacterValues
                .FirstOrDefault(x => x.Key == CHARACTER_VALUE.HONOR || x.Key == CHARACTER_VALUE.DOMINATION);
                if(priotiyValue.Key == CHARACTER_VALUE.HONOR) {
                    AdjustKingdomUnrest(_sourceKing.city.kingdom, CHARACTER_VALUE.HONOR, CHARACTER_VALUE.DOMINATION);
                    AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.HONOR, CHARACTER_VALUE.DOMINATION);
                    AdjustOtherKingsRel(_sourceKing, CHARACTER_VALUE.HONOR, CHARACTER_VALUE.DOMINATION);
                    ReturnHostage();
                } else {
                    Log ransomKillLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "ransom_accepted_decision_kill");
                    ransomKillLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    AdjustKingdomUnrest(_sourceKing.city.kingdom, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.HONOR);
                    AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.HONOR);
                    AdjustOtherKingsRel(_sourceKing, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.HONOR);
                    KillHostage();
                }
                
            } else {
                CHARACTER_VALUE chosenValue = CHARACTER_VALUE.HONOR;
                if(Random.Range(0,2) == 1) {
                    chosenValue = CHARACTER_VALUE.DOMINATION;
                }

                if (chosenValue == CHARACTER_VALUE.HONOR) {
                    AdjustKingdomUnrest(_sourceKing.city.kingdom, CHARACTER_VALUE.HONOR, CHARACTER_VALUE.DOMINATION);
                    AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.HONOR, CHARACTER_VALUE.DOMINATION);
                    AdjustOtherKingsRel(_sourceKing, CHARACTER_VALUE.HONOR, CHARACTER_VALUE.DOMINATION);
                    ReturnHostage();
                } else {
                    AdjustKingdomUnrest(_sourceKing.city.kingdom, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.HONOR);
                    AdjustGovernorsLoyalty(_sourceKing, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.HONOR);
                    AdjustOtherKingsRel(_sourceKing, CHARACTER_VALUE.DOMINATION, CHARACTER_VALUE.HONOR);
                    KillHostage();
                }
            }
        } else {
            //Cannot give any city, kill hostage
            Log ransomUnableLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "ransom_decision_unable");
            ransomUnableLog.AddToFillers(_targetKing, _targetKing.name, LOG_IDENTIFIER.TARGET_CHARACTER);

            KillHostage();
        }
        DoneEvent();
    }

    private void ReturnHostage() {
        Log ransomHonorLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "EvilIntent", "ransom_accepted_decision_honor");
        ransomHonorLog.AddToFillers(_sourceKing, _sourceKing.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

        _targetKing.city.AddCitizenToCity(_kidnappedCitizen);
    }

    private void AdjustGovernorsLoyalty(Citizen king, CHARACTER_VALUE chosenValue, CHARACTER_VALUE oppositeValue) {
        for (int i = 0; i < king.city.kingdom.cities.Count; i++) {
            Governor currGovernor = (Governor)king.city.kingdom.cities[i].governor.assignedRole;
            if (currGovernor.citizen.importantCharacterValues.ContainsKey(chosenValue) 
                || currGovernor.citizen.importantCharacterValues.ContainsKey(oppositeValue)) {

                KeyValuePair<CHARACTER_VALUE, int> priotiyValue = currGovernor.citizen.importantCharacterValues
                               .FirstOrDefault(x => x.Key == chosenValue || x.Key == oppositeValue);
                if(priotiyValue.Key == chosenValue) {
                    currGovernor.AddEventModifier(20, "Evil Intent Opinion", this);
                } else {
                    currGovernor.AddEventModifier(-20, "Evil Intent Opinion", this);
                }
            } else {
                currGovernor.AddEventModifier(-20, "Evil Intent Opinion", this);
            }
        }
    }

    private void AdjustKingdomUnrest(Kingdom kingdom, CHARACTER_VALUE chosenValue, CHARACTER_VALUE oppositeValue) {
        if (kingdom.importantCharacterValues.ContainsKey(chosenValue) 
            || kingdom.importantCharacterValues.ContainsKey(oppositeValue)) {
            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = kingdom.importantCharacterValues
                               .FirstOrDefault(x => x.Key == chosenValue || x.Key == oppositeValue);
            if (priotiyValue.Key == chosenValue) {
                kingdom.AdjustUnrest(-10);
            } else {
                kingdom.AdjustUnrest(10);
            }
        } else {
            kingdom.AdjustUnrest(10);
        }
    }

    private void AdjustOtherKingsRel(Citizen king, CHARACTER_VALUE chosenValue, CHARACTER_VALUE oppositeValue) {
        for (int i = 0; i < king.city.kingdom.discoveredKingdoms.Count; i++) {
            Citizen otherKing = king.city.kingdom.discoveredKingdoms[i].king;
            RelationshipKings otherKingRel = otherKing.GetRelationshipWithCitizen(king);
            if (otherKing.importantCharacterValues.ContainsKey(chosenValue) 
                || otherKing.importantCharacterValues.ContainsKey(oppositeValue)) {

                KeyValuePair<CHARACTER_VALUE, int> priotiyValue = otherKing.importantCharacterValues
                               .FirstOrDefault(x => x.Key == chosenValue || x.Key == oppositeValue);
                if (priotiyValue.Key == chosenValue) {
                    otherKingRel.AddEventModifier(20, "Evil Intent Opinion", this);
                } else {
                    otherKingRel.AddEventModifier(-20, "Evil Intent Opinion", this);
                }
            } else {
                otherKingRel.AddEventModifier(-20, "Evil Intent Opinion", this);
            }
        }
    }
}
