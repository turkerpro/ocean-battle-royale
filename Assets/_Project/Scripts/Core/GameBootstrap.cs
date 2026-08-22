using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace OceanBattleRoyale.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private static Camera _cam;
        private static Transform[] _ships;
        private static int _localIdx = 0;
        private static int _botCount = 30;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string scene = SceneManager.GetActiveScene().name;
            Debug.Log("[GameBootstrap] Scene: " + scene);

            if (scene == "MainMenu")
            {
                var go = new GameObject("MenuBoot");
                go.AddComponent<MenuBoot>();
            }
            else if (scene == "Prototype")
            {
                var go = new GameObject("GameBoot");
                go.AddComponent<GameBoot>();
            }
        }

        public class MenuBoot : MonoBehaviour
        {
            private void Start()
            {
                Debug.Log("[MenuBoot] Starting");
                DontDestroyOnLoad(gameObject);

                if (FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length == 0)
                {
                    var es = new GameObject("EventSystem");
                    es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                var canvasGO = new GameObject("Canvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                MakeButton(canvasGO.transform, "PlayBtn", new Color(0.2f, 0.6f, 0.3f),
                    new Vector2(0, -20), new Vector2(200, 50), () =>
                    {
                        SceneManager.LoadScene("Prototype");
                    });

                MakeButton(canvasGO.transform, "FSBtn", new Color(0.3f, 0.3f, 0.5f),
                    new Vector2(0, -80), new Vector2(200, 45), () =>
                    {
                        Screen.fullScreen = !Screen.fullScreen;
                    });

                Debug.Log("[MenuBoot] UI created");
            }

            private void MakeButton(Transform parent, string name, Color color,
                Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.AddComponent<Image>().color = color;
                var btn = go.AddComponent<Button>();
                btn.targetGraphic = go.GetComponent<Image>();
                btn.onClick.AddListener(onClick);
                var r = go.GetComponent<RectTransform>();
                r.anchoredPosition = pos;
                r.sizeDelta = size;
            }
        }

        public class GameBoot : MonoBehaviour
        {
            private void Start()
            {
                Debug.Log("[GameBoot] Starting");
                DontDestroyOnLoad(gameObject);

                CreateWater();
                CreateShips();
                SetupCamera();
                CreateInfo();

                Debug.Log("[GameBoot] Done, ships: " + _ships.Length);
            }

            private void CreateWater()
            {
                var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.name = "Water";
                plane.transform.localScale = new Vector3(100, 1, 100);

                var mr = plane.GetComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.06f, 0.22f, 0.5f);
                mr.material = mat;
            }

            private void CreateShips()
            {
                _ships = new Transform[_botCount];

                for (int i = 0; i < _botCount; i++)
                {
                    bool isPlayer = (i == _localIdx);
                    var ship = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    ship.name = isPlayer ? "Player" : "Bot_" + i;
                    ship.transform.position = new Vector3(
                        Random.Range(-400f, 400f), 1f,
                        Random.Range(-400f, 400f));
                    ship.transform.localScale = new Vector3(2f, 1f, 4f);

                    var r = ship.GetComponent<MeshRenderer>();
                    r.material = new Material(Shader.Find("Standard"));
                    r.material.color = isPlayer
                        ? new Color(0.1f, 0.8f, 0.2f)
                        : new Color(0.8f, 0.2f, 0.15f);

                    var rb = ship.AddComponent<Rigidbody>();
                    rb.useGravity = false;
                    rb.constraints = RigidbodyConstraints.FreezePositionY
                                   | RigidbodyConstraints.FreezeRotationX
                                   | RigidbodyConstraints.FreezeRotationZ;

                    ship.AddComponent<ShipBrain>();

                    _ships[i] = ship.transform;
                }
            }

            private void SetupCamera()
            {
                _cam = Camera.main;
                if (_cam == null)
                {
                    var go = new GameObject("Cam");
                    _cam = go.AddComponent<Camera>();
                    go.AddComponent<AudioListener>();
                }
                _cam.transform.position = new Vector3(0, 50, -30);
                _cam.transform.rotation = Quaternion.Euler(50, 0, 0);
                _cam.clearFlags = CameraClearFlags.SolidColor;
                _cam.backgroundColor = new Color(0.04f, 0.12f, 0.3f);
            }

            private void CreateInfo()
            {
                var canvasGO = new GameObject("HUD");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();

                var txtGO = new GameObject("InfoText");
                txtGO.transform.SetParent(canvasGO.transform, false);
                var txt = txtGO.AddComponent<Text>();
                txt.text = "WASD: Move | Click to lock mouse";
                txt.fontSize = 14;
                txt.color = new Color(1, 1, 1, 0.7f);
                txt.alignment = TextAnchor.UpperCenter;
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                var r = txtGO.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0, 1);
                r.anchorMax = new Vector2(1, 1);
                r.anchoredPosition = Vector2.zero;
                r.sizeDelta = new Vector2(0, 30);
            }

            private void Update()
            {
                if (_ships == null || _ships[_localIdx] == null) return;

                HandleInput();
                FollowPlayer();
            }

            private void HandleInput()
            {
                var t = _ships[_localIdx];
                float spd = 20f;
                float rot = 120f;

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    t.position += t.forward * spd * Time.deltaTime;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    t.position -= t.forward * spd * 0.5f * Time.deltaTime;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    t.Rotate(Vector3.up, -rot * Time.deltaTime);
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    t.Rotate(Vector3.up, rot * Time.deltaTime);

                var p = t.position;
                p.y = 1f;
                t.position = p;
            }

            private void FollowPlayer()
            {
                var t = _ships[_localIdx];
                if (t == null || _cam == null) return;
                _cam.transform.position = t.position + new Vector3(0, 45, -25);
                _cam.transform.LookAt(t.position);
            }
        }

        public class ShipBrain : MonoBehaviour
        {
            private Vector3 _target;
            private float _nextDir;
            private float _speed = 8f;

            private void Start()
            {
                _target = transform.position + Random.insideUnitSphere * 80f;
                _target.y = 1f;
                _nextDir = Time.time + Random.Range(2f, 6f);
            }

            private void Update()
            {
                if (gameObject.name == "Player") return;

                if (Time.time > _nextDir)
                {
                    _target = transform.position + new Vector3(
                        Random.Range(-150f, 150f), 1f, Random.Range(-150f, 150f));
                    _nextDir = Time.time + Random.Range(3f, 8f);
                }

                var dir = (_target - transform.position);
                dir.y = 0;
                if (dir.magnitude > 3f)
                {
                    transform.position += dir.normalized * _speed * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), Time.deltaTime * 3f);
                }
            }
        }
    }
}
