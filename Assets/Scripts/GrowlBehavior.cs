using UnityEngine;
using System.Collections;

public class GrowlBehavior : MonoBehaviour
{
	public float distance = 5;
	public float direction = 1;
	public float speed = 0.2f;
	
	[HideInInspector]
	private float startX;
	
	void Start()
	{
		startX = transform.position.x;
	}
	
	void Update()
	{
		transform.position = new Vector3(transform.position.x + direction * speed, transform.position.y, transform.position.z);
		
		if (transform.position.x >= startX + distance)
			Destroy(gameObject); 
	}
}
