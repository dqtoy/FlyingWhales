using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisible : Trait {

    public int level { get; private set; }

    public List<Character> charactersThatCanSee { get; private set; }

    public Invisible(int level) {
        name = "Invisible";
        description = "This character is invisible.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        this.level = level;
        charactersThatCanSee = new List<Character>();
    }

    public void AddCharacterThatCanSee(Character character) {
        charactersThatCanSee.Add(character);
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        base.OnAddTrait(sourcePOI);
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.StopAllActionTargettingThis();
            character.CancelAllJobsTargettingThisCharacter();
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        base.OnRemoveTrait(sourcePOI);
        if (sourcePOI is Character) {
            GameManager.Instance.StartCoroutine(RetriggerVisionCollision(sourcePOI as Character));
        }
    }
    #endregion

    private IEnumerator RetriggerVisionCollision(Character character) {
        for (int i = 0; i < character.marker.colliders.Length; i++) {
            character.marker.colliders[i].enabled = false;
        }
        yield return null;
        for (int i = 0; i < character.marker.colliders.Length; i++) {
            character.marker.colliders[i].enabled = true;
        }
    }
}
