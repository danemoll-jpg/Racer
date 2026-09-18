using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // Original procedural low-poly art. No imported assets or license dependencies.
    public static class VehicleVisual
    {
        static Material paint, rubber, glass, rider, metal;
        static Material Mat(string name, Color color) => new(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};
        public static Transform Build(Transform parent,VehicleProfile p,List<Transform> wheels=null)
        {
            if(!paint) { paint=Mat("Garage Car paint",new(.15f,.62f,.64f)); rubber=Mat("Garage rubber",new(.045f,.05f,.065f)); glass=Mat("Garage glass",new(.13f,.23f,.3f)); rider=Mat("Garage rider",new(.85f,.46f,.17f)); metal=Mat("Garage metal",new(.65f,.72f,.74f)); }
            var root=new GameObject("Vehicle visual").transform; root.SetParent(parent,false); root.gameObject.layer=parent.gameObject.layer;
            if(!p.Small)
            {
                Part(root,"Body",Vector3.zero,p.Size,paint);
                Part(root,"Longroof cabin",new(0,.65f,-.4f),new(1.65f,.7f,p.Id=="tourer"?2.7f:1.8f),glass);
                Part(root,"Roof",new(0,1.02f,-.4f),new(1.7f,.1f,p.Id=="tourer"?2.8f:1.9f),paint);
                Part(root,"Front lamps",new(0,.1f,p.Size.z*.5f),new(1.6f,.15f,.04f),metal);
            }
            else
            {
                Part(root,"Frame",new(0,0,0),new(p.Id=="moto"?.36f:1,.42f,1.75f),paint);
                Part(root,"Seat",new(0,.32f,-.3f),new(.5f,.18f,.85f),rubber);
                Part(root,"Tank",new(0,.35f,.38f),new(.48f,.35f,.65f),paint);
                Part(root,"Handlebars",new(0,.7f,.65f),new(.95f,.09f,.09f),metal);
                Part(root,"Rider torso",new(0,.85f,-.18f),new(.52f,.65f,.32f),rider);
                Part(root,"Helmet",new(0,1.35f,-.04f),new(.4f,.4f,.42f),rubber,PrimitiveType.Sphere);
                foreach(float side in new[]{-1f,1f})
                {
                    Part(root,"Rider leg",new(side*.29f,.3f,-.25f),new(.16f,.6f,.24f),rubber);
                    var arm=Part(root,"Rider arm",new(side*.3f,.87f,.23f),new(.14f,.15f,.75f),rider); arm.localRotation=Quaternion.Euler(15,0,side*8);
                }
                if(p.Id=="atv") foreach(float z in new[]{-.85f,.85f}) Part(root,"Fenders",new(0,.17f,z),new(1.5f,.16f,.52f),paint);
            }
            float[] tracks=p.Id=="moto"?new[]{0f}:new[]{-p.Size.x*.5f,p.Size.x*.5f};
            foreach(float x in tracks) foreach(float z in new[]{-p.Wheelbase*.5f,p.Wheelbase*.5f})
            {
                var wheel=Part(root,"Wheel",new(x,-.2f,z),new(.66f,p.Id=="moto"?.1f:.18f,.66f),rubber,PrimitiveType.Cylinder);
                wheel.localRotation=Quaternion.Euler(0,0,90); wheels?.Add(wheel);
                Part(wheel,"Spoke",new(0,1.01f,0),new(.8f,.025f,.09f),metal);
            }
            return root;
        }
        static Transform Part(Transform parent,string name,Vector3 position,Vector3 size,Material material,PrimitiveType shape=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(shape); go.name=name; go.layer=parent.gameObject.layer;
            var collider=go.GetComponent<Collider>(); collider.enabled=false; Object.Destroy(collider);
            go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=material; return go.transform;
        }
    }
}
