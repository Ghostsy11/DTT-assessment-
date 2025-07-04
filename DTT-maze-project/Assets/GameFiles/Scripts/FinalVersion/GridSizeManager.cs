using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

public class GridSizeManager : MonoBehaviour
{
    [Header("References (set automatically)")]
    [SerializeField] private MazeGenerator generator;

    [Header("Size Controls")]
    [Range(1, 5)] public int cubeSize = 1;
    [Range(0f, 0.5f)] public float spacing = 0.05f;

    private PrimsHelperMethods primsHelper;
    private PrimsFirstApproach firstApproach;
    private PrimsSecondApproach secondApproach;

    // Native job data
    private TransformAccessArray _transforms;         // Efficient access to transforms for parallel resizing
    private NativeArray<int> _xs;                     // Stores X positions (from Vector2Int)
    private NativeArray<float> _zs;                   // Stores Z positions (Y in Vector2Int is used as Z)
    private bool initialized;                         // Track if transform data has been built


    private void Awake()
    {
        generator = GetComponent<MazeGenerator>();
        firstApproach = GetComponent<PrimsFirstApproach>();
        secondApproach = GetComponent<PrimsSecondApproach>();
        primsHelper = GetComponent<PrimsHelperMethods>();
    }

    /// <summary>
    /// Resize and reposition all maze cubes using the current cubeSize & spacing.
    /// Runs a parallel job for performance.
    /// </summary>
    public void ApplyResize()
    {
        if (!initialized)
            InitializeTransforms(); // Setup NativeArrays and TransformAccessArray

        var job = new ResizeJob
        {
            cubeSize = cubeSize,
            spacing = spacing,
            xs = _xs,
            zs = _zs
        };

        JobHandle handle = job.Schedule(_transforms);
        handle.Complete(); // Wait until job is finished

        // Trigger cube animation (like dropping down) for whichever approach is active
        if (firstApproach != null && firstApproach.map != null)
        {
            primsHelper.PullDownPath(1, generator.cellsByLocation, firstApproach.map);
        }

        if (secondApproach != null && secondApproach.grid != null)
        {
            primsHelper.PullDownPath(1, generator.cellsByLocation, secondApproach.grid);
        }
    }

    /// <summary>
    /// Initializes transform data required for resizing job (runs once).
    /// </summary>
    private void InitializeTransforms()
    {
        int count = generator.cellsByLocation.Count;

        _xs = new NativeArray<int>(count, Allocator.Persistent);
        _zs = new NativeArray<float>(count, Allocator.Persistent);

        var transforms = new Transform[count];
        int i = 0;
        foreach (var kvp in generator.cellsByLocation)
        {
            transforms[i] = kvp.Value.transform;
            _xs[i] = kvp.Key.x;      // Store X from Vector2Int
            _zs[i] = kvp.Key.y;      // Store Z from Vector2Int.y
            i++;
        }

        _transforms = new TransformAccessArray(transforms);
        initialized = true;
    }

    /// <summary>
    /// Safely clean up job-related memory to avoid leaks.
    /// </summary>
    private void OnDestroy()
    {
        if (initialized)
        {
            _transforms.Dispose();
            _xs.Dispose();
            _zs.Dispose();
        }
    }

    /// <summary>
    /// Struct that defines the job logic to resize and reposition each transform in parallel.
    /// </summary>
    [BurstCompile]
    private struct ResizeJob : IJobParallelForTransform
    {
        public float cubeSize;   // Target size
        public float spacing;    // Target spacing

        [ReadOnly] public NativeArray<int> xs; // X positions
        [ReadOnly] public NativeArray<float> zs; // Z positions

        public void Execute(int index, TransformAccess transform)
        {
            // Scale the cube uniformly
            transform.localScale = Vector3.one * cubeSize;

            // Compute new world position with spacing applied
            float offset = cubeSize + spacing;
            float xPos = xs[index] * offset;
            float zPos = zs[index] * offset;

            transform.position = new Vector3(xPos, 0f, zPos);
        }
    }
}
