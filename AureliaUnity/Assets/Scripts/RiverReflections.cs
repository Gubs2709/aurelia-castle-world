using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Aurelia
{
    [ExecuteAlways]
    public class RiverReflections:MonoBehaviour
    {
        public float waterHeight=1.65f;
        public int resolution=768;
        Camera reflected;RenderTexture texture;bool rendering;
        void OnEnable(){RenderPipelineManager.beginCameraRendering+=Render;}
        void OnDisable(){RenderPipelineManager.beginCameraRendering-=Render;if(reflected){if(Application.isPlaying)Destroy(reflected.gameObject);else DestroyImmediate(reflected.gameObject);}if(texture){texture.Release();if(Application.isPlaying)Destroy(texture);else DestroyImmediate(texture);}}
        void Render(ScriptableRenderContext context,Camera source)
        {
            if(rendering||source!=Camera.main||source.cameraType!=CameraType.Game)return;
            if(!texture){texture=new RenderTexture(resolution,resolution,16,RenderTextureFormat.DefaultHDR){name="River planar reflection",useMipMap=false};texture.Create();}
            if(!reflected){var g=new GameObject("River reflection camera"){hideFlags=HideFlags.HideAndDontSave};reflected=g.AddComponent<Camera>();reflected.enabled=false;var data=reflected.GetUniversalAdditionalCameraData();data.renderShadows=false;data.renderPostProcessing=false;data.requiresColorOption=CameraOverrideOption.Off;data.requiresDepthOption=CameraOverrideOption.Off;}
            rendering=true;
            try
            {
                reflected.CopyFrom(source);reflected.enabled=false;reflected.targetTexture=texture;reflected.cullingMask=source.cullingMask&~(1<<4);reflected.depth=-10;reflected.useOcclusionCulling=false;
                Matrix4x4 mirror=Matrix4x4.identity;mirror.m11=-1;mirror.m13=waterHeight*2;
                reflected.worldToCameraMatrix=source.worldToCameraMatrix*mirror;var p=source.transform.position;p.y=waterHeight*2-p.y;reflected.transform.position=p;
                Vector3 point=reflected.worldToCameraMatrix.MultiplyPoint(new Vector3(0,waterHeight+.07f,0));Vector3 normal=reflected.worldToCameraMatrix.MultiplyVector(Vector3.up).normalized;
                reflected.projectionMatrix=source.CalculateObliqueMatrix(new Vector4(normal.x,normal.y,normal.z,-Vector3.Dot(point,normal)));
                GL.invertCulling=true;
                // Called inside this frame's render context; reflection camera is excluded above.
                #pragma warning disable CS0618
                UniversalRenderPipeline.RenderSingleCamera(context,reflected);
                #pragma warning restore CS0618
                Shader.SetGlobalTexture("_RiverReflection",texture);
            }
            finally{GL.invertCulling=false;rendering=false;}
        }
    }
}
