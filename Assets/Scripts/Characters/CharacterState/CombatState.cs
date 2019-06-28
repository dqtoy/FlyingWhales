using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : CharacterState {

    private int _currentAttackTimer; //When this timer reaches max, remove currently hostile target from hostile list
    private bool _hasTimerStarted;

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
        if (isPaused) {
            return;
        }
        if (stateComponent.character.doNotDisturb > 0) {
            if (!(characterState == CHARACTER_STATE.BERSERKED && stateComponent.character.doNotDisturb == 1 && stateComponent.character.GetNormalTrait("Combat Recovery") != null)) {
                StopStatePerTick();
                OnExitThisState();
                return;
            }
        }
        //if the character is away from home and is at an edge tile, go to home location
        if (!isAttacking && stateComponent.character.homeArea != null && stateComponent.character.homeArea != stateComponent.character.specificLocation && stateComponent.character.gridTileLocation.IsAtEdgeOfWalkableMap()) {
            StopStatePerTick();
            OnExitThisState();
            stateComponent.character.currentParty.GoToLocation(stateComponent.character.homeArea, PATHFINDING_MODE.NORMAL, stateComponent.character.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS), null, null, null, null);
            return;
        }
        if (_hasTimerStarted) {
            _currentAttackTimer += 1;
            if(_currentAttackTimer >= CombatManager.pursueDuration) {
                StopPursueTimer();
                //When pursue timer reaches max, character must remove the current closest hostile in hostile list, then stop pursue timer
                stateComponent.character.marker.RemoveHostileInRange(currentClosestHostile);
            }
        } else {
            //If character is pursuing the current closest hostile, check if that hostile is in range, if it is, start pursue timer
            if (isAttacking && stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.marker.targetPOI == currentClosestHostile) {
                if (stateComponent.character.marker.inVisionPOIs.Contains(currentClosestHostile)) {
                    StartPursueTimer();
                }
            }
        }
    }
    protected override void StartState() {
        stateComponent.character.marker.ShowHPBar();
        //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, stateComponent.character, "combat");
        stateComponent.character.StopCurrentAction(false);
        stateComponent.character.currentParty.RemoveAllOtherCharacters(); //Drop characters when entering combat
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Starting combat state for " + stateComponent.character.name);
        base.StartState();
        Messenger.AddListener<Character>(Signals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
    }
    protected override void EndState() {
        stateComponent.character.marker.StopCoroutine(CheckIfCurrentHostileIsInRange());
        base.EndState();
        stateComponent.character.marker.HideHPBar();
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Ending combat state for " + stateComponent.character.name);
        onEndStateAction?.Invoke();
        Messenger.RemoveListener<Character>(Signals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    public override void OnExitThisState() {
        stateComponent.character.marker.pathfindingAI.ClearAllCurrentPathData();
        stateComponent.character.marker.SetHasFleePath(false);
        base.OnExitThisState();
    }
    public override void SetOtherDataOnStartState(object otherData) {
        //Notice I didn't call the SetIsAttackingState because I only want to change the value of the boolean, I do not want to process the combat behavior
        if(otherData != null) {
            isAttacking = (bool) otherData;
        }
    }
    public override void AfterExitingState() {
        base.AfterExitingState();
        for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
            if(stateComponent.character.marker.inVisionPOIs[i] is Character) {
                Character currCharacter = stateComponent.character.marker.inVisionPOIs[i] as Character;
                stateComponent.character.CreateJobsOnEnterVisionWith(currCharacter);
            }
        }
    }
    #endregion

    /// <summary>
    /// Function that determines what a character should do in a certain point in time.
    /// Can be triggered by broadcasting signal <see cref="Signals.DETERMINE_COMBAT_REACTION"/>
    /// </summary>
    /// <param name="character">The character that should determine a reaction.</param>
    private void DetermineReaction(Character character) {
        if (stateComponent.character == character) {
            string summary = character.name + " will determine a combat reaction";
            //check flee first, the logic determines that this character will not flee, then attack by default
            bool willAttack = true;
            //- if character is berserked, must not flee
            if (stateComponent.previousMajorState != null && stateComponent.previousMajorState.characterState == CHARACTER_STATE.BERSERKED && !stateComponent.previousMajorState.isDone) {
                willAttack = true;
            }
            //- at some point, situation may trigger the character to flee, at which point it will attempt to move far away from target
            else if (character.GetNormalTrait("Injured") != null) {
                summary += "\n" + character.name + " is injured.";
                //-character gets injured(chance based dependent on the character)
                willAttack = false;
            } else if (character.IsHealthCriticallyLow()) {
                summary += "\n" + character.name + "'s health is critically low.";
                //-character's hp is critically low (chance based dependent on the character)
                willAttack = false;
            } else if (character.GetNormalTrait("Spooked") != null) { //TODO: Ask chy about spooked mechanics
                //- fear-type status effect
                willAttack = false;
            } else if (character.isStarving || character.isExhausted) {
                summary += "\n" + character.name + " is starving(" + character.isStarving.ToString() + ") or is exhausted(" + character.isExhausted.ToString() + ").";
                //-character is starving or exhausted
                willAttack = false;
            }
            summary += "\nDid " + character.name + " chose to attack? " + willAttack.ToString();
            //if (willAttack != isAttacking) {
            summary += "\n" + character.name + " will now change attacking mode to " + willAttack.ToString();
            SetIsAttacking(willAttack); //only execute if there was a change in attacking state
            //} else {
            //    summary += "\n" + character.name + " is already in that mode, continuing mode... ";
            //}
            Debug.Log(summary);
        }
    }

    private void SetIsAttacking(bool state) {
        isAttacking = state;
        if (isAttacking) {
            actionIconString = GoapActionStateDB.Hostile_Icon;
        } else {
            actionIconString = GoapActionStateDB.Flee_Icon;
        }
        stateComponent.character.marker.UpdateActionIcon();
        DoCombatBehavior();
    }
    private void StartCombatMovement() {
        string log = GameManager.Instance.TodayLogString() + "Starting combat movement for " + stateComponent.character.name;
        Debug.Log(log);
        //I set the value to its own because I only want to trigger the movement behavior, I do not want to change the boolean value
        //SetIsAttacking(isAttacking);
        DetermineReaction(stateComponent.character);
    }
    //Returns true if there is a hostile left, otherwise, returns false
    private void DoCombatBehavior() {
        string log = GameManager.Instance.TodayLogString() + "Reevaluating combat behavior of " + stateComponent.character.name;
        if (isAttacking) {
            log += "\n" + stateComponent.character.name + " is attacking!";
            if (currentClosestHostile != null && stateComponent.character.marker.hostilesInRange.Contains(currentClosestHostile)) {
                log += "\nCurrent closest hostile: " + currentClosestHostile.name + " is no longer in hostile list, setting another closest hostile...";
                SetClosestHostile();
            }else if(currentClosestHostile == null) {
                log += "\nNo current closest hostile, setting one...";
                SetClosestHostile();
            }else if(currentClosestHostile != null && stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.marker.targetPOI == currentClosestHostile) {
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
            //stateComponent.character.PrintLogIfActive(log);
        } else {
            Character closestHostile = stateComponent.character.marker.GetNearestValidHostile();
            if (closestHostile == null) {
                log += "\nNo more hostile characters, exiting combat state...";
                stateComponent.character.PrintLogIfActive(log);
                OnExitThisState();
                return;
            }
            if (stateComponent.character.marker.hasFleePath) {
                log += "\nAlready in flee mode";
                stateComponent.character.PrintLogIfActive(log);
                return;
            }
            log += "\n" + stateComponent.character.name + " is fleeing!";
            stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.OnStartFlee();
        }
    }

    #region Attacking
    private void PursueClosestHostile() {
        if (!stateComponent.character.currentParty.icon.isTravelling || stateComponent.character.marker.targetPOI != currentClosestHostile) {
            stateComponent.character.marker.GoTo(currentClosestHostile);
        }
    }
    private void SetClosestHostile() {
        currentClosestHostile = stateComponent.character.marker.GetNearestValidHostile();
    }
    //Will be constantly checked every frame
    private IEnumerator CheckIfCurrentHostileIsInRange() {
        //string log = GameManager.Instance.TodayLogString() + "Checking if current closest hostile is in range for " + stateComponent.character.name + " to attack...";
        if (currentClosestHostile == null) {
            //log += "\nNo current closest hostile, cannot trigger attack...";
            //stateComponent.character.PrintLogIfActive(log);
        }
        else if (currentClosestHostile.isDead) {
            //log += "\nCurrent closest hostile is dead, removing hostile in hostile list...";
            //stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.RemoveHostileInRange(currentClosestHostile);
        }
        else if (currentClosestHostile.specificLocation != stateComponent.character.specificLocation) {
            //log += "\nCurrent closest hostile is already in another location or is travelling to one, removing hostile in hostile list...";
            //stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.RemoveHostileInRange(currentClosestHostile);
        }
        //If character is attacking and distance is within the attack range of this character, attack
        //else, pursue again
        else if (isAttacking) {
            float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.marker.transform.position);
            if (distance <= stateComponent.character.characterClass.attackRange) { //&& currentClosestHostile.currentStructure == stateComponent.character.currentStructure
                //log += "\n" + stateComponent.character.name + " is within range of " + currentClosestHostile.name + ". Attacking...";
                //stateComponent.character.PrintLogIfActive(log);
                //&& currentClosestHostile.currentStructure == stateComponent.character.currentStructure //Commented out structure checking first for assault action (Need to discuss)
                Attack();
            } else {
                //log += "\n" + stateComponent.character.name + " is not in range of " + currentClosestHostile.name + ". Pursuing...";
                //stateComponent.character.PrintLogIfActive(log);
                PursueClosestHostile();
            }
        }

        yield return null;
        if (stateComponent.currentState == this) { //so that if the combat state has been exited, this no longer executes that results in endless execution of this coroutine.
            stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
        }
    }
    private void Attack() {
        //Check attack speed
        if (!stateComponent.character.marker.CanAttackByAttackSpeed()) {
            //attackSummary += "\nCannot attack yet because of attack speed.";
            //Debug.Log(attackSummary);

            //When character is in range but attack speed is still not fully charged, he/she will stop moving only and will wait until the attack speed is charged
            if (stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.currentParty.icon.travelLine == null) {
                stateComponent.character.marker.StopMovement(); //only stop movement if target is also not moving.
                //clear the marker's target poi when it reaches the target, so that the pursue closest hostile will still execute when the other character chooses to flee
                stateComponent.character.marker.SetTargetPOI(null);
            }
            //When the character stops movement, stop pursue timer
            StopPursueTimer();
            return;
        }

        //Check line of sight, if not in line of sight move to it again
        if (!stateComponent.character.marker.IsCharacterInLineOfSightWith(currentClosestHostile)) {
            //PursueClosestHostile();
            return;
        }

        //When character is in range and is in line of sight and attack speed is charged, stop movement so he/she can attack
        //You may notice there are 2 calls for stopping movement, it's because if the character's attack speed is charged but he/she is still not in line of sight, the movement should not stop
        //We only want the movement to stop if the attack speed is not charged or if the attack speed is charged and current hostile is in line of sight
        if (stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.currentParty.icon.travelLine == null) {
            stateComponent.character.marker.StopMovement(); //only stop movement if target is also not moving.
            //clear the marker's target poi when it reaches the target, so that the pursue closest hostile will still execute when the other character chooses to flee
            stateComponent.character.marker.SetTargetPOI(null);
        }
        //When the character stops movement, stop pursue timer
        StopPursueTimer();

        stateComponent.character.FaceTarget(currentClosestHostile);
        stateComponent.character.marker.SetAnimationTrigger("Attack");
    }
    public void OnAttackHit(Character characterHit) {
        string attackSummary = GameManager.Instance.TodayLogString() + stateComponent.character.name + " hit " + characterHit.name;
        if (characterHit != currentClosestHostile) {
            attackSummary = stateComponent.character.name + " hit " + characterHit.name + " instead of " + currentClosestHostile.name + "!";
        }

        //Reset Attack Speed
        stateComponent.character.marker.ResetAttackSpeed();
        characterHit.OnHitByAttackFrom(stateComponent.character, ref attackSummary);

        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if (characterHit.currentHP > 0) {
            attackSummary += "\n" + characterHit.name + " still has remaining hp " + characterHit.currentHP.ToString() + "/" + characterHit.maxHP.ToString();

            //if the character that was hit is not the actual target of this combat, do not make him/her enter combat state
            if (characterHit == currentClosestHostile) {
                if (!currentClosestHostile.marker.hostilesInRange.Contains(stateComponent.character)) {
                    currentClosestHostile.marker.AddHostileInRange(stateComponent.character, CHARACTER_STATE.COMBAT); //When the target is hit and it is still alive, add hostile
                }
            }
        }
        Debug.Log(attackSummary);
    }
    private void StartPursueTimer() {
        if (!_hasTimerStarted) {
            stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Starting pursue timer for " + stateComponent.character.name);
            _currentAttackTimer = 0;
            _hasTimerStarted = true;
        }
    }
    private void StopPursueTimer() {
        if (_hasTimerStarted) {
            stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Stopping pursue timer for " + stateComponent.character.name);
            _hasTimerStarted = false;
        }
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
                OnFinishedFleeingFrom(currCharacter);
                stateComponent.character.marker.RemoveHostileInRange(currCharacter, false); //removed hostile because of flee.
                //currCharacter.marker.RemoveHostileInRange(stateComponent.character); //removed hostile because of flee.
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
    private void OnFinishedFleeingFrom(Character targetCharacter) {
        if (stateComponent.character.IsHostileWith(targetCharacter)) {
            if (!targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery")) {
                stateComponent.character.marker.AddTerrifyingCharacter(targetCharacter);
            }
        }
        if (stateComponent.character.IsHostileOutsider(targetCharacter)) {
            if (stateComponent.character.role.roleType == CHARACTER_ROLE.LEADER || stateComponent.character.role.roleType == CHARACTER_ROLE.NOBLE || stateComponent.character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                int numOfJobs = 3 - targetCharacter.GetNumOfJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
                if (numOfJobs > 0) {
                    stateComponent.character.CreateLocationKnockoutJobs(targetCharacter, numOfJobs);
                }
            } else {
                if (!(targetCharacter.isDead || targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery") || targetCharacter.isAtHomeArea)) {
                    if (stateComponent.character.isAtHomeArea) {
                        if (!stateComponent.character.jobQueue.HasJobWithOtherData(JOB_TYPE.REPORT_HOSTILE, targetCharacter)) {
                            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPORT_HOSTILE, INTERACTION_TYPE.REPORT_HOSTILE, new Dictionary<INTERACTION_TYPE, object[]>() {
                                { INTERACTION_TYPE.REPORT_HOSTILE, new object[] { targetCharacter }}
                            });
                            job.SetCannotOverrideJob(true);
                            job.SetCancelOnFail(true);
                            stateComponent.character.jobQueue.AddJobInQueue(job, false);
                        }
                    }
                }
            }
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
    private void OnGamePaused(bool state) {
        if (state) {
            stateComponent.character.marker.StopCoroutine(CheckIfCurrentHostileIsInRange());
        } else {
            stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
        }
    }
    #endregion
}
