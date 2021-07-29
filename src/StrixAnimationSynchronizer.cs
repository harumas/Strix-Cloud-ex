using SoftGear.Strix.Unity.Runtime;
using UnityEngine;

namespace StrixEx {
    public class StrixAnimationSynchronizer : StrixBehaviour {
        public Animator Animator;

        public bool SynchronizeStates = true;
        int[] animationHashes;

        int[] propertyHashes;
        int[] transitionHashes;

        AnimatorControllerParameter[] parameters;
        const string stateHashPropertyName = "stateHash";
        const string normalizedTimePropertyName = "normalizedTime";

        void Start() {
            if (this.Animator == null)
                return;

            this.animationHashes = new int[this.Animator.layerCount];
            this.transitionHashes = new int[this.Animator.layerCount];
            this.parameters = this.Animator.parameters;
            this.propertyHashes = new int[parameters.Length];

            for (int i = 0; i < propertyHashes.Length; i++) {
                propertyHashes[i] = Animator.StringToHash(parameters[i].name);
            }
        }

        public override void OnStrixSerialize(StrixSerializationProperties properties) {
            if (this.Animator == null || propertyHashes.Length == 0)
                return;
            this.WriteAnimatorProperties(properties);
            this.WriteLayersStates(properties);
        }

        public override void OnStrixDeserialize(StrixSerializationProperties properties) {
            if (this.Animator == null || propertyHashes == null)
                return;
            this.Animator.applyRootMotion = false;
            this.ReadAnimatorProperties(properties);
            this.ReadLayersStates(properties);
        }

        void WriteLayersStates(StrixSerializationProperties properties) {
            if (!this.SynchronizeStates)
                return;
            for (int index = 0; index < this.Animator.layerCount; ++index) {
                int stateHash = 0;
                float normalizedTime = 0.0f;
                if (this.CheckAnimationStateChanged(index, out stateHash, out normalizedTime)) {
                    try {
                        string statePropertyName1 = this.GetLayerStatePropertyName(index, stateHashPropertyName);
                        properties.Set(StrixReplicatorIndexCache.GetDefaultIndexForProperty(statePropertyName1), stateHash, statePropertyName1);
                        string statePropertyName2 = this.GetLayerStatePropertyName(index, stateHashPropertyName);
                        properties.Set(
                            StrixReplicatorIndexCache.GetDefaultIndexForProperty(this.GetLayerStatePropertyName(index, statePropertyName2)),
                            normalizedTime, statePropertyName2);
                    }
                    catch (ConflictingPropertyException ex) {
                        Debug.LogError(
                            $"The key {ex.Key} for animation state \"{ex.AddedValueDescription}\" is conflicting with \"{ex.ExistingValueDescription}\". StrixAnimationSynchronizer will not work properly. Consider changing the key of the conflicting property: {ex.Key} (\"{ex.ExistingValueDescription}\")");
                    }
                }
            }
        }

        void WriteLayersWeights(StrixSerializationProperties properties) {
            if (this.Animator.layerCount <= 1)
                return;
            for (int index = 0; index < this.Animator.layerCount; ++index) {
                try {
                    string layerPropertyName = this.GetLayerPropertyName(index);
                    properties.Set(StrixReplicatorIndexCache.GetDefaultIndexForProperty(layerPropertyName), this.Animator.GetLayerWeight(index),
                        layerPropertyName);
                }
                catch (ConflictingPropertyException ex) {
                    Debug.LogError(
                        $"The key {ex.Key} for animation layer \"{ex.AddedValueDescription}\" is conflicting with \"{ex.ExistingValueDescription}\". StrixAnimationSynchronizer will not work properly. Consider changing the key of the conflicting property: {ex.Key} (\"{ex.ExistingValueDescription}\")");
                }
            }
        }

        void WriteAnimatorProperties(StrixSerializationProperties properties) {
            if (this.Animator == null)
                return;
            for (int index = 0; index < this.Animator.parameterCount; ++index) {
                // if (this.ParameterSynchronizationFlags[index]) {
                try {
                    AnimatorControllerParameter parameter = this.parameters[index];
                    string parameterPropertyName = this.GetAnimatorParameterPropertyName(parameter);
                    int indexForProperty = StrixReplicatorIndexCache.GetDefaultIndexForProperty(parameterPropertyName);
                    switch (parameter.type) {
                        case AnimatorControllerParameterType.Float:
                            properties.Set(indexForProperty, this.Animator.GetFloat(propertyHashes[index]), parameterPropertyName);
                            continue;
                        case AnimatorControllerParameterType.Int:
                            properties.Set(indexForProperty, this.Animator.GetInteger(propertyHashes[index]), parameterPropertyName);
                            continue;
                        case AnimatorControllerParameterType.Bool:
                            properties.Set(indexForProperty, this.Animator.GetBool(propertyHashes[index]), parameterPropertyName);
                            continue;
                        case AnimatorControllerParameterType.Trigger:
                            properties.Set(indexForProperty, this.Animator.GetBool(propertyHashes[index]), parameterPropertyName);
                            continue;
                        default:
                            continue;
                    }
                }
                catch (ConflictingPropertyException ex) {
                    Debug.LogError(string.Format(
                        "The key {0} for animation parameter \"{1}\" is conflicting with \"{2}\". StrixAnimationSynchronizer will not work properly. Consider changing the key of the conflicting property: {3} (\"{4}\")",
                        ex.Key, ex.AddedValueDescription, ex.ExistingValueDescription, ex.Key,
                        ex.ExistingValueDescription));
                }
                // }
            }
        }

