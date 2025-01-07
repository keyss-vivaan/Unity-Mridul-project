using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
 
public class RoomController : MonoBehaviour
{
    private GameObject room;
    public Material wallMaterial;
    public Material doorMaterial;
    public Material windowMaterial;
    public Material floorMaterial;
    public Material acMaterial; // Material for the air conditioner
    public Camera mainCamera;
 
    private List<GameObject> walls;
 
    private float roomWidth;
    private float roomLength;
    private float roomHeight;
 
    public float rotationSpeed = 15.0f;
    private float rotationY = 0.0f;
 
    private bool isUserControlling = false;
    public float mouseSensitivity = 100.0f;
    public float xAxisSensitivity = 10.0f;

     public void ThreeDdata(string jsonData)
    {
        SingletonExample.Instance.JsonData = jsonData;
    }
    
    void Start()
    {    
            if(SingletonExample.Instance.JsonData != "hi"){
                mainCamera = Camera.main;   
             UpdateRoomDimensions();
             SingletonExample.Instance.JsonData = "hi";
             Cursor.lockState = CursorLockMode.Locked;
            } 
       
    }
 
    void Update()
    {
        HandleMouseInput();
 
        if (!isUserControlling)
        {
            rotationY += rotationSpeed * Time.deltaTime;
            rotationY %= 360.0f;
        }
 
        if (room != null)
        {
            room.transform.localRotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
        }
 
        AdjustCameraToFitRoom();
    }
 
    void CreateRoom(Dictionary<string, List<Item>> itemsPerWall)
    {
        if (room != null)
        {
            Destroy(room);
        }
 
        room = new GameObject("Room");
        walls = new List<GameObject>();
 
        // Create walls
        walls.Add(CreateWall(new Vector3(0, roomHeight / 2, roomLength / 2), new Vector3(roomWidth, roomHeight, 0.15f), wallMaterial));
        walls.Add(CreateWall(new Vector3(0, roomHeight / 2, -roomLength / 2), new Vector3(roomWidth, roomHeight, 0.15f), wallMaterial));
        walls.Add(CreateWall(new Vector3(roomWidth / 2, roomHeight / 2, 0), new Vector3(0.15f, roomHeight, roomLength), wallMaterial));
        walls.Add(CreateWall(new Vector3(-roomWidth / 2, roomHeight / 2, 0), new Vector3(0.15f, roomHeight, roomLength), wallMaterial));
 
        CreateWall(new Vector3(0, 0, 0), new Vector3(roomWidth, 0.1f, roomLength), floorMaterial);
 
        foreach (var wallData in itemsPerWall)
        {
            foreach (var item in wallData.Value)
            {
                float itemWidth = item.Width;
                float itemHeight = item.Height;
                float marginLeft = item.MarginFromLeft;
                float marginBottom = item.MarginFromBottom;
 
                if (item.Label == "door")
                {
                    CreateDoor(itemWidth, itemHeight, marginLeft, marginBottom, wallData.Key);
                }
                else if (item.Label == "window")
                {
                    CreateWindow(itemWidth, itemHeight, marginLeft, marginBottom, wallData.Key);
                }
                else if (item.Label == "ac")
                {
                    CreateAC(itemWidth, itemHeight, marginLeft, marginBottom, wallData.Key);
                }
            }
        }
 
        AdjustCameraToFitRoom();
    }
 
    GameObject CreateWall(Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.parent = room.transform;
 
        Renderer wallRenderer = wall.GetComponent<Renderer>();
        wallRenderer.material = material;
 
        return wall;
    }
 
    void CreateDoor(float width, float height, float marginLeft, float marginBottom, string wall)
    {
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.transform.localScale = new Vector3(width, height, 0.5f);
        door.GetComponent<Renderer>().material = doorMaterial;
 
        PositionItem(door, width, height, marginLeft, marginBottom, wall, isWallItem: true);
    }
 
    void CreateWindow(float width, float height, float marginLeft, float marginBottom, string wall)
    {
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.transform.localScale = new Vector3(width, height, 0.5f);
        window.GetComponent<Renderer>().material = windowMaterial;
 
        PositionItem(window, width, height, marginLeft, marginBottom, wall, isWallItem: true);
    }
 
    void CreateAC(float width, float height, float marginLeft, float marginBottom, string wall)
    {
        GameObject ac = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ac.transform.localScale = new Vector3(width, height, 0.5f);
        ac.GetComponent<Renderer>().material = acMaterial;
 
        PositionItem(ac, width, height, marginLeft, marginBottom, wall, isWallItem: true); // Place AC on walls
    }
 
