using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScavengeEventFaction : Interaction {

    private const string Supply_Scavenged = "Supply Scavenged";
    private const string No_Supply = "No Supply";

    private LocationStructure structure;
    public override LocationStructure targetStructure {
        get { return structure; }
    }

    public ScavengeEventFaction(Area interactable) : base(interactable, INTERACTION_TYPE.SCAVENGE_EVENT_FACTION, 0) {
        _name = "Scavenge Event Faction";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalScavengeSuccess = new InteractionState(Supply_Scavenged, this);
        InteractionState normalScavengeFail = new InteractionState(No_Supply, this);

        //**Structure**: Move the character to a random Warehouse or Dungeon Structure
        List<LocationStructure> choices = new List<LocationStructure>();
        if (interactable.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
            choices.AddRange(interactable.structures[STRUCTURE_TYPE.WAREHOUSE]);
        }
        if (interactable.HasStructure(STRUCTURE_TYPE.DUNGEON)) {
            choices.AddRange(interactable.structures[STRUCTURE_TYPE.DUNGEON]);
        }
        structure = choices[Random.Range(0, choices.Count)];
        _characterInvolved.MoveToAnotherStructure(structure);

        AddToDebugLog(_characterInvolved.name + " will scavenge " + structure.ToString());

        CreateActionOptions(startState);
        normalScavengeSuccess.SetEffect(() => SupplyScavengedSuccessRewardEffect(normalScavengeSuccess));
        normalScavengeFail.SetEffect(() => NoSupplyRewardEffect(normalScavengeFail));

        _states.Add(startState.name, startState);
        _states.Add(normalScavengeSuccess.name, normalScavengeSuccess);
        _states.Add(normalScavengeFail.name, normalScavengeFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        //This action will no longer applicable if the Location has become occupied by a faction.
        if (interactable.owner != null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Action Option Effect
    private void DoNothingEffect(InteractionState state) {
        string nextState = string.Empty;
        SupplyPile pile = structure.GetSupplyPile();
        if (pile != null && pile.HasSupply()) {
            nextState = Supply_Scavenged;
        } else {
            nextState = No_Supply;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    private void SupplyScavengedSuccessRewardEffect(InteractionState state) {
        //**Mechanics**: If the Structure is a Dungeon, randomize from the Dungeon's Supply Pile range.
        SupplyPile pile = structure.GetSupplyPile();
        int obtainedSupply = pile.GetSuppliesObtained();
        //if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
        //    _characterInvolved.homeArea.GetSuppliesFrom(interactable, obtainedSupply);
        //} else {
        //    _characterInvolved.homeArea.AdjustSuppliesInBank(obtainedSupply);
        //}
        pile.TransferSuppliesTo(_characterInvolved.homeArea, obtainedSupply);

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, obtainedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NoSupplyRewardEffect(InteractionState state) {
        
    }
    
}
