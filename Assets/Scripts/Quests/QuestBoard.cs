using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestBoard {

	public BaseLandmark owner { get; private set; }

    public QuestBoard(BaseLandmark owner) {
        this.owner = owner;
    }

    public Quest GenerateQuestForCharacter(ECS.Character character) {
        if (character.role.roleType != CHARACTER_ROLE.HERO) {
            throw new System.Exception("Non hero character " + character.name + " is trying to get a quest!");
        }
        //get all landmarks that are part of the area that can spawn monsters
        //pick the nearest one
        List<MonsterSpawnerLandmark> landmarks = owner.tileLocation.areaOfTile.landmarks
            .Where(x => x is MonsterSpawnerLandmark && x.tileLocation.GetDistanceFrom(owner.tileLocation) != -1)
            .OrderBy(x => x.tileLocation.GetDistanceFrom(owner.tileLocation))
            .Select(x => x as MonsterSpawnerLandmark)
            .ToList();
        if (landmarks.Count == 0) {
            Debug.LogWarning("Could not generate fetch quest for " + character.name + " because there are no monster spawner landmarks in area!");
        } else {
            MonsterSpawnerLandmark chosenLandmark = landmarks[0];
            //once landmark has been picked. Pick a monster the character must encounter
            //MonsterPartyComponent chosenSet = chosenLandmark.monsterChoices.parties[Random.Range(0, chosenLandmark.monsterChoices.parties.Length)];
            //TextAsset chosenMonsterAsset = chosenSet.monsters[Random.Range(0, chosenSet.monsters.Length)];
            //Monster chosenMonster = MonsterManager.Instance.monstersDictionary[chosenMonsterAsset.name];
            //List<string> itemChoices = chosenMonster.itemDropsLookup.Where(x => x.Value > 0f).Select(x => x.Key).ToList();
            //string chosenItem = itemChoices[Random.Range(0, itemChoices.Count)]; //Then pick an item that that monster can drop
            int amount = Random.Range(2, 5); //randomize the amount of items needed for now
            FetchQuest fetchQuest = new FetchQuest(chosenLandmark, "Misc1", amount);
            return fetchQuest;
        }
        return null;
    }
}
