using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kleptomaniac : Trait {
    public List<Character> noItemCharacters { get; private set; }
    private Character owner;

    public Kleptomaniac() {
        name = "Kleptomaniac";
        description = "This character has irresistible urge to steal.";
        thoughtText = "[Character] has irresistible urge to steal.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        noItemCharacters = new List<Character>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        (sourceCharacter as Character).RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, "Kleptomania");
        owner = sourceCharacter as Character;
        base.OnAddTrait(sourceCharacter);
        Messenger.AddListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        base.OnRemoveTrait(sourceCharacter);
        Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
    }
    public override void OnDeath() {
        base.OnDeath();
        Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
    }
    public override string GetTestingData() {
        string testingData = string.Empty;
        testingData += "Known character's with no items: \n";
        for (int i = 0; i < noItemCharacters.Count; i++) {
            testingData += noItemCharacters[i].name + ", ";
        }
        return testingData;
    }
    #endregion

    public void AddNoItemCharacter(Character character) {
        noItemCharacters.Add(character);
    }
    public void RemoveNoItemCharacter(Character character) {
        noItemCharacters.Remove(character);
    }

    private void ClearNoItemsList() {
        noItemCharacters.Clear();
        Debug.Log(GameManager.Instance.TodayLogString() + "Cleared " + owner.name + "'s Kleptomaniac list of character's with no items.");
    }

    private void CheckForClearNoItemsList() {
        //Store the character into the Kleptomaniac trait if it does not have any items. 
        //Exclude all characters listed in Kleptomaniac trait from Steal actions. Clear out the list at the start of every even day.
        if (Utilities.IsEven(GameManager.days)) {
            ClearNoItemsList();
        }
    }
}
