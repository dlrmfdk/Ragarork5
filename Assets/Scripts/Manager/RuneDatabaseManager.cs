using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuneDatabaseManager : MonoBehaviour
{
    public static RuneDatabaseManager Instance { get; private set; }
    public Dictionary<string, RuneData> runeDataDict;
    public Dictionary<string, Sprite> iconDict;
    public Dictionary<string, BaseRuneEffectSO> effectDict;

    [Tooltip("에디터에서 모든 BaseRuneEffectSO 에셋을 연결")]
    [SerializeField] private List<BaseRuneEffectSO> effectSOList;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 1) JSON 파일 로드 및 존재 여부 확인
        TextAsset ta = Resources.Load<TextAsset>("RunesDatabase");
        if (ta == null)
        {
            Debug.LogError("RunesDatabase.json 파일을 Resources 폴더에 넣었는지 확인하세요.");
            return;
        }
        // 로드된 텍스트를 그대로 출력해 봅니다
        Debug.Log("Loaded JSON:\n" + ta.text);

        // 2) JSON 파싱 및 형식 검증
        var database = JsonUtility.FromJson<RuneDatabase>(ta.text);
        if (database == null || database.runes == null)
        {
            Debug.LogError("RunesDatabase.json 파싱에 실패했습니다. JSON 문법을 확인하세요.");
            return;
        }

        // 3) 룬 스펙 사전 구성 (파싱이 성공했을 때만 실행)
        runeDataDict = database.runes.ToDictionary(r => r.id, r => r);
        iconDict = database.runes.ToDictionary(r => r.id, r => Resources.Load<Sprite>(r.iconPath));
        effectDict = effectSOList.ToDictionary(e => e.name, e => e);

        Debug.Log($"룬 데이터베이스 로드 완료: {database.runes.Count}개 룬");
    }

} 
