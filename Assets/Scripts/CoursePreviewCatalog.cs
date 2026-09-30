using System;
using UnityEngine;

namespace Racer
{
    // Read-only presentation snapshots generated from saved courses, never race state.
    [Serializable] public sealed class CoursePreviewCatalog
    {
        [Serializable] public sealed class Path { public Vector3[] points; }
        [Serializable] public sealed class Course
        {
            public string scene,id;
            public Vector3[] main,gates;
            public Path[] branches;
        }
        public Course[] courses;
        static CoursePreviewCatalog cached;
        public static Course[] Courses
        {
            get
            {
                if(cached==null){var asset=Resources.Load<TextAsset>("WorldMaps/CoursePreviews");cached=asset?JsonUtility.FromJson<CoursePreviewCatalog>(asset.text):new CoursePreviewCatalog();}
                return cached.courses??Array.Empty<Course>();
            }
        }
    }
}
