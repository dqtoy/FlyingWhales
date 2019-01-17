using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionRecruitCharacter : Interaction {
    private const string Recruitment_Success = "Recruitment Success";
    private const string Recruitment_Fail = "Recruitment Fail";
    private const string Recruitment_Critical_Fail = "Recruitment Critical Fail";

    public MinionRecruitCharacter(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MINION_RECRUIT_CHARACTER, 0) {
        _name = "Minion Recruit Character";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState recruitSuccessState = new InteractionState(Recruitment_Success, this);
        InteractionState recruitFailState = new InteractionState(Recruitment_Fail, this);
        InteractionState recruitCritFailState = new InteractionState(Recruitment_Critical_Fail, this);

        recruitSuccessState.SetEffect(() => RecruitSuccessEffect(recruitSuccessState));
        recruitFailState.SetEffect(() => RecruitFailEffect(recruitFailState));
        recruitCritFailState.SetEffect(() => RecruitCritFailEffect(recruitCritFailState));

        _states.Add(recruitSuccessState.name, recruitSuccessState);
        _states.Add(recruitFailState.name, recruitFailState);
        _states.Add(recruitCritFailState.name, recruitCritFailState);

        RecruitOption();
    }
    #endregion

    #region Action Options
    private void RecruitOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Recruitment_Success, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Recruitment_Fail, investigatorCharacter.job.GetFailRate());
        effectWeights.AddElement(Recruitment_Critical_Fail, investigatorCharacter.job.GetCritFailRate());

        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    #endregion

    #region State Effects
    private void RecruitSuccessEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        _characterInvolved.RecruitAsMinion();
    }
    private void RecruitFailEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        _characterInvolved.RecruitAsMinion();
    }
    private void RecruitCritFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);
        state.AddLogFiller(new LogFiller(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));

        investigatorCharacter.Death();
    }
    #endregion
}
