/*
 The hero explores the world for both monsters and treasures. 
 They form party with Warriors to perform their duties. 
 They create Minor Roads when they explore roadless tiles.
 Place functions unique to heroes here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero : CharacterRole {

	public Hero(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.HERO;
   //     _allowedQuestTypes = new List<QUEST_TYPE>() {
   //         QUEST_TYPE.DEFEND,
   //         QUEST_TYPE.EXPLORE_TILE,
   //         QUEST_TYPE.EXPAND,
			//QUEST_TYPE.EXPEDITION,
			//QUEST_TYPE.SAVE_LANDMARK,
   //     };

   //     _allowedQuestAlignments = new List<ACTION_ALIGNMENT>() {
   //         ACTION_ALIGNMENT.HEROIC,
   //         ACTION_ALIGNMENT.LAWFUL,
   //         ACTION_ALIGNMENT.UNLAWFUL,
			//ACTION_ALIGNMENT.PEACEFUL,
   //     };

        //_roleTasks.Add (new DoNothing (this._character));
        //_roleTasks.Add (new Rest (this._character));
        //_roleTasks.Add (new ExploreTile (this._character, 5));
        //_roleTasks.Add (new UpgradeGear (this._character));
        //_roleTasks.Add (new MoveTo (this._character));
        //_roleTasks.Add (new TakeQuest (this._character));
        //_roleTasks.Add (new Attack (this._character, 10));
        //_roleTasks.Add (new Patrol (this._character, 10));

        //_defaultRoleTask = _roleTasks [1];

        //Messenger.AddListener(Signals.HOUR_ENDED, StartDepletion);
        SetFullness(100);
        SetEnergy(100);
        SetFun(60);
        SetPrestige(0);
        SetSanity(100);
        UpdateSafety();
        UpdateHappiness();
    }

    #region Overrides
    public override void DeathRole() {
        base.DeathRole();
        _character.onDailyAction -= StartDepletion;
        //Messenger.RemoveListener(Signals.HOUR_ENDED, StartDepletion);
    }
    public override void ChangedRole() {
        base.ChangedRole();
        _character.onDailyAction -= StartDepletion;
        //Messenger.RemoveListener(Signals.HOUR_ENDED, StartDepletion);
    }
    public override void OnAssignRole() {
        base.OnAssignRole();
        _character.onDailyAction += StartDepletion;
    }
    #endregion

    private void StartDepletion() {
        DepleteFullness();
        DepleteEnergy();
        DepleteFun();
        //DepleteSanity();
        DepletePrestige();
    }
}
