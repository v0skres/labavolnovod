using UnityEngine;
using UnityEngine.UI;

public class LabSetupController : MonoBehaviour
{
    [Header("Ссылки на 3D модель")]
    public Transform waveguideBody;     // Тело волновода
    public Transform piston;           // Поршень
    public Transform probeE;           // Датчик E (штырь)
    public Transform probeH;           // Датчик H (петля)

    [Header("Материалы для визуализации")]
    public Material waveguideMaterial;
    public Material pistonMaterial;
    public Material probeEMaterial;
    public Material probeHMaterial;

    [Header("Настройки движения")]
    public float pistonSpeed = 10f;    
    public float probeMoveSpeed = 5f; 

    [Header("Настройки анимации датчиков")]
    public float probeMoveSmoothTime = 0.1f; 
    public bool useSmoothMovement = true;    

    [Header("Пределы движения")]
    public float pistonMinZ = 0f;    
    public float pistonMaxZ = 300f;  
    public float probeMinX = -11.5f;   
    public float probeMaxX = 11.5f;  
    public float probeMinZ = 0f;      
    public float probeMaxZ = 300f;     

    [Header("Настройки датчика H")]
    public float probeHPosXMM = 0f;
    public float probeHPosZMM = 150f;
    public KeyCode probeHLeftKey = KeyCode.J;
    public KeyCode probeHRightKey = KeyCode.L;
    public KeyCode probeHUpKey = KeyCode.I;
    public KeyCode probeHDownKey = KeyCode.K;

    [Header("Ссылка на ядро расчета")]
    public H10_WaveguideCore waveguideCore;

    [Header("UI для измерений")]
    public Text measurementText;
    public Slider measurementSlider;
    public Text probeHMeasurementText;
    public Slider probeHMeasurementSlider;

    [Header("Начальные позиции")]
    [SerializeField] private float pistonStartPositionMM = 250f;

    [Header("Ссылка на усилитель")]
    public MeasuringAmplifier measuringAmplifier;

    [Header("Дисплеи позиций")]
    public TextMesh pistonDisplay;       
    public TextMesh probeEDisplay;       
    public TextMesh probeHDisplay;     

    [Header("Настройки отображения")]
    public bool showPositionsInMM = true; 
    public string pistonFormat = "Поршень: {0:F1} мм";
    public string probeEFormat = "E: x={0:F1} мм, z={1:F1} мм";
    public string probeHFormat = "H: x={0:F1} мм, z={1:F1} мм";

    public float pistonPositionMM = 0f;
    public float probeEPosXMM = 0f;
    public float probeEPosZMM = 150f; 
    private Vector3 pistonWorldStartPos;
    private Vector3 probeEWorldStartPos;
    private Vector3 probeHWorldStartPos;

    private Vector3 probeETargetPosition;
    private Vector3 probeHTargetPosition;
    private Vector3 probeEVelocity = Vector3.zero;
    private Vector3 probeHVelocity = Vector3.zero;

    private LineRenderer fieldLineRenderer;
    private LineRenderer magneticFieldLineRenderer;

