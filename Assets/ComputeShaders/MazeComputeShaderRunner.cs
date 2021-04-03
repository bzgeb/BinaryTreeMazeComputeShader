using UnityEngine;

public class MazeComputeShaderRunner : MonoBehaviour
{
    [SerializeField] ComputeShader _computeShader;

    [SerializeField] RenderTexture _renderTexture;
    [SerializeField] int _size;
    [SerializeField] Color _wallColor = Color.black;
    [SerializeField] Color _emptyColor = Color.red;
    Color _cachedWallColor;
    Color _cachedEmptyColor;
    [SerializeField, Range(0, 1000)] int _seed;
    int _cachedSeed;

    void Start()
    {
        _renderTexture = new RenderTexture(_size, _size, 24);
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();

        GenerateMaze();
    }

    void Update()
    {
        if (_cachedWallColor != _wallColor || _cachedSeed != _seed || _cachedEmptyColor != _emptyColor)
            GenerateMaze();
    }

    void GenerateMaze()
    {
        _computeShader.SetInt("_Resolution", _renderTexture.width);
        _computeShader.SetInt("_Seed", _seed);
        _computeShader.SetVector("_WallColor", _wallColor);
        _computeShader.SetVector("_EmptyColor", _emptyColor);

        var prepass = _computeShader.FindKernel("Prepass");
        _computeShader.SetTexture(prepass, "Result", _renderTexture);
        _computeShader.Dispatch(prepass, _renderTexture.width / 8, _renderTexture.height / 8, 1);

        var main = _computeShader.FindKernel("CSMain");
        _computeShader.SetTexture(main, "Result", _renderTexture);
        _computeShader.Dispatch(main, _renderTexture.width / 8, _renderTexture.height / 8, 1);

        _cachedSeed = _seed;
        _cachedWallColor = _wallColor;
        _cachedEmptyColor = _emptyColor;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest);
    }
}