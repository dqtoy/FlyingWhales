using UnityEngine;
using System.Collections;
using System.Linq;

public class ShowCompatibility : MonoBehaviour {

	void OnHover(bool isOver){
		if (isOver) {
			if (UIManager.Instance.currentlyShowingKingdom != null) {
				UIManager.Instance.ShowSmallInfo (this.KingdomCompatibilities (), this.transform);
			}
		} else {
			UIManager.Instance.HideSmallInfo ();
		}
	}

	private string KingdomCompatibilities(){
		string info = string.Empty;
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(UIManager.Instance.currentlyShowingKingdom.id != KingdomManager.Instance.allKingdoms[i].id){
				info += KingdomManager.Instance.allKingdoms [i].name + ": " + GetCompatibilityValue (UIManager.Instance.currentlyShowingKingdom, KingdomManager.Instance.allKingdoms [i]);
				info += "\n";
			}
		}
		return info;
	}

	private int GetCompatibilityValue(Kingdom kingdom1, Kingdom kingdom2){
		int compatibilityValue = 0;
		int[] firstKingdomHoroscope = kingdom1.horoscope.Concat (kingdom1.king.horoscope).ToArray();
		int[] secondKingdomHoroscope = kingdom2.horoscope.Concat (kingdom2.king.horoscope).ToArray();
		int count = firstKingdomHoroscope.Length;
		for(int i = 0; i < count; i++){
			if(firstKingdomHoroscope[i] == secondKingdomHoroscope[i]){
				compatibilityValue += 1;
			}
		}
		return compatibilityValue;
	}
}