    void PositionItem(GameObject item, float itemWidth, float itemHeight, float marginLeft, float marginBottom, string wall, bool isWallItem)
    {
        if (isWallItem)
        {
            float wallThickness = 0.25f; // Adjust based on your wall thickness
 
            switch (wall.ToLower())
            {
                case "south":
                    item.transform.position = new Vector3(marginLeft - roomWidth / 2 + itemWidth / 2, marginBottom + itemHeight / 2, roomLength / 2 - wallThickness / 2);
                    item.transform.rotation = Quaternion.identity;
                    item.transform.parent = walls[0].transform;
                    break;
                case "north":
                    item.transform.position = new Vector3(marginLeft - roomWidth / 2 + itemWidth / 2, marginBottom + itemHeight / 2, -roomLength / 2 + wallThickness / 2);
                    item.transform.rotation = Quaternion.Euler(0, 180, 0);
                    item.transform.parent = walls[1].transform;
                    break;
                case "west":
                    item.transform.position = new Vector3(roomWidth / 2 - wallThickness / 2, marginBottom + itemHeight / 2, marginLeft - roomLength / 2 + itemWidth / 2);
                    item.transform.rotation = Quaternion.Euler(0, 90, 0);
                    item.transform.parent = walls[2].transform;
                    break;
                case "east":
                    item.transform.position = new Vector3(-roomWidth / 2 + wallThickness / 2, marginBottom + itemHeight / 2, marginLeft - roomLength / 2 + itemWidth / 2);
                    item.transform.rotation = Quaternion.Euler(0, -90, 0);
                    item.transform.parent = walls[3].transform;
                    break;
            }
        }
    }
 
    void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
       
        if (Mathf.Abs(mouseX) > 0.1f)
        {
            isUserControlling = true;
            rotationY += mouseX;
        }
        else
        {
            isUserControlling = false;
        }
    }
 
    void AdjustCameraToFitRoom()
    {
        if (mainCamera == null || room == null)
            return;
 
        float maxDimension = Mathf.Max(roomWidth, roomLength, roomHeight);
        float distance = maxDimension * 1.7f; // Adjust this multiplier as needed

        // Set camera position and rotation
        mainCamera.transform.position = new Vector3(0, roomHeight / 2, -distance);
        mainCamera.transform.LookAt(new Vector3(0, roomHeight / 2, 0));
    }
 
public void UpdateRoomDimensions()
{
    var jsonObject = JObject.Parse(SingletonExample.Instance.JsonData);

    // Multiply room dimensions by 10
    roomWidth = (float)jsonObject["width"] * 10;
    roomLength = (float)jsonObject["length"] * 10;
    roomHeight = (float)jsonObject["height"] * 10;

    var itemsPerWall = new Dictionary<string, List<Item>>();

    foreach (var wallName in new string[] { "north", "south", "east", "west" })
    {
        var wallData = jsonObject[wallName];
        if (wallData != null)
        {
            var items = new List<Item>();

            var doors = wallData["door"];
            foreach (var door in doors)
            {
                // Multiply door dimensions and positions by 10
                items.Add(new Item
                {
                    Width = (float)door["width"] * 10,
                    Height = (float)door["height"] * 10,
                    MarginFromLeft = (float)door["left"] * 10,
                    MarginFromBottom = (float)door["bottom"] * 10,
                    Wall = wallName,
                    Label = "door"
                });
            }

            var windows = wallData["window"];
            foreach (var window in windows)
            {
                // Multiply window dimensions and positions by 10
                items.Add(new Item
                {
                    Width = (float)window["width"] * 10,
                    Height = (float)window["height"] * 10,
                    MarginFromLeft = (float)window["left"] * 10,
                    MarginFromBottom = (float)window["bottom"] * 10,
                    Wall = wallName,
                    Label = "window"
                });
            }

            var acs = wallData["ac"];
            foreach (var ac in acs)
            {
                // Multiply AC dimensions and positions by 10
                items.Add(new Item
                {
                    Width = (float)ac["width"] * 10,
                    Height = (float)ac["height"] * 10,
                    MarginFromLeft = (float)ac["left"] * 10,
                    MarginFromBottom = (float)ac["bottom"] * 10,
                    Wall = wallName,
                    Label = "ac"
                });
            }

            itemsPerWall[wallName] = items;
        }
    }

    CreateRoom(itemsPerWall);
}

 
    private class Item
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float MarginFromLeft { get; set; }
        public float MarginFromBottom { get; set; }
        public string Wall { get; set; }
        public string Label { get; set; }
    }
    
    public void ChangeScene(string jsonData)
    {
        // SingletonExample.Instance.JsonData = jsonData;
        SceneManager.LoadScene("Loading 1");
        // LoadARScene();
    }
     
     
    public void LoadARScene()
    {
        SceneManager.LoadScene("StateMachineScene");
    }
}