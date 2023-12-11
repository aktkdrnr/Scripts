using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


// MonopolyBoard 타입의 객체에 대한 커스텀 에디터를 정의합니다.
[CustomEditor(typeof(MonopolyBoard))]
public class NodeSetEditor : Editor
{
    // MonopolyBoard의 nodeSetList 프로퍼티에 대한 참조를 저장합니다.
    SerializedProperty nodeSetListProperty;

    // 에디터가 활성화될 때 호출됩니다.
    void OnEnable()
    {
        // serializedObject를 통해 nodeSetList 프로퍼티를 찾아 연결합니다.
        nodeSetListProperty = serializedObject.FindProperty("nodeSetList");
    }

    // 인스펙터 GUI를 커스텀합니다.
    public override void OnInspectorGUI()
    {
        // serializedObject의 정보를 업데이트합니다.
        serializedObject.Update();

        // 현재 선택된 MonopolyBoard 오브젝트를 가져옵니다.
        MonopolyBoard monopolyBoard = (MonopolyBoard)target;

        // nodeSetList 프로퍼티에 대한 에디터 필드를 생성합니다.
        EditorGUILayout.PropertyField(nodeSetListProperty, true);

        // "Change Image Colors" 버튼을 만들고, 클릭 이벤트를 처리합니다.
        if (GUILayout.Button("Change Image Colors"))
        {
            // Undo 기능을 위해 현재 상태를 기록합니다.
            Undo.RecordObject(monopolyBoard, "Change Image Colors");
            // 모든 NodeSet에 대해 반복합니다.
            for (int i = 0; i < monopolyBoard.nodeSetList.Count; i++)
            {
                MonopolyBoard.NodeSet nodeSet = monopolyBoard.nodeSetList[i];
                // 각 NodeSet 내의 모든 노드에 대해 반복합니다.
                for (int j = 0; j < nodeSet.nodesInSetList.Count; j++)
                {
                    MonopolyNode node = nodeSet.nodesInSetList[j];
                    Image image = node.propertyColorField;
                    // 해당 노드에 이미지가 있을 경우 색상을 변경합니다.
                    if (image != null)
                    {
                        // Undo 기능을 위해 이미지의 현재 상태를 기록합니다.
                        Undo.RecordObject(image, "Change Image Color");
                        // 이미지의 색상을 NodeSet의 색상으로 설정합니다.
                        image.color = nodeSet.setColor;
                    }
                }
            }
        }

        // 변경된 serializedObject의 정보를 적용합니다.
        serializedObject.ApplyModifiedProperties();
    }
}
