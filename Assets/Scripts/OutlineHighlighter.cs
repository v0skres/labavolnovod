using UnityEngine;

public class OutlineHighlighter : MonoBehaviour
{
    [Header("Настройки свечения")]
    public Color emissionColor = Color.yellow;
    public float emissionIntensity = 2f;
    public float fadeSpeed = 5f;

    private Renderer buttonRenderer;
    private Color originalEmissionColor;
    private float currentEmission = 0f;
    private bool shouldHighlight = false;

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();

        if (buttonRenderer != null)
        {
            originalEmissionColor = buttonRenderer.material.GetColor("_EmissionColor");

            buttonRenderer.material.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        float targetEmission = shouldHighlight ? emissionIntensity : 0f;
        currentEmission = Mathf.Lerp(currentEmission, targetEmission, Time.deltaTime * fadeSpeed);

        if (buttonRenderer != null)
        {
            Color finalColor = shouldHighlight ?
                emissionColor * currentEmission :
                originalEmissionColor;

            buttonRenderer.material.SetColor("_EmissionColor", finalColor);

            DynamicGI.SetEmissive(buttonRenderer, finalColor);
        }
    }

    void OnMouseEnter()
    {
        shouldHighlight = true;
    }

    void OnMouseExit()
    {
        shouldHighlight = false;
    }

    public void StartHighlight()
    {
        shouldHighlight = true;
    }

    public void StopHighlight()
    {
        shouldHighlight = false;
    }

    public void ToggleHighlight()
    {
        shouldHighlight = !shouldHighlight;
    }

    public void HighlightForInteraction(string buttonFunction)
    {
        StartHighlight();

        Invoke("StopHighlight", 2f);
    }
}