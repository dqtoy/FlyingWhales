using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Interaction {
    //interactable.tileLocation.areaOfTile is the TARGET
    //characterInvolved.currentParty.characters are the ATTACKERS

    private DefenderGroup _defenderGroup;
    private Minion _supporterMinion;
    private Combat _combat;

    public Attack(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.ATTACK, 0) {
        _name = "Attack";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }
    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState helpedAttackersWonState = new InteractionState("Helped Attackers Won", this);
        InteractionState helpedAttackersLostState = new InteractionState("Helped Attackers Lost", this);
        InteractionState helpedAttackersNoDefState = new InteractionState("Helped Attackers No Defense", this);
        InteractionState helpedDefendersWonState = new InteractionState("Helped Defenders Won", this);
        InteractionState helpedDefendersLostState = new InteractionState("Helped Defenders Lost", this);
        InteractionState soloDefenseWonState = new InteractionState("Solo Defense Won", this);
        InteractionState soloDefenseLostState = new InteractionState("Solo Defense Lost", this);
        InteractionState normalAttackersWonState = new InteractionState("Normal Attackers Won", this);
        InteractionState normalAttackersLostState = new InteractionState("Normal Attackers Lost", this);
        InteractionState normalAttackersNoDefState = new InteractionState("Normal Attackers No Defense", this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_characterInvolved.homeLandmark.tileLocation.areaOfTile, _characterInvolved.homeLandmark.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_2);
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
                neededObjects = new List<System.Type>() { typeof(Minion) },
                effect = () => SupportAttackersOption(state),
            };
            ActionOption supportDefendersOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Support the defenders.",
                duration = 0,
                jobNeeded = JOB.DIPLOMAT,
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

        _defenderGroup = interactable.tileLocation.areaOfTile.GetFirstDefenderGroup();
        _combat = null;
        if (_defenderGroup != null) {
            _combat = _characterInvolved.currentParty.CreateCombatWith(_defenderGroup.party);
            _combat.Fight();
        }
        if(_combat != null) {
            if(_combat.winningSide == SIDES.A) {
                //Helped Attackers Won
                SetCurrentState(_states["Helped Attackers Won"]);
            } else {
                //Helped Attackers Lost
                SetCurrentState(_states["Helped Attackers Lost"]);
            }
        } else {
            //Helped Attackers No Defense
            SetCurrentState(_states["Helped Attackers No Defense"]);
        }
    }
    private void SupportDefendersOption(InteractionState state) {
        _supporterMinion = state.assignedMinion;
        _defenderGroup = interactable.tileLocation.areaOfTile.GetFirstDefenderGroup();
        _combat = null;
        if (_defenderGroup != null) {
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
                SetCurrentState(_states["Helped Defenders Lost"]);
            } else {
                //Solo Defense Lost - means that attackers won
                SetCurrentState(_states["Solo Defense Lost"]);
            }
        } else {
            if (_defenderGroup != null) {
                //Helped Defenders Won
                SetCurrentState(_states["Helped Defenders Won"]);
            } else {
                //Solo Defense Won
                SetCurrentState(_states["Solo Defense Won"]);
            }
        }
    }
    private void StandAsideOption() {
        _defenderGroup = interactable.tileLocation.areaOfTile.GetFirstDefenderGroup();
        _combat = null;
        if (_defenderGroup != null) {
            _combat = _characterInvolved.currentParty.CreateCombatWith(_defenderGroup.party);
            _combat.Fight();
        }
        if (_combat != null) {
            if (_combat.winningSide == SIDES.A) {
                //Normal Attackers Won
                SetCurrentState(_states["Normal Attackers Won"]);
            } else {
                //Normal Attackers Lost
                SetCurrentState(_states["Normal Attackers Lost"]);
            }
        } else {
            //Normal Attackers No Defense
            SetCurrentState(_states["Normal Attackers No Defense"]);
        }
    }
    #endregion

    #region State Effects
    private void HelpedAttackersWonEffect(InteractionState state) {
        characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);
        _supporterMinion.character.currentParty.RemoveCharacter(_supporterMinion.character);
        _supporterMinion.SetEnabledState(true);
        //Remove Defender Group, if there is no more defender group after removing, trigger area death

        state.descriptionLog.AddToFillers(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2);
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
        }

        state.AddLogFiller(new LogFiller(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            if(_combat.charactersSideA[i] != _supporterMinion.character) {
                state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
            }
        }

        DefenderGroup newDefenders = interactable.tileLocation.areaOfTile.GetFirstDefenderGroup();
        if (newDefenders == null) {
            interactable.tileLocation.areaOfTile.Death();
            //Log area is cleared
        }

        characterInvolved.currentParty.GoHome(() => characterInvolved.currentParty.DisbandParty());
    }
    private void HelpedAttackersLostEffect(InteractionState state) {
        characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 1);

        state.descriptionLog.AddToFillers(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2);
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
        }

        state.AddLogFiller(new LogFiller(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            if (_combat.charactersSideA[i] != _supporterMinion.character) {
                state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
            }
        }
    }
    private void HelpedAttackersNoDefenseEffect(InteractionState state) {
        characterInvolved.faction.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 1);
        _supporterMinion.character.currentParty.RemoveCharacter(_supporterMinion.character);
        _supporterMinion.SetEnabledState(true);

        state.descriptionLog.AddToFillers(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2);

        state.AddLogFiller(new LogFiller(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));
        for (int i = 0; i < characterInvolved.currentParty.characters.Count; i++) {
            state.AddLogFiller(new LogFiller(characterInvolved.currentParty.characters[i], characterInvolved.currentParty.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }

        interactable.tileLocation.areaOfTile.Death();
        characterInvolved.currentParty.GoHome(() => characterInvolved.currentParty.DisbandParty());
    }
    private void HelpedDefendersLostEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 1);

        state.descriptionLog.AddToFillers(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }

        DefenderGroup newDefenders = interactable.tileLocation.areaOfTile.GetFirstDefenderGroup();
        if (newDefenders == null) {
            interactable.tileLocation.areaOfTile.Death();
            //Log area is cleared
        }
        characterInvolved.currentParty.GoHome(() => characterInvolved.currentParty.DisbandParty());
    }
    private void HelpedDefendersWonEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);
        _supporterMinion.character.currentParty.RemoveCharacter(_supporterMinion.character);
        _supporterMinion.SetEnabledState(true);

        state.descriptionLog.AddToFillers(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }
    }
    private void SoloDefenseLostEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 1);

        state.descriptionLog.AddToFillers(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }
        interactable.tileLocation.areaOfTile.Death();
        characterInvolved.currentParty.GoHome(() => characterInvolved.currentParty.DisbandParty());
    }
    private void SoloDefenseWonEffect(InteractionState state) {
        interactable.tileLocation.areaOfTile.owner.AdjustFavorFor(PlayerManager.Instance.player.playerFaction, 2);
        _supporterMinion.SetEnabledState(true);

        state.descriptionLog.AddToFillers(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2);
        state.AddLogFiller(new LogFiller(_supporterMinion, _supporterMinion.name, LOG_IDENTIFIER.MINION_2));
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }

    }
    private void NormalAttackersWonEffect(InteractionState state) {
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2);
        }

        DefenderGroup newDefenders = interactable.tileLocation.areaOfTile.GetFirstDefenderGroup();
        if (newDefenders == null) {
            interactable.tileLocation.areaOfTile.Death();
            //Log area is cleared
        }
        characterInvolved.currentParty.GoHome(() => characterInvolved.currentParty.DisbandParty());
    }
    private void NormalAttackersLostEffect(InteractionState state) {
        for (int i = 0; i < _combat.charactersSideA.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(_combat.charactersSideA[i], _combat.charactersSideA[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }
        for (int i = 0; i < _combat.charactersSideB.Count; i++) {
            state.descriptionLog.AddToFillers(_combat.charactersSideB[i], _combat.charactersSideB[i].name, LOG_IDENTIFIER.CHARACTER_LIST_2);
        }
    }
    private void NormalAttackersNoDefenseEffect(InteractionState state) {
        for (int i = 0; i < characterInvolved.currentParty.characters.Count; i++) {
            state.descriptionLog.AddToFillers(characterInvolved.currentParty.characters[i], characterInvolved.currentParty.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1);
            state.AddLogFiller(new LogFiller(characterInvolved.currentParty.characters[i], characterInvolved.currentParty.characters[i].name, LOG_IDENTIFIER.CHARACTER_LIST_1));
        }

        interactable.tileLocation.areaOfTile.Death();
        characterInvolved.currentParty.GoHome(() => characterInvolved.currentParty.DisbandParty());
    }
    #endregion
}
