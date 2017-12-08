﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	[System.Serializable]
	public class BodyPartsData {
		public RACE race;
		public List<BodyPart> bodyParts;
	}


	[System.Serializable]
	public class BodyPart: IBodyPart {
//		[SerializeField]
//		[HideInInspector]
//		private string strBodyPart;

		[SerializeField] internal BODY_PART bodyPart;
		[SerializeField] internal IMPORTANCE importance;
		[SerializeField] internal List<ATTRIBUTE> attributes;
		[SerializeField] internal List<SecondaryBodyPart> secondaryBodyParts;
		internal STATUS status;
	}
}

