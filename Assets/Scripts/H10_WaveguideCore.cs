using UnityEngine;
using System;

public class H10_WaveguideCore : MonoBehaviour
{
    // Константы
    public const float SPEED_OF_LIGHT = 299792458f; // м/с
    public const float VACUUM_IMPEDANCE = 376.73f; // Ом

    // === НАСТРАИВАЕМЫЕ ПАРАМЕТРЫ ===
    [Header("Размеры волновода")]
    [SerializeField, Range(10f, 100f)]
    private float _widthMM = 23f;      // Ширина в мм

    [SerializeField, Range(5f, 50f)]
    private float _heightMM = 10f;     // Высота в мм

    [Header("Материал заполнения")]
    [SerializeField, Range(1f, 10f)]
    private float _epsilon = 1f;       // ε

    [Header("Генератор")]
    [SerializeField, Range(1f, 20f)]
    private float _frequencyGHz = 9f;  // Частота в ГГц

    // === РАССЧИТАННЫЕ ПАРАМЕТРЫ ===
    [Header("Рассчитанные параметры")]
    [ReadOnly] public float a;                // Ширина в метрах
    [ReadOnly] public float b;                // Высота в метрах
    [ReadOnly] public float lambda0_mm;       // λ₀ в вакууме, мм
    [ReadOnly] public float lambda_crit_mm;   // λ_кр для H10, мм
    [ReadOnly] public float fc_GHz;           // f_кр, ГГц
    [ReadOnly] public float lambda_waveguide_mm; // λ_в, мм
    [ReadOnly] public bool isPropagating;     // Распространяется ли волна?
    [ReadOnly] public float v_phase;          // Фазовая скорость, м/с

    // Атрибут для полей только для чтения
    public class ReadOnlyAttribute : PropertyAttribute { }

    // Событие при изменении параметров
    public event Action OnParametersChanged;

    void Start()
    {
        CalculateAll();
        Debug.Log($"Старт: a={_widthMM}мм, ε={_epsilon}, f={_frequencyGHz}ГГц, fкр={fc_GHz:F3}ГГц");
    }

    void Update()
    {
        // Можно добавить автообновление, если нужно
    }

    void OnValidate()
    {
        CalculateAll();
    }

