using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TablePoison : GoapAction {
    protected override string failActionState { get { return "Poison Fail"; } }

    private Character _assumedTargetCharacter;

    public TablePoison(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TABLE_POISON, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Poison Table";
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //_isStealthAction = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        //**Effect 1**: Table - Add Trait (Poisoned)
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
        LocationGridTile knownLoc = actor.GetAwareness(poiTarget).knownGridLocation;
        if (knownLoc.structure is Dwelling) {
            Dwelling dwelling = knownLoc.structure as Dwelling;
            for (int i = 0; i < dwelling.residents.Count; i++) {
                //**Effect 2**: Owner/s - Add Trait (Sick)
                AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Sick", targetPOI = dwelling.residents[i] });
                AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = dwelling.residents[i] });
                //**Effect 3**: Kill Owner/s
                AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = dwelling.residents[i] });
            }
        }
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Poison Success");
            //if (!HasOtherCharacterInRadius()) {
            //    SetState("Poison Success");
            //} else {
            //    parentPlan.SetDoNotRecalculate(true);
            //    SetState("Poison Fail");
            //}
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 4;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Poison Fail");
    //}
    public override void OnWitnessedBy(Character witness) {
        base.OnWitnessedBy(witness);

        //Dwelling dwelling = (poiTarget as Table).structureLocation as Dwelling;

        //bool isTableOwner = dwelling.IsResident(witness);
        //bool hasPositiveRelWithOwner = false;
        //for (int i = 0; i < dwelling.residents.Count; i++) {
        //    Character resident = dwelling.residents[i];
        //    if (witness.HasRelationshipOfEffectWith(resident, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
        //        hasPositiveRelWithOwner = true;
        //        break;
        //    }
        //}

        //If someone witnesses this, there may be additional things performed aside from Crime Witness handling.
        Character tableOwner = ((poiTarget as Table).structureLocation as Dwelling).owner;
        //If the witness has a positive relationship with the owner of the table, or he is the owner of the table, or they are from the same faction and are not enemies:
        if (witness == tableOwner 
            || witness.HasRelationshipOfEffectWith(tableOwner, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE) 
            || (witness.faction == tableOwner.faction && !witness.HasRelationshipOfTypeWith(tableOwner, RELATIONSHIP_TRAIT.ENEMY))
            ) {
            //-If Civilian, Adventurer or Soldier or Unaligned Non-Beast, create a Remove Poison Job.
            if (witness.role.roleType == CHARACTER_ROLE.CIVILIAN || witness.role.roleType == CHARACTER_ROLE.ADVENTURER 
                || witness.role.roleType == CHARACTER_ROLE.SOLDIER || witness.role.roleType == CHARACTER_ROLE.BANDIT 
                || (witness.role.roleType != CHARACTER_ROLE.BEAST && witness.isFactionless)) {
                if (!witness.jobQueue.HasJob(JOB_TYPE.REMOVE_POISON, poiTarget)) {
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
                    witness.jobQueue.AddJobInQueue(job);
                }
            }
            //- If Noble or Faction Leader, create an Ask for Help Remove Poison Job.
            else if (witness.role.roleType == CHARACTER_ROLE.NOBLE || witness.role.roleType == CHARACTER_ROLE.LEADER) {
                witness.CreateAskForHelpJob(tableOwner, INTERACTION_TYPE.REMOVE_POISON_TABLE, poiTarget);
            }
        }
        //- The witness should not eat at the table until the Poison has been removed
        //Add character to poisoned trait of table
        //and when getting the cost of eating at this table, check if the character knows about the poison, if he/she does, increase cost.
        Poisoned poisonedTrait = poiTarget.GetNormalTrait("Poisoned") as Poisoned;
        if (poisonedTrait == null) {
            throw new System.Exception("Poisoned trait of " + poiTarget.ToString() + " is null! But it was just poisoned by " + actor.name);
        }
        poisonedTrait.AddAwareCharacter(witness);
    }
    protected override void OldNewsTrigger(IPointOfInterest poi) {
        base.OldNewsTrigger(poi);

    }
    #endregion

    #region State Effects
    public void PrePoisonSuccess() {
        SetCommittedCrime(CRIME.ATTEMPTED_MURDER, new Character[] { actor });
        //**Effect 1**: Add Poisoned Trait to target table
        AddTraitTo(poiTarget, new Poisoned(), actor);
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.SetIntelReaction(PoisonSuccessReactions);
        //UIManager.Instance.Pause();
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        //**Advertiser**: All Tables inside Dwellings
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        LocationGridTile knownLoc = actor.GetAwareness(poiTarget).knownGridLocation;
        if (knownLoc.structure is Dwelling) {
            Dwelling d = knownLoc.structure as Dwelling;
            if (d.residents.Count == 0) {
                return false;
            }
            Poisoned poisonedTrait = poiTarget.GetNormalTrait("Poisoned") as Poisoned;
            if (poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
                return false; //to prevent poisoning a table that has been already poisoned by this character
            }
            return !d.IsResident(actor);
        }

        return false;
    }
    #endregion

    #region Intel Reactions
    private List<string> PoisonSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();

        //NOTE: If the eat at table action of the intel is null, nobody has eaten at this table yet.
        //NOTE: Poisoned trait has a list of characters that poisoned it. If the poisoned trait that is currently on the table has the actor of this action in it's list
        //this action is still valid for reactions where the table is currently poisoned.
        PoisonTableIntel pti = sharedIntel as PoisonTableIntel;
        Poisoned poisonedTrait = poiTarget.GetNormalTrait("Poisoned") as Poisoned;
        Dwelling targetDwelling = poisonedTrait.poi.gridTileLocation.structure as Dwelling;

        bool tableHasPoison = poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor);

        if (isOldNews) {
            reactions.Add("This is old news.");
            return reactions;
        }

        if (_assumedTargetCharacter == null && targetDwelling.residents.Count > 0) {
            TileObject table = poiTarget as TileObject;
            if (targetDwelling.IsResident(recipient)) {
                _assumedTargetCharacter = recipient;
            } else {
                List<Character> positiveRelOwners = targetDwelling.residents.Where(x => recipient.GetRelationshipEffectWith(x) == RELATIONSHIP_EFFECT.POSITIVE).ToList();
                if(positiveRelOwners != null && positiveRelOwners.Count > 0) {
                    _assumedTargetCharacter = positiveRelOwners[UnityEngine.Random.Range(0, positiveRelOwners.Count)];
                } else {
                    List<Character> negativeRelOwners = targetDwelling.residents.Where(x => recipient.GetRelationshipEffectWith(x) == RELATIONSHIP_EFFECT.NEGATIVE).ToList();
                    if (negativeRelOwners != null && negativeRelOwners.Count > 0) {
                        _assumedTargetCharacter = negativeRelOwners[UnityEngine.Random.Range(0, negativeRelOwners.Count)];
                    } else {
                        List<Character> sameFactionOwners = targetDwelling.residents.Where(x => recipient.faction.id == x.faction.id).ToList();
                        if (sameFactionOwners != null && sameFactionOwners.Count > 0) {
                            _assumedTargetCharacter = sameFactionOwners[UnityEngine.Random.Range(0, sameFactionOwners.Count)];
                        } else {
                            _assumedTargetCharacter = targetDwelling.residents[UnityEngine.Random.Range(0, targetDwelling.residents.Count)];
                        }
                    }
                }
            }
        }

        if(_assumedTargetCharacter == null) {
            if (recipient == actor) {
                reactions.Add("What are you talking about?! I did not plan to poison anyone. How dare you accuse me?!");
                AddTraitTo(recipient, "Annoyed");
            } else {
                reactions.Add("I think it was a mistake.");
            }
        } else {
            Character targetCharacter = _assumedTargetCharacter;
            Sick sickTrait = targetCharacter.GetNormalTrait("Sick") as Sick;
            Dead deadTrait = targetCharacter.GetNormalTrait("Dead") as Dead;
            bool targetIsSick = sickTrait != null && sickTrait.gainedFromDoing != null && sickTrait.gainedFromDoing.poiTarget == poiTarget;
            bool targetIsDead = deadTrait != null && deadTrait.gainedFromDoing != null && deadTrait.gainedFromDoing.poiTarget == poiTarget;

            if (awareCharactersOfThisAction.Contains(recipient)) {
                //- If Recipient is Aware
                if (recipient == actor) {
                    reactions.Add("Yes, I did that.");
                } else {
                    reactions.Add(string.Format("I already know that {0} poisoned {1}.", actor.name, poiTarget.name));
                }
            } else {
                //- If Recipient is Not Aware
                //- Recipient is Actor
                CHARACTER_MOOD recipientMood = recipient.currentMoodType;
                if (recipient == actor) {
                    if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
                        //- If Negative Mood: "Are you threatening me?!"
                        reactions.Add("Are you threatening me?!");
                    } else {
                        //- If Positive Mood: "Yes I did that."
                        reactions.Add("Yes I did that.");
                    }
                }
                //- Recipient is Target
                else if (recipient == targetCharacter) {
                    if (recipient.faction == actor.faction) {
                        //- Same Faction
                        if (!recipient.HasRelationshipWith(actor)) {
                            if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
                                //- No Relationship (Negative Mood)
                                if (tableHasPoison) {
                                    reactions.Add(string.Format("{0} wants to poison me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                                    CreateRemovePoisonJob(recipient);
                                    recipient.CreateUndermineJobOnly(actor, "idle", status);
                                } else if (targetIsSick) {
                                    reactions.Add(string.Format("{0} poisoned me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                                    recipient.CreateUndermineJobOnly(actor, "idle", status);
                                } else {
                                    reactions.Add(string.Format("{0} wants to poison me?", actor.name));
                                }
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                }
                            } else {
                                //- No Relationship (Positive Mood)
                                if (tableHasPoison) {
                                    reactions.Add(string.Format("{0} wants to poison me? I've got to do something about this.", actor.name));
                                    CreateRemovePoisonJob(recipient);
                                } else if (targetIsSick) {
                                    reactions.Add(string.Format("{0} poisoned me?! Oh my. :(", actor.name));
                                } else {
                                    reactions.Add(string.Format("{0} wants to poison me?", actor.name));
                                }
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                }
                            }
                        } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE) {
                            //- Has Negative Relationship
                            if (tableHasPoison) {
                                reactions.Add(string.Format("That stupid {0} wants to poison me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                                CreateRemovePoisonJob(recipient);
                                recipient.CreateUndermineJobOnly(actor, "idle", status);
                            } else if (targetIsSick) {
                                reactions.Add(string.Format("That stupid {0} poisoned me?! {1} will get what {2} deserves!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                                recipient.CreateUndermineJobOnly(actor, "idle", status);
                            } else {
                                reactions.Add(string.Format("That stupid {0} wants to poison me?!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true), Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
                            }
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                        } else if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                            //- Has Positive Relationship
                            if (tableHasPoison) {
                                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                    reactions.Add(string.Format("Why does {0} want me dead? I've got to do something about this!", actor.name));
                                    if (!hasCrimeBeenReported) {
                                        recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                    }
                                } else {
                                    reactions.Add("I just have to remove the poison and everything will go back to the way it was.");
                                }
                                CreateRemovePoisonJob(recipient);
                            } else if (targetIsSick) {
                                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                    reactions.Add(string.Format("Why does {0} want me dead? I've got to do something about this!", actor.name));
                                    if (!hasCrimeBeenReported) {
                                        recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                    }
                                } else {
                                    reactions.Add("Relax. I didn't die. I just got sick. I'm sure I'll recover in no time.");
                                }
                            } else {
                                reactions.Add(string.Format("Why does {0} want me dead?", actor.name));
                            }
                        }
                    } else {
                        //- Not Same Faction
                        if (recipient.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
                            //- Has Positive Relationship
                            if (tableHasPoison) {
                                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                    reactions.Add(string.Format("{0} wants to poison me?! {1} will not get away with this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true)));
                                    recipient.CreateUndermineJobOnly(actor, "idle", status);
                                } else {
                                    reactions.Add("I just have to remove the poison and everything will go back to the way it was.");
                                }
                                CreateRemovePoisonJob(recipient);
                            } else if (targetIsSick) {
                                if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                    reactions.Add(string.Format("{0} poisoned me?! I will have my revenge!", actor.name));
                                    recipient.CreateUndermineJobOnly(actor, "idle", status);
                                } else {
                                    reactions.Add("Relax. I didn't die. I just got sick. I'm sure I'll recover in no time.");
                                }
                            } else {
                                reactions.Add(string.Format("{0} wants to poison me?! I knew those kind of people could never be trusted.", actor.name));
                            }
                        } else {
                            //- Has Negative/No Relationship
                            if (tableHasPoison) {
                                reactions.Add(string.Format("{0} will not get away with this!", actor.name));
                                CreateRemovePoisonJob(recipient);
                                recipient.CreateUndermineJobOnly(actor, "idle", status);
                            } else if (targetIsSick) {
                                reactions.Add(string.Format("{0} will not get away with this!", actor.name));
                                recipient.CreateUndermineJobOnly(actor, "idle", status);
                            } else {
                                reactions.Add(string.Format("{0} will not get away with this! I knew those kind of people could never be trusted.", actor.name));
                            }
                        }
                    }
                }
                //- Recipient Has Positive Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (tableHasPoison) {
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("{0} wants to poison {1}? I've got to do something about this.", actor.name, targetCharacter.name));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                }
                                recipient.CreateShareInformationJob(targetCharacter, this);
                            } else {
                                reactions.Add(string.Format("{0} wants to poison {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (targetIsSick) {
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("{0} poisoned {1}? I've got to do something about this.", actor.name, targetCharacter.name));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                }
                                recipient.CreateShareInformationJob(targetCharacter, this);
                            } else {
                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (targetIsDead) {
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("{0} poisoned {1}? I've got to do something about this.", actor.name, targetCharacter.name));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                                }
                            } else {
                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else {
                            reactions.Add(string.Format("{0} wants to poison {1}?", actor.name, targetCharacter.name));
                        }
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        if (tableHasPoison) {
                            reactions.Add(string.Format("{0} wants to poison {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsSick) {
                            reactions.Add(string.Format("{0} poisoned {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsDead) {
                            reactions.Add(string.Format("{0} poisoned {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                            }
                        } else {
                            reactions.Add(string.Format("{0} wants to poison {1}? What a horrible person!", actor.name, targetCharacter.name));
                        }
                    } else {
                        if (tableHasPoison) {
                            reactions.Add(string.Format("{0} could die. I've got to do something about this!", targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsSick) {
                            reactions.Add(string.Format("{0} almost died. I've got to do something about this!", targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsDead) {
                            reactions.Add(string.Format("{0} died. I've got to do something about this!", targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                            }
                        } else {
                            reactions.Add(string.Format("{0} could die.", targetCharacter.name));
                        }
                    }
                }
                //- Recipient Has Negative Relationship with Target
                else if (recipient.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (tableHasPoison) {
                            reactions.Add(string.Format("I hope {0} dies from that poison!", targetCharacter.name));
                        } else if (targetIsSick) {
                            reactions.Add(string.Format("{0} deserves worse but that will do.", targetCharacter.name));
                            AddTraitTo(recipient, "Cheery");
                        } else if (targetIsDead) {
                            reactions.Add("Good riddance.");
                            AddTraitTo(recipient, "Cheery");
                        } else {
                            reactions.Add(string.Format("I can't wait for {0} to die from that poison.", targetCharacter.name));
                        }
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        if (tableHasPoison) {
                            reactions.Add(string.Format("I hate both of them but I hope {0} dies from that poison!", targetCharacter.name));
                        } else if (targetIsSick) {
                            reactions.Add(string.Format("{0} deserves worse but that will do.", targetCharacter.name));
                            AddTraitTo(recipient, "Cheery");
                        } else if (targetIsDead) {
                            reactions.Add(string.Format("Good riddance. I hope {0} is next.", actor.name));
                            AddTraitTo(recipient, "Cheery");
                        } else {
                            reactions.Add(string.Format("I hate both of them but I can't wait for {0} to die from that poison.", targetCharacter.name));
                        }
                    } else {
                        if (tableHasPoison) {
                            reactions.Add(string.Format("I hope {0} dies from that poison!", targetCharacter.name));
                        } else if (targetIsSick) {
                            reactions.Add(string.Format("{0} deserves worse but that will do.", targetCharacter.name));
                            AddTraitTo(recipient, "Cheery");
                        } else if (targetIsDead) {
                            reactions.Add("Good riddance.");
                            AddTraitTo(recipient, "Cheery");
                        } else {
                            reactions.Add(string.Format("I can't wait for {0} to die from that poison.", targetCharacter.name));
                        }
                    }
                }
                //- Recipient Has No Relationship with Target
                else {
                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.GetRelationshipEffectWith(actor);
                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
                        if (tableHasPoison) {
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("{0} wants to poison {1}? This is unacceptable!", actor.name, targetCharacter.name));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                }
                                recipient.CreateShareInformationJob(targetCharacter, this);
                            } else {
                                reactions.Add(string.Format("{0} wants to poison {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (targetIsSick) {
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("{0} poisoned {1}? This is unacceptable!", actor.name, targetCharacter.name));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                                }
                                recipient.CreateShareInformationJob(targetCharacter, this);
                            } else {
                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else if (targetIsDead) {
                            if (CharacterManager.Instance.RelationshipDegradation(actor, recipient, this)) {
                                reactions.Add(string.Format("{0} poisoned {1}? This is unacceptable!", actor.name, targetCharacter.name));
                                if (!hasCrimeBeenReported) {
                                    recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                                }
                            } else {
                                reactions.Add(string.Format("{0} poisoned {1}? I don't believe that.", actor.name, targetCharacter.name));
                            }
                        } else {
                            reactions.Add(string.Format("{0} wants to poison {1}? Why?!", actor.name, targetCharacter.name));
                        }
                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                        if (tableHasPoison) {
                            reactions.Add(string.Format("{0} wants to poison {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsSick) {
                            reactions.Add(string.Format("{0} poisoned {1}? Why am I not surprised?", actor.name, targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsDead) {
                            reactions.Add(string.Format("{0} poisoned {1}? I can't let a killer loose.", actor.name, targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                            }
                        } else {
                            reactions.Add(string.Format("{0} wants to poison {1}? So horrible!", actor.name, targetCharacter.name));
                        }
                    } else {
                        if (tableHasPoison) {
                            reactions.Add(string.Format("{0} could die. I've got to do something about this!", targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsSick) {
                            reactions.Add(string.Format("{0} almost died. I've got to do something about this!", targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, this, actorAlterEgo, status);
                            }
                            recipient.CreateShareInformationJob(targetCharacter, this);
                        } else if (targetIsDead) {
                            reactions.Add(string.Format("{0} died. I've got to do something about this!", targetCharacter.name));
                            if (!hasCrimeBeenReported) {
                                recipient.ReactToCrime(CRIME.MURDER, this, actorAlterEgo, status);
                            }
                        } else {
                            reactions.Add(string.Format("{0} could die.", targetCharacter.name));
                        }
                    }
                }
            }
        }
        return reactions;
    }
    #endregion

    private void CreateRemovePoisonJob(Character recipient) {
        if (recipient.role.roleType == CHARACTER_ROLE.CIVILIAN || recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.BANDIT || (recipient.role.roleType != CHARACTER_ROLE.BEAST && recipient.isFactionless)) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
            recipient.jobQueue.AddJobInQueue(job);
        }
        //If Civilian, Noble or Faction Leader, create an Ask for Help Remove Poison Job.
        else if (recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
            recipient.CreateAskForHelpJob(_assumedTargetCharacter, INTERACTION_TYPE.REMOVE_POISON_TABLE, poiTarget);
        }
    }
}
