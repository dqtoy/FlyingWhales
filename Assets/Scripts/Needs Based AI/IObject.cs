using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObject {
    //Data
    List<ObjectState> states { get; }
    bool isInvisible { get; }
    int foodAdvertisementValue { get; }
    int energyAdvertisementValue { get; }
    int joyAdvertisementValue { get; }
    int prestigeAdvertisementValue { get; }
}
