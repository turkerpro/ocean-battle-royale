using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace OceanBattleRoyale.Editor
{
    public class SceneSetup : EditorWindow
    {
        [MenuItem("Tools/Ocean Battle Royale/Setup Prototype Scene")]
        public static void SetupPrototypeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupLighting();
            SetupOcean();
            SetupSpawnTest();
            SetupGameManager();
            CreatePlayerShipPrefab();
            CreateBotShipPrefab();
            CreateWeaponPrefabs();
            CreateMinePrefabs();
            CreateExplosionPrefab();

            string scenePath = "Assets/_Project/Scenes/Prototype.unity";
            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            Debug.Log("[SceneSetup] Prototype scene created at: " + scenePath);
        }

        [MenuItem("Tools/Ocean Battle Royale/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupLighting();
            CreateMainMenuUI();

            string scenePath = "Assets/_Project/Scenes/MainMenu.unity";
            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            Debug.Log("[SceneSetup] MainMenu scene created at: " + scenePath);
        }

        [MenuItem("Tools/Ocean Battle Royale/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            SetupPrototypeScene();
            SetupMainMenuScene();
        }

        private static void SetupLighting()
        {
            RenderSettings.skybox = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                name = "OceanSkybox"
            };
            RenderSettings.skybox.SetColor("_BaseColor", new Color(0.4f, 0.6f, 0.8f));
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1f;

            var sunGO = new GameObject("Directional Light");
            var sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            sun.color = new Color(1f, 0.95f, 0.85f);
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            sun.shadows = LightShadows.Soft;
            sun.shadowDistance = 500f;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.3f, 0.5f, 0.7f);
            RenderSettings.fogDensity = 0.001f;
        }

        private static void SetupOcean()
        {
            var oceanGO = new GameObject("Ocean");
            oceanGO.transform.position = Vector3.zero;
            oceanGO.transform.localScale = new Vector3(2000, 1, 2000);

            var mf = oceanGO.AddComponent<MeshFilter>();
            var mr = oceanGO.AddComponent<MeshRenderer>();

            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Shaders/OceanShader.shader");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.name = "OceanMaterial";
                mat.SetColor("_Color", new Color(0.0f, 0.3f, 0.5f, 1.0f));
                mat.SetColor("_DeepColor", new Color(0.0f, 0.15f, 0.3f, 1.0f));
                mat.SetColor("_ShallowColor", new Color(0.1f, 0.4f, 0.6f, 1.0f));
                mat.SetColor("_FoamColor", new Color(1, 1, 1, 0.8f));
                mat.SetFloat("_WaveSpeed", 1.0f);
                mat.SetFloat("_WaveScale", 0.1f);
                mat.SetFloat("_WaveHeight", 2.0f);
                mat.SetFloat("_FoamThreshold", 0.5f);
                mat.SetFloat("_FresnelPower", 3.0f);
                mat.SetFloat("_ReflectionStrength", 0.3f);
                mat.SetFloat("_CausticsSpeed", 0.5f);
                mat.SetFloat("_CausticsScale", 10.0f);
                mat.SetFloat("_LODDistance", 200.0f);
                mat.SetFloat("_MobileFallback", 0.0f);
                mr.material = mat;
            }

            var oceanManager = oceanGO.AddComponent<OceanBattleRoyale.World.OceanManager>();
            var serializedManager = new SerializedObject(oceanManager);
            serializedManager.FindProperty("_oceanMaterial").objectReferenceValue = mr.material;
            serializedManager.FindProperty("_oceanSize").floatValue = 2000f;
            serializedManager.FindProperty("_gridResolution").intValue = 64;
            serializedManager.ApplyModifiedProperties();
        }

        private static void SetupSpawnTest()
        {
            var spawnGO = new GameObject("SpawnTest");
            var spawnTest = spawnGO.AddComponent<OceanBattleRoyale.World.SpawnTest>();

            var botPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Resources/Prefabs/BotShip.prefab");
            if (botPrefab == null) botPrefab = CreateBotShipPrefab();

            var serialized = new SerializedObject(spawnTest);
            serialized.FindProperty("_shipPrefab").objectReferenceValue = botPrefab;
            serialized.FindProperty("_botCount").intValue = 50;
            serialized.FindProperty("_spawnRadius").floatValue = 500f;
            serialized.FindProperty("_safeZoneRadius").floatValue = 50f;
            serialized.ApplyModifiedProperties();
        }

        private static void SetupGameManager()
        {
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<OceanBattleRoyale.Core.GameManager>();

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Resources/Prefabs/PlayerShip.prefab");
            if (playerPrefab == null) playerPrefab = CreatePlayerShipPrefab();

            var spawnTest = FindObjectOfType<OceanBattleRoyale.World.SpawnTest>();

            var serialized = new SerializedObject(gm);
            serialized.FindProperty("_playerShipPrefab").objectReferenceValue = playerPrefab;
            serialized.FindProperty("_spawnTest").objectReferenceValue = spawnTest;
            serialized.FindProperty("_matchDuration").floatValue = 600f;
            serialized.ApplyModifiedProperties();
        }

        private static GameObject CreatePlayerShipPrefab()
        {
            var prefabPath = "Assets/_Project/Resources/Prefabs/PlayerShip.prefab";
            System.IO.Directory.CreateDirectory("Assets/_Project/Resources/Prefabs");

            var shipGO = new GameObject("PlayerShip");
            shipGO.AddComponent<OceanBattleRoyale.Network.NetworkedShip>();
            shipGO.AddComponent<OceanBattleRoyale.Ship.ShipPhysics>();
            shipGO.AddComponent<OceanBattleRoyale.Network.LocalPlayerController>();
            shipGO.AddComponent<OceanBattleRoyale.Combat.WeaponSystem>();
            shipGO.AddComponent<OceanBattleRoyale.Combat.MineSystem>();
            shipGO.AddComponent<OceanBattleRoyale.Ship.ShipProgression>();
            shipGO.AddComponent<OceanBattleRoyale.Combat.Damageable>();

            var rb = shipGO.AddComponent<Rigidbody>();
            rb.mass = 5000f; rb.drag = 0.3f; rb.angularDrag = 1.5f;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var hullGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullGO.name = "Hull";
            hullGO.transform.SetParent(shipGO.transform);
            hullGO.transform.localPosition = new Vector3(0, 0.5f, 0);
            hullGO.transform.localScale = new Vector3(4f, 1f, 8f);
            var hullRenderer = hullGO.GetComponent<Renderer>();
            var hullMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            hullMat.color = new Color(0.4f, 0.3f, 0.2f);
            hullRenderer.material = hullMat;
            Object.DestroyImmediate(hullGO.GetComponent<Collider>());

            var turretGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turretGO.name = "Turret";
            turretGO.transform.SetParent(shipGO.transform);
            turretGO.transform.localPosition = new Vector3(0, 1.5f, 0);
            turretGO.transform.localScale = new Vector3(1f, 0.5f, 1f);
            var turretRenderer = turretGO.GetComponent<Renderer>();
            var turretMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            turretMat.color = new Color(0.5f, 0.4f, 0.3f);
            turretRenderer.material = turretMat;
            Object.DestroyImmediate(turretGO.GetComponent<Collider>());

            var firePointGO = new GameObject("FirePoint");
            firePointGO.transform.SetParent(shipGO.transform);
            firePointGO.transform.localPosition = new Vector3(0, 2f, 5f);

            var firePointGO2 = new GameObject("FirePoint2");
            firePointGO2.transform.SetParent(shipGO.transform);
            firePointGO2.transform.localPosition = new Vector3(0, 2f, -5f);

            var deployPointGO = new GameObject("DeployPoint");
            deployPointGO.transform.SetParent(shipGO.transform);
            deployPointGO.transform.localPosition = new Vector3(0, 0.5f, -5f);

            var trailGO = new GameObject("WakeTrail");
            trailGO.transform.SetParent(shipGO.transform);
            trailGO.transform.localPosition = new Vector3(0, 0.2f, -4f);
            var trail = trailGO.AddComponent<TrailRenderer>();
            trail.time = 2f;
            trail.startWidth = 2f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1, 1, 1, 0.3f) };

            var audio = shipGO.AddComponent<AudioSource>();
            audio.spatialBlend = 1f;
            audio.maxDistance = 50f;

            var serializedShip = new SerializedObject(shipGO.GetComponent<OceanBattleRoyale.Network.NetworkedShip>());
            serializedShip.FindProperty("_hullRenderer").objectReferenceValue = hullRenderer;
            serializedShip.FindProperty("_turretRenderer").objectReferenceValue = turretRenderer;
            serializedShip.FindProperty("_wakeTrail").objectReferenceValue = trail;
            serializedShip.ApplyModifiedProperties();

            var serializedWeapon = new SerializedObject(shipGO.GetComponent<OceanBattleRoyale.Combat.WeaponSystem>());
            serializedWeapon.FindProperty("_firePoints").arraySize = 2;
            serializedWeapon.FindProperty("_firePoints").GetArrayElementAtIndex(0).objectReferenceValue = firePointGO.transform;
            serializedWeapon.FindProperty("_firePoints").GetArrayElementAtIndex(1).objectReferenceValue = firePointGO2.transform;
            serializedWeapon.FindProperty("_audioSource").objectReferenceValue = audio;
            serializedWeapon.FindProperty("_availableWeapons").arraySize = 5;
            var defaultWeapons = OceanBattleRoyale.Combat.WeaponData.GetDefaultWeapons();
            for (int i = 0; i < defaultWeapons.Length; i++)
            {
                serializedWeapon.FindProperty("_availableWeapons").GetArrayElementAtIndex(i).objectReferenceValue = defaultWeapons[i];
            }
            serializedWeapon.ApplyModifiedProperties();

            var serializedMine = new SerializedObject(shipGO.GetComponent<OceanBattleRoyale.Combat.MineSystem>());
            serializedMine.FindProperty("_deployPoint").objectReferenceValue = deployPointGO.transform;
            serializedMine.FindProperty("_audioSource").objectReferenceValue = audio;
            serializedMine.FindProperty("_availableMines").arraySize = 4;
            var defaultMines = OceanBattleRoyale.Combat.MineData.GetDefaultMines();
            for (int i = 0; i < defaultMines.Length; i++)
            {
                serializedMine.FindProperty("_availableMines").GetArrayElementAtIndex(i).objectReferenceValue = defaultMines[i];
            }
            serializedMine.ApplyModifiedProperties();

            var serializedProgression = new SerializedObject(shipGO.GetComponent<OceanBattleRoyale.Ship.ShipProgression>());
            var defaultTiers = new OceanBattleRoyale.Ship.ShipTierData[5];
            for (int i = 0; i < 5; i++) defaultTiers[i] = OceanBattleRoyale.Ship.ShipTierData.GetDefaultTier(i + 1);
            serializedProgression.FindProperty("_tierData").arraySize = 5;
            for (int i = 0; i < 5; i++)
            {
                serializedProgression.FindProperty("_tierData").GetArrayElementAtIndex(i).objectReferenceValue = defaultTiers[i];
            }
            serializedProgression.ApplyModifiedProperties();

            var serializedDamageable = new SerializedObject(shipGO.GetComponent<OceanBattleRoyale.Combat.Damageable>());
            serializedDamageable.FindProperty("_maxHealth").floatValue = 100f;
            serializedDamageable.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(shipGO, prefabPath);
            Object.DestroyImmediate(shipGO);
            AssetDatabase.Refresh();

            return prefab;
        }

        private static GameObject CreateBotShipPrefab()
        {
            var prefabPath = "Assets/_Project/Resources/Prefabs/BotShip.prefab";
            System.IO.Directory.CreateDirectory("Assets/_Project/Resources/Prefabs");

            var shipGO = new GameObject("BotShip");
            shipGO.AddComponent<OceanBattleRoyale.Network.NetworkedShip>();
            shipGO.AddComponent<OceanBattleRoyale.Ship.ShipPhysics>();
            shipGO.AddComponent<OceanBattleRoyale.Combat.Damageable>();

            var rb = shipGO.AddComponent<Rigidbody>();
            rb.mass = 5000f; rb.drag = 0.3f; rb.angularDrag = 1.5f;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var hullGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hullGO.name = "Hull";
            hullGO.transform.SetParent(shipGO.transform);
            hullGO.transform.localPosition = new Vector3(0, 0.5f, 0);
            hullGO.transform.localScale = new Vector3(4f, 1f, 8f);
            var hullRenderer = hullGO.GetComponent<Renderer>();
            var hullMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            hullMat.color = new Color(0.3f, 0.2f, 0.4f);
            hullRenderer.material = hullMat;
            Object.DestroyImmediate(hullGO.GetComponent<Collider>());

            var turretGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turretGO.name = "Turret";
            turretGO.transform.SetParent(shipGO.transform);
            turretGO.transform.localPosition = new Vector3(0, 1.5f, 0);
            turretGO.transform.localScale = new Vector3(1f, 0.5f, 1f);
            var turretRenderer = turretGO.GetComponent<Renderer>();
            var turretMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            turretMat.color = new Color(0.4f, 0.3f, 0.5f);
            turretRenderer.material = turretMat;
            Object.DestroyImmediate(turretGO.GetComponent<Collider>());

            var serializedShip = new SerializedObject(shipGO.GetComponent<OceanBattleRoyale.Network.NetworkedShip>());
            serializedShip.FindProperty("_hullRenderer").objectReferenceValue = hullRenderer;
            serializedShip.FindProperty("_turretRenderer").objectReferenceValue = turretRenderer;
            serializedShip.ApplyModifiedProperties();

            var serializedDamageable = new SerializedObject(shipGO.GetComponent<OceanBattleRoyale.Combat.Damageable>());
            serializedDamageable.FindProperty("_maxHealth").floatValue = 100f;
            serializedDamageable.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(shipGO, prefabPath);
            Object.DestroyImmediate(shipGO);
            AssetDatabase.Refresh();

            return prefab;
        }

        private static void CreateWeaponPrefabs()
        {
            System.IO.Directory.CreateDirectory("Assets/_Project/Resources/Prefabs/Weapons");
            CreateProjectilePrefab("CannonProjectile", 0.3f, Color.yellow, 5f);
            CreateProjectilePrefab("MGProjectile", 0.1f, Color.white, 2f);
            CreateProjectilePrefab("MissileProjectile", 0.2f, Color.red, 3f);
            CreateProjectilePrefab("LaserProjectile", 0.05f, Color.cyan, 1f);
            CreateProjectilePrefab("TorpedoProjectile", 0.4f, Color.green, 4f);
        }

        private static void CreateProjectilePrefab(string name, float size, Color color, float trailTime)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite((int)(size * 100), color);
            sr.sortingOrder = 10;

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = trailTime;
            trail.startWidth = size;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default")) { color = color };

            go.AddComponent<OceanBattleRoyale.Combat.Projectile>();

            var prefabPath = "Assets/_Project/Resources/Prefabs/Weapons/" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }

        private static Sprite CreateCircleSprite(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = size / 2;
            var radius = size / 2 - 1;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                        tex.SetPixel(x, y, color);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static void CreateMinePrefabs()
        {
            System.IO.Directory.CreateDirectory("Assets/_Project/Resources/Prefabs/Mines");
            CreateMinePrefab("ContactMine", OceanBattleRoyale.Combat.MineType.Contact, Color.red, 0.5f);
            CreateMinePrefab("ProximityMine", OceanBattleRoyale.Combat.MineType.Proximity, Color.yellow, 0.6f);
            CreateMinePrefab("MagneticMine", OceanBattleRoyale.Combat.MineType.Magnetic, Color.blue, 0.5f);
            CreateMinePrefab("DriftMine", OceanBattleRoyale.Combat.MineType.Drift, Color.green, 0.4f);
        }

        private static void CreateMinePrefab(string name, OceanBattleRoyale.Combat.MineType type, Color color, float size)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite((int)(size * 100), color);
            sr.sortingOrder = 5;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = size;

            go.AddComponent<OceanBattleRoyale.Combat.Mine>();

            var prefabPath = "Assets/_Project/Resources/Prefabs/Mines/" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }

        private static void CreateExplosionPrefab()
        {
            var go = new GameObject("ExplosionEffect");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.red);
            main.gravityModifier = 0f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(Color.yellow, Color.clear);

            var prefabPath = "Assets/_Project/Resources/Prefabs/ExplosionEffect.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }

        private static void CreateMainMenuUI()
        {
            var canvasGO = new GameObject("MainMenuCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.2f, 0.3f, 1f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "OCEAN BATTLE ROYALE";
            titleText.fontSize = 72;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800, 100);

            var buttonsGO = new GameObject("Buttons");
            buttonsGO.transform.SetParent(canvasGO.transform);
            var buttonsRect = buttonsGO.GetComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.5f, 0.3f);
            buttonsRect.anchorMax = new Vector2(0.5f, 0.6f);
            buttonsRect.anchoredPosition = Vector2.zero;
            buttonsRect.sizeDelta = new Vector2(400, 300);

            CreateButton(buttonsGO, "Quick Match", 0);
            CreateButton(buttonsGO, "Create Lobby", -70);
            CreateButton(buttonsGO, "Join Lobby", -140);
            CreateButton(buttonsGO, "Settings", -210);
            CreateButton(buttonsGO, "Quit", -280);

            var lobbyManager = canvasGO.AddComponent<OceanBattleRoyale.Core.LobbyManager>();
            var serialized = new SerializedObject(lobbyManager);
            serialized.FindProperty("_mainMenuPanel").objectReferenceValue = canvasGO;
            serialized.ApplyModifiedProperties();
        }

        private static GameObject CreateButton(GameObject parent, string text, float yOffset)
        {
            var btnGO = new GameObject("Btn_" + text);
            btnGO.transform.SetParent(parent.transform);
            var btn = btnGO.AddComponent<Button>();
            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.6f);
            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0, yOffset);
            btnRect.sizeDelta = new Vector2(300, 50);

            var txtGO = new GameObject("Text");
            txtGO.transform.SetParent(btnGO.transform);
            var txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = 24;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            var txtRect = txtGO.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            return btnGO;
        }
    }
}
