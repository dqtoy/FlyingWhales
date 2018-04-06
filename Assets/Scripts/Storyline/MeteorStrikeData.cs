using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class MeteorStrikeData : StorylineData {

	private PsytoxinCure _psytoxinCureQuest;
	private BaseLandmark _ritualStones;

	#region getters/setters
	public PsytoxinCure psytoxinCureQuest{
		get { return _psytoxinCureQuest; }
	}
	#endregion

	public MeteorStrikeData(BaseLandmark ritualStones) : base(STORYLINE.METEOR_STRIKE) {
		_ritualStones = ritualStones;
	}

	#region overrides
	public override bool InitialStorylineSetup() {
		//Step 1 - Choose region
		List<Region> targetRegions = new List<Region> ();
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region currRegion = GridMap.Instance.allRegions [i];
			if(currRegion.centerOfMass.landmarkOnTile != null && currRegion.centerOfMass.landmarkOnTile.owner != null && currRegion.centerOfMass.landmarkOnTile.specificLandmarkType != LANDMARK_TYPE.CRATER){
				targetRegions.Add (currRegion);
			}
		}
		if(targetRegions.Count <= 0){
			return false;
		}
		Region chosenRegion = targetRegions[UnityEngine.Random.Range(0, targetRegions.Count)];

		//Step 2 - Destroy All Life in Region
		for (int i = 0; i < chosenRegion.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = chosenRegion.allLandmarks [i];
			if(currLandmark.civilians > 0){
				currLandmark.KillAllCivilians ();
			}
		}
		if(Messenger.eventTable.ContainsKey("RegionDeath")){
			Messenger.Broadcast<Region> ("RegionDeath", chosenRegion);
		}

		//Step 3 - Initialize Crater
		BaseLandmark landmark = chosenRegion.centerOfMass.landmarkOnTile;
		Settlement settlement = landmark as Settlement;
		settlement.tileLocation.RuinStructureOnTile (false);
		settlement.ChangeLandmarkType (LANDMARK_TYPE.CRATER);

		Log meteorCrashLog = new Log(GameManager.Instance.Today(), "Quests", "MeteorStrike", "meteor_crash");
		meteorCrashLog.AddToFillers(landmark, landmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);

		_ritualStones.AddHistory(meteorCrashLog);
		landmark.AddHistory(meteorCrashLog);


		//Crater initialization
		settlement.SetNumOfPsytoxinated (0);
		LandmarkManager.Instance.craterLandmark = settlement;
		Messenger.AddListener ("Psytoxinated", settlement.ListenPsytoxinated);
		Messenger.AddListener ("Unpsytoxinated", settlement.ListenUnpsytoxinated);

		ECS.CharacterSetup charSetup = ECS.CombatManager.Instance.GetBaseCharacterSetup("Dehkbrug");
		ECS.Character nihvram = CharacterManager.Instance.CreateNewCharacter(charSetup.optionalRole, charSetup);
		nihvram.SetCharacterColor (Color.red);
		nihvram.SetName ("Nihvram");

		nihvram.SetHome(settlement);
		settlement.AddCharacterToLocation(nihvram);
		nihvram.DetermineAction();

		settlement.SpawnItemInLandmark ("Meteorite");

		if(Messenger.eventTable.ContainsKey("RegionPsytoxin")){
			Messenger.Broadcast<List<Region>> ("RegionPsytoxin", settlement.tileLocation.region.adjacentRegions);
		}
		PsytoxinCure psytoxinCure = new PsytoxinCure (QuestManager.Instance, settlement);
		QuestManager.Instance.AddQuestToAvailableQuests(psytoxinCure);
		AddRelevantQuest (psytoxinCure);

		Log craterLog = CreateLogForStoryline ("crater_description");
		craterLog.AddToFillers(nihvram, nihvram.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		Log dehkbrugLog = CreateLogForStoryline ("dehkbrug_description");
		craterLog.AddToFillers(nihvram, nihvram.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

		AddRelevantItem ("Dehkbrug", dehkbrugLog);
		AddRelevantItem (nihvram, CreateLogForStoryline ("crater_beast_description"));
		AddRelevantItem (settlement, craterLog);
		AddRelevantItem ("Psytoxin", CreateLogForStoryline ("psytoxin_description"));
		AddRelevantItem ("Slyx", CreateLogForStoryline ("slyx_description"));
		AddRelevantItem ("Meteorite", CreateLogForStoryline ("meteorite_description"));
		AddRelevantItem ("Neuroctus", CreateLogForStoryline ("neuroctus_description"));

		_storylineDescription = CreateLogForStoryline ("description");
		_storylineDescription.AddToFillers (settlement, settlement.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
		return true;
	}
	#endregion

	#region Logs
	protected override Log CreateLogForStoryline(string key) {
		return new Log(GameManager.Instance.Today(), "Storylines", "MeteorStrike", key);
	}
	#endregion
}
