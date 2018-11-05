using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanditRaid : Interaction {

    private BaseLandmark chosenLandmarkToRaid;
    private BaseLandmark originLandmark;

    private WeightedDictionary<LandmarkDefender> assaultSpawnWeights;
    private WeightedDictionary<LandmarkDefender> firstElementAssaultSpawnWeights; //TODO: Make this more elegant!

    public BanditRaid(IInteractable interactable) : base(interactable, INTERACTION_TYPE.BANDIT_RAID, 70) {
        _name = "Bandit Raid";
    }

    #region Overrides
    public override void CreateStates() {
        if (_interactable is BaseLandmark) {
            originLandmark = interactable as BaseLandmark;
            CreateExploreStates();
            CreateWhatToDoNextState("What do you want %minion% to do next?");
            ConstructAssaultSpawnWeights();
            chosenLandmarkToRaid = GetLandmarkToRaid(originLandmark);
            
            InteractionState startState = new InteractionState("State 1", this);
            //string startStateDesc = "The bandits are preparing to raid " + chosenLandmarkToRaid.landmarkName;
            //startState.SetDescription(startStateDesc);
            
            //action option states
            InteractionState endResult1State = new InteractionState("Raid", this); //raid
            InteractionState endResult2State = new InteractionState("Successfully Cancelled Raid", this); //successfully cancelled raid
            InteractionState endResult3State = new InteractionState("Failed to Cancel Raid", this); //failed to cancel raid
            InteractionState endResult4State = new InteractionState("Critically Failed to Cancel Raid", this); //critically failed to cancel raid
            InteractionState endResult5State = new InteractionState("Empowered Raid", this); //empowered raid
            InteractionState endResult6State = new InteractionState("Misused Funds", this); //misused funds
            InteractionState endResult7State = new InteractionState("Demon Dies", this); //demon dies

            CreateActionOptions(startState);
            CreateActionOptions(endResult1State);
            CreateActionOptions(endResult2State);
            CreateActionOptions(endResult3State);
            CreateActionOptions(endResult4State);
            CreateActionOptions(endResult5State);
            CreateActionOptions(endResult6State);

            endResult7State.SetEndEffect(() => DemonDiesRewardEffect(endResult7State));

            _states.Add(startState.name, startState);
            _states.Add(endResult1State.name, endResult1State);
            _states.Add(endResult2State.name, endResult2State);
            _states.Add(endResult3State.name, endResult3State);
            _states.Add(endResult4State.name, endResult4State);
            _states.Add(endResult5State.name, endResult5State);
            _states.Add(endResult6State.name, endResult6State);
            _states.Add(endResult7State.name, endResult7State);

            SetCurrentState(startState);
        }
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "State 1") {
            ActionOption stopThemFromAttacking = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Stop them from attacking.",
                //description = "We have sent %minion% to prevent the bandits from raiding " + chosenLandmarkToRaid.landmarkName + ".",
                duration = 0,
                needsMinion = false,
                effect = () => StopThemFromAttackingEffect(state),
            };
            ActionOption provideSomeAssistance = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 100, currency = CURRENCY.SUPPLY },
                name = "Provide them some assistance.",
                //description = "We have sent %minion% to send the bandits some Supplies to aid their upcoming raid of " + chosenLandmarkToRaid.landmarkName + ".",
                duration = 0,
                needsMinion = false,
                neededObjects = new List<System.Type>() { typeof(IUnit) },
                effect = () => ProvideThemSomeAssistanceEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                //description = "The bandits are preparing to raid " + chosenLandmarkToRaid.landmarkName + ".",
                duration = 0,
                needsMinion = false,
                effect = () => WhatToDoNextState(),
            };

            state.AddActionOption(stopThemFromAttacking);
            state.AddActionOption(provideSomeAssistance);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        } else {
            ActionOption continueSurveillance = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption returnToMe = new ActionOption {
                interactionState = state,
                cost = new ActionOptionCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Return to me.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreEndsOption(state),
            };
            state.AddActionOption(continueSurveillance);
            state.AddActionOption(returnToMe);
            state.SetDefaultOption(returnToMe);
        }
    }
    #endregion

    private BaseLandmark GetLandmarkToRaid(BaseLandmark originLandmark) {
        List<HexTile> surrounding = originLandmark.tileLocation.GetTilesInRange(8);
        List<BaseLandmark> choices = new List<BaseLandmark>();
        for (int i = 0; i < surrounding.Count; i++) {
            HexTile currTile = surrounding[i];
            if (currTile.landmarkOnTile != null) {
                if (currTile.landmarkOnTile.owner == null || currTile.landmarkOnTile.owner.id != originLandmark.owner.id) {
                    //select a location within 8 tile distance around the camp owned by a different faction
                    choices.Add(currTile.landmarkOnTile);
                }
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }

    private void StopThemFromAttackingEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Successfully Cancelled Raid", 25);
        effectWeights.AddElement("Failed to Cancel Raid", 10);
        effectWeights.AddElement("Critically Failed to Cancel Raid", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        //chosenEffect = "Critically Failed to Cancel Raid";
        if (chosenEffect == "Successfully Cancelled Raid") {
            SuccessfullyCancelledRaid(state, chosenEffect);
        } else if (chosenEffect == "Failed to Cancel Raid") {
            FailedToCancelRaid(state, chosenEffect);
        } else if (chosenEffect == "Critically Failed to Cancel Raid") {
            CriticalFailToCancelRaid(state, chosenEffect);
        }
    }
    private void ProvideThemSomeAssistanceEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Empowered Raid", 25);
        effectWeights.AddElement("Misused Funds", 10);
        effectWeights.AddElement("Demon Dies", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Empowered Raid") {
            EmpoweredRaid(state, chosenEffect);
        } else if (chosenEffect == "Misused Funds") {
            MisusedFunds(state, chosenEffect);
        } else if (chosenEffect == "Demon Dies") {
            DemonDies(state, chosenEffect);
        }
    }
    private void DoNothingEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Raid", 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Raid") {
            Raid(state, chosenEffect);
        }
    }

    private void SuccessfullyCancelledRaid(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " intimidated the bandits into stopping their attack. " +
        //    "What do you want " + _interactable.explorerMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        SuccessfullyCancelledRaidRewardEffect(_states[effectName]);
    }
    private void SuccessfullyCancelledRaidRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    private void FailedToCancelRaid(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " failed to stop the bandits from proceeding with their raid. " +
        //    "A group of bandits have left " + originLandmark.landmarkName + " " +
        //    "to raid " + chosenLandmarkToRaid.landmarkName + ". What do you want " + _interactable.explorerMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        FailedToCancelRaidRewardEffect(_states[effectName]);
    }
    private void FailedToCancelRaidRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    private void CriticalFailToCancelRaid(InteractionState state, string effectName) {
        BaseLandmark targetLandmark = PlayerManager.Instance.player.playerArea.GetRandomExposedLandmark();
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " failed to stop the bandits from proceeding with their raid. " +
        //    "Worse, they were so riled up by the demon that they decided to attack you instead. A group of bandits have left " + 
        //    originLandmark.landmarkName + " to attack " + targetLandmark.name + ". What do you want " + _interactable.explorerMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        //create a 3 army attack unit from Assault Spawn Weights 1. Change target to your area instead.
        CharacterParty army = CreateAssaultArmy(3);
        //force spawned army to raid target
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK);
        army.iactionData.AssignAction(characterAction, targetLandmark.landmarkObj);
        CriticalFailToCancelRaidRewardEffect(_states[effectName]);
    }
    private void CriticalFailToCancelRaidRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }

    private void EmpoweredRaid(InteractionState state, string effectName) {
        //_states[effectName].SetDescription("We provided the bandits with more supplies which they have gladly used to build a " +
        //    "bigger raid group than they initially planned. They have now left " +  originLandmark.landmarkName + " to raid " + 
        //    chosenLandmarkToRaid.name + ". What do you want " + _interactable.explorerMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        //create a 4 army attack unit from Assault Spawn Weights 1.
        CharacterParty army = CreateAssaultArmy(4);
        //force spawned army to raid target
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        army.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        EmpoweredRaidRewardEffect(_states[effectName]);
    }
    private void EmpoweredRaidRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    private void MisusedFunds(InteractionState state, string effectName) {
        //_states[effectName].SetDescription("We provided the bandits with more supplies but it doesn't look they used it for the " +
        //    "attack. They have now left " + originLandmark.landmarkName + " to raid " + chosenLandmarkToRaid.landmarkName +
        //    " but with a smaller group than we anticipated. What do you want " + _interactable.explorerMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        //Spawn attackers create a 3 army attack unit from Assault Spawn Weights 1.
        CharacterParty army = CreateAssaultArmy(3);
        //force spawned army to raid target
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        army.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
        MisusedFundsRewardEffect(_states[effectName]);
    }
    private void MisusedFundsRewardEffect(InteractionState state) {
        _interactable.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
    }
    private void DemonDies(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(_interactable.explorerMinion.name + " has not returned. We can only assume the worst.");
        SetCurrentState(_states[effectName]);
    }
    private void DemonDiesRewardEffect(InteractionState state) {
        //**Effect**: Demon is removed from Minion List
        PlayerManager.Instance.player.RemoveMinion(_interactable.explorerMinion);
    }

    private void Raid(InteractionState state, string effectName) {
        //_states[effectName].SetDescription("A group of bandits have left " + 
        //    originLandmark.landmarkName + " to raid " + chosenLandmarkToRaid.landmarkName + 
        //    ". What do you want " + _interactable.explorerMinion.name + " to do next?");
        SetCurrentState(_states[effectName]);
        //create a 3 army attack unit from Assault Spawn Weights 1
        CharacterParty army = CreateAssaultArmy(3);
        //force spawned army to raid target
        CharacterAction characterAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.RAID_LANDMARK);
        army.iactionData.AssignAction(characterAction, chosenLandmarkToRaid.landmarkObj);
    }
    private void RaidEffect(InteractionState state) {

    }

    private void ConstructAssaultSpawnWeights() {
        assaultSpawnWeights = new WeightedDictionary<LandmarkDefender>();
        firstElementAssaultSpawnWeights = new WeightedDictionary<LandmarkDefender>();

        LandmarkDefender marauder = new LandmarkDefender() {
            className = "Marauder",
            armyCount = 25
        };
        LandmarkDefender bowman = new LandmarkDefender() {
            className = "Bowman",
            armyCount = 25
        };
        LandmarkDefender healer = new LandmarkDefender() {
            className = "Healer",
            armyCount = 25
        };

        firstElementAssaultSpawnWeights.AddElement(marauder, 35);
        firstElementAssaultSpawnWeights.AddElement(bowman, 20);

        assaultSpawnWeights.AddElement(marauder, 35);
        assaultSpawnWeights.AddElement(bowman, 20);
        assaultSpawnWeights.AddElement(healer, 10);
    }

    private CharacterParty CreateAssaultArmy(int unitCount) {
        CharacterParty army = null;
        for (int i = 0; i < unitCount; i++) {
            LandmarkDefender chosenDefender;
            if (i == 0) {
                chosenDefender = firstElementAssaultSpawnWeights.PickRandomElementGivenWeights();
            } else {
                chosenDefender = assaultSpawnWeights.PickRandomElementGivenWeights();
            }
            CharacterArmyUnit armyUnit = CharacterManager.Instance.CreateCharacterArmyUnit(originLandmark.owner.race, chosenDefender, originLandmark.owner, originLandmark);
            if (army == null) {
                army = armyUnit.party as CharacterParty;
            } else {
                army.AddCharacter(armyUnit);
            }
        }
        return army;
    }
}
