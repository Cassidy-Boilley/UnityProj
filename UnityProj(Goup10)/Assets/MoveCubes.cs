using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Globalization;
using NetworkAPI;

//remote player
public class MoveCubes : MonoBehaviour
{
    NetworkComm networkComm;
    public GameObject localCube, remoteCube;
    public GameObject foodPrefab; // Assign this in the Inspector with your food prefab.
    public Vector3 localCubePos = new Vector3();
    public Vector3 remoteCubePos = new Vector3();
    private List<GameObject> foodObjects = new List<GameObject>(); // List to store food instances

    void Awake() { }
    void OnEnable() { }
    void Start()
    {
        networkComm = new NetworkComm();
        networkComm.MsgReceived += new NetworkComm.MsgHandler(processMsg);
        (new Thread(new ThreadStart(networkComm.ReceiveMessages))).Start();
        //(new Thread(new ThreadStart(threadfunc))).Start();
        localCubePos.x = 5.0f; localCubePos.y = 1.0f; localCubePos.z = 1.0f;
        remoteCubePos.x = 1.0f; remoteCubePos.y = 1.0f; remoteCubePos.z = 1.0f;
        remoteCube = GameObject.Find("Cube1"); localCube = GameObject.Find("Cube2");
        SpawnFoodAroundPlane(10); // Spawn 10 food items.

        remoteCube.transform.position = remoteCubePos;
        localCube.transform.position = localCubePos;
    }
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is food
        if (collision.gameObject.tag == "foodPrefab(Clone)") // Ensure your food prefab has the tag "Food"
        {
            GrowCube(localCube);
            Destroy(collision.gameObject); // Destroy the food object
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "foodPrefab(Clone)")
        {
            GrowCube(gameObject); // Assuming you want to grow the cube that hits the food.
            Destroy(other.gameObject); // Destroy the food object.
        }
    }


    void GrowCube(GameObject cube)
    {
        float growFactor = 0.1f; // Define how much the cube should grow
        cube.transform.localScale += new Vector3(growFactor, growFactor, growFactor); // Increase the scale of the cube
    }
    // Update is called once per frame
    // Update is called once per frame
    void Update()
    {
        // Control localCube with WASD
        if (Input.GetKey(KeyCode.W)) { localCubePos.z += 0.1f; } // Move forward
        if (Input.GetKey(KeyCode.S)) { localCubePos.z -= 0.1f; } // Move backward
        if (Input.GetKey(KeyCode.A)) { localCubePos.x -= 0.1f; } // Move left
        if (Input.GetKey(KeyCode.D)) { localCubePos.x += 0.1f; } // Move right

        // Send localCube's position with ID=1
        networkComm.sendMessage("ID=1;" + localCubePos.x.ToString(CultureInfo.InvariantCulture) + "," + localCubePos.y.ToString(CultureInfo.InvariantCulture) + "," + localCubePos.z.ToString(CultureInfo.InvariantCulture));
        localCube.transform.position = localCubePos;

        // Control remoteCube with Arrow Keys
        if (Input.GetKey(KeyCode.UpArrow)) { remoteCubePos.z += 0.1f; } // Move forward
        if (Input.GetKey(KeyCode.DownArrow)) { remoteCubePos.z -= 0.1f; } // Move backward
        if (Input.GetKey(KeyCode.LeftArrow)) { remoteCubePos.x -= 0.1f; } // Move left
        if (Input.GetKey(KeyCode.RightArrow)) { remoteCubePos.x += 0.1f; } // Move right

        // Send remoteCube's position with ID=2
        networkComm.sendMessage("ID=2;" + remoteCubePos.x.ToString(CultureInfo.InvariantCulture) + "," + remoteCubePos.y.ToString(CultureInfo.InvariantCulture) + "," + remoteCubePos.z.ToString(CultureInfo.InvariantCulture));
        remoteCube.transform.position = remoteCubePos;
        CheckFoodDistance(); // Check distance and handle food consumption


        // Collision detection example (remains the same)
        if (Vector3.Distance(localCubePos, remoteCubePos) < 2.0f) // Assuming cubes collide if closer than 2 units
        {
            // Determine the size of each cube
            float localCubeSize = localCube.transform.localScale.x; // Assuming uniform scaling
            float remoteCubeSize = remoteCube.transform.localScale.x; // Assuming uniform scaling

            // Trigger Game Over only if the cubes are of different sizes
            if (localCubeSize != remoteCubeSize)
            {
                GameOver();
            }
        }
        // HandleCubeCollision();

        //GrowCube(localCube); // Assuming you want to grow the cube that hits the food.

    }

    void GameOver()
    {
        Debug.Log("Game Over!"); // Log game over message
                                 // Here you can implement further game over logic, such as:
                                 // - Displaying a game over screen
                                 // - Stopping player input
                                 // - Optionally restarting the game or going back to the main menu

        // For example, to stop all movement you could disable this script:
        this.enabled = false;

        // Or, to load a game over scene, you could use:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverSceneName");
    }


    void SpawnFoodAroundPlane(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 foodPosition = new Vector3(Random.Range(-10.0f, 10.0f), 0.5f, Random.Range(-10.0f, 10.0f));
            GameObject foodInstance = Instantiate(foodPrefab, foodPosition, Quaternion.identity);
            foodObjects.Add(foodInstance); // Add the new food instance to the list
        }
    }

    void CheckFoodDistance()
    {
        float eatDistance = 1.0f; // Set the distance within which a cube can "eat" the food

        for (int i = foodObjects.Count - 1; i >= 0; i--)
        {
            GameObject food = foodObjects[i];
            if (food != null)
            {
                if (Vector3.Distance(localCubePos, food.transform.position) < eatDistance)
                {
                    GrowCube(localCube);
                    Destroy(food);
                    foodObjects.RemoveAt(i); // Remove the food from the list
                }
                else if (Vector3.Distance(remoteCubePos, food.transform.position) < eatDistance)
                {
                    GrowCube(remoteCube);
                    Destroy(food);
                    foodObjects.RemoveAt(i); // Remove the food from the list
                }
            }
        }
    }
    void OnDisable() { Debug.Log("OnDisable Called"); }
    public void processMsg(string message)
    {
        string[] msgParts = message.Split(";");
        if (!msgParts[0].Contains("ID=1"))
        {
            string[] coordinates = msgParts[1].Split(",");
            float x = float.Parse(coordinates[0], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);
            float z = float.Parse(coordinates[2], CultureInfo.InvariantCulture.NumberFormat);
            remoteCubePos.x = x; remoteCubePos.y = y; remoteCubePos.z = z;
        }
    }
    public void threadfunc()
    {
        float x = 1.0f, y = 1.0f, z = 1.0f;
        while (true)
        {
            Thread.Sleep(1000);
            processMsg("ID=2;" + x + "," + y + "," + z);
            x += 0.1f; y += 0.1f; z += 0.1f;
        }
    }
}