using UnityEngine;

namespace Racer
{
    public static class VehiclePaint
    {
        public static readonly string[] Names={"Teal","Red","Gold","Blue","White","Violet"};
        public static readonly Color[] Colors={new(.12f,.64f,.65f),new(.9f,.13f,.09f),new(.96f,.67f,.08f),new(.12f,.36f,.95f),new(.88f,.9f,.92f),new(.6f,.19f,.8f)};
        public static void Apply(Transform root,Color color)
        {
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if(!renderer.sharedMaterial || !renderer.sharedMaterial.name.Contains("Car")) continue;
                var block=new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor",color); block.SetColor("_Color",color); renderer.SetPropertyBlock(block);
            }
        }
    }
}
