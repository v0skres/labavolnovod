using UnityEngine;
using System;

public class H10_WaveguideCore : MonoBehaviour
{
    [Header("Параметры волновода")]
    [SerializeField] private float a = 0.023f;     
    [SerializeField] private float b = 0.010f;      
    [SerializeField] private float epsilon = 1.0f;  
    [SerializeField] private float mu = 1.0f;       

    [Header("Параметры сигнала")]
    [SerializeField] private float frequency = 9.0e9f; 
    [SerializeField] private float pistonPosition = 0.250f; 

    [Header("Режим работы")]
    [SerializeField] private bool isStandingWaveMode = false; 

    [Header("Расчётные параметры")]
    public float lambda0_mm;        
    private float lambda0;          
    private float lambda_w;         
    private float fc;             
    private float v_phase;         
    private float waveAngularFrequency; 

    [Header("Настройки анимации")]
    public float timeScale = 1.0f; 

    public event Action OnParametersChanged;

    void Start()
    {
        RecalculateParameters();
        Debug.Log("H10_WaveguideCore: Инициализирован");
    }

    void Update()
    {

    }

    // === ОСНОВНЫЕ РАСЧЁТЫ ===

    void RecalculateParameters()
    {
        float c = 3.0e8f; 

        lambda0 = c / frequency;
        lambda0_mm = lambda0 * 1000f;

        fc = c / (2.0f * a * Mathf.Sqrt(epsilon * mu));

        float lambda_crit = 2.0f * a * Mathf.Sqrt(epsilon * mu);

        if (frequency > fc)
        {
            lambda_w = lambda0 / Mathf.Sqrt(epsilon * mu - Mathf.Pow(lambda0 / (2.0f * a), 2));

            v_phase = c / Mathf.Sqrt(epsilon * mu - Mathf.Pow(lambda0 / (2.0f * a), 2));
        }
        else
        {
            lambda_w = 0;
            v_phase = 0;
        }

        waveAngularFrequency = 2.0f * Mathf.PI * frequency;

        OnParametersChanged?.Invoke();
    }

    // === МЕТОДЫ ДЛЯ ЭЛЕКТРИЧЕСКОГО ПОЛЯ ===

    public Vector3 GetElectricFieldAt(Vector3 position, float time)
    {
        if (!IsPropagating())
            return Vector3.zero;

        float x = position.x + a / 2f;
        float z = position.z;

        float scaledTime = time * timeScale;
        float phase = (scaledTime * waveAngularFrequency) % (2 * Mathf.PI);

        float amplitude = 1.0f;
        float beta = 2.0f * Mathf.PI / lambda_w;

        if (isStandingWaveMode)
        {
            float Ey = 2.0f * amplitude * (2.0f * a / lambda0) *
                       Mathf.Sin(Mathf.PI * x / a) *
                       Mathf.Sin(beta * z) * Mathf.Cos(phase);
            return new Vector3(0, Ey, 0);
        }
        else
        {
            float Ey = amplitude * (2.0f * a / lambda0) *
                       Mathf.Sin(Mathf.PI * x / a) *
                       Mathf.Sin(phase - beta * z);
            return new Vector3(0, Ey, 0);
        }
    }

    // === НОВЫЕ МЕТОДЫ ДЛЯ МАГНИТНОГО ПОЛЯ ===

    public Vector3 GetMagneticFieldAt(Vector3 position, float time)
    {
        if (!IsPropagating())
            return Vector3.zero;

        if (isStandingWaveMode)
        {
            return GetStandingWaveMagneticFieldAt(position, time);
        }
        else
        {
            return GetTravelingWaveMagneticFieldAt(position, time);
        }
    }

    public Vector3 GetTravelingWaveMagneticFieldAt(Vector3 position, float time)
    {
        if (!IsPropagating())
            return Vector3.zero;

        float x = position.x + a / 2f;
        float z = position.z;

        float scaledTime = time * timeScale;
        float phase = (scaledTime * waveAngularFrequency) % (2 * Mathf.PI);

        float amplitude = 1.0f;
        float beta = 2.0f * Mathf.PI / lambda_w;

        float Hz = amplitude * Mathf.Cos(Mathf.PI * x / a) *
                   Mathf.Cos(phase - beta * z);

        float Hx = -amplitude * (2.0f * a / lambda0) *
                   Mathf.Sin(Mathf.PI * x / a) *
                   Mathf.Sin(phase - beta * z);

        return new Vector3(Hx, 0, Hz);
    }

