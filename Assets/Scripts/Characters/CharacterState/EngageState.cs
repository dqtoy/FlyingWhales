using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngageState : CharacterState {

    public EngageState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Engage State";
        characterState = CHARACTER_STATE.ENGAGE;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        //duration = 288;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartEngageMovement();
    }
    //protected override void PerTickInState() {
    //    base.PerTickInState();
    //    stateComponent.character.marker.RedetermineEngage();
    //}
    public override void OnExitThisState() {
        stateComponent.character.currentParty.icon.SetIsTravelling(false);
        base.OnExitThisState();
    }
    #endregion

    private void StartEngageMovement() {
        stateComponent.character.marker.OnStartEngage();
    }

    public void CheckForEndState() {
        if (stateComponent.character.marker.hostilesInRange.Count == 0) {
            //can end engage
            OnExitThisState();
        } else {
            //engage another hostile
            Character hostile = stateComponent.character.marker.GetNearestHostile();
            stateComponent.SwitchToState(CHARACTER_STATE.ENGAGE, hostile);
            //stateComponent.character.marker.RedetermineEngage();
        }
    }
    public void CombatOnEngage() {
        Character engagerCharacter = stateComponent.character;
        Character targetCharacter = this.targetCharacter;
        if (CanCombatBeTriggeredBetween(engagerCharacter, targetCharacter)) {
            targetCharacter.marker.SetCannotCombat(true);

            targetCharacter.AdjustIsWaitingForInteraction(1);
            if (targetCharacter.currentAction != null && !targetCharacter.currentAction.isDone) {
                if (!targetCharacter.currentAction.isPerformingActualAction) {
                    targetCharacter.SetCurrentAction(null);
                } else {
                    targetCharacter.currentAction.currentState.EndPerTickEffect();
                }
            }
            targetCharacter.AdjustIsWaitingForInteraction(-1);

            engagerCharacter.AdjustIsWaitingForInteraction(1);
            if (engagerCharacter.currentAction != null && !engagerCharacter.currentAction.isDone) {
                if (!engagerCharacter.currentAction.isPerformingActualAction) {
                    engagerCharacter.SetCurrentAction(null);
                } else {
                    engagerCharacter.currentAction.currentState.EndPerTickEffect();
                }
            }
            engagerCharacter.AdjustIsWaitingForInteraction(-1);

            List<Character> attackers = new List<Character>();
            attackers.Add(engagerCharacter);

            List<Character> defenders = new List<Character>();
            defenders.Add(targetCharacter);

            float attackersChance = 0f;
            float defendersChance = 0f;

            CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < attackersChance) {
                //Hunter Win
                resultWeights.AddElement("Target Killed", 20);
                resultWeights.AddElement("Target Injured", 40);
            } else {
                //Target Win
                resultWeights.AddElement("Hunter Killed", 20);
                resultWeights.AddElement("Hunter Injured", 40);
            }

            string result = resultWeights.PickRandomElementGivenWeights();
            if (result == "Target Killed") {
                targetCharacter.Death();
            } else if (result == "Target Injured") {
                targetCharacter.AddTrait("Injured");
            } else if (result == "Hunter Killed") {
                engagerCharacter.Death();
            } else if (result == "Hunter Injured") {
                engagerCharacter.AddTrait("Injured");
            }
        }
    }
    private bool CanCombatBeTriggeredBetween(Character engagerCharacter, Character targetCharacter) {
        if (engagerCharacter.GetTrait("Combat Recovery") != null) {
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
