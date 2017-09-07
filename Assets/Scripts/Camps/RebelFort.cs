using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RebelFort: City {

	public RebelFort(HexTile hexTile, Kingdom kingdom, bool isRebel, Rebellions rebellion): base (hexTile, kingdom, isRebel){
		this.ChangeToRebelFort (rebellion, true);
	}
}
