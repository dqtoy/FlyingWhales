using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class RestAction : TaskAction {

    List<ECS.Character> charactersToRest;

    public RestAction(CharacterTask task) : base(task) {}

    #region overrides
    public override void InititalizeAction(ECS.Character target) {
        base.InititalizeAction(target);
        charactersToRest = new List<ECS.Character>();
        if (target.party != null) {
            charactersToRest.AddRange(target.party.partyMembers);
        } else {
            charactersToRest.Add(target);
        }
        for (int i = 0; i < charactersToRest.Count; i++) {
            charactersToRest[i].AddHistory("Taking a rest.");
        }
    }
    public override void ActionDone(TASK_ACTION_RESULT result) {
        Messenger.RemoveListener("OnDayEnd", Rest);
        base.ActionDone(result);
    }
    #endregion

  //  public void StartDailyRegeneration() {
  //      charactersToRest = new List<ECS.Character>();
  //      if (actionDoer.party != null) {
  //          charactersToRest.AddRange(actionDoer.party.partyMembers);
  //      } else {
  //          charactersToRest.Add(actionDoer);
  //      }
		//for (int i = 0; i < charactersToRest.Count; i++) {
		//	charactersToRest [i].AddHistory ("Taking a rest.");
		//}
  //      Messenger.AddListener("OnDayEnd", Rest);
  //  }

    public void RestIndefinitely() {
        for (int i = 0; i < charactersToRest.Count; i++) {
            ECS.Character currCharacter = charactersToRest[i];
            currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
        }
    }
    public void Rest() {
        for (int i = 0; i < charactersToRest.Count; i++) {
            ECS.Character currCharacter = charactersToRest[i];
            currCharacter.AdjustHP(currCharacter.raceSetting.restRegenAmount);
        }
        CheckIfCharactersAreFullyRested(charactersToRest);
    }

    private void CheckIfCharactersAreFullyRested(List<ECS.Character> charactersToRest) {
        bool allCharactersRested = true;
        for (int i = 0; i < charactersToRest.Count; i++) {
            ECS.Character currCharacter = charactersToRest[i];
            if (!currCharacter.IsHealthFull()) {
                allCharactersRested = false;
                break;
            }
        }
        if (allCharactersRested) {
            ActionDone(TASK_ACTION_RESULT.SUCCESS);
        }
    }
}
