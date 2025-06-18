/*
 * SSSMapGenerator : Ver. 1.0.2
 * Written by Takashi Sowa @ loloop
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace S3MG{

	public class MapGenerator : MonoBehaviour{
		//Instance for Singleton
		public static MapGenerator instance;

		//Switch from outside when you want to disable operations during each node event execution or other operations
		[SerializeField] public bool noMapOperation {get;set;} = false;

		[Header("▼Skip Node Processing : If true, node processing is skipped, and only the stage advances")]
		[SerializeField] public bool skipNodeProcessing = false;

		[Header("▼Generate on Execution")]
		[SerializeField] public bool playOnAwake = true;

		[Header("▼Make Start Node")]
		[SerializeField] public bool makeStart = true;

		[Header("▼Offset Nodes in Placement")]
		[SerializeField] public bool isOffset = true;

		[Header("▼Show Text on Nodes")]
		[SerializeField] public bool showText = true;

		[Header("▼Allow Path Intersection")]
		[SerializeField] public bool crossable = false;

		[Header("▼Completely Random Placement : Except for fixed nodes, do not apply the default map placement rules")]
		[SerializeField] public bool allRandom = false;

		[Header("▼Whether to randomize the seed : If true, the value of the variable seed is invalid")]
		[SerializeField] public bool randomizeSeed = true;
		[SerializeField] public int seed = 0;

		[Header("▼Map Basic Settings")]
		[SerializeField, Range(1,128)] public int floorNum = 15;
		[SerializeField, Range(1,32)] public int routeNum = 7;
		[SerializeField, Range(32,256)] public float normalNodeSize = 80;
		[SerializeField, Range(32,256)] public float startNodeSize = 160;
		[SerializeField, Range(32,256)] public float finalNodeSize = 160;
		[SerializeField] public float floorDistance = 80;
		[SerializeField] public float routeDistance = 80;

		[Header("▼Active Route Count : Limited to RouteNum if exceeded")]
		[SerializeField, Range(1,20)] public int activeRouteNum = 4;
		Node[] activeRouteNodeArray;

		[Header("▼Canvas to Display Map")]
		[SerializeField] public GameObject mapCanvas;
		RectTransform mapParent;

		[Header("▼Node Prefab: ButtonPrefab with Node class")]
		[SerializeField] public Node nodePref;

		[Header("▼Path Settings : Prefab is not required, Custom Prefab with settings like shaders is also acceptable")]
		[SerializeField] public Image pathImagePref;
		[SerializeField] public Color pathColor = Color.gray;
		[SerializeField] public Color passedPathColor = Color.white;
		[SerializeField, Range(0,16)] public float pathWidth = 4f;
		[SerializeField, Range(0,256)] public float paddingBetweenNodes = 0;

		[Header("▼Background Prefab : Prefab is not required, Image Prefab with 9-slice scaling")]
		[SerializeField] public GameObject backgroundPref;

		[Header("▼Padding Between Background and Map")]
		[SerializeField, Range(0,160)] public float backgroundPadding = 80;

		[Header("▼Mouse Drag Sensitivity and Gamepad Movement Sensitivity")]
		[SerializeField, Range(1,3)] public float mouseSensitive = 1.0f;
		[SerializeField, Range(1,6)] public float gamepadSensitive = 2.0f;

		[Header("▼Data for Each Node : Set ScriptableObject Data")]
		[SerializeField] public NodeData startNodeData;
		[SerializeField] public NodeData[] finalNodeData;
		[SerializeField] public FixedNodeData[] fixedNodeData;
		[SerializeField] public MapNodeData[] mapNodeData;

		[SerializeField] public Node nowNode {get;set;}

		Node startNode;
		Node finalNode;
		Node[,] map;
		List<RectTransform> pathsRectTransform = new List<RectTransform>();
		float mapWidth;
		float mapHeight;
		bool isCompleted = false;

		Vector2 oldMousePos;
		Vector2 newMousePos;

		/*------------------------------------------------------------
		Executed when the value in the inspector is changed:Implemented to initialize when added because serializable classes like FixedNodeData and MapNodeData cannot use constructors
		------------------------------------------------------------*/
		void OnValidate(){
			if(!Application.isPlaying && fixedNodeData != null){
				foreach(FixedNodeData node in fixedNodeData){
					if(node.nodeData == null) node.init();
				}
			}
			if(!Application.isPlaying && mapNodeData != null){
				foreach(MapNodeData node in mapNodeData){
					if(node.nodeData == null) node.init();
				}
			}
		}

		/*------------------------------------------------------------
		Executed only once when MonoBehaviour is created, Will work if the GameObject is active even if the component is disabled
		------------------------------------------------------------*/
		void Awake(){
			if(instance == null){
				instance = this;
				DontDestroyOnLoad(this.gameObject);
				DontDestroyOnLoad(mapCanvas);
			}else{
				Destroy(this.gameObject);
				Destroy(mapCanvas);
			}
		}

		/*------------------------------------------------------------
		Executed one frame after Awake, Executed when the GameObject is active and the component is enabled
		------------------------------------------------------------*/
		void Start(){
			if(playOnAwake) init();
           
        }

		/*------------------------------------------------------------
		Called once per frame, Executed when the GameObject and component are enabled
		------------------------------------------------------------*/
		void Update(){
			if(!isCompleted) return;
			if(noMapOperation) return;

			if(Mouse.current.leftButton.wasPressedThisFrame){
				oldMousePos = Mouse.current.position.ReadValue() - ((oldMousePos == Vector2.zero) ? mapParent.anchoredPosition : Vector2.zero);
			}
			else if(Mouse.current.leftButton.isPressed){
				newMousePos.x -= (oldMousePos.x - Mouse.current.position.ReadValue().x) * mouseSensitive;
				newMousePos.y -= (oldMousePos.y - Mouse.current.position.ReadValue().y) * mouseSensitive;
				if(newMousePos.x > mapWidth / 2) newMousePos.x = mapWidth / 2;
				if(newMousePos.x < -mapWidth / 2) newMousePos.x = -mapWidth / 2;
				if(newMousePos.y > Screen.height / 2) newMousePos.y = Screen.height / 2;
				if(newMousePos.y < -(mapHeight + backgroundPadding * 2) - Screen.height / 4) newMousePos.y = -(mapHeight + backgroundPadding * 2) - Screen.height / 4;
				mapParent.anchoredPosition = newMousePos;
				oldMousePos = Mouse.current.position.ReadValue();
			}

			if(Gamepad.current != null){
				if(Gamepad.current.leftStick.ReadValue().y > 0.3f) movementUpDown(true);
				if(Gamepad.current.leftStick.ReadValue().y < -0.3f) movementUpDown(false);
				if(Gamepad.current.leftStick.ReadValue().x < -0.3f) movementLeftRight(true);
				if(Gamepad.current.leftStick.ReadValue().x > 0.3f) movementLeftRight(false);
			}
		}

		/*------------------------------------------------------------
		Gamepad left-right movement
		------------------------------------------------------------*/
		void movementLeftRight(bool vector){
			if(vector){
				newMousePos.x += gamepadSensitive;
				if(newMousePos.x > mapWidth / 2) newMousePos.x = mapWidth / 2;
			}else{
				newMousePos.x -= gamepadSensitive;
				if(newMousePos.x < -mapWidth / 2) newMousePos.x = -mapWidth / 2;
			}
			mapParent.anchoredPosition = newMousePos;
		}

		/*------------------------------------------------------------
		Gamepad up-down movement
		------------------------------------------------------------*/
		void movementUpDown(bool vector){
			if(vector){
				newMousePos.y -= gamepadSensitive;
				if(newMousePos.y > Screen.height / 2) newMousePos.y = Screen.height / 2;
			}else{
				newMousePos.y += gamepadSensitive;
				if(newMousePos.y < -(mapHeight + backgroundPadding * 2) - Screen.height / 4) newMousePos.y = -(mapHeight + backgroundPadding * 2) - Screen.height / 4;
			}
			mapParent.anchoredPosition = newMousePos;
		}

		/*------------------------------------------------------------
		Initialization
		------------------------------------------------------------*/
		public void init(){
			if(dataCheckBeforeGeneration()) return;

			if(randomizeSeed) seed = Mathf.FloorToInt(Random.value * int.MaxValue);
			Random.InitState(seed);

			preGenerated();
			createMap();
			selectFirstNode();
			for(int i = 0; i < activeRouteNodeArray.Length; i++){
				connectingNodes(activeRouteNodeArray[i]);
			}
			if(backgroundPref != null) generateBackground();
			hideEmpty();
			setNode();
			activeNodeSelect();
			isCompleted = true;
		}

		/*------------------------------------------------------------
		Pre-generation data check
		------------------------------------------------------------*/
		bool dataCheckBeforeGeneration(){
			bool check = false;

			if(mapCanvas == null){
				Debug.Log($"Please register the Canvas to display the map.");
				check = true;
			}
			if(mapCanvas.transform.parent != null){
				Debug.Log($"Please place the Canvas to display the map in the Root.");
				check = true;
			}
			if(nodePref == null){
				Debug.Log($"Please register the Node Prefab.");
				check = true;
			}
			if(makeStart && startNodeData == null){
				Debug.Log($"Please register the Start Node data.");
				check = true;
			}
			if(finalNodeData.Length == 0 || mapNodeData.Length == 0){
				Debug.Log($"Please register all map node data except for fixedNodeData.");
				check = true;
			}

			for(int i = 0; i < finalNodeData.Length; i++){
				if(finalNodeData[i] == null){
					Debug.Log($"One or more finalNodeData is null.");
					check = true;
					break;
				}
			}

			for(int i = 0; i < mapNodeData.Length; i++){
				if(mapNodeData[i].nodeData == null){
					Debug.Log($"One or more mapNodeData is null.");
					check = true;
					break;
				}
			}

			for(int i = 0; i < fixedNodeData.Length; i++){
				if(fixedNodeData[i].nodeData == null){
					Debug.Log($"One or more fixedNodeData is null.");
					check = true;
					break;
				}
			}

			bool anySerializable = false;
			for(int i = 0; i < mapNodeData.Length; i++){
				if(mapNodeData[i].serializable){
					anySerializable = true;
					break;
				}
			}
			if(!anySerializable){
				Debug.Log($"Please register at least one continuous node in mapNodeData.");
				check = true;
			}

			bool appearedAfter1 = false;
			for(int i = 0; i < mapNodeData.Length; i++){
				if(!mapNodeData[i].reverseAppearedAfter && mapNodeData[i].appearedAfter == 1){
					appearedAfter1 = true;
					break;
				}
			}
			if(!appearedAfter1){
				Debug.Log($"Please register at least one node that can start from the first floor in mapNodeData.");
				check = true;
			}

			return check;
		}

		/*------------------------------------------------------------
		Pre-generation
		------------------------------------------------------------*/
		void preGenerated(){
			GameObject parentObject = new GameObject("Parent");
			parentObject.layer = LayerMask.NameToLayer("UI");
			mapParent = parentObject.AddComponent<RectTransform>();
			mapParent.SetParent(mapCanvas.transform);
			mapParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
			mapParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			mapParent.anchorMin = new Vector2(0.5f, 0.5f);
			mapParent.anchorMax = new Vector2(0.5f, 0.5f);
			mapParent.pivot = new Vector2(0.5f, 0.5f);
			Vector2 pos = mapParent.anchoredPosition;
			pos.x = 0;
			pos.y = -Screen.height / 2 + ((makeStart) ? startNodeSize / 2 : normalNodeSize / 2) + backgroundPadding;
			mapParent.anchoredPosition = pos;
			mapParent.SetAsFirstSibling();

			oldMousePos = newMousePos = mapParent.anchoredPosition;

			if(pathImagePref == null){
				GameObject pathObject = new GameObject("Path");
				pathObject.AddComponent<RectTransform>();
				Image image = pathObject.AddComponent<Image>();
				image.raycastTarget = false;
				pathObject.layer = LayerMask.NameToLayer("UI");
				pathImagePref = pathObject.GetComponent<Image>();
			}
		}

		/*------------------------------------------------------------
		Create map
		------------------------------------------------------------*/
		void createMap(){
			map = new Node[floorNum,routeNum];

			if(makeStart){
				startNode = Instantiate(nodePref, mapParent);
				setNodeSize(startNode, startNodeSize);
				startNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
				startNode.gameObject.name = $"Start Node";
				startNode.connected = true;
			}

			for(int i = 0; i < floorNum; i++){
				for(int j = 0; j < routeNum; j++){
					Node node = Instantiate(nodePref, mapParent);
					setNodeSize(node, normalNodeSize);
					node.floor = i;
					node.route = j;
					node.xPos = (routeDistance * j) + (normalNodeSize * j) - (routeDistance * (routeNum - 1) / 2 + (normalNodeSize * (routeNum - 1) / 2));
					node.yPos = (floorDistance * i) + (normalNodeSize * i) + ((makeStart) ? floorDistance + (startNodeSize / 2) + (normalNodeSize / 2) : 0);
					if(isOffset){
						node.xPos += Random.Range(-routeDistance * 0.9f / 2, routeDistance * 0.9f / 2 + 1);
						node.yPos += Random.Range(-floorDistance * 0.9f / 2, floorDistance * 0.9f / 2 + 1);
					}
					node.GetComponent<RectTransform>().anchoredPosition = new Vector2(node.xPos, node.yPos);
					map[i,j] = node;
					map[i,j].gameObject.name = $"{i},{j}";
				}
			}

			finalNode = Instantiate(nodePref, mapParent);
			setNodeSize(finalNode, finalNodeSize);
			finalNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (floorDistance * floorNum) + (normalNodeSize * floorNum) + (finalNodeSize / 2) + (normalNodeSize / 2) + ((makeStart) ? (startNodeSize / 2) + (normalNodeSize / 2) : 0));
			finalNode.gameObject.name = $"Final Node";
			finalNode.connected = true;

			mapWidth = (routeNum * normalNodeSize) + (routeNum * routeDistance);
			mapHeight = finalNode.GetComponent<RectTransform>().anchoredPosition.y + ((makeStart) ? startNodeSize / 2 : normalNodeSize / 2) + finalNodeSize / 2;

			if(mapHeight + backgroundPadding * 2 < Screen.height){
				Vector2 pos = mapParent.anchoredPosition;
				pos.y = -(mapHeight + backgroundPadding * 2) / 2 + ((makeStart) ? startNodeSize / 2 : normalNodeSize / 2) + backgroundPadding;
				mapParent.anchoredPosition = pos;
				oldMousePos = newMousePos = mapParent.anchoredPosition;
			}
		}

		/*------------------------------------------------------------
		Set node size
		------------------------------------------------------------*/
		void setNodeSize(Node target, float size){
			RectTransform rectTransform = target.gameObject.GetComponent<RectTransform>();
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		}

		/*------------------------------------------------------------
		Randomly select active route nodes and store them in an array : To prevent random selection from messing up the order, ensure the randomly selected active route nodes are stored in ascending order
		------------------------------------------------------------*/
		void selectFirstNode(){
			List<Node> tmp = new List<Node>();
			for(int i = 0; i < routeNum; i++){
				tmp.Add(map[0,i]);
			}

			if(activeRouteNum < 1) activeRouteNum = 1;
			if(activeRouteNum > routeNum) activeRouteNum = routeNum;

			for(int i = 0; i < routeNum - activeRouteNum; i++){
				tmp.RemoveAt(Random.Range(0, tmp.Count));
			}

			activeRouteNodeArray = tmp.ToArray();
		}

		/*------------------------------------------------------------
		Connect nodes
		------------------------------------------------------------*/
		void connectingNodes(Node node){
			if(!node.connected) node.connected = true;
			if(node.floor < floorNum - 1){
				int connectDirection = Random.Range(-1,2);
				if(node.route == 0 && connectDirection == -1) connectDirection++;
				if(node.route == routeNum - 1 && connectDirection == 1) connectDirection--;

				if(!crossable){
					if(node.route != 0 && connectDirection == -1 && map[node.floor, node.route - 1].nextNodes != null){
						for(int i = 0; i < map[node.floor, node.route - 1].nextNodes.Count; i++){
							if(map[node.floor, node.route - 1].nextNodes[i] == map[node.floor + 1, node.route]){
								connectDirection++;
								break;
							}
						}
					}
					if(node.route != routeNum - 1 && connectDirection == 1 && map[node.floor, node.route + 1].nextNodes != null){
						for(int i = 0; i < map[node.floor, node.route + 1].nextNodes.Count; i++){
							if(map[node.floor, node.route + 1].nextNodes[i] == map[node.floor + 1, node.route]){
								connectDirection--;
								break;
							}
						}
					}
				}

				if(makeStart){
					if(node.floor == 0){
						startNode.nextNodes.Add(node);
						node.prevNodes.Add(startNode);
						drawPath(startNode.GetComponent<RectTransform>(), node.GetComponent<RectTransform>());
					}
				}

				Node nextNode = map[node.floor + 1, node.route + connectDirection];

				bool haveNode = false;
				for(int i = 0; i < node.nextNodes.Count; i++){
					if(node.nextNodes[i] == nextNode) haveNode = true;
				}
				if(!haveNode) drawPath(node.GetComponent<RectTransform>(), nextNode.GetComponent<RectTransform>());

				node.nextNodes.Add(nextNode);
				nextNode.prevNodes.Add(node);

				connectingNodes(nextNode);
			}
			else{
				bool haveFinalNode = false;
				for(int i = 0; i < node.nextNodes.Count; i++){
					if(node.nextNodes[i] == finalNode) haveFinalNode = true;
				}
				if(!haveFinalNode) drawPath(node.GetComponent<RectTransform>(), finalNode.GetComponent<RectTransform>());

				node.nextNodes.Add(finalNode);
				finalNode.prevNodes.Add(node);

				if(floorNum == 1 && makeStart){
					startNode.nextNodes.Add(node);
					node.prevNodes.Add(startNode);
					drawPath(startNode.GetComponent<RectTransform>(), node.GetComponent<RectTransform>());
				}
			}
		}

		/*------------------------------------------------------------
		Draw paths
		------------------------------------------------------------*/
		void drawPath(RectTransform start, RectTransform end){
			Image path = Instantiate(pathImagePref, mapParent);
			path.color = pathColor;

			float distance = Vector2.Distance(start.anchoredPosition, end.anchoredPosition);
			distance -= (distance > paddingBetweenNodes * 2) ? paddingBetweenNodes * 2 : distance;
			path.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pathWidth);
			path.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, distance);

			path.rectTransform.pivot = new Vector2(0.5f, 0.5f);

			path.rectTransform.position = (start.position + end.position) / 2;

			float angle = Mathf.Atan2(end.anchoredPosition.y - start.anchoredPosition.y, end.anchoredPosition.x - start.anchoredPosition.x) * Mathf.Rad2Deg - 90;
			path.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

			path.transform.SetAsFirstSibling();

			pathsRectTransform.Add(path.rectTransform);
		}

		/*------------------------------------------------------------
		Generate background
		------------------------------------------------------------*/
		void generateBackground(){
			GameObject bg = Instantiate(backgroundPref, mapParent);
			RectTransform rectTransform = bg.gameObject.GetComponent<RectTransform>();
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mapWidth + (backgroundPadding * 2));
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mapHeight + (backgroundPadding * 2));
			Vector2 pos = rectTransform.anchoredPosition;
			rectTransform.anchoredPosition = new Vector2(pos.x, pos.y - ((makeStart) ? startNodeSize / 2 : normalNodeSize / 2) - backgroundPadding);
			bg.transform.SetAsFirstSibling();
		}

		/*------------------------------------------------------------
		Hide unvisited nodes
		------------------------------------------------------------*/
		void hideEmpty(){
			for(int i = 0; i < floorNum; i++){
				for(int j = 0; j < routeNum; j++){
					if(!map[i,j].connected) map[i,j].gameObject.SetActive(false);
				}
			}
		}

		/*------------------------------------------------------------
		Set each node
		------------------------------------------------------------*/
		void setNode(){
			if(makeStart) startNode.setNodeData(startNodeData.sprite, startNodeData.type, (showText) ? startNodeData.nodeName : "");

			int finalNodeNum = Random.Range(0, finalNodeData.Length);
			finalNode.setNodeData(finalNodeData[finalNodeNum].sprite, finalNodeData[finalNodeNum].type, (showText) ? finalNodeData[finalNodeNum].nodeName : "");

			List<Node> tmp = new List<Node>();
			tmp = getUnassignedConnectedNodes(tmp);
			float totalChance = 0;
			for(int i = 0; i < mapNodeData.Length; i++){
				totalChance += mapNodeData[i].chance;
			}

			if(allRandom){
				randomlyAssignNode(tmp, totalChance);
			}
			else{
				int loopCount = 0;
				while(true){
					loopCount++;
					applyRulesAndAssignNode(tmp, totalChance);
					duplicateBranchBackToNull();
					tmp.Clear();
					tmp = getUnassignedConnectedNodes(tmp);
					if(tmp.Count > 0 && loopCount > 100){
						randomlyAssignNode(tmp, totalChance);
						Debug.Log($"The outer loop exceeded 100 iterations, so assigning a random node.");
					}
					if(tmp.Count == 0 || loopCount > 100) break;
				}
				// Debug.Log($"Outer loop：{loopCount}");
			}

			for(int i = 0; i < fixedNodeData.Length; i++){
				if(fixedNodeData[i].appearedOn >= floorNum + 1) continue;
				for(int j = 0; j < routeNum; j++){
					if(map[fixedNodeData[i].appearedOn - 1, j].connected) map[fixedNodeData[i].appearedOn - 1, j].setNodeData(fixedNodeData[i].nodeData.sprite, fixedNodeData[i].nodeData.type, (showText) ? fixedNodeData[i].nodeData.nodeName : "");
				}
			}
		}

		/*------------------------------------------------------------
		Place nodes applying rules
		------------------------------------------------------------*/
		void applyRulesAndAssignNode(List<Node> tmp, float totalChance){
			int loopCount = 0;
			while(true){
				loopCount++;
				for(int i = 0; i < tmp.Count; i++){
					MapNodeData selectedNodeData = selectRandomNodeData(mapNodeData, totalChance);
					if(selectedNodeData != null){
						if(meetsCondition(tmp[i], selectedNodeData)){
							tmp[i].setNodeData(selectedNodeData.nodeData.sprite, selectedNodeData.nodeData.type, (showText) ? selectedNodeData.nodeData.nodeName : "");
						}
					}
				}
				tmp.Clear();
				tmp = getUnassignedConnectedNodes(tmp);
				if(tmp.Count > 0 && loopCount > 100){
					randomlyAssignNode(tmp, totalChance);
					Debug.Log($"The inner loop exceeded 100 iterations, so assigning a random node.");
				}
				if(tmp.Count == 0 || loopCount > 100) break;
			}
			// Debug.Log($"Inner Loop：{loopCount}");
		}

		/*------------------------------------------------------------
		Return a list of unset nodes
		------------------------------------------------------------*/
		List<Node> getUnassignedConnectedNodes(List<Node> tmp){
			for(int i = 0; i < floorNum; i++){
				for(int j = 0; j < routeNum; j++){
					if(map[i,j].connected && map[i,j].nodeType == null) tmp.Add(map[i,j]);
				}
			}
			return tmp;
		}

		/*------------------------------------------------------------
		Return nodes based on spawn probability
		------------------------------------------------------------*/
		MapNodeData selectRandomNodeData(MapNodeData[] mapNodeData, float totalChance){
			float randNum = Random.Range(0f, totalChance);
			float cumulativeChance = 0f;

			for(int j = 0; j < mapNodeData.Length; j++){
				cumulativeChance += mapNodeData[j].chance;
				if(randNum < cumulativeChance){
					return mapNodeData[j];
				}
			}
			return null;
		}

		/*------------------------------------------------------------
		Check map placement rules and return true if rules are met
		------------------------------------------------------------*/
		bool meetsCondition(Node node, MapNodeData selectedNodeData) {
			bool flag = false;

			if(!selectedNodeData.reverseAppearedAfter){
				if(node.floor >= selectedNodeData.appearedAfter - 1) flag = true;
			}

			if(selectedNodeData.reverseAppearedAfter){
				if(node.floor < selectedNodeData.appearedAfter - 1) flag = true;
			}

			if(!selectedNodeData.serializable){
				for(int i = 0; i < node.nextNodes.Count; i++){
					if(node.nextNodes[i].nodeType == selectedNodeData.nodeData.type){
						flag = false;
						break;
					}
				}
				for(int i = 0; i < node.prevNodes.Count; i++){
					if(node.prevNodes[i].nodeType == selectedNodeData.nodeData.type){
						flag = false;
						break;
					}
				}
			}

			return flag;
		}

		/*------------------------------------------------------------
		Set duplicate nodes of overlapping branches to null
		------------------------------------------------------------*/
		void duplicateBranchBackToNull(){
			List<Node> tmp = new List<Node>();
			for(int i = 0; i < floorNum; i++){
				for(int j = 0; j < routeNum; j++){
					if(map[i,j].nodeType != null) tmp.Add(map[i,j]);
				}
			}
			for(int i = 0; i < tmp.Count; i++){
				if(tmp[i].nextNodes.Count == 2 && tmp[i].nextNodes[0].nodeType == tmp[i].nextNodes[1].nodeType && tmp[i].nextNodes[0].xPos != tmp[i].nextNodes[1].xPos){
					tmp[i].nextNodes[0].setNodeData(null,null);
				}
				if(tmp[i].nextNodes.Count == 3){
					Node first = tmp[i].nextNodes[0];
					Node second = tmp[i].nextNodes[1];
					Node third = tmp[i].nextNodes[2];
					if(first.nodeType == second.nodeType && second.nodeType == third.nodeType && first.xPos != second.xPos && second.xPos != third.xPos){
						first.setNodeData(null,null);
						second.setNodeData(null,null);
					}
					if(first.nodeType == second.nodeType && first.xPos != second.xPos){
						first.setNodeData(null,null);
					}
					if(first.nodeType == third.nodeType && first.xPos != third.xPos){
						first.setNodeData(null,null);
					}
					if(second.nodeType == third.nodeType && second.xPos != third.xPos){
						second.setNodeData(null,null);
					}
				}
			}
		}

		/*------------------------------------------------------------
		Freely place all nodes according to spawn probability except fixed nodes
		------------------------------------------------------------*/
		void randomlyAssignNode(List<Node> tmp, float totalChance){
			for(int i = 0; i < tmp.Count; i++){
				MapNodeData selectedNodeData = selectRandomNodeData(mapNodeData, totalChance);
				if(selectedNodeData != null){
					tmp[i].setNodeData(selectedNodeData.nodeData.sprite, selectedNodeData.nodeData.type, (showText) ? selectedNodeData.nodeData.nodeName : "");
				}
			}
			Debug.Log($"Placing nodes randomly.");
		}

		/*------------------------------------------------------------
		Select the node to start
		------------------------------------------------------------*/
		void activeNodeSelect(){
			finalNode.disableButton();
			for(int i = 0; i < floorNum; i++){
				for(int j = 0; j < routeNum; j++){
					if(map[i,j].connected && map[i,j].nodeType != null) map[i,j].disableButton();
				}
			}

			if(makeStart){
				if(Gamepad.current != null) EventSystem.current.SetSelectedGameObject(startNode.gameObject);
			}
			else{
				for(int i = 0; i < activeRouteNodeArray.Length; i++){
					activeRouteNodeArray[i].enableButton();
				}
				if(Gamepad.current != null) EventSystem.current.SetSelectedGameObject(activeRouteNodeArray[0].gameObject);
			}
		}

		/*------------------------------------------------------------
		Paint the paths that have been passed
		------------------------------------------------------------*/
		public void paintPath(Node node){
			if(!makeStart && node.floor == 0) return;

			foreach(RectTransform path in pathsRectTransform){
				float distance = Vector2.Distance(path.position, (nowNode.GetComponent<RectTransform>().position + node.GetComponent<RectTransform>().position) / 2);
				if(Mathf.Approximately(distance, 0) || distance < 0.01f){
					path.GetComponent<Image>().color = passedPathColor;
					break;
				}
			}
		}

		/*------------------------------------------------------------
		Set nodes on the same floor as the argument to passed state, making them unclickable
		------------------------------------------------------------*/
		public void passedSameFloor(Node node){
			for(int i = 0; i < floorNum; i++){
				for(int j = 0; j < routeNum; j++){
					if(map[i,j].floor == node.floor){
						map[i,j].passedNode();
					}
				}
			}
		}

		/*------------------------------------------------------------
		Move to the next node: for operations from external classes
		------------------------------------------------------------*/
		public void toNextNode(){
			if(nowNode == finalNode) return;
			for(int i = 0; i < nowNode.nextNodes.Count; i++){
				nowNode.nextNodes[i].enableButton();
			}
			if(Gamepad.current != null) EventSystem.current.SetSelectedGameObject(nowNode.nextNodes[0].gameObject);
		}

		/*------------------------------------------------------------
		Activate map : for operations from external classes
		------------------------------------------------------------*/
		public void activeMap(){
			noMapOperation = false;
			mapCanvas.SetActive(true);
          
        }

		/*------------------------------------------------------------
		Deactivate map : for operations from external classes
		------------------------------------------------------------*/
		public void inactiveMap(){
			noMapOperation = true;
			mapCanvas.SetActive(false);
		}

		/*------------------------------------------------------------
		ReGenarate : for operations from external classes
		------------------------------------------------------------*/
		public void reGenarate(){
			if(mapParent != null){
				Destroy(mapParent.gameObject);
			}
			if(activeRouteNodeArray != null){
				for(int i = 0; i < activeRouteNodeArray.Length; i++){
					activeRouteNodeArray[i] = null;
				}
			}
			if(map != null){
				for(int i = 0; i < map.GetLength(0); i++){
					for(int j = 0; j < map.GetLength(1); j++){
						map[i,j] = null;
					}
				}
			}
			if(pathsRectTransform != null){
				for(int i = 0; i < pathsRectTransform.Count; i++){
					pathsRectTransform[i] = null;
				}
			}
			mapParent = null;
			activeRouteNodeArray = null;
			startNode = null;
			finalNode = null;
			map = null;
			pathsRectTransform = new List<RectTransform>();
			nowNode = null;
			isCompleted = false;
			noMapOperation = false;
			init();
		}

	}

}