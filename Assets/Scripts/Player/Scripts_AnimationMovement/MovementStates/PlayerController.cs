using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;

public class PlayerController : MonoBehaviour // player movement
{  
    CharacterController controller;
  
    public IdleStates Idle = new IdleStates();
    public WalkStates Walking = new WalkStates();
    public RunStates Running = new RunStates();

    [HideInInspector] public Animator anim;
    MovementBaseState currentState;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float gravity = -10.0f;

    
    public float currentMoveSpeed;
    public float walkSpeed = 3, walkBackSpeed = 2;
    public float runSpeed = 6, runBackSpeed = 5;

    [SerializeField] GameObject diedOfColdPrompt;
    [SerializeField] GameObject diedZombiePrompt;

    Vector3 velocity; 
    [HideInInspector] public Vector3 dir;
    [HideInInspector] public float hzInput, vInput;

    [SerializeField] AudioSource WalkAudio;
    [SerializeField] AudioClip walkAudioClip;
    protected float timeBetweenSteps = 0.6f;

    public static System.Action gamePaused;

    bool isMove;
    bool isRun;
    float timer;

    void Start()
    {
        WalkAudio.clip = walkAudioClip;

        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        SwitchState(Idle);     
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gamePaused?.Invoke();
        }

        GetDirectionAndMove();
        currentState.UpdateState(this);
        anim.SetFloat("hzInput",hzInput);
        anim.SetFloat("vInput",vInput);    
    }


    public void SwitchState(MovementBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);       
    }
 
    void FootStep()
    {
        isMove = (hzInput != 0 || vInput != 0) ? true : false;
       
        switch (isRun)
        {
            case true: 
                timeBetweenSteps = 0.3f;
                break;
            case false: 
                timeBetweenSteps = 0.6f;
                break;
        }

        if (isMove)
        {
            WalkAudio.clip = walkAudioClip;
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = timeBetweenSteps;
                WalkAudio.pitch = UnityEngine.Random.Range(0.95f, 1.15f);
                WalkAudio.Play();
            }
        }
        
        if (!isMove)
        {
            timer = timeBetweenSteps;
            WalkAudio.Stop();
        }
    }

    void GetDirectionAndMove()
    {
        hzInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        dir = transform.forward * vInput + transform.right * hzInput;

        FootStep();

        if (Input.GetKey(KeyCode.LeftShift))
            {
                isRun = true;
                Run();
            }
            else
            {
                isRun = false;
                Walk();
            }
      
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Walk()
    {
        moveSpeed = walkSpeed;
        controller.Move(dir.normalized * moveSpeed * Time.deltaTime);
    }

    void Run()
    {
        moveSpeed = runSpeed;
        controller.Move(dir.normalized * moveSpeed * Time.deltaTime);
    }

    void PlayerOnDeathAnim(bool val)
    {        
        CancelInvoke();
        if(val)
        {
            anim.SetTrigger("diedofCold");   
            StartCoroutine(ColdPromptUp());
        }

        if(!val)
        {
            anim.SetTrigger("diedByZombie");
            StartCoroutine(KilledByZombiePrompt());
        }
    }

    private void OnEnable()
    {
        Character.PlayerKilled += PlayerOnDeathAnim;
        Stove.PlayerDiedofCold += PlayerOnDeathAnim;
    }

    private void OnDisable()
    {
        Character.PlayerKilled -= PlayerOnDeathAnim;
        Stove.PlayerDiedofCold -= PlayerOnDeathAnim;
    }
    IEnumerator ColdPromptUp()
    {
        yield return new WaitForSeconds(2f);
        diedOfColdPrompt.SetActive(true);
        StartCoroutine(LoadMenuScene());
    }

    IEnumerator KilledByZombiePrompt()
    {
        yield return new WaitForSeconds(2f);

        diedZombiePrompt.SetActive(true);
        StartCoroutine(LoadMenuScene());
    }
    IEnumerator LoadMenuScene()
    {
        yield return new WaitForSeconds(3f);
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(3);
    }
}
