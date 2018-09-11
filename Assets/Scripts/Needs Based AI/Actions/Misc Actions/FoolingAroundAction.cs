﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class FoolingAroundAction : CharacterAction {

    public FoolingAroundAction() : base(ACTION_TYPE.FOOLING_AROUND) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        if (iparty is CharacterParty) {
            Character character = iparty.mainCharacter as Character;
            character.SetDoNotDisturb(true);
        }
    }
    public override void OnFirstEncounter(CharacterParty party, IObject targetObject) {
        base.OnFirstEncounter(party, targetObject);
        if (targetObject == null) {
            return;
        }
        if (targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if (icharacterObject.iparty is CharacterParty) {
                CharacterParty targetParty = icharacterObject.iparty as CharacterParty;
                Character targetCharacter = targetParty.mainCharacter as Character;

                if (targetParty.actionData.currentAction.actionData.actionType != ACTION_TYPE.FOOLING_AROUND) {
                    targetParty.actionData.currentAction.EndAction(targetParty, targetParty.actionData.currentTargetObject);
                    FoolingAroundAction actionToAssign = targetParty.mainCharacter.GetMiscAction(_actionData.actionType) as FoolingAroundAction;
                    targetParty.actionData.AssignAction(actionToAssign, party.icharacterObject);
                }
            }
        }
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveAllReward(party);
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        if (party.actionData.isDone) {
            return;
        }
        Character character = party.mainCharacter as Character;
        character.SetDoNotDisturb(false);

        if (targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if (icharacterObject.iparty is CharacterParty) {
                CharacterParty targetParty = icharacterObject.iparty as CharacterParty;
                Character targetCharacter = targetParty.mainCharacter as Character;
                targetCharacter.SetDoNotDisturb(false);
            }
        }
        //Relationship effects
    }
    public override IObject GetTargetObject(CharacterParty sourceParty) {
        Character mainCharacter = sourceParty.mainCharacter as Character;
        if (mainCharacter.GetAttribute(ATTRIBUTE.UNFAITHFUL) != null) {
            return GetTargetObjectAsUnfaithful(mainCharacter);
        } else if (mainCharacter.GetAttribute(ATTRIBUTE.LIBERATED) != null) {
            return GetTargetObjectAsLiberated(mainCharacter);
        }
        return GetTargetObjectAsDefault(mainCharacter); ;
    }
    public override CharacterAction Clone() {
        FoolingAroundAction action = new FoolingAroundAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion

    #region Utilities
    private IObject GetTargetObjectAsDefault(Character initiator) {
        Character partner = initiator.GetPartner();
        if(partner != null && partner.ownParty.specificLocation == initiator.ownParty.specificLocation) {
            return partner.ownParty.icharacterObject;
        }
        return null;
    }
    private IObject GetTargetObjectAsLiberated(Character initiator) {
        Character partner = initiator.GetPartner();
        if(partner != null && partner.ownParty.specificLocation == initiator.ownParty.specificLocation) {
            return partner.ownParty.icharacterObject;
        }

        //Initiator has no partner
        IObject target = GetLiberatedNoPartnerTarget(initiator);
        if(target != null) {
            return target;
        }
        return null;
    }
    private IObject GetTargetObjectAsUnfaithful(Character initiator) {
        Character partner = initiator.GetPartner();

        if(partner != null) {
            WeightedDictionary<Character> characterCandidates = new WeightedDictionary<Character>();
            for (int i = 0; i < initiator.ownParty.specificLocation.charactersAtLocation.Count; i++) {
                NewParty candidate = initiator.ownParty.specificLocation.charactersAtLocation[i];
                if (candidate.mainCharacter is Character) {
                    Character characterCandidate = candidate.mainCharacter as Character;
                    if (characterCandidate == partner) {
                        characterCandidates.AddElement(partner, 100);
                    } else {
                        if(characterCandidate.GetAttribute(ATTRIBUTE.UNFAITHFUL) != null) {
                            characterCandidates.AddElement(characterCandidate, 10);
                        }
                    }
                }
            }
            if(characterCandidates.Count > 0) {
                Character chosen = characterCandidates.PickRandomElementGivenWeights();
                return chosen.ownParty.icharacterObject;
            }
        } else {
            if(initiator.GetAttribute(ATTRIBUTE.LIBERATED) != null) {
                //Initiator has no partner
                IObject target = GetLiberatedNoPartnerTarget(initiator);
                if (target != null) {
                    return target;
                }
            }
        }
        return null;
    }
    private IObject GetLiberatedNoPartnerTarget(Character initiator) {
        List<Character> potentialCandidates = new List<Character>();
        for (int i = 0; i < initiator.ownParty.specificLocation.charactersAtLocation.Count; i++) {
            NewParty candidate = initiator.ownParty.specificLocation.charactersAtLocation[i];
            if(candidate.mainCharacter is Character) {
                Character characterCandidate = candidate.mainCharacter as Character;
                Character candidatePartner = characterCandidate.GetPartner();
                if(candidatePartner == null && characterCandidate.GetAttribute(ATTRIBUTE.LIBERATED) != null) {
                    potentialCandidates.Add(characterCandidate);
                }
            }
        }
        if(potentialCandidates.Count > 0) {
            return potentialCandidates[Utilities.rng.Next(0, potentialCandidates.Count)].ownParty.icharacterObject;
        }
        return null;
    }
    #endregion
}
