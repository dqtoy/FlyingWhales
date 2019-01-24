using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MysteriousSarcophagus : Interaction {

    private const string Start = "Start";
    private const string Combat_Undead_Character = "Combat Undead Character";
    private const string Cursed = "Cursed";
    private const string Gain_Supplies = "Gain Supplies";
    private const string Recruit_Undead_Character = "Recruit Undead Character";
    private const string Gain_Positive_Trait = "Gain Positive Trait";
    private const string Do_Nothing = "Do Nothing";
    private const string Minion_Killed_Undead = "Minion Killed Undead";
    private const string Undead_Killed_Minion = "Undead Killed Minion";
    private const string Minion_Flees_Success = "Minion Flees Success";
    private const string Minion_Forced_To_Kill_Undead = "Minion Forced To Kill Undead";
    private const string Fleeing_Minion_Killed = "Fleeing Minion Killed";

    private Character _undeadCharacter;

    public MysteriousSarcophagus(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MYSTERIOUS_SARCOPHAGUS, 110) {
        _name = "Mysterious Sarcophagus";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState combatUndeadState = new InteractionState(Combat_Undead_Character, this);
        InteractionState cursedState = new InteractionState(Cursed, this);
        InteractionState gainSuppliesState = new InteractionState(Gain_Supplies, this);
        InteractionState recruitUndeadState = new InteractionState(Recruit_Undead_Character, this);
        InteractionState gainPositiveState = new InteractionState(Gain_Positive_Trait, this);
        InteractionState minionKilledUndeadState = new InteractionState(Minion_Killed_Undead, this);
        InteractionState undeadKilledMinionState = new InteractionState(Undead_Killed_Minion, this);
        InteractionState minionFleesSuccessState = new InteractionState(Minion_Flees_Success, this);
        InteractionState minionForcedToKillState = new InteractionState(Minion_Forced_To_Kill_Undead, this);
        InteractionState fleeingMinionKilledState = new InteractionState(Fleeing_Minion_Killed, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);
        CreateActionOptions(combatUndeadState);

        combatUndeadState.SetEffect(() => CombatUndeadCharacterEffect(combatUndeadState), false);
        cursedState.SetEffect(() => CursedEffect(cursedState));
        gainSuppliesState.SetEffect(() => GainSuppliesEffect(gainSuppliesState));
        recruitUndeadState.SetEffect(() => RecruitUndeadCharacterEffect(recruitUndeadState));
        gainPositiveState.SetEffect(() => GainPositiveTraitEffect(gainPositiveState));
        minionKilledUndeadState.SetEffect(() => MinionKilledUndeadEffect(minionKilledUndeadState));
        undeadKilledMinionState.SetEffect(() => UndeadKilledMinionEffect(undeadKilledMinionState));
        minionFleesSuccessState.SetEffect(() => MinionFleesSuccessEffect(minionFleesSuccessState));
        minionForcedToKillState.SetEffect(() => MinionForcedToKillUndeadEffect(minionForcedToKillState));
        fleeingMinionKilledState.SetEffect(() => FleeingMinionKilledEffect(fleeingMinionKilledState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(combatUndeadState.name, combatUndeadState);
        _states.Add(cursedState.name, cursedState);
        _states.Add(gainSuppliesState.name, gainSuppliesState);
        _states.Add(recruitUndeadState.name, recruitUndeadState);
        _states.Add(gainPositiveState.name, gainPositiveState);
        _states.Add(minionKilledUndeadState.name, minionKilledUndeadState);
        _states.Add(undeadKilledMinionState.name, undeadKilledMinionState);
        _states.Add(minionFleesSuccessState.name, minionFleesSuccessState);
        _states.Add(minionForcedToKillState.name, minionForcedToKillState);
        _states.Add(fleeingMinionKilledState.name, fleeingMinionKilledState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == Start) {
            ActionOption ofCourseOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Of course.",
                duration = 0,
                effect = () => OfCourseOption(),
            };
            ActionOption ofCourseNotOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Of course not.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(ofCourseOption);
            state.AddActionOption(ofCourseNotOption);
            state.SetDefaultOption(ofCourseOption);

        }else if (state.name == Combat_Undead_Character) {
            ActionOption fightOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Fight it.",
                duration = 0,
                effect = () => FightOption(),
            };
            ActionOption retreatOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Retreat.",
                duration = 0,
                effect = () => RetreatOption(),
            };

            state.AddActionOption(fightOption);
            state.AddActionOption(retreatOption);
            state.SetDefaultOption(fightOption);
        }
    }
    #endregion

    #region Action Option
    private void OfCourseOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Cursed, 5);
        effectWeights.AddElement(Gain_Supplies, 15);
        effectWeights.AddElement(Combat_Undead_Character, 5);
        effectWeights.AddElement(Recruit_Undead_Character, 5);
        effectWeights.AddElement(Gain_Positive_Trait, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Do_Nothing, 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void FightOption() {
        Combat combat = _characterInvolved.currentParty.CreateCombatWith(_undeadCharacter.currentParty);
        combat.Fight();
        if(combat.winningSide == SIDES.A) {
            SetCurrentState(_states[Minion_Killed_Undead]);
        } else {
            SetCurrentState(_states[Undead_Killed_Minion]);
        }
    }
    private void RetreatOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Minion_Flees_Success, _characterInvolved.job.GetSuccessRate());
        effectWeights.AddElement("Fail", _characterInvolved.job.GetFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();

        if(result == Minion_Flees_Success) {
            SetCurrentState(_states[Minion_Flees_Success]);
        } else {
            Combat combat = _characterInvolved.currentParty.CreateCombatWith(_undeadCharacter.currentParty);
            combat.Fight();
            if (combat.winningSide == SIDES.A) {
                SetCurrentState(_states[Minion_Forced_To_Kill_Undead]);
            } else {
                SetCurrentState(_states[Fleeing_Minion_Killed]);
            }
        }
    }
    #endregion

    #region State Effects
    private void CursedEffect(InteractionState state) {
        //TODO: all of your units gain a random negative Trait (same negative Trait for all)
        WeightedDictionary<string> negativeTraitsWeights = new WeightedDictionary<string>();
        negativeTraitsWeights.AddElement("Spider Phobia", 35);
        negativeTraitsWeights.AddElement("Goblin Phobia", 10);
        negativeTraitsWeights.AddElement("Zombie Phobia", 5);

        string chosenTrait = negativeTraitsWeights.PickRandomElementGivenWeights();
        Trait negativeTrait = AttributeManager.Instance.allTraits[chosenTrait];
        for (int i = 0; i < _characterInvolved.faction.characters.Count; i++) {
            _characterInvolved.faction.characters[i].AddTrait(negativeTrait);
        }
        state.AddLogFiller(new LogFiller(null, chosenTrait, LOG_IDENTIFIER.STRING_1));
    }
    private void GainSuppliesEffect(InteractionState state) {
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1);
        _characterInvolved.LevelUp();
        _characterInvolved.ClaimReward(reward);

        state.AddLogFiller(new LogFiller(null, reward.amount.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void CombatUndeadCharacterEffect(InteractionState state) {
        SpawnUndeadCharacter();

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
    }
    private void RecruitUndeadCharacterEffect(InteractionState state) {
        _characterInvolved.LevelUp();
        SpawnUndeadCharacter();
        _undeadCharacter.RecruitAsMinion();

        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
        state.descriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);

        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2));
    }
    private void GainPositiveTraitEffect(InteractionState state) {
        _characterInvolved.LevelUp();
        //TODO: Positive Trait Reward 1
        WeightedDictionary<string> positiveTraitsWeights = new WeightedDictionary<string>();
        positiveTraitsWeights.AddElement("Spider Slayer", 35);
        positiveTraitsWeights.AddElement("Spider Hater", 10);
        positiveTraitsWeights.AddElement("Spider Resistance", 5);

        string chosenTrait = positiveTraitsWeights.PickRandomElementGivenWeights();
        Trait positiveTrait = AttributeManager.Instance.allTraits[chosenTrait];
        _characterInvolved.AddTrait(positiveTrait);

        state.AddLogFiller(new LogFiller(null, chosenTrait, LOG_IDENTIFIER.STRING_1));
    }
    private void DoNothingEffect(InteractionState state) {
        //explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_2));
    }
    private void MinionKilledUndeadEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2));
    }
    private void UndeadKilledMinionEffect(InteractionState state) {
        if(!interactable.tileLocation.areaOfTile.areaResidents.Contains(_undeadCharacter)) {
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), state.name.ToLower() + "-notresident" + "_description");
            stateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            stateDescriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special1");
            log.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.AddLogToInvolvedObjects(log);

            _undeadCharacter.Death();
        } else {
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), state.name.ToLower() + "-resident" + "_description");
            stateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            stateDescriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special2");
            log.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            log.AddToFillers(interactable.tileLocation.areaOfTile, interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1);
            state.AddLogToInvolvedObjects(log);
        }
    }
    private void MinionFleesSuccessEffect(InteractionState state) {
        if (!interactable.tileLocation.areaOfTile.areaResidents.Contains(_undeadCharacter)) {
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), state.name.ToLower() + "-notresident" + "_description");
            stateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            stateDescriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special1");
            log.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.AddLogToInvolvedObjects(log);
        } else {
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), state.name.ToLower() + "-resident" + "_description");
            stateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            stateDescriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special2");
            log.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            log.AddToFillers(interactable.tileLocation.areaOfTile, interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1);
            state.AddLogToInvolvedObjects(log);
        }
    }
    private void MinionForcedToKillUndeadEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2));
    }
    private void FleeingMinionKilledEffect(InteractionState state) {
        if (!interactable.tileLocation.areaOfTile.areaResidents.Contains(_undeadCharacter)) {
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), state.name.ToLower() + "-notresident" + "_description");
            stateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            stateDescriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special1");
            log.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.AddLogToInvolvedObjects(log);

            _undeadCharacter.Death();
        } else {
            Log stateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), state.name.ToLower() + "-resident" + "_description");
            stateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            stateDescriptionLog.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            state.OverrideDescriptionLog(stateDescriptionLog);

            Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special2");
            log.AddToFillers(_characterInvolved, _characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, Utilities.NormalizeString(_undeadCharacter.race.ToString()), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _undeadCharacter.characterClass.className, LOG_IDENTIFIER.STRING_2);
            log.AddToFillers(interactable.tileLocation.areaOfTile, interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1);
            state.AddLogToInvolvedObjects(log);
        }
    }
    #endregion

    private void SpawnUndeadCharacter() {
        RACE race = RACE.FAERY;
        int levelModifier = 0;
        if (interactable.tileLocation.areaOfTile.name == "Tessellated Triangle") {
            race = RACE.FAERY;
            levelModifier = 6;
        } else if (interactable.tileLocation.areaOfTile.name == "Gloomhollow Crypts") {
            race = RACE.SKELETON;
            levelModifier = 3;
        }
        WeightedDictionary<AreaCharacterClass> classWeights = LandmarkManager.Instance.GetDefaultClassWeights(race);
        string className = classWeights.PickRandomElementGivenWeights().className;
        _undeadCharacter = CharacterManager.Instance.CreateNewCharacter(className, race, Utilities.GetRandomGender(), interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile);
        _undeadCharacter.SetLevel(FactionManager.Instance.GetAverageFactionLevel() + levelModifier);
    }
}
