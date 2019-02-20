using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItem : Interaction {

    List<SpecialToken> items = new List<SpecialToken>();

    private SpecialToken targetToken;
    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public PickItem(Area interactable) : base(interactable, INTERACTION_TYPE.PICK_ITEM, 0) {
        _name = "Pick Item";
    }

    #region Overrides
    public override void PreLoad() {
        base.PreLoad();
        Log plannedLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), "planned_description", this);
        plannedLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        plannedLog.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1);
        _states["Start"].OverrideDescriptionLog(plannedLog);
    }
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //LoadItemChoices(characterInvolved);
        startState.SetEffect(() => StartRewardEffect(startState));

        _states.Add(startState.name, startState);
        //SetCurrentState(startState);
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (character.isHoldingItem) {
            return false;
        }
        //} else {
        //    for (int i = 0; i < interactable.possibleSpecialTokenSpawns.Count; i++) {
        //        SpecialToken token = interactable.possibleSpecialTokenSpawns[i];
        //        if(token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE || 
        //            (character.isAtHomeStructure && token.structureLocation == character.homeStructure)) {
        //            items.Add(token);
        //        }
        //    }
        //    if (items.Count <= 0) {
        //        return false;
        //    }
        //}
        return base.CanInteractionBeDoneBy(character);
    }
    public override bool CanStillDoInteraction(Character character) {
        if (character.isHoldingItem) {
            return false;
        } else {
            LoadItemChoices(character);
            if (items.Count <= 0) {
                return false;
            }
            targetToken = items[UnityEngine.Random.Range(0, items.Count)];
        }
        return base.CanStillDoInteraction(character);
    }
    public override object GetTarget() {
        return targetToken;
    }

    #endregion
    private void LoadItemChoices(Character character) {
        items = new List<SpecialToken>();
        for (int i = 0; i < interactable.possibleSpecialTokenSpawns.Count; i++) {
            SpecialToken token = interactable.possibleSpecialTokenSpawns[i];
            if (token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE ||
                (character.isAtHomeStructure && token.structureLocation == character.homeStructure)) {
                items.Add(token);
            }
        }
    }
    private void StartRewardEffect(InteractionState state) {
        _targetStructure = targetToken.structureLocation;
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
        //SpecialToken chosenToken = pickWeights.PickRandomElementGivenWeights();
        _characterInvolved.PickUpToken(targetToken, interactable);

        Log actualLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), "start_description", this);
        actualLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        actualLog.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1);

        state.OverrideDescriptionLog(actualLog);

        //**Mechanics**: Character drops the item it is holding and leaves it at the location.
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
    }
}
