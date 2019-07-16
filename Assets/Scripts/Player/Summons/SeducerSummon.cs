using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeducerSummon : Summon {

    public int seduceChance;
    private List<Character> doneCharacters; //list of characters that the succubus has invited to make love with, regardless of success
    public override int ignoreHostility {
        get {
            if (currentAction != null && currentAction.goapType.IsHostileAction()) {
                return 0; //allow hostility checking
            } else if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT || stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED)) {
                return 0; //if in combat or berserked state allow hostility checking
            }
            return 1; //default is that succubi won't be seen as hostiles.
        }
    }

    public SeducerSummon(SUMMON_TYPE type, GENDER gender) : base(type, CharacterRole.MINION, RACE.DEMON, gender) {
        seduceChance = 50;
        doneCharacters = new List<Character>();
        AddInteractionType(INTERACTION_TYPE.INVITE_TO_MAKE_LOVE);
        AddInteractionType(INTERACTION_TYPE.MAKE_LOVE);
    }

    #region Overrides
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
    }
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        Messenger.AddListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
        AdjustIgnoreHostilities(1);
    }
    public override void ThisCharacterSaw(Character target) {
        if (GetNormalTrait("Unconscious", "Resting") != null) {
            return;
        }
        //NOTE: removed ability of skeletons to watch/witness an event
        Spooked spooked = GetNormalTrait("Spooked") as Spooked;
        if (spooked != null) {
            if (marker.AddAvoidInRange(target)) {
                spooked.AddTerrifyingCharacter(target);
            }
        }
    }
    protected override void IdlePlans() {
        if (_hasAlreadyAskedForPlan) {
            return;
        }
        SetHasAlreadyAskedForPlan(true);
        //pick a random character that is sexually compatible with this character, to seduce. Exclude characters that this succubus has already invited.
        List<Character> choices = specificLocation.charactersAtLocation.Where(x => !doneCharacters.Contains(x) && CharacterManager.Instance.IsSexuallyCompatibleOneSided(x, this) && !x.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)).ToList();
        if (choices.Count > 0) {
            Character chosenCharacter = choices[Random.Range(0, choices.Count)];
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.ABDUCT, INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, chosenCharacter);
            job.SetCannotOverrideJob(true);
            job.SetCannotCancelJob(true);
            jobQueue.AddJobInQueue(job, false);
        } else {
            //just enter berserked mode.
            stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
            SetHasAlreadyAskedForPlan(false);
        }
    }
    protected override void OnActionStateSet(GoapAction action, GoapActionState state) {
        if (action.actor == this && action.goapType == INTERACTION_TYPE.INVITE_TO_MAKE_LOVE) {
            doneCharacters.Add(action.poiTarget as Character);
        }
    }
    #endregion
}