using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class MysteriousSarcophagus : Interaction {

    public MysteriousSarcophagus(IInteractable interactable) : base(interactable, INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS, 110) {
        _name = "Mysterious Sarcophagus";
    }

    #region Overrides
    public override void CreateStates() {
        //CreateExploreStates();
        //CreateWhatToDoNextState("%minion% left the sarcophagus alone. Do you want him to continue surveillance of " + _interactable.specificLocation.thisName + "?");

        InteractionState startState = new InteractionState("Start", this);
        InteractionState cursedState = new InteractionState("Cursed", this);
        //InteractionState accessoryUpgradeState = new InteractionState("Accessory Upgrade", this);
        InteractionState gainManaState = new InteractionState("Gain Mana", this);
        InteractionState gainSuppliesState = new InteractionState("Gain Supplies", this);
        InteractionState awakenUndeadHeroState = new InteractionState("Awaken Undead Hero", this);
        InteractionState gainPositiveState = new InteractionState("Gain Positive Trait", this);
        InteractionState doNothingState = new InteractionState("Do Nothing", this);

        //string startStateDesc = "%minion% has discovered an unopened sarcophagus in a hidden alcove. Should we open it?";
        //startState.SetDescription(startStateDesc);

        CreateActionOptions(startState);
        //CreateActionOptions(cursedState);
        //CreateActionOptions(accessoryUpgradeState);
        //CreateActionOptions(gainManaState);
        //CreateActionOptions(gainSuppliesState);
        //CreateActionOptions(awakenUndeadHeroState);
        //CreateActionOptions(gainPositiveState);

        cursedState.SetEndEffect(() => CursedRewardEffect(cursedState));
        //accessoryUpgradeState.SetEndEffect(() => AccessoryUpgradeRewardEffect(accessoryUpgradeState));
        gainManaState.SetEndEffect(() => GainManaRewardEffect(gainManaState));
        gainSuppliesState.SetEndEffect(() => GainSuppliesRewardEffect(gainSuppliesState));
        awakenUndeadHeroState.SetEndEffect(() => AwakenUndeadHeroRewardEffect(awakenUndeadHeroState));
        gainPositiveState.SetEndEffect(() => GainPositiveTraitRewardEffect(gainPositiveState));
        doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(cursedState.name, cursedState);
        //_states.Add(accessoryUpgradeState.name, accessoryUpgradeState);
        _states.Add(gainManaState.name, gainManaState);
        _states.Add(gainSuppliesState.name, gainSuppliesState);
        _states.Add(awakenUndeadHeroState.name, awakenUndeadHeroState);
        _states.Add(gainPositiveState.name, gainPositiveState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption ofCourseOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Of course.",
                //description = "We have sent %minion% to open the sarcophagus.",
                duration = 0,
                needsMinion = false,
                effect = () => OfCourseOption(state),
            };
            ActionOption ofCourseNotOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Of course not.",
                duration = 0,
                needsMinion = false,
                effect = () => DoNothingOption(state),
            };

            state.AddActionOption(ofCourseOption);
            state.AddActionOption(ofCourseNotOption);
            state.SetDefaultOption(ofCourseNotOption);
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

    private void OfCourseOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Cursed", 5);
        //effectWeights.AddElement("Accessory Upgrade", 10);
        effectWeights.AddElement("Gain Mana", 15);
        effectWeights.AddElement("Gain Supplies", 5);
        effectWeights.AddElement("Awaken Undead Hero", 5);
        effectWeights.AddElement("Gain Positive Trait", 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);

        //if (chosenEffect == "Cursed") {
        //    CursedRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Accessory Upgrade") {
        //    AccessoryUpgradeRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Gain Mana") {
        //    GainManaRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Gain Supplies") {
        //    GainSuppliesRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Awaken Undead Hero") {
        //    AwakenUndeadHeroRewardState(state, chosenEffect);
        //} else if (chosenEffect == "Gain Positive Trait") {
        //    GainPositiveTraitRewardState(state, chosenEffect);
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
    private void CursedRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " opened the sarcophagus and found nothing inside. But everyone felt a strange heaviness in the air and suddenly all of your followers have been cursed with a strange affliction.");
        SetCurrentState(_states[stateName]);
        CursedRewardEffect(_states[stateName]);
    }
    private void AccessoryUpgradeRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " opened the sarcophagus and found a powerful accessory that it can use.");
        SetCurrentState(_states[stateName]);
        AccessoryUpgradeRewardEffect(_states[stateName]);
    }
    private void GainManaRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " opened the sarcophagus and found a small magical source. We have converted it into an ample amount of mana.");
        SetCurrentState(_states[stateName]);
        GainManaRewardEffect(_states[stateName]);
    }
    private void GainSuppliesRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " opened the sarcophagus and found a small amount of ancient treasures that we have added to our Supplies.");
        SetCurrentState(_states[stateName]);
        GainSuppliesRewardEffect(_states[stateName]);
    }
    private void AwakenUndeadHeroRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " opened the sarcophagus and unleashed a powerful mummy. The mummy now guards " + _interactable.specificLocation.thisName + ". " + explorerMinion.name + " managed to escape");
        SetCurrentState(_states[stateName]);
        AwakenUndeadHeroRewardEffect(_states[stateName]);
    }
    private void GainPositiveTraitRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription(explorerMinion.name + " opened the sarcophagus and found it empty. As " + explorerMinion.name + " leaves, a wisp of sand blows through him and a strange power awakened within.");
        SetCurrentState(_states[stateName]);
        GainPositiveTraitRewardEffect(_states[stateName]);
    }
    #endregion

    #region State Effects
    private void CursedRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //TODO: all of your units gain a random negative Trait (same negative Trait for all)
        WeightedDictionary<string> negativeTraitsWeights = new WeightedDictionary<string>();
        negativeTraitsWeights.AddElement("Spider Phobia", 35);
        negativeTraitsWeights.AddElement("Goblin Phobia", 10);
        negativeTraitsWeights.AddElement("Zombie Phobia", 5);

        string chosenTrait = negativeTraitsWeights.PickRandomElementGivenWeights();
        Trait negativeTrait = AttributeManager.Instance.allTraits[chosenTrait];
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            PlayerManager.Instance.player.minions[i].icharacter.AddTrait(negativeTrait);
        }
        state.AddLogFiller(new LogFiller(null, chosenTrait, LOG_IDENTIFIER.STRING_1));
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(null, chosenTrait, LOG_IDENTIFIER.STRING_1);
        //}
    }
    private void AccessoryUpgradeRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        if (explorerMinion.icharacter.equippedAccessory != null) {
            explorerMinion.icharacter.UpgradeAccessory();
        }
    }
    private void GainManaRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Mana_Cache_Reward_2));
    }
    private void GainSuppliesRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
    }
    private void AwakenUndeadHeroRewardEffect(InteractionState state) {
        if(_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            Character zombieEarthbinderHero = CharacterManager.Instance.CreateNewCharacter("Earthbinder", RACE.ZOMBIE, GENDER.MALE, PlayerManager.Instance.player.playerFaction, PlayerManager.Instance.player.demonicPortal, false);
            zombieEarthbinderHero.SetLevel(15);
            landmark.AddDefender(zombieEarthbinderHero);
        }
    }
    private void GainPositiveTraitRewardEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //TODO: Positive Trait Reward 1
        WeightedDictionary<string> positiveTraitsWeights = new WeightedDictionary<string>();
        positiveTraitsWeights.AddElement("Spider Slayer", 35);
        positiveTraitsWeights.AddElement("Spider Hater", 10);
        positiveTraitsWeights.AddElement("Spider Resistance", 5);

        string chosenTrait = positiveTraitsWeights.PickRandomElementGivenWeights();
        Trait positiveTrait = AttributeManager.Instance.allTraits[chosenTrait];
        explorerMinion.icharacter.AddTrait(positiveTrait);
        state.AddLogFiller(new LogFiller(null, chosenTrait, LOG_IDENTIFIER.STRING_1));
        //if (state.minionLog != null) {
        //    state.minionLog.AddToFillers(null, chosenTrait, LOG_IDENTIFIER.STRING_1);
        //}
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
    #endregion
}
