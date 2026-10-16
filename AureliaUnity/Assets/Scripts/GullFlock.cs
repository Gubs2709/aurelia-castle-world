using UnityEngine;

namespace Aurelia
{
    // Soaring white gulls circling over a point, built from the gull sprite on crossed quads that flap.
    public class GullFlock:MonoBehaviour
    {
        public Material material;public int count=14;public Vector3 centre;public float radius=40,height=22;
        class Bird{public Transform t;public float a,r,h,sp,ph;}
        Bird[] birds;
        void Start()
        {
            var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.8f,0,-.5f),new Vector3(.8f,0,-.5f),new Vector3(.8f,0,.5f),new Vector3(-.8f,0,.5f)};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};mesh.triangles=new[]{0,2,1,0,3,2,0,1,2,0,2,3};mesh.RecalculateNormals();
            birds=new Bird[count];
            for(int i=0;i<count;i++)
            {
                var g=new GameObject("Gull");g.transform.SetParent(transform,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;var mr=g.AddComponent<MeshRenderer>();mr.sharedMaterial=material;mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                g.transform.localScale=Vector3.one*Random.Range(1.4f,2.4f);
                birds[i]=new Bird{t=g.transform,a=Random.value*6.28f,r=radius*Random.Range(.4f,1.1f),h=height+Random.Range(-6f,10f),sp=Random.Range(.12f,.24f)*(Random.value<.5f?1:-1),ph=Random.value*6};
            }
        }
        void Update()
        {
            foreach(var b in birds)
            {
                b.a+=b.sp*Time.deltaTime;var p=centre+new Vector3(Mathf.Cos(b.a)*b.r,b.h+Mathf.Sin(Time.time*.6f+b.ph)*1.5f,Mathf.Sin(b.a)*b.r);
                var d=(p-b.t.position);if(d.sqrMagnitude>.0001f)b.t.rotation=Quaternion.Slerp(b.t.rotation,Quaternion.LookRotation(d.normalized)*Quaternion.Euler(0,0,Mathf.Sin(Time.time*6+b.ph)*18),.2f);
                b.t.position=p;
                var s=b.t.localScale;float flap=1f-.4f*Mathf.Abs(Mathf.Sin(Time.time*5.5f+b.ph));b.t.localScale=new Vector3(Mathf.Sign(s.x)*Mathf.Abs(s.z)*flap*1.0f+0,s.y,s.z);
            }
        }
    }
}
