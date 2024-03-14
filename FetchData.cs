using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class FetchData : MonoBehaviour
{
    public GameObject carPrefab; // Variable anclada al modelo de carro en Unity
    public GameObject trafficLightPrefab; // Variable anclada al modelo de semáforo en Unity
    public Material greenLightMaterial;
    public Material redLightMaterial;
    public Material yellowLightMaterial;

    private Dictionary<string, GameObject> activeCars = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> trafficLights = new Dictionary<string, GameObject>();

    void Start()
    {
        InitializeTrafficLights();
        StartCoroutine(PeriodicGetDataCoroutine("http://127.0.0.1:5002/api/get_vehicle_data"));
    }

    IEnumerator PeriodicGetDataCoroutine(string url)
    {
        // Hace los gets al servidor
        while (true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log("JSON recibido: " + webRequest.downloadHandler.text);
                    ProcessData(webRequest.downloadHandler.text);
                }
            }
            // Hace el get cada .5 segundos
            yield return new WaitForSeconds(.5f);
        }
    }

    void ProcessData(string jsonData)
    {
        var parsedData = JSON.Parse(jsonData);
        UpdateCars(parsedData["agentes"]);
        UpdateTrafficLights(parsedData["semaforos"]);
    }

    void InitializeTrafficLights()
    {
        // Inicializa los semáforos en posiciones fijas
        trafficLights["W_E"] = Instantiate(trafficLightPrefab, new Vector3(23, 0, 24), Quaternion.identity);
        trafficLights["N_S"] = Instantiate(trafficLightPrefab, new Vector3(24, 0, 26), Quaternion.identity);
        trafficLights["E_W"] = Instantiate(trafficLightPrefab, new Vector3(26, 0, 25), Quaternion.identity);
        trafficLights["S_N"] = Instantiate(trafficLightPrefab, new Vector3(25, 0, 23), Quaternion.identity);
    }

    void UpdateTrafficLights(JSONNode trafficLightData)
    {
        // Actualiza el color del semáforo con los materiales
        foreach (KeyValuePair<string, JSONNode> item in trafficLightData.AsObject)
        {
            string direction = item.Key;
            string state = item.Value["estado"];
            Material lightMaterial = null;

            switch(state)
            {
                case "green":
                    lightMaterial = greenLightMaterial;
                    break;
                case "red":
                    lightMaterial = redLightMaterial;
                    break;
                case "yellow":
                    lightMaterial = yellowLightMaterial;
                    break;
            }

            if (lightMaterial != null && trafficLights.ContainsKey(direction))
            {
                trafficLights[direction].GetComponent<Renderer>().material = lightMaterial;
            }
        }
    }

    void UpdateCars(JSONNode carData)
    {
        HashSet<string> receivedIds = new HashSet<string>();

        foreach (KeyValuePair<string, JSONNode> agent in carData.AsObject)
    {
        string id = agent.Key;
        receivedIds.Add(id);
        // Actualiza la posición de los carros
        Vector3 position = new Vector3(agent.Value["posicion"]["x"].AsFloat, 0, agent.Value["posicion"]["y"].AsFloat);

        if (!activeCars.ContainsKey(id))
        {
            // Crea el carro
            GameObject newCar = Instantiate(carPrefab, position, Quaternion.identity);
            activeCars[id] = newCar;
        }
        else
        {
            activeCars[id].transform.position = position;
        }

            // Se encarga de que la dirección sea consistente
            switch (agent.Value["direccion"].Value)
            {
                case "N_S":
                    activeCars[id].transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case "S_N":
                    activeCars[id].transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case "E_W":
                    activeCars[id].transform.rotation = Quaternion.Euler(0, 270, 0);
                    break;
                case "W_E":
                    activeCars[id].transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
        }

        List<string> idsToRemove = new List<string>();
        foreach (var carId in activeCars.Keys)
        {
            if (!receivedIds.Contains(carId))
            {
                // Agrega los carros para ser eliminados
                idsToRemove.Add(carId);
            }
        }

        foreach (var idToRemove in idsToRemove)
        {
            // Elimina los carros que ya no están en la simulación
            Destroy(activeCars[idToRemove]);
            activeCars.Remove(idToRemove);
        }
    }
}
