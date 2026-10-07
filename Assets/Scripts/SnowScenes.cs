using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // 0.73 Snow-only scenes (Dan's 0.72 debug session 00b27e), built from the AmbientLife figures (same body, materials,
    // no colliders, LOD cull) and shown only while the weather is Snow, in races and Free Roam:
    //  - two people on one sled sliding down the grass slope on the right of the Street Loop road below Dan's sled position
    //    (519.21, 74.77, -153.47, heading 172), then standing and pulling the sled back up, again and again;
    //  - three people playing broom hockey with a ball on the frozen pool beside the deck at Dan's house.
    // Both are placed by world position, so every scene of the shared world that contains the places shows them; a scene
    // without the ground (or the pool) there simply has no scene. Household vignettes are untouched (no shared space).
    public sealed class SnowScenes : MonoBehaviour
    {
        // The sled lane: 11 m right of the road centre (6.5 m off the pavement edge), road stations ~805-850, top to bottom.
        static readonly Vector2[] Lane = { new(520.5f, -173.1f), new(522.7f, -178.4f), new(524.6f, -182.7f), new(526.6f, -187.6f), new(528.5f, -192.5f), new(530.4f, -197.3f), new(532.3f, -202.0f), new(534.1f, -207.1f) };
        // The broom-hockey triangle inside the pool (water 7.1 x 12 m centred (397.3, -12.5)).
        static readonly Vector2[] Rink = { new(395.6f, -16.0f), new(399.1f, -12.5f), new(395.6f, -9.0f) };
        const string PoolName = "Rear swimming pool";

        sealed class Figure { public Transform root, arm, otherArm, leftLeg, rightLeg; public Transform[] shoes; public ScenePerson person; }
        AmbientLife life; RaceDirector race;
        bool built; GameObject sledScene, hockeyScene;
        // Sled
        Transform sled, rope; Figure[] riders; readonly List<Vector3> path = new(); readonly List<Vector3> normals = new(); float pathLength;
        // Hockey
        Figure[] players; Transform ball; Vector3[] homes; float ice;
        Material sledPaint, rubber, broomStraw, ballPaint;
        float nextAnimation;

        void Start() { life = GetComponent<AmbientLife>(); race = GetComponent<RaceDirector>(); }
        public bool SledActive => sledScene && sledScene.activeSelf;
        public bool HockeyActive => hockeyScene && hockeyScene.activeSelf;
        public Vector3 SledPosition => sled ? sled.position : Vector3.zero;
        public Vector3 BallPosition => ball ? ball.position : Vector3.zero;

        void Update()
        {
            var look = WorldLook.Current;
            bool snow = life && look && look.Preset != null && look.Preset.snow > .5f;
            if (snow && !built) Build();
            if (sledScene && sledScene.activeSelf != snow) sledScene.SetActive(snow);
            if (hockeyScene && hockeyScene.activeSelf != snow) hockeyScene.SetActive(snow);
            if (!snow || Time.timeScale == 0 || Time.time < nextAnimation) return;
            nextAnimation = Time.time + 1 / 30f;
            Vector3 player = race && race.vehicle ? race.vehicle.transform.position : Vector3.zero;
            // 0.92 Part F: in split-screen the scenes play when either player is near
            if (sledScene && SplitScreen.DistanceToPlayers(path[0], player) < 260) Sled(Time.time);
            if (hockeyScene && SplitScreen.DistanceToPlayers(ball.position, player) < 220) Hockey(Time.time);
        }

        // ---------- construction ----------
        Material Mat(Color c) => new(Shader.Find("Universal Render Pipeline/Lit")) { color = c, enableInstancing = true };
        // 0.81 Part D: Dan, the friend and the brother in winter coats and knitted hats (ScenePeople). seated = also build
        // the sled pose (the sled riders switch between sitting on the sled and standing).
        Figure Character(Transform parent, int index, bool seated)
        {
            var root = new GameObject("Snow day resident").transform; root.SetParent(parent, false);
            var person = ScenePerson.Build(root, ScenePeople.Of(index), true, "Stand", seated ? "Sled" : null);
            if (!person) { Destroy(root.gameObject); return Person(parent, index + (seated ? 0 : 2)); }
            var f = new Figure { root = root, person = person }; Refresh(f); return f;
        }
        static void Refresh(Figure f) { f.arm = f.person.Arm; f.otherArm = f.person.OtherArm; f.leftLeg = f.person.LeftLeg; f.rightLeg = f.person.RightLeg; }
        Figure Person(Transform parent, int index)
        {
            // The AmbientLife resident (Create), same proportions and materials.
            var f = new Figure { root = new GameObject("Snow day resident").transform };
            f.root.SetParent(parent, false); f.root.localScale = new Vector3(.92f + index % 3 * .08f, .96f + index % 3 * .04f, 1);
            var shirt = life.shirts[(index + 1) % life.shirts.Length];
            life.Part(f.root, "Torso", PrimitiveType.Capsule, new(0, 1.13f, 0), new(.42f, .34f, .28f), shirt);
            life.Part(f.root, "Head", PrimitiveType.Sphere, new(0, 1.63f, 0), new(.28f, .32f, .29f), life.skin);
            life.Part(f.root, "Hair", PrimitiveType.Sphere, new(0, 1.75f, -.015f), new(.29f, .13f, .29f), life.hair);
            // Snow day: a knitted hat in the shirt colour over the hair.
            life.Part(f.root, "Knit hat", PrimitiveType.Sphere, new(0, 1.77f, -.01f), new(.30f, .2f, .30f), shirt);
            f.leftLeg = life.Part(f.root, "Left leg", PrimitiveType.Capsule, new(-.12f, .45f, 0), new(.16f, .40f, .18f), life.trousers);
            f.rightLeg = life.Part(f.root, "Right leg", PrimitiveType.Capsule, new(.12f, .45f, 0), new(.16f, .40f, .18f), life.trousers);
            f.shoes = new[] { life.Part(f.root, "Shoe", PrimitiveType.Cube, new(-.12f, .065f, .04f), new(.19f, .13f, .30f), life.hair), life.Part(f.root, "Shoe", PrimitiveType.Cube, new(.12f, .065f, .04f), new(.19f, .13f, .30f), life.hair) };
            f.arm = new GameObject("Gesture shoulder").transform; f.arm.SetParent(f.root, false); f.arm.localPosition = new(.24f, 1.36f, 0);
            life.Part(f.arm, "Sleeve", PrimitiveType.Capsule, new(0, -.16f, 0), new(.15f, .19f, .15f), shirt);
            life.Part(f.arm, "Hand", PrimitiveType.Sphere, new(0, -.39f, .01f), new(.13f, .14f, .13f), life.skin);
            f.otherArm = new GameObject("Other shoulder").transform; f.otherArm.SetParent(f.root, false); f.otherArm.localPosition = new(-.26f, 1.36f, 0);
            life.Part(f.otherArm, "Other arm", PrimitiveType.Capsule, new(0, -.28f, 0), new(.15f, .26f, .15f), shirt);
            return f;
        }
        static void Lod(Transform root) { var lod = root.gameObject.AddComponent<LODGroup>(); lod.SetLODs(new[] { new LOD(.004f, root.GetComponentsInChildren<Renderer>()) }); lod.RecalculateBounds(); }
        static bool Ground(Vector3 at, out RaycastHit ground)
        {
            ground = default; float best = float.NegativeInfinity; bool found = false;
            // The ground surface itself (terrain sheets are "Ground_..."), never a tree, sign or vehicle above it.
            foreach (var h in Physics.RaycastAll(new Vector3(at.x, at.y + 80, at.z), Vector3.down, 200, 1, QueryTriggerInteraction.Ignore))
                if (!h.rigidbody && (h.collider.name.StartsWith("Ground") || h.collider is TerrainCollider) && h.point.y > best) { best = h.point.y; ground = h; found = true; }
            return found;
        }
        void Build()
        {
            built = true;
            sledPaint = Mat(new Color(.78f, .12f, .10f)); rubber = Mat(new Color(.12f, .12f, .13f)); broomStraw = Mat(new Color(.80f, .66f, .34f)); ballPaint = Mat(new Color(1f, .45f, .05f));
            // Sled lane: the ground under the lane every metre (no lane, no scene).
            for (int i = 0; i + 1 < Lane.Length; i++)
            {
                var a = new Vector3(Lane[i].x, 0, Lane[i].y); var b = new Vector3(Lane[i + 1].x, 0, Lane[i + 1].y); int n = Mathf.CeilToInt(Vector3.Distance(a, b));
                for (int k = 0; k < n || (i == Lane.Length - 2 && k == n); k++)
                {
                    var p = Vector3.Lerp(a, b, k / (float)n); p.y = 120;
                    if (!Ground(p, out var g)) { path.Clear(); break; }
                    path.Add(g.point); normals.Add(g.normal);
                }
                if (path.Count == 0) break;
            }
            for (int i = 1; i < path.Count; i++) pathLength += Vector3.Distance(path[i - 1], path[i]);
            if (path.Count > 10 && path[0].y - path[^1].y > 8)
            {
                sledScene = new GameObject("Snow day / sledding (Snow only)"); sledScene.transform.SetParent(transform, false);
                sled = new GameObject("Sled").transform; sled.SetParent(sledScene.transform, false);
                life.Part(sled, "Sled deck", PrimitiveType.Cube, new(0, .16f, 0), new(.62f, .06f, 1.55f), sledPaint);
                life.Part(sled, "Sled curl", PrimitiveType.Cube, new(0, .30f, .86f), new(.62f, .06f, .36f), sledPaint).localRotation = Quaternion.Euler(-55, 0, 0);
                foreach (float side in new[] { -1f, 1f }) { life.Part(sled, "Runner", PrimitiveType.Cube, new(side * .26f, .05f, 0), new(.05f, .1f, 1.6f), rubber); life.Part(sled, "Side rail", PrimitiveType.Cube, new(side * .3f, .24f, -.05f), new(.04f, .05f, 1.2f), rubber); }
                rope = life.Part(sledScene.transform, "Pull rope", PrimitiveType.Cylinder, Vector3.zero, new(.025f, .5f, .025f), rubber); rope.gameObject.SetActive(false);
                riders = new[] { Character(sledScene.transform, 0, true), Character(sledScene.transform, 1, true) };
                Lod(sled); foreach (var r in riders) Lod(r.root);
                sledScene.SetActive(false);
            }
            // Hockey: only where the pool exists (it freezes with every other water body).
            ShallowWater pool = null; foreach (var w in ShallowWater.Active) if (w && w.name == PoolName && w.gameObject.scene == gameObject.scene) pool = w;
            if (pool)
            {
                ice = pool.Surface;
                hockeyScene = new GameObject("Snow day / broom hockey (Snow only)"); hockeyScene.transform.SetParent(transform, false);
                players = new Figure[3]; homes = new Vector3[3];
                for (int i = 0; i < 3; i++)
                {
                    homes[i] = new Vector3(Rink[i].x, ice, Rink[i].y);
                    var f = players[i] = Character(hockeyScene.transform, i, false); f.root.position = homes[i];
                    // Broom in the gesture hand: handle down from the hand, straw head at the ice.
                    var holdAt = f.person ? f.person.Hand : f.arm; var grip = f.person ? f.person.Grip(0) : f.arm.TransformPoint(new Vector3(0, -.39f, .01f));
                    var handle = life.Part(holdAt, "Broom handle", PrimitiveType.Cylinder, Vector3.zero, Vector3.one, broomStraw);
                    handle.SetPositionAndRotation(grip + Vector3.down * .5f + f.root.forward * .05f, f.root.rotation); handle.localScale = new Vector3(.04f, .55f, .04f);
                    var head = life.Part(holdAt, "Broom head", PrimitiveType.Cube, Vector3.zero, Vector3.one, broomStraw);
                    head.SetPositionAndRotation(grip + Vector3.down * 1.07f + f.root.forward * .05f, f.root.rotation * Quaternion.Euler(0, 90, 0)); head.localScale = new Vector3(.34f, .13f, .1f);
                    Lod(f.root);
                }
                ball = life.Part(hockeyScene.transform, "Hockey ball", PrimitiveType.Sphere, homes[0] + Vector3.up * .08f, Vector3.one * .16f, ballPaint);
                hockeyScene.SetActive(false);
            }
        }

        // ---------- sled: slide down, stand, pull it back up, sit, again ----------
        const float Hold = 2f, Slide = 6.5f, Stand = 2.2f, WalkSpeed = 1.8f, Sit = 1.8f;
        (Vector3 p, Vector3 f, Vector3 n) Along(float d)
        {
            d = Mathf.Clamp(d, 0, pathLength); float run = 0;
            for (int i = 1; i < path.Count; i++)
            {
                float seg = Vector3.Distance(path[i - 1], path[i]);
                if (run + seg >= d || i == path.Count - 1) { float t = seg > 0 ? Mathf.Clamp01((d - run) / seg) : 0; return (Vector3.Lerp(path[i - 1], path[i], t), (path[i] - path[i - 1]).normalized, Vector3.Slerp(normals[i - 1], normals[i], t)); }
                run += seg;
            }
            return (path[0], (path[1] - path[0]).normalized, normals[0]);
        }
        void Sled(float time)
        {
            float walk = pathLength / WalkSpeed, cycle = Hold + Slide + Stand + walk + Sit, t = Mathf.Repeat(time, cycle);
            float d; bool seated; float turn;// turn: 0 = sled faces downhill, 1 = faces uphill (being pulled)
            if (t < Hold) { d = 0; seated = true; turn = 0; }
            else if (t < Hold + Slide) { float x = (t - Hold) / Slide; d = pathLength * x * x * (3 - 2 * x); seated = true; turn = 0; }
            else if (t < Hold + Slide + Stand) { d = pathLength; seated = t < Hold + Slide + .7f; turn = Mathf.SmoothStep(0, 1, (t - Hold - Slide - .7f) / 1.3f); }
            else if (t < Hold + Slide + Stand + walk) { d = pathLength * (1 - (t - Hold - Slide - Stand) / walk); seated = false; turn = 1; }
            else { d = 0; float x = (t - Hold - Slide - Stand - walk) / Sit; seated = x > .6f; turn = 1 - Mathf.SmoothStep(0, 1, x / .6f); }
            var (p, f, n) = Along(d); var down = Vector3.ProjectOnPlane(f, n).normalized;
            var face = Vector3.Slerp(down, -down, turn); if (face.sqrMagnitude < .01f) face = Vector3.Cross(n, down);
            sled.SetPositionAndRotation(p, Quaternion.LookRotation(Vector3.ProjectOnPlane(face, n), n));
            bool pulling = !seated && t >= Hold + Slide + Stand && t < Hold + Slide + Stand + walk;
            rope.gameObject.SetActive(pulling);
            for (int i = 0; i < 2; i++)
            {
                var r = riders[i];
                if (seated)
                {
                    // Seated one behind the other, legs forward; the rear rider holds the front rider's shoulders.
                    var seat = sled.TransformPoint(new Vector3(0, .19f, r.person ? (i == 0 ? .22f : -.52f) : (i == 0 ? .28f : -.38f)));
                    r.root.SetPositionAndRotation(seat - sled.up * (r.person ? .17f : .66f), sled.rotation);
                    Legs(r, true, i == 0 ? .12f : .23f); float reach = r.person ? (i == 0 ? 0 : -30) : (i == 0 ? -55 : -80); r.arm.localRotation = Quaternion.Euler(reach, 0, 0); r.otherArm.localRotation = Quaternion.Euler(reach, 0, 0);
                }
                else
                {
                    // Standing / walking uphill beside the sled; the first one pulls the rope.
                    float ahead = pulling ? (i == 0 ? 1.9f : .4f) : (i == 0 ? 1.2f : -.9f), side = i == 0 ? .15f : 1.0f;
                    var (q, g, _) = Along(d - (pulling ? ahead : 0)); var up = -g; up.y = 0; if (up.sqrMagnitude < .001f) up = Vector3.forward;
                    var right = Vector3.Cross(Vector3.up, up.normalized);
                    var at = pulling ? q + right * side : sled.position + sled.right * (i == 0 ? 1f : -1f) + sled.forward * (i == 0 ? .3f : -.4f);
                    if (Ground(at, out var gh)) at.y = gh.point.y + .02f;
                    var look = pulling ? up.normalized : Vector3.ProjectOnPlane(-sled.right * (i == 0 ? 1 : -1), Vector3.up).normalized;
                    r.root.SetPositionAndRotation(at, Quaternion.LookRotation(look, Vector3.up));
                    Legs(r, false, .12f);
                    float swing = pulling ? Mathf.Sin(time * 7f + i) * 22 : 0;
                    r.leftLeg.localRotation = Quaternion.Euler(swing, 0, 0); r.rightLeg.localRotation = Quaternion.Euler(-swing, 0, 0);
                    r.arm.localRotation = Quaternion.Euler(pulling && i == 0 ? 25 : -swing * .6f, 0, 0); r.otherArm.localRotation = Quaternion.Euler(swing * .6f, 0, 0);
                }
            }
            if (pulling)
            {
                var hand = riders[0].person ? riders[0].person.Grip(0) : riders[0].arm.TransformPoint(new Vector3(0, -.39f, .01f)); var hook = sled.TransformPoint(new Vector3(0, .3f, .95f));
                rope.position = (hand + hook) * .5f; rope.up = (hand - hook).normalized; rope.localScale = new Vector3(.025f, Vector3.Distance(hand, hook) * .5f, .025f);
            }
        }
        static void Legs(Figure r, bool seated, float spread)
        {
            if (r.person) { r.person.Use(seated ? 1 : 0); Refresh(r); return; }
            // Seated: legs forward along the sled at hip height (the rear rider's either side of the front rider).
            // Standing: the AmbientLife pose.
            r.leftLeg.localPosition = seated ? new Vector3(-spread, .70f, .34f) : new Vector3(-.12f, .45f, 0); r.rightLeg.localPosition = seated ? new Vector3(spread, .70f, .34f) : new Vector3(.12f, .45f, 0);
            if (seated) { r.leftLeg.localRotation = r.rightLeg.localRotation = Quaternion.Euler(84, 0, 0); }
            r.shoes[0].localPosition = seated ? new Vector3(-spread, .74f, .78f) : new Vector3(-.12f, .065f, .04f); r.shoes[1].localPosition = seated ? new Vector3(spread, .74f, .78f) : new Vector3(.12f, .065f, .04f);
            r.shoes[0].localRotation = r.shoes[1].localRotation = Quaternion.Euler(seated ? -80 : 0, 0, 0);
        }

        // ---------- broom hockey: the ball is knocked round the three players ----------
        const float Pass = 2.2f;
        void Hockey(float time)
        {
            var centre = (homes[0] + homes[1] + homes[2]) / 3;
            float cycle = time / Pass; int shooter = Mathf.FloorToInt(cycle) % 3, receiver = (shooter + 1) % 3; float moment = Mathf.Repeat(cycle, 1);
            Vector3 Stick(int i) { var to = centre - homes[i]; to.y = 0; return homes[i] + to.normalized * .62f + Vector3.Cross(Vector3.up, to.normalized) * .24f; }
            // The ball rolls from the shooter's broom to the receiver's, slowing as it goes, after a short wind-up.
            float roll = Mathf.Clamp01((moment - .18f) / .7f); roll = 1 - (1 - roll) * (1 - roll);
            var from = Stick(shooter); var to2 = Stick(receiver);
            ball.position = new Vector3(Mathf.Lerp(from.x, to2.x, roll), ice + .08f, Mathf.Lerp(from.z, to2.z, roll));
            ball.Rotate(Vector3.right, 400 * Time.deltaTime * (roll < 1 ? 1 : 0), Space.Self);
            for (int i = 0; i < 3; i++)
            {
                var f = players[i]; float t = time * .8f + i * 2.1f;
                // A small shuffle on the ice, turned towards the ball.
                var at = homes[i] + new Vector3(Mathf.Sin(t) * .18f, 0, Mathf.Cos(t * 1.3f) * .14f); f.root.position = at;
                var look = ball.position - at; look.y = 0; if (look.sqrMagnitude < .04f) look = centre - at; look.y = 0;
                f.root.rotation = Quaternion.Slerp(f.root.rotation, Quaternion.LookRotation(look.normalized, Vector3.up) * Quaternion.Euler(0, -14, 0), .25f);
                // Broom: the shooter swings back then sweeps through; the receiver holds it on the ice; the third waits.
                float a = i == shooter ? (moment < .18f ? Mathf.Lerp(-24, -6, moment / .18f) : Mathf.Lerp(-6, -40, Mathf.Clamp01((moment - .18f) / .14f)) + Mathf.Clamp01((moment - .32f) / .4f) * 16) : i == receiver ? -26 + Mathf.Sin(t * 3) * 3 : -32;
                f.arm.localRotation = Quaternion.Euler(a, 0, 0); f.otherArm.localRotation = Quaternion.Euler(a * .8f, 0, 12);
                f.leftLeg.localRotation = Quaternion.Euler(Mathf.Sin(t * 2) * 6, 0, 0); f.rightLeg.localRotation = Quaternion.Euler(-Mathf.Sin(t * 2) * 6, 0, 0);
            }
        }
        void OnDestroy() { foreach (var m in new[] { sledPaint, rubber, broomStraw, ballPaint }) if (m) Destroy(m); }
    }
}
