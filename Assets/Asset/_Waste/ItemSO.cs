//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public class Item // 카드 정보 클래스
//{
//    public string name;         // 카드 이름
//    public Sprite sprite;       // 카드 이미지
//    public ItemType type;       // 카드 타입 (Attack, Skill, Power, Curse)
//    public float percent;       // 카드 등장 확률
//    public Rarity rarity;     // 카드 희귀도 (Common, Uncommon, Rare, Legendary, Curse)

//    // 각 카드 타입별 세부 효과 (해당 타입에 따라 유효한 값만 사용)
//    public AttackType attackType;   // 공격 카드일 경우
//    public SkillType skillType;     // 스킬 및 방어 효과 카드일 경우
//    public PowerType powerType;     // 파워 카드일 경우 (새 CSV에 추가)
//    public CurseType curseType;     // 저주 카드일 경우

   
//}

//// 카드 희귀도 (CSV "rarity")
//public enum Rarity
//{
//    Common,     // 일반 카드 
//    Rare,       // 희귀 카드 
//    Legendary,  // 전설 카드 
   
//}

//public enum ItemType
//{
//    Attack, // 공격 카드
//    Skill,  // 스킬 카드 (및 방어 효과 카드)
//    Power,  // 파워 카드 (예: 힘의 축복, 오딘의 지혜)
//    Curse   // 저주 카드
//}

//// 공격 카드에 해당하는 타입 (CSV "attack")
//public enum AttackType
//{
//    Smash,           // 강타 (id=1)
//    MultiStrike,     // 연속 타격 (id=3)
//    Stronger,        // 더 강하게! (id=4)
//    CostOfStrike,    // 타격의 대가 (id=7)
//    PoisonFang,      // 독 송곳니 (id=8)
//    FenrirFang,      // 펜리르의 송곳니 (id=14)
//    JotunFist,       // 요툰의 주먹 (id=15)
//    FireStrike,      // 불꽃의 일격 (id=17)
//    BloodDrain       // 흡혈 (id=22)
//}

//// 스킬 카드와 방어 효과 (예: "방어하기")에 해당하는 타입 (CSV "skill")
//public enum SkillType
//{
//    Defend,              // 방어하기 (id=2) – 플레이어의 방어도를 올린다.
//    HeavenlyEnergy,      // 선천진기 (id=6) – 이번 턴에 주는 데미지가 2배가 된다.
//    PoisonMist,          // 독안개 (id=9) – 모든 적에게 독을 부여한다.
//    JormungandAcid,      // 요르문간드의 산성액 (id=10) – 모든 적에게 부여된 독을 2배로 증가시키고, 즉시 데미지를 준다.
//    HeimdallPath,        // 헤임달의 통로 (id=11) – 카드를 1장 뽑는다.
//    ValhallaShield,      // 발할라의 방패 (id=13) – 1턴 동안 적의 공격에 피해를 입지 않는다.
//    HellfireSummon,      // 지옥불 소환 (id=18) – 덱과 패에 화상 카드를 2장씩 추가하고, 적에게 데미지를 준다.
//    SurturFury,          // 수르트의 분노 (id=19) – 덱과 패의 화상 카드 숫자에 X 5 데미지를 주고, 화상 카드들을 소멸시킨다.
//    LokiTrick            // 로키의 장난 (id=20) – 적에게 저주와 약화 상태를 2턴 동안 부여한다.
//}

//// 파워 카드에 해당하는 타입 (CSV "power")
//public enum PowerType
//{
//    BlessingOfStrength,  // 힘의 축복 (id=5) – 공격력이 증가한다.
//    OdinWisdom           // 오딘의 지혜 (id=12) – 최대 마나량이 1 증가한다.
//}

//// 저주 카드에 해당하는 타입 (CSV "curse")
//public enum CurseType
//{
//    Burn,             // 화상 (id=16) – 턴 종료 시 자신에게 5의 데미지를 주고 소멸한다.
//    ButterflyCurse    // 나비의 저주 (id=21) – 사용 불가, 턴 종료 시 소멸한다.
//}

//// ItemSO 스크립트: ScriptableObject를 생성하여 카드 데이터를 보관합니다.
//[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
//public class ItemSO : ScriptableObject
//{
//    public Item[] items;


//}