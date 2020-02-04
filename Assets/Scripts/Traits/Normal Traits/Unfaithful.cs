using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Unfaithful : Trait {

        public float affairChanceMultiplier { get; private set; }

        public Unfaithful() {
            name = "Unfaithful";
            description = "Unfaithful characters are prone to having illicit love affairs.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
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
        public override string TriggerFlaw(Character character) {
            string successLogKey = base.TriggerFlaw(character);
            if (character.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TYPE.LOVER) != null) {
                Character affair = (character.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TYPE.AFFAIR) as Character) ?? null;
                if (affair == null) {
                    if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                        List<Character> choices = new List<Character>();
                        for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                            Character choice = character.currentRegion.charactersAtLocation[i];
                            if (RelationshipManager.Instance.IsSexuallyCompatible(character, choice) &&
                                RelationshipManager.Instance.GetValidator(character).
                                    CanHaveRelationship(character, choice, RELATIONSHIP_TYPE.AFFAIR)) {
                                choices.Add(choice);
                            }
                        }

                        if (choices.Count > 0) {
                            //If no affair yet, the character will create a Have Affair Job which will attempt to have an affair with a viable target.
                            GoapPlanJob cheatJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.HAVE_AFFAIR, CollectionUtilities.GetRandomElement(choices), character);
                            character.jobQueue.AddJobInQueue(cheatJob);
                            return successLogKey;
                        } else {
                            return "fail_no_affair";
                        }
                    }
                } else {
                    if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                        //If already has a affair, the character will attempt to make love with one.
                        GoapPlanJob cheatJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.MAKE_LOVE, affair, character);
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

