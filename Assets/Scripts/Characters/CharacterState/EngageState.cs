using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngageState : CharacterState {

    public EngageState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Engage State";
        characterState = CHARACTER_STATE.ENGAGE;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        //duration = 288;
        duration = GameManager.ticksPerHour;
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartEngageMovement();
    }
    //protected override void EndState() {
    //    base.EndState();
    //    OnExitThisState();
    //}
    public override void OnExitThisState() {
        stateComponent.character.marker.SetCurrentlyEngaging(null);
        stateComponent.character.marker.SetTargetTransform(null);
        stateComponent.character.marker.pathfindingAI.ClearAllCurrentPathData();
        //stateComponent.character.marker.ClearArrivalAction();
        stateComponent.character.currentParty.icon.SetIsTravelling(false);
        base.OnExitThisState();
    }
    #endregion

    private void StartEngageMovement() {
        stateComponent.character.marker.OnStartEngage(targetCharacter);
    }

    public void CheckForEndState() {
        if (stateComponent.character.marker.GetNearestValidHostile() == null) {
            //can end engage
            OnExitThisState();
        } else {
            //engage another hostile
            stateComponent.character.marker.SetCurrentlyEngaging(null);
            stateComponent.character.marker.SetTargetTransform(null);
            Character hostile = stateComponent.character.marker.GetNearestValidHostile();
            stateComponent.SwitchToState(CHARACTER_STATE.ENGAGE, hostile);
            //stateComponent.character.marker.RedetermineEngage();
        }
    }
    public void CombatOnEngage() {
        Character engagerCharacter = stateComponent.character;
        Character targetCharacter = this.targetCharacter;
        if (CanCombatBeTriggeredBetween(engagerCharacter, targetCharacter)) {
            targetCharacter.marker.SetCannotCombat(true);
            targetCharacter.marker.SetCurrentlyCombatting(engagerCharacter);
            engagerCharacter.marker.SetCurrentlyCombatting(targetCharacter);

            targetCharacter.AdjustIsWaitingForInteraction(1);
            if (targetCharacter.currentAction != null && !targetCharacter.currentAction.isDone) {
                if (targetCharacter.currentParty.icon.isTravelling && targetCharacter.currentParty.icon.travelLine == null) {
                    targetCharacter.marker.StopMovement();
                }
                targetCharacter.StopCurrentAction();
            }
            targetCharacter.AdjustIsWaitingForInteraction(-1);

            engagerCharacter.AdjustIsWaitingForInteraction(1);
            engagerCharacter.StopCurrentAction();
            engagerCharacter.AdjustIsWaitingForInteraction(-1);

            List<Character> attackers = new List<Character>();
            attackers.Add(engagerCharacter);

            List<Character> defenders = new List<Character>();
            defenders.Add(targetCharacter);

            float attackersChance = 0f;
            float defendersChance = 0f;

            CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

            //WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            int chance = UnityEngine.Random.Range(0, 100);

            Debug.Log("Attackers Chance: " + attackersChance + "! Chance: " + chance);

#if TRAILER_BUILD
            if (engagerCharacter.name == "Fiona" && targetCharacter.name == "Jamie") {
                attackersChance = 100f; //fiona should always win
            }
#endif

            if (chance < attackersChance) {
                //Hunter Win
                CombatEncounterEvents(engagerCharacter, targetCharacter, true);
            } else {
                //Target Win
                CombatEncounterEvents(engagerCharacter, targetCharacter, false);
            }
        }
    }

    private void CombatEncounterEvents(Character actor, Character target, bool actorWon) {
        //Reference: https://trello.com/c/uY7JokJn/1573-combat-encounter-event

        Character winner;
        Character loser;
        if (actorWon) {
            winner = actor;
            loser = target;            
        } else {
            winner = target;
            loser = actor;
        }

        //**Character That Won**
        //Gain Combat Recovery trait(won't initiate combat for a while)
        //if(!(actor.stateComponent.previousMajorState != null && actor.stateComponent.previousMajorState.characterState == CHARACTER_STATE.BERSERKED)) {
            winner.AddTrait("Combat Recovery");
        //}

        //**Character That Lost**
        //40 Weight: Gain Unconscious trait (reduce to 0 if already Unconscious)
        //10 Weight: Gain Injured trait and enter Flee mode (reduce to 0 if already Injured)
        //5 Weight: death
        WeightedDictionary<string> loserResults = new WeightedDictionary<string>();
        if (loser.GetNormalTrait("Unconscious") == null) {
            loserResults.AddElement("Unconscious", 40);
        }
        if (loser.GetNormalTrait("Injured") == null) {
            loserResults.AddElement("Injured", 10);
        }
        loserResults.AddElement("Death", 5);

        string result = loserResults.PickRandomElementGivenWeights();

#if TRAILER_BUILD
        if (winner.name == "Fiona" && loser.name == "Jamie") {
            result = "Death"; //always kill jamie
        }
#endif

        switch (result) {
            case "Unconscious":
                Unconscious u = new Unconscious();
                loser.AddTrait(u);
                break;
            case "Injured":
                Injured injured = new Injured();
                loser.AddTrait(injured, winner);
                break;
            case "Death":
                loser.Death("combat");
                break;
            default:
                break;
        }

        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), result);
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddToFillers(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);

        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFrom(log, actor, target);
    }

    private bool CanCombatBeTriggeredBetween(Character engagerCharacter, Character targetCharacter) {
        if (engagerCharacter.GetNormalTrait("Combat Recovery") != null) {
            return false;
        }
        if (engagerCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.FLEE) {
            return false;
        }
        if (engagerCharacter.HasTraitOf(TRAIT_TYPE.DISABLER)) {
            return false;
        }
        if (engagerCharacter.currentAction != null) {
            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return true;
    }
}
