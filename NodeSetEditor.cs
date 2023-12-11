using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


// MonopolyBoard Ÿ���� ��ü�� ���� Ŀ���� �����͸� �����մϴ�.
[CustomEditor(typeof(MonopolyBoard))]
public class NodeSetEditor : Editor
{
    // MonopolyBoard�� nodeSetList ������Ƽ�� ���� ������ �����մϴ�.
    SerializedProperty nodeSetListProperty;

    // �����Ͱ� Ȱ��ȭ�� �� ȣ��˴ϴ�.
    void OnEnable()
    {
        // serializedObject�� ���� nodeSetList ������Ƽ�� ã�� �����մϴ�.
        nodeSetListProperty = serializedObject.FindProperty("nodeSetList");
    }

    // �ν����� GUI�� Ŀ�����մϴ�.
    public override void OnInspectorGUI()
    {
        // serializedObject�� ������ ������Ʈ�մϴ�.
        serializedObject.Update();

        // ���� ���õ� MonopolyBoard ������Ʈ�� �����ɴϴ�.
        MonopolyBoard monopolyBoard = (MonopolyBoard)target;

        // nodeSetList ������Ƽ�� ���� ������ �ʵ带 �����մϴ�.
        EditorGUILayout.PropertyField(nodeSetListProperty, true);

        // "Change Image Colors" ��ư�� �����, Ŭ�� �̺�Ʈ�� ó���մϴ�.
        if (GUILayout.Button("Change Image Colors"))
        {
            // Undo ����� ���� ���� ���¸� ����մϴ�.
            Undo.RecordObject(monopolyBoard, "Change Image Colors");
            // ��� NodeSet�� ���� �ݺ��մϴ�.
            for (int i = 0; i < monopolyBoard.nodeSetList.Count; i++)
            {
                MonopolyBoard.NodeSet nodeSet = monopolyBoard.nodeSetList[i];
                // �� NodeSet ���� ��� ��忡 ���� �ݺ��մϴ�.
                for (int j = 0; j < nodeSet.nodesInSetList.Count; j++)
                {
                    MonopolyNode node = nodeSet.nodesInSetList[j];
                    Image image = node.propertyColorField;
                    // �ش� ��忡 �̹����� ���� ��� ������ �����մϴ�.
                    if (image != null)
                    {
                        // Undo ����� ���� �̹����� ���� ���¸� ����մϴ�.
                        Undo.RecordObject(image, "Change Image Color");
                        // �̹����� ������ NodeSet�� �������� �����մϴ�.
                        image.color = nodeSet.setColor;
                    }
                }
            }
        }

        // ����� serializedObject�� ������ �����մϴ�.
        serializedObject.ApplyModifiedProperties();
    }
}
