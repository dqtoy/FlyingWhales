using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraterBeast : CharacterRole {
	private int _numOfSlyxesCanBeCalled;

	public int numOfSlyxesCanBeCalled{
		get { return _numOfSlyxesCanBeCalled; }
	}

	public CraterBeast(ECS.Character character): base (character) {
		_roleType = CHARACTER_ROLE.CRATER_BEAST;
		_allowedRoadTypes = new List<ROAD_TYPE>() {
			ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
		};
		_canPassHiddenRoads = true;

//		_roleTasks.Add (new DoNothing (this._character));
		_roleTasks.Clear();
		_roleTasks.Add (new Patrol (this._character));
		_roleTasks.Add (new CallSlyxes (this._character, 0));
		_roleTasks.Add (new SiphonSlyx (this._character, 5));
		_roleTasks.Add (new CommandInfection (this._character, 0));


		_defaultRoleTask = _roleTasks [0];
		_numOfSlyxesCanBeCalled = 0;
		_character.SetDoesNotTakePrisoners (true);
		Messenger.AddListener ("SlyxTransform", ACharacterBecameASlyx);
	}

	#region Overrides
	public override void DeathRole (){
		base.DeathRole ();
		_character.SetDoesNotTakePrisoners (false);
		Messenger.RemoveListener ("SlyxTransform", ACharacterBecameASlyx);
	}
	public override void ChangedRole (){
		base.ChangedRole ();
		_character.SetDoesNotTakePrisoners (false);
		Messenger.RemoveListener ("SlyxTransform", ACharacterBecameASlyx);
	}
	#endregion

	private void ACharacterBecameASlyx(){
		_numOfSlyxesCanBeCalled += 1;
	}

	internal void CallAllSlyxes(){
		Messenger.Broadcast<ILocation> ("CallSlyx", _character.specificLocation);
		_numOfSlyxesCanBeCalled = 0;
	}
}

