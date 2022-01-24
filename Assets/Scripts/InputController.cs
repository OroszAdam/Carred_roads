using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private const float DEADZONE = 100.0f;
    public static InputController Instance { set; get; }

    private bool tap, swipeLeft, swipeRight, swipeUp, swipeDown;
    private Vector2 swipeDelta, startTouch;

    public bool Tap { get { return tap; } }
    public Vector2 SwipeDelta { get { return swipeDelta; } }
    public bool SwipeLeft { get { return swipeLeft; } }
    public bool SwipeRight { get { return swipeRight; } }
    public bool SwipeUp { get { return swipeUp; } }
    public bool SwipeDown { get { return swipeDown; } }

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Reseting all the booleans
        tap = swipeLeft = swipeRight = swipeDown = swipeUp = false;

        //Check for input
        #region Standalone Inputs
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (Input.GetMouseButtonDown(0))
            {
                tap = true;
                startTouch = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                startTouch = swipeDelta = Vector2.zero;
            }
        }
        #endregion

        #region Mobile Inputs
        else if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            if (Input.touches.Length != 0)
            {
                if (Input.touches[0].phase == TouchPhase.Began) //"touches[0]" means first finger to touch
                {
                    tap = true;
                    startTouch = Input.mousePosition;
                }
                else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled) //canceled means that something happens (like an incoming call), so the touch ends
                {
                    startTouch = swipeDelta = Vector2.zero;
                }
            }
        }

        #endregion

        //Calculate distance
        swipeDelta = Vector2.zero;
        if (startTouch != Vector2.zero)
        {
            //Check with mobile
            if (Input.touches.Length != 0)
            {
                swipeDelta = Input.touches[0].position - startTouch;
            }
            //Check with standalone
            else if (Input.GetMouseButton(0))
            {
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
            }
        }

        //Check if we're beyond a deadzone
        if (swipeDelta.magnitude > DEADZONE)
        {
            //This is a confirmed swipe
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                //Left or right
                if (x < 0)
                {
                    swipeLeft = true;
                    //Debug.Log("Left");
                }
                else
                {
                    swipeRight = true;
                    //Debug.Log("Right");
                }

            }
            else
            {
                //Up or down
                if (y < 0)
                {
                    swipeDown = true;
                    //Debug.Log("Down");
                }

                else
                {
                    swipeUp = true;
                    //Debug.Log("Up");
                }

            }

            // Reset
            startTouch = swipeDelta = Vector2.zero;
        }
    }
}
