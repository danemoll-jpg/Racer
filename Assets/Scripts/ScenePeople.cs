using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.81 Part D: the people in the scripted scenes - the household vignettes (coffee at Dan's fence, the two at Kyle's,
    // football in Dan's yard), the campsite (two seated by the fire) and the Snow scenes (sled, broom hockey) - are three
    // recurring characters built from the 0.75 parametric rider: every two-person scene is Dan and Kyle, every
    // three-person scene Dan, Kyle and the brother. No likeness is intended; three plain, distinct looks.
    // 0.83 (Dan's description): Dan - light skin, brown hair, black T-shirt with a chest emblem (a plain white ring with a
    // simple generic bird; no lettering, no real logo), blue jeans; Kyle (0.81's "the friend") - light skin, black hair,
    // black leather jacket open over a white T-shirt, blue jeans; the brother - light skin, dark-brown hair in a longer cut
    // than Dan's, the 0.81 white T-shirt and shorts. No hats. Builds as in 0.81. Winter: Dan a black jacket with the
    // emblem on the left chest, Kyle the leather jacket, the brother violet; beanies, hair colour showing beneath.
    // THE LOOKS ARE HERE, in one place (RiderLook values: body 0 man / 1 woman; skin 0-5 very light..dark; hair 0 short,
    // 1 medium, 2 long, 3 ponytail, 4 bald; hairColor 0 black, 1 dark brown, 2 brown, 3 auburn, 4 red, 5 blonde, 6 grey,
    // 7 white; hat 0 none, 1 flat cap, 2 baseball cap, 3 beanie, 4 cowboy hat; shirt 0 T-shirt, 1 long sleeve, 2 jacket;
    // pants 0 jeans, 1 shorts; colours = the vehicle swatches 0 teal, 1 red, 2 gold, 3 blue, 4 white, 5 violet, 6 black;
    // 0.83: shirt 3 leather jacket, emblem 1 the chest emblem)
    // plus a build (height and width scale). Winter = the Snow scenes (coat and knitted hat); the same person keeps the
    // same body, skin, hair and build in every scene.
    public static class ScenePeople
    {
        public sealed class Character
        {
            public string Name; public RiderLook Everyday, Winter; public float Height = 1, Width = 1;
        }
        public static readonly Character Dan = new()
        {
            Name = "Dan", Height = 1, Width = 1,
            Everyday = new RiderLook { body = 0, skin = 1, hair = 0, hairColor = 2, hat = 0, hatColor = 6, shirt = 0, shirtColor = 6, pants = 0, pantsColor = 3, emblem = 1 },
            Winter = new RiderLook { body = 0, skin = 1, hair = 0, hairColor = 2, hat = 3, hatColor = 1, shirt = 2, shirtColor = 6, pants = 0, pantsColor = 3, emblem = 1 },
        };
        public static readonly Character Kyle = new()
        {
            Name = "Kyle", Height = 1.06f, Width = .96f,
            Everyday = new RiderLook { body = 0, skin = 0, hair = 1, hairColor = 0, hat = 0, hatColor = 2, shirt = 3, shirtColor = 6, pants = 0, pantsColor = 3 },
            Winter = new RiderLook { body = 0, skin = 0, hair = 1, hairColor = 0, hat = 3, hatColor = 0, shirt = 3, shirtColor = 6, pants = 0, pantsColor = 3 },
        };
        public static readonly Character Brother = new()
        {
            Name = "the brother", Height = .95f, Width = 1.1f,
            Everyday = new RiderLook { body = 0, skin = 1, hair = 1, hairColor = 1, hat = 0, hatColor = 1, shirt = 0, shirtColor = 4, pants = 1, pantsColor = 2 },
            Winter = new RiderLook { body = 0, skin = 1, hair = 1, hairColor = 1, hat = 3, hatColor = 5, shirt = 2, shirtColor = 5, pants = 0, pantsColor = 2 },
        };
        // two-person scenes: Dan and Kyle; three-person scenes add the brother
        public static Character Of(int index) => index % 3 == 0 ? Dan : index % 3 == 1 ? Kyle : Brother;

        // The campsite: the two "Seated guy" figures (scene objects, no colliders) keep their log seats; their old body
        // parts are hidden and Dan and Kyle sit in their place. Every scene with the camp.
        public static readonly List<string> Report = new();
        public static void DressCamp(Scene scene)
        {
            int k = 0;
            foreach (var guy in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).Where(t => t.name == "Seated guy").OrderBy(t => t.localPosition.x).ToArray())
            {
                if (guy.Find("Scene person")) continue;
                foreach (var r in guy.GetComponentsInChildren<Renderer>(true)) if (r.name != "Log seat") r.enabled = false;
                var who = Of(k++);
                var person = ScenePerson.Build(guy, who, false, "Sit");
                if (person) Report.Add($"camp: {who.Name} at {guy.position}");
            }
        }
    }

    // One person: the rider parts in one or two poses (the sled scene switches seated / standing), with root-space pivots
    // so the scenes' existing animation keeps working: Arm / OtherArm turn about the right / left shoulder like the old
    // figures' arms (Euler about the person's x), the elbows bend (SetElbow), the legs swing about the hips.
    public sealed class ScenePerson : MonoBehaviour
    {
        public sealed class Rig { public Transform Pose, Arm, OtherArm, ElbowR, ElbowL, Hand, OtherHand, LeftLeg, RightLeg; }
        public ScenePeople.Character Who;
        readonly List<Rig> rigs = new();
        public Rig Current { get; private set; }
        public Transform Arm => Current.Arm; public Transform OtherArm => Current.OtherArm; public Transform Hand => Current.Hand; public Transform OtherHand => Current.OtherHand;
        public Transform LeftLeg => Current.LeftLeg; public Transform RightLeg => Current.RightLeg;

        public static ScenePerson Build(Transform parent, ScenePeople.Character who, bool winter, string pose, string second = null)
        {
            var root = new GameObject("Scene person").transform; root.SetParent(parent, false); root.gameObject.layer = parent.gameObject.layer;
            var body = new GameObject("Build").transform; body.SetParent(root, false); body.localScale = new Vector3(who.Width, who.Height, who.Width);
            var person = root.gameObject.AddComponent<ScenePerson>(); person.Who = who;
            foreach (var p in second == null ? new[] { pose } : new[] { pose, second })
            {
                var holder = VehicleVisual.PersonFigure(body, p, winter ? who.Winter : who.Everyday);
                if (!holder) { Destroy(root.gameObject); return null; }
                person.rigs.Add(Make(holder));
            }
            person.Use(0);
            return person;
        }
        static Rig Make(Transform holder)
        {
            var space = holder.parent; var rig = new Rig { Pose = space };
            Transform Pivot(string name, Vector3 at, IEnumerable<Transform> parts)
            {
                var pv = new GameObject(name).transform; pv.SetParent(space, false); pv.position = at; pv.rotation = space.rotation; pv.gameObject.layer = space.gameObject.layer;
                foreach (var t in parts.ToArray()) t.SetParent(pv, true);
                return pv;
            }
            var sR = holder.Find("Shoulder R"); var sL = holder.Find("Shoulder L");
            if (sR) { rig.ElbowR = sR.Find("Elbow R"); rig.Hand = rig.ElbowR ? rig.ElbowR.Find("Wrist R") : null; rig.Arm = Pivot("Gesture shoulder", sR.position, new[] { sR }); }
            if (sL) { rig.ElbowL = sL.Find("Elbow L"); rig.OtherHand = rig.ElbowL ? rig.ElbowL.Find("Wrist L") : null; rig.OtherArm = Pivot("Other shoulder", sL.position, new[] { sL }); }
            // legs (Stand pose): parts "<...>_GR__slot" / "_GL__slot", origin on the hip joint
            foreach (var side in new[] { "R", "L" })
            {
                var parts = holder.Cast<Transform>().Where(t => t.name.Contains("_G" + side + "__")).ToList();
                if (parts.Count == 0) continue;
                var leg = Pivot(side == "R" ? "Right leg" : "Left leg", parts[0].position, parts);
                if (side == "R") rig.RightLeg = leg; else rig.LeftLeg = leg;
            }
            // stand-ins so the scenes can always set a rotation
            rig.RightLeg ??= new GameObject("Right leg (fixed)").transform; rig.RightLeg.SetParent(space, false);
            rig.LeftLeg ??= new GameObject("Left leg (fixed)").transform; rig.LeftLeg.SetParent(space, false);
            return rig;
        }
        public void Use(int index) { Current = rigs[Mathf.Clamp(index, 0, rigs.Count - 1)]; for (int i = 0; i < rigs.Count; i++) rigs[i].Pose.gameObject.SetActive(rigs[i] == Current); }
        public int Poses => rigs.Count;
        // Forearm bend at the elbow (degrees, forward and up from straight), both arms or one side (0 right, 1 left).
        public void SetElbow(int side, float bend)
        {
            var e = side == 0 ? Current.ElbowR : Current.ElbowL;
            // the joints keep the rider holder's frame (half a turn from the person's): + about its x is up and forward
            if (e) e.localRotation = Quaternion.Euler(bend, 0, 0);
        }
        // Hand position for props held in the hand: just past the wrist, along the forearm.
        public Vector3 Grip(int side)
        {
            var w = side == 0 ? Current.Hand : Current.OtherHand; var e = side == 0 ? Current.ElbowR : Current.ElbowL;
            if (!w || !e) return transform.position;
            return w.position + (w.position - e.position).normalized * .07f * transform.lossyScale.y;
        }
    }
}
