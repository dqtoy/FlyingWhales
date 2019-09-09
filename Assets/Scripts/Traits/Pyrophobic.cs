using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pyrophobic : Trait {

    private Character owner;
    private List<BurningSource> seenBurningSources;

    public Pyrophobic() {
        name = "Pyrophobic";
        description = "This character is afraid of fire.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        seenBurningSources = new List<BurningSource>();
    }

    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if (addedTo is Character) {
            owner = addedTo as Character;
        }
    }


    public bool AddKnownBurningSource(BurningSource burningSource) {
        if (!seenBurningSources.Contains(burningSource)) {
            seenBurningSources.Add(burningSource);
            burningSource.AddOnBurningExtinguishedAction(RemoveKnownBurningSource);
            burningSource.AddOnBurningObjectAddedAction(OnObjectStartedBurning);
            burningSource.AddOnBurningObjectRemovedAction(OnObjectStoppedBurning);
            return true;
        }
        return false;
    }
    public void RemoveKnownBurningSource(BurningSource burningSource) {
        if (seenBurningSources.Remove(burningSource)) {
            burningSource.RemoveOnBurningExtinguishedAction(RemoveKnownBurningSource);
            burningSource.RemoveOnBurningObjectAddedAction(OnObjectStartedBurning);
            burningSource.RemoveOnBurningObjectRemovedAction(OnObjectStoppedBurning);
        }
    }

    public void Flee(BurningSource source, Character character) {
        string summary = GameManager.Instance.TodayLogString() + character.name + " saw burning source " + source.ToString();
        if (character.marker.FleeFrom(source.objectsOnFire)) {
            summary += "\nStarted fleeing";
            //owner.marker.AddTerrifyingObject(source.objectsOnFire);
            character.CancelAllPlans();
            Log log = new Log(GameManager.Instance.Today(), "Trait", this.GetType().ToString(), "flee_pyrophobia");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(character, log);
        } else {
            summary += "\nDid not flee because already fleeing.";
        }
        Debug.Log(summary);
    }

    private void OnObjectStartedBurning(IPointOfInterest poi) {
        //owner.marker.AddTerrifyingObject(poi);
    }
    private void OnObjectStoppedBurning(IPointOfInterest poi) {
        //owner.marker.RemoveTerrifyingObject(poi);
    }
}
