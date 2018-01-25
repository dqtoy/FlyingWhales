using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RestAction : QuestAction {

    List<ECS.Character> charactersToRest;

    public RestAction(Quest quest) : base(quest) {}

    #region overrides
    public override void ActionDone(QUEST_ACTION_RESULT result) {
        Messenger.RemoveListener("OnDayEnd", Rest);
        base.ActionDone(result);
    }
    #endregion

    public void StartDailyRegeneration() {
        charactersToRest = new List<ECS.Character>();
        if (actionDoer.party != null) {
            charactersToRest.AddRange(actionDoer.party.partyMembers);
        } else {
            charactersToRest.Add(actionDoer);
        }
        Messenger.AddListener("OnDayEnd", Rest);
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
            ActionDone(QUEST_ACTION_RESULT.SUCCESS);
        }
    }
}
