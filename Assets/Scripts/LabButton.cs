using UnityEngine;

public class LabButton : MonoBehaviour
{
    [Header("Настройки кнопки")]
    public string buttonFunction; 

    [Header("Визуальная обратная связь")]
    public float pressDepth = 0.005f; 
    public Material pressedMaterial;  

    private Vector3 originalPosition;
    private Material originalMaterial;
    private Renderer buttonRenderer;

    void Start()
    {
        originalPosition = transform.localPosition;
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer != null)
            originalMaterial = buttonRenderer.material;

        Debug.Log($"Кнопка '{name}' готова. Функция: {buttonFunction}");
    }

    void OnMouseDown()
    {
        transform.localPosition = originalPosition - new Vector3(0, pressDepth, 0);

        if (buttonRenderer != null && pressedMaterial != null)
            buttonRenderer.material = pressedMaterial;

        ExecuteButtonFunction();

        Debug.Log($"Нажата кнопка: {name} -> {buttonFunction}");
    }

    void OnMouseUp()
    {
        transform.localPosition = originalPosition;

        if (buttonRenderer != null && originalMaterial != null)
            buttonRenderer.material = originalMaterial;
    }

    void ExecuteButtonFunction()
    {
        MicrowaveGenerator generator = FindObjectOfType<MicrowaveGenerator>();
        MeasuringAmplifier amplifier = FindObjectOfType<MeasuringAmplifier>();

        if (generator == null)
        {
            Debug.LogWarning("MicrowaveGenerator не найден в сцене!");
            return;
        }

        if (amplifier == null)
        {
            Debug.LogWarning("MeasuringAmplifier не найден в сцене!");
            return;
        }

        // === ГЕНЕРАТОР СВЧ ===
        if (buttonFunction.StartsWith("gen_"))
        {
            switch (buttonFunction)
            {
                case "gen_power":
                    generator.TogglePower();
                    break;

                case "gen_freq_up":
                    generator.FrequencyUp();
                    break;

                case "gen_freq_down":
                    generator.FrequencyDown();
                    break;

                case "gen_atten_up":
                    generator.ChangeAttenuation(10f);
                    break;

                case "gen_atten_down":
                    generator.ChangeAttenuation(-10f);
                    break;

                case "gen_modulation":
                    generator.ToggleModulation();
                    break;

                // Цифровые кнопки
                case "digit_0": generator.InputDigit(0); break;
                case "digit_1": generator.InputDigit(1); break;
                case "digit_2": generator.InputDigit(2); break;
                case "digit_3": generator.InputDigit(3); break;
                case "digit_4": generator.InputDigit(4); break;
                case "digit_5": generator.InputDigit(5); break;
                case "digit_6": generator.InputDigit(6); break;
                case "digit_7": generator.InputDigit(7); break;
                case "digit_8": generator.InputDigit(8); break;
                case "digit_9": generator.InputDigit(9); break;

                case "gen_enter":
                    break;

                case "gen_clear":
                    generator.ClearFrequencyInput();
                    break;

                default:
                    Debug.LogWarning($"Неизвестная функция генератора: {buttonFunction}");
                    break;
            }
        }
        // === ИЗМЕРИТЕЛЬНЫЙ УСИЛИТЕЛЬ ===
        else if (buttonFunction.StartsWith("amp_"))
        {
            switch (buttonFunction)
            {
                case "amp_power":
                    amplifier.TogglePower();
                    break;

                case "amp_rpu":
                    amplifier.ToggleRPU();
                    break;

                case "amp_zero":
                    amplifier.SetZero();
                    break;

                case "amp_range":
                    amplifier.CycleInputRange();
                    break;

                case "amp_sens_up":
                    amplifier.ChangeSensitivity(0.5f);
                    break;

                case "amp_sens_down":
                    amplifier.ChangeSensitivity(-0.5f);
                    break;

                case "amp_bias_up":
                    if (amplifier.IsRPUEnabled())
                        amplifier.ChangeBiasVoltage(0.1f);
                    break;

                case "amp_bias_down":
                    if (amplifier.IsRPUEnabled())
                        amplifier.ChangeBiasVoltage(-0.1f);
                    break;

                //case "amp_decimal":
                //    if (amplifier.IsRPUEnabled())
                //        amplifier.InputBiasDecimal();
                //   break;

                case "amp_minus":
                    if (amplifier.IsRPUEnabled())
                        amplifier.InputBiasMinus();
                    break;

                //case "amp_bias_enter":
                //    if (amplifier.IsRPUEnabled())
                //        amplifier.ApplyBiasVoltage();
                //    break;

                case "amp_bias_clear":
                    amplifier.ClearBiasInput();
                    break;

                default:
                    Debug.LogWarning($"Неизвестная функция усилителя: {buttonFunction}");
                    break;
            }
        }

        if (buttonFunction.StartsWith("exp_"))
        {
            LabWork1Controller expController = FindObjectOfType<LabWork1Controller>();
            if (expController == null)
            {
                Debug.LogWarning("LabWork1Controller не найден!");
                return;
            }

            switch (buttonFunction)
            {
                case "exp_ey_x":
                    expController.StartEyXMeasurement();
                    break;

                case "exp_hz_x":
                    expController.StartHzXMeasurement();
                    break;

                case "exp_lambda":
                    expController.MeasureWaveguideWavelength();
                    break;

                case "exp_probe_e":
                    amplifier.SwitchToElectricProbe();
                    break;

                case "exp_probe_h":
                    amplifier.SwitchToMagneticProbe();
                    break;

                case "exp_toggle_square":
                    amplifier.ToggleSquareDetection();
                    break;
            }
        }
    }
}