    // Основной метод расчета
    void CalculateAll()
    {
        // 1. Конвертируем мм в метры
        a = _widthMM * 0.001f;
        b = _heightMM * 0.001f;

        // 2. Проверяем a > b
        if (a <= b) a = b + 0.001f;

        // 3. Основные длины волн
        float frequencyHz = _frequencyGHz * 1e9f;
        float lambda0 = SPEED_OF_LIGHT / frequencyHz;
        lambda0_mm = lambda0 * 1000f;

        float lambda = lambda0 / Mathf.Sqrt(_epsilon);

        // 4. КРИТИЧЕСКИЕ ПАРАМЕТРЫ ДЛЯ H10
        lambda_crit_mm = 2f * a * 1000f;  // λ_кр = 2a, в мм

        float fc_Hz = SPEED_OF_LIGHT / (2f * a * Mathf.Sqrt(_epsilon));
        fc_GHz = fc_Hz / 1e9f;  // в ГГц

        // 5. Проверка условия распространения
        isPropagating = frequencyHz > fc_Hz;

        // 6. Длина волны в волноводе
        if (isPropagating)
        {
            float ratio = lambda / (2f * a);
            lambda_waveguide_mm = (lambda / Mathf.Sqrt(1f - ratio * ratio)) * 1000f;

            // 7. Фазовая скорость
            v_phase = SPEED_OF_LIGHT / (Mathf.Sqrt(_epsilon) * Mathf.Sqrt(1f - ratio * ratio));
        }
        else
        {
            lambda_waveguide_mm = 0f;
            v_phase = 0f;
        }

        // Оповещаем подписчиков
        OnParametersChanged?.Invoke();
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ИЗМЕНЕНИЯ ПАРАМЕТРОВ ===

    public void SetWidth(float widthMM)
    {
        _widthMM = Mathf.Clamp(widthMM, 10f, 100f);
        CalculateAll();
        Debug.Log($"Ширина установлена: {_widthMM}мм");
    }

    public void SetHeight(float heightMM)
    {
        _heightMM = Mathf.Clamp(heightMM, 5f, 50f);
        CalculateAll();
        Debug.Log($"Высота установлена: {_heightMM}мм");
    }

    public void SetEpsilon(float epsilon)
    {
        _epsilon = Mathf.Clamp(epsilon, 1f, 10f);
        CalculateAll();
        Debug.Log($"ε установлен: {_epsilon}");
    }

    public void SetFrequency(float freqGHz)
    {
        _frequencyGHz = Mathf.Clamp(freqGHz, 1f, 20f);
        CalculateAll();
        Debug.Log($"Частота установлена: {_frequencyGHz}ГГц");
    }

    // === ГЕТТЕРЫ ДЛЯ UI ===

    public float GetWidthMM() => _widthMM;
    public float GetHeightMM() => _heightMM;
    public float GetEpsilon() => _epsilon;
    public float GetFrequencyGHz() => _frequencyGHz;
    public float GetCriticalFrequencyGHz() => fc_GHz;
    public float GetCriticalWavelengthMM() => lambda_crit_mm;
    public bool IsPropagating() => isPropagating;
    public float GetWaveguideWavelengthMM() => lambda_waveguide_mm;
    public float GetPhaseVelocity() => v_phase;

    [Header("Дополнительные параметры")]
    [SerializeField, Range(0f, 300f)]
    private float _pistonPositionMM = 0f; // Положение поршня в мм

    [SerializeField]
    private float _sourcePower = 1f;      // Мощность источника

    [SerializeField]
    private OperationMode _mode = OperationMode.StandingWave;

    public enum OperationMode { TravellingWave, StandingWave }

    public float GetA() => a;  // Ширина в метрах
    public float GetB() => b;  // Высота в метрах

    public void SetPistonPosition(float positionMM)
    {
        _pistonPositionMM = Mathf.Clamp(positionMM, 0f, 300f);
        CalculateAll();
        Debug.Log($"Поршень установлен: {_pistonPositionMM} мм");
    }

    public float GetPistonPositionMM() => _pistonPositionMM;

    public void SetOperationMode(OperationMode mode)
    {
        _mode = mode;
        CalculateAll();
        Debug.Log($"Режим установлен: {mode}");
    }

    public void SetSourcePower(float power)
    {
        _sourcePower = Mathf.Clamp(power, 0.1f, 10f);
        CalculateAll();
    }

    // Функция для расчета электрического поля в точке
    public Vector3 GetElectricFieldAt(Vector3 point, float time)
    {
        if (!isPropagating)
            return Vector3.zero;

        float x = point.x;  // Поперечная координата (м)
        float z = point.z;  // Продольная координата (м)
        float omega = 2f * Mathf.PI * _frequencyGHz * 1e9f; // Угловая частота

        // Амплитудные коэффициенты
        float beta = 2f * Mathf.PI / (lambda_waveguide_mm * 0.001f); // Постоянная распространения
        float E0 = Mathf.Sqrt(_sourcePower * VACUUM_IMPEDANCE); // Примерная амплитуда

        Vector3 field = Vector3.zero;

        if (_mode == OperationMode.TravellingWave)
        {
            // Бегущая волна
            field.y = E0 * Mathf.Sin(Mathf.PI * x / a) *
                      Mathf.Sin(omega * time - beta * z);
        }
        else
        {
            // Стоячая волна (с учетом поршня)
            float effectiveZ = z - (_pistonPositionMM * 0.001f);
            field.y = 2f * E0 * Mathf.Sin(Mathf.PI * x / a) *
                      Mathf.Sin(beta * effectiveZ) *
                      Mathf.Cos(omega * time);
        }

        return field;
    }

    // Функция для расчета магнитного поля в точке
    public Vector3 GetMagneticFieldAt(Vector3 point, float time)
    {
        if (!isPropagating)
            return Vector3.zero;

        float x = point.x;
        float z = point.z;
        float omega = 2f * Mathf.PI * _frequencyGHz * 1e9f;
        float beta = 2f * Mathf.PI / (lambda_waveguide_mm * 0.001f);
        float H0 = E0 / VACUUM_IMPEDANCE;

        Vector3 field = Vector3.zero;

        if (_mode == OperationMode.TravellingWave)
        {
            field.x = -H0 * (lambda_waveguide_mm * 0.001f / (2f * a)) *
                      Mathf.Sin(Mathf.PI * x / a) *
                      Mathf.Sin(omega * time - beta * z);

            field.z = H0 * Mathf.Cos(Mathf.PI * x / a) *
                      Mathf.Cos(omega * time - beta * z);
        }
        else
        {
            float effectiveZ = z - (_pistonPositionMM * 0.001f);
            field.x = -2f * H0 * (lambda_waveguide_mm * 0.001f / (2f * a)) *
                      Mathf.Sin(Mathf.PI * x / a) *
                      Mathf.Cos(beta * effectiveZ) *
                      Mathf.Sin(omega * time);

            field.z = 2f * H0 * Mathf.Cos(Mathf.PI * x / a) *
                      Mathf.Sin(beta * effectiveZ) *
                      Mathf.Sin(omega * time);
        }

        return field;
    }

    // Метод для изменения размера волновода (удобная обертка)
    public void SetWaveguideSize(float widthMM, float heightMM)
    {
        SetWidth(widthMM);
        SetHeight(heightMM);
    }

    // Вспомогательное свойство
    private float E0 => Mathf.Sqrt(_sourcePower * VACUUM_IMPEDANCE);
}