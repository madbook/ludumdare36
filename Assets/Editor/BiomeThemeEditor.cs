using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (BiomeTheme))]
public class BiomeThemeEditor : Editor {
    BoardDisplay display;

    void OnEnable () {
        display = FindObjectOfType<BoardDisplay> ();
    }

    public override void OnInspectorGUI() {
		BiomeTheme theme = (BiomeTheme)target;

		if (DrawDefaultInspector()) {
			if (display != null && display.autoUpdate) {
                theme.ResetDoodads ();
				display.DrawBoard ();
			}
		}
	}
}
