using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기반 클래스의 기능 유지
        // (원한다면 base.OnInspectorGUI();를 지우고 완전히 커스텀으로 구성 가능)
        //  base.OnInspectorGUI(); // <- 전체 자동 표시 (비활성화하면 커스텀만 표기)

        // 직렬화된 객체 업데이트 시작
        serializedObject.Update();

        // "items" 배열 찾아오기
        SerializedProperty itemsProp = serializedObject.FindProperty("items");

        // 배열을 펼쳐서 그리기
        EditorGUILayout.PropertyField(itemsProp, new GUIContent("Items"), true);

        // 배열이 FoldOut으로 펼쳐지면, 내부 요소(각 Item)에 대해 커스텀 표시
        if (itemsProp.isExpanded)
        {
            EditorGUI.indentLevel++;

            // 배열 크기 수정
            EditorGUILayout.PropertyField(itemsProp.FindPropertyRelative("Array.size"));

            // 각 요소(Item)에 접근
            for (int i = 0; i < itemsProp.arraySize; i++)
            {
                SerializedProperty elementProp = itemsProp.GetArrayElementAtIndex(i);

                // 한 아이템에 대한 블록 그리기
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Item {i}", EditorStyles.boldLabel);

                // item.name
                SerializedProperty nameProp = elementProp.FindPropertyRelative("name");
                EditorGUILayout.PropertyField(nameProp);

                // item.sprite
                SerializedProperty spriteProp = elementProp.FindPropertyRelative("sprite");
                EditorGUILayout.PropertyField(spriteProp);

                // item.type
                SerializedProperty typeProp = elementProp.FindPropertyRelative("type");
                EditorGUILayout.PropertyField(typeProp);

                // item.percent
                SerializedProperty percentProp = elementProp.FindPropertyRelative("percent");
                EditorGUILayout.PropertyField(percentProp);

                // 이제 type에 따라 attackType, defenseType, skillType, curseType 중 하나만 노출
                ItemType currentType = (ItemType)typeProp.enumValueIndex;
                switch (currentType)
                {
                    case ItemType.Attack:
                        SerializedProperty attackProp = elementProp.FindPropertyRelative("attackType");
                        EditorGUILayout.PropertyField(attackProp, new GUIContent("Attack Type"));
                        break;
             

                    case ItemType.Skill:
                        SerializedProperty skillProp = elementProp.FindPropertyRelative("skillType");
                        EditorGUILayout.PropertyField(skillProp, new GUIContent("Skill Type"));
                        break;

                    case ItemType.Curse:
                        SerializedProperty curseProp = elementProp.FindPropertyRelative("curseType");
                        EditorGUILayout.PropertyField(curseProp, new GUIContent("Curse Type"));
                        break;
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }

        // 변경 사항 적용
        serializedObject.ApplyModifiedProperties();
    }
}
