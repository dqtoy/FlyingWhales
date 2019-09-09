﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inspiring : Trait {
    public Inspiring() {
        name = "Inspiring";
        description = "This character inspires other characters.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            //Anyone from same faction that sees this character gains +100 Happiness Recovery. Exclude those that consider him enemy.
            if (targetCharacter.faction == characterThatWillDoJob.faction && !targetCharacter.HasRelationshipOfTypeWith(characterThatWillDoJob, RELATIONSHIP_TRAIT.ENEMY)) {
                characterThatWillDoJob.AdjustHappiness(100);
                Debug.Log(GameManager.Instance.TodayLogString() + characterThatWillDoJob.name + " saw " + targetCharacter.name + " and became a bit happier!");
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}