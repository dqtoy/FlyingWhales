﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : CharacterState {

    private int _currentAttackTimer; //When this timer reaches max, remove currently hostile target from hostile list

    public bool isAttacking { get; private set; } //if not attacking, it is assumed that the character is fleeing
    public Character currentClosestHostile { get; private set; }
    private System.Action onEndStateAction; // What should happen when this state ends?

    public CombatState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Combat State";
        characterState = CHARACTER_STATE.COMBAT;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _currentAttackTimer = 0;
        //Default start of combat state is attacking
        isAttacking = true;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartCombatMovement();
    }
    protected override void PerTickInState() {
        if (stateComponent.character.doNotDisturb > 0) {
            if (!(characterState == CHARACTER_STATE.BERSERKED && stateComponent.character.doNotDisturb == 1 && stateComponent.character.GetNormalTrait("Combat Recovery") != null)) {
                StopStatePerTick();
                OnExitThisState();
                return;
            }
        }
    }
    protected override void StartState() {
        stateComponent.character.marker.ShowHPBar();
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Starting combat state for " + stateComponent.character.name);
        base.StartState();
        stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
    }
    protected override void EndState() {
        stateComponent.character.marker.StopCoroutine(CheckIfCurrentHostileIsInRange());
        base.EndState();
        stateComponent.character.marker.HideHPBar();
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Ending combat state for " + stateComponent.character.name);
        onEndStateAction?.Invoke();
    }
    public override void OnExitThisState() {
        stateComponent.character.marker.pathfindingAI.ClearAllCurrentPathData();
        base.OnExitThisState();
    }
    public override void SetOtherDataOnStartState(object otherData) {
        //Notice I didn't call the SetIsAttackingState because I only want to change the value of the boolean, I do not want to process the combat behavior
        if(otherData != null) {
            isAttacking = (bool) otherData;
        }
    }
    #endregion

    private void SetIsAttacking(bool state) {
        isAttacking = state;
        ReevaluateCombatBehavior();
    }
    private void OnArriveAtPickUpLocation() {
        if (stateComponent.character.currentAction == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + stateComponent.character.name + " arrived at pick up location of item during " + stateName + ", but current action is null");
            return;
        }
        stateComponent.character.currentAction.SetEndAction(PatrolAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void PatrolAgain(string result, GoapAction goapAction) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        ResumeState();
    }

    private void StartCombatMovement() {
        string log = GameManager.Instance.TodayLogString() + "Starting combat movement for " + stateComponent.character.name;
        //I set the value to its own because I only want to trigger the movement behavior, I do not want to change the boolean value
        SetIsAttacking(isAttacking);
    }

    //Returns true if there is a hostile left, otherwise, returns false
    public void ReevaluateCombatBehavior() {
        string log = GameManager.Instance.TodayLogString() + "Reevaluating combat behavior of " + stateComponent.character.name;
        if (isAttacking) {
            log += "\n" + stateComponent.character.name + " is attacking!";
            if (currentClosestHostile != null && stateComponent.character.marker.hostilesInRange.Contains(currentClosestHostile)) {
                log += "\nCurrent closest hostile: " + currentClosestHostile.name + " is no longer in hostile list, setting another closest hostile...";
                SetClosestHostile();
            }else if(currentClosestHostile == null) {
                log += "\nNo current closest hostile, setting one...";
                SetClosestHostile();
            }else if(currentClosestHostile != null && stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.currentParty.icon.targetPOI == currentClosestHostile) {
                log += "\nAlready in pursuit of current closest hostile: " + currentClosestHostile.name;
                stateComponent.character.PrintLogIfActive(log);
                return;
            }
            if (currentClosestHostile == null) {
                log += "\nNo more hostile characters, exiting combat state...";
                OnExitThisState();
            } else {
                log += "\nPursuing closest hostile target: " + currentClosestHostile.name;
                PursueClosestHostile();
            }
            stateComponent.character.PrintLogIfActive(log);
        } else {
            //TODO: Flee behavior
            log += "\n" + stateComponent.character.name + " is fleeing!";
            stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.OnStartFlee();
        }
    }

    #region Attacking
    private void PursueClosestHostile() {
        if (!stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.marker.targetPOI != currentClosestHostile) {
            stateComponent.character.marker.GoTo(currentClosestHostile);
        }
    }
    private void SetClosestHostile() {
        currentClosestHostile = stateComponent.character.marker.GetNearestValidHostile();
    }

    //Will be constantly checked every frame
    private IEnumerator CheckIfCurrentHostileIsInRange() {
        string log = GameManager.Instance.TodayLogString() + "Checking if current closest hostile is in range for " + stateComponent.character.name + " to attack...";
        if (currentClosestHostile == null) {
            log += "\nNo current closest hostile, cannot trigger attack...";
            stateComponent.character.PrintLogIfActive(log);
        }
        else if (currentClosestHostile.isDead) {
            log += "\nCurrent closest hostile is dead, removing hostile in hostile list...";
            stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.RemoveHostileInRange(currentClosestHostile);
        }
        else if (currentClosestHostile.specificLocation != stateComponent.character.specificLocation) {
            log += "\nCurrent closest hostile is already in another location or is travelling to one, removing hostile in hostile list...";
            stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.RemoveHostileInRange(currentClosestHostile);
        }
        //If character is attacking and distance is within the attack range of this character, attack
        //else, pursue again
        else if (isAttacking) {
            if (Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.marker.transform.position) <= stateComponent.character.characterClass.attackRange) {
                //&& currentClosestHostile.currentStructure == stateComponent.character.currentStructure //Commented out structure checking first for assault action (Need to discuss)
                Attack();
            } else {
                PursueClosestHostile();
            }
        }

        yield return null;
        if (stateComponent.currentState == this) { //so that if the combat state has been exited, this no longer executes that results in endless execution of this coroutine.
            stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
        }
    }

    private void Attack() {
        //Stop movement first before attacking
        if (stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.currentParty.icon.travelLine == null) {
            stateComponent.character.marker.StopMovement();
        }
        //Check attack speed
        if (!stateComponent.character.marker.CanAttackByAttackSpeed()) {
            //attackSummary += "\nCannot attack yet because of attack speed.";
            //Debug.Log(attackSummary);
            return;
        }
        stateComponent.character.marker.LookAt(currentClosestHostile.marker.transform.position);
        string attackSummary = stateComponent.character.name + " will attack " + currentClosestHostile.name;

        GameManager.Instance.CreateHitEffectAt(currentClosestHostile);
        //TODO: For readjustment, attack power is the old computation
        currentClosestHostile.AdjustHP(-10);//stateComponent.character.attackPower
        attackSummary += "\nDealt damage " + stateComponent.character.attackPower.ToString();

        //Reset Attack Speed
        stateComponent.character.marker.ResetAttackSpeed();

        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if(currentClosestHostile.currentHP <= 0) {
            attackSummary += "\n" + currentClosestHostile.name + "'s hp has reached 0.";
            WeightedDictionary<string> loserResults = new WeightedDictionary<string>();
            if (currentClosestHostile.GetNormalTrait("Unconscious") == null) {
                loserResults.AddElement("Unconscious", 40);
            }
            if (currentClosestHostile.GetNormalTrait("Injured") == null) {
                loserResults.AddElement("Injured", 10);
            }
            loserResults.AddElement("Death", 5);

            string result = loserResults.PickRandomElementGivenWeights();
            attackSummary += "\ncombat result is " + result; ;
            switch (result) {
                case "Unconscious":
                    Unconscious unconscious = new Unconscious();
                    currentClosestHostile.AddTrait(unconscious, stateComponent.character);
                    break;
                case "Injured":
                    Injured injured = new Injured();
                    currentClosestHostile.AddTrait(injured, stateComponent.character);
                    break;
                case "Death":
                    currentClosestHostile.Death(responsibleCharacter: stateComponent.character);
                    break;
            }
        } else {
            attackSummary += "\n" + currentClosestHostile.name + " still has remaining hp " + currentClosestHostile.currentHP.ToString() + "/" + currentClosestHostile.maxHP.ToString();
            //If the enemy still has hp, check if still in range, then process again
            //stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
            if (!currentClosestHostile.marker.hostilesInRange.Contains(stateComponent.character)) {
                currentClosestHostile.marker.AddHostileInRange(stateComponent.character, CHARACTER_STATE.COMBAT); //When the target is hit and it is still alive, add hostile
            }
        }
        Debug.Log(attackSummary);
    }
    #endregion

    #region Flee
    public void FinishedTravellingFleePath() {
        string log = GameManager.Instance.TodayLogString() + "Finished travelling flee path of " + stateComponent.character.name;
        //After travelling flee path, check hostile characters if they are still in vision, every hostile character that is not in vision must be removed form hostile list
        //Consequently, the removed character must also remove this character from his/her hostile list
        //Then check if hostile list is empty
        //If it is, end state immediately
        //If not, flee again
        for (int i = 0; i < stateComponent.character.marker.hostilesInRange.Count; i++) {
            Character currCharacter = stateComponent.character.marker.hostilesInRange[i];
            if (!stateComponent.character.marker.inVisionPOIs.Contains(currCharacter)) {
                stateComponent.character.marker.RemoveHostileInRange(currCharacter, false); //removed hostile because of flee.
                currCharacter.marker.RemoveHostileInRange(stateComponent.character); //removed hostile because of flee.
            }
        }
        if (stateComponent.character.marker.hostilesInRange.Count > 0) {
            //There is still in vision that is hostile, flee again
            log += "\nStill has hostile in vision, fleeing again...";
            stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.OnStartFlee();
        } else {
            log += "\nNo more hostiles, exiting combat state...";
            stateComponent.character.PrintLogIfActive(log);
            OnExitThisState();
        }
    }
    #endregion

    #region Utilities
    public void SetOnEndStateAction(System.Action action) {
        onEndStateAction = action;
    }
    public void ResetClosestHostile() {
        currentClosestHostile = null;
    }
    #endregion
}