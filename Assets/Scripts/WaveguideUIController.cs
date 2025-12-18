using UnityEngine;
using UnityEngine.UI;

public class WaveguideUIController : MonoBehaviour
{
    [Header("Ссылка на ядро")]
    public H10_WaveguideCore waveguideCore;

    [Header("Панели управления")]
    public GameObject controlPanel;
    public GameObject resultsPanel;

    [Header("Слайдеры управления")]
    public Slider widthSlider;
    public Slider heightSlider;
    public Slider epsilonSlider;
    public Slider frequencySlider;

    [Header("Поля ввода (альтернатива слайдерам)")]
    public InputField widthInput;
    public InputField heightInput;
    public InputField epsilonInput;
    public InputField frequencyInput;

    [Header("Отображение параметров")]
    public Text widthValueText;
    public Text heightValueText;
    public Text epsilonValueText;
    public Text frequencyValueText;

    [Header("Отображение результатов")]
    public Text lambda0Text;
    public Text lambdaCritText;
    public Text fcText;
    public Text lambdaWaveguideText;
    public Text propagationText;
    public Text phaseVelocityText;

    [Header("Графические элементы")]
    public Image propagationIndicator;
    public Color propagatingColor = Color.green;
    public Color notPropagatingColor = Color.red;

    void Start()
    {
        if (waveguideCore == null)
        {
            waveguideCore = FindObjectOfType<H10_WaveguideCore>();
            if (waveguideCore == null)
            {
                Debug.LogError("Не найден H10_WaveguideCore на сцене!");
                enabled = false;
                return;
            }
        }

        // Подписываемся на изменения
        waveguideCore.OnParametersChanged += UpdateUI;

        // Настраиваем слайдеры
        SetupSliders();

        // Настраиваем поля ввода
        SetupInputFields();

        // Первоначальное обновление
        UpdateUI();

        Debug.Log("WaveguideUIController: Инициализация завершена");
    }

    void SetupSliders()
    {
        if (widthSlider != null)
        {
            widthSlider.minValue = 10f;
            widthSlider.maxValue = 100f;
            widthSlider.value = waveguideCore.GetWidthMM();
            widthSlider.onValueChanged.AddListener(OnWidthSliderChanged);
        }

        if (heightSlider != null)
        {
            heightSlider.minValue = 5f;
            heightSlider.maxValue = 50f;
            heightSlider.value = waveguideCore.GetHeightMM();
            heightSlider.onValueChanged.AddListener(OnHeightSliderChanged);
        }

        if (epsilonSlider != null)
        {
            epsilonSlider.minValue = 1f;
            epsilonSlider.maxValue = 10f;
            epsilonSlider.value = waveguideCore.GetEpsilon();
            epsilonSlider.onValueChanged.AddListener(OnEpsilonSliderChanged);
        }

        if (frequencySlider != null)
        {
            frequencySlider.minValue = 1f;
            frequencySlider.maxValue = 20f;
            frequencySlider.value = waveguideCore.GetFrequencyGHz();
            frequencySlider.onValueChanged.AddListener(OnFrequencySliderChanged);
        }
    }

    void SetupInputFields()
    {
        if (widthInput != null)
        {
            widthInput.text = waveguideCore.GetWidthMM().ToString("F1");
            widthInput.onEndEdit.AddListener(OnWidthInputChanged);
        }

        if (heightInput != null)
        {
            heightInput.text = waveguideCore.GetHeightMM().ToString("F1");
            heightInput.onEndEdit.AddListener(OnHeightInputChanged);
        }

        if (epsilonInput != null)
        {
            epsilonInput.text = waveguideCore.GetEpsilon().ToString("F2");
            epsilonInput.onEndEdit.AddListener(OnEpsilonInputChanged);
        }

        if (frequencyInput != null)
        {
            frequencyInput.text = waveguideCore.GetFrequencyGHz().ToString("F2");
            frequencyInput.onEndEdit.AddListener(OnFrequencyInputChanged);
        }
    }

    // === ОБРАБОТЧИКИ ИЗМЕНЕНИЙ ===

    void OnWidthSliderChanged(float value)
    {
        waveguideCore.SetWidth(value);
        if (widthInput != null) widthInput.text = value.ToString("F1");
    }

    void OnHeightSliderChanged(float value)
    {
        waveguideCore.SetHeight(value);
        if (heightInput != null) heightInput.text = value.ToString("F1");
    }

    void OnEpsilonSliderChanged(float value)
    {
        waveguideCore.SetEpsilon(value);
        if (epsilonInput != null) epsilonInput.text = value.ToString("F2");
    }

    void OnFrequencySliderChanged(float value)
    {
        waveguideCore.SetFrequency(value);
        if (frequencyInput != null) frequencyInput.text = value.ToString("F2");
    }

