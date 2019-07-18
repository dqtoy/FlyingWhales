using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;
using Pathfinding;
using System.Linq;
using Pathfinding.RVO;

public class CharacterMarker : PooledObject {

    public delegate void HoverMarkerAction(Character character, LocationGridTile location);
    public HoverMarkerAction hoverEnterAction;
    public HoverMarkerAction hoverExitAction;
    public Character character { get; private set; }

    public Transform visualsParent;
    public TextMeshPro nameLbl;
    [SerializeField] private SpriteRenderer mainImg;
    [SerializeField] private SpriteRenderer hoveredImg;
    [SerializeField] private SpriteRenderer clickedImg;
    [SerializeField] private SpriteRenderer actionIcon;

    [Header("Actions")]
    [SerializeField] private StringSpriteDictionary actionIconDictionary;

    [Header("Animation")]
    public Animator animator;

    [Header("Pathfinding")]
    public CharacterAIPath pathfindingAI;    
    public AIDestinationSetter destinationSetter;
    public Seeker seeker;
    [SerializeField] private Collider2D[] colliders;
    [SerializeField] private Rigidbody2D[] rgBodies;
    public CharacterMarkerVisionCollision visionCollision;

    [Header("Combat")]
    public GameObject hpBarGO;
    public Image hpFill;
    //public GameObject aspeedGO;
    public Image aspeedFill;
    public Transform projectileParent;

    [Header("For Testing")]
    [SerializeField] private SpriteRenderer colorHighlight;

    //vision colliders
    public List<IPointOfInterest> inVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<Character> hostilesInRange { get; private set; } //POI's in this characters hostility collider
    public List<Character> avoidInRange { get; private set; } //POI's in this characters hostility collider
    public Action arrivalAction {
        get { return _arrivalAction; }
        private set {
            _arrivalAction = value;
            //if (_arrivalAction == null) {
            //    Debug.Log("Set arrival action of " + character.name + " to null" + " ST: " + StackTraceUtility.ExtractStackTrace());
            //} else {
            //    Debug.Log("Set arrival action of " + character.name + " to " + _arrivalAction.Method.Name + " ST: " + StackTraceUtility.ExtractStackTrace());
            //}
        }
    }

    //movement
    private Action _arrivalAction;
    public IPointOfInterest targetPOI { get; private set; }
    public POICollisionTrigger collisionTrigger { get; private set; }
    public Vector2 anchoredPos { get; private set; }
    public Vector3 centeredWorldPos { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    public bool cannotCombat { get; private set; }
    public float speedModifier { get; private set; }
    public int useWalkSpeed { get; private set; }
    public int targettedByRemoveNegativeTraitActionsCounter { get; private set; }
    public int isStoppedByOtherCharacter { get; private set; } //this is increased, when the action of another character stops this characters movement
    public List<IPointOfInterest> terrifyingObjects { get; private set; } //list of objects that this character is terrified of and must avoid

    private LocationGridTile _previousGridTile;
    private float progressionSpeedMultiplier;
    public float penaltyRadius;
    public bool useCanTraverse;

    public float attackSpeedMeter { get; private set; }
    private AnimatorOverrideController animatorOverrideController; //this is the controller that is made per character

    public void SetCharacter(Character character) {
        this.name = character.name + "'s Marker";
        nameLbl.SetText(character.name);
        this.character = character;
        mainImg.sortingOrder = InteriorMapManager.Default_Character_Sorting_Order + character.id;
        if (UIManager.Instance.characterInfoUI.isShowing) {
            clickedImg.gameObject.SetActive(UIManager.Instance.characterInfoUI.activeCharacter.id == character.id);
        }
        UpdateMarkerVisuals();
        UpdateActionIcon();

        inVisionPOIs = new List<IPointOfInterest>();
        hostilesInRange = new List<Character>();
        terrifyingObjects = new List<IPointOfInterest>();
        avoidInRange = new List<Character>();
        attackSpeedMeter = 0f;

        GameObject collisionTriggerGO = GameObject.Instantiate(InteriorMapManager.Instance.characterCollisionTriggerPrefab, this.transform);
        collisionTriggerGO.transform.localPosition = Vector3.zero;
        collisionTrigger = collisionTriggerGO.GetComponent<POICollisionTrigger>();
        collisionTrigger.Initialize(character);

        //flee
        SetHasFleePath(false);

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.AddListener<Character>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, TransferEngageToFleeList);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);

