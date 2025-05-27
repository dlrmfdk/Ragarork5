//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class EndTurnButton : MonoBehaviour
//{
//    [SerializeField] Sprite active;
//    [SerializeField] Sprite inactive;
//    [SerializeField] Sprite btnText;
//    private Button endTurnButton;
//    // Start is called before the first frame update
//    void Start()
//    {
//        endTurnButton = GetComponent<Button>(); 
//        SetUp(false);
//        TurnManager.OnTurnStarted += SetUp;
//        endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
//    }

//    // Update is called once per frame
//    private void OnDestroy()
//    {
//        TurnManager.OnTurnStarted -= SetUp;
//        endTurnButton.onClick.RemoveListener(OnEndTurnButtonClicked);
//    }
//    public void SetUp(bool isActive)
//    {
//        GetComponent<Image>().sprite = isActive ? active : inactive;
//        GetComponent<Button>().interactable = isActive;
//        // btnText.c = isActive ? new Color32(255, 195, 90, 255) : new Color32(55, 55, 55, 255); 
//    }
//    public void OnEndTurnButtonClicked()
//    {
//        if (TurnManager.Inst != null && !TurnManager.Inst.isLoading)
//        {
//            TurnManager.Inst.EndTurn();
//            Debug.Log("EndTurn button clicked.");
//        }
//    }

//    }