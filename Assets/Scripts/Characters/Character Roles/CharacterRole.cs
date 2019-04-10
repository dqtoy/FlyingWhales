/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class CharacterRole {
    public static readonly CharacterRole NONE = new CharacterRole(CHARACTER_ROLE.NONE, "Any", null);
    public static readonly CharacterRole CIVILIAN = new Civilian();
    public static readonly CharacterRole PLAYER = new PlayerRole();
    public static readonly CharacterRole BANDIT = new Bandit();
    public static readonly CharacterRole LEADER = new Leader();
    public static readonly CharacterRole BEAST = new Beast();
    public static readonly CharacterRole NOBLE = new Noble();
    public static readonly CharacterRole SOLDIER = new Soldier();
    public static readonly CharacterRole ADVENTURER = new Adventurer();
    public static readonly CharacterRole MINION = new MinionRole();
    public static readonly CharacterRole[] ALL = new CharacterRole[] { NONE, CIVILIAN, PLAYER, BANDIT, LEADER, BEAST, NOBLE, SOLDIER, ADVENTURER, MINION };

    public string classNameOrIdentifier { get; protected set; }
    public string name { get; protected set; }
    public CHARACTER_ROLE roleType { get; protected set; }
    public INTERACTION_CATEGORY[] interactionCategories { get; protected set; }
    public INTERACTION_TYPE[] allowedInteractions { get; protected set; }
    public virtual int reservedSupply { get { return 0; } }

    protected CharacterRole(CHARACTER_ROLE roleType, string classNameOrIdentifier, INTERACTION_CATEGORY[] interactionCategories) {
        this.roleType = roleType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(roleType.ToString());
        this.classNameOrIdentifier = classNameOrIdentifier;
        this.interactionCategories = interactionCategories;
    }

    public static CharacterRole GetRoleByRoleType(CHARACTER_ROLE roleType) {
        for (int i = 0; i < ALL.Length; i++) {
            if(ALL[i].roleType == roleType) {
                return ALL[i];
            }
        }
        throw new Exception("There's no character role instance for " + roleType.ToString());
    }

    #region Virtuals
    public virtual void OnDeath(Character character) { }
    public virtual void OnAssign(Character character) { }
    public virtual void OnChange(Character character) { }
    public virtual void AddRoleWorkPlansToCharacterWeights(WeightedDictionary<INTERACTION_TYPE> weights) { }
    public virtual GoapPlan PickRoleWorkPlanFromCharacterWeights(INTERACTION_TYPE pickedActionType, Character actor) { return null; }
    #endregion
    //protected Character _character;
    //   protected CHARACTER_ROLE _roleType;
    //protected bool _isRemoved;

    //   #region getters/setters
    //   public CHARACTER_ROLE roleType {
    //       get { return _roleType; }
    //   }
    //public Character character{
    //	get { return _character; }
    //}
    //public bool isRemoved {
    //	get { return _isRemoved; }
    //}
    //   #endregion

    //   public CharacterRole(Character character){
    //	_character = character;
    //	_isRemoved = false;
    //   }

    //   #region Virtuals
    //   public virtual void DeathRole(){
    //	_isRemoved = true;
    //       //_character.onDailyAction -= StartDepletion;
    //   }
    //public virtual void ChangedRole(){
    //	_isRemoved = true;
    //       //_character.onDailyAction -= StartDepletion;
    //   }
    //   public virtual void OnAssignRole() {
    //       //_character.onDailyAction += StartDepletion;
    //   }
    //   #endregion
}
