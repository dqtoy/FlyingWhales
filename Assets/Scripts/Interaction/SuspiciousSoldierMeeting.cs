using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SuspiciousSoldierMeeting : Interaction {

    public SuspiciousSoldierMeeting(Area interactable) : base(interactable, INTERACTION_TYPE.SUSPICIOUS_SOLDIER_MEETING, 80) {
        _name = "Suspicious Soldier Meeting";
    }

    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState("%minion% stopped keeping track of the soldiers' whereabouts. Do you want him to continue surveillance of " + _interactable.thisName +"?");
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

        reduceDefendersState.SetEffect(() => ReduceDefendersRewardEffect(reduceDefendersState));
        warDeclaredState.SetEffect(() => WarDeclaredRewardEffect(warDeclaredState));
        demonDisappearsState.SetEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        armyGainedState.SetEffect(() => ArmyGainedRewardEffect(armyGainedState));
        nothingHappensState.SetEffect(() => NothingHappensRewardEffect(nothingHappensState));
        doNothingState.SetEffect(() => DoNothingRewardEffect(doNothingState));

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
                effect = () => SendOutDemonOption(state),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(state),
            };

            state.AddActionOption(sendOutDemonOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
            //GameDate scheduleDate = GameManager.Instance.Today();
            //scheduleDate.AddHours(60);
            //state.SetTimeSchedule(doNothingOption, scheduleDate);
        }
    }
    #endregion

    private void SendOutDemonOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Reduce Defenders", 30);
        effectWeights.AddElement("Demon Disappears", 5);
        effectWeights.AddElement("Nothing Happens", 15);
        effectWeights.AddElement("Army Gained", 5);
        if (_interactable.owner.GetRelationshipWith(PlayerManager.Instance.player.playerFaction).relationshipStatus != FACTION_RELATIONSHIP_STATUS.ENEMY) {
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
        DefenderGroup randomGroup = interactable.GetRandomDefenderGroup();
        if(randomGroup != null) {
            if(randomGroup.party.characters.Count >= 2) {
                int numOfDeserters = UnityEngine.Random.Range(1, 3);
                if (numOfDeserters == 1) {
                    Character deserter = randomGroup.party.characters[UnityEngine.Random.Range(0, randomGroup.party.characters.Count)];
                    //landmark.RemoveDefender(deserter);
                    //if (state.minionLog != null) {
                    //    state.minionLog.AddToFillers(deserter, deserter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //}
                    state.AddLogFiller(new LogFiller(deserter, deserter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
                } else {
                    List<Character> icharacters = new List<Character>(randomGroup.party.characters);
                    int deserter1Index = UnityEngine.Random.Range(0, icharacters.Count);
                    Character deserter1 = icharacters[deserter1Index];
                    icharacters.RemoveAt(deserter1Index);
                    Character deserter2 = icharacters[UnityEngine.Random.Range(0, icharacters.Count)];

                    //landmark.RemoveDefender(deserter1);
                    //landmark.RemoveDefender(deserter2);

                    state.AddLogFiller(new LogFiller(deserter1, deserter1.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));

                    //this is for deserter2
                    Log newMinionLog = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_log1");
                    newMinionLog.AddToFillers(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
                    newMinionLog.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1);
                    newMinionLog.AddToFillers(deserter2, deserter2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    newMinionLog.AddLogToInvolvedObjects();

                    //if (state.minionLog != null) {
                    //    state.minionLog.AddToFillers(deserter1, deserter1.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

                    //    Log newMinionLog = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), _name.ToLower() + "_logminion");
                    //    newMinionLog.AddToFillers(explorerMinion, explorerMinion.name, LOG_IDENTIFIER.MINION_NAME);
                    //    newMinionLog.AddToFillers(interactable.tileLocation.landmarkOnTile, interactable.tileLocation.landmarkOnTile.name, LOG_IDENTIFIER.LANDMARK_1);
                    //    newMinionLog.AddToFillers(deserter2, deserter2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //    explorerMinion.icharacter.AddHistory(newMinionLog);
                    //}
                }
            } else if(randomGroup.party.characters.Count == 1) {
                Character deserter = randomGroup.party.characters[0];
                //landmark.RemoveDefender(deserter);
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
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));


    }
    private void WarDeclaredRewardEffect(InteractionState state) {
        //Tile owner faction will declare war on player
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        FactionManager.Instance.DeclareWarBetween(_interactable.owner, PlayerManager.Instance.player.playerFaction);
        //if(state.descriptionLog != null) {
        //    state.descriptionLog.AddToFillers(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1);
        //}
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(_interactable.faction, _interactable.faction.name, LOG_IDENTIFIER.FACTION_1);
        //}
        state.AddLogFiller(new LogFiller(_interactable.owner, _interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void ArmyGainedRewardEffect(InteractionState state) {
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        Minion newMinion = PlayerManager.Instance.player.CreateNewMinion("Knights", RACE.HUMANS);
        newMinion.character.SetLevel(UnityEngine.Random.Range(5, 9));
        PlayerManager.Instance.player.AddMinion(newMinion);

        //if (_interactable is BaseLandmark) {
        //    BaseLandmark landmark = _interactable;
        //    ICharacter icharacter = landmark.GetResidentCharacterOfClass("General");
        //    if (icharacter != null) {
        //        icharacter.Assassinate(explorerMinion.icharacter);
        //    }
        //}
    }
    private void NothingHappensRewardEffect(InteractionState state) {
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
    #endregion
}
