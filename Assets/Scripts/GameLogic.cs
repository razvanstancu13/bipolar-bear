using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour
{
	public PlayerController Player;

	[HideInInspector]
	private int _coinVaue = 1;
	private int _score;

	void Start()
	{
		_score = 0;
	}
	
	void Update()
	{
	
	}

	public void OnPlayerDead()
	{
		// Re-start from checkpoint
		UnityEngine.SceneManagement.SceneManager.LoadScene(1);
	}

	public void OnCoinCollected()
	{
		_score += _coinVaue;
	}
}
