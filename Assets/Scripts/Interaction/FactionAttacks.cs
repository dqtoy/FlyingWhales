using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class FactionAttacks : Interaction {

    private Area _targetArea;

    public FactionAttacks(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.FACTION_ATTACKS, 70) {
        _name = "Faction Attacks";
    }

    public void SetTargetArea(Area targetArea) {
        _targetArea = targetArea;
    }
    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(null, interactable.name, LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(null, _targetArea.name, LOG_IDENTIFIER.STRING_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);


        InteractionState attackStoppedState = new InteractionState("Attack Stopped", this);
        InteractionState attackContinuesState = new InteractionState("Attack Continues", this);
        InteractionState successfulEmpowermentState = new InteractionState("Successful Empowerment", this);
        InteractionState failedEmpowermentState = new InteractionState("Failed Empowerment", this);
        InteractionState redirectedAttackState = new InteractionState("Redirected Attack", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        CreateActionOptions(startState);

        attackStoppedState.SetEffect(() => AttackStoppedEffect(attackStoppedState));
        attackContinuesState.SetEffect(() => AttackContinuesEffect(attackContinuesState));
        successfulEmpowermentState.SetEffect(() => SuccessfulEmpowermentEffect(successfulEmpowermentState));
        failedEmpowermentState.SetEffect(() => FailedEmpowermentEffect(failedEmpowermentState));
        redirectedAttackState.SetEffect(() => RedirectedEffect(redirectedAttackState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(attackStoppedState.name, attackStoppedState);
        _states.Add(attackContinuesState.name, attackContinuesState);
        _states.Add(successfulEmpowermentState.name, successfulEmpowermentState);
        _states.Add(failedEmpowermentState.name, failedEmpowermentState);
        _states.Add(redirectedAttackState.name, redirectedAttackState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop them from attacking.",
                duration = 0,
                effect = () => StopOption(state),
            };
            ActionOption ritualOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.MANA },
                name = "Perform a ritual to empower their attacking units.",
                duration = 0,
                effect = () => RitualOption(state),
            };
            ActionOption redirectOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Redirect their attack.",
                duration = 0,
                neededObjects = new List<System.Type>() { typeof(LocationIntel) },
                effect = () => RedirectOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(state),
            };

            state.AddActionOption(stopOption);
            state.AddActionOption(ritualOption);
            state.AddActionOption(redirectOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    private void StopOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Attack Stopped", 20);
        effectWeights.AddElement("Attack Continues", 8);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void RitualOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Successful Empowerment", 20);
        effectWeights.AddElement("Failed Empowerment", 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void RedirectOption(InteractionState state) {
        SetCurrentState(_states["Redirected Attack"]);
    }
    private void DoNothingOption(InteractionState state) {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Do Nothing", 15);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Do Nothing"]);
    }

    #region State Effects
    private void AttackStoppedEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        state.AddLogFiller(new LogFiller(interactable.faction, interactable.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.STRING_1));
    }
    private void AttackContinuesEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        state.AddLogFiller(new LogFiller(interactable.faction.leader, interactable.faction.leader.name, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.STRING_1));

        AttackTargetArea();
    }
    private void SuccessfulEmpowermentEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        interactable.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);
        //Add Empowered Trait to Attacking Units - just one attacking unit or all?

        state.AddLogFiller(new LogFiller(interactable.faction, interactable.faction.name, LOG_IDENTIFIER.FACTION_1));

        AttackTargetArea();
    }
    private void FailedEmpowermentEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));

        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 95) {
            WeakenedUnits(state);
            AttackTargetArea();
        } else {
            //Will attack own settlement, attack on target will not continue
            ConfusedUnits(state);
        }
    }
    private void RedirectedEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        SetTargetArea(state.assignedLocation.location);
        state.AddLogFiller(new LogFiller(interactable.faction, interactable.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.STRING_1));
        AttackTargetArea();
    }
    private void DoNothingEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        state.AddLogFiller(new LogFiller(interactable.faction, interactable.faction.name, LOG_IDENTIFIER.FACTION_1));
        AttackTargetArea();
    }


    #endregion
    private void WeakenedUnits(InteractionState state) {
        state.AddLogFiller(new LogFiller(interactable.faction, interactable.faction.name, LOG_IDENTIFIER.FACTION_1));

        interactable.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, -3);
        //Add Weakened Trait to Attacking Units - just one attacking unit or all?
        //How do logs work? How can I override it?
    }
    private void ConfusedUnits(InteractionState state) {
        //Start attacking own settlement

        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 60) {
            FactionRelationship relationship = interactable.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction);
            relationship.ChangeRelationshipStatus(FACTION_RELATIONSHIP_STATUS.AT_WAR);
        }
    }
    private void AttackTargetArea(Party assaultParty = null) {
        //TODO
        BaseLandmark targetLandmark = null;
        float highestWinRate = 0f;
        List<BaseLandmark> areaExposedTiles = _targetArea.exposedTiles;
        for (int i = 0; i < areaExposedTiles.Count; i++) {
            BaseLandmark candidate = areaExposedTiles[i];
            float winRate = 0f;
            float loseRate = 0f;
            //if(candidate.defenders != null) {
            //    CombatManager.Instance.GetCombatChanceOfTwoLists(assaultParty.icharacters, candidate.defenders.icharacters, out winRate, out loseRate);
            //} else {
            //    CombatManager.Instance.GetCombatChanceOfTwoLists(assaultParty.icharacters, null, out winRate, out loseRate);
            //}
            if(winRate >= 30f) {
                if(targetLandmark == null) {
                    targetLandmark = candidate;
                    highestWinRate = winRate;
                } else {
                    if(winRate > highestWinRate) {
                        targetLandmark = candidate;
                        highestWinRate = winRate;
                    }
                }
            }
        }

        if (targetLandmark != null) {
            //Attack landmark
        }
    }
}
