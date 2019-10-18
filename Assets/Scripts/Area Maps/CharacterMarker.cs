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
    [SerializeField] private SpriteRenderer berserkedOutline;

    [Header("Actions")]
    [SerializeField] private StringSpriteDictionary actionIconDictionary;

    [Header("Animation")]
    public Animator animator;
    [SerializeField] private CharacterMarkerAnimationListener animationListener;

    [Header("Pathfinding")]
    public CharacterAIPath pathfindingAI;    
    public AIDestinationSetter destinationSetter;
    public Seeker seeker;
    public Collider2D[] colliders;
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
    public List<IPointOfInterest> unprocessedVisionPOIs { get; private set; } //POI's in this characters vision collider
    public List<Character> inVisionCharacters { get; private set; } //POI's in this characters vision collider
    public List<IPointOfInterest> hostilesInRange { get; private set; } //POI's in this characters hostility collider
    public List<IPointOfInterest> avoidInRange { get; private set; } //POI's in this characters hostility collider
    public List<GoapAction> actionsToWitness { get; private set; } //List of actions this character can witness, and has not been processed yet. Will be cleared after processing
    public Dictionary<Character, bool> lethalCharacters { get; private set; }
    public bool willProcessCombat { get; private set; }
    public Action arrivalAction {
        get { return _arrivalAction; }
        private set {
            _arrivalAction = value;
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
    public int useWalkSpeed { get; private set; }
    public int targettedByRemoveNegativeTraitActionsCounter { get; private set; }
    public List<IPointOfInterest> terrifyingObjects { get; private set; } //list of objects that this character is terrified of and must avoid
    public bool isMoving { get; private set; }

    private LocationGridTile _previousGridTile;
    private float progressionSpeedMultiplier;
    public float penaltyRadius;
    public bool useCanTraverse;

    public float attackSpeedMeter { get; private set; }
    private AnimatorOverrideController animatorOverrideController; //this is the controller that is made per character
    public float attackExecutedTime { get; private set; } //how long into the attack animation is this character's attack actually executed.

    public void SetCharacter(Character character) {
        this.name = character.name + "'s Marker";
        nameLbl.SetText(character.name);
        this.character = character;
        mainImg.sortingOrder = InteriorMapManager.Default_Character_Sorting_Order + character.id;
        nameLbl.sortingOrder = mainImg.sortingOrder;
        actionIcon.sortingOrder = mainImg.sortingOrder;
        hoveredImg.sortingOrder = mainImg.sortingOrder - 1;
        clickedImg.sortingOrder = mainImg.sortingOrder - 1;
        colorHighlight.sortingOrder = mainImg.sortingOrder - 1;
        berserkedOutline.sortingOrder = mainImg.sortingOrder + 1;
        hpBarGO.GetComponent<Canvas>().sortingOrder = mainImg.sortingOrder;
        if (UIManager.Instance.characterInfoUI.isShowing) {
            clickedImg.gameObject.SetActive(UIManager.Instance.characterInfoUI.activeCharacter.id == character.id);
        }
        UpdateMarkerVisuals();
        UpdateActionIcon();

        unprocessedVisionPOIs = new List<IPointOfInterest>();
        inVisionPOIs = new List<IPointOfInterest>();
        inVisionCharacters = new List<Character>();
        hostilesInRange = new List<IPointOfInterest>();
        terrifyingObjects = new List<IPointOfInterest>();
        avoidInRange = new List<IPointOfInterest>();
        lethalCharacters = new Dictionary<Character, bool>();
        actionsToWitness = new List<GoapAction>();
        attackSpeedMeter = 0f;
        OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
        //flee
        SetHasFleePath(false);

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        //Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        ///Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction); Moved  listener for action state set to CharacterManager for optimization <see cref="CharacterManager.OnCharacterFinishedAction(Character, GoapAction, string)"/>
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        ///Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet); Moved listener for action state set to CharacterManager for optimization <see cref="CharacterManager.OnActionStateSet(GoapAction, GoapActionState)">
        Messenger.AddListener<Character>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, TransferEngageToFleeList);
        Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.AddListener(Signals.TICK_ENDED, ProcessAllUnprocessedVisionPOIs);

        Messenger.AddListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);

        PathfindingManager.Instance.AddAgent(pathfindingAI);
        //visionCollision.Initialize();
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
    ///Moved listener for action state set to CharacterManager for optimization <see cref="CharacterManager.OnActionStateSet(GoapAction, GoapActionState)">
    //private void OnActionStateSet(GoapAction goapAction, GoapActionState goapState) {
    //    if (character == goapAction.actor) {
    //        UpdateAnimation();
    //    }
    //}

    ///Moved  listener for action state set to CharacterManager for optimization <see cref="CharacterManager.OnCharacterFinishedAction(Character, GoapAction, string)"/>
    //private void OnCharacterFinishedAction(Character character, GoapAction action, string result) {
    //    if (this.character == character) {
    //        //action icon
    //        UpdateActionIcon();
    //        UpdateAnimation();
    //    } else {
    //        //crime system:
    //        //if the other character committed a crime,
    //        //check if that character is in this characters vision 
    //        //and that this character can react to a crime (not in flee or engage mode)
    //        if (inVisionCharacters.Contains(character)
    //            && action.IsConsideredACrimeBy(this.character)
    //            && action.CanReactToThisCrime(this.character)
    //            && this.character.CanReactToCrime()) {
    //            bool hasRelationshipDegraded = false;
    //            this.character.ReactToCrime(action, ref hasRelationshipDegraded);
    //        }
    //    }
    //}
    public void OnCharacterGainedTrait(Character characterThatGainedTrait, Trait trait) {
        //this will make this character flee when he/she gains an injured trait
        if (characterThatGainedTrait == this.character) {
            SelfGainedTrait(characterThatGainedTrait, trait);
        } else {
            OtherCharacterGainedTrait(characterThatGainedTrait, trait);
        }
        if(trait.type == TRAIT_TYPE.DISABLER && terrifyingObjects.Count > 0) {
            RemoveTerrifyingObject(characterThatGainedTrait);
        }
    }
    public void OnCharacterLostTrait(Character character, Trait trait) {
        if (character == this.character) {
            string lostTraitSummary = GameManager.Instance.TodayLogString() + character.name + " has lost trait " + trait.name;
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
                    for (int i = 0; i < inVisionCharacters.Count; i++) {
                        Character currCharacter = inVisionCharacters[i];
                        if (!AddHostileInRange(currCharacter)) {
                            //If not hostile, try to react to character's action
                            character.ThisCharacterSaw(currCharacter);
                        }
                    }
                    break;
            }
            character.PrintLogIfActive(lostTraitSummary);
            UpdateAnimation();
            UpdateActionIcon();
        } else if (inVisionCharacters.Contains(character)) {
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
    private void SelfGainedTrait(Character characterThatGainedTrait, Trait trait) {
        string gainTraitSummary = GameManager.Instance.TodayLogString() + characterThatGainedTrait.name + " has gained trait " + trait.name;
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
        character.PrintLogIfActive(gainTraitSummary);
    }
    private void OtherCharacterGainedTrait(Character otherCharacter, Trait trait) {
        if (trait.name == "Invisible") {
            RemoveHostileInRange(otherCharacter);
            RemoveAvoidInRange(otherCharacter);
            RemovePOIFromInVisionRange(otherCharacter);
            //if (character.currentAction != null && character.currentAction.poiTarget == otherCharacter) {
            //    //If current action target is invisible and it is moving towards target stop it
            //    character.currentAction.StopAction(true);
            //}
        } else {
            //if (inVisionCharacters.Contains(otherCharacter)) {
            //    character.CreateJobsOnEnterVisionWith(otherCharacter);
            //}
            if (inVisionCharacters.Contains(otherCharacter)) {
                character.CreateJobsOnTargetGainTrait(otherCharacter, trait);
            }
            if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
                RemoveHostileInRange(otherCharacter); //removed hostile because he/she became unconscious.
                RemoveAvoidInRange(otherCharacter);
            }
        }
    }
    private void OnItemRemovedFromTile(SpecialToken token, LocationGridTile removedFrom) {
        if (hostilesInRange.Contains(token)) {
            RemoveHostileInRange(token);
        }
    }
    private void OnTileObjectRemovedFromTile(TileObject obj, Character removedBy, LocationGridTile removedFrom) {
        if (hostilesInRange.Contains(obj)) {
            RemoveHostileInRange(obj);
        }
    }
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
        int negativeDisablerCount = character.GetNumberOfTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
        if ((negativeDisablerCount >= 2 || (negativeDisablerCount == 1 && character.GetNormalTrait("Paralyzed") == null)) || character.isDead) {
            actionIcon.gameObject.SetActive(false);
            return;
        }
        if (character.isChatting && (character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT)) {
            if (character.isFlirting) {
                actionIcon.sprite = actionIconDictionary[GoapActionStateDB.Flirt_Icon];
            } else {
                actionIcon.sprite = actionIconDictionary[GoapActionStateDB.Social_Icon];
            }
            actionIcon.gameObject.SetActive(true);
        } else {
            if (character.currentAction != null) {
                if (character.currentAction.actionIconString != GoapActionStateDB.No_Icon) {
                    actionIcon.sprite = actionIconDictionary[character.currentAction.actionIconString];
                    actionIcon.gameObject.SetActive(true);
                } else {
                    actionIcon.gameObject.SetActive(false);
                }
            } else if (character.stateComponent.currentState != null) {
                if (character.stateComponent.currentState.actionIconString != GoapActionStateDB.No_Icon) {
                    actionIcon.sprite = actionIconDictionary[character.stateComponent.currentState.actionIconString];
                    actionIcon.gameObject.SetActive(true);
                } else {
                    actionIcon.gameObject.SetActive(false);
                }
            } else if (hasFleePath) {
                actionIcon.sprite = actionIconDictionary[GoapActionStateDB.Flee_Icon];
                actionIcon.gameObject.SetActive(true);
            } else {
                //no action or state
                actionIcon.gameObject.SetActive(false);
            }
        }
    }
    public void OnThisCharacterDoingAction(GoapAction action) {
         UpdateActionIcon();
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
        //Debug.Log(GameManager.Instance.TodayLogString() + "reset marker: " + name);
        hoverEnterAction = null;
        hoverExitAction = null;
        destinationTile = null;
        fleeSpeedModifier = 0;
        SetMarkerColor(Color.white);
        actionIcon.gameObject.SetActive(false);
        StopPerTickFlee();
        PathfindingManager.Instance.RemoveAgent(pathfindingAI);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        //Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        //Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_REMOVED, OnCharacterLostTrait);
        //Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.RemoveListener<Character>(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, TransferEngageToFleeList);
        Messenger.RemoveListener(Signals.TICK_ENDED, ProcessAllUnprocessedVisionPOIs);
        Messenger.RemoveListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);

        visionCollision.Reset();
        GameObject.Destroy(collisionTrigger.gameObject);
        collisionTrigger = null;
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }
        pathfindingAI.ResetThis();
        character = null;
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
            Action action = this.arrivalAction;
            ClearArrivalAction();
            action?.Invoke();
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
                if (targetCharacter.marker == null) {
                    this.arrivalAction?.Invoke(); //target character is already dead.
                    ClearArrivalAction();
                    return;
                }
                SetTargetTransform(targetCharacter.marker.transform);
                //if the target is a character, 
                //check first if he/she is still at the location, 
                if (targetCharacter.specificLocation != character.specificLocation) {
                    this.arrivalAction?.Invoke();
                    ClearArrivalAction();
                } else if (targetCharacter.currentParty != null && targetCharacter.currentParty.icon != null && targetCharacter.currentParty.icon.isTravellingOutside) {
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
        StopMovement();
        Action action = arrivalAction;
        //set arrival action to null, because some arrival actions set it
        ClearArrivalAction();
        action?.Invoke();

        targetPOI = null;
    }
    private void StartMovement() {
        isMoving = true;
        UpdateSpeed();
        pathfindingAI.SetIsStopMovement(false);
        character.currentParty.icon.SetIsTravelling(true);
        UpdateAnimation();
        Messenger.AddListener(Signals.TICK_ENDED, PerTickMovement);
        //Messenger.Broadcast(Signals.CHARACTER_STARTED_MOVING, character);
    }
    public void StopMovement() {
        isMoving = false;
        string log = character.name + " StopMovement function is called!";
        character.PrintLogIfActive(log);
        if (character.currentParty.icon != null) {
            character.currentParty.icon.SetIsTravelling(false);
        }
        pathfindingAI.SetIsStopMovement(true);
        UpdateAnimation();
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickMovement);
        //Messenger.Broadcast(Signals.CHARACTER_STOPPED_MOVING, character);
    }
    private void PerTickMovement() {
        if (character == null) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickMovement);
            return;
        }
        character.PerTickDuringMovement();
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
    public void BerserkedMarker() {
        berserkedOutline.gameObject.SetActive(true);
    }
    public void UnberserkedMarker() {
        berserkedOutline.gameObject.SetActive(false);
    }
    public void HighlightMarker(Color color) {
        colorHighlight.gameObject.SetActive(true);
        colorHighlight.color = color;
    }
    public void UnhighlightMarker() {
        colorHighlight.gameObject.SetActive(false);
    }
    [ContextMenu("Visuals Forward")]
    public void PrintForwardPosition() {
        Debug.Log(visualsParent.up);
    }
    [Header("For Testing")]
    [SerializeField] private string targetCharacterName;
    [ContextMenu("Check If Path Possible")]
    private void CheckIfPathIsPossible() {
        Character targetCharacter = CharacterManager.Instance.GetCharacterByName(targetCharacterName);
        NNConstraint nodeConstraint = new NNConstraint();
        nodeConstraint.constrainDistance = false;
        GraphNode node1 = this.character.gridTileLocation.parentAreaMap.pathfindingGraph.GetNearest(this.transform.position, nodeConstraint).node;
        GraphNode node2 = targetCharacter.gridTileLocation.parentAreaMap.pathfindingGraph.GetNearest(targetCharacter.marker.transform.position, nodeConstraint).node;
        if (node1 == null) {
            Debug.LogWarning("Node1 is null!");
        }
        if (node2 == null) {
            Debug.LogWarning("Node2 is null!");
        }
        if (node1 != null && node2 != null) {
            bool hasPath = PathUtilities.IsPathPossible(node1, node2);
            Debug.Log(this.character.name + " has path to " + targetCharacter.name + "? " + hasPath.ToString());
        }
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
    public void UpdateAnimation() {
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
        } else if (character.isStoppedByOtherCharacter > 0) {
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
        if (triggerName == "Attack" && character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT) {
            return; //because sometime trigger is set even though character is no longer in combat state.
        }
        animator.SetTrigger(triggerName);
        if (triggerName == "Attack") {
            //start coroutine to call 
            animationListener.OnAttackAnimationTriggered();
        }
    }
    public void SetAnimationBool(string name, bool value) {
        animator.SetBool(name, value);
    }
    private void UpdateAnimationSpeed() {
        animator.speed = 1f * progressionSpeedMultiplier;
    }
    #endregion

    #region Utilities
    private float GetSpeed() {
        float speed = GetSpeedWithoutProgressionMultiplier();
        speed *= progressionSpeedMultiplier;
        return speed;
    }
    private float GetSpeedWithoutProgressionMultiplier() {
        float speed = character.runSpeed;
        if (targettedByRemoveNegativeTraitActionsCounter > 0) {
            speed = character.walkSpeed;
        } else {
            if (useWalkSpeed > 0) {
                speed = character.walkSpeed;
            } else {
                if (character.stateComponent.currentState != null) {
                    if (character.stateComponent.currentState.characterState == CHARACTER_STATE.EXPLORE
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.PATROL
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL
                        || character.stateComponent.currentState.characterState == CHARACTER_STATE.STROLL_OUTSIDE) {
                        //Walk
                        speed = character.walkSpeed;
                    }
                }
                if (character.currentAction != null) {
                    if (character.currentAction.goapType == INTERACTION_TYPE.RETURN_HOME || character.currentAction.goapType.IsEmergencyAction()) {
                        //Run
                        speed = character.runSpeed;
                    }
                }
            }
        }
        speed += (speed * character.speedModifier);
        if (speed <= 0f) {
            speed = 0.5f;
        }
        speed += (speed * fleeSpeedModifier);
        if (speed < 0.5f) {
            speed = 0.5f;
        }
        return speed;
    }
    public void UpdateSpeed() {
        pathfindingAI.speed = GetSpeed();
        //Debug.Log("Updated speed of " + character.name + ". New speed is: " + pathfindingAI.speed.ToString());
    }
    public void AdjustUseWalkSpeed(int amount) {
        useWalkSpeed += amount;
        useWalkSpeed = Mathf.Max(0, useWalkSpeed);
    }
    public void AdjustTargettedByRemoveNegativeTraitActions(int amount) {
        targettedByRemoveNegativeTraitActionsCounter += amount;
        targettedByRemoveNegativeTraitActionsCounter = Mathf.Max(0, targettedByRemoveNegativeTraitActionsCounter);
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
            attackExecutedTime = assets.biteTiming;
        } else {
            switch (character.characterClass.rangeType) {
                case RANGE_TYPE.MELEE:
                    attackClip = assets.slashClip;
                    attackExecutedTime = assets.slashTiming;
                    break;
                case RANGE_TYPE.RANGED:
                    if (character.characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
                        attackClip = assets.arrowClip;
                        attackExecutedTime = assets.arrowTiming;
                    } else {
                        attackClip = assets.magicClip;
                        attackExecutedTime = assets.magicTiming;
                    }
                    break;
                default:
                    attackClip = assets.slashClip;
                    attackExecutedTime = assets.slashTiming;
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
    public void InitialPlaceMarkerAt(LocationGridTile tile, bool addToLocation = true) {
        PlaceMarkerAt(tile, addToLocation);
        pathfindingAI.UpdateMe();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = true;
        }
        visionCollision.Initialize();
        CreateCollisionTrigger();
        UpdateSpeed();
    }
    public void PlaceMarkerAt(LocationGridTile tile, bool addToLocation = true) {
        this.gameObject.transform.SetParent(tile.parentAreaMap.objectsParent);
        pathfindingAI.Teleport(tile.centeredWorldLocation);
        if (addToLocation) {
            tile.structure.location.AddCharacterToLocation(character);
            tile.structure.AddCharacterAtLocation(character, tile);
        }
        SetActiveState(true);
        UpdateAnimation();
        UpdatePosition();
        UpdateActionIcon();
        for (int i = 0; i < colliders.Length; i++) {
            if (!colliders[i].enabled) {
                colliders[i].enabled = true;
            }
        }
        //if (!visionCollision.isInitialized) {
        //    visionCollision.Initialize();
            
        //}
        //if (collisionTrigger == null) {
        //    CreateCollisionTrigger();
        //}
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
    public void OnDeath(LocationGridTile deathTileLocation, bool isOutsideSettlement = false) {
        if (character.race == RACE.SKELETON || character is Summon || character.minion != null || isOutsideSettlement) {
            character.DestroyMarker();
        } else {
            for (int i = 0; i < colliders.Length; i++) {
                colliders[i].enabled = false;
            }
            pathfindingAI.ClearAllCurrentPathData();
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
            StartCoroutine(UpdatePositionNextFrame());
        }
    }
    private IEnumerator UpdatePositionNextFrame() {
        yield return null;
        UpdatePosition();
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
    private bool CanDoStealthActionToTarget(Character target) {
        if (!target.isDead) {
            if (target.marker.inVisionCharacters.Count > 1) {
                return false; //if there are 2 or more in vision of target character it means he is not alone anymore
            } else if (target.marker.inVisionCharacters.Count == 1) {
                if (!target.marker.inVisionCharacters.Contains(character)) {
                    return false; //if there is only one in vision of target character and it is not this character, it means he is not alone
                }
            }
        } else {
            if (inVisionCharacters.Count > 1) {
                return false;
            }
        }
        return true;
    }
    public bool CanDoStealthActionToTarget(IPointOfInterest target) {
        if(target is Character) {
            return CanDoStealthActionToTarget(target as Character);
        }
        if (inVisionCharacters.Count > 0) {
            return false;
        }
        return true;
    }
    public void SetMarkerColor(Color color) {
        mainImg.color = color;
    }
    public void QuickShowHPBar() {
        StartCoroutine(QuickShowHPBarCoroutine());
    }
    private IEnumerator QuickShowHPBarCoroutine() {
        ShowHPBar();
        yield return new WaitForSeconds(1f);
        if (!(character.stateComponent.currentState is CombatState)) {
            HideHPBar();
        }
    }
    #endregion

    #region Vision Collision
    private void CreateCollisionTrigger() {
        GameObject collisionTriggerGO = GameObject.Instantiate(InteriorMapManager.Instance.characterCollisionTriggerPrefab, this.transform);
        collisionTriggerGO.transform.localPosition = Vector3.zero;
        collisionTrigger = collisionTriggerGO.GetComponent<POICollisionTrigger>();
        collisionTrigger.Initialize(character);
    }
    public void AddPOIAsInVisionRange(IPointOfInterest poi) {
        if (!inVisionPOIs.Contains(poi)) {
            inVisionPOIs.Add(poi);
            unprocessedVisionPOIs.Add(poi);
            if (poi is Character) {
                inVisionCharacters.Add(poi as Character);
            }
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
            unprocessedVisionPOIs.Remove(poi);
            if (poi is Character) {
                inVisionCharacters.Remove(poi as Character);
                Messenger.Broadcast(Signals.CHARACTER_REMOVED_FROM_VISION, character, poi as Character);
            }
        }
    }
    public void ClearPOIsInVisionRange() {
        inVisionPOIs.Clear();
        unprocessedVisionPOIs.Clear();
        inVisionCharacters.Clear();
    }
    public void LogPOIsInVisionRange() {
        string summary = character.name + "'s POIs in range: ";
        for (int i = 0; i < inVisionPOIs.Count; i++) {
            summary += "\n- " + inVisionPOIs[i].ToString();
        }
        Debug.Log(summary);
    }
    private void OnAddPOIAsInVisionRange(IPointOfInterest poi) {
        if (character.currentAction != null && character.currentAction.actionLocationType == ACTION_LOCATION_TYPE.TARGET_IN_VISION && character.currentAction.poiTarget == poi) {
            StopMovement();
            character.PerformGoapAction();
        }
    }
    private void ProcessAllUnprocessedVisionPOIs() {
        if(unprocessedVisionPOIs.Count > 0 && (character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT)) {
            string log = GameManager.Instance.TodayLogString() + character.name + " tick ended! Processing all unprocessed in visions...";
            if (!character.isDead && character.GetNormalTrait("Unconscious", "Resting", "Zapped") == null) {
                for (int i = 0; i < unprocessedVisionPOIs.Count; i++) {
                    IPointOfInterest poi = unprocessedVisionPOIs[i];
                    log += "\n - Reacting to " + poi.name;
                    //Collect all actions to witness and avoid duplicates
                    List<GoapAction> actions = character.ThisCharacterSaw(poi);
                    if (actions != null && actions.Count > 0) {
                        for (int j = 0; j < actions.Count; j++) {
                            GoapAction action = actions[j];
                            if ((action.isPerformingActualAction && !action.isDone && action.goapType != INTERACTION_TYPE.WATCH) ||
                                (action.currentState != null && action.currentState.name == action.whileMovingState)) {
                                //Cannot witness a watch action
                                IPointOfInterest poiTarget = null;
                                if (action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                                    poiTarget = (action as MakeLove).targetCharacter;
                                } else {
                                    poiTarget = action.poiTarget;
                                }
                                if (action.actor != character && poiTarget != character) {
                                    if (!actionsToWitness.Contains(action)) {
                                        actionsToWitness.Add(action);
                                    }
                                }
                            }
                        }
                    }

                    log += "\n - Reacting to character traits...";
                    //Character reacts to traits
                    if(character.stateComponent.currentState == null || !character.stateComponent.currentState.OnEnterVisionWith(poi)) {
                        if (!character.CreateJobsOnEnterVisionWith(poi, true)) {
                            if (poi is Character) {
                                visionCollision.ChatHandling(poi as Character);
                            }
                        }
                    }
                }

                //Witness all actions
                log += "\n - Witnessing collected actions:";
                if (actionsToWitness.Count > 0) {
                    for (int i = 0; i < actionsToWitness.Count; i++) {
                        GoapAction action = actionsToWitness[i];
                        log += "\n   - Witnessed: " + action.goapName + " of " + action.actor.name + " with target " + action.poiTarget.name;
                        character.ThisCharacterWitnessedEvent(action);
                    }
                } else {
                    log += "\n   - No collected actions";
                }
            } else {
                log += "\n - Character is either dead, unconscious, resting, or zapped, not processing...";
            }
            unprocessedVisionPOIs.Clear();
            character.PrintLogIfActive(log);
        }
        actionsToWitness.Clear();
        if (willProcessCombat && (hostilesInRange.Count > 0 || avoidInRange.Count > 0)) {
            string log = GameManager.Instance.TodayLogString() + character.name + " process combat switch is turned on and there are hostiles or avoid in list, processing combat...";
            ProcessCombatBehavior();
            willProcessCombat = false;
            character.PrintLogIfActive(log);
        }
    }
    #endregion

    #region Hosility Collision
    public bool AddHostileInRange(IPointOfInterest poi, bool checkHostility = true, bool processCombatBehavior = true, bool isLethal = true) {
        if (!hostilesInRange.Contains(poi)) {
            if (this.character.GetNormalTrait("Zapped") == null && !this.character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) 
                && !this.character.isFollowingPlayerInstruction && CanAddPOIAsHostile(poi, checkHostility)) {
                if (!WillCharacterTransferEngageToFleeList(isLethal)) {
                    hostilesInRange.Add(poi);
                    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                        lethalCharacters.Add(poi as Character, isLethal);
                    }
                    this.character.PrintLogIfActive(GameManager.Instance.TodayLogString() + poi.name + " was added to " + this.character.name + "'s hostile range!");
                    //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
                    //if (processCombatBehavior) {
                    //    ProcessCombatBehavior();
                    //}
                    willProcessCombat = true;
                } else {
                    //Transfer to flee list
                    return AddAvoidInRange(poi, processCombatBehavior);
                }
                return true;
            }
        }
        return false;
    }
    private bool CanAddPOIAsHostile(IPointOfInterest poi, bool checkHostility) {
        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character character = poi as Character;
            return !character.isDead && !this.character.isFollowingPlayerInstruction &&
                (!checkHostility || this.character.IsHostileWith(character));
        } else {
            return true; //allow any other object types
        }
    }
    public bool AddHostileInRange(IPointOfInterest poi, out CharacterState reaction, bool checkHostility = true, bool processCombatBehavior = true, bool isLethal = true) {
        if (AddHostileInRange(poi, checkHostility, processCombatBehavior, isLethal)) {
            reaction = character.stateComponent.currentState;
            return true;
        }
        reaction = null;
        return false;
    }
    public void RemoveHostileInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (hostilesInRange.Remove(poi)) {
            if (poi is Character) {
                lethalCharacters.Remove(poi as Character);
            }
            string removeHostileSummary = GameManager.Instance.TodayLogString() + poi.name + " was removed from " + character.name + "'s hostile range.";
            character.PrintLogIfActive(removeHostileSummary);
            //When removing hostile in range, check if character is still in combat state, if it is, reevaluate combat behavior, if not, do nothing
            if (processCombatBehavior && character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                CombatState combatState = character.stateComponent.currentState as CombatState;
                if (combatState.forcedTarget == poi) {
                    combatState.SetForcedTarget(null);
                }
                if (combatState.currentClosestHostile == poi) {
                    combatState.ResetClosestHostile();
                }
                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
            }
        }
    }
    public void ClearHostilesInRange(bool processCombatBehavior = true) {
        if(hostilesInRange.Count > 0) {
            hostilesInRange.Clear();
            lethalCharacters.Clear();
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
        //NOTE: This is no longer needed since this will only cause duplicates because CreateJobsOnEnterVisionWith will also be called upon adding the Dead trait
        //if (inVisionCharacters.Contains(otherCharacter)) {
        //    character.CreateJobsOnEnterVisionWith(otherCharacter); //this is used to create jobs that involve characters that died within the character's range of vision
        //}


        //RemovePOIFromInVisionRange(otherCharacter);
        //visionCollision.RemovePOIAsInRangeButDifferentStructure(otherCharacter);

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
    public bool IsLethalCombatForTarget(Character character) {
        if (lethalCharacters.ContainsKey(character)) {
            return lethalCharacters[character];
        }
        return true;
    }
    public bool HasLethalCombatTarget() {
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi is Character) {
                Character hostile = poi as Character;
                if (IsLethalCombatForTarget(hostile)) {
                    return true;
                }
            }
            
        }
        return false;
    }
    #endregion

    #region Avoid In Range
    public bool AddAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (poi is Character) {
            return AddAvoidInRange(poi as Character, processCombatBehavior);
        } else {
            if (character.GetNormalTrait("Berserked") == null) {
                if (!avoidInRange.Contains(poi)) {
                    avoidInRange.Add(poi);
                    willProcessCombat = true;
                    return true;
                }
            }
        }
        return false;
    }
    public bool AddAvoidsInRange(List<IPointOfInterest> pois, bool processCombatBehavior = true) {
        //Only react to the first hostile that is added
        IPointOfInterest otherPOI = null;
        for (int i = 0; i < pois.Count; i++) {
            IPointOfInterest poi = pois[i];
            if (poi is Character) {
                Character characterToAvoid = poi as Character;
                if (characterToAvoid.isDead || characterToAvoid.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) || characterToAvoid.GetNormalTrait("Berserked") != null) {
                    continue; //skip
                }
            }
            if (!avoidInRange.Contains(poi)) {
                avoidInRange.Add(poi);
                if (otherPOI == null) {
                    otherPOI = poi;
                }
            }

        }
        if (otherPOI != null) {
            willProcessCombat = true;
            return true;
        }
        return false;
    }
    private bool AddAvoidInRange(Character poi, bool processCombatBehavior = true) {
        if (!poi.isDead && !poi.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER) && character.GetNormalTrait("Berserked") == null) { //, "Resting"
            if (!avoidInRange.Contains(poi)) {
                avoidInRange.Add(poi);
                //NormalReactToHostileCharacter(poi, CHARACTER_STATE.FLEE);
                //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
                //if (processCombatBehavior) {
                //    ProcessCombatBehavior();
                //}
                willProcessCombat = true;
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
            willProcessCombat = true;
            return true;
        }
        return false;
    }
    public void RemoveAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (avoidInRange.Remove(poi)) {
            //Debug.Log("Removed avoid in range " + poi.name + " from " + this.character.name);
            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                }
            }
        }
    }
    //public void RemoveAvoidInRange(Character poi, bool processCombatBehavior = true) {
    //    if (avoidInRange.Remove(poi)) {
    //        //Debug.Log("Removed avoid in range " + poi.name + " from " + this.character.name);
    //        //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
    //        if (processCombatBehavior) {
    //            if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
    //                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
    //            } 
    //        }
    //    }
    //}
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

    #region Flee
    public bool hasFleePath { get; private set; }
    private float fleeSpeedModifier;
    private const float Starting_Flee_Modifier = 0.3f; //starts at positive value, increase speed by 30%, then gradually reduce every other tick, eventually becoming negative.
    private const int Tick_Flee_Will_Slow_Down = 2; //What tick will the fleeing character slow down at.
    private const int Tick_Flee_In_Slow_Down_Check = 3; //What tick will the fleeing character check for exit state when he/she is at his/her lowest speed.
    private int ticksInFlee;
    public void OnStartFlee() {
        if (avoidInRange.Count == 0) {
            return;
        }
        pathfindingAI.ClearAllCurrentPathData();
        SetHasFleePath(true);
        pathfindingAI.canSearch = false; //set to false, because if this is true and a destination has been set in the ai path, the ai will still try and go to that point instead of the computed flee path
        FleeMultiplePath fleePath = FleeMultiplePath.Construct(this.transform.position, avoidInRange.Select(x => x.gridTileLocation.worldLocation).ToArray(), 20000);
        fleePath.aimStrength = 1;
        fleePath.spread = 4000;
        seeker.StartPath(fleePath);
    }
    public void OnFleePathComputed(Path path) {
        //|| character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT 
        if (character == null || character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return; //this is for cases that the character is no longer in a combat state, but the pathfinding thread returns a flee path
        }
        //Debug.Log(character.name + " computed flee path");
        arrivalAction = OnFinishedTraversingFleePath;
        StartMovement();
        fleeSpeedModifier = Starting_Flee_Modifier;
        Messenger.AddListener(Signals.TICK_STARTED, PerTickFlee);
        ticksInFlee = 0;
        //Debug.Log(GameManager.Instance.TodayLogString() + character.name + " will start fleeing");
    }
    public void OnFinishedTraversingFleePath() {
        //Debug.Log(name + " has finished traversing flee path.");
        SetHasFleePath(false);
        if (character.stateComponent.currentState is CombatState) {
            (character.stateComponent.currentState as CombatState).FinishedTravellingFleePath();
        }
        UpdateAnimation();
        UpdateActionIcon();
        StopPerTickFlee();
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
    public void AddTerrifyingObject(List<IPointOfInterest> objs) {
        for (int i = 0; i < objs.Count; i++) {
            AddTerrifyingObject(objs[i]);
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
    /// <summary>
    /// Function that determines if the character's hostile list must be transfered to avoid list
    /// Can be triggered by broadcasting signal <see cref="Signals.TRANSFER_ENGAGE_TO_FLEE_LIST"/>
    /// </summary>
    /// <param name="character">The character that should determine the transfer.</param>
    private void TransferEngageToFleeList(Character character) {
        if (this.character == character) {
            string summary = GameManager.Instance.TodayLogString() + character.name + " will determine the transfer from engage list to flee list";
            if(character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                summary += "\n" + character.name + " has negative disabler trait. Ignoring transfer.";
                character.PrintLogIfActive(summary);
                return;
            }
            if (hostilesInRange.Count == 0 && avoidInRange.Count == 0) {
                summary +=  "\n" + character.name + " does not have any characters in engage or avoid list. Ignoring transfer.";
                character.PrintLogIfActive(summary);
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
                if (HasLethalCombatTarget()) {
                    for (int i = 0; i < hostilesInRange.Count; i++) {
                        IPointOfInterest hostile = hostilesInRange[i];
                        if (inVisionPOIs.Contains(hostile)) {
                            AddAvoidInRange(hostile, false);
                        } else {
                            RemoveHostileInRange(hostile, false);
                            i--;
                        }
                    }
                    ClearHostilesInRange(false);
                }
                if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
                } else {
                    if (!character.currentParty.icon.isTravellingOutside) {
                        character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
                    }
                }
            }
            character.PrintLogIfActive(summary);
        }
    }
    private void PerTickFlee() {
        ticksInFlee++;
        if (GetSpeedWithoutProgressionMultiplier() <= 0.5f) {
            //lowest speed
            if (ticksInFlee >= Tick_Flee_In_Slow_Down_Check) {
                ticksInFlee = 0;
                if (!StillHasAvoidInActualRange() && character.stateComponent.currentState is CombatState) {
                    (character.stateComponent.currentState as CombatState).OnReachLowFleeSpeedThreshold();
                }
            }
        } else {
            //slow down current speed
            if (ticksInFlee >= Tick_Flee_Will_Slow_Down) {
                ticksInFlee = 0;
                fleeSpeedModifier -= 0.4f;
                UpdateSpeed();
            }
        }
    }
    /// <summary>
    /// Does this character still have a character that it needs to avoid in its actual visual range?
    /// </summary>
    /// <returns>True or false</returns>
    private bool StillHasAvoidInActualRange() {
        for (int i = 0; i < avoidInRange.Count; i++) {
            IPointOfInterest currAvoid = avoidInRange[i];
            if (inVisionCharacters.Contains(currAvoid)
                || inVisionPOIs.Contains(currAvoid)
                || visionCollision.poisInRangeButDiffStructure.Contains(currAvoid)) {
                return true;
            }
        }
        return false;
    }
    public void StopPerTickFlee() {
        fleeSpeedModifier = 0f;
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickFlee);
    }
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
    public IPointOfInterest GetNearestValidHostile() {
        IPointOfInterest nearest = null;
        float nearestDist = 9999f;
        //first check only the hostiles that are in the same area as this character
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi.IsValidCombatTarget()) {
                float dist = Vector2.Distance(this.transform.position, poi.worldPosition);
                if (nearest == null || dist < nearestDist) {
                    nearest = poi;
                    nearestDist = dist;
                }
            }
            
        }
        //if no character was returned, choose at random from the list, since we are sure that all characters in the list are not in the same area as this character
        if (nearest == null) {
            List<Character> hostileCharacters = hostilesInRange.Where(x => x.poiType == POINT_OF_INTEREST_TYPE.CHARACTER).Select(x => x as Character).ToList();
            if (hostileCharacters.Count > 0) {
                nearest = hostileCharacters[UnityEngine.Random.Range(0, hostileCharacters.Count)];
            }
        }
        return nearest;
    }
    public bool IsCharacterInLineOfSightWith(IPointOfInterest target) {
        //return targetCharacter.currentStructure == character.currentStructure;
        //precompute our ray settings
        Vector3 start = transform.position;
        Vector3 direction = target.worldPosition - transform.position;

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
                    POICollisionTrigger collisionTrigger = hit.collider.gameObject.GetComponent<POICollisionTrigger>();
                    if (collisionTrigger != null) {
                        if (collisionTrigger.poi == target) {
                            return true;
                        } else if (!(collisionTrigger.poi is Character)) {
                            return false; //if the poi collision is not from a character, consider the target as obstructed
                        }
                        
                    }
                }
                //Debug.LogWarning(character.name + " collided with: " + hit.collider.gameObject.name);
            }
        }
        return false;
    }
    public bool WillCharacterTransferEngageToFleeList(bool isLethal) {
        bool willTransfer = false;
        if(character.GetNormalTrait("Coward") != null && character.GetNormalTrait("Berserked") == null) {
            willTransfer = true;
        } else if (!isLethal && !HasLethalCombatTarget()) {
            willTransfer = false;
        }
        //- if character is berserked, must not flee
        else if (character.GetNormalTrait("Berserked") != null) {
            willTransfer = false;
        }
        //- at some point, situation may trigger the character to flee, at which point it will attempt to move far away from target
        //else if (character.GetNormalTrait("Injured") != null) {
        //    //summary += "\n" + character.name + " is injured.";
        //    //-character gets injured(chance based dependent on the character)
        //    willTransfer = true;
        //} 
        else if (character.IsHealthCriticallyLow()) {
            //summary += "\n" + character.name + "'s health is critically low.";
            //-character's hp is critically low (chance based dependent on the character)
            willTransfer = true;
        }
        //else if (character.GetNormalTrait("Spooked") != null) {
        //    //- fear-type status effect
        //    willTransfer = true;
        //} 
        else if (character.isStarving && character.GetNormalTrait("Vampiric") == null) {
            //-character is starving and is not a vampire
            willTransfer = true;
        } else if (character.isExhausted) {
            //-character is exhausted
            willTransfer = true;
        }
        return willTransfer;
    }
    public void OnThisCharacterEndedCombatState() {
        StopPerTickFlee();
    }
    public void ProcessCombatBehavior() {
        if (this.character.stateComponent.currentState != null && this.character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
            Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, this.character);
        } else {
            this.character.stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
        }
    }
    #endregion
}
