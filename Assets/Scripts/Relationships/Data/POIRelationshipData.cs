using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIRelationshipData : IRelationshipData {
    public int relationshipValue { get; private set; }
    public List<RELATIONSHIP_TYPE> relationships { get; private set; }
    public int flirtationCount { get; private set; }

    public RELATIONSHIP_EFFECT relationshipStatus => GetRelationshipStatus();
    public POIRelationshipData() {
        relationships = new List<RELATIONSHIP_TYPE>();
    }

    public void AdjustRelationshipValue(int amount) {
        relationshipValue += amount;
        relationshipValue = Mathf.Max(0, relationshipValue);
    }

    #region Adding
    public void AddRelationship(RELATIONSHIP_TYPE relType) {
        relationships.Add(relType);
    }
    #endregion

    #region Removing
    public void RemoveRelationship(RELATIONSHIP_TYPE relType) {
        relationships.Remove(relType);
    }
    #endregion

    #region Inquiry
    public bool HasRelationship(params RELATIONSHIP_TYPE[] rels) {
        for (int i = 0; i < rels.Length; i++) {
            if (relationships.Contains(rels[i])) {
                return true; //as long as the relationship has at least 1 relationship type from the list, consider this as true.
            }
        }
        return false;
    }
    private RELATIONSHIP_EFFECT GetRelationshipStatus() {
        if (HasRelationship(RELATIONSHIP_TYPE.FRIEND, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.PARAMOUR)) {
            return RELATIONSHIP_EFFECT.POSITIVE;
        } else if (HasRelationship(RELATIONSHIP_TYPE.ENEMY)) {
            return RELATIONSHIP_EFFECT.NEGATIVE;
        } else {
            return RELATIONSHIP_EFFECT.NEUTRAL;
        }

        //if (relationshipValue < 0) {
        //    return RELATIONSHIP_EFFECT.NEGATIVE;
        //} else if (relationshipValue > 0) {
        //    return RELATIONSHIP_EFFECT.POSITIVE;
        //} else {
        //    return RELATIONSHIP_EFFECT.NEUTRAL;
        //}
    }
    public RELATIONSHIP_TYPE GetFirstMajorRelationship() {
        for (int i = 0; i < relationships.Count; i++) {
            RELATIONSHIP_TYPE rel = relationships[i];
            if (rel != RELATIONSHIP_TYPE.ENEMY && rel != RELATIONSHIP_TYPE.FRIEND) {
                return rel;
            }
        }
        return RELATIONSHIP_TYPE.NONE;
    }
    #endregion

    #region Getting
    public List<RELATIONSHIP_TYPE> GetAllRelationshipOfEffect(RELATIONSHIP_EFFECT effect) {
        List<RELATIONSHIP_TYPE> rels = new List<RELATIONSHIP_TYPE>();
        for (int i = 0; i < relationships.Count; i++) {
            RELATIONSHIP_TYPE currRel = relationships[i];
            if (RelationshipManager.Instance.GetRelationshipEffect(currRel) == effect) {
                rels.Add(currRel);
            }
        }
        return rels;
    }
    #endregion

    #region Flirting
    public void IncreaseFlirtationCount() {
        flirtationCount++;
    }
    #endregion

    #region Reaction
    public bool OnSeeCharacter(Character targetCharacter, Character characterThatWillDoJob) {
        if (!targetCharacter.isDead && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.UNDERMINE_ENEMY, targetCharacter)) {
            int chance = UnityEngine.Random.Range(0, 100);
            int value = 0;
            CHARACTER_MOOD currentMood = characterThatWillDoJob.currentMoodType;
            if (currentMood == CHARACTER_MOOD.DARK) {
                value = 20;
            } else if (currentMood == CHARACTER_MOOD.BAD) {
                value = 10;
            }
            if (chance < value) {
                return characterThatWillDoJob.CreateUndermineJobOnly(targetCharacter, "saw");
            }
        }
        return false;
    }
    #endregion

}
