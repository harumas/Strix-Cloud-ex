using System.Collections;
using System.Collections.Generic;
using SoftGear.Strix.Unity.Runtime;
using Unity.Mathematics;
using UnityEngine;

namespace StrixEx {
    public static class StrixExtensions {
        public static void SetVector3(
            this StrixSerializationProperties properties,
            int[] indexes,
            SoftGear.Strix.Client.Ingame.Interpolation.Vector3 v,
            string description = null
        ) {
            properties.Set(indexes[0], (object) v.X, description + "_X");
            properties.Set(indexes[1], (object) v.Y, description + "_Y");
            properties.Set(indexes[2], (object) v.Z, description + "_Z");
        }

        public static void SetVector3(
            this StrixSerializationProperties properties,
            int[] indexes,
            Vector3 v,
            string description = null
        ) {
            properties.Set(indexes[0], (object) v.x, description + "_X");
            properties.Set(indexes[1], (object) v.y, description + "_Y");
            properties.Set(indexes[2], (object) v.z, description + "_Z");
        }

        public static bool GetVector3(this StrixSerializationProperties properties, int[] indexes, ref Vector3 v) {
            bool flag = properties.Get<float>(indexes[0], ref v.x);
            if (properties.Get<float>(indexes[1], ref v.y))
                flag = true;
            if (properties.Get<float>(indexes[2], ref v.z))
                flag = true;

            return flag;
        }

        public static void SetQuaternion(this StrixSerializationProperties properties, int[] indexes, Quaternion v, string description = null) {
            properties.Set(indexes[0], (object) v.x, description + "_X");
            properties.Set(indexes[1], (object) v.y, description + "_Y");
            properties.Set(indexes[2], (object) v.z, description + "_Z");
            properties.Set(indexes[3], (object) v.w, description + "_W");
        }

        public static bool GetQuaternion(this StrixSerializationProperties properties, int[] indexes, ref Quaternion v) {
            bool flag = properties.Get<float>(indexes[0], ref v.x);
            if (properties.Get<float>(indexes[1], ref v.y))
                flag = true;
            if (properties.Get<float>(indexes[2], ref v.z))
                flag = true;
            if (properties.Get<float>(indexes[3], ref v.w))
                flag = true;

            return flag;
        }
    }
}