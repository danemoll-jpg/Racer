using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Racer.Editor {
public sealed class ForwardJsonResolver:DefaultContractResolver {
protected override IList<JsonProperty> CreateProperties(System.Type type,MemberSerialization serialization) {
var properties=base.CreateProperties(type,serialization);
if(type==typeof(UnityEngine.Vector3))return properties.Where(p=>p.PropertyName=="x"||p.PropertyName=="y"||p.PropertyName=="z").ToList();
if(type==typeof(UnityEngine.Bounds))return properties.Where(p=>p.PropertyName=="center"||p.PropertyName=="size").ToList();
return properties;
}
}}
