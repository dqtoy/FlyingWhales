using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureAction : Interaction {

    private Character targetCharacter;

    private const string Start = "Start";
    private const string Release_Success = "Release Success";
    private const string Release_Character_Tortured_Died = "Release Character Tortured Died";
    private const string Release_Character_Tortured_Injured = "Release Character Tortured Injured";
    private const string Release_Character_Tortured_Recruited = "Release Character Tortured Recruited";
    private const string Release_Critical_Fail = "Release Critical Fail";
    private const string Persuade_Success = "Persuade Success";
    private const string Persuade_Character_Tortured_Died = "Persuade Character Tortured Died";
    private const string Persuade_Character_Tortured_Injured = "Persuade Character Tortured Injured";
    private const string Persuade_Character_Tortured_Recruited = "Persuade Character Tortured Recruited";
    private const string Persuade_Critical_Fail = "Persuade Critical Fail";
    private const string Character_Tortured_Died = "Character Tortured Died";
    private const string Character_Tortured_Injured = "Character Tortured Injured";
    private const string Character_Tortured_Recruited = "Character Tortured Recruited";

    public TortureAction(BaseLandmark interactable): base(interactable, INTERACTION_TYPE.TORTURE_ACTION, 0) {
        _name = "Torture Action";
        _jobFilter = new JOB[] { JOB.DIPLOMAT, JOB.DISSUADER };
    }

    #region Override
    public override void CreateStates() {
        if (targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState releaseSuccess = new InteractionState(Release_Success, this);
        InteractionState releaseCharacterDied = new InteractionState(Release_Character_Tortured_Died, this);
        InteractionState releaseCharacterInjured = new InteractionState(Release_Character_Tortured_Injured, this);
        InteractionState releaseCharacterRecruited = new InteractionState(Release_Character_Tortured_Recruited, this);
        InteractionState releaseCriticalFail = new InteractionState(Release_Critical_Fail, this);
        InteractionState persuadeSuccess = new InteractionState(Persuade_Success, this);
        InteractionState persuadeCharacterDied = new InteractionState(Persuade_Character_Tortured_Died, this);
        InteractionState persuadeCharacterInjured = new InteractionState(Persuade_Character_Tortured_Injured, this);
        InteractionState persuadeCharacterRecruited = new InteractionState(Persuade_Character_Tortured_Recruited, this);
        InteractionState persuadeCriticalFail = new InteractionState(Persuade_Critical_Fail, this);
        InteractionState characterDied = new InteractionState(Character_Tortured_Died, this);
        InteractionState characterInjured = new InteractionState(Character_Tortured_Injured, this);
        InteractionState characterRecruited = new InteractionState(Character_Tortured_Recruited, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        releaseSuccess.SetEffect(() => ReleaseSuccessEffect(releaseSuccess));
        releaseCharacterDied.SetEffect(() => ReleaseCharacterDiedEffect(releaseCharacterDied));
        releaseCharacterInjured.SetEffect(() => ReleaseCharacterInjuredEffect(releaseCharacterInjured));
        releaseCharacterRecruited.SetEffect(() => ReleaseCharacterRecruitedEffect(releaseCharacterRecruited));
        releaseCriticalFail.SetEffect(() => ReleaseCritFailEffect(releaseCriticalFail));

        persuadeSuccess.SetEffect(() => PersuadeSuccessEffect(persuadeSuccess));
        persuadeCharacterDied.SetEffect(() => PersuadeCharacterDiedEffect(persuadeCharacterDied));
        persuadeCharacterInjured.SetEffect(() => PersuadeCharacterInjuredEffect(persuadeCharacterInjured));
        persuadeCharacterRecruited.SetEffect(() => PersuadeCharacterRecruitedEffect(persuadeCharacterRecruited));
        persuadeCriticalFail.SetEffect(() => PersuadeCritFailEffect(persuadeCriticalFail));

        characterDied.SetEffect(() => CharacterDiedEffect(characterDied));
        characterInjured.SetEffect(() => CharacterInjuredEffect(characterInjured));
        characterRecruited.SetEffect(() => CharacterRecruitedEffect(characterRecruited));

        _states.Add(startState.name, startState);
        _states.Add(releaseSuccess.name, releaseSuccess);
        _states.Add(releaseCharacterDied.name, releaseCharacterDied);
        _states.Add(releaseCharacterInjured.name, releaseCharacterInjured);
        _states.Add(releaseCharacterRecruited.name, releaseCharacterRecruited);
        _states.Add(releaseCriticalFail.name, releaseCriticalFail);

        _states.Add(persuadeSuccess.name, persuadeSuccess);
        _states.Add(persuadeCharacterDied.name, persuadeCharacterDied);
        _states.Add(persuadeCharacterInjured.name, persuadeCharacterInjured);
        _states.Add(persuadeCharacterRecruited.name, persuadeCharacterRecruited);
        _states.Add(persuadeCriticalFail.name, persuadeCriticalFail);

        _states.Add(characterDied.name, characterDied);
        _states.Add(characterInjured.name, characterInjured);
        _states.Add(characterRecruited.name, characterRecruited);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption release = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Release " + targetCharacter.name + " before " + Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.SUBJECTIVE, false) + " gets tortured.",
                effect = () => ReleaseOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                disabledTooltipText = "Minion must be a Diplomat",
            };
            ActionOption persuade = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Persuade to stop " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.POSSESSIVE, false) + " plan to torture " + targetCharacter.name + ".",
                effect = () => PersuadeOptionEffect(state),
                jobNeeded = JOB.DISSUADER,
                disabledTooltipText = "Minion must be a Dissuader",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(release);
            state.AddActionOption(persuade);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        SetTargetCharacter(GetTargetCharacter(character));
        if (targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void ReleaseOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Release_Success, investigatorMinion.character.job.GetSuccessRate());
        effectWeights.AddElement("Fail", investigatorMinion.character.job.GetFailRate());
        effectWeights.AddElement(Release_Critical_Fail, investigatorMinion.character.job.GetCritFailRate());

        string result = effectWeights.PickRandomElementGivenWeights();
        if(result == "Fail") {
            effectWeights.Clear();
            effectWeights.AddElement(Release_Character_Tortured_Died, 10);
            effectWeights.AddElement(Release_Character_Tortured_Injured, 40);
            effectWeights.AddElement(Release_Character_Tortured_Recruited, 20);
            result = effectWeights.PickRandomElementGivenWeights();
        }
        SetCurrentState(_states[result]);
    }
    private void PersuadeOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Persuade_Success, investigatorMinion.character.job.GetSuccessRate());
        effectWeights.AddElement("Fail", investigatorMinion.character.job.GetFailRate());
        effectWeights.AddElement(Persuade_Critical_Fail, investigatorMinion.character.job.GetCritFailRate());

        string result = effectWeights.PickRandomElementGivenWeights();
        if (result == "Fail") {
            effectWeights.Clear();
            effectWeights.AddElement(Persuade_Character_Tortured_Died, 10);
            effectWeights.AddElement(Persuade_Character_Tortured_Injured, 40);
            effectWeights.AddElement(Persuade_Character_Tortured_Recruited, 20);
            result = effectWeights.PickRandomElementGivenWeights();
        }
        SetCurrentState(_states[result]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Character_Tortured_Died, 10);
        effectWeights.AddElement(Character_Tortured_Injured, 40);
        effectWeights.AddElement(Character_Tortured_Recruited, 20);

        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region Reward Effect
    private void ReleaseSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        investigatorMinion.LevelUp();
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, targetCharacter.faction, 1, state);

        targetCharacter.ReleaseFromAbduction();
    }
    private void ReleaseCharacterDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.Death();
    }
    private void ReleaseCharacterInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void ReleaseCharacterRecruitedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        Abducted abductedTrait = targetCharacter.GetTrait("Abducted") as Abducted;
        targetCharacter.RemoveTrait(abductedTrait);
        targetCharacter.ChangeFactionTo(_characterInvolved.faction);
    }
    private void ReleaseCritFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(investigatorMinion, investigatorMinion.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(investigatorMinion, investigatorMinion.name, LOG_IDENTIFIER.MINION_1));

        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _characterInvolved.faction, -1, state);

        targetCharacter.Death();
        investigatorMinion.character.Death();
    }
    private void PersuadeSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        investigatorMinion.LevelUp();
    }
    private void PersuadeCharacterDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.Death();
    }
    private void PersuadeCharacterInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void PersuadeCharacterRecruitedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        Abducted abductedTrait = targetCharacter.GetTrait("Abducted") as Abducted;
        targetCharacter.RemoveTrait(abductedTrait);
        targetCharacter.ChangeFactionTo(_characterInvolved.faction);
    }
    private void PersuadeCritFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(investigatorMinion, investigatorMinion.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(investigatorMinion, investigatorMinion.name, LOG_IDENTIFIER.MINION_1));

        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _characterInvolved.faction, -1, state);

        _characterInvolved.LevelUp();
        targetCharacter.Death();
        investigatorMinion.character.Death();
    }
    private void CharacterDiedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.Death();
    }
    private void CharacterInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void CharacterRecruitedEffect(InteractionState state) {
        Abducted abductedTrait = targetCharacter.GetTrait("Abducted") as Abducted;
        targetCharacter.RemoveTrait(abductedTrait);
        targetCharacter.ChangeFactionTo(_characterInvolved.faction);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty() && currCharacter.GetTrait("Abducted") != null) {
                return currCharacter;
            }
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
