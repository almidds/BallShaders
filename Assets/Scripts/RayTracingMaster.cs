using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{

    [SerializeField]
    private ComputeShader rayTracingShader;

    private RenderTexture _target;

    private Camera _camera;

    [SerializeField]
    private Texture skyboxTexture;

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        SetShaderParameters();
        Render(dest);
    }

    private void Render(RenderTexture dest){
        InitRenderTexture();

        rayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(_target, dest);
    }

    private void InitRenderTexture(){
        if(_target == null || _target.width != Screen.width || _target.height != Screen.height){
            if(_target != null){
                _target.Release();
            }

            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }
    // Start is called before the first frame update
    private void Awake(){
        _camera = GetComponent<Camera>();
    }

    private void SetShaderParameters(){
        rayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        rayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
    }
}
