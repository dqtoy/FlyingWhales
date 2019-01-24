/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class CharacterRole {
	protected Character _character;
    protected CHARACTER_ROLE _roleType;
	protected bool _isRemoved;

    #region getters/setters
    public CHARACTER_ROLE roleType {
        get { return _roleType; }
    }
	public Character character{
		get { return _character; }
	}
	public bool isRemoved {
		get { return _isRemoved; }
	}
    #endregion

    public CharacterRole(Character character){
		_character = character;
		_isRemoved = false;
    }
    
    #region Virtuals
    public virtual void DeathRole(){
		_isRemoved = true;
        //_character.onDailyAction -= StartDepletion;
    }
	public virtual void ChangedRole(){
		_isRemoved = true;
        //_character.onDailyAction -= StartDepletion;
    }
    public virtual void OnAssignRole() {
        //_character.onDailyAction += StartDepletion;
    }
    #endregion

}
