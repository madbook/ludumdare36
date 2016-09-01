using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (BoardDisplay))]
public class BoardDisplayEditor : Editor {
	public override void OnInspectorGUI() {
		BoardDisplay display = (BoardDisplay)target;

		if (DrawDefaultInspector()) {
			if (display.drawOnEditorUpdate) {
				display.DrawBoard ();
			}
		}

		if (GUILayout.Button ("Refresh")) {
			display.DrawBoard ();
		}
	}
}
