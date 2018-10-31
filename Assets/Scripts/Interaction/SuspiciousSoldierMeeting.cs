using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspiciousSoldierMeeting : Interaction {

    public SuspiciousSoldierMeeting(IInteractable interactable) : base(interactable, INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING, 60) {
        _name = "Suspicious Soldier Meeting";
    }

    #region Overrides
    public override void CreateStates() {
        CreateExploreStates();
        CreateWhatToDoNextState(_interactable.explorerMinion.name + " stopped keeping track of the soldiers' whereabouts. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName +"?");
        InteractionState startState = new InteractionState("Start", this);
        InteractionState reduceDefendersState = new InteractionState("Reduce Defenders", this);
        InteractionState warDeclaredState = new InteractionState("War Declared", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState generalDiesState = new InteractionState("General Dies", this);
        InteractionState nothingHappensState = new InteractionState("Nothing Happens", this);

        string startStateDesc = _interactable.explorerMinion.name + " has discovered a group of soldiers leaving the Garrison and meeting in secret.";
        startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        CreateActionOptions(reduceDefendersState);
        CreateActionOptions(warDeclaredState);
        CreateActionOptions(generalDiesState);
        CreateActionOptions(nothingHappensState);

        //reduceDefendersState.SetEndEffect(() => ReduceDefendersRewardEffect(reduceDefendersState));
        //warDeclaredState.SetEndEffect(() => WarDeclaredRewardEffect(warDeclaredState));
        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        //generalDiesState.SetEndEffect(() => GeneralDiesRewardEffect(generalDiesState));
        //nothingHappensState.SetEndEffect(() => NothingHappensRewardEffect(nothingHappensState));

        _states.Add(startState.name, startState);
        _states.Add(reduceDefendersState.name, reduceDefendersState);
        _states.Add(warDeclaredState.name, warDeclaredState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(generalDiesState.name, generalDiesState);
        _states.Add(nothingHappensState.name, nothingHappensState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon.",
                description = "We have sent %minion% to watch the soldiers and follow them on their next secret meeting.",
                duration = 0,
                needsMinion = false,
                effect = () => SendOutDemonOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => WhatToDoNextState(),
            };

            state.AddActionOption(sendOutDemonOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
            //GameDate scheduleDate = GameManager.Instance.Today();
            //scheduleDate.AddHours(60);
            //state.SetTimeSchedule(doNothingOption, scheduleDate);
        } else {
            ActionOption continueSurveillanceOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption returnToMeOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Return to me.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreEndsOption(state),
            };

            state.AddActionOption(continueSurveillanceOption);
            state.AddActionOption(returnToMeOption);
            state.SetDefaultOption(returnToMeOption);
        }
    }
    #endregion

    private void SendOutDemonOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Reduce Defenders", 30);
        effectWeights.AddElement("Demon Disappears", 5);
        effectWeights.AddElement("Nothing Happens", 15);
        if (_interactable.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
            effectWeights.AddElement("War Declared", 5);
        }
        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            if (landmark.GetResidentCharacterOfClass("General") != null) {
                effectWeights.AddElement("General Dies", 5);
            }
        }

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Reduce Defenders") {
            ReduceDefendersRewardState(state, chosenEffect);
        } else if (chosenEffect == "Demon Disappears") {
            DemonDisappearsRewardState(state, chosenEffect);
        } else if (chosenEffect == "War Declared") {
            WarDeclaredRewardState(state, chosenEffect);
        } else if (chosenEffect == "General Dies") {
            GeneralDiesRewardState(state, chosenEffect);
        } else if (chosenEffect == "Nothing Happens") {
            NothingHappensRewardState(state, chosenEffect);
        }
    }

    #region States
    private void ReduceDefendersRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(_interactable.explorerMinion.name + " was able to tempt the disgruntled soldiers to abandon their posts. A significant amount of defenders are now missing from the Garrison!");
        SetCurrentState(_states[stateName]);
        ReduceDefendersRewardEffect(_states[stateName]);
    }
    private void WarDeclaredRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(_interactable.explorerMinion.name + " was discovered by the soldiers! He managed to run away unscathed but " + _interactable.faction.name + " is now aware of our sabotage attempts and have declared war upon us.");
        SetCurrentState(_states[stateName]);
        WarDeclaredRewardEffect(_states[stateName]);
    }
    private void GeneralDiesRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(_interactable.explorerMinion.name + " discovered that the soldiers were planning a surprise party for the General's birthday. Knowing that the General is alone now, he slipped in and assassinated him successfully.");
        SetCurrentState(_states[stateName]);
        GeneralDiesRewardEffect(_states[stateName]);
    }
    private void NothingHappensRewardState(InteractionState state, string stateName) {
        _states[stateName].SetDescription(_interactable.explorerMinion.name + " followed the soldiers and found that they were merely having a drunken party away from their superiors.");
        SetCurrentState(_states[stateName]);
        NothingHappensRewardEffect(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void ReduceDefendersRewardEffect(InteractionState state) {
        //Each Defender slot in the Garrison loses a random percentage between 15% to 50%
        if(_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            if(landmark.defenders != null) {
                for (int i = 0; i < landmark.defenders.icharacters.Count; i++) {
                    if(landmark.defenders.icharacters[i] is CharacterArmyUnit) {
                        CharacterArmyUnit defenderArmy = landmark.defenders.icharacters[i] as CharacterArmyUnit;
                        int percentageLoss = UnityEngine.Random.Range(15, 51);
                        float percentage = percentageLoss / 100f;
                        int loss = (int)(defenderArmy.armyCount * percentage);
                        defenderArmy.AdjustArmyCount(-loss);
                    } else if (landmark.defenders.icharacters[i] is MonsterArmyUnit) {
                        MonsterArmyUnit defenderArmy = landmark.defenders.icharacters[i] as MonsterArmyUnit;
                        int percentageLoss = UnityEngine.Random.Range(15, 51);
                        float percentage = percentageLoss / 100f;
                        int loss = (int) (defenderArmy.armyCount * percentage);
                        defenderArmy.AdjustArmyCount(-loss);
                    }
                }
            }
        }
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    private void WarDeclaredRewardEffect(InteractionState state) {
        //Tile owner faction will declare war on player
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        FactionManager.Instance.DeclareWarBetween(_interactable.faction, PlayerManager.Instance.player.playerFaction);
    }
    private void GeneralDiesRewardEffect(InteractionState state) {
        //Resident General dies by assassination
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            ICharacter icharacter = landmark.GetResidentCharacterOfClass("General");
            if (icharacter != null) {
                icharacter.Assassinate(_interactable.explorerMinion.icharacter);
            }
        }
    }
    private void NothingHappensRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    #endregion
}
