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
				hex.GetComponent<HexTile>().tileName = hex.name;
				hex.GetComponent<HexTile>().xCoordinate = x;
				hex.GetComponent<HexTile>().yCoordinate = y;
				int sortingOrder = x - y;
				hex.GetComponent<HexTile>().GetComponent<SpriteRenderer> ().sortingOrder = sortingOrder;
				hex.GetComponent<HexTile>().centerPiece.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
				hex.GetComponent<HexTile>().kingdomColorSprite.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
				hex.GetComponent<HexTile>().highlightGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
				hex.GetComponent<HexTile>().structureGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 3;
				hex.GetComponent<HexTile>().cityNameGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 5;
				listHexes.Add(hex);
				map[x, y] = hex.GetComponent<HexTile>();
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

//	public IEnumerable<Tile> AllTiles
//	{
//		get
//		{
//			for (var x = 0; x < width; x++)
//				for (var y = 0; y < height; y++)
//					yield return GameBoard[x, y];
//		}
//	}
}
