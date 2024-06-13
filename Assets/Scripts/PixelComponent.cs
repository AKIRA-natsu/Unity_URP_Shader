using UnityEngine;

namespace Game.Controller {
    [RequireComponent(typeof(Renderer))]
    public class PixelComponent : MonoBehaviour {
        #if UNITY_EDITOR
        [Range(0, 100)]
        [SerializeField]
        private int slider;

        private void OnValidate() {
            if (value.Value != slider)
                value.Value = slider;
        }
        #endif

        public BindableRangeValue<int> value = new (100, 0);

        public ComputeShader shader;
        public Texture2D initTex;

        private int _kernel;
        private Vector2Int dispatchCount;
        
        private void Start () {
            // SystemInfo.supportsComputeShaders.Log();
            _kernel = shader.FindKernel ("CSMain");
            Reinit();
        }

        [ContextMenu("Reinit")]
        public void Reinit() {
            var size = initTex.width;
            RenderTexture tex = new(size, size, 0);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            tex.enableRandomWrite = true;
            tex.Create();
            
            Graphics.Blit(initTex, tex);

            RenderTexture fall = new(size, size, 0);
            fall.wrapMode = TextureWrapMode.Clamp;
            fall.filterMode = FilterMode.Point;
            fall.enableRandomWrite = true;
            fall.Create();

            this.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
            shader.SetTexture(_kernel, "_InputTexture", initTex);
            shader.SetTexture(_kernel, "_FallTexture", fall);
            shader.SetTexture(_kernel, "_OutputTexture", tex);
            shader.GetKernelThreadGroupSizes(_kernel, out uint threadX, out uint threadY, out _);
            dispatchCount.x = Mathf.CeilToInt(size / threadX);
            dispatchCount.y = Mathf.CeilToInt(size / threadY);

            #if UNITY_EDITOR
            slider =
            #endif
            value.Value = 100;
        }

        private void Update() {
            shader.Dispatch(_kernel, dispatchCount.x , dispatchCount.y, 1);
        }

        public void UpdateTransparent(int value, int total) {
            shader.SetFloat("_PixelsPercentage", value);
        }
    }
}