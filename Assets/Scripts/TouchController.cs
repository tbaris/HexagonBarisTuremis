using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public bool tapSelect;
    public bool turnRight;
    public bool turnLeft;
    public bool isAGemSelected;
    public float turnTreshhold = 2;
    private Vector3 SelectedGemPosition;
    private Vector3 touchStartPoint;
    private Vector3 touchEndPoint;

    

    // Start is called before the first frame update

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Input control for editor
        #region Editor Input 
           
            if (Input.GetMouseButtonDown(0) )
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    touchStartPoint = hit.point;
                }
               
            }
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    touchEndPoint = hit.point;
                }
            inputSense(touchStartPoint, touchEndPoint);
                   
            }


        #endregion

            // Input control for mobile
        #region Touch Input
        if (Input.touchCount > 0 )
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    RaycastHit hit;
                    var ray = Camera.main.ScreenPointToRay(touch.position);

                    if (Physics.Raycast(ray, out hit))
                    {
                        touchStartPoint = hit.point;
                    }
                    break;
                case TouchPhase.Ended:
                    RaycastHit hit2;
                    var ray2 = Camera.main.ScreenPointToRay(touch.position);

                    if (Physics.Raycast(ray2, out hit2))
                    {
                        touchEndPoint = hit2.point;
                    }
                    inputSense(touchStartPoint, touchEndPoint);
                    break;
                    
            }
            
        }
        #endregion



    }
    public void inputSense(Vector3 touchStartPoint,Vector3 touchEndPoint) //Decide to which way to turn depends on swipe rotation
    {
        if (transform.GetComponent<GameController>().playerCanMove)
        {
            if ((touchStartPoint - touchEndPoint).magnitude > turnTreshhold && isAGemSelected)
            {
                Vector3 swipeVector = touchStartPoint - touchEndPoint;
                float turnAngle = Vector3.SignedAngle(touchStartPoint - SelectedGemPosition, touchEndPoint - SelectedGemPosition, Vector3.forward);

               // transform.GetComponent<GameController>().moveCount++;
                if (turnAngle < 0)
                {
                    //Debug.Log("right");
                    transform.GetComponent<GameController>().tryAmove(false);
                }
                else
                {
                  //  Debug.Log("left");
                    transform.GetComponent<GameController>().tryAmove(true);
                }
                isAGemSelected = false;

               
            }
            else
            {
                SelectedGemPosition = touchStartPoint;
                transform.GetComponent<GameController>().sellectGem(touchStartPoint);
                isAGemSelected = true;
            }
        }
        else { Debug.Log("wait!!"); }
    }
}
