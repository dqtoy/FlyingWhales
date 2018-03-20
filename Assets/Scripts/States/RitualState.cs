using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RitualState : State {

	public RitualState(CharacterTask parentTask): base (parentTask, STATE.RITUAL){

	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }

		//TODO: Change this to random ritual effects
		DoMeteorStrike ();
		return true;
	}
	#endregion

	private void DoMeteorStrike(){
		//Step 1 - Choose region
		List<Region> targetRegions = new List<Region> ();
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region currRegion = GridMap.Instance.allRegions [i];
			if(currRegion.centerOfMass.landmarkOnTile != null && currRegion.centerOfMass.landmarkOnTile.owner != null && currRegion.centerOfMass.landmarkOnTile.specificLandmarkType != LANDMARK_TYPE.CRATER){
				targetRegions.Add (currRegion);
			}
		}
		if(targetRegions.Count <= 0){
			return;
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
		Settlement settlement = (Settlement)landmark;
		settlement.tileLocation.RuinStructureOnTile (false);
		settlement.ChangeLandmarkType (LANDMARK_TYPE.CRATER);

		Log meteorCrashLog = new Log(GameManager.Instance.Today(), "Quests", "MeteorStrike", "meteor_crash");
		meteorCrashLog.AddToFillers(landmark, landmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);

		if(_parentTask is DoRitual){
			((DoRitual)_parentTask).ritualStones.AddHistory(meteorCrashLog);
		}
		landmark.AddHistory(meteorCrashLog);
	}
}