        PathfindingManager.Instance.AddAgent(pathfindingAI);
        visionCollision.Initialize();
    }

    #region Monobehavior
    private void OnEnable() {
        if (character != null) {
            UpdateAnimation();
        }
    }
    private void Update() {
        if (GameManager.Instance.gameHasStarted && !GameManager.Instance.isPaused) {
            if (attackSpeedMeter < character.attackSpeed) {
                attackSpeedMeter += ((Time.deltaTime * 1000f) * progressionSpeedMultiplier);
                UpdateAttackSpeedMeter();
            }
        }
    }
    #endregion

    #region Pointer Functions
    public void SetHoverAction(HoverMarkerAction hoverEnterAction, HoverMarkerAction hoverExitAction) {
        this.hoverEnterAction = hoverEnterAction;
        this.hoverExitAction = hoverExitAction;
    }
    public void HoverAction() {
        if (hoverEnterAction != null) {
            hoverEnterAction.Invoke(character, character.gridTileLocation);
        }
        //show hovered image
        hoveredImg.gameObject.SetActive(true);
    }
    public void HoverExitAction() {
        if (hoverExitAction != null) {
            hoverExitAction.Invoke(character, character.gridTileLocation);
        }
        //hide hovered image
        hoveredImg.gameObject.SetActive(false);
    }
    public void OnPointerClick(BaseEventData bd) {
        PointerEventData ped = bd as PointerEventData;
        //character.gridTileLocation.OnClickTileActions(ped.button);
        if(ped.button == PointerEventData.InputButton.Left) {
            //This checker is used so that when a character is clicked and it is because there is a player ability that will target that character, the character info ui will not show
            UIManager.Instance.ShowCharacterInfo(character);
        }
#if UNITY_EDITOR
        else if (ped.button == PointerEventData.InputButton.Right) {
            UIManager.Instance.poiTestingUI.ShowUI(character);
        }
#endif
    }
    #endregion

    #region Listeners
    private void OnActionStateSet(GoapAction goapAction, GoapActionState goapState) {
        if (character == goapAction.actor) {
            UpdateAnimation();
        }
    }
    private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
        if (this.character == character) {
            //action icon
            UpdateActionIcon();
            UpdateAnimation();
        } else {
            //crime system:
            //if the other character committed a crime,
            //check if that character is in this characters vision 
            //and that this character can react to a crime (not in flee or engage mode)
            if (action.IsConsideredACrimeBy(this.character)
                && action.CanReactToThisCrime(this.character)
                && inVisionPOIs.Contains(character)
                && this.character.CanReactToCrime()) {
                bool hasRelationshipDegraded = false;
                this.character.ReactToCrime(action, ref hasRelationshipDegraded);
            }
        }
    }
    public void OnCharacterGainedTrait(Character characterThatGainedTrait, Trait trait) {
        //this will make this character flee when he/she gains an injured trait
        if (characterThatGainedTrait == this.character) {
            string gainTraitSummary = characterThatGainedTrait.name + " has gained trait " + trait.name;
            if (trait.type == TRAIT_TYPE.DISABLER) { //if the character gained a disabler trait, hinder movement
                pathfindingAI.ClearAllCurrentPathData();
                pathfindingAI.canSearch = false;
                pathfindingAI.AdjustDoNotMove(1);
                gainTraitSummary += "\nGained trait is a disabler trait, adjusting do not move value.";
            }
            if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
                //if the character gained an unconscious trait, exit current state if it is flee
                if (characterThatGainedTrait.stateComponent.currentState != null && characterThatGainedTrait.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    characterThatGainedTrait.stateComponent.currentState.OnExitThisState();
                    gainTraitSummary += "\nGained trait is unconscious, and characters current state is combat, exiting combat state.";
                }

                //Once a character has a negative disabler trait, clear hostile and avoid list
                ClearHostilesInRange(false);
                ClearAvoidInRange(false);
            }
            UpdateAnimation();
            UpdateActionIcon();
            Debug.Log(gainTraitSummary);
        } else {
            if (inVisionPOIs.Contains(characterThatGainedTrait)) {
                character.CreateJobsOnEnterVisionWith(characterThatGainedTrait);
            }
            if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
                RemoveHostileInRange(characterThatGainedTrait); //removed hostile because he/she became unconscious.
                RemoveAvoidInRange(characterThatGainedTrait);
            }
        }
        if(trait.type == TRAIT_TYPE.DISABLER && terrifyingObjects.Count > 0) {
            RemoveTerrifyingObject(characterThatGainedTrait);
        }
    }
    public void OnCharacterLostTrait(Character character, Trait trait) {
        if (character == this.character) {
            string lostTraitSummary = character.name + " has lost trait " + trait.name;
            if (trait.type == TRAIT_TYPE.DISABLER) { //if the character lost a disabler trait, adjust hinder movement value
                pathfindingAI.AdjustDoNotMove(-1);
                lostTraitSummary += "\nLost trait is a disabler trait, adjusting do not move value.";
            }
            //if the character does not have any other negative disabler trait
            //check for reactions.
            switch (trait.name) {
                case "Unconscious":
                case "Resting":
                    lostTraitSummary += "\n" + character.name + " is checking for reactions towards characters in vision...";
                    for (int i = 0; i < inVisionPOIs.Count; i++) {
                        IPointOfInterest currInVision = inVisionPOIs[i];
                        if (currInVision is Character) {
                            Character currCharacter = currInVision as Character;
                            if (!AddHostileInRange(currCharacter)) {
                                //If not hostile, try to react to character's action
                                character.ThisCharacterSaw(currCharacter);
                            }
                        }
                    }
                    break;
            }
            Debug.Log(lostTraitSummary);
            UpdateAnimation();
            UpdateActionIcon();
        } else if (inVisionPOIs.Contains(character)) {
            //if the character that lost a trait is not this character and that character is in this character's hostility range
            //and the trait that was lost is a negative disabler trait, react to them.
            AddHostileInRange(character);
        }
    }
    /// <summary>
    /// Listener for when a party starts travelling towards another area.
    /// </summary>
    /// <param name="travellingParty">The travelling party.</param>
    private void OnCharacterAreaTravelling(Party travellingParty) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            if (travellingParty.characters.Contains(targetCharacter)) {
                Action action = this.arrivalAction;
                if(action != null) {
                    if (character.currentParty.icon.isTravelling) {
                        if(character.currentParty.icon.travelLine != null) {
                            character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                        } else {
                            StopMovement();
                        }
                    }
                }
                //set arrival action to null, because some arrival actions set it when executed
                ClearArrivalAction();
                action?.Invoke();
            }
        }
        for (int i = 0; i < travellingParty.characters.Count; i++) {
            Character traveller = travellingParty.characters[i];
            if(traveller != character) {
                RemoveHostileInRange(traveller); //removed hostile because he/she left the area.
                RemoveAvoidInRange(traveller);
                RemovePOIFromInVisionRange(traveller);
                visionCollision.RemovePOIAsInRangeButDifferentStructure(traveller);
            }
        }

    }
    private void OnCharacterStartedState(Character character, CharacterState state) {
        if (character == this.character) {
            UpdateActionIcon();
        } else {
            if(state.characterState == CHARACTER_STATE.COMBAT && this.character.GetNormalTrait("Unconscious", "Resting") == null) {
                if (inVisionPOIs.Contains(character)) {
                    this.character.ThisCharacterWatchEvent(character, null, null);
                }
            }
        }
    }
    //private void OnCharacterEndedState(Character character, CharacterState state) {
    //    if (character == this.character) {
    //        UpdateActionIcon();
    //    }
    //}
    #endregion

    #region UI
    private void OnMenuOpened(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            if ((menu as CharacterInfoUI).activeCharacter.id == character.id) {
                clickedImg.gameObject.SetActive(true);
            } else {
                clickedImg.gameObject.SetActive(false);
            }

        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is CharacterInfoUI) {
            clickedImg.gameObject.SetActive(false);
            //UnhighlightMarker();
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED progSpeed) {
        if (progSpeed == PROGRESSION_SPEED.X1) {
            progressionSpeedMultiplier = 1f;
        } else if (progSpeed == PROGRESSION_SPEED.X2) {
            progressionSpeedMultiplier = 1.5f;
        } else if (progSpeed == PROGRESSION_SPEED.X4) {
            progressionSpeedMultiplier = 2f;
        }
        UpdateSpeed();
        UpdateAnimationSpeed();
    }
    #endregion

    #region Action Icon
    public void UpdateActionIcon() {
        if (character == null) {
            return;
        }
        if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) || character.isDead) {
            actionIcon.gameObject.SetActive(false);
            return;
        }
        if (character.isChatting && (character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT)) {
            actionIcon.sprite = actionIconDictionary[GoapActionStateDB.Social_Icon];
            actionIcon.gameObject.SetActive(true);
        } else {
            if (character.stateComponent.currentState != null) {
                if (character.stateComponent.currentState.actionIconString != GoapActionStateDB.No_Icon) {
                    actionIcon.sprite = actionIconDictionary[character.stateComponent.currentState.actionIconString];
                    actionIcon.gameObject.SetActive(true);
                } else {
                    actionIcon.gameObject.SetActive(false);
                }
            }
            //else if (character.targettedByAction.Count > 0) {
            //    if (character.targettedByAction != null && character.targettedByAction[0].actionIconString != GoapActionStateDB.No_Icon) {
            //        actionIcon.sprite = actionIconDictionary[character.targettedByAction[0].actionIconString];
            //        actionIcon.gameObject.SetActive(true);
            //    } else {
            //        actionIcon.gameObject.SetActive(false);
            //    }
            //} 
            else {
                if (character.currentAction != null && character.currentAction.actionIconString != GoapActionStateDB.No_Icon) {
                    actionIcon.sprite = actionIconDictionary[character.currentAction.actionIconString];
                    actionIcon.gameObject.SetActive(true);
                } else {
                    actionIcon.gameObject.SetActive(false);
                }
            }
        }
    }
    private void OnCharacterDoingAction(Character character, GoapAction action) {
        if (this.character == character) {
            UpdateActionIcon();
        }
    }
    public void OnCharacterTargettedByAction(GoapAction action) {
        UpdateActionIcon();
        for (int i = 0; i < action.expectedEffects.Count; i++) {
            if(action.expectedEffects[i].conditionType == GOAP_EFFECT_CONDITION.REMOVE_TRAIT) {
                if(action.expectedEffects[i].conditionKey is string) {
                    string key = (string) action.expectedEffects[i].conditionKey;
                    if(AttributeManager.Instance.allTraits.ContainsKey(key) && AttributeManager.Instance.allTraits[key].effect == TRAIT_EFFECT.NEGATIVE) {
                        AdjustTargettedByRemoveNegativeTraitActions(1);
                    } else if (key == "Negative") {
                        AdjustTargettedByRemoveNegativeTraitActions(1);
                    }
                }
            }
        }
    }
    public void OnCharacterRemovedTargettedByAction(GoapAction action) {
        UpdateActionIcon();
        for (int i = 0; i < action.expectedEffects.Count; i++) {
            if (action.expectedEffects[i].conditionType == GOAP_EFFECT_CONDITION.REMOVE_TRAIT) {
                if (action.expectedEffects[i].conditionKey is string) {
                    string key = (string) action.expectedEffects[i].conditionKey;
                    if (AttributeManager.Instance.allTraits.ContainsKey(key) && AttributeManager.Instance.allTraits[key].effect == TRAIT_EFFECT.NEGATIVE) {
                        AdjustTargettedByRemoveNegativeTraitActions(-1);
                    } else if (key == "Negative") {
                        AdjustTargettedByRemoveNegativeTraitActions(-1);
                    }
                }
            }
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        //StopMovement();
        //if (currentMoveCoroutine != null) {
        //    StopCoroutine(currentMoveCoroutine);
        //}
        //character = null;
        hoverEnterAction = null;
        hoverExitAction = null;
        destinationTile = null;
        PathfindingManager.Instance.RemoveAgent(pathfindingAI);
        //InteriorMapManager.Instance.RemoveAgent(pathfindingAI);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.RemoveListener<Character>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, TransferEngageToFleeList);
        //Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        visionCollision.Reset();
        pathfindingAI.ClearAllCurrentPathData();
        GameObject.Destroy(collisionTrigger.gameObject);
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }

    }
    #endregion

    #region Pathfinding Movement
    public void GoTo(LocationGridTile destinationTile, Action arrivalAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
        //If any time a character goes to a structure outside the trap structure, the trap structure data will be cleared out
        if (character.trapStructure.structure != null && character.trapStructure.structure != destinationTile.structure) {
            character.trapStructure.SetStructureAndDuration(null, 0);
        }
        pathfindingAI.ClearAllCurrentPathData();
        pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.destinationTile = destinationTile;
        this.arrivalAction = arrivalAction;
        this.targetPOI = null;
        if (destinationTile == character.gridTileLocation) {
            //if (this.arrivalAction != null) {
            //    Debug.Log(character.name + " is already at " + destinationTile.ToString() + " executing action " + this.arrivalAction.Method.Name);
            //} else {
            //    Debug.Log(character.name + " is already at " + destinationTile.ToString() + " executing action null.");
            //}
            
            this.arrivalAction?.Invoke();
            ClearArrivalAction();
        } else {
            SetDestination(destinationTile.centeredWorldLocation);
            StartMovement();
        }
        
    }
    public void GoTo(IPointOfInterest targetPOI, Action arrivalAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
        pathfindingAI.ClearAllCurrentPathData();
        pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.arrivalAction = arrivalAction;
        this.targetPOI = targetPOI;
        switch (targetPOI.poiType) {
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                Character targetCharacter = targetPOI as Character;
                SetTargetTransform(targetCharacter.marker.transform);
                //if the target is a character, 
                //check first if he/she is still at the location, 
                if (targetCharacter.specificLocation != character.specificLocation) {
                    //if not, execute the arrival action
                    //if (this.arrivalAction != null) {
                    //    Debug.Log(character.name + "'s target " + targetCharacter.name + " is no longer at " + character.specificLocation.name + " executing action " + this.arrivalAction.Method.Name);
                    //} else {
                    //    Debug.Log(character.name + "'s target " + targetCharacter.name + " is no longer at " + character.specificLocation.name + " executing action null");
                    //}
                    this.arrivalAction?.Invoke();
                    ClearArrivalAction();
                } else if (targetCharacter.currentParty != null && targetCharacter.currentParty.icon != null && targetCharacter.currentParty.icon.isAreaTravelling) {
                    OnCharacterAreaTravelling(targetCharacter.currentParty);
                } 
                break;
            default:
                SetDestination(targetPOI.gridTileLocation.centeredWorldLocation);
                break;
        }

        StartMovement();
    }
    public void GoTo(Vector3 destination, Action arrivalAction = null, STRUCTURE_TYPE[] notAllowedStructures = null) {
        pathfindingAI.ClearAllCurrentPathData();
        pathfindingAI.SetNotAllowedStructures(notAllowedStructures);
        this.destinationTile = destinationTile;
        this.arrivalAction = arrivalAction;
        SetTargetTransform(null);
        SetDestination(destination);
        StartMovement();

    }
    public void ArrivedAtTarget() {
        if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
            if((character.stateComponent.currentState as CombatState).isAttacking){
                return;
            }
        }
        StopMovementOnly();

        
        //if (this.arrivalAction != null) {
        //    Debug.Log(character.name + " arrived at location, executing arrival action " + this.arrivalAction.Method.Name);
        //} else {
        //    Debug.Log(character.name + " arrived at location, executing arrival action None");
        //}

        //if (targetPOI is Character && this.arrivalAction != null) {
        //    //check if target character is actually near the target
        //    if (!character.IsNear(targetPOI)) {
        //        Debug.LogWarning(character.name + " reached " + targetPOI.name + " but they are not near.");
        //        UIManager.Instance.Pause();
        //    }
        //}

        Action action = arrivalAction;
        //set arrival action to null, because some arrival actions set it
        ClearArrivalAction();
        action?.Invoke();

        targetPOI = null;
    }
    private void StartMovement() {
        UpdateSpeed();
        pathfindingAI.SetIsStopMovement(false);
        character.currentParty.icon.SetIsTravelling(true);
        UpdateAnimation();
    }
    public void StopMovement(Action afterStoppingAction = null) {
        StopMovementOnly();
    }
    public void StopMovementOnly() {
        string log = character.name + " StopMovement function is called!";
        character.PrintLogIfActive(log);
        if (character.currentParty.icon != null) {
            character.currentParty.icon.SetIsTravelling(false);
        }
        pathfindingAI.SetIsStopMovement(true);
        UpdateAnimation();
    }
    /// <summary>
    /// Make this marker look at a specific point (In World Space).
    /// </summary>
    /// <param name="target">The target point in world space</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public void LookAt(Vector3 target, bool force = false) {
        if (!force) {
            if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                return;
            }
        }
        
        Vector3 diff = target - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        Rotate(Quaternion.Euler(0f, 0f, rot_z - 90), force);
    }
    /// <summary>
    /// Rotate this marker to a specific angle.
    /// </summary>
    /// <param name="target">The angle this character must rotate to.</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public void Rotate(Quaternion target, bool force = false) {
        if (!force) {
            if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                return;
            }
        }
        visualsParent.rotation = target;
    }
    public void SetDestination(Vector3 destination) {
        pathfindingAI.destination = destination;
        pathfindingAI.canSearch = true;
        //if (!float.IsPositiveInfinity(destination.x)) {
            
        //    //pathfindingAI.SearchPath();
        //} 
        //else {
        //    pathfindingAI.canSearch = false;
        //}
        
    }
    public void SetTargetTransform(Transform target) {
        destinationSetter.target = target;
        pathfindingAI.canSearch = true;
        //if (target != null) {
            
        //    //pathfindingAI.SearchPath();
        //} 
        //else {
        //    pathfindingAI.canSearch = false;
        //}
    }
    public void ClearArrivalAction() {
        arrivalAction = null;
    }
    #endregion

    #region For Testing
    public void HighlightMarker(Color color) {
        colorHighlight.gameObject.SetActive(true);
        colorHighlight.color = color;
    }
    public void UnhighlightMarker() {
        colorHighlight.gameObject.SetActive(false);
    }
    public void HighlightHostilesInRange() {
        for (int i = 0; i < hostilesInRange.Count; i++) {
            hostilesInRange.ElementAt(i).marker.HighlightMarker(Color.red);
        }
    }
    [ContextMenu("Visuals Forward")]
    public void PrintForwardPosition() {
        Debug.Log(visualsParent.up);
    }
    #endregion

    #region Animation
    public void PlayWalkingAnimation() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        PlayAnimation("Walk");
    }
    public void PlayIdle() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        PlayAnimation("Idle");
    }
    private void PlaySleepGround() {
        if (!this.gameObject.activeInHierarchy) {
            return;
        }
        PlayAnimation("Sleep Ground");
    }
    public void PlayAnimation(string animation) {
        //Debug.Log(character.name + " played " + animation + " animation.");
        animator.Play(animation, 0, 0.5f);
        //StartCoroutine(PlayAnimation(animation));
    }
    private void UpdateAnimation() {
        //if (isInCombatTick) {
        //    return;
        //}
        if (!character.IsInOwnParty()) {
            if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                PlaySleepGround();
            }
            return; //if not in own party do not update any other animations
        }
        if (character.isDead) {
            PlayAnimation("Dead");
        } else if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            PlaySleepGround();
        } else if (isStoppedByOtherCharacter > 0) {
            PlayIdle();
        } else if (character.currentParty.icon != null && character.currentParty.icon.isTravelling) {
            //|| character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
            PlayWalkingAnimation();
        } else if (character.currentAction != null && character.currentAction.currentState != null && !string.IsNullOrEmpty(character.currentAction.currentState.animationName)) {
            PlayAnimation(character.currentAction.currentState.animationName);
        } else if (character.currentAction != null && !string.IsNullOrEmpty(character.currentAction.animationName)) {
            PlayAnimation(character.currentAction.animationName);
        } else {
            PlayIdle();
        }
    }
    public void PauseAnimation() {
        animator.speed = 0;
    }
    public void UnpauseAnimation() {
        animator.speed = 1;
    }
    public void SetAnimationTrigger(string triggerName) {
        //Debug.Log("Set animation trigger " + triggerName + " of " + this.name);
        if (triggerName == "Attack" && character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT) {
            return; //because sometime trigger is set even though character is no longer in combat state. TODO: Find out why that is.
        }
        animator.SetTrigger(triggerName);
    }
    public void SetAnimationBool(string name, bool value) {
        animator.SetBool(name, value);
    }
    private void UpdateAnimationSpeed() {
        animator.speed = 1f * progressionSpeedMultiplier;
    }
    #endregion

    #region Utilities
    public void OnMarkerInitiallyPlaced() {
        pathfindingAI.UpdateMe();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = true;
        }
        //Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
    }
    private float GetSpeed() {
        float speed = character.raceSetting.runSpeed;
        if(targettedByRemoveNegativeTraitActionsCounter > 0) {
            speed = character.raceSetting.walkSpeed;
        } else {
            if (useWalkSpeed > 0) {
                speed = character.raceSetting.walkSpeed;
            } else {
                if (character.stateComponent.currentState != null) {
                    if (character.stateComponent.currentState.characterState == CHARACTER_STATE.EXPLORE 
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.PATROL
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE) {
                        //Walk
                        speed = character.raceSetting.walkSpeed;
                    }
                } 
                if (character.currentAction != null) {
                    if (character.currentAction.goapType == INTERACTION_TYPE.RETURN_HOME) {
                        //Walk
                        speed = character.raceSetting.runSpeed;
                    }
                }
            }
        }
        speed += (speed * speedModifier);
        if (speed <= 0f) {
            speed = 0.5f;
        }
        speed *= progressionSpeedMultiplier;
        return speed;
    }
    public void UpdateSpeed() {
        pathfindingAI.speed = GetSpeed();
    }
    public void AdjustSpeedModifier(float amount) {
        speedModifier += amount;
        UpdateSpeed();
    }
    public void AdjustUseWalkSpeed(int amount) {
        useWalkSpeed += amount;
        useWalkSpeed = Mathf.Max(0, useWalkSpeed);
    }
    public void AdjustTargettedByRemoveNegativeTraitActions(int amount) {
        targettedByRemoveNegativeTraitActionsCounter += amount;
        targettedByRemoveNegativeTraitActionsCounter = Mathf.Max(0, targettedByRemoveNegativeTraitActionsCounter);
    }
    public void AdjustIsStoppedByOtherCharacter(int amount) {
        isStoppedByOtherCharacter += amount;
        UpdateAnimation();
    }
    public void SetActiveState(bool state) {
        this.gameObject.SetActive(state);
    }
    /// <summary>
    /// Set the state of all visual aspects of this marker.
    /// </summary>
    /// <param name="state">The state the visuals should be in (active or inactive)</param>
    public void SetVisualState(bool state) {
        mainImg.gameObject.SetActive(state);
        nameLbl.gameObject.SetActive(state);
        actionIcon.enabled = state;
        hoveredImg.enabled = state;
        clickedImg.enabled = state;
    }
    public void UpdateMarkerVisuals() {
        MarkerAsset assets = CharacterManager.Instance.GetMarkerAsset(character.race, character.gender);
        mainImg.sprite = assets.defaultSprite;
        animatorOverrideController = new AnimatorOverrideController(assets.animator);
        //for (int i = 0; i < assets.animator.animationClips.Length; i++) {
        //    AnimationClip currClip = assets.animator.animationClips[i];
        //    animatorOverrideController[currClip.name] = currClip;
        //}
        animatorOverrideController["Idle"] = assets.idle;
        animatorOverrideController["Dead"] = assets.dead;
        animatorOverrideController["Raise Dead"] = assets.raiseDead;
        animatorOverrideController["Sleep Ground"] = assets.sleepGround;
        animatorOverrideController["Walk"] = assets.walk;

        //update attack animations based on class
        AnimationClip attackClip = null;
        if (character.role.roleType == CHARACTER_ROLE.BEAST) {
            attackClip = assets.biteClip;
        } else {
            switch (character.characterClass.rangeType) {
                case RANGE_TYPE.MELEE:
                    attackClip = assets.slashClip;
                    break;
                case RANGE_TYPE.RANGED:
                    if (character.characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                        attackClip = assets.arrowClip;
                    } else {
                        attackClip = assets.magicClip;
                    }
                    break;
                default:
                    attackClip = assets.slashClip;
                    break;
            }
        }
        animatorOverrideController["Attack"] = attackClip;
        animator.runtimeAnimatorController = animatorOverrideController;
    }
    public void UpdatePosition() {
        //This is checked per update, stress test this for performance

        //I'm keeping a separate field called anchoredPos instead of using the rect transform anchoredPosition directly because the multithread cannot access transform components
        anchoredPos = transform.localPosition;

        if (_previousGridTile != character.gridTileLocation) {
            character.specificLocation.areaMap.OnCharacterMovedTo(character, character.gridTileLocation, _previousGridTile);
            _previousGridTile = character.gridTileLocation;
        }
    }
    /// <summary>
    /// Used for placing a character for the first time.
    /// </summary>
    /// <param name="tile">The tile the character should be placed at.</param>
    public void InitialPlaceMarkerAt(LocationGridTile tile) {
        PlaceMarkerAt(tile);
        OnMarkerInitiallyPlaced();
    }
    public void PlaceMarkerAt(LocationGridTile tile, bool addToLocation = true) {
        this.gameObject.transform.SetParent(tile.parentAreaMap.objectsParent);
        pathfindingAI.Teleport(tile.centeredWorldLocation);
        if (addToLocation) {
            tile.structure.location.AddCharacterToLocation(character);
            tile.structure.AddCharacterAtLocation(character, tile);
        }
        SetActiveState(true);
        UpdatePosition();
        UpdateAnimation();
        UpdateActionIcon();
        for (int i = 0; i < colliders.Length; i++) {
            if (!colliders[i].enabled) {
                colliders[i].enabled = true;
            }
        }
    }
    public void PlaceMarkerAt(Vector3 localPos, Vector3 lookAt) {
        StartCoroutine(Positioner(localPos, lookAt));
    }
    public void PlaceMarkerAt(Vector3 localPos, Quaternion lookAt) {
        StartCoroutine(Positioner(localPos, lookAt));
    }
    private IEnumerator Positioner(Vector3 localPos, Vector3 lookAt) {
        yield return null;
        transform.localPosition = localPos;
        LookAt(lookAt, true);
    }
    private IEnumerator Positioner(Vector3 localPos, Quaternion lookAt) {
        yield return null;
        transform.localPosition = localPos;
        Rotate(lookAt, true);
    }
    public void OnDeath(LocationGridTile deathTileLocation) {
        if (character.race == RACE.SKELETON) {
            character.DestroyMarker();
        } else {
            for (int i = 0; i < colliders.Length; i++) {
                colliders[i].enabled = false;
            }
            UpdateAnimation();
            UpdateActionIcon();
            gameObject.transform.SetParent(deathTileLocation.parentAreaMap.objectsParent);
            LocationGridTile placeMarkerAt = deathTileLocation;
            if (deathTileLocation.isOccupied) {
                placeMarkerAt = deathTileLocation.GetNearestUnoccupiedTileFromThis();
            }
            transform.position = placeMarkerAt.centeredWorldLocation;
            ClearHostilesInRange();
            ClearPOIsInVisionRange();
            ClearAvoidInRange();
            visionCollision.OnDeath();
        }
    }
    public void OnReturnToLife() {
        gameObject.SetActive(true);
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = true;
        }
        UpdateAnimation();
        UpdateActionIcon();
    }
    public void UpdateCenteredWorldPos() {
        centeredWorldPos = character.gridTileLocation.centeredWorldLocation;
    }
    public void SetTargetPOI(IPointOfInterest poi) {
        this.targetPOI = poi;
    }
    #endregion

    #region Vision Collision
    public void AddPOIAsInVisionRange(IPointOfInterest poi) {
        if (!inVisionPOIs.Contains(poi)) {
            inVisionPOIs.Add(poi);
            //if (poi is Character) {
            //    //Debug.Log(character.name + " saw " + (poi as Character).name);
            //    Character character = poi as Character;
            //    if (Vector2.Distance(this.transform.position, character.marker.transform.position) > 6f) {
            //        Debug.LogError(this.name + " is trying to add a character to it's vision that is actually far (" + character.marker.name + ")");
            //    }
            //}
            character.AddAwareness(poi);
            OnAddPOIAsInVisionRange(poi);
        }
    }
    public void RemovePOIFromInVisionRange(IPointOfInterest poi) {
        if (inVisionPOIs.Remove(poi)) {
            if (poi is Character) {
                Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_VISION, character, poi as Character);
            }
        }
    }
    public void LogPOIsInVisionRange() {
        string summary = character.name + "'s POIs in range: ";
        for (int i = 0; i < inVisionPOIs.Count; i++) {
            summary += "\n- " + inVisionPOIs.ElementAt(i).ToString();
        }
        Debug.Log(summary);
    }
    public void ClearPOIsInVisionRange() {
        inVisionPOIs.Clear();
    }
    private void OnAddPOIAsInVisionRange(IPointOfInterest poi) {
        if(poi is Character) {
            Character target = poi as Character;
            character.ThisCharacterSaw(target);
        }
    }
    #endregion

    #region Hosility Collision
    public bool AddHostileInRange(Character poi, CHARACTER_STATE forcedReaction = CHARACTER_STATE.NONE, bool checkHostility = true, bool processCombatBehavior = true) {
        if (!hostilesInRange.Contains(poi)) {
            if (character.GetNormalTrait("Zapped") == null && !poi.isDead && !poi.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) &&
                (!checkHostility || forcedReaction != CHARACTER_STATE.NONE || this.character.IsHostileWith(poi))) {
                if (!WillCharacterTransferEngageToFleeList()) {
                    hostilesInRange.Add(poi);
                    //NormalReactToHostileCharacter(poi, forcedReaction);

                    //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
                    if (processCombatBehavior) {
                        if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                            Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                        } else {
                            character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                        }
                    }
                } else {
                    //Transfer to flee list
                    return AddAvoidInRange(poi, processCombatBehavior);
                }
                return true;
            }
        }
        return false;
    }
    public bool AddHostileInRange(Character poi, out CharacterState reaction, CHARACTER_STATE forcedReaction = CHARACTER_STATE.NONE, bool checkHostility = true, bool processCombatBehavior = true) {
        if (AddHostileInRange(poi, forcedReaction, checkHostility, processCombatBehavior)) {
            reaction = character.stateComponent.currentState;
            return true;
        }
        reaction = null;
        return false;
    }
    //public bool AddHostilesInRange(List<Character> pois, CHARACTER_STATE forcedReaction = CHARACTER_STATE.NONE, bool checkHostility = true) {
    //    //Only react to the first hostile that is added
    //    Character otherPOI = null;
    //    for (int i = 0; i < pois.Count; i++) {
    //        Character poi = pois[i];
    //        if (!hostilesInRange.Contains(poi)) {
    //            if (!poi.isDead &&
    //                //if the function states that it should not check the normal hostility, always allow
    //                (!checkHostility
    //                //if forced reaction is not equal to none, it means that this character must treat the other character as hostile, regardless of conditions
    //                || forcedReaction != CHARACTER_STATE.NONE)
    //                || this.character.IsHostileWith(poi)) {
    //                hostilesInRange.Add(poi);
    //                if(otherPOI == null) {
    //                    otherPOI = poi;
    //                }
    //            }
    //        }
    //    }
    //    if(otherPOI != null) {
    //        NormalReactToHostileCharacter(otherPOI, forcedReaction);
    //        ////When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
    //        //if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
    //        //    (character.stateComponent.currentState as CombatState).ReevaluateCombatBehavior();
    //        //} else {
    //        //    character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
    //        //}
    //        return true;
    //    }
    //    return false;
    //}
    public void RemoveHostileInRange(Character poi, bool processCombatBehavior = true) {
        if (hostilesInRange.Remove(poi)) {
            //Debug.Log("Removed hostile in range " + poi.name + " from " + this.character.name);
            //UnhighlightMarker(); //This is for testing only!
            //OnHostileInRangeRemoved(poi);
            string removeHostileSummary = poi.name + " was removed from " + character.name + "'s hostile range.";
            //When removing hostile in range, check if character is still in combat state, if it is, reevaluate combat behavior, if not, do nothing
            if (processCombatBehavior && character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                CombatState combatState = character.stateComponent.currentState as CombatState;
                if (combatState.currentClosestHostile == poi) {
                    combatState.ResetClosestHostile();
                }
                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
            }
            character.PrintLogIfActive(removeHostileSummary);
        }
    }
    public void ClearHostilesInRange(bool processCombatBehavior = true) {
        if(hostilesInRange.Count > 0) {
            hostilesInRange.Clear();
            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                } 
                //else {
                //    character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                //}
            }
        }
    }
    public void OnOtherCharacterDied(Character otherCharacter) {
        if (inVisionPOIs.Contains(otherCharacter)) {
            character.CreateJobsOnEnterVisionWith(otherCharacter); //this is used to create jobs that involve characters that died within the character's range of vision
        }
        RemovePOIFromInVisionRange(otherCharacter);
        visionCollision.RemovePOIAsInRangeButDifferentStructure(otherCharacter);
        //RemoveHostileInRange(otherCharacter);

        //if (this.hasFleePath) { //if this character is fleeing, remove the character that died from his/her hostile list
        //    //this is for cases when this character is fleeing from a character that died because another character assaulted them,
        //    //and so, the character that died was not removed from this character's hostile list
        //    hostilesInRange.Remove(otherCharacter);
        //}
        RemoveHostileInRange(otherCharacter); //removed hostile because he/she died.
        RemoveAvoidInRange(otherCharacter);

        if (targetPOI == otherCharacter) {
            //if (this.arrivalAction != null) {
            //    Debug.Log(otherCharacter.name + " died, executing arrival action " + this.arrivalAction.Method.Name);
            //} else {
            //    Debug.Log(otherCharacter.name + " died, executing arrival action None");
            //}
            //execute the arrival action, the arrival action should handle the cases for when the target is missing
            Action action = arrivalAction;
            //set arrival action to null, because some arrival actions set it when executed
            ClearArrivalAction();
            action?.Invoke();
        }
    }
    #endregion

    #region Avoid In Range
    public bool AddAvoidInRange(Character poi, bool processCombatBehavior = true) {
        if (!poi.isDead && !poi.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) && character.GetNormalTrait("Berserked") == null) {
            if (!avoidInRange.Contains(poi)) {
                avoidInRange.Add(poi);
                //NormalReactToHostileCharacter(poi, CHARACTER_STATE.FLEE);
                //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
                if (processCombatBehavior) {
                    if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                        Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                    } else {
                        character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                    }
                }
                return true;
            }
        }
        return false;
    }
    public bool AddAvoidsInRange(List<Character> pois, bool processCombatBehavior = true) {
        //Only react to the first hostile that is added
        Character otherPOI = null;
        for (int i = 0; i < pois.Count; i++) {
            Character poi = pois[i];
            if (!poi.isDead && !poi.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) && poi.GetNormalTrait("Berserked") == null) {
                if (!avoidInRange.Contains(poi)) {
                    avoidInRange.Add(poi);
                    if (otherPOI == null) {
                        otherPOI = poi;
                    }
                    //return true;
                }
            }
        }
        if (otherPOI != null) {
            //NormalReactToHostileCharacter(otherPOI, CHARACTER_STATE.FLEE);
            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                } else {
                    character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                }
            }
            return true;
        }
        return false;
    }
    public void RemoveAvoidInRange(Character poi, bool processCombatBehavior = true) {
        if (avoidInRange.Remove(poi)) {
            Debug.Log("Removed avoid in range " + poi.name + " from " + this.character.name);
            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                } 
                //else {
                //    character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                //}
            }
        }
    }
    public void ClearAvoidInRange(bool processCombatBehavior = true) {
        if(avoidInRange.Count > 0) {
            avoidInRange.Clear();

            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                } 
                //else {
                //    character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                //}
            }
        }
    }
    #endregion

    #region Reactions
    //private void NormalReactToHostileCharacter(Character otherCharacter, CHARACTER_STATE forcedReaction = CHARACTER_STATE.NONE) {
    //    string summary = character.name + " will react to hostile " + otherCharacter.name;
    //    if (forcedReaction != CHARACTER_STATE.NONE) {
    //        character.stateComponent.SwitchToState(forcedReaction, otherCharacter);
    //        summary += "\n" + character.name + " was forced to " + forcedReaction.ToString() + ".";
    //    } else {

    //        //character.stateComponent.SwitchToState(CHARACTER_STATE.FLEE, otherCharacter);
    //        //return;
    //        //- Determine whether to enter Flee mode or Engage mode:
    //        //if the character will do a combat action towards the other character, do not flee.
    //        if (character.doNotDisturb > 0 && character.HasTraitOf(TRAIT_TYPE.DISABLER)) {
    //            //- Disabled characters will not do anything
    //            summary += "\n" + character.name + " will not do anything.";
    //        } else if (!this.character.IsDoingCombatActionTowards(otherCharacter) && (character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED) && (character.stateComponent.previousMajorState == null || character.stateComponent.previousMajorState.characterState != CHARACTER_STATE.BERSERKED)
    //            && (character.GetNormalTrait("Injured") != null
    //            || character.role.roleType == CHARACTER_ROLE.CIVILIAN
    //            || character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER)) {
    //            //- Injured characters, Civilians, Nobles and Faction Leaders always enter Flee mode
    //            //Check if that character is already in the list of terrifying characters, if it is, do not flee because it will avoid that character already, if not, enter flee mode
    //            //if (!character.marker.terrifyingCharacters.Contains(otherCharacter)) {
    //                character.stateComponent.SwitchToState(CHARACTER_STATE.FLEE, otherCharacter);
    //                summary += "\n" + character.name + " chose to flee.";
    //            //}
    //        } else if (character.role.roleType == CHARACTER_ROLE.BEAST || character.role.roleType == CHARACTER_ROLE.ADVENTURER
    //            || character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.BANDIT 
    //            || (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED)) {
    //            //- Uninjured Beasts, Adventurers and Soldiers will enter Engage mode.
    //            //if (otherCharacter.IsDoingCombatActionTowards(this.character)) {
    //            //    summary += "\n" + otherCharacter.name + " is already or will engage with this character (" + this.character.name + "), waiting for that, instead of starting new engage state.";
    //            //} else
    //            if (this.character.IsDoingCombatActionTowards(otherCharacter)) {
    //                //if the other character is already going to assault this character, and this character chose to engage, wait for the other characters assault instead
    //                summary += "\n" + this.character.name + " is already or will engage with " + otherCharacter.name + ". Not entering engage state.";
    //            } else {
    //                //- A character that is in Flee mode will not trigger combat (but the other side still may)
    //                if (hasFleePath) {
    //                    summary += "\n" + character.name + " is fleeing. Ignoring " + otherCharacter.name;
    //                    Debug.Log(summary);
    //                    return;
    //                }

    //                ////- A character that is performing an Action will not trigger combat (but the other side still may)
    //                //if (character.currentAction != null && character.currentAction.isPerformingActualAction) {
    //                //    summary += "\n" + character.name + " is currently performing" + character.currentAction.goapName + ". Ignoring " + otherCharacter.name;
    //                //    Debug.Log(summary);
    //                //    return;
    //                //}

    //                //- A character in Combat Recovery will not trigger combat (but the other side still may)
    //                //- A character that has a Disabler trait will not trigger combat (but the other side still may)
    //                //since combat recovery is already a disabler trait, only use 1 case here
    //                if (character.HasTraitOf(TRAIT_TYPE.DISABLER)) {
    //                    summary += "\n" + character.name + " has a disabler trait. Ignoring " + otherCharacter.name;
    //                    Debug.Log(summary);
    //                    return;
    //                }

    //                //- If the other character has a Negative Disabler trait, this character will not trigger combat
    //                if (otherCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
    //                    summary += "\n" + otherCharacter.name + " has a negative disabler trait. Ignoring " + otherCharacter.name;
    //                    Debug.Log(summary);
    //                    return;
    //                }

    //                if (this.character.stateComponent.currentState != null &&
    //                    this.character.stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE) {
    //                    //if the character is already in engage mode, check if the other character is nearer than the one that he/she is currently engaging
    //                    Character originalTarget = this.character.stateComponent.currentState.targetCharacter;
    //                    float distanceToOG = Vector2.Distance(this.transform.position, originalTarget.marker.transform.position);
    //                    float distanceToNewTarget = Vector2.Distance(this.transform.position, otherCharacter.marker.transform.position);
    //                    if (distanceToNewTarget < distanceToOG) {
    //                        //if yes, engage the other character instead, 
    //                        character.stateComponent.SwitchToState(CHARACTER_STATE.ENGAGE, otherCharacter);
    //                    } else {
    //                        summary += "\n" + character.name + " is already engaging " + originalTarget.name + " and his/her distance is still nearer than " + otherCharacter.name + ". Keeping " + originalTarget.name + " as engage target.";
    //                        Debug.Log(summary);
    //                        return;
    //                    }
    //                    //else, keep chasing the original target
    //                } else {
    //                    //if the character is not yet in engage mode, engage the new target
    //                    character.stateComponent.SwitchToState(CHARACTER_STATE.ENGAGE, otherCharacter);
    //                }
    //                summary += "\n" + character.name + " chose to engage.";
    //            }
    //        } else if (character.GetNormalTrait("Spooked") != null) {
    //            character.stateComponent.SwitchToState(CHARACTER_STATE.FLEE, otherCharacter);
    //            summary += "\n" + character.name + " is spooked. Chose to flee.";
    //        }
    //    }
    //    if(character.stateComponent.currentState == null || character.stateComponent.currentState.characterState == CHARACTER_STATE.ENGAGE) {
    //        summary += "\n" + character.name + " is engaging, creating assault jobs for the target: " + otherCharacter.name;
    //        int numOfJobs = 3 - otherCharacter.GetNumOfJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
    //        if (numOfJobs > 0) {
    //            character.CreateLocationKnockoutJobs(otherCharacter, numOfJobs);
    //        }
    //    }
    //    Debug.Log(summary);
    //}
    #endregion

    #region Flee
    public bool hasFleePath { get; private set; }
    public void OnStartFlee() {
        if (avoidInRange.Count == 0) {
            return;
        }
        pathfindingAI.ClearAllCurrentPathData();
        SetHasFleePath(true);
        pathfindingAI.canSearch = false; //set to false, because if this is true and a destination has been set in the ai path, the ai will still try and go to that point instead of the computed flee path
        FleeMultiplePath fleePath = FleeMultiplePath.Construct(this.transform.position, avoidInRange.Select(x => x.marker.transform.position).ToArray(), 10000);
        fleePath.aimStrength = 1;
        fleePath.spread = 4000;
        seeker.StartPath(fleePath);
    }
    public void OnFleePathComputed(Path path) {
        if (character == null || character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT 
            || character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return; //this is for cases that the character is no longer in a combat state, but the pathfinding thread returns a flee path
        }
        //Debug.Log(character.name + " computed flee path");
        arrivalAction = OnFinishedTraversingFleePath;
        StartMovement();
    }
    public void OnFinishedTraversingFleePath() {
        //Debug.Log(name + " has finished traversing flee path.");
        SetHasFleePath(false);
        (character.stateComponent.currentState as CombatState).FinishedTravellingFleePath();
        UpdateAnimation();
        UpdateActionIcon();
    }
    public void SetHasFleePath(bool state) {
        hasFleePath = state;
    }
    public void AddTerrifyingObject(IPointOfInterest obj) {
        //terrifyingCharacters += amount;
        //terrifyingCharacters = Math.Max(0, terrifyingCharacters);
        if (!terrifyingObjects.Contains(obj)) {
            terrifyingObjects.Add(obj);
            //rvoController.avoidedAgents.Add(character.marker.fleeingRVOController.rvoAgent);
        }
    }
    public void RemoveTerrifyingObject(IPointOfInterest obj) {
        terrifyingObjects.Remove(obj);
        //if (terrifyingCharacters.Remove(character)) {
        //    //rvoController.avoidedAgents.Remove(character.marker.fleeingRVOController.rvoAgent);
        //}
    }
    public void ClearTerrifyingObjects() {
        terrifyingObjects.Clear();
        //rvoController.avoidedAgents.Clear();
    }
    //private void UpdateFleeingRVOController() {
    //    if (terrifyingCharacters.Count > 0) {
    //        rvoController.collidesWith = RVOLayer.DefaultAgent | RVOLayer.DefaultObstacle | RVOLayer.Layer2;
    //    } else {
    //        rvoController.collidesWith = RVOLayer.DefaultAgent | RVOLayer.DefaultObstacle;
    //    }
    //}
    /// <summary>
    /// Function that determines if the character's hostile list must be transfered to avoid list
    /// Can be triggered by broadcasting signal <see cref="Signals.TRANSFER_ENGAGE_TO_FLEE_LIST"/>
    /// </summary>
    /// <param name="character">The character that should determine the transfer.</param>
    private void TransferEngageToFleeList(Character character) {
        if (this.character == character) {
            string summary = character.name + " will determine the transfer from engage list to flee list";
            if(character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                summary += "\n" + character.name + " has negative disabler trait. Ignoring transfer.";
                //Debug.Log(summary);
                return;
            }
            if (hostilesInRange.Count == 0 && avoidInRange.Count == 0) {
                summary +=  "\n" + character.name + " does not have any characters in engage or avoid list. Ignoring transfer.";
                //Debug.Log(summary);
                return;
            }
            //check flee first, the logic determines that this character will not flee, then attack by default
            bool willTransfer = true;
            if (character.GetNormalTrait("Berserked") != null) {
                willTransfer = false;
            }
            summary += "\nDid " + character.name + " chose to transfer? " + willTransfer.ToString();

            //Transfer all from engage list to flee list
            if (willTransfer) {
                //When transferring to flee list, if the character is not in vision just remove him/her in hostiles range
                for (int i = 0; i < hostilesInRange.Count; i++) {
                    Character hostile = hostilesInRange[i];
                    if (inVisionPOIs.Contains(hostile)) {
                        AddAvoidInRange(hostile, false);
                    } else {
                        RemoveHostileInRange(hostile, false);
                        i--;
                    }
                }
                ClearHostilesInRange(false);
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                } else {
                    if (!character.currentParty.icon.isAreaTravelling) {
                        character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                    }
                }
            }
            Debug.Log(summary);
        }
    }
    #endregion

    #region Engage
    //public Character currentlyEngaging { get; private set; }
    //public Character currentlyCombatting { get; private set; }
    //private string engageSummary;
    //    public void OnStartEngage(Character target) {
    //        //determine nearest hostile in range
    //        //Character nearestHostile = GetNearestValidHostile();
    //        //set them as a target
    //        //SetTargetTransform(nearestHostile.marker.transform);
    //        if (currentlyEngaging != null) {
    //            engageSummary += this.character.name + " has decided to no longer pursue " + currentlyEngaging.name + "\n";
    //        }
    //        engageSummary += this.character.name + " will now engage " + target.name + "\n";

    //        GoTo(target, OnReachEngageTarget);
    //        SetCurrentlyEngaging(target);
    //        //StartMovement();
    //    }
    //    public void OnReachEngageTarget() {
    //        if (currentlyEngaging == null || this.character.isDead) {
    //            return;
    //        }

    //        if (currentlyEngaging.specificLocation != character.specificLocation) {
    //            RemoveHostileInRange(currentlyEngaging); //quick fix for when the target character already is in another location
    //            return;
    //        }

    //        engageSummary += this.character.name + " has reached engage target " + currentlyEngaging.name + "\n";
    //        Character enemy = currentlyEngaging;
    //        LookAt(enemy.marker.transform.position);
    //        //stop the enemy's movement
    //        enemy.marker.pathfindingAI.AdjustDoNotMove(1);

    //#if TRAILER_BUILD
    //        Messenger.AddListener(Signals.TICK_STARTED, CombatTick);
    //        isInCombatTick = true;
    //        //enemy.marker.isInCombatTick = true;
    //        //CombatTick();
    //#else
    //        ExecuteCombat();
    //#endif
    //    }
    //    private const int Fixed_Combat_Ticks = 3;
    //    private int currentCombatTick = 0;
    //    public bool isInCombatTick = false;
    //    public Character lastHitBy;
    //    public void CombatTick() {
    //        if (currentCombatTick < Fixed_Combat_Ticks) {
    //            currentCombatTick++;
    //            Debug.Log(character.name + " hit " + currentlyEngaging?.name);
    //            //Play animation here
    //            GameManager.Instance.CreateHitEffectAt(currentlyEngaging);
    //            PlayAnimation("Attack");
    //            //StartCoroutine(CombatAnimation());
    //        }
    //        if (currentCombatTick == Fixed_Combat_Ticks) {
    //            //Do actual combat here
    //            ExecuteCombat();
    //            Messenger.RemoveListener(Signals.TICK_STARTED, CombatTick);
    //            isInCombatTick = false;
    //        }
    //    }
    //    private void ExecuteCombat() {
    //        Character enemy = currentlyEngaging;
    //        //determine whether to start combat or not
    //        if (cannotCombat) {
    //            cannotCombat = false;
    //            if (character.stateComponent.currentState is EngageState) {
    //                (character.stateComponent.currentState as EngageState).CheckForEndState();
    //            } else {
    //                throw new Exception(character.name + " reached engage target, but not in engage state! CurrentState is " + character.stateComponent.currentState?.stateName ?? "Null");
    //            }
    //        } else {
    //            EngageState engageState = character.stateComponent.currentState as EngageState;
    //            Character thisCharacter = this.character;

    //            //remove enemy's current action
    //            enemy.StopCurrentAction();

    //            engageState.CombatOnEngage();

    //            this.OnFinishCombatWith(enemy);
    //            enemy.marker.OnFinishCombatWith(this.character);
    //        }
    //        enemy.marker.pathfindingAI.AdjustDoNotMove(-1);
    //        Debug.Log(engageSummary);
    //        engageSummary = string.Empty;
    //    }
    //    public void SetCannotCombat(bool state) {
    //        cannotCombat = state;
    //    }
    //public Character GetNearestValidHostile() {
    //    Character nearest = null;
    //    float nearestDist = 9999f;
    //    for (int i = 0; i < hostilesInRange.Count; i++) {
    //        Character currHostile = hostilesInRange.ElementAt(i);
    //        if (IsValidCombatTarget(currHostile)) {
    //            float dist = Vector2.Distance(this.transform.position, currHostile.marker.transform.position);
    //            if (nearest == null || dist < nearestDist) {
    //                nearest = currHostile;
    //                nearestDist = dist;
    //            }
    //        }
    //    }
    //    return nearest;
    //}
    //public Character GetNearestValidAvoid() {
    //    Character nearest = null;
    //    float nearestDist = 9999f;
    //    for (int i = 0; i < avoidInRange.Count; i++) {
    //        Character currAvoid = avoidInRange.ElementAt(i);
    //        if (IsValidCombatTarget(currAvoid)) {
    //            float dist = Vector2.Distance(this.transform.position, currAvoid.marker.transform.position);
    //            if (nearest == null || dist < nearestDist) {
    //                nearest = currAvoid;
    //                nearestDist = dist;
    //            }
    //        }
    //    }
    //    return nearest;
    //}
    //public float GetNearestValidHostileDistance() {
    //    Character nearest = null;
    //    float nearestDist = 9999f;
    //    for (int i = 0; i < hostilesInRange.Count; i++) {
    //        Character currHostile = hostilesInRange.ElementAt(i);
    //        if (IsValidCombatTarget(currHostile)) {
    //            float dist = Vector2.Distance(this.transform.position, currHostile.marker.transform.position);
    //            if (nearest == null || dist < nearestDist) {
    //                nearest = currHostile;
    //                nearestDist = dist;
    //            }
    //        }
    //    }
    //    return nearestDist;
    //}
    //private bool IsValidCombatTarget(Character otherCharacter) {
    //    //- If the other character has a Negative Disabler trait, this character will not trigger combat
    //    return !otherCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
    //}
    //public void SetCurrentlyEngaging(Character character) {
    //    currentlyEngaging = character;
    //    //Debug.Log(GameManager.Instance.TodayLogString() + this.character.name + " set as engaging " + currentlyEngaging?.ToString() ?? "null");
    //}
    //public void SetCurrentlyCombatting(Character character) {
    //    currentlyCombatting = character;
    //    //Debug.Log(GameManager.Instance.TodayLogString() + this.character.name + " set as engaging " + currentlyEngaging?.ToString() ?? "null");
    //}
    ///// <summary>
    ///// This is called after this character finishes a combat encounter with another character
    ///// regardless if he/she started it or not.
    ///// </summary>
    ///// <param name="otherCharacter">The character this character fought with</param>
    //private void OnFinishCombatWith(Character otherCharacter) {
    //    if (!this.character.isDead && currentlyCombatting != null && currentlyCombatting == otherCharacter) {
    //        SetCurrentlyEngaging(null);
    //        SetCurrentlyCombatting(null);
    //        SetTargetTransform(null);
    //        if (otherCharacter.isDead) { //remove hostile character in range, because the listener for character death is only for characters that did not enter combat with the other character
    //            RemoveHostileInRange(otherCharacter);
    //        }
    //    }
    //}
    #endregion

    #region Combat
    public void ShowHPBar() {
        hpBarGO.SetActive(true);
        UpdateHP();
    }
    public void HideHPBar() {
        hpBarGO.SetActive(false);
    }
    public void UpdateHP() {
        if (hpBarGO.activeSelf) {
            hpFill.fillAmount = (float) character.currentHP / character.maxHP;
        }
    }
    public void UpdateAttackSpeedMeter() {
        if (hpBarGO.activeSelf) {
            aspeedFill.fillAmount = attackSpeedMeter / character.attackSpeed;
        }
    }
    public void ResetAttackSpeed() {
        attackSpeedMeter = 0f;
        UpdateAttackSpeedMeter();
    }
    public bool CanAttackByAttackSpeed() {
        return attackSpeedMeter >= character.attackSpeed;
    }
    public Character GetNearestValidHostile() {
        Character nearest = null;
        float nearestDist = 9999f;
        //first check only the hostiles that are in the same area as this character
        for (int i = 0; i < hostilesInRange.Count; i++) {
            Character currHostile = hostilesInRange.ElementAt(i);
            if (IsValidCombatTarget(currHostile)) {

                float dist = Vector2.Distance(this.transform.position, currHostile.marker.transform.position);
                if (nearest == null || dist < nearestDist) {
                    nearest = currHostile;
                    nearestDist = dist;
                }
            }
        }
        //if no character was returned, choose at random from the list, since we are sure that all characters in the list are not in the same area as this character
        if (nearest == null && hostilesInRange.Count > 0) {
            nearest = hostilesInRange[UnityEngine.Random.Range(0, hostilesInRange.Count)];
        }
        return nearest;
    }
    public Character GetNearestValidAvoid() {
        Character nearest = null;
        float nearestDist = 9999f;
        //first check only the hostiles that are in the same area as this character
        for (int i = 0; i < avoidInRange.Count; i++) {
            Character currHostile = avoidInRange.ElementAt(i);
            if (IsValidCombatTarget(currHostile)) {
                float dist = Vector2.Distance(this.transform.position, currHostile.marker.transform.position);
                if (nearest == null || dist < nearestDist) {
                    nearest = currHostile;
                    nearestDist = dist;
                }
            }
        }
        //if no character was returned, choose at random from the list, since we are sure that all characters in the list are not in the same area as this character
        if (nearest == null && avoidInRange.Count > 0) {
            nearest = avoidInRange[UnityEngine.Random.Range(0, avoidInRange.Count)];
        }
        return nearest;
    }
    private bool IsValidCombatTarget(Character otherCharacter) {
        //- If the other character has a Negative Disabler trait, this character will not trigger combat
        return !otherCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
    }
    public bool IsCharacterInLineOfSightWith(Character targetCharacter) {
        //return targetCharacter.currentStructure == character.currentStructure;
        //precompute our ray settings
        Vector3 start = transform.position;
        Vector3 direction = targetCharacter.marker.transform.position - transform.position;

        //draw the ray in the editor
        //Debug.DrawRay(start, direction * 10f, Color.red, 1000f);

        //do the ray test
        RaycastHit2D[] hitObjects = Physics2D.RaycastAll(start, direction, 10f);
        for (int i = 0; i < hitObjects.Length; i++) {
            RaycastHit2D hit = hitObjects[i];
            if (hit.collider != null) {
                if(hit.collider.gameObject.name == "Structure_Tilemap" || hit.collider.gameObject.name == "Walls_Tilemap") {
                    return false;
                } else {
                    CharacterCollisionTrigger collisionTrigger = hit.collider.gameObject.GetComponent<CharacterCollisionTrigger>();
                    if (collisionTrigger != null && collisionTrigger.poi == targetCharacter) {
                        return true;
                    }
                }
                //Debug.LogWarning(character.name + " collided with: " + hit.collider.gameObject.name);
            }
        }
        return false;
    }
    public bool WillCharacterTransferEngageToFleeList() {
        bool willTransfer = false;
        //- if character is berserked, must not flee
        if (character.GetNormalTrait("Berserked") != null) {
            willTransfer = false;
        }
        //- at some point, situation may trigger the character to flee, at which point it will attempt to move far away from target
        else if (character.GetNormalTrait("Injured") != null) {
            //summary += "\n" + character.name + " is injured.";
            //-character gets injured(chance based dependent on the character)
            willTransfer = true;
        } else if (character.IsHealthCriticallyLow()) {
            //summary += "\n" + character.name + "'s health is critically low.";
            //-character's hp is critically low (chance based dependent on the character)
            willTransfer = true;
        } else if (character.GetNormalTrait("Spooked") != null) { //TODO: Ask chy about spooked mechanics
                                                                  //- fear-type status effect
            willTransfer = true;
        } else if (character.isStarving && character.GetNormalTrait("Vampiric") == null) {
            //-character is starving and is not a vampire
            willTransfer = true;
        } else if (character.isExhausted) {
            //-character is exhausted
            willTransfer = true;
        }
        return willTransfer;
    }
    #endregion
}
