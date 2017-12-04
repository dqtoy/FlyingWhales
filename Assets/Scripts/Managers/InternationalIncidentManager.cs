using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InternationalIncidentManager : MonoBehaviour {
	public static InternationalIncidentManager Instance;

	void Awake(){
		Instance = this;
	}

	void Start () {
		GameDate newDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		newDate.AddDays (7);
		SchedulingManager.Instance.AddEntry (newDate, () => RandomInternationalIncident ());
	}
	

	private void RandomInternationalIncident(){
		Dictionary<Kingdom, int> kingdomWeightDict = new Dictionary<Kingdom, int> ();
		Dictionary<SharedKingdomRelationship, int> incidentDict = new Dictionary<SharedKingdomRelationship, int> ();
		incidentDict.Add (new SharedKingdomRelationship(null, null), 200);

		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom kingdom = KingdomManager.Instance.allKingdoms [i];
			int weight = 0;
			if(kingdom.king.otherTraits.Contains(TRAIT.PACIFIST)){
				weight -= 20;
			}
			if(kingdom.king.otherTraits.Contains(TRAIT.DIPLOMATIC)){
				weight -= 20;
			}
			if(kingdom.king.otherTraits.Contains(TRAIT.HOSTILE)){
				weight += 20;
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
				if(kr.sharedRelationship.isAdjacent && !incidentDict.ContainsKey(kr.sharedRelationship)){
					totalWeight = 0;
					totalWeight += 50;

					totalWeight -= 2 * kr.sharedRelationship.kr1.totalLike;
					totalWeight -= 2 * kr.sharedRelationship.kr2.totalLike;

					totalWeight += kingdomWeightDict [kr.sourceKingdom];
					totalWeight += kingdomWeightDict [kr.targetKingdom];

					if(totalWeight < 0){
						totalWeight = 0;
					}

					incidentDict.Add (kr.sharedRelationship, totalWeight);
				}
			}
		}

		if(incidentDict.Count > 1){
			SharedKingdomRelationship sharedKingdomRelationship = Utilities.PickRandomElementWithWeights<SharedKingdomRelationship> (incidentDict);
			if(sharedKingdomRelationship.kr1 != null && sharedKingdomRelationship.kr2 != null){
				sharedKingdomRelationship.kr1.sourceKingdom.StartInternationalIncident (sharedKingdomRelationship.kr2.sourceKingdom, "random");
			}
		}

		if(KingdomManager.Instance.allKingdoms.Count > 1){
			GameDate newDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			newDate.AddDays (7);
			SchedulingManager.Instance.AddEntry (newDate, () => RandomInternationalIncident ());
		}
	}
}
