using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class CombatState : CharacterState {

    private int _currentAttackTimer; //When this timer reaches max, remove currently hostile target from hostile list
    private bool _hasTimerStarted;

    public bool isAttacking { get; private set; } //if not attacking, it is assumed that the character is fleeing
    public IPointOfInterest currentClosestHostile { get; private set; }
    public GoapPlanJob jobThatTriggeredThisState { get; private set; }
    public Character forcedTarget { get; private set; }
    public List<Character> allCharactersThatDegradedRel { get; private set; }

    //Is this character fighting another character or has a character in hostile range list who is trying to apprehend him/her because he/she is a criminal?
    //See: https://trello.com/c/uCZfbCSa/2819-criminals-should-eventually-flee-settlement-and-leave-faction
    public bool isBeingApprehended { get; private set; }


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
            StopStatePerTick();
            OnExitThisState();
            return;
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
                if (stateComponent.character.marker.inVisionPOIs.Contains(currentClosestHostile)) {
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
        //if (stateComponent.character.currentActionNode is Assault && !stateComponent.character.currentActionNode.isPerformingActualAction) {
        //    stateComponent.character.currentActionNode.Perform(); //this is for when a character will assault a target, but his/her attack range is less than his/her vision range. (Because end reached distance of assault action is set to attack range)
        //}
        stateComponent.character.StopCurrentActionNode(false);
        stateComponent.character.currentParty.RemoveAllOtherCharacters(); //Drop characters when entering combat
        if(stateComponent.character is SeducerSummon) { //If succubus/incubus enters a combat, automatically change its faction to the player faction if faction is still disguised
            if(stateComponent.character.faction == FactionManager.Instance.disguisedFaction) {
                stateComponent.character.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
            }
        }
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Starting combat state for " + stateComponent.character.name);
        stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
    }
    protected override void EndState() {
        stateComponent.character.marker.StopCoroutine(CheckIfCurrentHostileIsInRange());
        base.EndState();
        stateComponent.character.marker.HideHPBar();
        stateComponent.character.marker.SetAnimationBool("InCombat", false);
        stateComponent.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + "Ending combat state for " + stateComponent.character.name);
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
        if (!stateComponent.character.isDead) {
            if(isBeingApprehended && stateComponent.character.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL) && !stateComponent.character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
                //If this criminal character is being apprehended and survived (meaning he did not die, or is not unconscious or restrained)
                if (stateComponent.character.faction != FactionManager.Instance.neutralFaction) {
                    //Leave current faction
                    stateComponent.character.ChangeFactionTo(FactionManager.Instance.neutralFaction);
                }

                Region newHomeRegion = GetCriminalNewHomeLocation();
                stateComponent.character.MigrateHomeTo(newHomeRegion);

                string log = GameManager.Instance.TodayLogString() + stateComponent.character.name + " is a criminal and survived being apprehended." +
                    " Changed faction to: " + stateComponent.character.faction.name + " and home to: " + stateComponent.character.homeRegion.name;
                stateComponent.character.PrintLogIfActive(log);

                //stateComponent.character.CancelAllJobsAndPlans();
                //stateComponent.character.PlanIdleReturnHome(true);
                stateComponent.character.defaultCharacterTrait.SetHasSurvivedApprehension(true);
                return;
            }

            //Made it so that dead characters no longer check invision characters after exiting a state.
            for (int i = 0; i < stateComponent.character.marker.inVisionCharacters.Count; i++) {
                Character currCharacter = stateComponent.character.marker.inVisionCharacters[i];
                stateComponent.character.CreateJobsOnEnterVisionWith(currCharacter);
            }
        }
    }
    #endregion

    private Region GetCriminalNewHomeLocation() {
        List<Region> potentialRegions = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            if(stateComponent.character.homeRegion != region && !region.coreTile.isCorrupted) {
                potentialRegions.Add(region);
            }
        }

        if(potentialRegions.Count > 0) {
            return potentialRegions[UnityEngine.Random.Range(0, potentialRegions.Count)];
        } else {
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region region = GridMap.Instance.allRegions[i];
                if (stateComponent.character.homeRegion != region && region != PlayerManager.Instance.player.playerArea.region) {
                    potentialRegions.Add(region);
                }
            }
            return potentialRegions[UnityEngine.Random.Range(0, potentialRegions.Count)];
        }
    }
    /// <summary>
    /// Function that determines what a character should do in a certain point in time.
    /// Can be triggered by broadcasting signal <see cref="Signals.DETERMINE_COMBAT_REACTION"/>
    /// </summary>
    /// <param name="character">The character that should determine a reaction.</param>
    private void DetermineReaction(Character character) {
        DetermineIsBeingApprehended();
        if (stateComponent.character == character) {
            string summary = character.name + " will determine a combat reaction";
            if (stateComponent.character.marker.hostilesInRange.Count > 0) {
                summary += "\nStill has hostiles, will attack...";
                stateComponent.character.PrintLogIfActive(summary);
                SetIsAttacking(true);
            } else if (stateComponent.character.marker.avoidInRange.Count > 0) {
                summary += "\nStill has characters to avoid, checking if those characters are still in range...";
                for (int i = 0; i < stateComponent.character.marker.avoidInRange.Count; i++) {
                    IPointOfInterest currAvoid = stateComponent.character.marker.avoidInRange[i];
                    if (!stateComponent.character.marker.inVisionPOIs.Contains(currAvoid) 
                        && !stateComponent.character.marker.visionCollision.poisInRangeButDiffStructure.Contains(currAvoid)) {
                        //I added checking for poisInRangeButDiffStructure beacuse characters are being removed from the character's avoid range when they exit a structure. (Myk)
                        OnFinishedFleeingFrom(currAvoid);
                        stateComponent.character.marker.RemoveAvoidInRange(currAvoid, false);
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
    //Determine if this character is being apprehended by one of his hostile/avoid in range
    private void DetermineIsBeingApprehended() {
        if (isBeingApprehended) {
            return;
        }
        for (int i = 0; i < stateComponent.character.marker.hostilesInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.character.marker.hostilesInRange[i];
            if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character hostile = poi as Character;
                if (hostile.stateComponent.currentState != null && hostile.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    CombatState combatState = hostile.stateComponent.currentState as CombatState;
                    if (combatState.jobThatTriggeredThisState != null && combatState.jobThatTriggeredThisState.jobType == JOB_TYPE.APPREHEND
                        && combatState.jobThatTriggeredThisState.targetPOI == stateComponent.character) {
                        isBeingApprehended = true;
                        return;
                    }
                }
            }
            
        }
        for (int i = 0; i < stateComponent.character.marker.avoidInRange.Count; i++) {
            if(stateComponent.character.marker.avoidInRange[i].poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                Character hostile = stateComponent.character.marker.avoidInRange[i] as Character;
                if (hostile.stateComponent.currentState != null && hostile.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    CombatState combatState = hostile.stateComponent.currentState as CombatState;
                    if (combatState.jobThatTriggeredThisState != null && combatState.jobThatTriggeredThisState.jobType == JOB_TYPE.APPREHEND
                        && combatState.jobThatTriggeredThisState.targetPOI == stateComponent.character) {
                        isBeingApprehended = true;
                        return;
                    }
                }
            }
        }
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
    private void DoCombatBehavior(Character newTarget = null) {
        string log = GameManager.Instance.TodayLogString() + "Reevaluating combat behavior of " + stateComponent.character.name;
        if (isAttacking) {
            stateComponent.character.marker.StopPerTickFlee();
            log += "\n" + stateComponent.character.name + " is attacking!";
            Trait taunted = stateComponent.character.traitContainer.GetNormalTrait("Taunted");
            if (forcedTarget != null) {
                log += "\n" + stateComponent.character.name + " has a forced target. Setting " + forcedTarget.name + " as target.";
                SetClosestHostile(forcedTarget);
            } else if (taunted != null) {
                log += "\n" + stateComponent.character.name + " is taunted. Setting " + taunted.responsibleCharacter.name + " as target.";
                SetClosestHostile(taunted.responsibleCharacter);
            } else if (currentClosestHostile != null && !stateComponent.character.marker.hostilesInRange.Contains(currentClosestHostile)) {
                log += "\nCurrent closest hostile: " + currentClosestHostile.name + " is no longer in hostile list, setting another closest hostile...";
                SetClosestHostile();
            } else if(currentClosestHostile == null) {
                log += "\nNo current closest hostile, setting one...";
                SetClosestHostile();
            } else {
                log += "\nChecking if the current closest hostile is still the closest hostile, if not, set new closest hostile...";
                IPointOfInterest newClosestHostile =  stateComponent.character.marker.GetNearestValidHostile();
                if(newClosestHostile != null && currentClosestHostile != newClosestHostile) {
                    SetClosestHostile(newClosestHostile);
                } else if (currentClosestHostile != null && stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.marker.targetPOI == currentClosestHostile) {
                    log += "\nAlready in pursuit of current closest hostile: " + currentClosestHostile.name;
                    stateComponent.character.PrintLogIfActive(log);
                    return;
                }
            }
            if (currentClosestHostile == null) {
                log += "\nNo more hostile characters, exiting combat state...";
                OnExitThisState();
            } else {
                float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.worldPosition);
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
            if (stateComponent.character.canMove == false) {
                log += "\nCannot move, not fleeing";
                stateComponent.character.PrintLogIfActive(log);
                return;
            }
            log += "\n" + stateComponent.character.name + " is fleeing!";
            stateComponent.character.PrintLogIfActive(log);
            stateComponent.character.marker.OnStartFlee();


            IPointOfInterest objToAvoid = stateComponent.character.marker.avoidInRange[stateComponent.character.marker.avoidInRange.Count - 1];
            string avoidReason = "unknown";
            if(stateComponent.character.marker.avoidReason != string.Empty) {
                avoidReason = stateComponent.character.marker.avoidReason;
            }
            Log fleeLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "start_flee");
            fleeLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            fleeLog.AddToFillers(objToAvoid, objToAvoid.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            fleeLog.AddToFillers(null, avoidReason, LOG_IDENTIFIER.STRING_1);
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
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = stateComponent.character.marker.GetNearestValidHostile();
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_combat_target");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            stateComponent.character.RegisterLogAndShowNotifToThisCharacterOnly(log, null, false);
        }
    }
    private void SetClosestHostile(IPointOfInterest poi) {
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = poi;
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
            float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.worldPosition);
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
    public void OnAttackHit(IPointOfInterest poi) {
        if (poi == null) {
            return; //NOTE: Sometimes this happens even though the passed value is this character's currentClosestHostile.
        }
        string attackSummary = GameManager.Instance.TodayLogString() + stateComponent.character.name + " hit " + poi.name;
        if (poi != currentClosestHostile) {
            attackSummary = stateComponent.character.name + " hit " + poi.name + " instead of " + currentClosestHostile.name + "!";
        }

        //Reset Attack Speed
        stateComponent.character.marker.ResetAttackSpeed();
        poi.OnHitByAttackFrom(stateComponent.character, this, ref attackSummary);

        //If the hostile reaches 0 hp, evalueate if he/she dies, get knock out, or get injured
        if (poi.currentHP > 0) {
            attackSummary += "\n" + poi.name + " still has remaining hp " + poi.currentHP.ToString() + "/" + poi.maxHP.ToString();
            if (poi is Character) {
                Character hitCharacter = poi as Character;
                //if the character that was hit is not the actual target of this combat, do not make him/her enter combat state
                if (poi == currentClosestHostile) {
                    //When the target is hit and it is still alive, add hostile
                    hitCharacter.marker.AddHostileInRange(stateComponent.character, false, isLethal: stateComponent.character.marker.IsLethalCombatForTarget(hitCharacter));
                    //also add the hit character as degraded rel, so that when the character that owns this state is hit by the other character because of retaliation, relationship degradation will no longer happen
                    //Reference: https://trello.com/c/mvLDnyBf/2875-retaliation-should-not-trigger-relationship-degradation
                    hitCharacter.marker.AddOnProcessCombatAction((combatState) => combatState.AddCharacterThatDegradedRel(stateComponent.character));
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
    private void OnFinishedFleeingFrom(IPointOfInterest fledFrom) {
        if (fledFrom is Character) {
            Character character = fledFrom as Character;
            if (stateComponent.character.IsHostileWith(character)) {
                stateComponent.character.marker.AddTerrifyingObject(fledFrom);
            }
            if (stateComponent.character.IsHostileOutsider(character)) {
                if (stateComponent.character.role.roleType == CHARACTER_ROLE.LEADER || stateComponent.character.role.roleType == CHARACTER_ROLE.NOBLE || stateComponent.character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                    int numOfJobs = 3 - character.GetNumOfJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
                    if (numOfJobs > 0) {
                        stateComponent.character.CreateLocationKnockoutJobs(character, numOfJobs);
                    }
                } else {    
                    if (!(character.isDead || (character.isAtHomeRegion && character.isPartOfHomeFaction))) {
                        if (stateComponent.character.isAtHomeRegion && stateComponent.character.isPartOfHomeFaction) {
                            int numOfJobs = 3 - character.GetNumOfJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
                            if (numOfJobs > 0) {
                                stateComponent.character.CreateLocationKnockoutJobs(character, numOfJobs);
                            }
                            //if (!stateComponent.character.jobQueue.HasJobWithOtherData(JOB_TYPE.REPORT_HOSTILE, fledFrom)) {
                            //    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REPORT_HOSTILE, INTERACTION_TYPE.REPORT_HOSTILE, new Dictionary<INTERACTION_TYPE, object[]>() {
                            //    { INTERACTION_TYPE.REPORT_HOSTILE, new object[] { fledFrom }}
                            //});
                            //    //job.SetCannotOverrideJob(true);
                            //    job.SetCancelOnFail(true);
                            //    stateComponent.character.jobQueue.AddJobInQueue(job, false);
                            //}
                        }
                    }
                }
            }
        }
        
    }
    #endregion

    #region Utilities
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
        if (!allCharactersThatDegradedRel.Contains(character)) {
            allCharactersThatDegradedRel.Add(character);
        }
    }
    public void SetActionThatTriggeredThisState(GoapPlanJob action) {
        jobThatTriggeredThisState = action;
    }
    #endregion
}
