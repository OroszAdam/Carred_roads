using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int COIN_SCORE_AMOUNT = 5;

    public static GameManager Instance { get; set; }

    public bool IsDead { get; set; }
    private bool isGameStarted = false;
    private PlayerMotor motor;

    // UI
    public Text scoreText, coinText, modifierText;
    // Scores; 'modifierScore' is the multiplier for 'score' that increases as we go further on the map
    private float score, coinScore, modifierScore;
    private int lastScore;

    private void Awake()
    {
        Instance = this;
        modifierScore = 1;
        motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();

        scoreText.text = score.ToString("0");
        coinText.text = coinScore.ToString("0");
        modifierText.text = "x" + modifierScore.ToString("0.0");
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;

        Application.targetFrameRate = 60;
        Time.timeScale = 1f;
    }

    public void Update()
    {
        //Start running after an initial tap (idle animation yet needed)
        if (InputController.Instance.Tap && !isGameStarted)
        {
            isGameStarted = true;
            motor.StartRunning();
        }

        if (isGameStarted && !IsDead)
        {
            //BUMP the score up
            score += (Time.deltaTime * modifierScore);
            if (lastScore != (int)score)
            {
                lastScore = (int)score;
                scoreText.text = score.ToString("0");
            }
                
        }
    }

    //Every time we get a protein, coinScore grows by 1, and we update the text.
    public void GetCoin()
    {
        coinScore++;
        coinText.text = coinScore.ToString("0");
        score += COIN_SCORE_AMOUNT;
        scoreText.text = score.ToString("0");

    }

    //This function is called in PlayerMotor based on the actual speed we are going.
    public void UpdateModifier(float modifierAmount)
    {
        modifierScore = 1.0f + modifierAmount;
        modifierText.text = "x" + modifierScore.ToString("0.0");
    }

    public void GameOver() 
    {
        
    }

}
