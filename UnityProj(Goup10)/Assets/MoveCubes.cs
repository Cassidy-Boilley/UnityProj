using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Globalization;
using NetworkAPI;

public class MoveCubes : MonoBehaviour
{
    NetworkComm networkComm;
    public GameObject localCube, remoteCube;
    public Vector3 localCubePos = new Vector3();
    public Vector3 remoteCubePos = new Vector3();
    private float baseMove = 0.05f;
    public GameObject foodPrefab;
    private List<GameObject> foodObjects = new List<GameObject>();

    void Start()
    {
        networkComm = new NetworkComm();
        networkComm.MsgReceived += processMsg;
        _ = ReceiveMessagesAsync();

        localCubePos.x = 4.0f; localCubePos.y = 1.0f; localCubePos.z = -0.5f;
        remoteCubePos.x = -4.0f; remoteCubePos.y = 1.0f; remoteCubePos.z = -0.5f;
        remoteCube = GameObject.Find("Cube1"); localCube = GameObject.Find("Cube2");
        remoteCube.transform.position = remoteCubePos;
        localCube.transform.position = localCubePos;
        SpawnFoodAroundPlane(10);
    }

    async Task ReceiveMessagesAsync()
    {
        while (true)
        {
            await Task.Delay(10); // Adjust delay as needed
            await networkComm.ReceiveMessagesAsync();
        }
    }

    void CheckFoodDistance()
    {
        float eatDistance = 1.0f;

        for (int i = foodObjects.Count - 1; i >= 0; i--)
        {
            GameObject food = foodObjects[i];
            if (food != null)
            {
                if (Vector3.Distance(localCubePos, food.transform.position) < eatDistance)
                {
                    GrowCube(localCube);
                    Destroy(food);
                    foodObjects.RemoveAt(i);
                }
                else if (Vector3.Distance(remoteCubePos, food.transform.position) < eatDistance)
                {
                    GrowCube(remoteCube);
                    Destroy(food);
                    foodObjects.RemoveAt(i);
                }
            }
        }
    }

    void SpawnFoodAroundPlane(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 foodPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), -.5f);
            GameObject foodInstance = Instantiate(foodPrefab, foodPosition, Quaternion.identity);
            foodObjects.Add(foodInstance);
        }
    }

    void GrowCube(GameObject cube)
    {
        float growFactor = 0.1f;
        cube.transform.localScale += new Vector3(growFactor, growFactor, growFactor);
    }

    void Update()
    {
        if (Input.anyKey)
        {
            if (Input.GetKey(KeyCode.RightArrow)) { localCubePos.x += baseMove; }
            if (Input.GetKey(KeyCode.LeftArrow)) { localCubePos.x -= baseMove; }
            if (Input.GetKey(KeyCode.UpArrow)) { localCubePos.y -= baseMove; }
            if (Input.GetKey(KeyCode.DownArrow)) { localCubePos.y += baseMove; }
            _ = networkComm.SendMessageAsync("ID=4;" + localCubePos.x + "," + localCubePos.y + "," + localCubePos.z);
            Debug.Log("Sending local cube coordinates: " + localCubePos);
        }

        localCube.transform.position = localCubePos;
        remoteCube.transform.position = remoteCubePos;

        CheckFoodDistance();

        if (Vector3.Distance(localCubePos, remoteCubePos) < 2.0f)
        {
            float localCubeSize = localCube.transform.localScale.x;
            float remoteCubeSize = remoteCube.transform.localScale.x;

            if (localCubeSize != remoteCubeSize)
            {
                GameOver();
            }
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        this.enabled = false;
    }

    public void processMsg(string message)
    {
        string[] msgParts = message.Split(';');
        if (msgParts.Length >= 2 && msgParts[0].Contains("ID=2"))
        {
            string[] coordinates = msgParts[1].Split(',');
            if (coordinates.Length >= 3)
            {
                float x = float.Parse(coordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                float y = float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);
                float z = float.Parse(coordinates[2], CultureInfo.InvariantCulture.NumberFormat);
                remoteCubePos = new Vector3(x, y, z);
                Debug.Log("Received remote cube coordinates: " + remoteCubePos);
            }
        }
    }
}
