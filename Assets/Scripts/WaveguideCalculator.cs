using UnityEngine;

public class WaveguideCalculator : MonoBehaviour
{
    // Константы
    private const float SPEED_OF_LIGHT = 299792458f; // м/с

    [Header("=== ВХОДНЫЕ ПАРАМЕТРЫ ===")]
    [SerializeField, Range(10f, 100f)]
    private float widthMM = 23f;          // Ширина a в мм

    [SerializeField, Range(1f, 10f)]
    private float epsilon = 1f;           // ε

    [Header("=== РАССЧИТАННЫЕ ПАРАМЕТРЫ ===")]
    [SerializeField] private float a_meters;           // Ширина в метрах
    [SerializeField] private float lambda_crit_mm;     // λ_кр в мм
    [SerializeField] private float fc_GHz;             // f_кр в ГГц

    // Флаг для пересчета
    private bool needsUpdate = true;

    void Start()
    {
        Recalculate();
        Debug.Log($"СТАРТ: a={widthMM}мм -> fкр={fc_GHz:F3}ГГц");
    }

    void Update()
    {
        if (needsUpdate)
        {
            Recalculate();
            needsUpdate = false;
        }
    }

    void OnValidate()
    {
        needsUpdate = true;
        Debug.Log($"OnValidate: widthMM={widthMM}, epsilon={epsilon}");
    }

    // ПРОСТЕЙШИЙ РАСЧЕТ
    void Recalculate()
    {
        // 1. мм → метры
        a_meters = widthMM * 0.001f;

        // 2. Критическая длина волны: λ_кр = 2a
        lambda_crit_mm = 2f * a_meters * 1000f;  // в мм

        // 3. Критическая частота: f_кр = c / (λ_кр * √ε)
        float fc_Hz = SPEED_OF_LIGHT / (2f * a_meters * Mathf.Sqrt(epsilon));
        fc_GHz = fc_Hz / 1e9f;  // в ГГц

        Debug.Log($"РАСЧЕТ: a={widthMM}мм, ε={epsilon}");
        Debug.Log($"  λ_кр = {lambda_crit_mm:F1} мм");
        Debug.Log($"  f_кр = {fc_GHz:F3} ГГц");

        // Проверка по ручному расчету
        CheckWithManualCalculation();
    }

    void CheckWithManualCalculation()
    {
        // Ручной расчет для проверки
        float a = widthMM * 0.001f;
        float lambda_crit = 2f * a;
        float fc = SPEED_OF_LIGHT / (lambda_crit * Mathf.Sqrt(epsilon)) / 1e9f;

        Debug.Log($"РУЧНОЙ РАСЧЕТ: a={widthMM}мм ({a:F4}м), λкр={lambda_crit * 1000:F1}мм, fкр={fc:F3}ГГц");
    }

    // Методы для изменения из других скриптов
    public void SetWidth(float newWidthMM)
    {
        widthMM = Mathf.Clamp(newWidthMM, 10f, 100f);
        needsUpdate = true;
        Debug.Log($"SetWidth вызван: {widthMM}мм");
    }

    public void SetEpsilon(float newEpsilon)
    {
        epsilon = Mathf.Clamp(newEpsilon, 1f, 10f);
        needsUpdate = true;
        Debug.Log($"SetEpsilon вызван: {epsilon}");
    }

    // Геттеры
    public float GetCriticalFrequencyGHz() => fc_GHz;
    public float GetCriticalWavelengthMM() => lambda_crit_mm;
    public float GetWidthMM() => widthMM;
    public float GetEpsilon() => epsilon;
}