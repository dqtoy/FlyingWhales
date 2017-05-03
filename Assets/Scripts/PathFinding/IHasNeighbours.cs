using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFind{
	public interface IHasNeighbours<N>{
		IEnumerable<N> ValidTiles { get; }
		IEnumerable<N> RoadTiles { get; } 
//		IEnumerable<N> CombatTiles { get; } 
		IEnumerable<N> PurchasableTiles { get; } 
	}
}