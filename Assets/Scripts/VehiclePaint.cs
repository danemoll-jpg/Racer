using UnityEngine;

namespace Racer
{
    public static class VehiclePaint
    {
        public static readonly string[] Names={"Teal","Red","Gold","Blue","White","Violet","Black"};
        public static readonly Color[] Colors={new(.12f,.64f,.65f),new(.9f,.13f,.09f),new(.96f,.67f,.08f),new(.12f,.36f,.95f),new(.88f,.9f,.92f),new(.6f,.19f,.8f),new(.018f,.021f,.025f)};
        public static bool IsBodyPaint(Material material) => material && material.name.IndexOf("paint",System.StringComparison.OrdinalIgnoreCase)>=0;
        public static void Apply(Transform root,Color color)
        {
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if(!IsBodyPaint(renderer.sharedMaterial)) continue;
                var block=new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor",color); block.SetColor("_Color",color);
                block.SetFloat("_Smoothness",.55f); block.SetFloat("_Metallic",.22f); renderer.SetPropertyBlock(block);
            }
        }
    }
}
