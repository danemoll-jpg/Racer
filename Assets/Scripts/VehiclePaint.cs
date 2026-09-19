using UnityEngine;

namespace Racer
{
    public static class VehiclePaint
    {
        // Validation captures every unpainted renderer, including water LineRenderers.
        // Unity owns a nonempty block on those even before paint. Test invariance,
        // rather than incorrectly treating an engine-owned block as paint leakage.
        public static System.Collections.Generic.Dictionary<Renderer,string> UnpaintedSnapshot(Transform root)
        {
            var result=new System.Collections.Generic.Dictionary<Renderer,string>();
            foreach(var r in root.GetComponentsInChildren<Renderer>())
            {
                var m=r.sharedMaterial;if(!m||IsBodyPaint(m))continue;
                var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);
                var text=new System.Text.StringBuilder(m.GetEntityId()+"/"+m.color+"/"+b.isEmpty);
                var shader=m.shader;
                for(int i=0;i<shader.GetPropertyCount();i++)
                {
                    int id=shader.GetPropertyNameId(i);text.Append('|').Append(id).Append(':').Append(b.HasProperty(id));
                    switch(shader.GetPropertyType(i))
                    {
                        case UnityEngine.Rendering.ShaderPropertyType.Color: text.Append(b.GetColor(id));break;
                        case UnityEngine.Rendering.ShaderPropertyType.Vector: text.Append(b.GetVector(id));break;
                        case UnityEngine.Rendering.ShaderPropertyType.Texture: text.Append(b.GetTexture(id)?.GetEntityId().ToString()??"none");break;
                        default: text.Append(b.GetFloat(id).ToString("R",System.Globalization.CultureInfo.InvariantCulture));break;
                    }
                }
                text.Append(b.GetColor("_BaseColor")).Append(b.GetColor("_Color")).Append(b.GetFloat("_Smoothness")).Append(b.GetFloat("_Metallic"));
                result.Add(r,text.ToString());
            }
            return result;
        }
        public static bool UnpaintedUnchanged(Transform root,System.Collections.Generic.Dictionary<Renderer,string> before)
        {
            var after=UnpaintedSnapshot(root);if(before.Count!=after.Count)return false;
            foreach(var pair in before)if(!after.TryGetValue(pair.Key,out var value)||value!=pair.Value)return false;
            return true;
        }
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
