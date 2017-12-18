using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class RaceComponent : MonoBehaviour {
		public RACE race;
		public List<BodyPart> bodyParts;
		public int baseStr;
		public int baseInt;
		public int baseAgi;
		public int baseHP;
		public int strGain;
		public int intGain;
		public int agiGain;
		public int hpGain;
	}
}

