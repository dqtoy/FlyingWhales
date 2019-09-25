using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dead : Trait {
    public List<Character> charactersThatSawThisDead { get; private set; }
    public Dead() {
        name = "Dead";
        description = "This character is dead.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_FOOD };
        charactersThatSawThisDead = new List<Character>();
    }

    #region General
    public void AddCharacterThatSawThisDead(Character character) {
        charactersThatSawThisDead.Add(character);
    }
    #endregion

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (responsibleCharacter != characterThatWillDoJob && targetCharacter.race != RACE.SKELETON && !(targetCharacter is Summon) && !targetCharacter.HasJobTargettingThis(JOB_TYPE.BURY)) {
                GoapPlanJob buryJob = new GoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter);
                buryJob.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = targetCharacter }, INTERACTION_TYPE.CARRY_CORPSE);
                buryJob.AllowDeadTargets();
                buryJob.SetCanBeDoneInLocation(true);
                if (InteractionManager.Instance.CanTakeBuryJob(characterThatWillDoJob, buryJob)) {
                    characterThatWillDoJob.jobQueue.AddJobInQueue(buryJob, false);
                    return true;
                } else {
                    buryJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanTakeBuryJob);
                    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(buryJob, false);
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    public override string GetToolTipText() {
        if (responsibleCharacter == null) {
            return description;
        }
        return "This character was killed by " + responsibleCharacter.name;
    }
    #endregion
}

public class SaveDataDead : SaveDataTrait {
    public List<int> characterIDsThatSawThisDead;

    public override void Save(Trait trait) {
        base.Save(trait);
        Dead dead = trait as Dead;
        characterIDsThatSawThisDead = new List<int>();
        for (int i = 0; i < dead.charactersThatSawThisDead.Count; i++) {
            characterIDsThatSawThisDead.Add(dead.charactersThatSawThisDead[i].id);
        }
    }

    public override Trait Load(ref Character responsibleCharacter) {
        Trait trait = base.Load(ref responsibleCharacter);
        Dead dead = trait as Dead;
        for (int i = 0; i < characterIDsThatSawThisDead.Count; i++) {
            dead.AddCharacterThatSawThisDead(CharacterManager.Instance.GetCharacterByID(characterIDsThatSawThisDead[i]));
        }
        return trait;
    }
}
