using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    Vector3 initPos;

    public const float LANE_DISTANCE = 13;
    private const float TURN_SPEED = 0.1f;

    //
    private bool isRunning = false;

    //Animation

    private Animator anim;


    // Movement
    private CharacterController controller;
    private Collider collider;
    private float jumpForce = 37.0f;
    private float gravity = 45f;
    private float verticalVelocity;


    //-1 is leftmost, 0 is left lane, 1 is right, 2 is rightmost
    private int desiredLane = 1;

    //condition to move, minimal distance


    //Speed modification
    public float originalSpeed = 60.0f;
    private float speed;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 2.5f;
    private float speedIncreaseAmount = 0.1f;

    private void Start()
    {
        speed = originalSpeed;
        controller = GetComponent<CharacterController>();
        collider = GetComponent<BoxCollider>();
        //Debug.Log(collider);
        anim = GetComponent<Animator>();
        initPos = transform.position;
    }

    private void Update()
    {
        // Don't do anything until game is started
        if (!isRunning) return;

        // Speed modification
        if(Time.time - speedIncreaseLastTick > speedIncreaseTime)
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;

            //Change the modifier and it's text.
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }

        // Get the input about which lane to be on
        if (InputController.Instance.SwipeLeft)
            MoveLane(false);
        if (InputController.Instance.SwipeRight)
            MoveLane(true);

        // Calculate where we should be
        Vector3 targetPosition = initPos + transform.position.z * Vector3.forward;

        //leftmost
        if (desiredLane == -1){
            targetPosition += Vector3.left * 2 * LANE_DISTANCE + new Vector3(-3, 0, 0) ;
            //Debug.Log(targetPosition);
            }
        // opposite inner lane
        else if (desiredLane == 0){
            targetPosition +=  Vector3.left *  LANE_DISTANCE + new Vector3(-2, 0, 0) ;
            //Debug.Log(targetPosition);
            }
        // rightmost
        else if (desiredLane == 2){
            targetPosition += Vector3.right * LANE_DISTANCE ;
            //Debug.Log(targetPosition);
            }

        

        //Calculate move delta
        Vector3 moveVector = Vector3.zero;

        // Side (lane change)
        // The constant multiplier is based purely on tuning by experience
        moveVector.x = (targetPosition - transform.position).normalized.x * 50;

        bool isGrounded = IsGrounded();
        anim.SetBool("Grounded", isGrounded);

        // // Calculate Y
        // if (isGrounded)
        // {
        //     //verticalVelocity = -0.1f;

        //     if(InputController.Instance.SwipeUp)
        //     {
        //         // Jump
        //         anim.SetInteger("RandomJump", Random.Range(1, 3));
        //         anim.SetTrigger("Jump");
        //         verticalVelocity = jumpForce;
        //     }
        //     // else if(InputController.Instance.SwipeDown)
        //     // {
        //     //     //Slide
        //     //     StartSliding();
        //     //     Invoke("StopSliding", 1.0f);
        //     // }
        // }
        // else
        // {
        //     verticalVelocity -= (gravity * Time.deltaTime);

        //     // Fall if downswipe in air
        //     if(InputController.Instance.SwipeDown)
        //     {
        //         verticalVelocity = -jumpForce;
        //     }
        // }

        // Down
        moveVector.y = verticalVelocity;

        // Forward
        moveVector.z = speed;

        // Moving the character
        //controller.Move(moveVector * Time.deltaTime);

        this.gameObject.transform.position += moveVector * Time.deltaTime; 

        // // Rotating the character a bit when changing lanes
        // Vector3 dir = controller.velocity;
        // if (dir != Vector3.zero)
        // {
        //     dir.y = 0;
        //     //The forward vector of the character is where the velocity is pointing
        //     transform.forward = Vector3.Lerp(transform.forward, -dir, TURN_SPEED);
        // }
    }
 
    //EZT MÉG MAJD VÁLTOZTATOM SZTEM BRO
    // private void StartSliding()
    // {
    //     anim.SetBool("Sliding", true);
    //     controller.height /= 2;
    //     controller.center = new Vector3(controller.center.x, controller.center.y / 2, controller.center.z);
    // }

    // private void StopSliding()
    // {
    //     anim.SetBool("Sliding", false);
    //     controller.height *= 2;
    //     controller.center = new Vector3(controller.center.x, controller.center.y * 2, controller.center.z);
    // }

    private void MoveLane(bool goingRight)
    {
        desiredLane += (goingRight) ? 1 : -1;
        desiredLane = Mathf.Clamp(desiredLane, -1, 2);
        //Debug.Log(desiredLane);
    }

    private bool IsGrounded()
    {
        // Cast a ray downwards from the bottom of the character controller 
        Ray groundRay = new Ray(new Vector3(
                collider.bounds.center.x,
                (collider.bounds.center.y - collider.bounds.extents.y) + 0.5f,
                collider.bounds.center.z),
            Vector3.down);

        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.cyan, 1.0f);

        return (Physics.Raycast(groundRay, 0.5f + 0.1f));
    }

    public void StartRunning()
    {
        isRunning = true;
        anim.SetTrigger("StartRunning");
    }


    private void Crash()
    {
        //Play death aniamation
        anim.SetTrigger("Death");

        //Stop running
        isRunning = false;

        GameManager.Instance.IsDead = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                //Collision to an obstacle means rip bro
                Crash();
            break;
        }
    }
}
