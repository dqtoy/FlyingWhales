using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Villain : CharacterRole {

	public Villain(ECS.Character character): base (character) {
		_roleType = CHARACTER_ROLE.VILLAIN;
		_allowedRoadTypes = new List<ROAD_TYPE>() {
			ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
		};
		_canPassHiddenRoads = true;

		_allowedQuestAlignments = new List<ACTION_ALIGNMENT>() {
			ACTION_ALIGNMENT.VILLAINOUS,
			ACTION_ALIGNMENT.LAWFUL,
			ACTION_ALIGNMENT.UNLAWFUL
		};

		//_roleTasks.Add (new DoNothing (this._character));
		//_roleTasks.Add (new Rest (this._character));
		//_roleTasks.Add (new ExploreTile (this._character, 5));
		//_roleTasks.Add (new UpgradeGear (this._character));
		//_roleTasks.Add (new MoveTo (this._character));
		//_roleTasks.Add (new TakeQuest (this._character));
  //      _roleTasks.Add (new Attack (this._character, 10));
		//_roleTasks.Add (new Patrol (this._character, 10));

		//_defaultRoleTask = _roleTasks [1];

        SetFood(1000);
        SetEnergy(1000);
        SetJoy(600);
        SetPrestige(400);

        Messenger.AddListener("OnDayEnd", StartDepletion);
	}

    #region Overrides
    public override void DeathRole() {
        base.DeathRole();
        Messenger.RemoveListener("OnDayEnd", StartDepletion);
    }
    public override void ChangedRole() {
        base.ChangedRole();
        Messenger.RemoveListener("OnDayEnd", StartDepletion);
    }
    #endregion

    private void StartDepletion() {
        DepleteFood();
        DepleteEnergy();
        DepleteJoy();
        DepletePrestige();
    }

}
