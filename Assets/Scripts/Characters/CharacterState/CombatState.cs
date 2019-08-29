using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatState : CharacterState {

    private int _currentAttackTimer; //When this timer reaches max, remove currently hostile target from hostile list
    private bool _hasTimerStarted;

    public bool isAttacking { get; private set; } //if not attacking, it is assumed that the character is fleeing
    public Character currentClosestHostile { get; private set; }
    private System.Action onEndStateAction; // What should happen when this state ends?
    public GoapAction actionThatTriggeredThisState { get; private set; }
    public Character forcedTarget { get; private set; }
    public List<Character> allCharactersThatDegradedRel { get; private set; }

    public CombatState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Combat State";
        characterState = CHARACTER_STATE.COMBAT;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _currentAttackTimer = 0;
        //Default start of combat state is attacking
        isAttacking = true;
        allCharactersThatDegradedRel = new List<Character>();
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
        if (stateComponent.currentState != this) {
            return; //to prevent exiting from this function, when this state was already exited by another funtion in the same stack.
        }
        if (stateComponent.character.doNotDisturb > 0) {
            //if (!(characterState == CHARACTER_STATE.BERSERKED && stateComponent.character.doNotDisturb == 1 && stateComponent.character.GetNormalTrait("Combat Recovery") != null)) {
                StopStatePerTick();
                OnExitThisState();
                return;
            //}
        }
        //if the character is away from home and is at an edge tile, go to home location
        //if (!isAttacking && stateComponent.character.homeArea != null && stateComponent.character.homeArea != stateComponent.character.specificLocation && stateComponent.character.gridTileLocation.IsAtEdgeOfWalkableMap()) {
        //    StopStatePerTick();
        //    OnExitThisState();
        //    //stateComponent.character.PlanIdleReturnHome();
        //    stateComponent.character.currentParty.GoToLocation(stateComponent.character.homeArea, PATHFINDING_MODE.NORMAL, stateComponent.character.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS), null, null, null, null);
        //    return;
        //}
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
                if (stateComponent.character.marker.inVisionCharacters.Contains(currentClosestHostile)) {
                    StartPursueTimer();
                }
            }
        }
    }
    protected override void StartState() {
        stateComponent.character.marker.ShowHPBar();
        stateComponent.character.marker.SetAnimationBool("InCombat", true);
        //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, stateComponent.character, "combat");
        Messenger.AddListener<Character>(Signals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);

        base.StartState();
        if (stateComponent.character.currentAction is AssaultCharacter && !stateComponent.character.currentAction.isPerformingActualAction) {
            stateComponent.character.currentAction.PerformActualAction(); //this is for when a character will assault a target, but his/her attack range is less than his/her vision range. (Because end reached distance of assault action is set to attack range)
        }
        stateComponent.character.StopCurrentAction(false);
        stateComponent.character.currentParty.RemoveAllOtherCharacters(); //Drop characters when entering combat
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Starting combat state for " + stateComponent.character.name);
        stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
    }
    protected override void EndState() {
        stateComponent.character.marker.StopCoroutine(CheckIfCurrentHostileIsInRange());
        base.EndState();
        stateComponent.character.marker.HideHPBar();
        stateComponent.character.marker.SetAnimationBool("InCombat", false);
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
        for (int i = 0; i < stateComponent.character.marker.inVisionCharacters.Count; i++) {
            Character currCharacter = stateComponent.character.marker.inVisionCharacters[i];
            stateComponent.character.CreateJobsOnEnterVisionWith(currCharacter);
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
            if (stateComponent.character.marker.hostilesInRange.Count > 0) {
                summary += "\nStill has hostiles, will attack...";
                stateComponent.character.PrintLogIfActive(summary);
                SetIsAttacking(true);
            } else if (stateComponent.character.marker.avoidInRange.Count > 0) {
                summary += "\nStill has characters to avoid, checking if those characters are still in range...";
                for (int i = 0; i < stateComponent.character.marker.avoidInRange.Count; i++) {
                    Character currCharacter = stateComponent.character.marker.avoidInRange[i];
                    if (!stateComponent.character.marker.inVisionCharacters.Contains(currCharacter) 
                        && !stateComponent.character.marker.visionCollision.poisInRangeButDiffStructure.Contains(currCharacter)) {
                        //I added checking for poisInRangeButDiffStructure beacuse characters are being removed from the character's avoid range when they exit a structure. (Myk)
                        OnFinishedFleeingFrom(currCharacter);
                        stateComponent.character.marker.RemoveAvoidInRange(currCharacter, false);
                        i--;
                    }
                }
                if (stateComponent.character.marker.avoidInRange.Count > 0) {
                    summary += "\nStill has characters to avoid in range, fleeing...";
                    stateComponent.character.PrintLogIfActive(summary);
                    SetIsAttacking(false);
                } else {
                    summary += "\nNo more hostile or avoid characters, exiting combat state...";
                    stateComponent.character.PrintLogIfActive(summary);
                    OnExitThisState();
                }
            } else {
                summary += "\nNo more hostile or avoid characters, exiting combat state...";
                stateComponent.character.PrintLogIfActive(summary);
                OnExitThisState();
            }
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
        //Debug.Log(log);
        //I set the value to its own because I only want to trigger the movement behavior, I do not want to change the boolean value
        //SetIsAttacking(isAttacking);
        DetermineReaction(stateComponent.character);
    }
    public void SwitchTarget(Character newTarget) {
        stateComponent.character.marker.AddHostileInRange(newTarget, processCombatBehavior: false);
        currentClosestHostile = newTarget;
        DoCombatBehavior();
    }
    //Returns true if there is a hostile left, otherwise, returns false
    private void DoCombatBehavior() {
        string log = GameManager.Instance.TodayLogString() + "Reevaluating combat behavior of " + stateComponent.character.name;
        if (isAttacking) {
            log += "\n" + stateComponent.character.name + " is attacking!";
            Trait taunted = stateComponent.character.GetNormalTrait("Taunted");
            if (forcedTarget != null) {
                log += "\n" + stateComponent.character.name + " has a forced target. Setting " + forcedTarget.name + " as target.";
                SetClosestHostile(forcedTarget);
            } else if (taunted != null) {
                log += "\n" + stateComponent.character.name + " is taunted. Setting " + taunted.responsibleCharacter.name + " as target.";
                SetClosestHostile(taunted.responsibleCharacter);
            } else if (currentClosestHostile != null && !stateComponent.character.marker.hostilesInRange.Contains(currentClosestHostile)) {
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
                float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.marker.transform.position);
                if (distance > stateComponent.character.characterClass.attackRange || !stateComponent.character.marker.IsCharacterInLineOfSightWith(currentClosestHostile)) {
                    log += "\nPursuing closest hostile target: " + currentClosestHostile.name;
                    PursueClosestHostile();
                } else {
                    log += "\nAlready within range of: " + currentClosestHostile.name + ". Skipping pursuit...";
                }
            }
            //stateComponent.character.PrintLogIfActive(log);
        } else {
            //Character closestHostile = stateComponent.character.marker.GetNearestValidAvoid();
            if (stateComponent.character.marker.avoidInRange.Count <= 0) {
                log += "\nNo more avoid characters, exiting combat state...";
                stateComponent.character.PrintLogIfActive(log);
                OnExitThisState();
                return;
            }
            if (stateComponent.character.marker.hasFleePath) {
                log += "\nAlready in flee mode";
                stateComponent.character.PrintLogIfActive(log);
                return;
            }
            if (stateComponent.character.marker.pathfindingAI.IsNotAllowedToMove()) {
                log += "\nCannot move, not fleeing";
                stateComponent.character.PrintLogIfActive(log);
                return;
            }
            log += "\n" + stateComponent.character.name + " is fleeing!";
            stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.OnStartFlee();

            Log fleeLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "start_flee");
            fleeLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            stateComponent.character.RegisterLogAndShowNotifToThisCharacterOnly(fleeLog, null, false);
        }
    }

    #region Attacking
    private void PursueClosestHostile() {
        if (!stateComponent.character.currentParty.icon.isTravelling || stateComponent.character.marker.targetPOI != currentClosestHostile) {
            stateComponent.character.marker.GoTo(currentClosestHostile);
        }
    }
    private void SetClosestHostile() {
        Character previousClosestHostile = currentClosestHostile;
        currentClosestHostile = stateComponent.character.marker.GetNearestValidHostile();
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_combat_target");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            stateComponent.character.RegisterLogAndShowNotifToThisCharacterOnly(log, null, false);
        }
    }
    private void SetClosestHostile(Character character) {
        Character previousClosestHostile = currentClosestHostile;
        currentClosestHostile = character;
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_combat_target");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            stateComponent.character.RegisterLogAndShowNotifToThisCharacterOnly(log, null, false);
        }
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
            if (distance <= stateComponent.character.characterClass.attackRange && stateComponent.character.marker.IsCharacterInLineOfSightWith(currentClosestHostile)) { //&& currentClosestHostile.currentStructure == stateComponent.character.currentStructure
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
        if (stateComponent.currentState == this && !isExecutingAttack) { //so that if the combat state has been exited, this no longer executes that results in endless execution of this coroutine.
            stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
        }
    }
    private void Attack() {
        string summary = stateComponent.character.name + " will attack " + currentClosestHostile?.name;

        //When in range and in line of sight, stop movement
        if (stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.currentParty.icon.travelLine == null) {
            stateComponent.character.marker.StopMovement(); //only stop movement if target is also not moving.
            //clear the marker's target poi when it reaches the target, so that the pursue closest hostile will still execute when the other character chooses to flee
            stateComponent.character.marker.SetTargetPOI(null);
        }
        //When the character stops movement, stop pursue timer
        StopPursueTimer();

        //Check attack speed
        if (!stateComponent.character.marker.CanAttackByAttackSpeed()) {
            //float aspeed = stateComponent.character.marker.attackSpeedMeter;
            summary += "\nCannot attack because of attack speed. Waiting...";
            //Debug.Log(summary);
            return;
        }

        summary += "\nExecuting attack...";
        stateComponent.character.FaceTarget(currentClosestHostile);
        stateComponent.character.marker.SetAnimationTrigger("Attack");
        isExecutingAttack = true;
        //Debug.Log(summary);
    }
    public bool isExecutingAttack;
    public void OnAttackHit(Character characterHit) {
        string attackSummary = GameManager.Instance.TodayLogString() + stateComponent.character.name + " hit " + characterHit.name;
        if (characterHit != currentClosestHostile) {
            attackSummary = stateComponent.character.name + " hit " + characterHit.name + " instead of " + currentClosestHostile.name + "!";
        }

        //Reset Attack Speed
        stateComponent.character.marker.ResetAttackSpeed();
        characterHit.OnHitByAttackFrom(stateComponent.character, this, ref attackSummary);

        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if (characterHit.currentHP > 0) {
            attackSummary += "\n" + characterHit.name + " still has remaining hp " + characterHit.currentHP.ToString() + "/" + characterHit.maxHP.ToString();
            //if the character that was hit is not the actual target of this combat, do not make him/her enter combat state
            if (characterHit == currentClosestHostile) {
                //If character that attacked is not invisible or invisible but can be seen by character hit, character hit should react
                Invisible invisible = stateComponent.character.GetNormalTrait("Invisible") as Invisible;
                if (invisible == null || invisible.charactersThatCanSee.Contains(characterHit)) {
                    currentClosestHostile.marker.AddHostileInRange(stateComponent.character, false, isLethal: stateComponent.character.marker.IsLethalCombatForTarget(currentClosestHostile)); //When the target is hit and it is still alive, add hostile
                }
            }
        }
        if (stateComponent.currentState == this) { //so that if the combat state has been exited, this no longer executes that results in endless execution of this coroutine.
            attackSummary += "\n" + stateComponent.character.name + "'s state is still this, running check coroutine.";
            stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
        } else {
            attackSummary += "\n" + stateComponent.character.name + "'s state no longer this, NOT running check coroutine. Current state is" + stateComponent.currentState?.stateName ?? "Null";
        }
        //Debug.Log(attackSummary);
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
        log += "\nFinished travelling flee path, determining action...";
        stateComponent.character.PrintLogIfActive(log);
        DetermineReaction(stateComponent.character);
    }
    public void OnReachLowFleeSpeedThreshold() {
        string log = GameManager.Instance.TodayLogString() + stateComponent.character.name + " has reached low flee speed threshold, determining action...";
        stateComponent.character.PrintLogIfActive(log);
        DetermineReaction(stateComponent.character);
    }
    private void OnFinishedFleeingFrom(Character targetCharacter) {
        if (stateComponent.character.IsHostileWith(targetCharacter)) {
            //if (!targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery")) {
                stateComponent.character.marker.AddTerrifyingObject(targetCharacter);
            //}
        }
        if (stateComponent.character.IsHostileOutsider(targetCharacter)) {
            if (stateComponent.character.role.roleType == CHARACTER_ROLE.LEADER || stateComponent.character.role.roleType == CHARACTER_ROLE.NOBLE || stateComponent.character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                int numOfJobs = 3 - targetCharacter.GetNumOfJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
                if (numOfJobs > 0) {
                    stateComponent.character.CreateLocationKnockoutJobs(targetCharacter, numOfJobs);
                }
            } else {
                if (!(targetCharacter.isDead || (targetCharacter.isAtHomeArea && targetCharacter.isPartOfHomeFaction))) { //|| targetCharacter.HasTraitOf(TRAIT_TYPE.DISABLER, "Combat Recovery")
                    if (stateComponent.character.isAtHomeArea && stateComponent.character.isPartOfHomeFaction) {
                        if (!stateComponent.character.jobQueue.HasJobWithOtherData(JOB_TYPE.REPORT_HOSTILE, targetCharacter)) {
                            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPORT_HOSTILE, INTERACTION_TYPE.REPORT_HOSTILE, new Dictionary<INTERACTION_TYPE, object[]>() {
                                { INTERACTION_TYPE.REPORT_HOSTILE, new object[] { targetCharacter }}
                            });
                            //job.SetCannotOverrideJob(true);
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
    public void SetActionThatTriggeredThisState(GoapAction action) {
        actionThatTriggeredThisState = action;
    }
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
    public void SetForcedTarget(Character character) {
        forcedTarget = character;
        if (forcedTarget == null) {
            stateComponent.character.SetIsFollowingPlayerInstruction(false); //the force target has been removed.
        }
    }
    public void AddCharacterThatDegradedRel(Character character) {
        allCharactersThatDegradedRel.Add(character);
    }
    #endregion
}