        bool CheckAnimationStateChanged(int index, out int stateHash, out float normalizedTime) {
            stateHash = 0;
            normalizedTime = 0.0f;
            AnimatorStateInfo animatorStateInfo = this.Animator.GetCurrentAnimatorStateInfo(index);
            if (animatorStateInfo.fullPathHash == this.animationHashes[index])
                return false;
            if (this.animationHashes[index] != 0) {
                stateHash = animatorStateInfo.fullPathHash;
                normalizedTime = animatorStateInfo.normalizedTime;
            }

            this.transitionHashes[index] = 0;
            this.animationHashes[index] = animatorStateInfo.fullPathHash;
            return true;
        }

        void ReadAnimatorProperties(StrixSerializationProperties properties) {
            for (int index = 0; index < this.Animator.parameterCount; ++index) {
                // if (this.ParameterSynchronizationFlags[index]) {
                AnimatorControllerParameter parameter = this.Animator.parameters[index];
                int indexForProperty = StrixReplicatorIndexCache.GetDefaultIndexForProperty(this.GetAnimatorParameterPropertyName(parameter));
                switch (parameter.type) {
                    case AnimatorControllerParameterType.Float:
                        float num1 = 0.0f;
                        if (properties.Get<float>(indexForProperty, ref num1)) {
                            this.Animator.SetFloat(propertyHashes[index], num1);
                            continue;
                        }

                        continue;
                    case AnimatorControllerParameterType.Int:
                        int num2 = 0;
                        if (properties.Get<int>(indexForProperty, ref num2)) {
                            this.Animator.SetInteger(propertyHashes[index], num2);
                            continue;
                        }

                        continue;
                    case AnimatorControllerParameterType.Bool:
                        bool flag1 = false;
                        if (properties.Get<bool>(indexForProperty, ref flag1)) {
                            this.Animator.SetBool(propertyHashes[index], flag1);
                            continue;
                        }

                        continue;
                    case AnimatorControllerParameterType.Trigger:
                        bool flag2 = false;
                        if (properties.Get<bool>(indexForProperty, ref flag2)) {
                            this.Animator.SetBool(propertyHashes[index], flag2);
                            continue;
                        }

                        continue;
                    default:
                        continue;
                }
                // }
            }
        }

        void ReadLayersStates(StrixSerializationProperties properties) {
            if (!this.SynchronizeStates)
                return;
            for (int index = 0; index < this.Animator.layerCount; ++index) {
                int stateNameHash = 0;
                float normalizedTime = 0.0f;
                if (!properties.Get<int>(
                        StrixReplicatorIndexCache.GetDefaultIndexForProperty(this.GetLayerStatePropertyName(index, stateHashPropertyName)),
                        ref stateNameHash) ||
                    !properties.Get<float>(
                        StrixReplicatorIndexCache.GetDefaultIndexForProperty(this.GetLayerStatePropertyName(index, normalizedTimePropertyName)),
                        ref normalizedTime))
                    break;
                if (stateNameHash != 0 && normalizedTime != 0.0 &&
                    this.Animator.GetCurrentAnimatorStateInfo(index).fullPathHash != stateNameHash)
                    this.Animator.Play(stateNameHash, 0, normalizedTime);
            }
        }

        void ReadLayersWeights(StrixSerializationProperties properties) {
            if (this.Animator.layerCount <= 1)
                return;
            for (int index = 0; index < this.Animator.layerCount; ++index) {
                string layerPropertyName = this.GetLayerPropertyName(index);
                float weight = 0.0f;
                if (properties.Get<float>(StrixReplicatorIndexCache.GetDefaultIndexForProperty(layerPropertyName), ref weight))
                    this.Animator.SetLayerWeight(index, weight);
            }
        }

        string GetLayerPropertyName(int index) => "_al" + this.Animator.GetLayerName(index);

        string GetLayerStatePropertyName(int index, string property) => "_al" + index + "_" + property;

        string GetAnimatorParameterPropertyName(AnimatorControllerParameter parameter) => "_ap" + parameter.name;
    }
}