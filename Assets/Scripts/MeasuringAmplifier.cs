using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 

public class MeasuringAmplifier : MonoBehaviour
{
    [Header("Основные параметры")]
    public float measurementValue = 0f;
    public float sensitivity = 1.0f;
    public bool isOn = false;
    public bool rpuMode = false;
    public float biasVoltage = 0f;

    [Header("Связи с другими компонентами")]
    public MicrowaveGenerator microwaveGenerator;
    public LabSetupController labSetup;
    public H10_WaveguideCore waveguideCore;

    [Header("Тип измерения")]
    public bool isElectricProbe = true;
    public bool measureSquare = true;

    [Header("Калибровка")]
    public float calibrationFactor = 100f;

    [Header("Визуальные элементы")]
    public Light powerIndicator;
    public Light rpuIndicator;
    public TextMesh valueDisplay;
    public Transform needle;

    [Header("Настройки стрелки")]
    public float minAngle = -135f;
    public float maxAngle = 135f;
    public float maxValue = 100f;

    [Header("Отображение режима")]
    public TextMesh modeDisplay;

    [Header("Настройки измерения")]
    [SerializeField] private float smoothingTime = 0.3f; 
    [SerializeField] private int averagingSamples = 10;  
    [SerializeField] private float updateRate = 10f;     

    private float lastUpdateTime = 0f;
    private Queue<float> measurementHistory = new Queue<float>();
    private float smoothedValue = 0f;

    void Start()
    {
        if (microwaveGenerator == null)
            microwaveGenerator = FindObjectOfType<MicrowaveGenerator>();

        if (labSetup == null)
            labSetup = FindObjectOfType<LabSetupController>();

        if (waveguideCore == null)
            waveguideCore = FindObjectOfType<H10_WaveguideCore>();

        for (int i = 0; i < averagingSamples; i++)
        {
            measurementHistory.Enqueue(0f);
        }

        UpdateDisplay();
    }

    void Update()
    {
        float currentTime = Time.time;
        if (currentTime - lastUpdateTime >= 1f / updateRate)
        {
            lastUpdateTime = currentTime;

            if (isOn && microwaveGenerator != null && microwaveGenerator.IsOn() &&
                labSetup != null && waveguideCore != null)
            {
                UpdateMeasurementFromWaveguide();
            }
        }

        if (needle != null && isOn)
        {
            UpdateNeedle();
        }
    }

    public void UpdateMeasurementFromWaveguide()
    {
        if (!waveguideCore.IsPropagating())
        {
            measurementValue = 0f;
            smoothedValue = 0f;
            UpdateDisplay();
            return;
        }

        Vector3 probePosition = GetCurrentProbePosition();

        float fieldValue = 0f;

        if (isElectricProbe)
        {
            Vector3 eField = waveguideCore.GetElectricFieldAt(probePosition, Time.time);
            fieldValue = Mathf.Abs(eField.y);
        }
        else
        {
            Vector3 hField = waveguideCore.GetMagneticFieldAt(probePosition, Time.time);

            if (isElectricProbe)
                fieldValue = Mathf.Abs(hField.x); 
            else
                fieldValue = Mathf.Abs(hField.z); 
        }

        if (measureSquare)
        {
            fieldValue = Mathf.Pow(fieldValue, 2);
        }

        float rawValue = fieldValue * sensitivity * calibrationFactor;

        if (rpuMode)
        {
            rawValue += biasVoltage * 5f;
        }

        float noise = Mathf.PerlinNoise(Time.time * 0.1f, 0) * 0.02f * rawValue;
        rawValue = Mathf.Clamp(rawValue + noise, 0f, maxValue * 1.2f);

        measurementHistory.Dequeue();
        measurementHistory.Enqueue(rawValue);

        float sum = 0f;
        foreach (float val in measurementHistory)
        {
            sum += val;
        }
        float averagedValue = sum / averagingSamples;

        smoothedValue = Mathf.Lerp(smoothedValue, averagedValue,
            Time.deltaTime / smoothingTime);

        measurementValue = Mathf.Clamp(smoothedValue, 0f, maxValue);

        UpdateDisplay();
    }

    Vector3 GetCurrentProbePosition()
    {
        if (labSetup == null) return Vector3.zero;

        if (isElectricProbe)
        {
            return new Vector3(
                labSetup.probeEPosXMM * 0.001f,
                0f,
                labSetup.probeEPosZMM * 0.001f
            );
        }
        else
        {
            return new Vector3(
                labSetup.probeHPosXMM * 0.001f,
                0f,
                labSetup.probeHPosZMM * 0.001f
            );
        }
    }

