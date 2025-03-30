/*
 * SSSMapGenerator : Ver. 1.0.2
 * Written by Takashi Sowa @ loloop
*/

using UnityEngine;

namespace S3MG{

	/*------------------------------------------------------------
	Map Node Data : Serializable class
	------------------------------------------------------------*/
	[System.Serializable]
	public class MapNodeData{
		[SerializeField] public NodeData nodeData;
		[SerializeField] public bool serializable;
		[SerializeField, Range(0,1)] public float chance;
		[SerializeField, Range(1,100)] public int appearedAfter;
		[SerializeField] public bool reverseAppearedAfter;
		public void init(){
			serializable = true;
			chance = 1;
			appearedAfter = 1;
			reverseAppearedAfter = false;
		}
	}

	/*------------------------------------------------------------
	Fixed Node Data : Serializable class
	------------------------------------------------------------*/
	[System.Serializable]
	public class FixedNodeData{
		[SerializeField] public NodeData nodeData;
		[SerializeField, Range(1,100)] public int appearedOn;
		public void init(){
			appearedOn = 1;
		}
	}

}
