 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 System.IO.Directory.CreateDirectory("Docs/McFaddenEntrance");UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");Physics.SyncTransforms();
 var at=new Vector3(511.6f,81,-139.5f);var sign=GameObject.Find("House 3 / Rocky Way Acres entrance").transform;
 var rows=UnityEngine.Object.FindObjectsByType<MeshFilter>().Where(m=>m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(new Bounds(at,new Vector3(55,20,50)))).Select(m=>m.name+" | "+AssetDatabase.GetAssetPath(m.sharedMesh)+" | "+m.GetComponent<Renderer>().bounds).ToList();
 var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();float station=street.Project(at,out float dist);rows.Add("REFERENCE "+at+" main="+street.At(station,out var f)+" forward="+f+" width="+street.HalfWidth(station)+" distance="+dist);
 var drive=GameObject.Find("House 3 valley driveway").GetComponent<Racer.RaceRoad>();rows.Add("DRIVE "+string.Join(";",drive.points.Take(8)));rows.Add("SIGN "+sign.position+" forward="+sign.forward);
 System.IO.File.WriteAllLines("Docs/McFaddenEntrance/after.txt",rows);
 Shot("after-overhead",at+new Vector3(0,65,0),at,Vector3.forward);Shot("after-road",at+new Vector3(15,7,23),at+new Vector3(-8,0,0),Vector3.up);
 return string.Join("\n",rows);
 void Shot(string name,Vector3 position,Vector3 target,Vector3 up){var go=new GameObject("Entrance inspection camera");var c=go.AddComponent<Camera>();c.CopyFrom(Camera.main);c.enabled=false;c.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position,up));var rt=new RenderTexture(1200,800,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1200,800,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1200,800),0,0);tex.Apply();System.IO.File.WriteAllBytes("Docs/McFaddenEntrance/"+name+".png",tex.EncodeToPNG());RenderTexture.active=old;c.targetTexture=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);}


