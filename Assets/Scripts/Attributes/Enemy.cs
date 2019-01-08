using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Trait {
    public Character targetCharacter;

    public override string nameInUI {
        get { return "Enemy: " + targetCharacter.name; }
    }

    public Enemy(Character target) {
        targetCharacter = target;
        name = "Enemy";
        description = "This character is an enemy of " + targetCharacter.name;
        type = TRAIT_TYPE.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(Character sourceCharacter) {
        Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "enemy");
        log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
    }
    public override void OnRemoveTrait(Character sourceCharacter) {
        Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "not_enemy");
        log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
    }
    public override bool IsUnique() {
        return false;
    }
    #endregion
}
