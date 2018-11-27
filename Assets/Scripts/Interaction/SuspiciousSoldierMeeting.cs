using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class SuspiciousSoldierMeeting : Interaction {

    public SuspiciousSoldierMeeting(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING, 80) {
        _name = "Suspicious Soldier Meeting";
    }

    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState("%minion% stopped keeping track of the soldiers' whereabouts. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName +"?");
        InteractionState startState = new InteractionState("Start", this);
        InteractionState reduceDefendersState = new InteractionState("Reduce Defenders", this);
        InteractionState warDeclaredState = new InteractionState("War Declared", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        InteractionState armyGainedState = new InteractionState("Army Gained", this);
        InteractionState nothingHappensState = new InteractionState("Nothing Happens", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        //string startStateDesc = "%minion% has discovered a group of soldiers leaving the Garrison and meeting in secret.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(reduceDefendersState);
        //CreateActionOptions(warDeclaredState);
        //CreateActionOptions(generalDiesState);
        //CreateActionOptions(nothingHappensState);

        reduceDefendersState.SetEndEffect(() => ReduceDefendersRewardEffect(reduceDefendersState));
        warDeclaredState.SetEndEffect(() => WarDeclaredRewardEffect(warDeclaredState));
        demonDisappearsState.SetEndEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        armyGainedState.SetEndEffect(() => ArmyGainedRewardEffect(armyGainedState));
        nothingHappensState.SetEndEffect(() => NothingHappensRewardEffect(nothingHappensState));
        doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(reduceDefendersState.name, reduceDefendersState);
        _states.Add(warDeclaredState.name, warDeclaredState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        _states.Add(armyGainedState.name, armyGainedState);
        _states.Add(nothingHappensState.name, nothingHappensState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 30, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon.",
                //description = "We have sent %minion% to watch the soldiers and follow them on their next secret meeting.",
                duration = 0,
                needsMinion = false,
                effect = () => SendOutDemonOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingOption(state),
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
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Continue surveillance of the area.",
                duration = 0,
                needsMinion = false,
                effect = () => ExploreContinuesOption(state),
            };
            ActionOption returnToMeOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
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
        effectWeights.AddElement("Army Gained", 5);
        if (_interactable.faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus != FACTION_RELATIONSHIP_STATUS.AT_WAR) {
            effectWeights.AddElement("War Declared", 5);
        }
        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == "Reduce Defenders") {
        //    ReduceDefendersRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Demon Disappears") {
        //    DemonDisappearsRewardState(state, chosenEffect);
        //} else if (chosenEffect == "War Declared") {
        //    WarDeclaredRewardState(state, chosenEffect);
        //} else if (chosenEffect == "General Dies") {
        //    GeneralDiesRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Nothing Happens") {
        //    NothingHappensRewardState(state, chosenEffect);
        //}
    }
    private void DoNothingOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Do Nothing", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Left Alone") {
        //    LeftAloneRewardState(state, chosenEffect);
        //}
    }

    #region States
    private void ReduceDefendersRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " was able to tempt the disgruntled soldiers to abandon their posts. A significant amount of defenders are now missing from the Garrison!");
        SetCurrentState(_states[stateName]);
        ReduceDefendersRewardEffect(_states[stateName]);
    }
    private void WarDeclaredRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " was discovered by the soldiers! He managed to run away unscathed but " + _interactable.faction.name + " is now aware of our sabotage attempts and have declared war upon us.");
        SetCurrentState(_states[stateName]);
        WarDeclaredRewardEffect(_states[stateName]);
    }
    private void GeneralDiesRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " discovered that the soldiers were planning a surprise party for the General's birthday. Knowing that the General is alone now, he slipped in and assassinated him successfully.");
        SetCurrentState(_states[stateName]);
        ArmyGainedRewardEffect(_states[stateName]);
    }
    private void NothingHappensRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " followed the soldiers and found that they were merely having a drunken party away from their superiors.");
        SetCurrentState(_states[stateName]);
        NothingHappensRewardEffect(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void ReduceDefendersRewardEffect(InteractionState state) {
        //Each Defender slot in the Garrison loses a random percentage between 15% to 50%
        BaseLandmark landmark = _interactable;
        if(landmark.defenders != null) {
            if(landmark.defenders.icharacters.Count >= 2) {
                int numOfDeserters = UnityEngine.Random.Range(1, 3);
                if (numOfDeserters == 1) {
                    ICharacter deserter = landmark.defenders.icharacters[UnityEngine.Random.Range(0, landmark.defenders.icharacters.Count)];
                    landmark.RemoveDefender(deserter);
                    //if (state.minionLog != null) {
                    //    state.minionLog.AddToFillers(deserter, deserter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //}
                    state.AddLogFiller(new LogFiller(deserter, deserter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
                } else {
                    List<ICharacter> icharacters = new List<ICharacter>(landmark.defenders.icharacters);
                    int deserter1Index = UnityEngine.Random.Range(0, icharacters.Count);
                    ICharacter deserter1 = icharacters[deserter1Index];
                    icharacters.RemoveAt(deserter1Index);
                    ICharacter deserter2 = icharacters[UnityEngine.Random.Range(0, icharacters.Count)];

                    landmark.RemoveDefender(deserter1);
                    landmark.RemoveDefender(deserter2);

                    state.AddLogFiller(new LogFiller(deserter1, deserter1.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));

                    //this is for deserter2
                    Log newMinionLog = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_log1");
                    newMinionLog.AddToFillers(explorerMinion, explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
                    newMinionLog.AddToFillers(interactable.specificLocation.tileLocation.landmarkOnTile, interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
                    newMinionLog.AddToFillers(deserter2, deserter2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    newMinionLog.AddLogToInvolvedObjects();

                    //if (state.minionLog != null) {
                    //    state.minionLog.AddToFillers(deserter1, deserter1.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

                    //    Log newMinionLog = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), _name.ToLower() + "_logminion");
                    //    newMinionLog.AddToFillers(explorerMinion, explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
                    //    newMinionLog.AddToFillers(interactable.specificLocation.tileLocation.landmarkOnTile, interactable.specificLocation.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
                    //    newMinionLog.AddToFillers(deserter2, deserter2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //    explorerMinion.icharacter.AddHistory(newMinionLog);
                    //}
                }
            } else if(landmark.defenders.icharacters.Count == 1) {
                ICharacter deserter = landmark.defenders.icharacters[0];
                landmark.RemoveDefender(deserter);
                //if (state.minionLog != null) {
                //    state.minionLog.AddToFillers(deserter, deserter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //}
                state.AddLogFiller(new LogFiller(deserter, deserter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
            }
                
            //for (int i = 0; i < landmark.defenders.icharacters.Count; i++) {
                    
                //if(landmark.defenders.icharacters[i] is CharacterArmyUnit) {
                //    CharacterArmyUnit defenderArmy = landmark.defenders.icharacters[i] as CharacterArmyUnit;
                //    int percentageLoss = UnityEngine.Random.Range(15, 51);
                //    float percentage = percentageLoss / 100f;
                //    int loss = (int)(defenderArmy.armyCount * percentage);
                //    defenderArmy.AdjustArmyCount(-loss);
                //} else if (landmark.defenders.icharacters[i] is MonsterArmyUnit) {
                //    MonsterArmyUnit defenderArmy = landmark.defenders.icharacters[i] as MonsterArmyUnit;
                //    int percentageLoss = UnityEngine.Random.Range(15, 51);
                //    float percentage = percentageLoss / 100f;
                //    int loss = (int) (defenderArmy.armyCount * percentage);
                //    defenderArmy.AdjustArmyCount(-loss);
                //}
            //}
        }
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));


    }
    private void WarDeclaredRewardEffect(InteractionState state) {
        //Tile owner faction will declare war on player
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        FactionManager.Instance.DeclareWarBetween(_interactable.faction, PlayerManager.Instance.player.playerFaction);
        //if(state.descriptionLog != null) {
        //    state.descriptionLog.AddToFillers(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1);
        //}
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1);
        //}
        state.AddLogFiller(new LogFiller(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void ArmyGainedRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion("Knights", RACE.HUMANS, false);
        newMinion.icharacter.SetLevel(UnityEngine.Random.Range(5, 9));
        PlayerManager.Instance.player.AddMinion(newMinion);

        //if (_interactable is BaseLandmark) {
        //    BaseLandmark landmark = _interactable as BaseLandmark;
        //    ICharacter icharacter = landmark.GetResidentCharacterOfClass("General");
        //    if (icharacter != null) {
        //        icharacter.Assassinate(explorerMinion.icharacter);
        //    }
        //}
    }
    private void NothingHappensRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
    #endregion
}
