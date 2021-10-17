using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SlimeIA))]
public class SlimeIAEditor : Editor
{
    void OnSceneGUI()
    {
        SlimeIA slimeIA = (SlimeIA)target;
        Handles.color = Color.green;
        Handles.DrawWireDisc((slimeIA.GetInitialPosition() != Vector3.zero ? slimeIA.GetInitialPosition() : slimeIA.transform.position), Vector3.up, slimeIA.GetMovimentRange());
    }
}
