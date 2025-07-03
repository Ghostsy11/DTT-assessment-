using UnityEngine;

public class ExcationMazeOrder : MonoBehaviour
{

    [SerializeField] MazeDictMeshRenderer meshRenderer;
    [SerializeField] GridSizeManager gridSizeManager;
    [SerializeField] MazeGenerator mazeGenerator;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void RenderWholeMaze()
    {
        meshRenderer.RenderCubesGradually();
    }

    public void UnrenderWholeMaze()
    {
        meshRenderer.UnrenderCubesGradually();
    }

    public void ResizeTheGrid()
    {
        gridSizeManager.ApplyResize();
    }

    public void SetTypeGenerateMazeFast()
    {
        mazeGenerator.generateType = MazeGenerator.GenerateType.GenerateOnce;
    }

    public void SetTypeGenerateMazeGradually()
    {
        mazeGenerator.generateType = MazeGenerator.GenerateType.GenerateBatched;
    }

    public void SetTypeGenerateMazePreInstantiated()
    {
        mazeGenerator.generateType = MazeGenerator.GenerateType.PreInstantiated;
    }

    public void GenetaingMazeBasedOnType()
    {
        mazeGenerator.Generate();
    }

}
