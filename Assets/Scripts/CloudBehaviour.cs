using UnityEngine;
using System.Collections;

public class CloudBehaviour : MonoBehaviour
{

	public float stopX;
	public float respawnX;
	public float speed;

	void Start()
	{
	}
	
	void Update()
	{
		transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
		
		if (transform.position.x < stopX)
			transform.position = new Vector3(respawnX, transform.position.y, transform.position.z);
	}
}
