using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using ECS;

public struct ActionJob : IJob {
    public NativeArray<AreaData> data;

	public void Execute() {
        for (int i = 0; i < data[0].allowedLandmarkTypes.Count; i++) {
            Debug.Log(data[0].allowedLandmarkTypes[i]);
        }
    }
}
