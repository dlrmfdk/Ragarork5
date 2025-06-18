//using System.Collections.Generic;
//using UnityEngine;

//// 카드 데이터 구조 (CSV와 매핑)
//[System.Serializable]
//public class CardData
//{
//    public int id;
//    public string cardName;     // ex) "휘두르기", "방어하기" 등
//    public int cost;            //마나 코스트
//    public string type;         // "attack", "defence", "skill", "curse"
//    public string rarity;       // "common", "uncommon", "rare", "legend", "curse" 등
//    public string color;        // "red", "blue", "yellow", "white", "black", "green", etc.
//    public bool targetEnemy;
//    public bool targetSelf;
//    public bool exhaust;        // 사용 후 소멸 여부
//    public string description;  // 예: "적에게 {d}의 데미지를 준다."
//    public float defaultValue;  // CSV의 default 열 (ex: 6, 8, - , 공백 등)
    
//}

//public class CardDatabase : MonoBehaviour
//{
//    public static CardDatabase Instance { get; private set; }   
//    public List<CardData> cardList = new List<CardData>();

//    private void Awake()
//    {
//        // 싱글턴 중복 방지
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        LoadCardDataFromCSV();
//    }

//    private void LoadCardDataFromCSV()
//    {
//        // CSVReader 통해 "Skill.csv" 로드 (파일명은 "Skill"로)
//        List<Dictionary<string, object>> data = CSVReader.Read("Skill");
//        cardList.Clear();

//        for (int i = 0; i < data.Count; i++)
//        {
//            CardData card = new CardData();

//            card.id = data[i].ContainsKey("id") ? (int)data[i]["id"] : 0;
//            card.cardName = data[i].ContainsKey("cardName") ? data[i]["cardName"].ToString() : "";
//            card.cost = data[i].ContainsKey("cost") ? (int)data[i]["cost"] : 0;
//            card.type = data[i].ContainsKey("type") ? data[i]["type"].ToString() : "";
//            card.rarity = data[i].ContainsKey("rarity") ? data[i]["rarity"].ToString() : "";
//            card.color = data[i].ContainsKey("color") ? data[i]["color"].ToString() : "";
//            card.description = data[i].ContainsKey("description") ? data[i]["description"].ToString() : "";

//            // bool 파싱 (TRUE, FALSE 로 들어온다고 가정)
//            card.targetEnemy = data[i].ContainsKey("targetEnemy")
//                ? bool.Parse(data[i]["targetEnemy"].ToString())
//                : false;
//            card.targetSelf = data[i].ContainsKey("targetSelf")
//                ? bool.Parse(data[i]["targetSelf"].ToString())
//                : false;
//            card.exhaust = data[i].ContainsKey("exhaust")
//                ? bool.Parse(data[i]["exhaust"].ToString())
//                : false;

//            // defaultValue 파싱 (float)
//            float parsedVal = 0f;
//            if (data[i].ContainsKey("default"))
//            {
//                string defaultStr = data[i]["default"] != null
//                    ? data[i]["default"].ToString()
//                    : "";
//                if (float.TryParse(defaultStr, out float result))
//                    parsedVal = result;
//            }
//            card.defaultValue = parsedVal;

//            cardList.Add(card);
//        }

//        Debug.Log($"[CardDatabase] CSV 로드 완료! 카드 개수: {cardList.Count}");
//    }

//    // cardName으로 CardData 찾는 편의 메서드
//    public CardData GetCardDataByName(string cardName)
//    {
//        return cardList.Find(c => c.cardName == cardName);
//    }

//    // (선택) ID로 찾고 싶다면 이런 것도 가능
//    public CardData GetCardDataById(int id)
//    {
//        return cardList.Find(c => c.id == id);
//    }
//}