    void OnWidthInputChanged(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            floatValue = Mathf.Clamp(floatValue, 10f, 100f);
            waveguideCore.SetWidth(floatValue);
            if (widthSlider != null) widthSlider.value = floatValue;
        }
    }

    void OnHeightInputChanged(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            floatValue = Mathf.Clamp(floatValue, 5f, 50f);
            waveguideCore.SetHeight(floatValue);
            if (heightSlider != null) heightSlider.value = floatValue;
        }
    }

    void OnEpsilonInputChanged(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            floatValue = Mathf.Clamp(floatValue, 1f, 10f);
            waveguideCore.SetEpsilon(floatValue);
            if (epsilonSlider != null) epsilonSlider.value = floatValue;
        }
    }

    void OnFrequencyInputChanged(string value)
    {
        if (float.TryParse(value, out float floatValue))
        {
            floatValue = Mathf.Clamp(floatValue, 1f, 20f);
            waveguideCore.SetFrequency(floatValue);
            if (frequencySlider != null) frequencySlider.value = floatValue;
        }
    }

    // === ОБНОВЛЕНИЕ UI ===

    void UpdateUI()
    {
        UpdateControlValues();
        UpdateResultValues();
        UpdateVisualFeedback();
    }

    void UpdateControlValues()
    {
        // Обновляем текстовые поля значений
        if (widthValueText != null)
            widthValueText.text = waveguideCore.GetWidthMM().ToString("F1") + " мм";

        if (heightValueText != null)
            heightValueText.text = waveguideCore.GetHeightMM().ToString("F1") + " мм";

        if (epsilonValueText != null)
            epsilonValueText.text = waveguideCore.GetEpsilon().ToString("F2");

        if (frequencyValueText != null)
            frequencyValueText.text = waveguideCore.GetFrequencyGHz().ToString("F2") + " ГГц";

        // Синхронизируем слайдеры и поля ввода
        if (widthSlider != null)
            widthSlider.SetValueWithoutNotify(waveguideCore.GetWidthMM());

        if (heightSlider != null)
            heightSlider.SetValueWithoutNotify(waveguideCore.GetHeightMM());

        if (epsilonSlider != null)
            epsilonSlider.SetValueWithoutNotify(waveguideCore.GetEpsilon());

        if (frequencySlider != null)
            frequencySlider.SetValueWithoutNotify(waveguideCore.GetFrequencyGHz());

        if (widthInput != null)
            widthInput.SetTextWithoutNotify(waveguideCore.GetWidthMM().ToString("F1"));

        if (heightInput != null)
            heightInput.SetTextWithoutNotify(waveguideCore.GetHeightMM().ToString("F1"));

        if (epsilonInput != null)
            epsilonInput.SetTextWithoutNotify(waveguideCore.GetEpsilon().ToString("F2"));

        if (frequencyInput != null)
            frequencyInput.SetTextWithoutNotify(waveguideCore.GetFrequencyGHz().ToString("F2"));
    }

    void UpdateResultValues()
    {
        if (lambda0Text != null)
            lambda0Text.text = $"λ₀: {waveguideCore.lambda0_mm:F1} мм";

        if (lambdaCritText != null)
            lambdaCritText.text = $"λ_кр: {waveguideCore.GetCriticalWavelengthMM():F1} мм";

        if (fcText != null)
            fcText.text = $"f_кр: {waveguideCore.GetCriticalFrequencyGHz():F3} ГГц";

        if (lambdaWaveguideText != null)
        {
            string text = waveguideCore.IsPropagating()
                ? $"λ_в: {waveguideCore.GetWaveguideWavelengthMM():F1} мм"
                : "λ_в: ---";
            lambdaWaveguideText.text = text;
        }

        if (propagationText != null)
        {
            string status = waveguideCore.IsPropagating()
                ? "✓ Волна распространяется"
                : "✗ Волна затухает (f < f_кр)";
            propagationText.text = status;
            propagationText.color = waveguideCore.IsPropagating() ? Color.green : Color.red;
        }

        if (phaseVelocityText != null)
        {
            string text = waveguideCore.IsPropagating()
                ? $"V_фаз: {(waveguideCore.GetPhaseVelocity() / 1e6):F0} Мм/с"
                : "V_фаз: ---";
            phaseVelocityText.text = text;
        }
    }

    void UpdateVisualFeedback()
    {
        if (propagationIndicator != null)
        {
            propagationIndicator.color = waveguideCore.IsPropagating()
                ? propagatingColor
                : notPropagatingColor;
        }
    }

    // === КНОПКИ ДЛЯ БЫСТРЫХ ТЕСТОВ ===

    public void TestStandardWaveguide()
    {
        waveguideCore.SetWidth(23f);
        waveguideCore.SetHeight(10f);
        waveguideCore.SetEpsilon(1f);
        waveguideCore.SetFrequency(9f);
        Debug.Log("Установлен стандартный волновод 23×10 мм, воздух");
    }

    public void TestNarrowWaveguide()
    {
        waveguideCore.SetWidth(15f);
        waveguideCore.SetHeight(10f);
        waveguideCore.SetEpsilon(1f);
        Debug.Log("Установлен узкий волновод 15×10 мм");
    }

    public void TestDielectricFilled()
    {
        waveguideCore.SetWidth(23f);
        waveguideCore.SetHeight(10f);
        waveguideCore.SetEpsilon(2.25f); // Полиэтилен
        Debug.Log("Установлен волновод с полиэтиленом (ε=2.25)");
    }

    public void TestBelowCutoff()
    {
        waveguideCore.SetWidth(23f);
        waveguideCore.SetEpsilon(1f);
        waveguideCore.SetFrequency(5f); // Ниже f_кр
        Debug.Log("Установлена частота ниже отсечки (5 ГГц)");
    }

    public void TestAboveCutoff()
    {
        waveguideCore.SetWidth(23f);
        waveguideCore.SetEpsilon(1f);
        waveguideCore.SetFrequency(10f); // Выше f_кр
        Debug.Log("Установлена частота выше отсечки (10 ГГц)");
    }

    void OnDestroy()
    {
        if (waveguideCore != null)
        {
            waveguideCore.OnParametersChanged -= UpdateUI;
        }
    }
}