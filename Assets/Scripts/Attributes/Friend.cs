using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friend : Trait {
    public Character targetCharacter;

    public override string nameInUI {
        get { return "Friend: " + targetCharacter.name;}
    }

    public Friend(Character target) {
        targetCharacter = target;
        name = "Friend";
        description = "This character is a friend of " + targetCharacter.name;
        type = TRAIT_TYPE.POSITIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(Character sourceCharacter) {
        Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "friend");
        log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
    }
    public override void OnRemoveTrait(Character sourceCharacter) {
        Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "not_friend");
        log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
    }
    public override bool IsUnique() {
        return false;
    }
    #endregion
}
