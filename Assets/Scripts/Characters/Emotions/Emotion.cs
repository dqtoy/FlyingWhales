using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Emotion {
	public string name { get; private set; }
    public EMOTION emotionType { get; private set; }
    public string[] responses { get; protected set; }
    public string[] mutuallyExclusive { get; protected set; }

    public Emotion(EMOTION emotionType) {
        this.emotionType = emotionType;
        name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(emotionType.ToString());
        responses = new string[] { name };
    }
    public virtual string ProcessEmotion(Character actor, IPointOfInterest target) {
        return responses[UnityEngine.Random.Range(0, responses.Length)];
    }
    protected void CreateKnockoutJob(Character character, Character targetCharacter) {
        if (!character.jobQueue.HasJob(JOB_TYPE.KNOCKOUT, targetCharacter)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, character);
            character.jobQueue.AddJobInQueue(job);
        }
    }
    protected void CreateKillJob(Character character, Character targetCharacter) {
        if (!character.jobQueue.HasJob(JOB_TYPE.KILL, targetCharacter)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KILL, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, character);
            character.jobQueue.AddJobInQueue(job);
        }
    }
    public bool IsEmotionCompatibleWithThis(string emotionName) {
        return mutuallyExclusive == null || !mutuallyExclusive.Contains(emotionName);
    }
}
