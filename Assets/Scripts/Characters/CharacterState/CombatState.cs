using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : CharacterState {

    private int _currentDuration;

    public bool isAttacking { get; private set; } //if not attacking, it is assumed that the character is fleeing
    public Character currentClosestHostile { get; private set; }

    public CombatState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Combat State";
        characterState = CHARACTER_STATE.COMBAT;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _currentDuration = 0;
        
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartCombatMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
        } else if (stateComponent.character.role.roleType != CHARACTER_ROLE.BEAST && targetPOI is SpecialToken) {
            SpecialToken token = targetPOI as SpecialToken;
            if (token.characterOwner == null) {
                //Patrollers should not pick up items from their warehouse
                if (token.structureLocation != null && token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE
                    && token.specificLocation == stateComponent.character.homeArea) {
                    return false;
                }
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PICK_ITEM, stateComponent.character, targetPOI);
                if (goapAction.targetTile != null) {
                    SetCurrentlyDoingAction(goapAction);
                    goapAction.CreateStates();
                    stateComponent.character.SetCurrentAction(goapAction);
                    stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtPickUpLocation);
                    PauseState();
                } else {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't pick up item " + targetPOI.name + " because there is no tile to go to!");
                }
                return true;
            }
        }
        return base.OnEnterVisionWith(targetPOI);
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
    }
    protected override void EndState() {
        base.EndState();
        stateComponent.character.marker.HideHPBar();
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Ending combat state for " + stateComponent.character.name);
    }
    public override void OnExitThisState() {
        stateComponent.character.marker.pathfindingAI.ClearAllCurrentPathData();
        base.OnExitThisState();
    }
    public override void SetOtherDataOnStartState(object otherData) {
        //Notice I didn't call the SetIsAttackingState because I only want to change the value of the boolean, I do not want to process the movement behavior
        isAttacking = (bool) otherData;
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
            log += "\n" + stateComponent.character.name + " is attacking! Pursuing closest hostile...";
            SetClosestHostile();
            if(currentClosestHostile == null) {
                log += "\nNo more hostile characters, ending combat state...";
                OnExitThisState();
            } else {
                log += "\nClosest hostile target: " + currentClosestHostile.name;
                PursueClosestHostile();
            }
        } else {
            //TODO: Flee behavior
        }
        stateComponent.character.PrintLogIfActive(log);
    }

    #region Attacking
    private void PursueClosestHostile() {
        stateComponent.character.marker.GoTo(currentClosestHostile);
    }
    private void SetClosestHostile() {
        currentClosestHostile = stateComponent.character.marker.GetNearestValidHostile();
    }
    private void CheckIfCurrentHostileIsInRange() {
        string log = GameManager.Instance.TodayLogString() + "Checking if current closest hostile is in range for " + stateComponent.character.name + " to attack...";
        if (currentClosestHostile == null) {
            log += "\nNo current closest hostile, cannot trigger reached end point...";
            stateComponent.character.PrintLogIfActive(log);
            return;
        }
        if (currentClosestHostile.isDead) {
            log += "\nCurrent closest hostile is dead, removing hostile in hostile list...";
            stateComponent.character.PrintLogIfActive(log);
            //TODO: Remove hostile in hostile list. Must also reevaluate movement behavior
            return;
        }
        if (currentClosestHostile.specificLocation != stateComponent.character.specificLocation) {
            log += "\nCurrent closest hostile is already in another location or is travelling to one, removing hostile in hostile list...";
            stateComponent.character.PrintLogIfActive(log);
            //TODO: Remove hostile in hostile list. Must also reevaluate movement behavior
            return;
        }

        //If distance is within the attack range of this character, attack
        //else, pursue again
        if(Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.marker.transform.position) <= stateComponent.character.attackRange) {
            Attack();
        } else {
            PursueClosestHostile();
        }
    }

    private void Attack() {
        //Stop movement first before attacking
        if (stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.currentParty.icon.travelLine == null) {
            stateComponent.character.marker.StopMovement();
        }
        //Check attack speed
        if (!stateComponent.character.marker.CanAttackByAttackSpeed()) {
            return;
        }

        //TODO: For readjustment, attack power is the old computation
        currentClosestHostile.AdjustHP(stateComponent.character.attackPower);

        //Reset Attack Speed
        stateComponent.character.marker.ResetAttackSpeed();

        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if(currentClosestHostile.currentHP <= 0) {
            WeightedDictionary<string> loserResults = new WeightedDictionary<string>();
            if (currentClosestHostile.GetNormalTrait("Unconscious") == null) {
                loserResults.AddElement("Unconscious", 40);
            }
            if (currentClosestHostile.GetNormalTrait("Injured") == null) {
                loserResults.AddElement("Injured", 10);
            }
            loserResults.AddElement("Death", 5);

            string result = loserResults.PickRandomElementGivenWeights();
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
                    currentClosestHostile.Death();
                    break;
            }
        } else {
            //If the enemy still has hp, check if still in range, then process again
            CheckIfCurrentHostileIsInRange();
        }
    }
    #endregion
}
