using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkBuff : Trait {

    private int _flatAttackMod;
    private int _flatHPMod;
    private int _flatSpeedMod;

    public BerserkBuff(int level) {
        name = "Berserk Buff";
        description = "Temporary increased in stats.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        if (level == 1) {
            _flatAttackMod = 100;
            _flatHPMod = 500;
            _flatSpeedMod = 100;
        } else if (level == 2) {
            _flatAttackMod = 200;
            _flatHPMod = 1000;
            _flatSpeedMod = 200;
        } else if (level == 3) {
            _flatAttackMod = 300;
            _flatHPMod = 1500;
            _flatSpeedMod = 300;
        }
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        if(sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.AdjustAttackMod(_flatAttackMod);
            character.AdjustMaxHPMod(_flatHPMod);
            character.AdjustSpeedMod(_flatSpeedMod);
        }
        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
        base.OnRemoveTrait(sourceCharacter, removedBy);
        if (sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.AdjustAttackMod(_flatAttackMod);
            character.AdjustMaxHPMod(_flatHPMod);
            character.AdjustSpeedMod(_flatSpeedMod);
        }
    }
    #endregion
}
