#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.94 targeted checks, added to the 0.80 runner (muted; the editor starts on a copy of Dan's save).
//  landshot94:Scene:branch:label:s1,s2,...   shots along a branch from the given stations (eye 2 m above the ground, looking
//                                            along the branch, the HUD hidden)
public sealed partial class Report080Checks {
 IEnumerator Run094(string[] a)=>a[0] switch{"landshot94"=>LandShot094(a[1],a[2],a[3],a[4]),_=>Walk094(a)??Run093(a)};
 IEnumerator LandShot094(string scene,string title,string label,string stations){
  yield return StartOn093(scene,"moto");var wr=Branch093(title);wr.Initialize();
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;yield return new WaitForSeconds(1);
  foreach(var spec in stations.Split(',')){var sh=spec.Split('@');float s0=F(sh[0]),up=sh.Length>1?F(sh[1]):2;var eye=wr.At(s0,out var f);f.y=0;float ground=Physics.Raycast(eye+Vector3.up*5,Vector3.down,out var h,20,~0,QueryTriggerInteraction.Ignore)?h.point.y:eye.y;eye.y=ground+up;
   yield return Late(()=>{cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(f.normalized+Vector3.down*(up>3?.35f:.08f)));Shot($"{label}-from-s{s0:F0}{(up!=2?"-high":"")}");});}
  foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;yield return Menu();}
}
}
#endif
