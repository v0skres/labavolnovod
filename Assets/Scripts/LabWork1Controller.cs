using UnityEngine;
using System.Collections.Generic;

public class LabWork1Controller : MonoBehaviour
{
    [Header("Компоненты")]
    public MicrowaveGenerator generator;
    public MeasuringAmplifier amplifier;
    public LabSetupController labSetup;
    public H10_WaveguideCore waveguide;

    [Header("Эксперимент 1: E_y(x)")]
    public bool isMeasuringEyX = false;
    private List<Vector2> eyXMeasurements = new List<Vector2>();

    [Header("Эксперимент 2: H_z(x)")]
    public bool isMeasuringHzX = false;
    private List<Vector2> hzXMeasurements = new List<Vector2>();

    [Header("UI для результатов")]
    public LineRenderer eyXGraph;
    public LineRenderer hzXGraph;

    void Start()
    {
        // Инициализация графиков
        InitializeGraphs();
    }

    void Update()
    {
        // Автоматические измерения если активированы
        if (isMeasuringEyX)
        {
            PerformEyXMeasurement();
        }

        if (isMeasuringHzX)
        {
            PerformHzXMeasurement();
        }
    }

    // === МЕТОДЫ ДЛЯ ЛАБОРАТОРНОЙ РАБОТЫ ===

    // Задание 1.4.4: Измерение E_y(x)
    public void StartEyXMeasurement()
    {
        if (!CheckEquipment())
            return;

        isMeasuringEyX = true;
        eyXMeasurements.Clear();

        // Устанавливаем датчик E в начальное положение
        labSetup.SetProbeEPositionX(labSetup.probeMinX);
        labSetup.SetProbeEPositionZ(0.075f); // Фиксированное z

        amplifier.SwitchToElectricProbe();

        Debug.Log("Начато измерение E_y(x)");
    }

    void PerformEyXMeasurement()
    {
        float currentX = labSetup.probeEPosXMM;

        // Измеряем
        amplifier.UpdateMeasurementFromWaveguide();
        float eyValue = amplifier.measurementValue;

        // Сохраняем
        eyXMeasurements.Add(new Vector2(currentX, eyValue));

        // Двигаем дальше
        float step = 1f; // 1 мм
        float newX = currentX + step;

        if (newX <= labSetup.probeMaxX)
        {
            labSetup.SetProbeEPositionX(newX);
        }
        else
        {
            isMeasuringEyX = false;
            Debug.Log($"Измерение E_y(x) завершено. Точек: {eyXMeasurements.Count}");
            PlotEyXGraph();
        }
    }

    // Задание 1.4.5: Измерение H_z(x)
    public void StartHzXMeasurement()
    {
        if (!CheckEquipment())
            return;

        isMeasuringHzX = true;
        hzXMeasurements.Clear();

        // Устанавливаем датчик H у боковой стенки
        labSetup.SetProbeHPositionX(labSetup.probeMinX);
        labSetup.SetProbeHPositionZ(0.075f);

        amplifier.SwitchToMagneticProbe();

        Debug.Log("Начато измерение H_z(x)");
    }

    void PerformHzXMeasurement()
    {
        float currentX = labSetup.probeHPosXMM;

        // Измеряем
        amplifier.UpdateMeasurementFromWaveguide();
        float hzValue = amplifier.measurementValue;

        // Сохраняем
        hzXMeasurements.Add(new Vector2(currentX, hzValue));

        // Двигаем дальше
        float step = 1f; // 1 мм
        float newX = currentX + step;

        if (newX <= labSetup.probeMaxX)
        {
            labSetup.SetProbeHPositionX(newX);
        }
        else
        {
            isMeasuringHzX = false;
            Debug.Log($"Измерение H_z(x) завершено. Точек: {hzXMeasurements.Count}");
            PlotHzXGraph();
        }
    }

    // Задание 1.4.6: Измерение λ_в
    public void MeasureWaveguideWavelength()
    {
        if (!CheckEquipment())
            return;

        // Устанавливаем режим стоячей волны
        waveguide.SetWaveMode(true);

        // Используем датчик E
        amplifier.SwitchToElectricProbe();

        // Перемещаем датчик для поиска узлов
        Debug.Log("Поиск узлов E_y для определения λ_в");

        // В реальном эксперименте здесь будет автоматическое сканирование
        // и поиск минимумов (узлов)

        float lambdaB = amplifier.MeasureLambdaB();
        float theoretical = waveguide.GetWaveguideWavelengthMM();

        Debug.Log($"Экспериментальное λ_в: {lambdaB:F1} мм");
        Debug.Log($"Теоретическое λ_в: {theoretical:F1} мм");
        Debug.Log($"Расхождение: {Mathf.Abs(lambdaB - theoretical):F1} мм");
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    bool CheckEquipment()
    {
        if (generator == null || !generator.IsOn())
        {
            Debug.LogWarning("Генератор не включен!");
            return false;
        }

        if (amplifier == null || !amplifier.isOn)
        {
            Debug.LogWarning("Усилитель не включен!");
            return false;
        }

        if (!waveguide.IsPropagating())
        {
            Debug.LogWarning($"Частота ниже критической! f_кр = {waveguide.GetCriticalFrequencyGHz():F2} ГГц");
            return false;
        }

        return true;
    }

    void InitializeGraphs()
    {
        if (eyXGraph != null)
        {
            eyXGraph.startWidth = 0.002f;
            eyXGraph.endWidth = 0.002f;
            eyXGraph.startColor = Color.red;
            eyXGraph.endColor = Color.red;
        }

        if (hzXGraph != null)
        {
            hzXGraph.startWidth = 0.002f;
            hzXGraph.endWidth = 0.002f;
            hzXGraph.startColor = Color.blue;
            hzXGraph.endColor = Color.blue;
        }
    }

    void PlotEyXGraph()
    {
        if (eyXGraph == null || eyXMeasurements.Count == 0)
            return;

        eyXGraph.positionCount = eyXMeasurements.Count;

        for (int i = 0; i < eyXMeasurements.Count; i++)
        {
            Vector2 point = eyXMeasurements[i];
            Vector3 position = new Vector3(
                point.x * 0.001f, // мм → м
                point.y * 0.01f,   // Масштабируем для визуализации
                0.1f               // Смещаем перед камерой
            );
            eyXGraph.SetPosition(i, position);
        }
    }

    void PlotHzXGraph()
    {
        if (hzXGraph == null || hzXMeasurements.Count == 0)
            return;

        hzXGraph.positionCount = hzXMeasurements.Count;

        for (int i = 0; i < hzXMeasurements.Count; i++)
        {
            Vector2 point = hzXMeasurements[i];
            Vector3 position = new Vector3(
                point.x * 0.001f, // мм → м
                point.y * 0.01f,   // Масштабируем для визуализации
                0.1f               // Смещаем перед камерой
            );
            hzXGraph.SetPosition(i, position);
        }
    }

    // === UI МЕТОДЫ ===

    public void OnFrequencyChanged(float freqGHz)
    {
        generator.frequencyMHz = freqGHz * 1000f;
        generator.UpdateDisplay();
        waveguide.SetFrequency(freqGHz);
    }

    public void OnPistonPositionChanged(float posMM)
    {
        labSetup.pistonPositionMM = posMM;
        labSetup.UpdatePistonPosition();
        waveguide.SetPistonPosition(posMM);
    }
}