using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyCharacterEncountered : Interaction {

    public FriendlyCharacterEncountered(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.FRIENDLY_CHARACTER_ENCOUNTERED, 70) {
        _name = "Friendly Character Encountered";
        _jobFilter = new JOB[] { JOB.RECRUITER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterRecruitedState = new InteractionState("Character Recruited", this);
        InteractionState nothingHappenedState = new InteractionState("Nothing Happened", this);

        if(_characterInvolved.race == RACE.BEAST) {
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-beast" + "_description");
            startStateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(characterInvolved.race.ToString()), LOG_IDENTIFIER.STRING_1);
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        } else {
            if (_characterInvolved.isFactionless) {
                Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-neutral" + "_description");
                startStateDescriptionLog.AddToFillers(null, characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1);
                startState.OverrideDescriptionLog(startStateDescriptionLog);
            } else {
                Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-friendly" + "_description");
                startStateDescriptionLog.AddToFillers(null, characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1);
                startStateDescriptionLog.AddToFillers(characterInvolved.faction, characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
                startState.OverrideDescriptionLog(startStateDescriptionLog);
            }
        }
        

        CreateActionOptions(startState);

        characterRecruitedState.SetEffect(() => RecruitEffect(characterRecruitedState));
        nothingHappenedState.SetEffect(() => NothingHappenedEffect(nothingHappenedState));

        _states.Add(startState.name, startState);
        _states.Add(characterRecruitedState.name, characterRecruitedState);
        _states.Add(nothingHappenedState.name, nothingHappenedState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption recruitOption = new ActionOption {
                interactionState = state,
                cost = _characterInvolved.characterClass.recruitmentCost,
                name = "Recruit " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + ".",
                duration = 0,
                effect = () => RecruitOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(recruitOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void RecruitOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Character Recruited", 30);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Character Recruited"]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement("Nothing Happened", 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states["Nothing Happened"]);
    }
    #endregion

    #region State Effects
    private void RecruitEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        _characterInvolved.RecruitAsMinion();
    }
    private void NothingHappenedEffect(InteractionState state) {
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    #endregion
}
