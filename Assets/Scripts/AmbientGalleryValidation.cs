using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace Racer
{
    public sealed class AmbientGalleryValidation:MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var a=Environment.GetCommandLineArgs();if(a.Contains("-ambientGallery")&&a.Contains("-racerTestSave"))new GameObject("Traffic art evidence").AddComponent<AmbientGalleryValidation>();}
        IEnumerator Start()
        {
            yield return null;Application.runInBackground=true;
            var root=new GameObject("Traffic art gallery");root.transform.position=new(10000,10000,10000);
            for(int i=0;i<4;i++)
            {
                var go=new GameObject(AmbientVehicle.BodyNames[i]);go.transform.SetParent(root.transform,false);go.transform.localPosition=new((i%2)*6,0,(i/2)*7);go.transform.localRotation=Quaternion.Euler(0,155,0);
                var art=go.AddComponent<AmbientVehicle>();art.Initialize();var select=typeof(AmbientVehicle).GetMethod("Select",BindingFlags.NonPublic|BindingFlags.Instance);
                for(int n=0;n<64&&art.BodyType!=i;n++)select.Invoke(art,null);
                var label=new GameObject("Label").AddComponent<TextMesh>();label.transform.SetParent(root.transform,false);label.transform.localPosition=go.transform.localPosition+new Vector3(0,-.2f,-3);label.transform.localRotation=Quaternion.Euler(45,0,0);label.text=AmbientVehicle.BodyNames[i];label.anchor=TextAnchor.MiddleCenter;label.fontSize=48;label.characterSize=.15f;label.color=Color.white;
            }
            var camera=new GameObject("Gallery camera").AddComponent<Camera>();camera.transform.position=root.transform.position+new Vector3(3,14,-13);camera.transform.LookAt(root.transform.position+new Vector3(3,0,3));camera.orthographic=true;camera.orthographicSize=8;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new(.055f,.09f,.12f);camera.farClipPlane=50;
            yield return null;
            var rt=new RenderTexture(1400,1000,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(1400,1000,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1400,1000),0,0);texture.Apply();
            Directory.CreateDirectory("Docs/CR050-053/traffic");File.WriteAllBytes("Docs/CR050-053/traffic/body-gallery.png",texture.EncodeToPNG());RenderTexture.active=previous;
            Application.Quit();
        }
    }
}
