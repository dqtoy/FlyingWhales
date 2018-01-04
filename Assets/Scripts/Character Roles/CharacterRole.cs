﻿/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterRole {
    protected List<ROAD_TYPE> allowedRoadTypes; //states what roads this role can use.
    protected bool canPassHiddenRoads; //can the character use roads that haven't been discovered yet?
}
