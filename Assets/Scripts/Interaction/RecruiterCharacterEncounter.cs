using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruiterCharacterEncounter : Interaction {

    private const string Start = "Start";
    private const string Induce_Recruit = "Induce Recruit";
    private const string Recruitment_Success = "Recruitment Success";
    private const string Recruitment_Fail = "Recruitment Fail";
    private const string Do_Nothing = "Do Nothing";

    public RecruiterCharacterEncounter(Area interactable) : base(interactable, INTERACTION_TYPE.RECRUITER_CHARACTER_ENCOUNTER, 0) {
        _name = "Recruiter Character Encounter";
        _jobFilter = new JOB[] { JOB.RECRUITER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState induceRecruitState = new InteractionState(Induce_Recruit, this);
        InteractionState recruitmentSuccessState = new InteractionState(Recruitment_Success, this);
        InteractionState recruitmentFailState = new InteractionState(Recruitment_Fail, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        induceRecruitState.SetEffect(() => InduceRecruitEffect(induceRecruitState));
        recruitmentSuccessState.SetEffect(() => RecruitmentSuccessEffect(recruitmentSuccessState));
        recruitmentFailState.SetEffect(() => RecruitmentFailEffect(recruitmentFailState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(induceRecruitState.name, induceRecruitState);
        _states.Add(recruitmentSuccessState.name, recruitmentSuccessState);
        _states.Add(recruitmentFailState.name, recruitmentFailState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption coerceOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Coerce <b>" + _characterInvolved.name + "</b> to recruit.",
                enabledTooltipText = _characterInvolved.name + " may be induced to recruit soon.",
                disabledTooltipText = _characterInvolved.name + " must be a Recruiter.",
                canBeDoneAction = () => CanInduceRecruit(),
                effect = () => InduceOption(state),
            };

            ActionOption recruitOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Recruit " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + ".",
                enabledTooltipText = "May recruit a new character...",
                effect = () => RecruitOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " alone.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(coerceOption);
            state.AddActionOption(recruitOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private bool CanInduceRecruit() {
        if (_characterInvolved.job.jobType != JOB.RECRUITER) {
            return false;
        }
        return true;
    }
    private void InduceOption(InteractionState state) {
        SetCurrentState(_states[Induce_Recruit]);
    }
    private void RecruitOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Recruitment_Success, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Recruitment_Fail, investigatorCharacter.job.GetFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void InduceRecruitEffect(InteractionState state) {
        investigatorCharacter.LevelUp();

        MoveToRecruit moveToRecruit = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RECRUIT, interactable) as MoveToRecruit;
        Character characterToBeRecruited = moveToRecruit.GetTargetCharacter(_characterInvolved);
        moveToRecruit.SetCharacterToBeRecruited(characterToBeRecruited);
        _characterInvolved.InduceInteraction(moveToRecruit);

        Area targetArea = characterToBeRecruited.specificLocation;

        state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2);

        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void RecruitmentSuccessEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        characterInvolved.RecruitAsMinion();
    }
    private void RecruitmentFailEffect(InteractionState state) {
    }
    private void DoNothingEffect(InteractionState state) {
    }
    #endregion
}
