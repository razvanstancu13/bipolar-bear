using UnityEngine;
using System.Collections;

public enum PlayerState : int {None = 0, Aggressive, Hysteric, Ecstatic};
public enum PlayerAbility : int {None = 0, Punch, Growl, SuperJump};

public class PlayerController : MonoBehaviour
{
	// Random Criteria
	public PlayerAbility randomCriteria;
	public int randomCriteriaCounter = 0;

	public Texture2D textureAggressive;
	public Texture2D textureHysteric;
	public Texture2D textureEcstatic;

	public Texture2D textureTextPunchCtrl;
	public Texture2D textureTextPunchSpace;
	public Texture2D textureTextGrawlCtrl;
	public Texture2D textureTextGrawlSpace;
	public Texture2D textureTextSuperJumpW;

	// sounds
	public AudioClip soundJump;
	public AudioClip soundCollectPill;
	public AudioClip soundPunch;
	public AudioClip soundGrawl;
	public AudioClip soundStep;
	public AudioClip soundDeath;

	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	// Abilities
	public float jumpBonus = 3f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private GameLogic _gameLogic;
	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	private AudioSource _audioSource;

	// input
	private bool _right;
	private bool _left;
	private bool _up;
	private bool _action;
	private bool _consume;
	private bool _secondaryAction;

	private float actionTimer = 0;
	public bool inAction = false;

	private float stateTimer = 0;

	public PlayerState _currentState;
	public PlayerAbility _currentAbility;
	public PlayerAbility _extraAbility;
	private float _remainingTimeForCurrentState;
	private float _extraAbilityTimer;
	public bool hasExtraAbility = false;

	public int numberOfPills = 0;

	private float freezeTimer = 0;
	private bool isFrozen = false;

	// Prefabs
	public Transform growlPrefab;

	// GUI Indicators
	public Transform secondaryIndicator;
	public Transform secondaryTextIndicator;
	public Transform stateIndicator;
	public Transform stateTextIndicator;
	public Transform counterText;
	public Transform pillCounterText;

	


	void Awake()
	{
		_gameLogic = GameObject.Find ("GameManager").GetComponent<GameLogic>();
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();
		_audioSource = GetComponent<AudioSource>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}

