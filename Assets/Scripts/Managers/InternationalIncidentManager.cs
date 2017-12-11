using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InternationalIncidentManager : MonoBehaviour {
	public static InternationalIncidentManager Instance;

	private SharedKingdomRelationship defaultSharedKR;

	void Awake(){
		Instance = this;
	}

	void Start () {
		defaultSharedKR = new SharedKingdomRelationship (null, null);
		ScheduleIncident ();
	}
	
	private void ScheduleIncident(){
		GameDate newDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		newDate.AddDays (7);
		SchedulingManager.Instance.AddEntry (newDate, () => RandomInternationalIncident ());
	}
	private void RandomInternationalIncident(){
		Debug.Log ("------------------------------------- RANDOM INTERNATIONAL INCIDENT " + GameManager.Instance.month.ToString () + "/" + GameManager.Instance.days.ToString () + "/" + GameManager.Instance.year.ToString () + " ----------------------------------");
		Dictionary<Kingdom, int> kingdomWeightDict = new Dictionary<Kingdom, int> ();
		Dictionary<SharedKingdomRelationship, int> incidentDict = new Dictionary<SharedKingdomRelationship, int> ();

		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom kingdom = KingdomManager.Instance.allKingdoms [i];
			int weight = 0;
			for (int j = 0; j < kingdom.king.allTraits.Count; j++) {
				weight += kingdom.king.allTraits [j].GetRandomInternationalIncidentWeight ();
			}

			if(kingdom.stability < 0){
				weight -= kingdom.stability;
			}
			kingdomWeightDict.Add (kingdom, weight);
		}

		int totalWeight = 0;
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom kingdom = KingdomManager.Instance.allKingdoms [i];
			foreach (KingdomRelationship kr in kingdom.relationships.Values) {
				if(kr.sharedRelationship.isAdjacent && !kr.sharedRelationship.isAtWar && !incidentDict.ContainsKey(kr.sharedRelationship)){
					totalWeight = 0;
					totalWeight += 50;

					totalWeight -= 2 * kr.sharedRelationship.kr1.totalLike;
					totalWeight -= 2 * kr.sharedRelationship.kr2.totalLike;

					totalWeight += kingdomWeightDict [kr.sourceKingdom];
					totalWeight += kingdomWeightDict [kr.targetKingdom];

					totalWeight -= (25 * kr.sharedRelationship.internationalIncidents.Count);

					if(totalWeight < 0){
						totalWeight = 0;
					}

					incidentDict.Add (kr.sharedRelationship, totalWeight);
					Debug.Log ("PAIR (" + kr.sourceKingdom.name + " and " + kr.targetKingdom.name + ") : " + totalWeight.ToString());
				}
			}
		}

		int noIntlIncidentsWeight = incidentDict.Count * 2000;
		incidentDict.Add (defaultSharedKR, noIntlIncidentsWeight);
		Debug.Log ("NO INTERNATIONAL INCIDENTS: " + noIntlIncidentsWeight.ToString());

		if(incidentDict.Count > 1){
			SharedKingdomRelationship sharedKingdomRelationship = Utilities.PickRandomElementWithWeights<SharedKingdomRelationship> (incidentDict);
			if(sharedKingdomRelationship.kr1 != null && sharedKingdomRelationship.kr2 != null){
				sharedKingdomRelationship.kr1.sourceKingdom.StartInternationalIncident (sharedKingdomRelationship.kr2.sourceKingdom, "random");
			}
		}

		if(KingdomManager.Instance.allKingdoms.Count > 1){
			ScheduleIncident ();
		}
		Debug.Log ("------------------------------------- END INTERNATIONAL INCIDENT ----------------------------------");
	}
}
