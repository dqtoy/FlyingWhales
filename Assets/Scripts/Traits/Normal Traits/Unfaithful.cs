using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Unfaithful : Trait {

        public float affairChanceMultiplier { get; private set; }
        //public float makeLoveChanceMultiplier { get; private set; }

        public Unfaithful() {
            name = "Unfaithful";
            description = "Unfaithful characters are prone to having illicit love affairs.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        protected override void OnChangeLevel() {
            base.OnChangeLevel();
            if (level == 1) {
                affairChanceMultiplier = 5f;
            } else if (level == 2) {
                affairChanceMultiplier = 10f;
            } else if (level == 3) {
                affairChanceMultiplier = 5f;
            }
        }
        //public override string GetRequirementDescription(Character character) {
        //    string baseDesc = base.GetRequirementDescription(character);
        //    return baseDesc + " The character must also have a lover.";
        //}
        //public override bool CanFlawBeTriggered(Character character) {
        //    bool canBeTriggered = base.CanFlawBeTriggered(character);
        //    if (canBeTriggered) {
        //        //the character must have a lover.
        //        canBeTriggered = character.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER) != null;
        //    }
        //    return canBeTriggered;
        //}
        public override string TriggerFlaw(Character character) {
            string successLogKey = base.TriggerFlaw(character);
            if (character.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TYPE.LOVER) != null) {
                Character paramour = (character.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TYPE.PARAMOUR) as AlterEgoData)?.owner ?? null;
                if (paramour == null) {
                    if (!character.jobQueue.HasJob(JOB_TYPE.HAVE_AFFAIR)) {
                        List<Character> choices = new List<Character>();
                        for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                            Character choice = character.currentRegion.charactersAtLocation[i];
                            if (RelationshipManager.Instance.IsSexuallyCompatible(character, choice) &&
                                RelationshipManager.Instance.GetValidator(character.currentAlterEgo).
                                    CanHaveRelationship(character.currentAlterEgo, choice.currentAlterEgo, RELATIONSHIP_TYPE.PARAMOUR)) {
                                choices.Add(choice);
                            }
                        }

                        if (choices.Count > 0) {
                            //If no paramour yet, the character will create a Have Affair Job which will attempt to have an affair with a viable target.
                            GoapPlanJob cheatJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAVE_AFFAIR, INTERACTION_TYPE.HAVE_AFFAIR, Utilities.GetRandomElement(choices), character);
                            character.jobQueue.AddJobInQueue(cheatJob);
                            return successLogKey;
                        } else {
                            return "fail_no_paramour";
                        }
                    }
                } else {
                    if (!character.jobQueue.HasJob(JOB_TYPE.CHEAT)) {
                        //If already has a paramour, the character will attempt to make love with one.
                        GoapPlanJob cheatJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHEAT, INTERACTION_TYPE.MAKE_LOVE, paramour, character);
                        character.jobQueue.AddJobInQueue(cheatJob);
                    }
                }
                return successLogKey;
            } else {
                return "fail";
            }
        }
        #endregion

    }
}

