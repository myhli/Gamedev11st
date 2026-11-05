using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonMapBuilder : EditorWindow
{
    private const string ROOT_DECOR_NAME = "--- DUNGEON ENTITIES ---";

    public enum DungeonShape
    {
        TwoRoomsWithCorridor,
        SingleEnclosedRoom
    }

    [Header("Sprite Selection")]
    [SerializeField] private Sprite floorSprite;
    [SerializeField] private Sprite wallSprite;

    [Header("Generation Settings")]
    [SerializeField] private DungeonShape shape = DungeonShape.TwoRoomsWithCorridor;
    [SerializeField] private bool spawnCoinsAndEnemies = true;
    [SerializeField] private int roomWidth = 20;
    [SerializeField] private int roomHeight = 12;

    [MenuItem("Tools/Dungeon Generator/Open Dungeon Window...", false, 0)]
    public static void ShowWindow()
    {
        DungeonMapBuilder window = GetWindow<DungeonMapBuilder>("Dungeon Generator");
        window.minSize = new Vector2(360, 420);
        window.InitSprites();
    }

    [MenuItem("Tools/Dungeon Generator/Generate Dungeon Map", false, 1)]
    public static void MenuGenerateMap()
    {
        DungeonMapBuilder window = GetWindow<DungeonMapBuilder>("Dungeon Generator");
        window.InitSprites();
        window.ExecuteGeneration();
    }

    [MenuItem("Tools/Dungeon Generator/Clear Generated Map", false, 2)]
    public static void MenuClearMap()
    {
        ClearDungeon();
    }

    private void OnEnable()
    {
        InitSprites();
    }

    public void InitSprites()
    {
        if (floorSprite != null && wallSprite != null) return;

        string tilesetPath = FindTilesetPath();
        if (string.IsNullOrEmpty(tilesetPath)) return;

        // Requirement 1: Load sub-sprites using AssetDatabase.LoadAllAssetsAtPath().OfType<Sprite>()
        Sprite[] subSprites = AssetDatabase.LoadAllAssetsAtPath(tilesetPath)
            .OfType<Sprite>()
            .ToArray();

        if (subSprites.Length > 0)
        {
            if (floorSprite == null)
            {
                // Pilih sub-sprite lantai yang representatif
                floorSprite = subSprites.Length > 15 ? subSprites[15] : subSprites[0];
            }

            if (wallSprite == null)
            {
                // Pilih sub-sprite tembok solid
                wallSprite = subSprites.Length > 5 ? subSprites[5] : subSprites[subSprites.Length - 1];
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("🏰 Dungeon Map Generator (Tilemap)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Membangun layout ruangan dungeon menggunakan Grid & 2 Layer Tilemap terpisah (Floor_Tilemap & Walls_Tilemap ber-collider).",
            MessageType.Info
        );

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Tileset & Sprite Selection", EditorStyles.boldLabel);

        // Requirement 4: EditorGUILayout.ObjectField untuk 'Floor Sprite' dan 'Wall Sprite'
        EditorGUI.BeginChangeCheck();
        floorSprite = (Sprite)EditorGUILayout.ObjectField("Floor Sprite", floorSprite, typeof(Sprite), false);
        wallSprite = (Sprite)EditorGUILayout.ObjectField("Wall Sprite", wallSprite, typeof(Sprite), false);
        if (EditorGUI.EndChangeCheck())
        {
            Repaint();
        }

        if (GUILayout.Button("🔄 Auto-Detect Sub-Sprites dari Set 1", GUILayout.Height(24)))
        {
            floorSprite = null;
            wallSprite = null;
            InitSprites();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Layout Configuration", EditorStyles.boldLabel);
        shape = (DungeonShape)EditorGUILayout.EnumPopup("Dungeon Layout", shape);
        if (shape == DungeonShape.SingleEnclosedRoom)
        {
            roomWidth = EditorGUILayout.IntSlider("Room Width", roomWidth, 10, 40);
            roomHeight = EditorGUILayout.IntSlider("Room Height", roomHeight, 8, 30);
        }

        spawnCoinsAndEnemies = EditorGUILayout.Toggle("Spawn Coins & Enemies", spawnCoinsAndEnemies);

        GUILayout.Space(15);
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
        if (GUILayout.Button("🏰 Generate Dungeon Map", GUILayout.Height(40)))
        {
            ExecuteGeneration();
        }

        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        GUILayout.Space(5);
        if (GUILayout.Button("🗑️ Clear Tilemap & Entities", GUILayout.Height(30)))
        {
            ClearDungeon();
        }
        GUI.backgroundColor = Color.white;
    }

    public void ExecuteGeneration()
    {
        InitSprites();

        if (floorSprite == null || wallSprite == null)
        {
            EditorUtility.DisplayDialog("Sprite Kosong", "Pastikan Floor Sprite dan Wall Sprite sudah terisi sebelum generate.", "OK");
            return;
        }

        // 1. Dapatkan atau buat Grid induk
        Grid grid = GetOrCreateGrid();

        // 2. Dapatkan atau buat Floor_Tilemap dan Walls_Tilemap
        Tilemap floorTilemap = GetOrCreateFloorTilemap(grid);
        Tilemap wallsTilemap = GetOrCreateWallsTilemap(grid);

        // 3. Bersihkan tilemap terlebih dahulu
        floorTilemap.ClearAllTiles();
        wallsTilemap.ClearAllTiles();

        // 4. Buat runtime Tile instance
        Tile floorTile = ScriptableObject.CreateInstance<Tile>();
        floorTile.sprite = floorSprite;

        Tile wallTile = ScriptableObject.CreateInstance<Tile>();
        wallTile.sprite = wallSprite;

        // 5. Bangun layout dungeon
        if (shape == DungeonShape.TwoRoomsWithCorridor)
        {
            BuildTwoRoomsDungeon(floorTilemap, wallsTilemap, floorTile, wallTile);
        }
        else
        {
            BuildSingleRoomDungeon(floorTilemap, wallsTilemap, floorTile, wallTile, roomWidth, roomHeight);
        }

        // 6. Update Composite Collider geometry
        CompositeCollider2D compCol = wallsTilemap.GetComponent<CompositeCollider2D>();
        if (compCol != null)
        {
            compCol.GenerateGeometry();
        }

        // 7. Spawn Coins, Enemies & Deactivate Old Stretched Walls
        if (spawnCoinsAndEnemies)
        {
            SpawnEntities();
        }

        // 8. Tandai scene dirty agar bisa disimpan
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Selection.activeGameObject = grid.gameObject;
        Debug.Log("✅ Dungeon Tilemap Berhasil Dibangun Menggunakan Sub-Sprite Set 1!");
    }

    public static void ClearDungeon()
    {
        Grid grid = Object.FindAnyObjectByType<Grid>();
        if (grid != null)
        {
            Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
            foreach (Tilemap tm in tilemaps)
            {
                Undo.RecordObject(tm, "Clear Tilemap");
                tm.ClearAllTiles();
            }
        }

        GameObject entities = GameObject.Find(ROOT_DECOR_NAME);
        if (entities != null)
        {
            Undo.DestroyObjectImmediate(entities);
        }

        Debug.Log("Dungeon Tilemap & Entities telah dibersihkan.");
    }

    private static string FindTilesetPath()
    {
        string[] candidates = new string[]
        {
            "Assets/Sprites/Dungeon Gathering Free Version/Set 1.png",
            "Assets/Sprites/Dungeon Gathering Free Version/Set 1.0.png"
        };

        foreach (string path in candidates)
        {
            if (System.IO.File.Exists(path)) return path;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D Set 1");
        if (guids != null && guids.Length > 0)
        {
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        return "Assets/Sprites/Dungeon Gathering Free Version/Set 1.png";
    }

    private static Grid GetOrCreateGrid()
    {
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj == null)
        {
            gridObj = new GameObject("Grid");
            Undo.RegisterCreatedObjectUndo(gridObj, "Create Grid");
        }

        Grid grid = gridObj.GetComponent<Grid>();
        if (grid == null)
        {
            grid = gridObj.AddComponent<Grid>();
        }
        grid.cellSize = new Vector3(1f, 1f, 0f);
        return grid;
    }

    private static Tilemap GetOrCreateFloorTilemap(Grid grid)
    {
        Transform t = grid.transform.Find("Floor_Tilemap");
        GameObject obj;
        if (t != null)
        {
            obj = t.gameObject;
        }
        else
        {
            obj = new GameObject("Floor_Tilemap");
            obj.transform.SetParent(grid.transform);
            obj.transform.localPosition = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(obj, "Create Floor_Tilemap");
        }

        Tilemap tm = obj.GetComponent<Tilemap>();
        if (tm == null) tm = obj.AddComponent<Tilemap>();

        TilemapRenderer tr = obj.GetComponent<TilemapRenderer>();
        if (tr == null) tr = obj.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 0;

        return tm;
    }

    private static Tilemap GetOrCreateWallsTilemap(Grid grid)
    {
        Transform t = grid.transform.Find("Walls_Tilemap");
        GameObject obj;
        if (t != null)
        {
            obj = t.gameObject;
        }
        else
        {
            obj = new GameObject("Walls_Tilemap");
            obj.transform.SetParent(grid.transform);
            obj.transform.localPosition = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(obj, "Create Walls_Tilemap");
        }

        Tilemap tm = obj.GetComponent<Tilemap>();
        if (tm == null) tm = obj.AddComponent<Tilemap>();

        TilemapRenderer tr = obj.GetComponent<TilemapRenderer>();
        if (tr == null) tr = obj.AddComponent<TilemapRenderer>();
        tr.sortingOrder = 1;

        // TilemapCollider2D & CompositeCollider2D (Rigidbody2D Static)
        TilemapCollider2D tmCol = obj.GetComponent<TilemapCollider2D>();
        if (tmCol == null) tmCol = obj.AddComponent<TilemapCollider2D>();

        CompositeCollider2D compCol = obj.GetComponent<CompositeCollider2D>();
        if (compCol == null) compCol = obj.AddComponent<CompositeCollider2D>();

        // Kompatibel dengan Unity 6
        tmCol.compositeOperation = Collider2D.CompositeOperation.Merge;

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        return tm;
    }

    private static void BuildTwoRoomsDungeon(Tilemap floorTm, Tilemap wallsTm, Tile floorTile, Tile wallTile)
    {
        // Ruangan 1 (Spawn Chamber): X [-18, -12], Y [-5, 5]
        // Lorong (Corridor): X [-11, -7], Y [-1, 1]
        // Ruangan 2 (Treasure / Boss): X [-6, 0], Y [-6, 6]

        HashSet<Vector3Int> floorPositions = new HashSet<Vector3Int>();
        HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>();

        // 1. Tentukan area lantai (walkable)
        // Room 1 Floor
        for (int x = -17; x <= -12; x++)
        {
            for (int y = -4; y <= 4; y++)
            {
                floorPositions.Add(new Vector3Int(x, y, 0));
            }
        }

        // Corridor Floor
        for (int x = -11; x <= -7; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                floorPositions.Add(new Vector3Int(x, y, 0));
            }
        }

        // Room 2 Floor
        for (int x = -6; x <= -1; x++)
        {
            for (int y = -5; y <= 5; y++)
            {
                floorPositions.Add(new Vector3Int(x, y, 0));
            }
        }

        // 2. Tentukan area dinding (sekeliling tepi lantai)
        Vector3Int[] neighbors = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, -1, 0)
        };

        foreach (Vector3Int floorPos in floorPositions)
        {
            foreach (Vector3Int offset in neighbors)
            {
                Vector3Int checkPos = floorPos + offset;
                if (!floorPositions.Contains(checkPos))
                {
                    wallPositions.Add(checkPos);
                }
            }
        }

        // 3. Pasang tiles ke tilemap
        foreach (Vector3Int pos in floorPositions)
        {
            floorTm.SetTile(pos, floorTile);
        }

        foreach (Vector3Int pos in wallPositions)
        {
            wallsTm.SetTile(pos, wallTile);
        }
    }

    private static void BuildSingleRoomDungeon(Tilemap floorTm, Tilemap wallsTm, Tile floorTile, Tile wallTile, int width, int height)
    {
        int minX = -10 - (width / 2);
        int maxX = minX + width - 1;
        int minY = -(height / 2);
        int maxY = minY + height - 1;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (x == minX || x == maxX || y == minY || y == maxY)
                {
                    wallsTm.SetTile(pos, wallTile);
                }
                else
                {
                    floorTm.SetTile(pos, floorTile);
                }
            }
        }
    }

    private static void SpawnEntities()
    {
        // Bersihkan entities lama
        GameObject existing = GameObject.Find(ROOT_DECOR_NAME);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject root = new GameObject(ROOT_DECOR_NAME);
        Undo.RegisterCreatedObjectUndo(root, "Spawn Dungeon Entities");

        GameObject coinContainer = new GameObject("Coins");
        coinContainer.transform.SetParent(root.transform);

        GameObject enemyContainer = new GameObject("Enemies");
        enemyContainer.transform.SetParent(root.transform);

        GameObject decorContainer = new GameObject("Decorations");
        decorContainer.transform.SetParent(root.transform);

        // Assets
        Sprite coinSprite = LoadSubSprite("Assets/Sprites/Dungeon Gathering Free Version/Coin Sheet.png", "Coin Sheet_0");
        Material coinMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/MaterialKoin.mat");

        Sprite orcSprite = LoadSubSprite("Assets/Sprites/Characters(100x100 split)/Orc/Orc/Orc_Idle.png", "Orc_Idle_0");
        Sprite torchSprite = LoadSubSprite("Assets/Sprites/Dungeon Gathering Free Version/Torch Yellow.png", "Torch Yellow_0");
        Sprite vaseSprite = LoadSubSprite("Assets/Sprites/Dungeon Gathering Free Version/Vase Shine Anim.png", "Vase Shine Anim_0");
        Sprite potionSprite = LoadSubSprite("Assets/Sprites/Dungeon Gathering Free Version/Potion 1.png", "Potion 1_0");

        // 1. Koin di lorong (-11 s/d -7)
        for (int x = -11; x <= -7; x++)
        {
            CreateCoin($"Coin_Corridor_{x}", new Vector2(x + 0.5f, 0.5f), coinSprite, coinMat, coinContainer.transform);
        }

        // 2. Koin melingkar di Ruang 2 (pusat di (-3.5, 0.5))
        Vector2 centerR2 = new Vector2(-3.5f, 0.5f);
        int ringCount = 8;
        float radius = 1.6f;
        for (int i = 0; i < ringCount; i++)
        {
            float angle = i * (Mathf.PI * 2f / ringCount);
            Vector2 p = centerR2 + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            CreateCoin($"Coin_Ring_{i + 1}", p, coinSprite, coinMat, coinContainer.transform);
        }
        CreateCoin("Coin_Treasure_Center", centerR2, coinSprite, coinMat, coinContainer.transform, 1.3f);

        // 3. Orc Enmies
        CreateOrc("Orc_Corridor_Sentry", new Vector2(-7.5f, 0.5f), orcSprite, enemyContainer.transform, 2.0f);
        CreateOrc("Orc_North_Guard", new Vector2(-3.5f, 3.5f), orcSprite, enemyContainer.transform, 2.4f);
        CreateOrc("Orc_South_Guard", new Vector2(-3.5f, -2.5f), orcSprite, enemyContainer.transform, 2.4f);

        // 4. Dekorasi Obor & Guci
        if (torchSprite != null)
        {
            CreateDecor("Torch_Corr_N", new Vector2(-9.0f, 1.8f), torchSprite, decorContainer.transform, 1.0f);
            CreateDecor("Torch_Corr_S", new Vector2(-9.0f, -0.8f), torchSprite, decorContainer.transform, 1.0f);
            CreateDecor("Torch_R1", new Vector2(-15.0f, 4.5f), torchSprite, decorContainer.transform, 1.2f);
            CreateDecor("Torch_R2", new Vector2(-3.5f, 5.5f), torchSprite, decorContainer.transform, 1.2f);
        }

        if (vaseSprite != null)
        {
            CreateDecor("Vase_R1", new Vector2(-16.5f, -3.5f), vaseSprite, decorContainer.transform, 1.5f);
            CreateDecor("Vase_R2", new Vector2(-1.5f, -4.5f), vaseSprite, decorContainer.transform, 1.5f);
        }

        if (potionSprite != null)
        {
            CreateDecor("Potion_Collectible", new Vector2(-16.5f, 3.5f), potionSprite, decorContainer.transform, 1.2f);
        }

        // 5. Posisikan Player ke Ruang 1 (Spawn)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Undo.RecordObject(player.transform, "Position Player");
            player.transform.position = new Vector3(-15.0f, 0.5f, 0f);
        }

        // 6. Matikan dinding dan koin lama yang berantakan
        DeactivateOldObject("wall");
        DeactivateOldObject("wall (1)");
        DeactivateOldObject("wall (2)");
        DeactivateOldObject("wall (3)");
        DeactivateOldObject("--- DUNGEON MAP ---");
        DeactivateOldObject("Coin");
        DeactivateOldObject("Coin (1)");
        DeactivateOldObject("Coin (2)");
        DeactivateOldObject("Coin (3)");
        DeactivateOldObject("Coin (4)");

        // Nonaktifkan orc lama yang tidak terkelompok
        GameObject[] oldOrcs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var obj in oldOrcs)
        {
            if (obj.name == "Orc" && obj.transform.parent == null)
            {
                Undo.RecordObject(obj, "Deactivate Old Orc");
                obj.SetActive(false);
            }
        }
    }

    private static Sprite LoadSubSprite(string assetPath, string spriteName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object obj in assets)
        {
            if (obj is Sprite s && s.name == spriteName) return s;
        }
        return null;
    }

    private static void CreateCoin(string name, Vector2 pos, Sprite sprite, Material mat, Transform parent, float scale = 1f)
    {
        GameObject coin = new GameObject(name);
        coin.tag = "Coin";
        coin.transform.SetParent(parent);
        coin.transform.position = new Vector3(pos.x, pos.y, 0f);
        coin.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = coin.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (mat != null) sr.material = mat;
        sr.sortingOrder = 2;

        CircleCollider2D col = coin.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;
    }

    private static void CreateOrc(string name, Vector2 pos, Sprite sprite, Transform parent, float speed)
    {
        GameObject orc = new GameObject(name);
        orc.tag = "Enemy";
        orc.layer = 6;
        orc.transform.SetParent(parent);
        orc.transform.position = new Vector3(pos.x, pos.y, 0f);

        SpriteRenderer sr = orc.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 4;

        Rigidbody2D rb = orc.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D col = orc.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.9f);

        EnemyAI ai = orc.AddComponent<EnemyAI>();
        SerializedObject soAI = new SerializedObject(ai);
        soAI.FindProperty("speed").floatValue = speed;
        soAI.FindProperty("contactDamage").intValue = 1;
        soAI.ApplyModifiedPropertiesWithoutUndo();

        orc.AddComponent<EnemyHealth>();
    }

    private static void CreateDecor(string name, Vector2 pos, Sprite sprite, Transform parent, float scale = 1f)
    {
        GameObject decor = new GameObject(name);
        decor.transform.SetParent(parent);
        decor.transform.position = new Vector3(pos.x, pos.y, 0f);
        decor.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = decor.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;
    }

    private static void DeactivateOldObject(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null && obj.transform.parent == null)
        {
            Undo.RecordObject(obj, "Deactivate Old Object");
            obj.SetActive(false);
        }
    }
}
