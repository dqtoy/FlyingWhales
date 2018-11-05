using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryHum : Interaction {
    public MysteryHum(IInteractable interactable) : base(interactable, INTERACTION_TYPE.MYSTERY_HUM, 70) {
        _name = "Mystery Hum";
    }

    #region Overrides
    public override void CreateStates() {
        CreateExploreStates();
        CreateWhatToDoNextState("%minion% ignored the humming in the area. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState demonAttacksState = new InteractionState("Demon Attacks", this);
        InteractionState armyRecruitedState = new InteractionState("Army Recruited", this);

        //string startStateDesc = "%minion% reports of some mysterious humming coming from within the caves.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(demonAttacksState);
        CreateActionOptions(armyRecruitedState);

        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        demonAttacksState.SetEndEffect(() => DemonAttacksRewardEffect(demonAttacksState));
        //armyRecruitedState.SetEndEffect(() => ArmyRecruitedRewardEffect(armyRecruitedState));

        _states.Add(startState.name, startState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(demonAttacksState.name, demonAttacksState);
        _states.Add(armyRecruitedState.name, armyRecruitedState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon.",
                //description = "We have sent %minion% to investigate the source of the mysterious humming.",
                duration = 0,
                needsMinion = true,
                effect = () => SendOutDemonEffect(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave it alone.",
                duration = 1,
                needsMinion = false,
                effect = () => WhatToDoNextState(),
            };

            state.AddActionOption(sendOutDemonOption);
            state.AddActionOption(doNothingOption);

            //GameDate scheduleDate = GameManager.Instance.Today();
            //scheduleDate.AddHours(150);
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
    private void SendOutDemonEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Demon Disappears", 10);
        effectWeights.AddElement("Demon Attacks", 10);
        effectWeights.AddElement("Army Recruited", 30);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Demon Disappears") {
            DemonDisappearsRewardState(state, chosenEffect);
        } else if (chosenEffect == "Demon Attacks") {
            DemonAttacksRewardState(state, chosenEffect);
        } else if (chosenEffect == "Army Recruited") {
            ArmyRecruitedRewardState(state, chosenEffect);
        }
    }
    private void DemonAttacksRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription("After " + _interactable.explorerMinion.name + " investigated the hum, it returned with a strange glow in its eyes. It is now attacking us!");
        SetCurrentState(_states[effectName]);
    }
    private void ArmyRecruitedRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription("The mysterious hum came from a group of Zombie Earthbinders staying within a hidden section of the cave. We have bent them to our will and they now join our army.");
        SetCurrentState(_states[effectName]);
        ArmyRecruitedRewardEffect(_states[effectName]);
    }
    private void DemonAttacksRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.RemoveMinion(_interactable.explorerMinion);
        _interactable.explorerMinion.SetEnabledState(true);

        List<BaseLandmark> playerLandmarks = PlayerManager.Instance.player.demonicPortal.tileLocation.areaOfTile.landmarks;
        BaseLandmark playerLandmarkToAttack = playerLandmarks[UnityEngine.Random.Range(0, playerLandmarks.Count)];
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        _interactable.explorerMinion.icharacter.currentParty.iactionData.AssignAction(characterAction, playerLandmarkToAttack.landmarkObj);
    }
    private void ArmyRecruitedRewardEffect(InteractionState state) {
        if (!PlayerManager.Instance.player.areMinionsMaxed) {
            CharacterArmyUnit zombieEarthbinders = CharacterManager.Instance.CreateCharacterArmyUnit("Earthbinder", RACE.ZOMBIE, PlayerManager.Instance.player.playerFaction, PlayerManager.Instance.player.demonicPortal);
            zombieEarthbinders.LevelUp(5);
            PlayerManager.Instance.player.AddMinion(new Minion(zombieEarthbinders, PlayerManager.Instance.player.GetAbility("Inspect")));
        }
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
}
