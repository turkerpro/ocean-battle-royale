using UnityEngine;
using UnityEngine.Rendering;

namespace OceanBattleRoyale.World
{
    public class OceanManager : MonoBehaviour
    {
        [Header("Ocean Settings")]
        [SerializeField] private Material _oceanMaterial;
        [SerializeField] private float _oceanSize = 2000f;
        [SerializeField] private int _gridResolution = 64;

        [Header("LOD Settings")]
        [SerializeField] private float _lod0Distance = 100f;
        [SerializeField] private float _lod1Distance = 300f;
        [SerializeField] private float _lod2Distance = 600f;

        [Header("Quality")]
        [SerializeField] private bool _enableCaustics = true;
        [SerializeField] private bool _enableFoam = true;
        [SerializeField] private bool _mobileFallback = false;

        private Mesh _oceanMesh;
        private MaterialPropertyBlock _props;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _props = new MaterialPropertyBlock();
            GenerateOceanMesh();
            ApplyQualitySettings();
        }

        private void GenerateOceanMesh()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

            _oceanMesh = new Mesh { name = "OceanMesh", indexFormat = IndexFormat.UInt32 };

            int vertCount = (_gridResolution + 1) * (_gridResolution + 1);
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] triangles = new int[_gridResolution * _gridResolution * 6];

            float halfSize = _oceanSize * 0.5f;
            float step = _oceanSize / _gridResolution;

            int vertIndex = 0;
            for (int z = 0; z <= _gridResolution; z++)
            {
                for (int x = 0; x <= _gridResolution; x++)
                {
                    vertices[vertIndex] = new Vector3(
                        -halfSize + x * step,
                        0,
                        -halfSize + z * step
                    );
                    uvs[vertIndex] = new Vector2(
                        (float)x / _gridResolution,
                        (float)z / _gridResolution
                    );
                    vertIndex++;
                }
            }

            int triIndex = 0;
            for (int gz = 0; gz < _gridResolution; gz++)
            {
                for (int gx = 0; gx < _gridResolution; gx++)
                {
                    int a = gz * (_gridResolution + 1) + gx;
                    int b = a + 1;
                    int c = a + _gridResolution + 1;
                    int d = c + 1;

                    triangles[triIndex++] = a;
                    triangles[triIndex++] = c;
                    triangles[triIndex++] = b;

                    triangles[triIndex++] = b;
                    triangles[triIndex++] = c;
                    triangles[triIndex++] = d;
                }
            }

            _oceanMesh.vertices = vertices;
            _oceanMesh.uv = uvs;
            _oceanMesh.triangles = triangles;
            _oceanMesh.RecalculateBounds();
            _oceanMesh.RecalculateNormals();

            mf.mesh = _oceanMesh;
            mr.material = _oceanMaterial;
        }

        private void Update()
        {
            if (_mainCamera == null) return;

            _props.SetFloat("_Time", Time.time);
            _props.SetVector("_WorldSpaceCameraPos", _mainCamera.transform.position);

            float dist = Vector3.Distance(_mainCamera.transform.position, transform.position);
            float lodFactor = Mathf.Clamp01(dist / _lod2Distance);

            _props.SetFloat("_LODDistance", Mathf.Lerp(_lod0Distance, _lod2Distance, lodFactor));
            _props.SetFloat("_MobileFallback", _mobileFallback || (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && lodFactor > 0.5f) ? 1f : 0f);

            _props.SetFloat("_WaveHeight", Mathf.Lerp(2f, 0.5f, lodFactor));
            _props.SetFloat("_FoamThreshold", Mathf.Lerp(0.4f, 0.6f, lodFactor));

            Graphics.SetPropertyBlock(_props, GetComponent<MeshRenderer>());
        }

        private void ApplyQualitySettings()
        {
            if (_oceanMaterial == null) return;

            _oceanMaterial.EnableKeyword(_enableCaustics ? "_CAUSTICS_ON" : "_CAUSTICS_OFF");
            _oceanMaterial.EnableKeyword(_enableFoam ? "_FOAM_ON" : "_FOAM_OFF");

            int qualityLevel = QualitySettings.GetQualityLevel();
            _mobileFallback = (qualityLevel == 0);
        }

        public void SetQualityLevel(int level)
        {
            _enableCaustics = level >= 2;
            _enableFoam = level >= 1;
            _mobileFallback = level == 0;
            ApplyQualitySettings();
        }

        private void OnValidate()
        {
            if (_oceanMesh != null)
            {
                GenerateOceanMesh();
            }
        }

        private void OnDestroy()
        {
            if (_oceanMesh != null)
            {
                DestroyImmediate(_oceanMesh);
            }
        }
    }
}
