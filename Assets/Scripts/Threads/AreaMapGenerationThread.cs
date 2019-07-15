using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaMapGenerationThread : Multithread {

	public Area area { get; private set; }
    public AreaInnerTileMap areaMap { get; private set; }
    public TownMapSettings generatedSettings { get; private set; }
    public string log;

    public AreaMapGenerationThread(Area area, AreaInnerTileMap areaMap) {
        this.area = area;
        this.areaMap = areaMap;
    }

    public override void DoMultithread() {
        base.DoMultithread();
        try {
            areaMap.Initialize(area);
            generatedSettings = areaMap.GenerateInnerStructures(out log);
        } catch (System.Exception e) {
            Debug.LogError("Problem with " + area.name + "'s AreaMapGenerationThread!\n" + e.Message + "\n" + e.StackTrace);
        }
        
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        //onFinishAction.Invoke(area, areaMap, generatedSettings);
        LandmarkManager.Instance.OnFinishedGeneratingAreaMap(this);


    }
}
