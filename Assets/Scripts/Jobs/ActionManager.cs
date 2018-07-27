using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class ActionManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        CreateJob();
	}
    private void CreateJob() {
        AreaData areaData = new AreaData {
            areaType = AREA_TYPE.DEMONIC_INTRUSION,
            baseAreaType = BASE_AREA_TYPE.DUNGEON,
            allowedLandmarkTypes = new List<LANDMARK_TYPE>() {
                LANDMARK_TYPE.DEMONIC_PORTAL,
                LANDMARK_TYPE.ELVEN_HOUSES,
            },
            possibleMonsterSpawns = new List<MonsterPartyComponent>()
        };

        NativeArray<AreaData> mainData = new NativeArray<AreaData>(1, Allocator.TempJob);
        mainData[0] = areaData;

        ActionJob actionJob = new ActionJob {
            data = mainData
        };
        JobHandle jobHandle = actionJob.Schedule();
        jobHandle.Complete();

        mainData.Dispose();
        Debug.Log("DONE WITH JOBS!");
    }
}