    void UpdateNeedle()
    {
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, measurementValue / maxValue);
        float currentAngle = needle.localEulerAngles.z;

        if (currentAngle > 180) currentAngle -= 360;

        float newAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 2f);
        needle.localRotation = Quaternion.Euler(0, 0, newAngle);
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ КНОПОК  ===

    public void TogglePower()
    {
        isOn = !isOn;
        UpdateDisplay();
        Debug.Log($"Усилитель: {(isOn ? "ВКЛ" : "ВЫКЛ")}");
    }

    public void ToggleRPU()
    {
        rpuMode = !rpuMode;
        UpdateDisplay();
        Debug.Log($"РПУ: {(rpuMode ? "ВКЛ" : "ВЫКЛ")}");
    }

    public void SetZero()
    {
        measurementValue = 0f;
        UpdateDisplay();
        Debug.Log("Ноль установлен");
    }

    public void CycleInputRange()
    {
        calibrationFactor *= 2f;
        if (calibrationFactor > 400f) calibrationFactor = 25f;

        Debug.Log($"Диапазон: ×{calibrationFactor / 25f:F0}");
        UpdateDisplay();
    }

    public void ChangeSensitivity(float delta)
    {
        sensitivity = Mathf.Clamp(sensitivity + delta, 0.1f, 10f);
        UpdateDisplay();
        Debug.Log($"Чувствительность: {sensitivity:F1}");
    }

    public void ChangeBiasVoltage(float delta)
    {
        if (!rpuMode) return;

        biasVoltage = Mathf.Clamp(biasVoltage + delta, -10f, 10f);
        UpdateDisplay();
        Debug.Log($"Смещение: {biasVoltage:F2} В");
    }

    public void InputBiasMinus()
    {
        if (!rpuMode) return;

        biasVoltage = -biasVoltage;
        UpdateDisplay();
        Debug.Log($"Смена знака смещения: {biasVoltage:F2} В");
    }

    public void ClearBiasInput()
    {
        biasVoltage = 0f;
        UpdateDisplay();
        Debug.Log("Смещение сброшено");
    }

    void UpdateDisplay()
    {
        if (valueDisplay != null)
        {
            string valueText = isOn ? $"{measurementValue:F1}" : "---";

            if (isOn && measurementValue >= maxValue * 0.95f)
            {
                valueText = ">100.0";
            }

            valueDisplay.text = valueText;
        }

        if (modeDisplay != null)
        {
            modeDisplay.text = isElectricProbe ? "E" : "H";
            modeDisplay.color = isElectricProbe ? Color.red : Color.blue;
        }

        if (powerIndicator != null)
        {
            powerIndicator.enabled = isOn;
            powerIndicator.color = isOn ? Color.green : Color.red;
        }

        if (rpuIndicator != null)
        {
            rpuIndicator.enabled = rpuMode;
            rpuIndicator.color = rpuMode ? Color.blue : Color.gray;
        }
    }

    // === МЕТОДЫ ДЛЯ ЭКСПЕРИМЕНТАЛЬНЫХ ЗАДАНИЙ ===

    public void SwitchToElectricProbe()
    {
        isElectricProbe = true;
        measureSquare = true; 
        Debug.Log("Переключено на датчик E (штырь)");
        UpdateDisplay();
    }

    public void SwitchToMagneticProbe()
    {
        isElectricProbe = false;
        measureSquare = true; 
        Debug.Log("Переключено на датчик H (петля)");
        UpdateDisplay();
    }

    public void ToggleSquareDetection()
    {
        measureSquare = !measureSquare;
        Debug.Log($"Квадратичное детектирование: {(measureSquare ? "ВКЛ" : "ВЫКЛ")}");
    }

    public float MeasureLambdaB()
    {
        if (!isOn || !isElectricProbe) return 0f;


        Debug.Log("Измерение λ_в (режим стоячей волны)");
        return waveguideCore.GetWaveguideWavelengthMM();
    }

    public bool IsOn()
    {
        return isOn;
    }

    public bool IsRPUEnabled()
    {
        return rpuMode;
    }

    public float GetMeasurement()
    {
        return measurementValue;
    }
}