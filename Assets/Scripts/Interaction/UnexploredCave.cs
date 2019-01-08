using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnexploredCave : Interaction {
    public UnexploredCave(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.UNEXPLORED_CAVE, 70) {
        _name = "Unexplored Cave";
    }
    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState("%minion% ignored the point of interest in the location. Do you want him to continue surveillance of " + _interactable.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState supplyState = new InteractionState("Supply", this);
        InteractionState manaState = new InteractionState("Mana", this);
        InteractionState demonDisappearsState = new InteractionState("Demon Disappears", this);
        //InteractionState demonWeaponUpgradeState = new InteractionState("Demon Weapon Upgrade", this);
        //InteractionState demonArmorUpgradeState = new InteractionState("Demon Armor Upgrade", this);
        InteractionState unleashedMonsterState = new InteractionState("Unleashed Monster", this);
        InteractionState nothingState = new InteractionState("Nothing", this);
        InteractionState leftAloneState = new InteractionState("Left Alone", this);

        //string startStateDesc = "%minion% has discovered a previously unexplored cave. We can send out a minion to explore further.";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(supplyState);
        //CreateActionOptions(manaState);
        //CreateActionOptions(demonWeaponUpgradeState);
        //CreateActionOptions(demonArmorUpgradeState);
        //CreateActionOptions(unleashedMonsterState);
        //CreateActionOptions(nothingState);

        supplyState.SetEffect(() => SupplyRewardEffect(supplyState));
        manaState.SetEffect(() => ManaRewardEffect(manaState));
        demonDisappearsState.SetEffect(() => DemonDisappearsRewardEffect(demonDisappearsState));
        //demonWeaponUpgradeState.SetEndEffect(() => DemonWeaponUpgradeEffect(demonWeaponUpgradeState));
        //demonArmorUpgradeState.SetEndEffect(() => DemonArmorUpgradeEffect(demonArmorUpgradeState));
        unleashedMonsterState.SetEffect(() => UnleashedMonsterEffect(unleashedMonsterState));
        nothingState.SetEffect(() => NothingEffect(nothingState));
        leftAloneState.SetEffect(() => LeftAloneRewardEffect(leftAloneState));

        _states.Add(startState.name, startState);
        _states.Add(supplyState.name, supplyState);
        _states.Add(manaState.name, manaState);
        _states.Add(demonDisappearsState.name, demonDisappearsState);
        //_states.Add(demonWeaponUpgradeState.name, demonWeaponUpgradeState);
        //_states.Add(demonArmorUpgradeState.name, demonArmorUpgradeState);
        _states.Add(unleashedMonsterState.name, unleashedMonsterState);
        _states.Add(nothingState.name, nothingState);
        _states.Add(leftAloneState.name, leftAloneState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if(state.name == "Start") {
            ActionOption sendOutDemonOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Send out a Demon.",
                //description = "We have sent %minion% to explore the interesting location.",
                duration = 0,
                effect = () => SendOutDemonEffect(state),
            };

            ActionOption leaveAloneOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave it alone.",
                duration = 0,
                effect = () => LeftAloneOption(state),
            };
            state.AddActionOption(sendOutDemonOption);
            state.AddActionOption(leaveAloneOption);
            state.SetDefaultOption(leaveAloneOption);
        }
    }
    #endregion

    private void SendOutDemonEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Supply", 15);
        effectWeights.AddElement("Mana", 10);
        effectWeights.AddElement("Demon Disappears", 5);
        //effectWeights.AddElement("Demon Weapon Upgrade", 10);
        //effectWeights.AddElement("Demon Armor Upgrade", 10);
        effectWeights.AddElement("Unleashed Monster", 5);
        effectWeights.AddElement("Nothing", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Supply") {
        //    SupplyRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Mana") {
        //    ManaRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Demon Disappears") {
        //    DemonDisappearsRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Demon Weapon Upgrade") {
        //    DemonWeaponUpgradeRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Demon Armor Upgrade") {
        //    DemonArmorUpgradeRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Unleashed Monster") {
        //    UnleashedMonsterRewardState(state, chosenEffect);
        //}else if (chosenEffect == "Nothing") {
        //    NothingRewardState(state, chosenEffect);
        //}
    }
    private void LeftAloneOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Left Alone", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Left Alone") {
        //    LeftAloneRewardState(state, chosenEffect);
        //}
    }
    private void DemonWeaponUpgradeRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has returned with an improved Weapon.");
        SetCurrentState(_states[effectName]);
        DemonWeaponUpgradeEffect(_states[effectName]);
    }
    private void DemonArmorUpgradeRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has returned with an improved Armor.");
        SetCurrentState(_states[effectName]);
        DemonArmorUpgradeEffect(_states[effectName]);
    }
    private void UnleashedMonsterRewardState(InteractionState state, string effectName) {
        if(_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable;
            if(landmark.charactersWithHomeOnLandmark.Count > 0) {
                //_states[effectName].SetDescription(explorerMinion.name + " has awakened a " + landmark.charactersWithHomeOnLandmark[0].name + ". It now defends the cave from intruders.");
                SetCurrentState(_states[effectName]);
                UnleashedMonsterEffect(_states[effectName]);
            }
        }
    }
    private void DemonWeaponUpgradeEffect(InteractionState state) {
        //investigatorMinion.character.UpgradeWeapon();
    }
    private void DemonArmorUpgradeEffect(InteractionState state) {
        //investigatorMinion.character.UpgradeArmor();
    }
    private void UnleashedMonsterEffect(InteractionState state) {
        //TODO: awaken monster and put it in defenders list
        WeightedDictionary<string> monsterWeights = new WeightedDictionary<string>();
        monsterWeights.AddElement("Spider Spinner", 10);
        monsterWeights.AddElement("Wolf Ravager", 10);

        string chosenMonster = monsterWeights.PickRandomElementGivenWeights();
        RACE chosenRace = RACE.HUMANS;
        string chosenClass = string.Empty;
        if(chosenMonster == "Spider Spinner") {
            chosenRace = RACE.SPIDER;
            chosenClass = "Spinner";
        }else if (chosenMonster == "Wolf Ravager") {
            chosenRace = RACE.WOLF;
            chosenClass = "Ravager";
        }

        if(chosenClass != string.Empty) {
            Character newDefender = CharacterManager.Instance.CreateNewCharacter(chosenClass, chosenRace, GENDER.MALE, null, _interactable, false);
            BaseLandmark landmark = _interactable;
            //landmark.AddDefender(newDefender);

            if (state.descriptionLog != null) {
                state.descriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(chosenRace), LOG_IDENTIFIER.STRING_1);
                state.descriptionLog.AddToFillers(null, chosenClass, LOG_IDENTIFIER.STRING_2);
            }
            state.AddLogFiller(new LogFiller(null, Utilities.GetNormalizedSingularRace(chosenRace), LOG_IDENTIFIER.STRING_1));
            state.AddLogFiller(new LogFiller(null, chosenClass, LOG_IDENTIFIER.STRING_2));
            //if (state.minionLog != null) {
            //    state.minionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(chosenRace), LOG_IDENTIFIER.STRING_1);
            //    state.minionLog.AddToFillers(null, chosenClass, LOG_IDENTIFIER.STRING_2);
            //}
        }
    }

    private void LeftAloneRewardEffect(InteractionState state) {
        //explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
}
