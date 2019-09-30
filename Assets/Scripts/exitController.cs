using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exitController : MonoBehaviour
{
    public GameObject quitobject;
    public GameObject noMoveLeft;
    public GameObject bombExploded;
    private GameController gameCont;
    public bool clickedBefore = false;
    public bool clickedBeforeButton = false;
    public bool secondClick = false;
    private bool isGameOverStarted;
    public bool quitGame = false;


    void Start()
    {
        gameCont = transform.GetComponent<GameController>();
    }
    void Update()
    {
        if ((Input.GetKey(KeyCode.Escape) || quitGame) && !clickedBefore )
        {
            clickedBefore = true;
            quitobject.SetActive(true);
            StartCoroutine(quitingTimer());
            quitGame = false;
        }
    }
    public void QuitGameButton()
    {
        quitGame = true;
    }

    public void GameOver(bool isGameOver) //invokes gameovertimer  

    {
        if(!isGameOver)
        {
            if(!clickedBeforeButton)
            {
                clickedBeforeButton = true;
                quitobject.SetActive(true);
                StartCoroutine(gameOverTimer(isGameOver));
                secondClick = false;
            }
            else
            {
                secondClick = true;
            }

        }
        else
        {
            clickedBeforeButton = true;
            if(gameCont.isBombExploded)
            {
                bombExploded.SetActive(true);
            }
            else
            {
                noMoveLeft.SetActive(true);
            }
          
            StartCoroutine(gameOverTimer(isGameOver));
            secondClick = false;
        }
    }


    IEnumerator gameOverTimer(bool isGameOver)//returns to main menu with gameover
    {

        yield return null;

        const float timerTime = 3f;
        float counter = 0;

        while (counter < timerTime)
        {
            counter += Time.deltaTime;
            if (secondClick) 
            {
                gameCont.GameOver();
                
                secondClick = false;
                Debug.Log("1");
                
            }
            yield return null;
        }
        if (isGameOver && !isGameOverStarted)
        {
           
                gameCont.GameOver();
           
            Debug.Log("2");
            isGameOverStarted = true;
          
           
        }
        quitobject.SetActive(false);
        noMoveLeft.SetActive(false);
        bombExploded.SetActive(false);
        isGameOverStarted = false;
        secondClick = false;
        clickedBeforeButton = false;
        
    }

    IEnumerator quitingTimer()
    {

        yield return null;

        const float timerTime = 3f;
        float counter = 0;

        while (counter < timerTime)
        {
            counter += Time.deltaTime;
            if ((Input.GetKey(KeyCode.Escape) || quitGame))
            {
                Quit();
            }
            yield return null;
        }
   
        quitobject.SetActive(false);
       

        clickedBefore = false;
    }

    void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif

#if UNITY_ANDROID
        Application.Quit();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }

}
