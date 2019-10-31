using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuryCharacter : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;
    public BuryCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.BURY_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void SetTargetStructure() {
        if (_targetStructure == null) {
            //first check if the actor's current location has a cemetery
            _targetStructure = actor.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
            //if the target structure is null, check the actor's home area, if it has a cemetery and use that
            if (_targetStructure == null) {
                _targetStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
            }
            //if the target structure is still null, get a random area that has a cemetery, then target that
            if (_targetStructure == null) {
                List<Area> choices = LandmarkManager.Instance.allAreas.Where(x => x.HasStructure(STRUCTURE_TYPE.CEMETERY)).ToList();
                _targetStructure = choices[Utilities.rng.Next(0, choices.Count)].GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
            }
        }
        base.SetTargetStructure();
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = poiTarget }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeRegion, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Bury Success");
    }
    protected override int GetCost() {
        return 1;
    }
    public override void OnStopActionWhileTravelling() {
        base.OnStopActionWhileTravelling();
        Character targetCharacter = poiTarget as Character;
        actor.ownParty.RemoveCharacter(targetCharacter, false);
        targetCharacter.SetCurrentStructureLocation(targetCharacter.gridTileLocation.structure, false);
    }
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is LocationStructure) {
            _targetStructure = otherData[0] as LocationStructure;
            SetTargetStructure();
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region State Effects
    private void PreBurySuccess() {
        if (parentPlan.job != null) {
            if (parentPlan.job.jobType == JOB_TYPE.BURY) {
                currentState.SetIntelReaction(NormalBurySuccessIntelReaction);
            } else if (parentPlan.job.jobType == JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM) {
                currentState.SetIntelReaction(SerialKillerBurySuccessIntelReaction);
            }
        }
    }
    private void AfterBurySuccess() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);

        Character targetCharacter = poiTarget as Character;
        //**After Effect 1**: Remove Target from Actor's Party.
        actor.ownParty.RemoveCharacter(targetCharacter, false);
        //**After Effect 2**: Place a Tombstone tile object in adjacent unoccupied tile, link it with Target.
        LocationGridTile chosenLocation = actor.gridTileLocation;
        if (chosenLocation.isOccupied) {
            List<LocationGridTile> choices = actor.gridTileLocation.UnoccupiedNeighbours.Where(x => x.structure == actor.currentStructure).ToList();
            if (choices.Count > 0) {
                chosenLocation = choices[Random.Range(0, choices.Count)];
            }
            
        }
        Tombstone tombstone = new Tombstone(actor.currentStructure);
        tombstone.SetCharacter(targetCharacter);
        actor.currentStructure.AddPOI(tombstone, chosenLocation);
        targetCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.BURY, except:parentPlan.job);
        List<Character> characters = targetCharacter.GetAllCharactersThatHasRelationship();
        if(characters != null) {
            for (int i = 0; i < characters.Count; i++) {
                characters[i].AddAwareness(tombstone);
            }
        }
        //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, poiTarget, this as GoapAction);
        //targetCharacter.CancelAllJobsTargettingThisCharacter("target is already buried", false);
    }
    #endregion

    #region Preconditions
    private bool IsInActorParty() {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty;
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        Character targetCharacter = poiTarget as Character;
        //target character must be dead
        if (!targetCharacter.isDead) {
            return false;
        }
        //check that the charcater has been buried (has a grave)
        if (targetCharacter.grave != null) {
            return false;
        }
        //if(targetCharacter.IsInOwnParty() || targetCharacter.currentParty != actor.ownParty) {
        //    return false;
        //}
        return true;
    }
    #endregion

    #region Intel Reactions
    private List<string> NormalBurySuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;

        RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("It is always sad to bury a fellow resident.");
                }
                //- Positive Relationship with Target
                else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                    recipient.AddTrait("Heartbroken");
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        bool triggerBrokenhearted = false;
                        Heartbroken heartbroken = recipient.GetNormalTrait("Heartbroken") as Heartbroken;
                        if (heartbroken != null) {
                            triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                        }
                        if (!triggerBrokenhearted) {
                            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.REMEMBER_FALLEN, targetCharacter.grave);
                            job.SetCancelOnFail(true);
                            recipient.jobQueue.AddJobInQueue(job);
                        } else {
                            heartbroken.TriggerBrokenhearted();
                        }
                    }
                    reactions.Add("Another good one bites the dust.");
                }
                //- Negative Relationship with Target
                else if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                    recipient.AddTrait("Satisfied");
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        bool triggerBrokenhearted = false;
                        Heartbroken heartbroken = recipient.GetNormalTrait("Heartbroken") as Heartbroken;
                        if (heartbroken != null) {
                            triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                        }
                        if (!triggerBrokenhearted) {
                            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.SPIT, targetCharacter.grave);
                            job.SetCancelOnFail(true);
                            recipient.jobQueue.AddJobInQueue(job);
                        } else {
                            heartbroken.TriggerBrokenhearted();
                        }
                    }
                    reactions.Add(string.Format("Is it terrible to think that {0} deserved that?", targetCharacter.name));
                }
                //-  Positive or No Relationship with Actor, Actor has a positive relationship with Target
                else if ((relWithActor == RELATIONSHIP_EFFECT.POSITIVE || relWithActor == RELATIONSHIP_EFFECT.NONE) && actor.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    reactions.Add(string.Format("Burying someone close to you is probably the worst feeling ever. I feel for {0}.", actor.name));
                }
                //- Others
                else {
                    reactions.Add("This isn't relevant to me.");
                }
            }
        }
        return reactions;
    }
    private List<string> SerialKillerBurySuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;

        RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
        RELATIONSHIP_EFFECT relWithTarget = recipient.GetRelationshipEffectWith(targetCharacter);

        if (isOldNews) {
            //Old News
            reactions.Add("This is old news.");
        } else {
            //Not Yet Old News
            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                reactions.Add("I know that already.");
            } else {
                //- Recipient is Actor
                if (recipient == actor) {
                    reactions.Add("Do not tell anybody, please!");
                }
                //- Positive Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                    if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                        reactions.Add(string.Format("{0} is dear to me but {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
                    } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                        reactions.Add(string.Format("{0} is a terrible person so I'm sure there was a reason {1} offed {2}.", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    } else {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                        reactions.Add(string.Format("{0} is dear to me but {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
                    }
                }
                //- Negative Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                    if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed");
                        }
                        reactions.Add(string.Format("{0} is the worst! {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), targetCharacter.name));
                    } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                        reactions.Add(string.Format("I may not like {0} but {1} must still be punished for killing {2}!", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    } else {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed");
                        }
                        reactions.Add(string.Format("{0} is the worst! {1} must be punished for killing {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), targetCharacter.name));
                    }
                }
                //- No Relationship with Actor
                else if (relWithActor == RELATIONSHIP_EFFECT.NONE) {
                    if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.CreateKnockoutJob(actor);
                        } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                            recipient.CreateUndermineJobOnly(actor, "informed");
                        }
                        reactions.Add(string.Format("{0} must be punished for killing {1}!", actor.name, targetCharacter.name));
                    } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                        reactions.Add(string.Format("I may not like {0} but {1} must still be punished for killing {2}!", targetCharacter.name, actor.name, Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
                    } else {
                        recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                        if (status == SHARE_INTEL_STATUS.WITNESSED) {
                            recipient.marker.AddAvoidInRange(actor, reason: "saw something shameful");
                        }
                        reactions.Add(string.Format("{0} must be punished for killing {1}!", actor.name, targetCharacter.name));
                    }
                }
            }
        }
        return reactions;
    }
    #endregion
}

public class BuryCharacterData : GoapActionData {
    public BuryCharacterData() : base(INTERACTION_TYPE.BURY_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character targetCharacter = poiTarget as Character;
        //target character must be dead
        if (!targetCharacter.isDead) {
            return false;
        }
        //check that the charcater has been buried (has a grave)
        if (targetCharacter.grave != null) {
            return false;
        }
        return true;
    }
}
