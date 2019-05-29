using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Criminal : Trait {
    public override bool isPersistent { get { return true; } }
    public Criminal() {
        name = "Criminal";
        description = "This character has been branded as a criminal by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        base.OnAddTrait(sourcePOI);
        if (sourcePOI is Character) {
            Character sourceCharacter = sourcePOI as Character;
            //When a character gains this Trait, add this log to the location and the character:
            Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "add_criminal");
            addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            sourceCharacter.AddHistory(addLog);
            sourceCharacter.specificLocation.AddHistory(addLog);
        }
        
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        base.OnRemoveTrait(sourcePOI);
        if (sourcePOI is Character) {
            Character sourceCharacter = sourcePOI as Character;
            //When a character loses this Trait, add this log to the location and the character:
            Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "remove_criminal");
            addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            sourceCharacter.AddHistory(addLog);
            sourceCharacter.specificLocation.AddHistory(addLog);
        }
        
    }
    #endregion
}
