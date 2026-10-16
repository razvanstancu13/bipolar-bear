using UnityEngine;
using System.Collections;

public enum MoveDirection : int {Left = 0, Right};

public class EnemyLogic : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float moveSpeed = 4f;
	public float moveRadius = 5f;
	public float groundDamping = 20f;
	public Vector3 _velocity;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;
	private float initialPositionX;
	private MoveDirection direction;
	private bool stunned = false;
	private float stunTimer;

	
	private Animator _animator;

	void Awake()
	{
		_animator = GetComponent<Animator>();
		direction = MoveDirection.Right;
		normalizedHorizontalSpeed = 1;
	}

	void Start()
	{
		initialPositionX = transform.position.x;
	}
	
	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.name == "Growl")
		{
			Destroy(coll.gameObject);
			_animator.Play(Animator.StringToHash("EskimoIdle"));
			Stun();
		}
	}	

	void Update()
	{
	}
	
	void FixedUpdate()
	{
		if (stunned)
		{
			stunTimer -= Time.deltaTime;

			if (stunTimer <= 0.0f)
				stunned = false;
		}
		else
		{
			if (direction == MoveDirection.Right)
			{
				if (transform.localScale.x > 0f)
					transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	
				_animator.Play(Animator.StringToHash("EskimoWalk"));
				if (transform.position.x > initialPositionX + moveRadius)
				{
					direction = MoveDirection.Left;
					normalizedHorizontalSpeed = -1;
				}
			}
			else
			{
				if (transform.localScale.x < 0f)
					transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	
				_animator.Play(Animator.StringToHash("EskimoWalk"));
				if (transform.position.x < initialPositionX - moveRadius)
				{
					direction = MoveDirection.Right;
					normalizedHorizontalSpeed = 1;
				}
			}
	
			// grab our current _velocity to use as a base for all calculations
			_velocity = new Vector3(normalizedHorizontalSpeed * 8f, 0f);
			// apply horizontal speed smoothing it
			var smoothedMovementFactor = groundDamping; // how fast do we change direction?
			_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * moveSpeed, Time.fixedDeltaTime * smoothedMovementFactor );
			
			// Update the velocity of the object
			GetComponent<Rigidbody2D>().linearVelocity = _velocity;
		}
	}

	void Stun()
	{
		GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0.0f, 0.0f);
		stunned = true;
		stunTimer = 3f;

		_animator.Play(Animator.StringToHash ("EskimoIdle"));
	}
}
