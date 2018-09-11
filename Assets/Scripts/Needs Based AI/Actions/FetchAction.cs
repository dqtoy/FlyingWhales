using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchAction : CharacterAction {

    private int fetchCooldown;

    private const int Obtain_Item_Chance = 100;

    public FetchAction() : base(ACTION_TYPE.FETCH) { }

    #region overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if (fetchCooldown > 0) {
            fetchCooldown--;

            if (fetchCooldown == 0) {
                fetchCooldown = GetFetchCooldown();
                string fetchLog = string.Empty;
                //check if chance to obtain item is met
                if (Random.Range(0, 100) < Obtain_Item_Chance) {
                    //then check what monster party the squad will encounter
                    MonsterSet set = (targetObject.specificLocation as MonsterSpawnerLandmark).monsterChoices;
                    MonsterPartyComponent chosenParty = set.parties[Random.Range(0, set.parties.Length)];
                    fetchLog += "Encountered party " + chosenParty.name;
                    for (int i = 0; i < chosenParty.monsters.Length; i++) {
                        TextAsset currMonsterAsset = chosenParty.monsters[i];
                        Monster monster = MonsterManager.Instance.monstersDictionary[currMonsterAsset.name];
                        List<string> droppedItems = monster.GetRandomDroppedItems(); //then check what item drops the monster party will drop
                        fetchLog += "\nItems dropped from " + monster.name + ":";
                        for (int j = 0; j < droppedItems.Count; j++) {
                            string droppedItemName = droppedItems[j];
                            Item item = ItemManager.Instance.allItems[droppedItemName].CreateNewCopy();
                            (party.mainCharacter as Character).PickupItem(item, false);
                            fetchLog += "\n" + item.itemName;
                        }
                    }
                } else {
                    fetchLog = party.name + " did not obtain any item.";
                }
                Debug.Log("[" + GameManager.Instance.Today().GetDayAndTicksString() + "]" + party.name + "Fetch Log: \n" + fetchLog);
                CheckForQuestCompletion(party);
            }
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
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        fetchCooldown = GetFetchCooldown();
    }
    #endregion

    private int GetFetchCooldown() {
        return Random.Range(3, 7); //Every 3-6 ticks the party has a chance to obtain an item.
    }

    private void CheckForQuestCompletion(CharacterParty party) {
        if (party.actionData.questAssociatedWithCurrentAction != null) {
            if (party.actionData.questAssociatedWithCurrentAction is FetchQuest) {
                (party.actionData.questAssociatedWithCurrentAction as FetchQuest).CheckIfQuestIsCompleted();
            }
        }
    }
}
