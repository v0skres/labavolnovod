using UnityEngine;
using UnityEngine.UI;

public class MicrowaveGenerator : MonoBehaviour
{
    [Header("Основные параметры")]
    public float frequencyMHz = 9000f;
    public bool isOn = false;
    public bool isModulated = true;
    public float attenuationDB = 0f;

    [Header("Визуальные элементы")]
    public Light powerIndicator;
    public TextMesh frequencyDisplay;

    [Header("Режим ввода")]
    private string frequencyInput = "";

    void Start()
    {
        UpdateDisplay();
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ КНОПОК ===

    public void TogglePower()
    {
        isOn = !isOn;
        UpdateDisplay();
        Debug.Log($"Генератор: {(isOn ? "ВКЛ" : "ВЫКЛ")}");
    }

    public void InputDigit(int digit)
    {
        if (!isOn) return;

        frequencyInput += digit.ToString();
        if (frequencyInput.Length > 4) frequencyInput = frequencyInput.Substring(1);

        if (int.TryParse(frequencyInput, out int freq))
        {
            if (freq >= 7500 && freq <= 10500)
            {
                frequencyMHz = freq;
                UpdateDisplay();
                Debug.Log($"Введено: {digit}, Частота: {frequencyMHz} МГц");
            }
        }
    }

    public void FrequencyUp()
    {
        if (!isOn) return;

        frequencyMHz = Mathf.Min(frequencyMHz + 100f, 10500f);
        frequencyInput = frequencyMHz.ToString("F0");
        UpdateDisplay();
        Debug.Log($"Частота +100 МГц: {frequencyMHz} МГц");
    }

    public void FrequencyDown()
    {
        if (!isOn) return;

        frequencyMHz = Mathf.Max(frequencyMHz - 100f, 7500f);
        frequencyInput = frequencyMHz.ToString("F0");
        UpdateDisplay();
        Debug.Log($"Частота -100 МГц: {frequencyMHz} МГц");
    }

    public void ChangeAttenuation(float deltaDB)
    {
        attenuationDB = Mathf.Clamp(attenuationDB + deltaDB, 0f, 60f);
        UpdateDisplay();
        Debug.Log($"Аттенюатор: {attenuationDB} дБ");
    }

    public void ToggleModulation()
    {
        isModulated = !isModulated;
        UpdateDisplay();
        Debug.Log($"Модуляция: {(isModulated ? "ВКЛ" : "ВЫКЛ")}");
    }

    public void ClearFrequencyInput()
    {
        frequencyInput = "";
        frequencyMHz = 9000f;
        UpdateDisplay();
        Debug.Log("Частота сброшена: 9000 МГц");
    }

    public void UpdateDisplay()
    {
        if (frequencyDisplay != null)
        {
            frequencyDisplay.text = $"{frequencyMHz:F0} МГц";
        }

        if (powerIndicator != null)
        {
            powerIndicator.enabled = isOn;
            powerIndicator.color = isOn ? Color.green : Color.red;
        }
    }

    public float GetFrequencyGHz()
    {
        return frequencyMHz / 1000f;
    }

    public bool IsOn()
    {
        return isOn;
    }
}