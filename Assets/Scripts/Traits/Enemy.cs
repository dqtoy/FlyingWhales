using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : RelationshipTrait {
    public override string nameInUI {
        get { return "Enemy: " + targetCharacter.name; }
    }

    public Enemy(Character target) : base (target){
        name = "Enemy";
        description = "This character is an enemy of " + targetCharacter.name;
        relType = RELATIONSHIP_TRAIT.ENEMY;
        type = TRAIT_TYPE.RELATIONSHIP;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character sourceCharacter = sourcePOI as Character;
            if (!GameManager.Instance.gameHasStarted) {
                return; //do not log initial relationships
            }
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "enemy");
            log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            sourceCharacter.AddHistory(log);
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character sourceCharacter = sourcePOI as Character;
            if (!GameManager.Instance.gameHasStarted) {
                return; //do not log initial relationships
            }
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "not_enemy");
            log.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            sourceCharacter.AddHistory(log);
        }
    }
    public override bool IsUnique() {
        return false;
    }
    #endregion
}
