using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Room
{
    public string name;
    public string unit;
    public float height;
    public List<Wall> walls;
}

[System.Serializable]
public class Wall
{
    public string name;
    public float length;
    public float angle_from_segment_point;
    public List<Item> items;
}

[System.Serializable]
public class Item
{
    public string type;
    public float width;
    public float height;
    public float margin_from_left;
    public float margin_from_right;
    public float margin_from_bottom;
    public float margin_from_top;
}

public class RoomClass : MonoBehaviour
{
    public Room room;
    public string filePath;
    public string exportPath;

    private void Start()
    {
        exportPath = Application.dataPath + "/Resources/export.json";
        LoadRoomFromJsonFile(filePath);
    }

    // Method to load the JSON from a file and parse it into a Room object
    public void LoadRoomFromJsonFile(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            room = JsonUtility.FromJson<RoomWrapper>(json).room;
            Debug.Log("Room loaded successfully.");
        }
        catch (IOException e)
        {
            Debug.LogError("Error reading file: " + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing JSON: " + e.Message);
        }
    }

    public void ExportRoomToJsonFile()
    {
        try
        {
            RoomWrapper wrapper = new RoomWrapper { room = room };
            string json = JsonUtility.ToJson(wrapper, true); // Pretty print
            File.WriteAllText(exportPath, json);
            Debug.Log("Room exported successfully to " + exportPath);
        }
        catch (IOException e)
        {
            Debug.LogError("Error writing file: " + e.Message);
        }
    }
}


[System.Serializable]
public class RoomWrapper
{
    public Room room;
}

