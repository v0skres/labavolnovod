using UnityEngine;

public class TestScript : MonoBehaviour
{
    public WaveguideCalculator waveguide;

    void Start()
    {
        if (waveguide == null)
            waveguide = GetComponent<WaveguideCalculator>();

        if (waveguide != null)
        {
            StartCoroutine(RunTests());
        }
        else
        {
            Debug.LogError("TestScript: WaveguideCalculator не найден!");
        }
    }

    System.Collections.IEnumerator RunTests()
    {
        Debug.Log("=== НАЧАЛО ТЕСТОВ ===");

        yield return new WaitForSeconds(1f);

        // Тест 1: Стандартный
        Debug.Log("ТЕСТ 1: a=23мм, ε=1");
        waveguide.SetWidth(23f);
        waveguide.SetEpsilon(1f);
        yield return new WaitForSeconds(0.5f);

        // Тест 2: Узкий
        Debug.Log("ТЕСТ 2: a=15мм, ε=1");
        waveguide.SetWidth(15f);
        yield return new WaitForSeconds(0.5f);

        // Тест 3: Широкий
        Debug.Log("ТЕСТ 3: a=40мм, ε=1");
        waveguide.SetWidth(40f);
        yield return new WaitForSeconds(0.5f);

        // Тест 4: Диэлектрик
        Debug.Log("ТЕСТ 4: a=23мм, ε=2.25");
        waveguide.SetWidth(23f);
        waveguide.SetEpsilon(2.25f);
        yield return new WaitForSeconds(0.5f);

        Debug.Log("=== ТЕСТЫ ЗАВЕРШЕНЫ ===");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            waveguide.SetWidth(23f);
            waveguide.SetEpsilon(1f);
            Debug.Log($"1: a=23мм -> fкр={waveguide.GetCriticalFrequencyGHz():F3}ГГц");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            waveguide.SetWidth(15f);
            waveguide.SetEpsilon(1f);
            Debug.Log($"2: a=15мм -> fкр={waveguide.GetCriticalFrequencyGHz():F3}ГГц");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            waveguide.SetWidth(23f);
            waveguide.SetEpsilon(2.25f);
            Debug.Log($"3: a=23мм, ε=2.25 -> fкр={waveguide.GetCriticalFrequencyGHz():F3}ГГц");
        }

        // Отображение текущих значений
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"ТЕКУЩЕЕ: a={waveguide.GetWidthMM()}мм, " +
                     $"ε={waveguide.GetEpsilon()}, " +
                     $"fкр={waveguide.GetCriticalFrequencyGHz():F3}ГГц");
        }
    }
}