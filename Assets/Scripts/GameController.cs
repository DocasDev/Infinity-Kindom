using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public enum EnemyState
{
    IDLE, ALERT, PATROL, FURY, FOLLOW, EXPLORE, RESETPOSITION, DIE
}

public enum GameState
{
    GAMEPLAY, DIE
}

public class GameController : MonoBehaviour
{
    [Header("Global Config")]
    public Transform player;
    public GameState currentGameState;

    [Header("UI")]
    public Text txtGem;
    public Image HPBarFilled;

    [Header("Drop Config")]
    public GameObject gemPrefab;
    [Range(0, 100)]
    public int gemPercDrop = 25;

    [Header("Slime IA")]
    public float slimeIdleWaitTime = 5f;
    public float slimeAlertWaitTime = 3f;
    public float slimeDistanceToAttack = 2.3f;
    public float slimeAttackDelay = 1.5f;
    public float slimeLookAtSpeed = 1f;

    [Header("Rain Config")]
    public float rainDurationDelay = 30f;
    public float rainIncrementDelay = 3f;
    public float rainIncrement = 100f;
    public int rainMaxAmountEmition = 5000;
    public int rainMinAmountEmition = 1000;

    [Header("Day Config")]
    public PostProcessVolume postB;
    public float dayIncrementDelay = .05f;

    private bool isNight;
    private int gems;

    private void Start()
    {
        txtGem.text = gems.ToString();
    }

    private void Update()
    {
        if (postB.weight >= 1f)
        {
            isNight = true;
        }
        else if (postB.weight <= 0)
        {
            isNight = false;
        }

        if (isNight)
        {
            postB.weight -= dayIncrementDelay * Time.deltaTime;
        }
        else
        {
            postB.weight += dayIncrementDelay * Time.deltaTime;
        }
    }

    public void ChangeGameState(GameState newGameState)
    {
        currentGameState = newGameState;
    }

    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }

    public void SetGems(int amount)
    {
        gems += amount;
        txtGem.text = gems.ToString();
    }

    public bool Perc(int p)
    {
        return Random.Range(1, 100) <= p;
    }
}
