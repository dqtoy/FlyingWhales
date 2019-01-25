
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchAction : CharacterAction {

    private const int Obtain_Item_Chance = 100;


    public FetchAction() : base(ACTION_TYPE.FETCH) { }

    #region overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if (party is CharacterParty) {
            //FetchQuest fetchQuest = (party.owner.currentQuest as FetchQuest);
            //if (fetchQuest.fetchCooldown > 0) {
            //    fetchQuest.AdjustFetchCooldown(-1);

            //    if (fetchQuest.fetchCooldown == 0) {
            //        fetchQuest.SetFetchCooldown(GetFetchCooldown(party as CharacterParty));
            //        string fetchLog = string.Empty;
            //        //check if chance to obtain item is met
            //        if (Random.Range(0, 100) < Obtain_Item_Chance) {
            //            //then check what monster party the squad will encounter
            //            MonsterSet set = (targetObject.specificLocation as MonsterSpawnerLandmark).monsterChoices;
            //            MonsterPartyComponent chosenParty = set.parties[Random.Range(0, set.parties.Length)];
            //            fetchLog += "Encountered party " + chosenParty.name;
            //            for (int i = 0; i < chosenParty.monsters.Length; i++) {
            //                TextAsset currMonsterAsset = chosenParty.monsters[i];
            //                Monster monster = MonsterManager.Instance.monstersDictionary[currMonsterAsset.name];
            //                List<string> droppedItems = monster.GetRandomDroppedItems(); //then check what item drops the monster party will drop
            //                fetchLog += "\nItems dropped from " + monster.name + ":";
            //                for (int j = 0; j < droppedItems.Count; j++) {
            //                    string droppedItemName = droppedItems[j];
            //                    Item item = ItemManager.Instance.allItems[droppedItemName].CreateNewCopy();
            //                    party.mainCharacter.PickupItem(item, false);
            //                    fetchLog += "\n" + item.itemName;
            //                }
            //            }
            //        } else {
            //            fetchLog = party.name + " did not obtain any item.";
            //        }
            //        Debug.Log("[" + GameManager.Instance.continuousDays + "]" + party.name + "Fetch Log: \n" + fetchLog);
            //        CheckForQuestCompletion(party as CharacterParty);
            //    }
            //}
        }
        //TODO: Add Item Obtaining from monster drops
        //GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        FetchAction action = new FetchAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        //FetchQuest fetchQuest = (iparty.owner.currentQuest as FetchQuest);
        //fetchQuest.SetFetchCooldown(GetFetchCooldown(iparty as CharacterParty));
    }
    #endregion

    private int GetFetchCooldown(CharacterParty party) {
        if (party.actionData.isBeingAssistedByPlayer) {
            return Random.Range(3, 6);
        }
        return Random.Range(4, 8); //Every 4-7 ticks the party has a chance to obtain an item.
    }

    private void CheckForQuestCompletion(CharacterParty party) {
        if (party.actionData.questAssociatedWithCurrentAction != null) {
            if (party.actionData.questAssociatedWithCurrentAction is FetchQuest) {
                (party.actionData.questAssociatedWithCurrentAction as FetchQuest).CheckIfQuestIsCompleted();
            }
        }
    }
}
