using UnityEngine;

namespace Racer
{
    // Stable IDs are save/record identities; changing tuning requires a rules version bump.
    public sealed class VehicleProfile
    {
        public string Id, Name, Class, Description;
        public float Speed, Acceleration, Grip, Response, Mass, Wheelbase, Pitch;
        public Vector3 Size, Camera;
        public bool Small => Class != "Car";
        public static readonly VehicleProfile[] All = {
            new() { Id="original", Name="Street Classic", Class="Car", Description="Balanced handling / high stability / strong contact", Speed=49, Acceleration=14.5f, Grip=25, Response=8, Mass=1200, Wheelbase=2.6f, Pitch=1, Size=new(1.85f,.65f,3.7f), Camera=new(0,3.6f,-7.5f) },
            new() { Id="tourer", Name="Longroof GT", Class="Car", Description="Long, fast cruiser / deliberate turn-in / strongest contact", Speed=52, Acceleration=13.5f, Grip=24, Response=6.5f, Mass=1500, Wheelbase=3, Pitch=.85f, Size=new(2,.8f,4.5f), Camera=new(0,3.8f,-8.2f) },
            new() { Id="moto", Name="Needle 600", Class="Motorcycle", Description="Fastest / quickest response / fragile on impacts and landings", Speed=61, Acceleration=18, Grip=32, Response=12, Mass=220, Wheelbase=1.65f, Pitch=1.4f, Size=new(.65f,.8f,2.25f), Camera=new(0,3,-6.5f) },
            new() { Id="atv", Name="Trail Four", Class="ATV", Description="Fast / responsive / planted stance / vulnerable to cars", Speed=56, Acceleration=17, Grip=30, Response=10.5f, Mass=340, Wheelbase=1.65f, Pitch=1.16f, Size=new(1.45f,.65f,2.35f), Camera=new(0,3.2f,-6.8f) }
        };
        public static VehicleProfile Find(string id)
        {
            foreach(var profile in All) if(profile.Id==id) return profile;
            return All[0];
        }
    }
}
