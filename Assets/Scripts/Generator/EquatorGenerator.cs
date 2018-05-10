using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EquatorGenerator : MonoBehaviour {
	public static EquatorGenerator Instance;

	public EQUATOR_LINE equatorLine;
	public string hexEquatorName;
	public List<GameObject> listEquator;

	void Awake(){
		Instance = this;
	}

	internal void GenerateEquator(){
		this.equatorLine = GetEquatorLine();
		this.hexEquatorName = GetHexEquatorName();
		DrawEquator();
	}

	internal void DrawEquator(){
		listEquator.Clear();

		string[] splittedName = hexEquatorName.Split(new char[]{','});
		int[] xy = {int.Parse(splittedName[0]), int.Parse(splittedName[1])};		

		if(this.equatorLine == EQUATOR_LINE.HORIZONTAL){
			for(int i = xy[0]; i < (int)GridMap.Instance.width; i++){
				listEquator.Add(GridMap.Instance.GetHex(i + "," + xy[1]));
			}
		}
		if(this.equatorLine == EQUATOR_LINE.VERTICAL){
			for(int i = xy[1]; i < (int)GridMap.Instance.height; i++){
				listEquator.Add(GridMap.Instance.GetHex(xy[0] + "," + i));
			}
		}
		if(this.equatorLine == EQUATOR_LINE.DIAGONAL_LEFT){
			if(xy[1] == 0){
				int x = xy[0];
				for(int y = xy[1]; y < GridMap.Instance.height; y++){
					listEquator.Add(GridMap.Instance.GetHex(x + "," + y));
					if(y % 2 == 1){
						x+=1;
					}
					x--;
					if(x < 0){
						x = 0;
					}
				}
			}else{
				int x = xy[0];
				for(int y = xy[1]; y >= 0; y--){
					listEquator.Add(GridMap.Instance.GetHex(x + "," + y));
					if(y % 2 == 0){
						x-=1;
					}
					x++;
					if(x >= GridMap.Instance.width){
						x = (int)GridMap.Instance.width - 1;
					}
				}
			}

		}
		if(this.equatorLine == EQUATOR_LINE.DIAGONAL_RIGHT){
			if(xy[1] == 0){
				int x = xy[0];
				for(int y = xy[1]; y < GridMap.Instance.height; y++){
					listEquator.Add(GridMap.Instance.GetHex(x + "," + y));
					if(y % 2 == 0){
						x-=1;
					}
					x++;
					if(x >= GridMap.Instance.width){
						x = (int)GridMap.Instance.width - 1;
					}
				}
			}else{
				int x = xy[0];
				for(int y = xy[1]; y >= 0; y--){
					listEquator.Add(GridMap.Instance.GetHex(x + "," + y));
					if(y % 2 == 1){
						x+=1;
					}
					x--;
					if(x < 0){
						x = 0;
					}
				}
			}
		}

		for(int i = 0; i < listEquator.Count; i++){
//			listEquator[i].GetComponent<SpriteRenderer>().color = Color.red;
		}
	}

	internal string GetHexEquatorName(){
		return 0 + "," + ((int)GridMap.Instance.height / 2);
		//if(this.equatorLine == EQUATOR_LINE.HORIZONTAL){
		//	int randomY = UnityEngine.Random.Range(0, (int)GridMap.Instance.height);
		//	return 0 + "," + randomY;
		//}else if(this.equatorLine == EQUATOR_LINE.VERTICAL){
		//	int randomX = UnityEngine.Random.Range(0, (int)GridMap.Instance.width);
		//	return randomX + "," + 0;
		//}else{
		//	int randomX = UnityEngine.Random.Range(0, (int)GridMap.Instance.height);
		//	int[] verticalPos = {0,(int)GridMap.Instance.height - 1};
		//	int randomPos = UnityEngine.Random.Range(0,2);
		//	return randomX + "," + verticalPos[randomPos];
		//}

		//return string.Empty;
	}
	private EQUATOR_LINE GetEquatorLine(){
		return EQUATOR_LINE.HORIZONTAL;

		//int randomLine = UnityEngine.Random.Range(0,4);
		//if(randomLine == 0){
		//	return EQUATOR_LINE.HORIZONTAL;
		//}else if(randomLine == 1){
		//	return EQUATOR_LINE.VERTICAL;
		//}else if(randomLine == 2){
		//	return EQUATOR_LINE.DIAGONAL_RIGHT;
		//}else{
		//	return EQUATOR_LINE.DIAGONAL_LEFT;
		//}
	}
}
