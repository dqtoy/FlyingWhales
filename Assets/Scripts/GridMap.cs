using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridMap : MonoBehaviour {
	public static GridMap Instance;

	public GameObject goHex;

	public float width;
	public float height;

	public float xOffset;
	public float yOffset;

	public int tileSize;

	public float elevationFrequency;
	public float moistureFrequency;

	public List<GameObject> listHexes;

	public HexTile[,] map;

	void Awake(){
		Instance = this;
	}

	internal void GenerateGrid () {
		map = new HexTile[(int)width, (int)height];
		listHexes = new List<GameObject>();
        int id = 1;
		for (int x = 0;  x < width; x++){
			for(int y = 0; y < height; y++){
				float xPosition = x * xOffset;

				float yPosition = y * yOffset;
				if (y % 2 == 1) {
					xPosition += xOffset / 2;
				}
				GameObject hex = GameObject.Instantiate(goHex) as GameObject;
				hex.transform.parent = this.transform;
				hex.transform.position = new Vector3(xPosition, yPosition,0f);
				hex.transform.localScale = new Vector3(tileSize,tileSize,0f);
				hex.name = x + "," + y;
                HexTile currHex = hex.GetComponent<HexTile>();
                currHex.id = id;
                currHex.tileName = hex.name;
                currHex.xCoordinate = x;
                currHex.yCoordinate = y;
//				int sortingOrder = x - y;
//				hex.GetComponent<HexTile>().SetSortingOrder(sortingOrder);
				listHexes.Add(hex);
				map[x, y] = hex.GetComponent<HexTile>();
                id++;
			}
		}
		listHexes.ForEach(o => o.GetComponent<HexTile>().FindNeighbours(map));
	}

	internal GameObject GetHex(string hexName){
		for(int i = 0; i < listHexes.Count; i++){
			if(hexName == listHexes[i].name){
				return listHexes[i];
			}
		}
		return null;
	}

    public void GenerateNeighboursWithSameTag() {
        for (int i = 0; i < listHexes.Count; i++) {
            HexTile currHex = listHexes[i].GetComponent<HexTile>();
            currHex.sameTagNeighbours = currHex.AllNeighbours.Where(x => x.tileTag == currHex.tileTag).ToList();
        }
    }
}