    void Start()
    {
        pistonPositionMM = pistonStartPositionMM;
        pistonPositionMM = Mathf.Clamp(pistonPositionMM, pistonMinZ, pistonMaxZ);
        probeEPosXMM = Mathf.Clamp(probeEPosXMM, probeMinX, probeMaxX);
        probeEPosZMM = Mathf.Clamp(probeEPosZMM, probeMinZ, probeMaxZ);
        probeHPosXMM = Mathf.Clamp(probeHPosXMM, probeMinX, probeMaxX);
        probeHPosZMM = Mathf.Clamp(probeHPosZMM, probeMinZ, probeMaxZ);

        if (piston != null)
        {
            pistonWorldStartPos = piston.position;
            pistonPositionMM = (piston.position.z - pistonWorldStartPos.z) * 1000f;
        }

        if (probeE != null)
            probeEWorldStartPos = probeE.position;

        if (probeH != null)
            probeHWorldStartPos = probeH.position;

        if (probeE != null)
        {
            probeEPosXMM = (probeE.position.x - probeEWorldStartPos.x) * 1000f;
            probeEPosZMM = (probeE.position.z - probeEWorldStartPos.z) * 1000f;
            probeEPosXMM = Mathf.Clamp(probeEPosXMM, probeMinX, probeMaxX);
            probeEPosZMM = Mathf.Clamp(probeEPosZMM, probeMinZ, probeMaxZ);
        }

        if (probeH != null)
        {
            probeHPosXMM = (probeH.position.x - probeHWorldStartPos.x) * 1000f;
            probeHPosZMM = (probeH.position.z - probeHWorldStartPos.z) * 1000f;
            probeHPosXMM = Mathf.Clamp(probeHPosXMM, probeMinX, probeMaxX);
            probeHPosZMM = Mathf.Clamp(probeHPosZMM, probeMinZ, probeMaxZ);
        }

        probeETargetPosition = CalculateProbeEPosition();
        probeHTargetPosition = CalculateProbeHPosition();

        if (probeE != null)
        {
            probeE.position = probeETargetPosition;
            Debug.Log($"Датчик E: начальная позиция = {probeE.position}");
        }
        if (probeH != null)
        {
            probeH.position = probeHTargetPosition;
            Debug.Log($"Датчик H: начальная позиция = {probeH.position}");
        }

        CreateFieldVisualizers();

        ApplyMaterials();

        UpdatePistonPosition();

        Debug.Log("LabSetupController: Инициализация завершена");
        Debug.Log($"Начальные позиции: E(x={probeEPosXMM:F1}мм, z={probeEPosZMM:F1}мм), " +
                 $"H(x={probeHPosXMM:F1}мм, z={probeHPosZMM:F1}мм)");

        InitializeDisplays();
    }

    void Update()
    {
        HandleControls();

        UpdateProbePositionsSmoothly();

        VisualizeFields();

        UpdateMeasurementUI();

        if (measuringAmplifier != null && measuringAmplifier.isOn)
        {
            UpdateProbeMeasurementInAmplifier();
        }

        UpdatePositionDisplays();
    }

    void InitializeDisplays()
    {
        CreateDisplaysIfNeeded();

        UpdatePositionDisplays();
    }

    void CreateDisplaysIfNeeded()
    {
        if (pistonDisplay == null && piston != null)
        {
            pistonDisplay = CreateDisplay("PistonDisplay", piston);
            pistonDisplay.transform.localPosition = new Vector3(0, 0.02f, 0); 
        }

        if (probeEDisplay == null && probeE != null)
        {
            probeEDisplay = CreateDisplay("ProbeEDisplay", probeE);
            probeEDisplay.transform.localPosition = new Vector3(0, 0.03f, 0); 
            probeEDisplay.color = Color.red;
        }

        if (probeHDisplay == null && probeH != null)
        {
            probeHDisplay = CreateDisplay("ProbeHDisplay", probeH);
            probeHDisplay.transform.localPosition = new Vector3(0, 0.03f, 0); 
            probeHDisplay.color = Color.blue;
        }
    }

    TextMesh CreateDisplay(string name, Transform parent)
    {
        GameObject displayObj = new GameObject(name);
        displayObj.transform.SetParent(parent);
        displayObj.transform.localPosition = Vector3.zero;
        displayObj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = displayObj.AddComponent<TextMesh>();
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.01f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        return textMesh;
    }

    void UpdatePositionDisplays()
    {
        if (pistonDisplay != null)
        {
            pistonDisplay.text = string.Format(pistonFormat, pistonPositionMM);
            pistonDisplay.gameObject.SetActive(waveguideCore != null && waveguideCore.IsPropagating());
        }

        if (probeEDisplay != null)
        {
            probeEDisplay.text = string.Format(probeEFormat, probeEPosXMM, probeEPosZMM);
        }

        if (probeHDisplay != null)
        {
            probeHDisplay.text = string.Format(probeHFormat, probeHPosXMM, probeHPosZMM);
        }
    }

    void UpdateProbeMeasurementInAmplifier()
    {
        if (measuringAmplifier == null) return;
    }

    void UpdateProbePositionsSmoothly()
    {
        if (useSmoothMovement)
        {
            if (probeE != null)
            {
                probeETargetPosition = CalculateProbeEPosition();
                probeE.position = Vector3.SmoothDamp(
                    probeE.position,
                    probeETargetPosition,
                    ref probeEVelocity,
                    probeMoveSmoothTime
                );
            }

            if (probeH != null)
            {
                probeHTargetPosition = CalculateProbeHPosition();
                probeH.position = Vector3.SmoothDamp(
                    probeH.position,
                    probeHTargetPosition,
                    ref probeHVelocity,
                    probeMoveSmoothTime
                );
            }
        }
        else
        {
            UpdateProbeEPosition();
            UpdateProbeHPosition();
        }
    }

