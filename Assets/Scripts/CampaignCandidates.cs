using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CampaignCandidates {

	public General general;
	public List<HexTile> path;

	public CampaignCandidates(General general, List<HexTile> path){
		this.general = general;
		this.path = new List<HexTile>(path);
	}
}