    public Vector3 GetStandingWaveMagneticFieldAt(Vector3 position, float time)
    {
        if (!IsPropagating())
            return Vector3.zero;

        float x = position.x + a / 2f;
        float z = position.z;

        float scaledTime = time * timeScale;
        float phase = (scaledTime * waveAngularFrequency) % (2 * Mathf.PI); 

        float beta = 2.0f * Mathf.PI / lambda_w;
        float amplitude = 1.0f;

        float Hz = 2.0f * amplitude * Mathf.Cos(Mathf.PI * x / a) *
                   Mathf.Sin(beta * z) * Mathf.Sin(phase); 

        float Hx = -2.0f * amplitude * (2.0f * a / lambda0) *
                   Mathf.Sin(Mathf.PI * x / a) *
                   Mathf.Cos(beta * z) * Mathf.Sin(phase); 

        return new Vector3(Hx, 0, Hz);
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    public bool IsPropagating()
    {
        return frequency > fc;
    }

    public float GetCriticalFrequencyGHz()
    {
        return fc / 1e9f;
    }

    public float GetCriticalWavelengthMM()
    {
        return (2.0f * a * Mathf.Sqrt(epsilon * mu)) * 1000f;
    }

    public float GetWaveguideWavelengthMM()
    {
        return IsPropagating() ? lambda_w * 1000f : 0f;
    }

    public float GetPhaseVelocity()
    {
        return v_phase;
    }

    public float GetWidthMM()
    {
        return a * 1000f;
    }

    public float GetHeightMM()
    {
        return b * 1000f;
    }

    public float GetEpsilon()
    {
        return epsilon;
    }

    public float GetFrequencyGHz()
    {
        return frequency / 1e9f;
    }

    public float GetA()
    {
        return a;
    }

    // === МЕТОДЫ ДЛЯ ИЗМЕНЕНИЯ ПАРАМЕТРОВ ===

    public void SetWidth(float widthMM)
    {
        a = Mathf.Clamp(widthMM, 10f, 100f) * 0.001f; 
        RecalculateParameters();
    }

    public void SetHeight(float heightMM)
    {
        b = Mathf.Clamp(heightMM, 5f, 50f) * 0.001f; 
        RecalculateParameters();
    }

    public void SetWaveguideSize(float widthMM, float heightMM)
    {
        a = Mathf.Clamp(widthMM, 10f, 100f) * 0.001f;
        b = Mathf.Clamp(heightMM, 5f, 50f) * 0.001f;
        RecalculateParameters();
    }

    public void SetEpsilon(float eps)
    {
        epsilon = Mathf.Clamp(eps, 1f, 10f);
        RecalculateParameters();
    }

    public void SetFrequency(float freqGHz)
    {
        frequency = Mathf.Clamp(freqGHz, 1f, 20f) * 1e9f; 
        RecalculateParameters();
    }

    public void SetPistonPosition(float positionMM)
    {
        float oldPosition = pistonPosition;
        pistonPosition = Mathf.Clamp(positionMM, 0f, 300f) * 0.001f; 
        RecalculateParameters();
    }

    public void SetWaveMode(bool standingWave)
    {
        isStandingWaveMode = standingWave;
        Debug.Log($"Режим волны изменен: {(standingWave ? "Стоячая" : "Бегущая")}");
    }

    public void SetPistonClosed(bool isClosed)
    {
        isStandingWaveMode = isClosed;
        if (isClosed)
        {
            Debug.Log("Поршень закрыт - режим стоячей волны");
        }
        else
        {
            Debug.Log("Поршень открыт - режим бегущей волны");
        }
    }

    // === МЕТОДЫ ДЛЯ ДОМАШНЕГО ЗАДАНИЯ ===

    public float CalculateAttenuation(float sigma, float tanDelta)
    {
        if (!IsPropagating()) return 0f;

        float alpha_met = (1.0f / b) *
                          Mathf.Sqrt(Mathf.PI * Mathf.Sqrt(epsilon) /
                          (377.0f * lambda0 * sigma)) *
                          (1.0f + (2.0f * b / a) * Mathf.Pow(lambda0 / (2.0f * a), 2)) /
                          Mathf.Sqrt(1.0f - Mathf.Pow(lambda0 / (2.0f * a), 2));

        float alpha_diel = (Mathf.PI / lambda0) *
                           (tanDelta / Mathf.Sqrt(1.0f - Mathf.Pow(lambda0 / (2.0f * a), 2)));

        float alpha_np = alpha_met + alpha_diel;

        float alpha_db = 8.686f * alpha_np;

        return alpha_db;
    }

    public float CalculateCharacteristicImpedance()
    {
        if (!IsPropagating()) return 0f;

        float Z_H10 = (377.0f * Mathf.Sqrt(mu / epsilon)) /
                      Mathf.Sqrt(1.0f - Mathf.Pow(lambda0 / (2.0f * a), 2));

        return Z_H10;
    }

    // === МЕТОДЫ ДЛЯ ТЕСТИРОВАНИЯ ===

    public void PrintDebugInfo()
    {
        Debug.Log("=== ПАРАМЕТРЫ ВОЛНОВОДА ===");
        Debug.Log($"Размеры: {a * 1000:F1}×{b * 1000:F1} мм");
        Debug.Log($"ε = {epsilon:F2}, μ = {mu:F2}");
        Debug.Log($"Частота: {frequency / 1e9:F2} ГГц");
        Debug.Log($"λ₀: {lambda0 * 1000:F1} мм");
        Debug.Log($"λ_кр: {2 * a * 1000:F1} мм");
        Debug.Log($"f_кр: {fc / 1e9:F3} ГГц");
        Debug.Log($"Распространение: {(IsPropagating() ? "ДА" : "НЕТ")}");

        if (IsPropagating())
        {
            Debug.Log($"λ_в: {lambda_w * 1000:F1} мм");
            Debug.Log($"V_фаз: {v_phase / 1e6:F0} Мм/с");
            Debug.Log($"Z_H10: {CalculateCharacteristicImpedance():F1} Ом");
        }

        Debug.Log($"Режим: {(isStandingWaveMode ? "Стоячая волна" : "Бегущая волна")}");
        Debug.Log($"Позиция поршня: {pistonPosition * 1000:F1} мм");
    }

    // === МЕТОДЫ ДЛЯ ЭКСПЕРИМЕНТАЛЬНОЙ ЧАСТИ ===

    public float MeasureFieldAtProbe(Vector3 probePosition, bool isElectricField, float time)
    {
        if (!IsPropagating()) return 0f;

        if (isElectricField)
        {
            Vector3 eField = GetElectricFieldAt(probePosition, time);
            return eField.magnitude;
        }
        else
        {
            Vector3 hField = GetMagneticFieldAt(probePosition, time);
            return hField.magnitude;
        }
    }

    public float[] GetFieldDistributionX(bool isElectricField, float fixedZ, float time)
    {
        int points = 50;
        float[] distribution = new float[points];

        for (int i = 0; i < points; i++)
        {
            float x = (-a / 2f) + (i / (float)points) * a;
            Vector3 position = new Vector3(x, 0, fixedZ);

            if (isElectricField)
            {
                distribution[i] = GetElectricFieldAt(position, time).magnitude;
            }
            else
            {
                distribution[i] = GetMagneticFieldAt(position, time).magnitude;
            }
        }

        return distribution;
    }

    public float[] GetFieldDistributionZ(bool isElectricField, float fixedX, float time)
    {
        int points = 100;
        float[] distribution = new float[points];

        for (int i = 0; i < points; i++)
        {
            float z = (i / (float)points) * pistonPosition;
            Vector3 position = new Vector3(fixedX, 0, z);

            if (isElectricField)
            {
                distribution[i] = GetElectricFieldAt(position, time).magnitude;
            }
            else
            {
                distribution[i] = GetMagneticFieldAt(position, time).magnitude;
            }
        }

        return distribution;
    }
}