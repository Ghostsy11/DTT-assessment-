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

    // Internal data for the job
    private TransformAccessArray _transforms;
    private NativeArray<int> _xs;             // X from Vector2Int
    private NativeArray<float> _zs;           // Z from Vector2Int
    private bool initialized;


    private void Awake()
    {
        generator = GetComponent<MazeGenerator>();
        firstApproach = GetComponent<PrimsFirstApproach>();
        secondApproach = GetComponent<PrimsSecondApproach>();
        primsHelper = GetComponent<PrimsHelperMethods>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ApplyResize();
            //secondApproach.PullDownPath(1);
        }
    }

    /// <summary>
    /// Call this to resize/reposition all maze cubes to the current cubeSize & spacing.
    /// </summary>
    public void ApplyResize()
    {
        if (!initialized)
            InitializeTransforms();

        var job = new ResizeJob
        {
            cubeSize = cubeSize,
            spacing = spacing,
            xs = _xs,
            zs = _zs
        };

        JobHandle handle = job.Schedule(_transforms);
        handle.Complete();

        //  Safely animate based on which approach is active
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
    /// Builds the TransformAccessArray and X/Z arrays from the generator's dictionary.
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
            _xs[i] = kvp.Key.x;    // X component of Vector2Int
            _zs[i] = kvp.Key.y;    // Y component is used as Z
            i++;
        }

        _transforms = new TransformAccessArray(transforms);
        initialized = true;
    }

    private void OnDestroy()
    {
        if (initialized)
        {
            _transforms.Dispose();
            _xs.Dispose();
            _zs.Dispose();
        }
    }

    [BurstCompile]
    private struct ResizeJob : IJobParallelForTransform
    {
        public float cubeSize;
        public float spacing;

        [ReadOnly] public NativeArray<int> xs;
        [ReadOnly] public NativeArray<float> zs;

        public void Execute(int index, TransformAccess transform)
        {
            // Uniform scale
            transform.localScale = Vector3.one * cubeSize;

            // Calculate position with spacing
            float offset = cubeSize + spacing;
            float xPos = xs[index] * offset;
            float zPos = zs[index] * offset;
            transform.position = new Vector3(xPos, 0f, zPos);
        }
    }
}
