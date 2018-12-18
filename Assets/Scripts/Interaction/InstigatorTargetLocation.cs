using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstigatorTargetLocation : Interaction {

    private const string Start = "Start";
    private const string Induce_Attack = "Induce Attack";
    private const string Do_Nothing = "Do Nothing";

    private LocationToken _targetLocationToken;
    private List<Character> _attackers;

    public InstigatorTargetLocation(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.INSTIGATOR_TARGET_LOCATION, 0) {
        _name = "Instigator Target Location";
        _jobFilter = new JOB[] { JOB.INSTIGATOR };
    }

    #region Overrides
    public override void CreateStates() {
        _targetLocationToken = _tokenTrigger as LocationToken;

        InteractionState startState = new InteractionState(Start, this);
        InteractionState induceAttackState = new InteractionState(Induce_Attack, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        induceAttackState.SetEffect(() => InduceAttackEffect(induceAttackState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(induceAttackState.name, induceAttackState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption induceOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Induce attack on " + _targetLocationToken.nameInBold,
                effect = () => InduceOption(state),
            };
            induceOption.canBeDoneAction = () => CanInduceAttack(induceOption);

            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do Nothing.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(induceOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private bool CanInduceAttack(ActionOption option) {
        SetAttackers();
        if(_attackers == null) {
            option.disabledTooltipText = "This location is not strong enough to launch an assault on " + _targetLocationToken.nameInBold;
            return false;
        } else {
            FactionRelationship relationship = interactable.tileLocation.areaOfTile.owner.GetRelationshipWith(_targetLocationToken.location.owner);
            if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY) {
                option.disabledTooltipText = "This location refuses to attack an Ally Faction.";
                return false;
            } else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                option.disabledTooltipText = "This location refuses to attack a Friend Faction.";
                return false;
            }
        }
        return true;
    }
    private void SetAttackers() {
        Area areaToAttack = interactable.tileLocation.areaOfTile;
        Area targetArea = _targetLocationToken.location;
        List<Character> residentsAtArea = new List<Character>();
        for (int i = 0; i < areaToAttack.areaResidents.Count; i++) {
            Character resident = areaToAttack.areaResidents[i];
            if (resident.role.roleType != CHARACTER_ROLE.LEADER && resident.role.roleType != CHARACTER_ROLE.CIVILIAN && !resident.currentParty.icon.isTravelling && !resident.isDefender && resident.specificLocation.tileLocation.areaOfTile.id == areaToAttack.id) {
                residentsAtArea.Add(resident);
            }
        }
        if (residentsAtArea.Count >= 3) {
            //If has at least 3 residents in area
            int numOfMembers = 3;
            if (residentsAtArea.Count >= 4) {
                numOfMembers = 4;
            }
            List<List<Character>> characterCombinations = Utilities.ItemCombinations(residentsAtArea, 5, numOfMembers, numOfMembers);
            if (characterCombinations.Count > 0) {
                List<Character> currentAttackCharacters = null;
                float highestWinChance = 0f;
                for (int i = 0; i < characterCombinations.Count; i++) {
                    List<Character> attackCharacters = characterCombinations[i];
                    DefenderGroup defender = targetArea.GetFirstDefenderGroup();
                    float winChance = 0f;
                    float loseChance = 0f;
                    if (defender != null) {
                        CombatManager.Instance.GetCombatChanceOfTwoLists(attackCharacters, defender.party.characters, out winChance, out loseChance);
                    } else {
                        CombatManager.Instance.GetCombatChanceOfTwoLists(attackCharacters, null, out winChance, out loseChance);
                    }
                    if (winChance > 40f) {
                        if (currentAttackCharacters == null) {
                            currentAttackCharacters = attackCharacters;
                            highestWinChance = winChance;
                        } else {
                            if (winChance > highestWinChance) {
                                currentAttackCharacters = attackCharacters;
                                highestWinChance = winChance;
                            }
                        }
                    }
                }
                _attackers = currentAttackCharacters;
            }
        }
    }
    private void InduceOption(InteractionState state) {
        SetCurrentState(_states[Induce_Attack]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void InduceAttackEffect(InteractionState state) {
        investigatorMinion.LevelUp();

        FactionRelationship relationship = interactable.tileLocation.areaOfTile.owner.GetRelationshipWith(_targetLocationToken.location.owner);
        if(relationship.relationshipStatus != FACTION_RELATIONSHIP_STATUS.ENEMY) {
            relationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.ENEMY);

            Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special");
            log.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
            log.AddToFillers(_targetLocationToken.location.owner, _targetLocationToken.location.owner.name, LOG_IDENTIFIER.FACTION_2);
            state.AddLogToInvolvedObjects(log);
        }

        MoveToAttack moveToAttack = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_ATTACK, interactable) as MoveToAttack;
        moveToAttack.SetTargetAndAttackers(_targetLocationToken.location, _attackers);
        _attackers[0].InduceInteraction(moveToAttack);

        state.descriptionLog.AddToFillers(_targetLocationToken.location, _targetLocationToken.location.name, LOG_IDENTIFIER.LANDMARK_2);
        for (int i = 0; i < _attackers.Count; i++) {
            state.descriptionLog.AddToFillers(_attackers[0], _attackers[0].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(_attackers[0], _attackers[0].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }
    }
    private void DoNothingEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, _targetLocationToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion
}
