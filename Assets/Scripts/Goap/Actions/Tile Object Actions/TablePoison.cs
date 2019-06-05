using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TablePoison : GoapAction {
    protected override string failActionState { get { return "Poison Fail"; } }

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
                witness.CreateAskForHelpJob(tableOwner, INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, poiTarget);
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
    private List<string> PoisonSuccessReactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();

        PoisonTableIntel pti = sharedIntel as PoisonTableIntel;
        Poisoned poisonedTrait = poiTarget.GetNormalTrait("Poisoned") as Poisoned;
        Character tableOwner = pti.targetDwelling.owner;
        //NOTE: If the eat at table action of the intel is null, nobody has eaten at this table yet.
        //NOTE: Poisoned trait has a list of characters that poisoned it. If the poisoned trait that is currently on the table has the actor of this action in it's list
        //this action is still valid for reactions where the table is currently poisoned.

        //Recipient is the same as the actor:
        if (recipient == actor) {
            // **Recipient Response Text**: "I know what I've done!"
            reactions.Add("I know what I've done!");
            //-**Recipient Effect * *: no effect
        }

        //Recipient is the owner of the Poisoned Table and the Table is still currently poisoned by the actor of this action:
        else if (pti.targetDwelling.IsResident(recipient) && poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
            //- **Recipient Response Text**: "[Actor Name] wants to poison me? I've got to do something about this!"
            reactions.Add(string.Format("{0} wants to poison me? I've got to do something about this!", actor.name));
            //-**Recipient Effect**: Recipient will create a Remove Poison Job to his personal job queue. 
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
            recipient.jobQueue.AddJobInQueue(job);
            //Add Enemy relationship if they are not yet enemies. 
            if (!recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.ENEMY);
            }
            //Apply Crime System handling as if the Recipient witnessed Actor commit an Attempted Murder.
            recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, actorAlterEgo, null, this);
        }
        //Recipient is the owner of the Poisoned Table and have gained Sick trait by using the Table:
        else if (pti.targetDwelling.IsResident(recipient) && pti.eatAtTableAction != null && pti.eatAtTableAction.HasActualEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Sick", recipient)) {
            //- **Recipient Response Text**: "That despicable [Actor Name] made me gravely ill! I almost died. [He/She] will pay for this!"
            reactions.Add(string.Format("That despicable {0} made me gravely ill! I almost died. {1} will pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true)));
            //-**Recipient Effect**: Remove any positive relationships between Actor and Recipient. 
            List<RelationshipTrait> traitsToRemove = recipient.GetAllRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE);
            CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove);
            //Add Enemy relationship if they are not yet enemies. 
            if (!recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.ENEMY);
            }
            //Apply Crime System handling as if the Recipient witnessed Actor commit an Assault.
            recipient.ReactToCrime(CRIME.ASSAULT, actorAlterEgo, null, this);
        }
        //Recipient has a positive relationship with a character that became Sick by using the Table:
        else if (pti.eatAtTableAction != null && pti.eatAtTableAction.HasActualEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Sick", pti.eatAtTableAction.actor)
            && recipient.HasRelationshipOfEffectWith(pti.eatAtTableAction.actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE)) {
            //- **Recipient Response Text**: "That despicable [Actor Name] almost killed [Sick Character Name]! That's an assault!"
            reactions.Add(string.Format("That despicable {0} almost killed {1}! That's an assault!", actor.name, pti.eatAtTableAction.actor));
            //-**Recipient Effect * *: Remove any positive relationships between Actor and Recipient.
            List<RelationshipTrait> traitsToRemove = recipient.GetAllRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE);
            CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove);
            //Add Enemy relationship if they are not yet enemies. 
            if (!recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.ENEMY);
            }
            //Apply Crime System handling as if the Recipient witnessed Actor commit an Assault.
            recipient.ReactToCrime(CRIME.ASSAULT, actorAlterEgo, null, this);
        }
        //Recipient has a positive relationship with a character killed by using the Table:
        else if (pti.eatAtTableAction != null && pti.eatAtTableAction.HasActualEffect(GOAP_EFFECT_CONDITION.DEATH, null, pti.eatAtTableAction.actor)
            && recipient.HasRelationshipOfEffectWith(pti.eatAtTableAction.actor, TRAIT_EFFECT.POSITIVE)) {
            //- **Recipient Response Text**: "That despicable [Actor Name] killed [Killed Character Name]! [He/She] is a murderer!"
            reactions.Add(string.Format("That despicable {0} killed {1}! {2} is a murderer!", actor.name, pti.eatAtTableAction.actor, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true)));
            //-**Recipient Effect * *: Remove any positive relationships between Actor and Recipient. 
            List<RelationshipTrait> traitsToRemove = recipient.GetAllRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE);
            CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove);
            //Add Enemy relationship if they are not yet enemies. 
            if (!recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.ENEMY);
            }
            //Apply Crime System handling as if the Recipient witnessed Actor commit a Murder.
            recipient.ReactToCrime(CRIME.MURDER, actorAlterEgo, null, this);
        }
        //Recipient has a negative relationship with a character that has gotten sick by using the Table:
        else if (pti.eatAtTableAction != null && pti.eatAtTableAction.HasActualEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Sick", pti.eatAtTableAction.actor)
            && recipient.HasRelationshipOfEffectWith(pti.eatAtTableAction.actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "I am glad that [Actor Name] dealt with [Sick Character Name]! Too bad it didn't end up killing [Target Name]."
            reactions.Add(string.Format("I am glad that {0} dealt with {1}! Too bad it didn't end up killing {1}!", actor.name, pti.eatAtTableAction.actor));
            //-**Recipient Effect * *: no effect
        }
        //Recipient has a negative relationship with a character killed by using the Table:
         else if (pti.eatAtTableAction != null && pti.eatAtTableAction.HasActualEffect(GOAP_EFFECT_CONDITION.DEATH, null, pti.eatAtTableAction.actor)
            && recipient.HasRelationshipOfEffectWith(pti.eatAtTableAction.actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "I am glad that [Actor Name] dealt with [Killed Character Name]!"
            reactions.Add(string.Format("I am glad that {0} dealt with {1}!", actor.name, pti.eatAtTableAction.actor));
            //-**Recipient Effect * *: If Actor and Recipient have no relationships yet, they will become friends.
            if (!recipient.HasRelationshipWith(actor)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.FRIEND);
            }
        }
        //Recipient has a positive relationship with owner of the Table and the Table is still currently poisoned:
        else if (tableOwner != null && recipient.HasRelationshipOfEffectWith(tableOwner, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE) && poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
            //- **Recipient Response Text**: "Thank you for letting me know about this. I've got to find a way to remove that poison to save [Target Name]!
            reactions.Add(string.Format("Thank you for letting me know about this. I've got to find a way to remove that poison to save {0}!", tableOwner.name));
            //-**Recipient Effect * *: If Adventurer or Soldier or Unaligned Non - Beast, create a Remove Poison Job.
            if (recipient.role.roleType == CHARACTER_ROLE.CIVILIAN || recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.BANDIT || (recipient.role.roleType != CHARACTER_ROLE.BEAST && recipient.isFactionless)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
                recipient.jobQueue.AddJobInQueue(job);
            }
            //If Civilian, Noble or Faction Leader, create an Ask for Help Remove Poison Job.
            else if (recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                recipient.CreateAskForHelpJob(tableOwner, INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, poiTarget);
            }
            //Apply Crime System handling as if the Recipient witnessed Actor commit an Attempted Murder.
            recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, actorAlterEgo, null, this);
        }
        //Recipient has no relationship with owner of the table but they are from the same faction and the Table is still currently poisoned:
        else if (tableOwner != null && !recipient.HasRelationshipWith(tableOwner) && tableOwner.faction == recipient.faction && poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
            //- **Recipient Response Text**: "Thank you for letting me know about this. I've got to find a way to remove that poison to save [Target Name]!
            reactions.Add(string.Format("Thank you for letting me know about this. I've got to find a way to remove that poison to save {0}!", tableOwner.name));
            //-**Recipient Effect * *: If Adventurer or Soldier or Unaligned Non - Beast, create a Remove Poison Job.
            if (recipient.role.roleType == CHARACTER_ROLE.CIVILIAN || recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.BANDIT || (recipient.role.roleType != CHARACTER_ROLE.BEAST && recipient.isFactionless)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
                recipient.jobQueue.AddJobInQueue(job);
            }
            //If Civilian, Noble or Faction Leader, create an Ask for Help Remove Poison Job.
            else if (recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                recipient.CreateAskForHelpJob(tableOwner, INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, poiTarget);
            }
            //Apply Crime System handling as if the Recipient witnessed Actor commit an Attempted Murder.
            recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, actorAlterEgo, null, this);
        }
        //Recipient and Target are enemies and the Table is still currently poisoned:
        else if (tableOwner != null && recipient.HasRelationshipOfTypeWith(tableOwner, RELATIONSHIP_TRAIT.ENEMY) && poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
            //- **Recipient Response Text**: "I hope that kills [Target Name]."
            reactions.Add(string.Format("I hope that kills {0}.", tableOwner.name));
            //-**Recipient Effect * *: no effect
        }
        //Recipient and Actor have a positive relationship and the Table is still currently poisoned. Recipient and Target are not enemies:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE) && poisonedTrait != null
            && poisonedTrait.responsibleCharacters.Contains(actor) && tableOwner != null && !recipient.HasRelationshipOfTypeWith(tableOwner, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] is attempting murder! I've got to put a stop to this."
            reactions.Add(string.Format("{0} is attempting murder! I've got to put a stop to this.", actor.name));
            //-**Recipient Effect * *: If Adventurer or Soldier or Unaligned Non - Beast, create a Remove Poison Job.
            if (recipient.role.roleType == CHARACTER_ROLE.CIVILIAN || recipient.role.roleType == CHARACTER_ROLE.ADVENTURER || recipient.role.roleType == CHARACTER_ROLE.SOLDIER || recipient.role.roleType == CHARACTER_ROLE.BANDIT || (recipient.role.roleType != CHARACTER_ROLE.BEAST && recipient.isFactionless)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
                recipient.jobQueue.AddJobInQueue(job);
            }
            //If Civilian, Noble or Faction Leader, create an Ask for Help Remove Poison Job.
            else if (recipient.role.roleType == CHARACTER_ROLE.NOBLE || recipient.role.roleType == CHARACTER_ROLE.LEADER) {
                recipient.CreateAskForHelpJob(tableOwner, INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, poiTarget);
            }
        }
        //Recipient and Actor have a positive relationship and the Table already killed the Target. Recipient and Target are not enemies:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE) && pti.eatAtTableAction != null 
            && tableOwner != null && pti.eatAtTableAction.HasActualEffect(GOAP_EFFECT_CONDITION.DEATH, null, tableOwner) && !recipient.HasRelationshipOfTypeWith(tableOwner, RELATIONSHIP_TRAIT.ENEMY)) {
            //- **Recipient Response Text**: "[Actor Name] killed somebody! This is horrible!"
            reactions.Add(string.Format("{0} killed somebody! This is horrible!", actor.name));
            //-**Recipient Effect * *: Apply Crime System handling as if the Recipient witnessed Actor commit a Murder.
            recipient.ReactToCrime(CRIME.MURDER, actorAlterEgo, null, this);
        }
        //Recipient and Actor have no positive relationship but are from the same faction and the Table is still currently poisoned. Recipient and Target have no relationship:
        else if (!recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE, RELATIONSHIP_TRAIT.RELATIVE) && recipient.faction == actor.faction && poisonedTrait != null
            && poisonedTrait.responsibleCharacters.Contains(actor) && tableOwner != null && !recipient.HasRelationshipWith(tableOwner)) {
            //- **Recipient Response Text**: "[Actor Name] is attempting murder!"
            reactions.Add(string.Format("{0} is attempting murder!", actor.name));
            //-**Recipient Effect * *: Apply Crime System handling as if the Recipient witnessed Actor commit an Attempted Murder.
            recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, actorAlterEgo, null, this);
        }

        //Recipient has no relationship with the Actor but they are from the same faction and a character has been killed by using the Table.
        else if (!recipient.HasRelationshipWith(actor) && actor.faction == recipient.faction && pti.eatAtTableAction != null
            && tableOwner != null && pti.eatAtTableAction.HasActualEffect(GOAP_EFFECT_CONDITION.DEATH, null, tableOwner)) {
            //-**Recipient Response Text**: "[Actor Name] killed somebody! This is horrible!"
            reactions.Add(string.Format("{0} killed somebody! This is horrible!", actor.name));
            //- **Recipient Effect * *: Apply Crime System handling as if the Recipient witnessed Actor commit a Murder.
            recipient.ReactToCrime(CRIME.MURDER, actorAlterEgo, null, this);
        }
        return reactions;
    }
    #endregion
}
