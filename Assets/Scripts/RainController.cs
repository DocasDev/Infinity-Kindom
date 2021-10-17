using System.Collections;
using UnityEngine;

public class RainController : MonoBehaviour
{
    private GameController _GameController;
    private ParticleSystem rain;
    private ParticleSystem.EmissionModule rainModule;
    private float maxAmount;
    private bool isRain;

    void Start()
    {
        _GameController = FindObjectOfType(typeof(GameController)) as GameController;
        rain = GetComponent<ParticleSystem>();
        rainModule = rain.emission;
        StartCoroutine("Rain");
    }

    IEnumerator Rain()
    {
        yield return new WaitForSeconds(_GameController.rainDurationDelay);
        //50% de chance de não começar a chover
        bool startRain = Random.Range(0, 100) > 50;
        //50% de chance de não continuar a chover
        bool continueRain = isRain && (Random.Range(0, 100) > 50);
        if (isRain && !continueRain) //Reduz a chuva
        {
            for (float r = rainModule.rateOverTime.constant; r > 0; r -= _GameController.rainIncrement)
            {
                rainModule.rateOverTime = r;
                yield return new WaitForSeconds(_GameController.rainIncrementDelay);
            }

            rainModule.rateOverTime = 0;
            isRain = false;
        }
        else if(startRain || continueRain) //Começa/Continua a chover
        {
            maxAmount = Random.Range(maxAmount, _GameController.rainMaxAmountEmition);
            for (float r = rainModule.rateOverTime.constant; r < maxAmount; r += _GameController.rainIncrement)
            {
                rainModule.rateOverTime = r;
                yield return new WaitForSeconds(_GameController.rainIncrementDelay);
            }

            rainModule.rateOverTime = maxAmount;
            isRain = true;
        }
        StartCoroutine("Rain");
    }
}
