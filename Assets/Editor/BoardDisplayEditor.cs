using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (BoardDisplay))]
public class BoardDisplayEditor : Editor {
	public override void OnInspectorGUI() {
		BoardDisplay display = (BoardDisplay)target;

		if (DrawDefaultInspector()) {
			if (display.autoUpdate) {
				display.DrawBoard ();
			}
		}

		if (GUILayout.Button ("Refresh")) {
			display.DrawBoard ();
		}
	}
}
