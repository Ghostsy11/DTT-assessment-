using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExcationMazeOrder : MonoBehaviour
{
    //  Core Components
    [Header("Core Script References")]
    [Tooltip("Handles enabling/disabling or destroying pooled maze cubes.")]
    [SerializeReference] MazeGridPoolManager poolManager;

    [Tooltip("Handles resizing and spacing of maze cubes.")]
    [SerializeField] GridSizeManager gridSizeManager;

    [Tooltip("Main controller for grid generation and map configuration.")]
    [SerializeField] MazeGenerator mazeGenerator;

    [Tooltip("Animated step-by-step Prim’s algorithm (First Approach).")]
    [SerializeField] PrimsFirstApproach primsFirst;

    [Tooltip("Frame-by-frame logic-based Prim’s algorithm (Second Approach).")]
    [SerializeField] PrimsSecondApproach primsSecond;


    //  Maze Size Sliders
    [Header("Maze Dimension Sliders")]
    [Tooltip("Text UI for Z-Length of the maze.")]
    [SerializeField] TextMeshProUGUI zText;

    [Tooltip("Slider UI to control Z-Length of the maze.")]
    [SerializeField] Slider zLength;

    [Tooltip("Text UI for X-Width of the maze.")]
    [SerializeField] TextMeshProUGUI xText;

    [Tooltip("Slider UI to control X-Width of the maze.")]
    [SerializeField] Slider xWidth;


    //  Cube Size Controls
    [Header("Cube Size & Spacing")]
    [Tooltip("Slider to adjust the size (scale) of each cube.")]
    [SerializeField] Slider cubeSizeSlider;

    [Tooltip("Text display for current cube size.")]
    [SerializeField] TextMeshProUGUI cubeSizeText;

    [Tooltip("Slider to control spacing between cubes.")]
    [SerializeField] Slider spacingSlider;

    [Tooltip("Text display for current spacing value.")]
    [SerializeField] TextMeshProUGUI spacingText;


    //  Prim’s Step Controls
    [Header("Prim’s Step Settings")]
    [Tooltip("Slider to control step delay (used in First Approach).")]
    [SerializeField] Slider stepDelaySlider;

    [Tooltip("Text display for current step delay.")]
    [SerializeField] TextMeshProUGUI stepDelayText;

    [Tooltip("Slider to control step size used when carving.")]
    [SerializeField] Slider stepSizeSlider;

    [Tooltip("Text display for current step size.")]
    [SerializeField] TextMeshProUGUI stepSizeText;


    //  First Approach Controls
    [Header("First Approach UI Controls")]
    [Tooltip("Toggle to select First Approach (Animated Coroutine).")]
    [SerializeField] Toggle primsFirstApproch;

    [Tooltip("Button to start maze generation using First Approach.")]
    [SerializeField] Button primsFirstApprochButton;

    [Tooltip("Button to re-run/reform the maze using First Approach.")]
    [SerializeField] Button primsFirstReformTheMaze;


    //  Second Approach Controls
    [Header("Second Approach UI Controls")]
    [Tooltip("Toggle to select Second Approach (Frame-based logic).")]
    [SerializeField] Toggle primsSecoundApproch;

    [Tooltip("Button to start maze generation using Second Approach.")]
    [SerializeField] Button primsSecoundApprochButton;

    [Tooltip("Button to re-run/reform the maze using Second Approach.")]
    [SerializeField] Button primsSecoundReformTheMaze;

    [SerializeField] private MazeCameraController cameraController;
    private int currentViewIndex = 0;


    void Start()
    {
        SetFieldsReady();
        ResetUI();
    }

    private void Update()
    {
        if (cameraController == null)
        {

            cameraController = FindObjectOfType<MazeCameraController>();
        }
        else { return; }
    }

    public void OnXWidthSliderChanged(float value)
    {
        int rounded = Mathf.RoundToInt(value);
        mazeGenerator.xWidth = rounded;
        xText.text = rounded.ToString();
        poolManager.DestroyAllCubes();

    }

    public void OnZLengthSliderChanged(float value)
    {
        int rounded = Mathf.RoundToInt(value);
        mazeGenerator.zLength = rounded;
        zText.text = rounded.ToString();
        poolManager.DestroyAllCubes();
    }

    public void OnCubeSizeChanged(float value)
    {
        int rounded = Mathf.RoundToInt(value);
        gridSizeManager.cubeSize = rounded;
        cubeSizeText.text = rounded.ToString();
    }

    public void OnSpacingChanged(float value)
    {
        gridSizeManager.spacing = value;
        spacingText.text = value.ToString();
    }
    public void OnStepDelayChanged(float value)
    {
        primsFirst.stepDelay = value;
        stepDelayText.text = value.ToString("F2");
    }

    public void OnStepSizeChanged(float value)
    {
        int rounded = Mathf.RoundToInt(value);
        primsFirst.stepSize = rounded;
        stepSizeText.text = rounded.ToString();
    }


    private void OnFirstApproachToggled(bool isOn)
    {
        if (isOn)
        {
            // Lock both toggles after choosing
            primsFirstApproch.interactable = false;
            primsSecoundApproch.interactable = false;
            primsSecoundApproch.isOn = false;

            // Enable the "Generate" button, but disable it after it's used
            primsFirstApprochButton.interactable = true;
            primsFirstApprochButton.onClick.RemoveAllListeners(); // avoid double subscription
            primsFirstApprochButton.onClick.AddListener(() =>
            {
                // Disable generate after click
                primsFirstApprochButton.interactable = false;
                cameraController.FocusOnMaze(mazeGenerator.xWidth, mazeGenerator.zLength);
                // Start generation
                StartCoroutine(primsFirst.GenerateAndRun());
            });

            // Allow reform anytime after generation
            primsFirstReformTheMaze.interactable = true;
            primsFirstReformTheMaze.onClick.RemoveAllListeners();
            primsFirstReformTheMaze.onClick.AddListener(() =>
            {
                StartCoroutine(primsFirst.ReRunRoutine());
            });

            // Disable second buttons
            primsSecoundApprochButton.interactable = false;
            primsSecoundReformTheMaze.interactable = false;
        }
    }

    private void OnSecondApproachToggled(bool isOn)
    {
        if (isOn)
        {
            primsSecoundApproch.interactable = false;
            primsFirstApproch.interactable = false;
            primsFirstApproch.isOn = false;

            primsSecoundApprochButton.interactable = true;
            primsSecoundApprochButton.onClick.RemoveAllListeners();
            primsSecoundApprochButton.onClick.AddListener(() =>
            {
                primsSecoundApprochButton.interactable = false;
                cameraController.FocusOnMaze(mazeGenerator.xWidth, mazeGenerator.zLength);
                StartCoroutine(primsSecond.GenerateOrder());
            });

            primsSecoundReformTheMaze.interactable = true;
            primsSecoundReformTheMaze.onClick.RemoveAllListeners();
            primsSecoundReformTheMaze.onClick.AddListener(() =>
            {
                StartCoroutine(primsSecond.ReRunTheMaze());
            });

            primsFirstApprochButton.interactable = false;
            primsFirstReformTheMaze.interactable = false;
        }
    }

    private void SetFieldsReady()
    {
        // Getting refrances
        mazeGenerator = GetComponent<MazeGenerator>();
        gridSizeManager = GetComponent<GridSizeManager>();
        poolManager = GetComponent<MazeGridPoolManager>();
        primsFirst = GetComponent<PrimsFirstApproach>();
        primsSecond = GetComponent<PrimsSecondApproach>();

        // Disable all buttons at start until one approach is selected
        primsFirstApprochButton.interactable = false;
        primsFirstReformTheMaze.interactable = false;
        primsSecoundApprochButton.interactable = false;
        primsSecoundReformTheMaze.interactable = false;

        // Set slider range first
        xWidth.minValue = 10;
        xWidth.maxValue = 250;
        zLength.minValue = 10;
        zLength.maxValue = 250;

        // Set initial slider values based on MazeGenerator
        xWidth.value = mazeGenerator.xWidth;
        zLength.value = mazeGenerator.zLength;
        xText.text = mazeGenerator.xWidth.ToString();
        zText.text = mazeGenerator.zLength.ToString();

        // Register event listeners
        xWidth.onValueChanged.AddListener(OnXWidthSliderChanged);
        zLength.onValueChanged.AddListener(OnZLengthSliderChanged);


        // Setup for cubeSize
        cubeSizeSlider.minValue = 1;
        cubeSizeSlider.maxValue = 5;
        cubeSizeSlider.wholeNumbers = true;

        cubeSizeSlider.value = gridSizeManager.cubeSize;
        cubeSizeText.text = gridSizeManager.cubeSize.ToString();
        cubeSizeSlider.onValueChanged.AddListener(OnCubeSizeChanged);

        // Setup for spacing
        spacingSlider.minValue = 0f;
        spacingSlider.maxValue = 0.5f;
        spacingSlider.value = gridSizeManager.spacing;
        spacingText.text = gridSizeManager.spacing.ToString();
        spacingSlider.onValueChanged.AddListener(OnSpacingChanged);


        // Step Delay setup
        stepDelaySlider.minValue = 0f;
        stepDelaySlider.maxValue = 1f;
        stepDelaySlider.value = primsFirst.stepDelay;
        stepDelaySlider.onValueChanged.AddListener(OnStepDelayChanged);
        stepDelayText.text = primsFirst.stepDelay.ToString("F2");

        // Step Size setup
        stepSizeSlider.minValue = 2;
        stepSizeSlider.maxValue = 6;
        stepSizeSlider.wholeNumbers = true;
        stepSizeSlider.value = primsFirst.stepSize;
        stepSizeSlider.onValueChanged.AddListener(OnStepSizeChanged);
        stepSizeText.text = primsFirst.stepSize.ToString();

        // Add listeners for toggles
        primsFirstApproch.onValueChanged.AddListener(OnFirstApproachToggled);
        primsSecoundApproch.onValueChanged.AddListener(OnSecondApproachToggled);
    }


    public void OnViewButtonClicked()
    {
        currentViewIndex++;
        cameraController.SetViewMode(currentViewIndex);
        cameraController.FocusOnMaze(mazeGenerator.xWidth, mazeGenerator.zLength);
    }


    public void ResetUI()
    {
        // Reset toggles
        primsFirstApproch.interactable = true;
        primsFirstApproch.isOn = false;

        primsSecoundApproch.interactable = true;
        primsSecoundApproch.isOn = false;

        // Reset buttons
        primsFirstApprochButton.interactable = false;
        primsFirstApprochButton.onClick.RemoveAllListeners();

        primsFirstReformTheMaze.interactable = false;
        primsFirstReformTheMaze.onClick.RemoveAllListeners();

        primsSecoundApprochButton.interactable = false;
        primsSecoundApprochButton.onClick.RemoveAllListeners();

        primsSecoundReformTheMaze.interactable = false;
        primsSecoundReformTheMaze.onClick.RemoveAllListeners();
    }


}
