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
    public float pistonSpeed = 10f;    // мм/сек
    public float probeMoveSpeed = 5f;  // мм/сек

    [Header("Пределы движения")]
    public float pistonMinZ = 0f;      // Минимальная позиция поршня (мм)
    public float pistonMaxZ = 300f;    // Максимальная позиция поршня (мм)
    public float probeMinX = -11.5f;   // Минимальная X позиция датчика (мм)
    public float probeMaxX = 11.5f;    // Максимальная X позиция датчика (мм)
    public float probeMinZ = 0f;       // Минимальная Z позиция датчика (мм)
    public float probeMaxZ = 300f;     // Максимальная Z позиция датчика (мм)

    [Header("Ссылка на ядро расчета")]
    public H10_WaveguideCore waveguideCore;

    [Header("UI для измерений")]
    public Text measurementText;
    public Slider measurementSlider;

    [Header("Начальные позиции")]
    [SerializeField] private float pistonStartPositionMM = 250f;

    // Текущие позиции в миллиметрах
    private float pistonPositionMM = 0f;
    private float probeEPosXMM = 0f;
    private float probeEPosZMM = 150f; // Начальная позиция по центру
    private Vector3 pistonWorldStartPos;
    private Vector3 probeEWorldStartPos;
    private Vector3 probeHWorldStartPos;

    // Для визуализации поля
    private LineRenderer fieldLineRenderer;

    void Start()
    {
        // Инициализация позиций
        pistonPositionMM = pistonStartPositionMM;
        pistonPositionMM = Mathf.Clamp(pistonPositionMM, pistonMinZ, pistonMaxZ);
        probeEPosXMM = Mathf.Clamp(probeEPosXMM, probeMinX, probeMaxX);
        probeEPosZMM = Mathf.Clamp(probeEPosZMM, probeMinZ, probeMaxZ);

        if (piston != null)
        {
            // Сохраняем мировую стартовую позицию
            pistonWorldStartPos = piston.position;
            // Вычисляем текущее смещение в мм
            // Предполагаем движение вдоль оси Z мира
            pistonPositionMM = (piston.position.z - pistonWorldStartPos.z) * 1000f;
        }

        if (probeE != null)
            probeEWorldStartPos = probeE.position;

        if (probeH != null)
            probeHWorldStartPos = probeH.position;

        // Инициализация позиций из текущего положения
        if (probeE != null)
        {
            // Для датчика E берем мировую позицию и преобразуем в мм
            // Предполагаем, что движение по X и Z происходит в мировых координатах
            probeEPosXMM = (probeE.position.x - probeEWorldStartPos.x) * 1000f;
            probeEPosZMM = (probeE.position.z - probeEWorldStartPos.z) * 1000f;

            probeEPosXMM = Mathf.Clamp(probeEPosXMM, probeMinX, probeMaxX);
            probeEPosZMM = Mathf.Clamp(probeEPosZMM, probeMinZ, probeMaxZ);
        }

        // Применяем начальные позиции
        //UpdatePistonPosition();
        //UpdateProbePosition();

        // Создаем LineRenderer для визуализации поля
        CreateFieldVisualizer();

        // Применяем материалы
        ApplyMaterials();

        Debug.Log("LabSetupController: Инициализирован");
    }

    void Update()
    {
        // Обработка управления
        HandleControls();

        // Визуализация поля
        VisualizeField();

        // Обновляем UI измерений
        UpdateMeasurementUI();
    }

    void CreateFieldVisualizer()
    {
        GameObject lineObj = new GameObject("FieldLines");
        lineObj.transform.SetParent(transform);
        fieldLineRenderer = lineObj.AddComponent<LineRenderer>();
        fieldLineRenderer.startWidth = 0.002f;
        fieldLineRenderer.endWidth = 0.002f;
        fieldLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fieldLineRenderer.startColor = Color.red;
        fieldLineRenderer.endColor = Color.blue;
        fieldLineRenderer.positionCount = 0;
        fieldLineRenderer.useWorldSpace = false; // Важно: используем локальные координаты
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
        // Управление поршнем
        HandlePistonControl();

        // Управление датчиком E
        HandleProbeControl();

        // Управление датчиком H (если нужно)
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
            pistonPositionMM += moveInput * pistonSpeed * Time.deltaTime;
            pistonPositionMM = Mathf.Clamp(pistonPositionMM, pistonMinZ, pistonMaxZ);

            // Обновляем позицию поршня
            UpdatePistonPosition();

            // Обновляем в ядре расчета
            waveguideCore.SetPistonPosition(pistonPositionMM);

            Debug.Log($"Поршень: {pistonPositionMM:F1} мм");
        }
    }

    void UpdatePistonPosition()
    {
        if (piston == null) return;

        // Двигаем в мировых координатах вдоль оси Z
        Vector3 newWorldPos = pistonWorldStartPos;
        newWorldPos.z += pistonPositionMM * 0.001f; // Добавляем смещение в метрах

        piston.position = newWorldPos;
    }

    void HandleProbeControl()
    {
        if (probeE == null || waveguideCore == null) return;

        float moveX = 0f, moveZ = 0f;
        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;
        if (Input.GetKey(KeyCode.W)) moveZ = 1f;
        if (Input.GetKey(KeyCode.S)) moveZ = -1f;

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            probeEPosXMM += moveX * probeMoveSpeed * Time.deltaTime;
            probeEPosZMM += moveZ * probeMoveSpeed * Time.deltaTime;

            // Ограничиваем движение внутри волновода
            probeEPosXMM = Mathf.Clamp(probeEPosXMM, probeMinX, probeMaxX);
            probeEPosZMM = Mathf.Clamp(probeEPosZMM, probeMinZ, probeMaxZ);

            // Обновляем позицию датчика
            UpdateProbeEPosition();

            // Обновляем измерение
            UpdateProbeMeasurement();
        }
    }

    void HandleProbeHControl()
    {
        // Аналогично для датчика H при необходимости
        // Можно использовать другие клавиши
    }

    void UpdateProbeEPosition()
    {
        if (probeE == null) return;

        // Используем мировые координаты как с поршнем
        Vector3 newWorldPos = probeEWorldStartPos;

        // Двигаем по мировым осям X и Z
        newWorldPos.x += probeEPosXMM * 0.001f; // мм → м
        newWorldPos.z += probeEPosZMM * 0.001f; // мм → м
                                                // Y оставляем как было изначально

        probeE.position = newWorldPos;
    }

    void UpdateProbeMeasurement()
    {
        if (waveguideCore == null || probeE == null) return;

        // Создаем вектор позиции в метрах
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

    void VisualizeField()
    {
        if (waveguideCore == null || fieldLineRenderer == null || !waveguideCore.IsPropagating())
            return;

        // Визуализируем распределение поля вдоль волновода
        int points = 100;
        fieldLineRenderer.positionCount = points;

        float a = waveguideCore.GetA() * 1000f; // м → мм
        float length = pistonMaxZ; // Используем максимальную длину поршня

        for (int i = 0; i < points; i++)
        {
            float z = (i / (float)(points - 1)) * length;
            Vector3 point = new Vector3(0, 0, z * 0.001f); // По центру волновода по x

            Vector3 field = waveguideCore.GetElectricFieldAt(point, Time.time);

            // Создаем точку с учетом амплитуды поля
            Vector3 fieldOffset = field.normalized * (field.magnitude * 0.01f);
            Vector3 finalPoint = point + fieldOffset;

            fieldLineRenderer.SetPosition(i, finalPoint);

            // Динамическое изменение цвета
            if (i % 2 == 0)
            {
                fieldLineRenderer.startColor = Color.Lerp(Color.red, Color.blue,
                    Mathf.Sin(Time.time + i * 0.1f) * 0.5f + 0.5f);
                fieldLineRenderer.endColor = Color.Lerp(Color.blue, Color.red,
                    Mathf.Cos(Time.time + i * 0.1f) * 0.5f + 0.5f);
            }
        }
    }

    void UpdateMeasurementUI()
    {
        if (measurementText == null || waveguideCore == null)
            return;

        Vector3 probePos = new Vector3(
            probeEPosXMM * 0.001f,
            0f,
            probeEPosZMM * 0.001f
        );

        Vector3 eField = waveguideCore.GetElectricFieldAt(probePos, Time.time);
        float fieldStrength = eField.magnitude;

        measurementText.text =
            $"Датчик E:\n" +
            $"Позиция: x={probeEPosXMM:F1} мм, z={probeEPosZMM:F1} мм\n" +
            $"Напряженность: {fieldStrength:F3} В/м\n" +
            $"Режим: {(waveguideCore.IsPropagating() ? "Распространение" : "Ниже отсечки")}\n" +
            $"Длина волны в волноводе: {waveguideCore.GetWaveguideWavelengthMM():F2} мм";

        if (measurementSlider != null)
        {
            measurementSlider.value = Mathf.Clamp01(fieldStrength / 10f);
        }
    }

    // Метод для изменения размера волновода в реальном времени
    public void UpdateWaveguideSize(float widthMM, float heightMM)
    {
        if (waveguideBody != null && waveguideCore != null)
        {
            // Обновляем пределы движения датчика
            probeMinX = -widthMM / 2f;
            probeMaxX = widthMM / 2f;

            // Масштабируем 3D модель
            Vector3 scale = waveguideBody.localScale;
            scale.x = widthMM / 1000f; // мм → м
            scale.y = heightMM / 1000f; // мм → м
            waveguideBody.localScale = scale;

            // Обновляем в ядре
            waveguideCore.SetWaveguideSize(widthMM, heightMM);

            Debug.Log($"Размер волновода обновлен: {widthMM}×{heightMM} мм");
        }
    }

    // Метод для сброса всех позиций
    public void ResetPositions()
    {
        pistonPositionMM = pistonMinZ;
        probeEPosXMM = 0f;
        probeEPosZMM = (probeMinZ + probeMaxZ) / 2f; // По центру

        UpdatePistonPosition();
        UpdateProbeEPosition();

        if (waveguideCore != null)
        {
            waveguideCore.SetPistonPosition(pistonPositionMM);
        }

        Debug.Log("Позиции сброшены");
    }

    // Методы для вызова из UI
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

    public void SetProbePositionX(float xMM)
    {
        probeEPosXMM = Mathf.Clamp(xMM, probeMinX, probeMaxX);
        UpdateProbeEPosition();
    }

    public void SetProbePositionZ(float zMM)
    {
        probeEPosZMM = Mathf.Clamp(zMM, probeMinZ, probeMaxZ);
        UpdateProbeEPosition();
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация в редакторе только при выделении
        if (waveguideBody != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Полупрозрачный циан
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
        }

        if (probeH != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(probeH.position, 0.008f);
        }
    }
}