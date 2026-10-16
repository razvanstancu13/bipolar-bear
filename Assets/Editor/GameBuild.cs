using System.Linq;
using UnityEditor;
using UnityEngine;

// Builds the game: menu "Bipolar Bear > Build Windows", or -executeMethod GameBuild.Windows. Output: Builds/Windows.
public static class GameBuild
{
	static readonly string[] Scenes = { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/GameScene.unity" };

	[MenuItem("Bipolar Bear/Build Windows")]
	public static void Windows()
	{
		// the menu and a restart load scene 1, Esc in the level loads scene 0, so this order matters
		EditorBuildSettings.scenes = Scenes.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
		var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions
		{
			scenes = Scenes,
			locationPathName = "Builds/Windows/BipolarBear.exe",
			target = BuildTarget.StandaloneWindows64,
		});
		Debug.Log("BB build: " + r.summary.result + ", " + r.summary.totalErrors + " errors, " + (r.summary.totalSize / 1048576f).ToString("F1") + " MB");
		if (Application.isBatchMode)
			EditorApplication.Exit(r.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
	}
}
