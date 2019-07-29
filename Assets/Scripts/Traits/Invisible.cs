using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisible : Trait {

    public Character owner { get; private set; }
    public List<Character> charactersThatCanSee { get; private set; }
    public List<Character> inRangeOfVisionCharacters { get; private set; } //Must keep a list of all characters that is in vision of this character but is not added in inVisionPOIs because this character is invisible
    public Invisible() {
        name = "Invisible";
        description = "This character is invisible.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        charactersThatCanSee = new List<Character>();
        inRangeOfVisionCharacters = new List<Character>();
    }

    public void AddCharacterThatCanSee(Character character) {
        charactersThatCanSee.Add(character);
        if (RemoveInRangeOfVisionCharacter(character)) {
            //If a character can see this character and it is in vision range, transfer this character to inVisionPOIs list of the character that can see
            character.marker.AddPOIAsInVisionRange(owner);
        }
    }
    public void AddInRangeOfVisionCharacter(Character character) {
        inRangeOfVisionCharacters.Add(character);
    }
    public bool RemoveInRangeOfVisionCharacter(Character character) {
        return inRangeOfVisionCharacters.Remove(character);
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        base.OnAddTrait(sourcePOI);
        if (sourcePOI is Character) {
            owner = sourcePOI as Character;
            owner.StopAllActionTargettingThis();
            owner.CancelAllJobsTargettingThisCharacter();
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI, Character removedBy) {
        base.OnRemoveTrait(sourcePOI, removedBy);
        GameManager.Instance.StartCoroutine(RetriggerVisionCollision(owner));
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
