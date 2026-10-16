using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuLogic : MonoBehaviour
{
	public LegacyGUITexture playButton;      // was a GUITexture (removed from Unity), see LegacyGUITexture
	public Texture playButtonUnpressed;
	public Texture playButtonPressed;

	void Start ()
	{
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
			Application.Quit();

		else if (Input.GetMouseButtonDown(0) && playButton.HitTest(Input.mousePosition))
		{
			playButton.texture = playButtonPressed;
		}
		else if (Input.GetMouseButtonUp(0) && playButton.HitTest(Input.mousePosition))
		{
			playButton.texture = playButtonUnpressed;
			SceneManager.LoadScene(1);
		}
	}
}
