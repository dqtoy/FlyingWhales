using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class TheSpire : LocationStructure {
        public override Vector2 selectableSize { get; }
        public TheSpire(ILocation location) : base(STRUCTURE_TYPE.THE_SPIRE, location){
            selectableSize = new Vector2(10f, 10f);
        }
    }
}