    Vector3 CalculateProbeEPosition()
    {
        Vector3 newWorldPos = probeEWorldStartPos;
        newWorldPos.x += probeEPosXMM * 0.001f; 
        newWorldPos.z += probeEPosZMM * 0.001f; 
        return newWorldPos;
    }

    Vector3 CalculateProbeHPosition()
    {
        Vector3 newWorldPos = probeHWorldStartPos;
        newWorldPos.x += probeHPosXMM * 0.001f; 
        newWorldPos.z += probeHPosZMM * 0.001f; 
        return newWorldPos;
    }

    void UpdateProbeEPosition()
    {
        if (probeE != null)
        {
            probeE.position = CalculateProbeEPosition();
        }
    }

    void UpdateProbeHPosition()
    {
        if (probeH != null)
        {
            probeH.position = CalculateProbeHPosition();
        }
    }

    void CreateFieldVisualizers()
    {
        GameObject eFieldLineObj = new GameObject("ElectricFieldLines");
        eFieldLineObj.transform.SetParent(transform);
        fieldLineRenderer = eFieldLineObj.AddComponent<LineRenderer>();
        fieldLineRenderer.startWidth = 0.002f;
        fieldLineRenderer.endWidth = 0.002f;
        fieldLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fieldLineRenderer.startColor = Color.red;
        fieldLineRenderer.endColor = Color.red;
        fieldLineRenderer.positionCount = 0;
        fieldLineRenderer.useWorldSpace = false;

        GameObject hFieldLineObj = new GameObject("MagneticFieldLines");
        hFieldLineObj.transform.SetParent(transform);
        magneticFieldLineRenderer = hFieldLineObj.AddComponent<LineRenderer>();
        magneticFieldLineRenderer.startWidth = 0.002f;
        magneticFieldLineRenderer.endWidth = 0.002f;
        magneticFieldLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        magneticFieldLineRenderer.startColor = Color.blue;
        magneticFieldLineRenderer.endColor = Color.blue;
        magneticFieldLineRenderer.positionCount = 0;
        magneticFieldLineRenderer.useWorldSpace = false;
    }

    void ApplyMaterials()
    {
        ApplyMaterial(waveguideBody, waveguideMaterial);
        ApplyMaterial(piston, pistonMaterial);
        ApplyMaterial(probeE, probeEMaterial);
        ApplyMaterial(probeH, probeHMaterial);
    }

