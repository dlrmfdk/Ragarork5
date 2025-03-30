/*
 * SSSMapGenerator : Ver. 1.0.2
 * Written by Takashi Sowa @ loloop
*/

using UnityEngine;

namespace S3MG{

	[CreateAssetMenu(menuName = "S3MG/createNodeData", fileName = "NodeData")]
	public class NodeData : ScriptableObject{

		public enum Type{
			Empty,
			Start,
			Camp,
			Shop,
			Event,
			Treasure,
			Trap,
			Enemy,
			Middle,
			Final,
			Random,
		}

		public Sprite sprite;
		public Type type;
		public string nodeName;

	}

}