using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingCharacter : Trait {
    private Character _targetCharacter;

    public PatrollingCharacter() : base() {
        name = "Patrolling Character";
        description = "This character is a placeholder trait ";
        type = TRAIT_TYPE.ABILITY;
        effect = TRAIT_EFFECT.POSITIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    public PatrollingCharacter(Character target) : base (){
        _targetCharacter = target;
        name = "Patrolling Character";
        description = "This character is guarding " + _targetCharacter.name;
        type = TRAIT_TYPE.ABILITY;
        effect = TRAIT_EFFECT.POSITIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    //public override void OnAddTrait(Character sourceCharacter) {
    //    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "enemy");
    //    log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //    sourceCharacter.AddHistory(log);
    //}
    //public override void OnRemoveTrait(Character sourceCharacter) {
    //    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "not_enemy");
    //    log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //    sourceCharacter.AddHistory(log);
    //}
    //public override bool IsUnique() {
    //    return false;
    //}
    #endregion
}
