using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using ECS;

public class CombatRoom {
    private List<ICombatInitializer> _combatants;
    private ILocation _location;

    #region getters/setters
    public List<ICombatInitializer> combatants {
        get { return _combatants; }
    }
    public ILocation location {
        get { return _location; }
    }
    #endregion

    public CombatRoom(ICombatInitializer combatant1, ICombatInitializer combatant2, ILocation location) {
        _combatants = new List<ICombatInitializer>();
        _location = location;
        AddCombatant(combatant1);
        AddCombatant(combatant2);
        Messenger.AddListener(Signals.DAY_START, CheckForCombat);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }

    public void AddCombatant(ICombatInitializer combatant) {
        if (!_combatants.Contains(combatant)) {
            _combatants.Add(combatant);
            CombatManager.Instance.SetCombatantCombatRoom(combatant, this);
            if (combatant.avatar != null) {
                combatant.avatar.PauseMovement();
            }
        }
    }

    public void RemoveCombatant(ICombatInitializer combatant) {
        if (!_combatants.Remove(combatant)) {
            throw new System.Exception("Could not remove combatant " + combatant.mainCharacter.name + " " + combatant.GetType().ToString() + " from the combat room!");
        }
        CombatManager.Instance.RemoveCombatant(combatant);
        if (combatant.avatar != null) {
            combatant.avatar.ResumeMovement();
        }
        if (_combatants.Count == 0) {
            //ClearCombatRoom();
            //There are no more combatants in this room
            Messenger.RemoveListener(Signals.DAY_START, CheckForCombat);
            Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
    }

    //private void ClearCombatRoom() {
    //    for (int i = 0; i < _combatants.Count; i++) {
    //        ICombatInitializer combatant = _combatants[i];
    //        CombatManager.Instance.RemoveCombatant(combatant);
    //        if (combatant.avatar != null) {
    //            combatant.avatar.ResumeMovement();
    //        }
    //    }
    //    _combatants.Clear();
    //    Messenger.RemoveListener(Signals.DAY_START, CheckForCombat);
    //    Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    //}

    private void CheckForCombat() {
        //At the start of each day:
        if (HasHostilities() && HasCombatInitializers()){
            PairUpCombats();
        }
        ContinueMovement();
    }
    private void PairUpCombats() {
        List<ICombatInitializer> combatInitializers = GetCharactersByCombatPriority();
        if (combatInitializers != null) {
            for (int i = 0; i < combatInitializers.Count; i++) {
                ICombatInitializer currInitializer = combatInitializers[i];
                Debug.Log("Finding combat pair for " + currInitializer.mainCharacter.name);
                if (currInitializer.isInCombat) {
                    continue; //this current group is already in combat, skip it
                }
                //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<ICombatInitializer> combatGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (combatGroups.Count > 0) {
                    ICombatInitializer chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<ICombatInitializer> neutralGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (neutralGroups.Count > 0) {
                    ICombatInitializer chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
                List<ICombatInitializer> stealthGroups = new List<ICombatInitializer>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (stealthGroups.Count > 0) {
                    //The chance of initiating combat is 35%
                    if (Random.Range(0, 100) < 35) {
                        ICombatInitializer chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                        StartCombatBetween(currInitializer, chosenEnemy);
                        continue; //the attacking group has found an enemy! skip to the next group
                    }
                }
            }
        }
    }
    private List<ICombatInitializer> GetCharactersByCombatPriority() {
        //if (_combatants.Count <= 0) {
        //    return null;
        //}
        //return _combatants.Where(x => x.currentAction.combatPriority > 0).OrderByDescending(x => x.currentAction.combatPriority).ToList();
        return null;
    }
    private List<ICombatInitializer> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, ICombatInitializer except = null) {
        List<ICombatInitializer> groups = new List<ICombatInitializer>();
        for (int i = 0; i < _combatants.Count; i++) {
            ICombatInitializer currGroup = _combatants[i];
            if (notInCombatOnly) {
                if (currGroup.isInCombat) {
                    continue; //skip
                }
            }
            if (currGroup.GetCurrentStance() == stance) {
                if (except != null && currGroup == except) {
                    continue; //skip
                }
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    private bool HasCombatInitializers() {
        for (int i = 0; i < _combatants.Count; i++) {
            ICombatInitializer currChar = _combatants[i];
            //if (currChar.currentAction.combatPriority > 0) {
            //    return true;
            //}
        }
        return false;
    }
    private bool HasHostilities() {
        for (int i = 0; i < _combatants.Count; i++) {
            ICombatInitializer currItem = _combatants[i];
            for (int j = 0; j < _combatants.Count; j++) {
                ICombatInitializer otherItem = _combatants[j];
                if (currItem != otherItem) {
                    if (currItem.IsHostileWith(otherItem)) {
                        return true; //there are characters with hostilities
                    }
                }
            }
        }
        return false;
    }
    private void StartCombatBetween(ICombatInitializer combatant1, ICombatInitializer combatant2) {
        Combat combat = new Combat(combatant1, combatant2, _location);
        combatant1.SetIsInCombat(true);
        combatant2.SetIsInCombat(true);
        string combatant1Name = string.Empty;
        string combatant2Name = string.Empty;
        if (combatant1 is Party) {
            combatant1Name = (combatant1 as Party).name;
            combat.AddCharacters(SIDES.A, (combatant1 as Party).partyMembers);
        } else {
            combatant1Name = (combatant1 as Character).name;
            combat.AddCharacter(SIDES.A, combatant1 as Character);
        }
        if (combatant2 is Party) {
            combatant2Name = (combatant2 as Party).name;
            combat.AddCharacters(SIDES.B, (combatant2 as Party).partyMembers);
        } else {
            combatant2Name = (combatant2 as Character).name;
            combat.AddCharacter(SIDES.B, combatant2 as Character);
        }
        Log combatLog = new Log(GameManager.Instance.Today(), "General", "Combat", "start_combat");
        combatLog.AddToFillers(combatant1, combatant1Name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        combatLog.AddToFillers(combat, " fought with ", LOG_IDENTIFIER.COMBAT);
        combatLog.AddToFillers(combatant2, combatant2Name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //if (true) {

        //}
        //AddHistory(combatLog);
        combatant1.mainCharacter.AddHistory(combatLog);
        combatant2.mainCharacter.AddHistory(combatLog);
        Debug.Log("Starting combat between " + combatant1Name + " and  " + combatant2Name);

        //this.specificLocation.SetCurrentCombat(combat);
        CombatThreadPool.Instance.AddToThreadPool(combat);
    }
    private void ContinueMovement() {
        List<ICombatInitializer> nonCombating = _combatants.Where(x => !x.isInCombat).ToList();
        //check all combatants that have not been paired up, if they havent been paired up, resume their movement
        for (int i = 0; i < nonCombating.Count; i++) {
            ICombatInitializer currCombatant = nonCombating[i];
            RemoveCombatant(currCombatant);
        }
    }

    private void OnCharacterDied(ECS.Character character) {
        //check combatants, and remove if the passed character matches any
        for (int i = 0; i < _combatants.Count; i++) {
            ICombatInitializer currCombatant = _combatants[i];
            if (currCombatant is ECS.Character) {
                if (character.id == (currCombatant as ECS.Character).id) {
                    RemoveCombatant(currCombatant);
                    break;
                }
            } else if (currCombatant is Party) {
                if (character.id == (currCombatant as Party).partyLeader.id) {
                    RemoveCombatant(currCombatant);
                    break;
                }
            }
        }
    }
}
