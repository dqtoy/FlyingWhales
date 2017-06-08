using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RebelFort: City {
	
	public RebelFort(HexTile hexTile, Kingdom kingdom, Rebellion rebellion): base (hexTile, kingdom){
		this.ChangeToRebelFort (rebellion);
	}
}
