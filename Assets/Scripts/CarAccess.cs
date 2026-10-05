using System.Collections.Generic;

namespace Racer
{
    // 0.80 Part C: cars may race on every course (player and AI). Courses are not reshaped for them. Every course and
    // every shortcut was driven with both cars and a car AI field (Docs/Report080/VALIDATION.md); all passed, so both
    // lists are empty. They are where a shortcut or a course would be closed to cars: a closed shortcut is never taken
    // by car AI and gives a player car no entry credit (the normal reset brings it back); a restricted course keeps the
    // old motorcycles / ATVs only rule. Motorcycle and ATV availability is unchanged; records stay per vehicle.
    public static class CarAccess
    {
        // "Scene/branch title"
        static readonly HashSet<string> closedBranches = new() { };
        // scene names
        static readonly HashSet<string> restrictedCourses = new() { };

        public static bool CourseAllowsCars(string scene) => !restrictedCourses.Contains(scene);
        public static bool ClosedToCars(WoodlandRoute branch) => branch && closedBranches.Contains(branch.gameObject.scene.name + "/" + branch.title);
        public static bool Open(WoodlandRoute branch, ArcadeVehicle vehicle)
        {
            if (!ClosedToCars(branch)) return true;
            var configuration = vehicle ? vehicle.GetComponent<VehicleConfiguration>() : null;
            return !configuration || configuration.Profile.Small;
        }
    }
}
