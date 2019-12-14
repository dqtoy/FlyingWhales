public class RegionConnectionData {
    public Region region { get; private set; }
    public int travelTimeInTicks { get; private set; }

    public RegionConnectionData(Region region, Region owner) {
        this.region = region;
        travelTimeInTicks = DetermineTravelTimeBetween(owner, region);
    }

    private int DetermineTravelTimeBetween(Region region1, Region region2) {
        float distance = (region1.coreTile.transform.position - region2.coreTile.transform.position).magnitude;
        //travel time is 1 tick per unit of distance
        return (int) distance;
    }
}