	void Start() 
	{
		randomCriteria = (PlayerAbility)Random.Range(1,3);
		SetRandomState();

		//BoxCollider2D _boxCollider = gameObject.GetComponent<BoxCollider2D>();
		//_boxCollider.center = new Vector2(0.5f, _boxCollider.center.y);
	}

	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		if (hit.collider.gameObject.name == "Enemy")
		{
			if (!inAction || _currentAbility != PlayerAbility.Punch)
			{
				//Debug.Log( "Collided with enemy : " + hit.collider.gameObject.name);
				_gameLogic.OnPlayerDead();
			}
			else
			{
				if (_currentAbility == PlayerAbility.Punch)
				{
					//Debug.Log( "Killed enemy : " + hit.collider.gameObject.name);
					Destroy (hit.collider.gameObject);
				}
			}
		}
		else if (hit.collider.gameObject.name == "Coin")
		{
			_gameLogic.OnCoinCollected();
			Destroy(hit.collider.gameObject);
			Debug.Log("COIN COLLECTED");
		}
//		else if (hit.collider.gameObject.name == "Pill")
//		{
//			numberOfPills++;
//			Destroy(hit.collider.gameObject);
//			Debug.Log("PILL COLLECTED");
//			//_gameLogic.OnCoinCollected();
//		}
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );

		string _name = col.gameObject.name;
		if (_name == "Pill")
		{
			numberOfPills++;
			pillCounterText.GetComponent<LegacyGUIText>().text = numberOfPills.ToString();
			_audioSource.PlayOneShot(soundCollectPill, 1f);
			Destroy(col.gameObject);
		}
		else if (_name == "props_cristal" || 
		         _name == "props_cristal2" ||
		         _name == "props_cristal3" ||
				_name == "props_tepi" ||
		         _name == "props_vulcan")
		{
			_gameLogic.OnPlayerDead();
		}
	
	}


	void onTriggerExitEvent( Collider2D col )
	{
		Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion

	// the Update loop only gathers input. Actual movement is handled in FixedUpdate because we are using the Physics system for movement
	void Update()
	{
		// a minor bit of trickery here. FixedUpdate sets _up to false so to ensure we never miss any jump presses we leave _up
		// set to true if it was true the previous frame
		if (Input.GetKeyDown(KeyCode.Escape))
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);

		_up = _up || Input.GetKeyDown( KeyCode.W );
		_right = Input.GetKey( KeyCode.D );
		_left = Input.GetKey( KeyCode.A );
		_action = Input.GetKey( KeyCode.Space);
		_consume = Input.GetKey(KeyCode.Return);
		_secondaryAction = Input.GetKey(KeyCode.LeftControl);
	}

	void FixedUpdate()
	{
		if (isFrozen)
		{
			freezeTimer -= Time.deltaTime;
			if (freezeTimer < 0.0f)
			    isFrozen = false;
		}
		else if (randomCriteriaCounter > 5)
		{
			freezeTimer = 5.0f;
			isFrozen = true;
			if (randomCriteria == PlayerAbility.Growl)
				_animator.Play( Animator.StringToHash( "Sleep" ) );
			else if (randomCriteria == PlayerAbility.Punch)
				_animator.Play( Animator.StringToHash( "Cry" ) );
			randomCriteriaCounter = 0;
		}
		
		float orientation = transform.localScale.x < 0f ? -1f : 1f;

		

		//Debug.Log ("Input velocity : " + _velocity.x + "," + _velocity.y + "," + _velocity.z);
		
		if (!isFrozen)
		{
			// grab our current _velocity to use as a base for all calculations
			_velocity = _controller.velocity;

			if( _controller.isGrounded )
				_velocity.y = 0;
			
			if( _right )
			{
				normalizedHorizontalSpeed = 1;
				if( transform.localScale.x < 0f )
					transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
				
				if( _controller.isGrounded )
				{
					_animator.Play( Animator.StringToHash( "Run" ) );
					//_audioSource.PlayOneShot(soundStep, 1f);
				}
			}
			else if( _left )
			{
				normalizedHorizontalSpeed = -1;
				if( transform.localScale.x > 0f )
					transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
				
				if( _controller.isGrounded )
				{
					_animator.Play( Animator.StringToHash( "Run" ) );
					//_audioSource.PlayOneShot(soundStep, 1f);
				}
			}
			else
			{
				normalizedHorizontalSpeed = 0;
				
				if( _controller.isGrounded && !inAction)
					_animator.Play( Animator.StringToHash( "Idle" ) );
			}
			
			
			// we can only jump whilst grounded
			if( _controller.isGrounded && _up )
			{
				_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
				_animator.Play( Animator.StringToHash( "Jump" ) );
				_audioSource.PlayOneShot(soundJump, 1f);
				
			}
	
			if (hasExtraAbility)
			{
				_extraAbilityTimer -= Time.deltaTime;
				if (_extraAbilityTimer <= 0.0f)
				{
					hasExtraAbility = false;
					secondaryIndicator.GetComponent<LegacyGUITexture>().texture = null;
					secondaryTextIndicator.GetComponent<LegacyGUITexture>().texture = null;
				}
			}
			
			if (inAction)
			{
				actionTimer -= Time.deltaTime;
				if (actionTimer <= 0.0f)
				{
					inAction = false;
					if (_currentAbility == PlayerAbility.Punch)
					{
						BoxCollider2D _boxCollider = gameObject.GetComponent<BoxCollider2D>();
						_boxCollider.size = new Vector2(1.0f, _boxCollider.size.y);
					}
					
					//transform.GetChild(0).gameObject.SetActive(false);
				}
			}	
			else if (_action && _currentAbility != PlayerAbility.SuperJump)
			{
				if (randomCriteria == _currentAbility)
					randomCriteriaCounter++;
				
				// Play the action animation
				if (_currentAbility == PlayerAbility.Growl)
				{
					actionTimer = 1.2f;
					inAction = true;
	
					_animator.Play( Animator.StringToHash( "Grawl" ) );

					Transform growlObject = (Transform)Instantiate(growlPrefab, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
					growlObject.gameObject.GetComponent<GrowlBehavior>().direction = orientation;
					growlObject.name = "Growl";
					_audioSource.PlayOneShot(soundGrawl, 1f);
				}
				else if (_currentAbility == PlayerAbility.Punch)
				{
					actionTimer = 0.3f;
					inAction = true;
					_animator.Play( Animator.StringToHash( "Punch" ) );
					_audioSource.PlayOneShot(soundPunch, 1f);
				}
			}
			else if (_secondaryAction && _extraAbility != PlayerAbility.SuperJump)
			{	
				if (randomCriteria == _extraAbility)
					randomCriteriaCounter++;

				// Play the action animation
				if (_extraAbility == PlayerAbility.Growl)
				{
					actionTimer = 1.2f;
					inAction = true;
	
					_animator.Play( Animator.StringToHash( "Grawl" ) );

					Transform growlObject = (Transform)Instantiate(growlPrefab, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
					growlObject.gameObject.GetComponent<GrowlBehavior>().direction = orientation;
					growlObject.name = "Growl";
					_audioSource.PlayOneShot(soundGrawl, 1f);
				}
				else if (_extraAbility == PlayerAbility.Punch)
				{
					actionTimer = 0.3f;
					inAction = true;
					BoxCollider2D _boxCollider = gameObject.GetComponent<BoxCollider2D>();
					_boxCollider.size = new Vector2(2.0f, _boxCollider.size.y);
					_animator.Play( Animator.StringToHash( "Punch" ) );
					_audioSource.PlayOneShot(soundPunch, 1f);
				}
			}
			else if (_consume && !hasExtraAbility)
			{
				if (numberOfPills > 0)
				{
					if (_currentState == PlayerState.Aggressive)
					{
						_extraAbility = PlayerAbility.Growl;
						secondaryIndicator.GetComponent<LegacyGUITexture>().texture = textureHysteric;
						secondaryTextIndicator.GetComponent<LegacyGUITexture>().texture = textureTextGrawlCtrl;
					}
					else if (_currentState == PlayerState.Hysteric)
					{
						_extraAbility = PlayerAbility.SuperJump;
						jumpHeight += jumpBonus;
						secondaryIndicator.GetComponent<LegacyGUITexture>().texture = textureEcstatic;
						secondaryTextIndicator.GetComponent<LegacyGUITexture>().texture = textureTextSuperJumpW;
					}
					else if (_currentState == PlayerState.Ecstatic)
					{
						_extraAbility = PlayerAbility.Punch;
						secondaryIndicator.GetComponent<LegacyGUITexture>().texture = textureAggressive;
						secondaryTextIndicator.GetComponent<LegacyGUITexture>().texture = textureTextPunchCtrl;
					}
	
					Debug.Log ("Consumed a pill. Extra ability : " + _extraAbility);
					_extraAbilityTimer = stateTimer;
					hasExtraAbility = true;
	
					numberOfPills--;
					pillCounterText.GetComponent<LegacyGUIText>().text = numberOfPills.ToString();
				}
			}
		}
		else
		{
			_velocity = new Vector3(0.0f, 0.0f, 0.0f);
		}

		// apply horizontal speed smoothing it
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * runSpeed, Time.fixedDeltaTime * smoothedMovementFactor );
		
		// apply gravity before moving
		_velocity.y += gravity * Time.fixedDeltaTime;

		_controller.move( _velocity * Time.fixedDeltaTime );

		// reset input
		_up = false;

		stateTimer -= Time.deltaTime;
		counterText.GetComponent<LegacyGUIText>().text = Mathf.FloorToInt(stateTimer).ToString();
		if (stateTimer <= 0.0f)
			SetRandomState();
	}

	void SetRandomState()
	{
		if (_currentAbility == PlayerAbility.SuperJump || _extraAbility == PlayerAbility.SuperJump)
			jumpHeight -= jumpBonus;
		
		PlayerState temp = _currentState;
		while (_currentState == temp)
		{
			_currentState = (PlayerState)Random.Range(1, 4);
		}
		_currentAbility = (PlayerAbility)_currentState;
		_extraAbility = PlayerAbility.None;
		
		if (_currentAbility == PlayerAbility.SuperJump)
			jumpHeight += jumpBonus;

		stateTimer = Random.Range(5, 10);
		counterText.GetComponent<LegacyGUIText>().text = Mathf.FloorToInt(stateTimer).ToString();

		if (_currentState == PlayerState.Aggressive)
		{
			stateIndicator.GetComponent<LegacyGUITexture>().texture = textureAggressive;
			stateTextIndicator.GetComponent<LegacyGUITexture>().texture = textureTextPunchSpace;
		}
		else if (_currentState == PlayerState.Hysteric)
		{
			stateIndicator.GetComponent<LegacyGUITexture>().texture = textureHysteric;
			stateTextIndicator.GetComponent<LegacyGUITexture>().texture = textureTextGrawlSpace;
		}
		else if (_currentState == PlayerState.Ecstatic)
		{
			stateIndicator.GetComponent<LegacyGUITexture>().texture = textureEcstatic;
			stateTextIndicator.GetComponent<LegacyGUITexture>().texture = textureTextSuperJumpW;
		}

		Debug.Log("Changed state to : " + _currentState + " for " + stateTimer + " seconds!");
	}
	
	void SetExtraAbility(int ability)
	{
		_extraAbility = (PlayerAbility)ability;
	}
	
	int GetCurrentState()
	{
		return (int)(_currentState);
	}

	public void CollectPill()
	{
		numberOfPills++;
	}

}
