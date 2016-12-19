using System;
using System.Linq;
using UnityEngine;

namespace HouraiTeahouse.SmashBrew {

    /// <summary> The pallete swap behaviour for changing out the </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public class MaterialSwap : MonoBehaviour {

        [Serializable]
        public class Swap {

            [Serializable]
            public class MaterialSet {

                [SerializeField]
                [Resource(typeof(Material))]
                [Tooltip("The materials to apply to the renderers")]
                string[] _materials;

                public void Set(Renderer[] targets) {
                    if (targets == null)
                        return;
                    var loadedMaterials = _materials.Select(Resources.Load<Material>).ToArray();
                    foreach (Renderer renderer in targets)
                        if(renderer != null)
                            renderer.sharedMaterials = loadedMaterials;
                }

            }

            [SerializeField]
            [Tooltip("The set of materials to swap to")]
            MaterialSet[] MaterialSets;

            [SerializeField]
            [Tooltip("The set of renderers to apply the materials to")]
            Renderer[] TargetRenderers;

            /// <summary>
            /// The count of available swaps for this material swap set
            /// </summary>
            public int Count {
                get { return MaterialSets.Length; }
            }

            public void Set(int palleteSwap) {
                if (palleteSwap < 0 || palleteSwap >= MaterialSets.Length)
                    return;
                MaterialSets[palleteSwap].Set(TargetRenderers);
            }

        }

        int _color;

        [SerializeField]
        Swap[] _swaps;

        /// <summary> Gets the number of pallete swaps are available </summary>
        public int Count {
            get { return !_swaps.IsNullOrEmpty() ? _swaps.Max(s => s.Count) : 0; }
        }

        public int Pallete {
            get { return _color; }
            set {
                _color = value;
                if (_swaps == null)
                    return;
                foreach (Swap swap in _swaps)
                    swap.Set(value);
            }
        }

#if UNITY_EDITOR
        // For editor only.
        public void Refesh(int value) {
            _color = value;
            foreach (Swap swap in _swaps)
                swap.Set(value);
        }
#endif
    }

}
