using UnityEngine;

namespace Aurelia
{
    // Procedural walk, run, idle and jump poses for the Blender-authored knight (rigid parts on a pivot hierarchy).
    public class KnightAnimator:MonoBehaviour
    {
        Transform hips,spine,chest,head,sL,sR,eL,eR,hL,hR,kL,kR,cape,root;
        CharacterController body;Vector3 last;float phase,speed,idle,airborne;
        static Transform Find(Transform r,string n){foreach(var t in r.GetComponentsInChildren<Transform>(true))if(t.name==n)return t;return null;}
        void Awake()
        {
            root=transform;hips=Find(root,"Hips");spine=Find(root,"Spine");chest=Find(root,"Chest");head=Find(root,"Head");sL=Find(root,"ShoulderL");sR=Find(root,"ShoulderR");eL=Find(root,"ElbowL");eR=Find(root,"ElbowR");
            hL=Find(root,"HipL");hR=Find(root,"HipR");kL=Find(root,"KneeL");kR=Find(root,"KneeR");cape=Find(root,"Cape");body=GetComponentInParent<CharacterController>();last=root.position;
        }
        void LateUpdate()
        {
            float dt=Mathf.Max(.0001f,Time.deltaTime);Vector3 p=root.position;Vector3 d=(p-last)/dt;last=p;float horiz=new Vector2(d.x,d.z).magnitude;if(horiz>30)horiz=0;
            speed=Mathf.Lerp(speed,horiz,1-Mathf.Exp(-dt*12));idle+=dt;
            bool air=body&&!body.isGrounded&&Mathf.Abs(d.y)>1.2f;airborne=Mathf.MoveTowards(airborne,air?1:0,dt*6);
            float amp=Mathf.Clamp01(speed/4.5f);float run=Mathf.Clamp01((speed-5.5f)/5f);
            phase+=dt*(speed*1.15f+.01f)*(1.1f+run*.15f);float s=Mathf.Sin(phase),c=Mathf.Cos(phase);
            float legSwing=(34+run*22)*amp,armSwing=(30+run*28)*amp,knee=(26+run*40)*amp;
            float breathe=Mathf.Sin(idle*1.7f)*(1-amp);
            Set(hips,Quaternion.Euler(0,s*5*amp,0),new Vector3(0,Mathf.Abs(c)*.035f*amp-.02f*airborne,0));
            Set(spine,Quaternion.Euler(amp*(5+run*10)-breathe*1.2f,-s*6*amp,s*2*amp),null);
            Set(chest,Quaternion.Euler(breathe*1.5f,0,0),null);
            Set(head,Quaternion.Euler(-amp*3+breathe*1.2f+Mathf.Sin(idle*.6f)*1.5f,Mathf.Sin(idle*.45f)*(1-amp)*8,0),null);
            Set(hL,Quaternion.Euler(Mathf.Lerp(s*legSwing,-35,airborne),0,3),null);Set(hR,Quaternion.Euler(Mathf.Lerp(-s*legSwing,20,airborne),0,-3),null);
            Set(kL,Quaternion.Euler(Mathf.Lerp(Mathf.Max(0,-c-s*.0f)*knee*(s>0?.4f:1),50,airborne),0,0),null);Set(kR,Quaternion.Euler(Mathf.Lerp(Mathf.Max(0,c)*knee*(s<0?.4f:1),20,airborne),0,0),null);
            // swords arm opposite to the leg
            Set(sL,Quaternion.Euler(Mathf.Lerp(-s*armSwing,-150,airborne)+breathe*1.5f,0,-6-amp*3),null);Set(sR,Quaternion.Euler(Mathf.Lerp(s*armSwing,-150,airborne)-breathe*1.5f,0,6+amp*3),null);
            Set(eL,Quaternion.Euler(Mathf.Lerp(-18-Mathf.Max(0,s)*30*amp-run*30*amp,-30,airborne),0,0),null);Set(eR,Quaternion.Euler(Mathf.Lerp(-18-Mathf.Max(0,-s)*30*amp-run*30*amp,-30,airborne),0,0),null);
            if(cape)cape.localRotation=Quaternion.Euler(8f+amp*(14f+run*22f)+airborne*18f+Mathf.Sin(idle*2.1f+phase)*3f*(.4f+amp),Mathf.Sin(idle*1.3f)*2f,Mathf.Sin(idle*1.1f)*(1.5f+amp*3f));
        }
        readonly System.Collections.Generic.Dictionary<Transform,Vector3> basePos=new System.Collections.Generic.Dictionary<Transform,Vector3>();
        void Set(Transform t,Quaternion q,Vector3? offset)
        {
            if(!t)return;t.localRotation=q;if(offset.HasValue){if(!basePos.ContainsKey(t))basePos[t]=t.localPosition;t.localPosition=basePos[t]+offset.Value;}
        }
    }
}
