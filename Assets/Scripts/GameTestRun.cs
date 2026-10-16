using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

// Test run, only when the game is started with  -bear-test <folder> [walk]  (does nothing in normal play):
// screenshot of the menu, then the level; logs where the bear is, its pills, every level (re)load and every error;
// then quits. walk = the bear holds "right" (D) the whole time, as if the key were pressed.
public class GameTestRun : MonoBehaviour
{
	string _dir, _mode = "";
	int _loads, _errors;
	float _levelStart;
	static readonly FieldInfo Right = typeof(PlayerController).GetField("_right", BindingFlags.NonPublic | BindingFlags.Instance);

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Start0()
	{
		var args = System.Environment.GetCommandLineArgs();
		int i = System.Array.IndexOf(args, "-bear-test");
		if (i < 0 || i + 1 >= args.Length)
			return;
		var go = new GameObject("GameTestRun");
		DontDestroyOnLoad(go);
		var t = go.AddComponent<GameTestRun>();
		t._dir = args[i + 1];
		if (i + 2 < args.Length && !args[i + 2].StartsWith("-"))
			t._mode = args[i + 2];
	}

	void Awake()
	{
		Application.logMessageReceived += (msg, stack, type) =>
		{
			if (type == LogType.Error || type == LogType.Exception)
			{
				_errors++;
				if (_errors <= 20)
					Debug.Log("BEARTEST error: " + msg + " | " + stack.Split('\n')[0]);
			}
		};
		SceneManager.sceneLoaded += (s, m) =>
		{
			_loads++;
			var p = FindAnyObjectByType<PlayerController>();
			Debug.Log("BEARTEST loaded " + s.name + " at " + Time.time.ToString("F1") + " s" +
			          (_loads > 2 ? " (after " + (Time.time - _levelStart).ToString("F1") + " s in the level)" : ""));
			_levelStart = Time.time;
		};
	}

	void Update()
	{
		// PlayerController reads the keys in Update; this runs after it (LateUpdate would too), before FixedUpdate moves
		if (_mode != "walk")
			return;
		var p = FindAnyObjectByType<PlayerController>();
		if (p) Right.SetValue(p, true);
	}

	void LateUpdate() => Update();

	IEnumerator Start()
	{
		Directory.CreateDirectory(_dir);
		yield return new WaitForSeconds(1.5f);
		yield return Shot("1-menu");
		SceneManager.LoadScene(1);
		foreach (var t in new[] { 1.5f, 2f, 3f, 4f })
		{
			yield return new WaitForSeconds(t);
			var p = FindAnyObjectByType<PlayerController>();
			Debug.Log("BEARTEST t=" + Time.time.ToString("F1") + " scene=" + SceneManager.GetActiveScene().name +
			          " bear=" + (p ? p.transform.position.ToString("F1") + " state=" + p._currentState + " pills=" + p.numberOfPills + " sprite=" + (p.GetComponent<SpriteRenderer>().sprite ? p.GetComponent<SpriteRenderer>().sprite.name : "NONE") : "none") +
			          " loads=" + _loads + " errors=" + _errors);
			yield return Shot("2-level-" + Time.time.ToString("F0"));
		}
		Debug.Log("BEARTEST done: mode=" + (_mode == "" ? "normal" : _mode) + " loads=" + _loads + " errors=" + _errors);
		Application.Quit();
	}

	IEnumerator Shot(string name)
	{
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(Path.Combine(_dir, (_mode == "" ? "" : _mode + "-") + name + ".png"));
		yield return null;
	}
}
