﻿using UnityEditor;
using UnityEngine;

namespace Crescendo.API.Editor {

    [CustomEditor(typeof (CharacterEditorData))]
    public class CharacterEditorDataEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (GUILayout.Button("Open Character Editor")) {
                CharacterEditorWindow window = CharacterEditorWindow.ShowWindow();
                window.Target = target as CharacterEditorData;
            }
        }

    }

}