    void ApplyMaterial(Transform obj, Material material)
    {
        if (obj != null && material != null)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null) renderer.material = material;
        }
    }

    void HandleControls()
    {
        HandlePistonControl();

        HandleProbeEControl();

        HandleProbeHControl();
    }

    void HandlePistonControl()
    {
        if (piston == null || waveguideCore == null) return;

        float moveInput = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) moveInput = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveInput = -1f;

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float oldPosition = pistonPositionMM;
            pistonPositionMM += moveInput * pistonSpeed * Time.deltaTime;
            pistonPositionMM = Mathf.Clamp(pistonPositionMM, pistonMinZ, pistonMaxZ);

            UpdatePistonPosition();

            if (Mathf.Abs(oldPosition - pistonPositionMM) > 0.01f)
            {
                waveguideCore.SetPistonPosition(pistonPositionMM);

                Debug.Log($"LabSetupController: Поршень: {pistonPositionMM:F1} мм " +
                         $"(Δ={(pistonPositionMM - oldPosition):F2} мм, " +
                         $"клавиша: {(moveInput > 0 ? "Вперёд" : "Назад")})");
            }
        }
    }

    public void UpdatePistonPosition()
    {
        if (piston == null) return;

        Vector3 newWorldPos = pistonWorldStartPos;
        newWorldPos.z += pistonPositionMM * 0.001f;
        piston.position = newWorldPos;
    }

    void HandleProbeEControl()
    {
        if (probeE == null || waveguideCore == null) return;

        float moveX = 0f, moveZ = 0f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;
        if (Input.GetKey(KeyCode.W)) moveZ = 1f;
        if (Input.GetKey(KeyCode.S)) moveZ = -1f;

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            float oldX = probeEPosXMM;
            float oldZ = probeEPosZMM;

            probeEPosXMM += moveX * probeMoveSpeed * Time.deltaTime;
            probeEPosZMM += moveZ * probeMoveSpeed * Time.deltaTime;

            probeEPosXMM = Mathf.Clamp(probeEPosXMM, probeMinX, probeMaxX);
            probeEPosZMM = Mathf.Clamp(probeEPosZMM, probeMinZ, probeMaxZ);

            probeETargetPosition = CalculateProbeEPosition();

            if (!useSmoothMovement)
            {
                UpdateProbeEPosition();
            }

            UpdateProbeEMeasurement();

            Debug.Log($"Датчик E: X {oldX:F1}→{probeEPosXMM:F1} мм, " +
                     $"Z {oldZ:F1}→{probeEPosZMM:F1} мм, " +
                     $"ΔX={moveX}, ΔZ={moveZ}");

            if (measuringAmplifier != null && measuringAmplifier.isElectricProbe)
            {
                measuringAmplifier.UpdateMeasurementFromWaveguide();
            }
        }
    }

    void UpdateProbeEMeasurement()
    {
        if (waveguideCore == null || probeE == null) return;

        Vector3 probePos = new Vector3(
            probeEPosXMM * 0.001f,
            0f,
            probeEPosZMM * 0.001f
        );

        Vector3 eField = waveguideCore.GetElectricFieldAt(probePos, Time.time);
        float fieldStrength = eField.magnitude;

        Debug.Log($"Датчик E: x={probeEPosXMM:F1}мм, " +
                 $"z={probeEPosZMM:F1}мм, " +
                 $"E={fieldStrength:F3} В/м");
    }

    void HandleProbeHControl()
    {
        if (probeH == null || waveguideCore == null) return;

        float moveX = 0f, moveZ = 0f;
        if (Input.GetKey(probeHLeftKey)) moveX = -1f;
        if (Input.GetKey(probeHRightKey)) moveX = 1f;
        if (Input.GetKey(probeHUpKey)) moveZ = 1f;
        if (Input.GetKey(probeHDownKey)) moveZ = -1f;

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            float oldX = probeHPosXMM;
            float oldZ = probeHPosZMM;

            probeHPosXMM += moveX * probeMoveSpeed * Time.deltaTime;
            probeHPosZMM += moveZ * probeMoveSpeed * Time.deltaTime;

            probeHPosXMM = Mathf.Clamp(probeHPosXMM, probeMinX, probeMaxX);
            probeHPosZMM = Mathf.Clamp(probeHPosZMM, probeMinZ, probeMaxZ);

            probeHTargetPosition = CalculateProbeHPosition();

            if (!useSmoothMovement)
            {
                UpdateProbeHPosition();
            }

            UpdateProbeHMeasurement();

            Debug.Log($"Датчик H: X {oldX:F1}→{probeHPosXMM:F1} мм, " +
                     $"Z {oldZ:F1}→{probeHPosZMM:F1} мм, " +
                     $"ΔX={moveX}, ΔZ={moveZ}");
        }
    }

    void UpdateProbeHMeasurement()
    {
        if (waveguideCore == null || probeH == null) return;

        Vector3 probePos = new Vector3(
            probeHPosXMM * 0.001f,
            0f,
            probeHPosZMM * 0.001f
        );

        Vector3 hField = waveguideCore.GetMagneticFieldAt(probePos, Time.time);
        float fieldStrength = hField.magnitude;

        Debug.Log($"Датчик H: x={probeHPosXMM:F1}мм, " +
                 $"z={probeHPosZMM:F1}мм, " +
                 $"H={fieldStrength:F3} А/м");
    }

    void VisualizeFields()
    {
        VisualizeElectricField();
        VisualizeMagneticField();
    }

    void VisualizeElectricField()
    {
        if (waveguideCore == null || fieldLineRenderer == null || !waveguideCore.IsPropagating())
            return;

        int points = 100;
        fieldLineRenderer.positionCount = points;

        float a = waveguideCore.GetA() * 1000f;
        float length = pistonMaxZ;

        for (int i = 0; i < points; i++)
        {
            float z = (i / (float)(points - 1)) * length;
            Vector3 point = new Vector3(0, 0, z * 0.001f);

            Vector3 field = waveguideCore.GetElectricFieldAt(point, Time.time);
            Vector3 fieldOffset = field.normalized * (field.magnitude * 0.01f);
            Vector3 finalPoint = point + fieldOffset;

            fieldLineRenderer.SetPosition(i, finalPoint);

            Color color = Color.Lerp(Color.red, Color.yellow,
                Mathf.Sin(Time.time + i * 0.1f) * 0.5f + 0.5f);
            fieldLineRenderer.startColor = color;
            fieldLineRenderer.endColor = color;
        }
    }

    void VisualizeMagneticField()
    {
        if (waveguideCore == null || magneticFieldLineRenderer == null || !waveguideCore.IsPropagating())
            return;

        int points = 80;
        magneticFieldLineRenderer.positionCount = points;

        float a = waveguideCore.GetA() * 1000f;
        float length = pistonMaxZ;

        for (int i = 0; i < points; i++)
        {
            float z = (i / (float)(points - 1)) * length;
            Vector3 point = new Vector3(0.01f, 0, z * 0.001f);

            Vector3 field = waveguideCore.GetMagneticFieldAt(point, Time.time);
            Vector3 fieldOffset = field.normalized * (field.magnitude * 0.01f);
            Vector3 finalPoint = point + fieldOffset;

            magneticFieldLineRenderer.SetPosition(i, finalPoint);

            Color color = Color.Lerp(Color.blue, Color.cyan,
                Mathf.Cos(Time.time + i * 0.1f) * 0.5f + 0.5f);
            magneticFieldLineRenderer.startColor = color;
            magneticFieldLineRenderer.endColor = color;
        }
    }

    void UpdateMeasurementUI()
    {
        if (measurementText == null || waveguideCore == null)
            return;

        Vector3 probeEPos = new Vector3(
            probeEPosXMM * 0.001f,
            0f,
            probeEPosZMM * 0.001f
        );

        Vector3 eField = waveguideCore.GetElectricFieldAt(probeEPos, Time.time);
        float eFieldStrength = eField.magnitude;

        Vector3 probeHPos = new Vector3(
            probeHPosXMM * 0.001f,
            0f,
            probeHPosZMM * 0.001f
        );

        Vector3 hField = waveguideCore.GetMagneticFieldAt(probeHPos, Time.time);
        float hFieldStrength = hField.magnitude;

        measurementText.text =
            $"Датчик E (штырь):\n" +
            $"Позиция: x={probeEPosXMM:F1} мм, z={probeEPosZMM:F1} мм\n" +
            $"E_y: {eFieldStrength:F3} В/м\n" +
            $"Режим: {(waveguideCore.IsPropagating() ? "Распространение" : "Ниже отсечки")}";

        if (measurementSlider != null)
        {
            measurementSlider.value = Mathf.Clamp01(eFieldStrength / 10f);
        }

        if (probeHMeasurementText != null)
        {
            probeHMeasurementText.text =
                $"Датчик H (петля):\n" +
                $"Позиция: x={probeHPosXMM:F1} мм, z={probeHPosZMM:F1} мм\n" +
                $"H_x: {hField.x:F3} А/м\n" +
                $"H_z: {hField.z:F3} А/м\n" +
                $"|H|: {hFieldStrength:F3} А/м";
        }

        if (probeHMeasurementSlider != null)
        {
            probeHMeasurementSlider.value = Mathf.Clamp01(hFieldStrength / 10f);
        }
    }

    public void UpdateWaveguideSize(float widthMM, float heightMM)
    {
        if (waveguideBody != null && waveguideCore != null)
        {
            probeMinX = -widthMM / 2f;
            probeMaxX = widthMM / 2f;

            Vector3 scale = waveguideBody.localScale;
            scale.x = widthMM / 1000f;
            scale.y = heightMM / 1000f;
            waveguideBody.localScale = scale;

            waveguideCore.SetWaveguideSize(widthMM, heightMM);

            Debug.Log($"Размер волновода обновлен: {widthMM}×{heightMM} мм");
        }
    }

    public void ResetPositions()
    {
        pistonPositionMM = pistonMinZ;
        probeEPosXMM = 0f;
        probeEPosZMM = (probeMinZ + probeMaxZ) / 2f;
        probeHPosXMM = 0f;
        probeHPosZMM = (probeMinZ + probeMaxZ) / 2f;

        probeETargetPosition = CalculateProbeEPosition();
        probeHTargetPosition = CalculateProbeHPosition();

        UpdatePistonPosition();
        UpdateProbeEPosition();
        UpdateProbeHPosition();

        if (waveguideCore != null)
        {
            waveguideCore.SetPistonPosition(pistonPositionMM);
        }

        Debug.Log("Позиции сброшены");
    }

    public void MovePistonForward()
    {
        pistonPositionMM = Mathf.Min(pistonPositionMM + 10f, pistonMaxZ);
        UpdatePistonPosition();
        if (waveguideCore != null) waveguideCore.SetPistonPosition(pistonPositionMM);
    }

    public void MovePistonBackward()
    {
        pistonPositionMM = Mathf.Max(pistonPositionMM - 10f, pistonMinZ);
        UpdatePistonPosition();
        if (waveguideCore != null) waveguideCore.SetPistonPosition(pistonPositionMM);
    }

    public void SetProbeEPositionX(float xMM)
    {
        probeEPosXMM = Mathf.Clamp(xMM, probeMinX, probeMaxX);
        probeETargetPosition = CalculateProbeEPosition();

        if (!useSmoothMovement)
        {
            UpdateProbeEPosition();
        }
    }

    public void SetProbeEPositionZ(float zMM)
    {
        probeEPosZMM = Mathf.Clamp(zMM, probeMinZ, probeMaxZ);
        probeETargetPosition = CalculateProbeEPosition();

        if (!useSmoothMovement)
        {
            UpdateProbeEPosition();
        }
    }

    public void SetProbeHPositionX(float xMM)
    {
        probeHPosXMM = Mathf.Clamp(xMM, probeMinX, probeMaxX);
        probeHTargetPosition = CalculateProbeHPosition();

        if (!useSmoothMovement)
        {
            UpdateProbeHPosition();
        }
    }

    public void SetProbeHPositionZ(float zMM)
    {
        probeHPosZMM = Mathf.Clamp(zMM, probeMinZ, probeMaxZ);
        probeHTargetPosition = CalculateProbeHPosition();

        if (!useSmoothMovement)
        {
            UpdateProbeHPosition();
        }
    }

    public void ToggleWaveMode(bool isStandingWave)
    {
        if (waveguideCore != null)
        {
            waveguideCore.SetWaveMode(isStandingWave);
            Debug.Log($"Режим волны: {(isStandingWave ? "Стоячая" : "Бегущая")}");
        }
    }

    public void ToggleSmoothMovement()
    {
        useSmoothMovement = !useSmoothMovement;
        Debug.Log($"Плавное движение: {(useSmoothMovement ? "ВКЛ" : "ВЫКЛ")}");
    }

    public void SetSmoothTime(float smoothTime)
    {
        probeMoveSmoothTime = Mathf.Clamp(smoothTime, 0.01f, 1f);
        Debug.Log($"Время сглаживания: {probeMoveSmoothTime:F2} сек");
    }

    void OnDrawGizmosSelected()
    {
        if (waveguideBody != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawCube(waveguideBody.position, waveguideBody.lossyScale);
        }

        if (piston != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(piston.position, new Vector3(0.03f, 0.015f, 0.01f));
        }

        if (probeE != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(probeE.position, 0.005f);
            Gizmos.DrawLine(probeE.position, probeE.position + probeE.up * 0.02f);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(probeETargetPosition, 0.003f);
                Gizmos.DrawLine(probeE.position, probeETargetPosition);
            }
        }

        if (probeH != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(probeH.position, 0.008f);

            Vector3 center = probeH.position;
            float radius = 0.01f;
            int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * Mathf.PI * 2 / segments;
                float angle2 = (i + 1) * Mathf.PI * 2 / segments;
                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);
                Gizmos.DrawLine(point1, point2);
            }

            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(probeHTargetPosition, 0.003f);
                Gizmos.DrawLine(probeH.position, probeHTargetPosition);
            }
        }
    }

    void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.5f);

        GUILayout.BeginArea(new Rect(10, 10, 300, 180));
        GUILayout.BeginVertical("Box");

        GUILayout.Label("=== УПРАВЛЕНИЕ ===");
        GUILayout.Label($"Датчик E: W/A/S/D");
        GUILayout.Label($"Датчик H: I/J/K/l");
        GUILayout.Label($"Поршень: ↑/↓");

        GUILayout.Space(10);

        //if (GUILayout.Button("Переключить плавное движение"))
        //{
        //    ToggleSmoothMovement();
        //}

        //if (GUILayout.Button("Сбросить в центр"))
        //{
        //    ResetPositions();
        //}

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}