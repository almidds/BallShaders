using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{

    [SerializeField]
    private ComputeShader rayTracingShader;

    private RenderTexture _target;

    private Camera _camera;

    [SerializeField]
    private Texture skyboxTexture;

    // Anti-aliasing
    private uint _currentSample = 0;
    private Material _addMaterial;

    [SerializeField]
    Light directionalLight;

    private List<Transform> _transformsToWatch = new List<Transform>();

    struct Sphere{
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    };

    [SerializeField]
    private Vector2 SphereRadius = new Vector2(3.0f, 8.0f);

    [SerializeField]
    private uint SpheresMax = 100;

    [SerializeField]
    private float SpheresPlacementRadius = 100.0f;
    
    private ComputeBuffer _sphereBuffer;

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        SetShaderParameters();
        Render(dest);
    }

    private void Render(RenderTexture dest){
        // Compute Shader
        InitRenderTexture();

        rayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Anti-aliasing shader
        if(_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);

        Graphics.Blit(_target, dest, _addMaterial);
        _currentSample++;
    }

    private void InitRenderTexture(){
        if(_target == null || _target.width != Screen.width || _target.height != Screen.height){
            if(_target != null){
                _target.Release();
            }

            _target = new RenderTexture(Screen.width,
                                        Screen.height,
                                        0,
                                        RenderTextureFormat.ARGBFloat,
                                        RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }



    // Start is called before the first frame update
    private void Awake(){
        _camera = GetComponent<Camera>();

        _transformsToWatch.Add(transform);
        _transformsToWatch.Add(directionalLight.transform);
    }

    private void Update(){
        foreach(Transform t in _transformsToWatch){
            if(t.hasChanged){
                _currentSample = 0;
                t.hasChanged = false;
            }
        }
    }

    private void OnEnable(){
        _currentSample = 0;
        SetUpScene();
    }

    private void OnDisable(){
        if(_sphereBuffer != null){
            _sphereBuffer.Release();
        }
    }

    private void SetUpScene(){
        List<Sphere> spheres = new List<Sphere>();

        for(int i = 0; i < SpheresMax; i++){
            Sphere sphere = new Sphere();
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpheresPlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            foreach(Sphere other in spheres){
                float minDist = sphere.radius + other.radius;
                if(Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist){
                    goto SkipSphere;
                }
            }

            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;

            spheres.Add(sphere);

            SkipSphere:
                continue;
        }

        _sphereBuffer = new ComputeBuffer(spheres.Count, 40);
        _sphereBuffer.SetData(spheres);
    }

    private void SetShaderParameters(){
        rayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        rayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
        rayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));

        Vector3 sceneLight = directionalLight.transform.forward;
        rayTracingShader.SetVector("_DirectionalLight", new Vector4(sceneLight.x,
                                                                    sceneLight.y,
                                                                    sceneLight.z,
                                                                    directionalLight.intensity));

        rayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
    }
}
