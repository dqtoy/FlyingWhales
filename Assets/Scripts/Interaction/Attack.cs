using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Interaction {
    //interactable is the TARGET
    //characterInvolved.currentParty.characters are the ATTACKERS

    private DefenderGroup _defenderGroup;
    private Minion _supporterMinion;
    private Combat _combat;

    private const string Start = "Start";
    private const string Helped_Attackers_Won = "Helped Attackers Won";
    private const string Helped_Attackers_Lost = "Helped Attackers Lost";
    private const string Helped_Attackers_No_Defense = "Helped Attackers No Defense";
    private const string Helped_Defenders_Won = "Helped Defenders Won";
    private const string Helped_Defenders_Lost = "Helped Defenders Lost";
    private const string Solo_Defense_Won = "Solo Defense Won";
    private const string Solo_Defense_Lost = "Solo Defense Lost";
    private const string Normal_Attackers_Won = "Normal Attackers Won";
    private const string Normal_Attackers_Lost = "Normal Attackers Lost";
    private const string Normal_Attackers_No_Defense = "Normal Attackers No Defense";

    public Attack(Area interactable) : base(interactable, INTERACTION_TYPE.ATTACK, 0) {
        _name = "Attack";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }
    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState helpedAttackersWonState = new InteractionState(Helped_Attackers_Won, this);
        InteractionState helpedAttackersLostState = new InteractionState(Helped_Attackers_Lost, this);
        InteractionState helpedAttackersNoDefState = new InteractionState(Helped_Attackers_No_Defense, this);
        InteractionState helpedDefendersWonState = new InteractionState(Helped_Defenders_Won, this);
        InteractionState helpedDefendersLostState = new InteractionState(Helped_Defenders_Lost, this);
        InteractionState soloDefenseWonState = new InteractionState(Solo_Defense_Won, this);
        InteractionState soloDefenseLostState = new InteractionState(Solo_Defense_Lost, this);
        InteractionState normalAttackersWonState = new InteractionState(Normal_Attackers_Won, this);
        InteractionState normalAttackersLostState = new InteractionState(Normal_Attackers_Lost, this);
        InteractionState normalAttackersNoDefState = new InteractionState(Normal_Attackers_No_Defense, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.homeArea, _characterInvolved.homeArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);


        CreateActionOptions(startState);

        helpedAttackersWonState.SetEffect(() => HelpedAttackersWonEffect(helpedAttackersWonState));
        helpedAttackersLostState.SetEffect(() => HelpedAttackersLostEffect(helpedAttackersLostState));
        helpedAttackersNoDefState.SetEffect(() => HelpedAttackersNoDefenseEffect(helpedAttackersNoDefState));
        helpedDefendersWonState.SetEffect(() => HelpedDefendersWonEffect(helpedDefendersWonState));
        helpedDefendersLostState.SetEffect(() => HelpedDefendersLostEffect(helpedDefendersLostState));
        soloDefenseWonState.SetEffect(() => SoloDefenseWonEffect(soloDefenseWonState));
        soloDefenseLostState.SetEffect(() => SoloDefenseLostEffect(soloDefenseLostState));
        normalAttackersWonState.SetEffect(() => NormalAttackersWonEffect(normalAttackersWonState));
        normalAttackersLostState.SetEffect(() => NormalAttackersLostEffect(normalAttackersLostState));
        normalAttackersNoDefState.SetEffect(() => NormalAttackersNoDefenseEffect(normalAttackersNoDefState));

        _states.Add(startState.name, startState);
        _states.Add(helpedAttackersWonState.name, helpedAttackersWonState);
        _states.Add(helpedAttackersLostState.name, helpedAttackersLostState);
        _states.Add(helpedAttackersNoDefState.name, helpedAttackersNoDefState);
        _states.Add(helpedDefendersWonState.name, helpedDefendersWonState);
        _states.Add(helpedDefendersLostState.name, helpedDefendersLostState);
        _states.Add(soloDefenseWonState.name, soloDefenseWonState);
        _states.Add(soloDefenseLostState.name, soloDefenseLostState);
        _states.Add(normalAttackersWonState.name, normalAttackersWonState);
        _states.Add(normalAttackersLostState.name, normalAttackersLostState);
        _states.Add(normalAttackersNoDefState.name, normalAttackersNoDefState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption supportAttackersOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Support the attackers.",
                duration = 0,
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be Diplomat.",
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => SupportAttackersOption(state),
            };
            ActionOption supportDefendersOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Support the defenders.",
                duration = 0,
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Minion must be Diplomat.",
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => SupportDefendersOption(state),
            };
            ActionOption standAsideOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Stand Aside.",
                duration = 0,
                effect = () => StandAsideOption(),
            };

            state.AddActionOption(supportAttackersOption);
            state.AddActionOption(supportDefendersOption);
            state.AddActionOption(standAsideOption);
            state.SetDefaultOption(standAsideOption);
        }
    }
    #endregion

    #region Action Options
    private void SupportAttackersOption(InteractionState state) {
        _supporterMinion = state.assignedMinion;
        //Add minion to support
        _characterInvolved.currentParty.AddCharacter(_supporterMinion.character);

        _defenderGroup = interactable.GetDefenseGroup();
        _combat = null;
        if (_defenderGroup != null && _defenderGroup.party != null) {
            _combat = _characterInvolved.currentParty.CreateCombatWith(_defenderGroup.party);
            _combat.Fight();
        }
        if(_combat != null) {
            if(_combat.winningSide == SIDES.A) {
                //Helped Attackers Won
                SetCurrentState(_states[Helped_Attackers_Won]);
            } else {
                //Helped Attackers Lost
                SetCurrentState(_states[Helped_Attackers_Lost]);
            }
        } else {
            //Helped Attackers No Defense
            SetCurrentState(_states[Helped_Attackers_No_Defense]);
        }
    }
    private void SupportDefendersOption(InteractionState state) {
        _supporterMinion = state.assignedMinion;
        _defenderGroup = interactable.GetDefenseGroup();
        _combat = null;
        if (_defenderGroup != null && _defenderGroup.party != null) {
            //Add minion to support
            _defenderGroup.party.AddCharacter(_supporterMinion.character);
            _combat = _characterInvolved.currentParty.CreateCombatWith(_defenderGroup.party);
        } else {
            _combat = _characterInvolved.currentParty.CreateCombatWith(_supporterMinion.character.currentParty);
        }
        _combat.Fight();

        if (_combat.winningSide == SIDES.A) {
            if(_defenderGroup != null) {
                //Helped Defenders Lost - means that attackers won
                SetCurrentState(_states[Helped_Defenders_Lost]);
            } else {
                //Solo Defense Lost - means that attackers won
                SetCurrentState(_states[Solo_Defense_Lost]);
            }
        } else {
            if (_defenderGroup != null) {
                //Helped Defenders Won
                SetCurrentState(_states[Helped_Defenders_Won]);
            } else {
                //Solo Defense Won
                SetCurrentState(_states[Solo_Defense_Won]);
            }
        }
    }
    private void StandAsideOption() {
        _defenderGroup = interactable.GetDefenseGroup();
        _combat = null;
        if (_defenderGroup != null && _defenderGroup.party != null) {
            _combat = _characterInvolved.currentParty.CreateCombatWith(_defenderGroup.party);
            _combat.Fight();
        }
        if (_combat != null) {
            if (_combat.winningSide == SIDES.A) {
                //Normal Attackers Won
                SetCurrentState(_states[Normal_Attackers_Won]);
            } else {
                //Normal Attackers Lost
                SetCurrentState(_states[Normal_Attackers_Lost]);
            }
        } else {
            //Normal Attackers No Defense
            SetCurrentState(_states[Normal_Attackers_No_Defense]);
        }
    }
    #endregion

    #region State Effects
    private void HelpedAttackersWonEffect(InteractionState state) {
        characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 2);
        _supporterMinion.character.currentParty.RemoveCharacter(_supporterMinion.character);
        _supporterMinion.SetEnabledState(true);
        //Remove Defender Group, if there is no more defender group after removing, trigger area death

        state.descriptionLog.AddToFillers(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2);
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        }

        state.AddLogFiller(new LogFiller(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            if(_combat.charactersSideA[i] != _supporterMinion.character) {
                state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
            }
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            if (_combat.charactersSideB[i] != _supporterMinion.character) {
                state.AddLogFiller(new LogFiller(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2), false);
            }
        }
        //DefenderGroup newDefenders = interactable.GetFirstDefenderGroup();
        //if (newDefenders == null) {

        Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special");
        log.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1);
        if(interactable.owner == null) {
            log.AddToFillers(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1);
        } else {
            log.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogToInvolvedObjects(log);

        interactable.Death();
        //}
    }
    private void HelpedAttackersLostEffect(InteractionState state) {
        characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);

        state.descriptionLog.AddToFillers(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2);
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
        }

        state.AddLogFiller(new LogFiller(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            if (_combat.charactersSideA[i] != _supporterMinion.character) {
                state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
            }
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            if (_combat.charactersSideB[i] != _supporterMinion.character) {
                state.AddLogFiller(new LogFiller(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2), false);
            }
        }
    }
    private void HelpedAttackersNoDefenseEffect(InteractionState state) {
        characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);
        _supporterMinion.character.currentParty.RemoveCharacter(_supporterMinion.character);
        _supporterMinion.SetEnabledState(true);

        state.descriptionLog.AddToFillers(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2);

        state.AddLogFiller(new LogFiller(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2));
        if (interactable.owner == null) {
            state.AddLogFiller(new LogFiller(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1));
        } else {
            state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        }
        for (int i = 0; i < characterInvolved.currentParty.characters.Count; i++) {
            state.AddLogFiller(new LogFiller(characterInvolved.currentParty.characters[i], characterInvolved.currentParty.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }

        interactable.Death();
    }
    private void HelpedDefendersLostEffect(InteractionState state) {
        interactable.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);

        state.descriptionLog.AddToFillers(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.AddLogFiller(new LogFiller(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2), false);
        }

        //DefenderGroup newDefenders = interactable.GetFirstDefenderGroup();
        //if (newDefenders == null) {

        Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special");
        log.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1);
        if (interactable.owner == null) {
            log.AddToFillers(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1);
        } else {
            log.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogToInvolvedObjects(log);

        interactable.Death();
        //}
    }
    private void HelpedDefendersWonEffect(InteractionState state) {
        interactable.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 2);
        _supporterMinion.character.currentParty.RemoveCharacter(_supporterMinion.character);
        _supporterMinion.SetEnabledState(true);

        state.descriptionLog.AddToFillers(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.AddLogFiller(new LogFiller(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2), false);
        }
    }
    private void SoloDefenseLostEffect(InteractionState state) {
        interactable.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 1);

        state.descriptionLog.AddToFillers(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2));
        if (interactable.owner == null) {
            state.AddLogFiller(new LogFiller(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1));
        } else {
            state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        }
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }

        interactable.Death();
    }
    private void SoloDefenseWonEffect(InteractionState state) {
        interactable.owner.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, 2);
        _supporterMinion.SetEnabledState(true);

        state.descriptionLog.AddToFillers(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion.character, _supporterMinion.character.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }

    }
    private void NormalAttackersWonEffect(InteractionState state) {
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2), false);
        }

        //DefenderGroup newDefenders = interactable.GetFirstDefenderGroup();
        //if (newDefenders == null) {

        Log log = new Log(GameManager.Instance.Today(), "Events", GetType().ToString(), state.name.ToLower() + "_special");
        log.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_1);
        if (interactable.owner == null) {
            log.AddToFillers(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1);
        } else {
            log.AddToFillers(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogToInvolvedObjects(log);

        interactable.Death();
        //}
    }
    private void NormalAttackersLostEffect(InteractionState state) {
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2, false);
            state.AddLogFiller(new LogFiller(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2), false);
        }
        _defenderGroup.DisbandGroup(); //disband defender group so that the members of the defender group can be placed back to the location
    }
    private void NormalAttackersNoDefenseEffect(InteractionState state) {
        if (interactable.owner == null) {
            state.AddLogFiller(new LogFiller(null, FactionManager.Instance.neutralFaction.name, LOG_IDENTIFIER.FACTION_1));
        } else {
            state.AddLogFiller(new LogFiller(interactable.owner, interactable.owner.name, LOG_IDENTIFIER.FACTION_1));
        }
        for (int i = 0; i < characterInvolved.currentParty.characters.Count; i++) {
            state.descriptionLog.AddToFillers(characterInvolved.currentParty.characters[i], characterInvolved.currentParty.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1, false);
            state.AddLogFiller(new LogFiller(characterInvolved.currentParty.characters[i], characterInvolved.currentParty.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1), false);
        }

        interactable.Death();
    }
    #endregion
}
