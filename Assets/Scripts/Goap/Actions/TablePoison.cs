using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TablePoison : GoapAction {
    protected override string failActionState { get { return "Poison Fail"; } }

    public TablePoison(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TABLE_POISON, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Poison Table";
        actionIconString = GoapActionStateDB.Hostile_Icon;
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
                //**Effect 3**: Kill Owner/s
                AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = dwelling.residents[i] });
            }
        }
        
        
        
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation != null && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            SetState("Poison Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 4;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Poison Fail");
    //}
    #endregion

    #region State Effects
    public void PrePoisonSuccess() {
        //**Effect 1**: Add Poisoned Trait to target table
        AddTraitTo(poiTarget, new Poisoned(), actor);
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        //**Advertiser**: All Tables inside Dwellings
        LocationGridTile knownLoc = actor.GetAwareness(poiTarget).knownGridLocation;
        return knownLoc.structure is Dwelling;
    }
    #endregion

    #region Intel Reactions
    private List<string> PoisonSuccessReactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();

        PoisonTableIntel pti = sharedIntel as PoisonTableIntel;
        Poisoned poisonedTrait = poiTarget.GetTrait("Poisoned") as Poisoned;

        //Recipient is the owner of the Poisoned Table and the Table is still currently poisoned by the actor of this action:
        if (pti.targetDwelling.IsResident(recipient) && poisonedTrait != null && poisonedTrait.responsibleCharacters.Contains(actor)) {
            //- **Recipient Response Text**: "[Actor Name] wants to poison me? I've got to do something about this!"
            reactions.Add(string.Format("{0} wants to poison me? I've got to do something about this!", actor.name));
            //-**Recipient Effect**: Recipient will create a Remove Poison Job to his personal job queue. 
            GoapPlanJob job = new GoapPlanJob("Remove Poison", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
            recipient.jobQueue.AddJobInQueue(job);
            //Add Enemy relationship if they are not yet enemies. 
            if (!recipient.HasRelationshipOfTypeWith(actor, RELATIONSHIP_TRAIT.ENEMY)) {
                CharacterManager.Instance.CreateNewRelationshipBetween(recipient, actor, RELATIONSHIP_TRAIT.ENEMY);
            }
            //Apply Crime System handling as if the Recipient witnessed Actor commit an Attempted Murder.
            recipient.ReactToCrime(CRIME.ATTEMPTED_MURDER, actor, null, false);
        }
        ////Recipient is the owner of the Poisoned Table and have gained Sick trait by using the Table:
        //else if (location.IsResident(recipient) && sickTrait != null && sickTrait.gainedFromDoing.goapType == INTERACTION_TYPE.EAT_DWELLING_TABLE) {
        //    //- **Recipient Response Text**: "That despicable [Actor Name] made me gravely ill! I almost died. [He/She] will pay for this!"
        //    reactions.Add(string.Format("That despicable {0} made me gravely ill! I almost died. {1} will pay for this!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, true)));
        //    //-**Recipient Effect**: Remove any positive relationships between Actor and Recipient. 
        //    List<RelationshipTrait> traitsToRemove = recipient.GetAllRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE);
        //    CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove);
        //    //Add Enemy relationship if they are not yet enemies. 
        //    if (!recipient.HasRelationshipOfTypeWith(poisonedTrait.responsibleCharacter, RELATIONSHIP_TRAIT.ENEMY)) {
        //        CharacterManager.Instance.CreateNewRelationshipBetween(recipient, poisonedTrait.responsibleCharacter, RELATIONSHIP_TRAIT.ENEMY);
        //    }
        //    //Apply Crime System handling as if the Recipient witnessed Actor commit an Assault.
        //}
        return reactions;
    }
    #endregion
}
