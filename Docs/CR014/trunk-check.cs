var woods=UnityEngine.GameObject.Find("Woods replacing later subdivisions");
var expected=new System.Collections.Generic.HashSet<string>();var actual=new System.Collections.Generic.HashSet<string>();int vertices=0;
System.Func<UnityEngine.Vector3,string> key=p=>$"{p.x:F3},{p.y:F3},{p.z:F3}";
foreach(var b in woods.GetComponentsInChildren<UnityEngine.BoxCollider>())foreach(float x in new[]{-.5f,.5f})foreach(float y in new[]{-.5f,.5f})foreach(float z in new[]{-.5f,.5f})expected.Add(key(b.transform.TransformPoint(b.center+UnityEngine.Vector3.Scale(b.size,new UnityEngine.Vector3(x,y,z)))));
foreach(var f in woods.GetComponentsInChildren<UnityEngine.MeshFilter>()){var v=f.sharedMesh.vertices;var colors=f.sharedMesh.colors;for(int i=0;i<v.Length;i++)if(UnityEngine.Mathf.Abs(colors[i].r-.25f)<.0001f&&UnityEngine.Mathf.Abs(colors[i].g-.20f)<.0001f){actual.Add(key(f.transform.TransformPoint(v[i])));vertices++;}}
int missing=expected.Except(actual).Count(),extra=actual.Except(expected).Count();
System.IO.File.WriteAllText("Docs/CR014/trunk-alignment.txt",$"{(missing==0&&extra==0?"PASS":"FAIL")} baked bark vertices versus authored collider corners, 1mm rounding: expected unique corners {expected.Count}, actual {actual.Count}; missing {missing}, extra {extra}; bark vertices {vertices}. Leaves have no colliders.\n");
return new {missing,extra,vertices};
