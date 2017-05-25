using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ExpansionUI : MonoBehaviour {

	public UIPopupList hexTilesPopUpList;

	public UILabel chosenHexTileLbl;

	void OnEnable(){
		hexTilesPopUpList.Clear();

		for (int i = 0; i < UIManager.Instance.currentlyShowingCity.hexTile.connectedTiles.Count; i++) {
			if (!UIManager.Instance.currentlyShowingCity.hexTile.connectedTiles [i].isOccupied) {
				hexTilesPopUpList.AddItem (UIManager.Instance.currentlyShowingCity.hexTile.connectedTiles [i].tileName);
			}
		}
	}

	public void StartExpansion(){
//		string[] coordinates = chosenHexTileLbl.text.Split(',');
//		int xCoordinate = Int32.Parse (coordinates [0]);
//		int yCoordinate = Int32.Parse (coordinates [1]);
//		HexTile hexTileToExpandTo = GridMap.Instance.map[xCoordinate, yCoordinate];
//
//		Expansion newExpansion = new Expansion (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, UIManager.Instance.currentlyShowingCity.governor);
//		newExpansion.hexTileToExpandTo = hexTileToExpandTo;
	}
}
