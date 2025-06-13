/*
 * SSSMapGenerator : Ver. 1.0.2
 * Written by Takashi Sowa @ loloop
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace S3MG{

	public class Node : MonoBehaviour{

		[SerializeField] public int floor {get; set;}
		[SerializeField] public int route {get; set;}

		[SerializeField] public List<Node> prevNodes {get; set;} = new List<Node>();
		[SerializeField] public List<Node> nextNodes {get; set;} = new List<Node>();

		[SerializeField] public bool connected {get; set;} = false;
		[SerializeField] public bool visited {get; set;} = false;

		[SerializeField] public float xPos {get; set;}
		[SerializeField] public float yPos {get; set;}

		[SerializeField] public NodeData.Type? nodeType {get; set;} = null;

		[SerializeField] Image nodeImage;
		[SerializeField] Button nodeButton;
		[SerializeField] Image visitImage;
		[SerializeField] TextMeshProUGUI nodeText;
		[SerializeField] AudioSource AS;

		[SerializeField] float lerpDuration = 0.8f;
		[SerializeField] Color lerpColor = Color.gray;
		Color defaultColor = Color.white;
		Color currentColor;
		bool isForward = true;
		float lerpStartTime;

		[SerializeField] public bool onButton {get; set;} = false;
		float fillTime = 0.4f;
		float elapsedTime = 0f;

		MapGenerator  mapGenerator;

		/*------------------------------------------------------------
		Executed only once when MonoBehaviour is created, Will work if the GameObject is active even if the component is disabled
		------------------------------------------------------------*/
		void Awake(){
			mapGenerator = FindFirstObjectByType<MapGenerator>();
			currentColor = defaultColor;
		}

		/*------------------------------------------------------------
		Called once per frame, Executed when the GameObject and component are enabled
		------------------------------------------------------------*/
		void Update(){
			if(!nodeButton.enabled || visited) return;

			float t = (Time.time - lerpStartTime) / lerpDuration;
			if(t > 1f){
				t = 0f;
				lerpStartTime = Time.time;
				isForward = !isForward;
				currentColor = isForward ? defaultColor : lerpColor;
			}
			nodeImage.color = Color.Lerp(currentColor, isForward ? lerpColor : defaultColor, t);

			if(onButton) fillUp();
		}

		/*------------------------------------------------------------
		Set image, type, and text on a node
		------------------------------------------------------------*/
		public void setNodeData(Sprite sprite, NodeData.Type? type, string text = ""){
			nodeImage.sprite = sprite;
			nodeType = type;
			nodeText.text = text;
		}

		/*------------------------------------------------------------
		Enable the Button component
		------------------------------------------------------------*/
		public void enableButton(){
			nodeButton.enabled = true;
		}

		/*------------------------------------------------------------
		Disable the Button component
		------------------------------------------------------------*/
		public void disableButton(){
			nodeButton.enabled = false;
		}

		/*------------------------------------------------------------
		Mark as passed
		------------------------------------------------------------*/
		public void passedNode(){
			disableButton();
			if(visited){
				nodeImage.color = defaultColor;
			}else{
				nodeImage.color = Color.gray;
				onButton = false;
				fillReset();
			}
		}

		/*------------------------------------------------------------
		Fill up
		------------------------------------------------------------*/
		void fillUp(){
			if(elapsedTime < fillTime){
				float amountValue = Mathf.Lerp(0f, 1.0f, elapsedTime / fillTime);
				visitImage.fillAmount = amountValue;
				elapsedTime += Time.deltaTime;
			}
			else{
				AS.Play();
				entryStart();
			}
		}

		/*------------------------------------------------------------
		Fill reset
		------------------------------------------------------------*/
		void fillReset(){
			visitImage.fillAmount = 0;
			elapsedTime = 0f;
		}

		/*------------------------------------------------------------
		Branch events based on node type
		------------------------------------------------------------*/
		void entryStart(){
			visited = true;

			if(nodeType != NodeData.Type.Start) mapGenerator.paintPath(this);

			if(nodeType != NodeData.Type.Start && nodeType != NodeData.Type.Final){
				mapGenerator.passedSameFloor(this);
			}
			else{
				passedNode();
			}

			mapGenerator.nowNode = this;

			if(mapGenerator.skipNodeProcessing){
				mapGenerator.toNextNode();
			}
			else{
				switch(nodeType){
					case NodeData.Type.Start:
						handleStart();
						break;
					case NodeData.Type.Camp:
						handleCamp();
						break;
					case NodeData.Type.Shop:
						handleShop();
						break;
					case NodeData.Type.Event:
						handleEvent();
						break;
					case NodeData.Type.Treasure:
						handleTreasure();
						break;
					case NodeData.Type.Trap:
						handleTrap();
						break;
					case NodeData.Type.Enemy:
						handleEnemy();
						break;
					case NodeData.Type.Middle:
						handleMiddle();
						break;
					case NodeData.Type.Final:
						handleFinal();
						break;
					case NodeData.Type.Random:
						handleRandom();
						break;
					default:
						Debug.Log($"Nothing is set");
						break;
				}
			}

		}

		/*------------------------------------------------------------
		Processing each node
		------------------------------------------------------------*/
		void handleStart(){
			Debug.Log("Start the game");
		}

		void handleCamp(){
			Debug.Log("Open a camp");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("RestScene");
        }

		void handleShop(){
			Debug.Log("Open a shop");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("ShopScene");
        }

		void handleEvent(){
			Debug.Log("Execute event");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("sampleScene");
        }

		void handleTreasure(){
			Debug.Log("Obtain treasure");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("sampleScene");
        }

		void handleTrap(){
			Debug.Log("Execute trap");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("sampleScene");
        }

		void handleEnemy(){
			Debug.Log("Fight a enemy");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("NormalBattle");
        }

        void handleMiddle(){
			Debug.Log("Midpoint");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("sampleScene");
        }

		void handleFinal(){
			switch(nodeText.text){
				case "Boss01" :
					Debug.Log("Fight the Boss 01");
					break;
				case "Boss02" :
					Debug.Log("Fight the Boss 02");
					break;
				default :
					Debug.Log("Fight the Boss 03");
					break;
			}
		}

		void handleRandom(){
			Debug.Log("Execute random process");
            MapGenerator.instance.inactiveMap();  // ¸Ê ºñÈ°¼ºÈ­
            SceneManager.LoadScene("sampleScene");
        }
	}

}