using UnityEngine;
using UnityEngine.InputSystem;

namespace Aurelia
{
    // 2D billboard knight from a 5x3 side-view sprite sheet (13 walk frames). Idle holds a standing frame with a breathing bob;
    // attack (left mouse while the cursor is locked, or E) lunges the sprite and sweeps a slash arc.
    public class SpriteKnight:MonoBehaviour
    {
        public Material material,slashMaterial;
        public int cols=5,rows=3,frames=13,idleFrame=5;
        public float height=2.23f,footPad=.15f;
        Transform quad,slash;Material mat,slashMat;Vector3 last;float speed,anim,idle,attackT=-1;bool faceRight=true;Camera cam;
        void Start()
        {
            cam=Camera.main;last=transform.parent?transform.parent.position:transform.position;
            var g=new GameObject("Sprite");g.transform.SetParent(transform,false);g.layer=2;quad=g.transform;
            var mf=g.AddComponent<MeshFilter>();mf.sharedMesh=Quad();var mr=g.AddComponent<MeshRenderer>();mat=new Material(material);mr.sharedMaterial=mat;
            var s=new GameObject("Slash");s.transform.SetParent(transform,false);s.layer=2;slash=s.transform;var sm=s.AddComponent<MeshFilter>();sm.sharedMesh=Quad();var sr=s.AddComponent<MeshRenderer>();slashMat=new Material(slashMaterial);sr.sharedMaterial=slashMat;sr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;s.SetActive(false);
            SetFrame(idleFrame);
        }
        static Mesh Quad()
        {
            var m=new Mesh();m.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};m.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};m.triangles=new[]{0,2,1,0,3,2};m.normals=new[]{-Vector3.forward,-Vector3.forward,-Vector3.forward,-Vector3.forward};m.RecalculateBounds();return m;
        }
        void SetFrame(int f)
        {
            f=Mathf.Clamp(f,0,frames-1);int c=f%cols,r=f/cols;float w=1f/cols,h=1f/rows;mat.SetVector("_UV",new Vector4(c*w,1-(r+1)*h,w,h));
        }
        void LateUpdate()
        {
            if(!quad)return;if(!cam)cam=Camera.main;if(!cam)return;
            var root=transform.parent?transform.parent:transform;Vector3 delta=root.position-last;last=root.position;float dt=Mathf.Max(.0001f,Time.deltaTime);
            float sp=new Vector2(delta.x,delta.z).magnitude/dt;if(sp>40)sp=0;speed=Mathf.Lerp(speed,sp,1-Mathf.Exp(-dt*14));
            Vector3 away=transform.position-cam.transform.position;away.y=0;if(away.sqrMagnitude<.0001f)away=cam.transform.forward;
            transform.rotation=Quaternion.LookRotation(away.normalized);
            // direction on screen
            Vector3 camRight=cam.transform.right;camRight.y=0;camRight.Normalize();float side=Vector3.Dot(new Vector3(delta.x,0,delta.z).normalized,camRight);
            if(speed>.6f&&Mathf.Abs(side)>.18f)faceRight=side>0;
            bool attackPressed=Cursor.lockState==CursorLockMode.Locked&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame||Keyboard.current!=null&&Keyboard.current[Key.E].wasPressedThisFrame;
            if(attackPressed&&attackT<0)attackT=0;
            float fx=faceRight?1:-1;float bob=0,lunge=0,tilt=0;
            if(attackT>=0)
            {
                attackT+=dt;float u=attackT/.5f;
                // wind-up (0..0.3), strike (0.3..0.6), recover
                float wind=Mathf.SmoothStep(0,1,Mathf.Clamp01(u/.3f)),strike=Mathf.SmoothStep(0,1,Mathf.Clamp01((u-.3f)/.25f)),rec=Mathf.SmoothStep(0,1,Mathf.Clamp01((u-.6f)/.4f));
                tilt=(-10*wind+28*strike)*(1-rec);lunge=(-.1f*wind+.55f*strike)*(1-rec);SetFrame(Mathf.RoundToInt(Mathf.Lerp(idleFrame,3,Mathf.Clamp01(wind-strike*.6f))));
                slash.gameObject.SetActive(u>.28f&&u<.8f);float su=Mathf.Clamp01((u-.28f)/.45f);slashMat.SetFloat("_Fade",Mathf.Sin(su*Mathf.PI)*.95f);
                slash.localPosition=new Vector3(fx*(.85f+su*.5f),1.0f,-.05f);slash.localScale=new Vector3(fx*2.6f,2.6f,1);slash.localRotation=Quaternion.Euler(0,0,fx*(70-su*130));
                if(u>=1){attackT=-1;slash.gameObject.SetActive(false);}
            }
            else if(speed>.5f)
            {
                anim+=dt*Mathf.Clamp(speed*2.7f,5f,24f);SetFrame((int)anim%frames);
                bob=Mathf.Abs(Mathf.Sin(anim*.5f))*.02f;
            }
            else
            {
                idle+=dt;SetFrame(idleFrame);bob=Mathf.Sin(idle*1.8f)*.012f;anim=0;
            }
            quad.localScale=new Vector3(fx*height,height*(1+(attackT<0&&speed<=.5f?Mathf.Sin(idle*1.8f)*.004f:0)),1);
            quad.localPosition=new Vector3(fx*lunge,height*.5f-footPad+bob,0);
            quad.localRotation=Quaternion.Euler(0,0,-fx*tilt);
        }
    }
}
