using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIRelationshipData : IRelationshipData {
    public int relationshipValue { get; private set; }
    public List<RELATIONSHIP_TRAIT> relationships { get; private set; }
    public int flirtationCount { get; private set; }

    public RELATIONSHIP_EFFECT relationshipStatus {
        get { return GetRelationshipStatus(); }
    }

    public POIRelationshipData() {
        relationships = new List<RELATIONSHIP_TRAIT>();
    }

    public void AdjustRelationshipValue(int amount) {
        relationshipValue += amount;
        relationshipValue = Mathf.Max(0, relationshipValue);
    }

    #region Adding
    public void AddRelationship(RELATIONSHIP_TRAIT relType) {
        relationships.Add(relType);
    }
    #endregion

    #region Removing
    public void RemoveRelationship(RELATIONSHIP_TRAIT relType) {
        relationships.Remove(relType);
    }
    #endregion

    #region Inquiry
    public bool HasRelationship(params RELATIONSHIP_TRAIT[] rels) {
        for (int i = 0; i < rels.Length; i++) {
            if (relationships.Contains(rels[i])) {
                return true; //as long as the relationship has at least 1 relationship type from the list, consider this as true.
            }
        }
        return false;
    }
    private RELATIONSHIP_EFFECT GetRelationshipStatus() {
        if (relationshipValue < 0) {
            return RELATIONSHIP_EFFECT.NEGATIVE;
        } else if (relationshipValue > 0) {
            return RELATIONSHIP_EFFECT.POSITIVE;
        } else {
            return RELATIONSHIP_EFFECT.NEUTRAL;
        }
    }
    #endregion

    #region Getting
    public List<RELATIONSHIP_TRAIT> GetAllRelationshipOfEffect(RELATIONSHIP_EFFECT effect) {
        List<RELATIONSHIP_TRAIT> rels = new List<RELATIONSHIP_TRAIT>();
        for (int i = 0; i < relationships.Count; i++) {
            RELATIONSHIP_TRAIT currRel = relationships[i];
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
