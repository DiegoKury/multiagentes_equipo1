using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON; // Asegúrate de incluir SimpleJSON en el proyecto

public class FetchData : MonoBehaviour
{
    public GameObject carPrefab; // Variable anclada al modelo de carro en Unity

    private Dictionary<string, GameObject> activeCars = new Dictionary<string, GameObject>();

    // Start se llama antes del primer frame
    void Start()
    {
        // Inicia la corutina para enviar solicitudes GET periódicas al endpoint de Flask
        StartCoroutine(PeriodicGetDataCoroutine("http://127.0.0.1:5002/api/get_vehicle_data"));
    }

    // Corutina para enviar periódicamente solicitudes GET al endpoint de Flask
    IEnumerator PeriodicGetDataCoroutine(string url)
    {
        while (true) // Crea un bucle infinito para seguir obteniendo datos
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Solicita y espera la página deseada
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("Error HTTP: " + webRequest.error);
                }
                else
                {
                    // Registra el JSON recibido para depuración
                    Debug.Log("JSON recibido: " + webRequest.downloadHandler.text);
                    ProcessData(webRequest.downloadHandler.text);
                }
            }
            yield return new WaitForSeconds(.5f); // Tiempo de espera
        }
    }

    void ProcessData(string jsonData)
    {
        var parsedData = JSON.Parse(jsonData);
        var agents = parsedData["agentes"];

        // Crea un conjunto de IDs recibidos en la actualización actual para comparación
        HashSet<string> receivedIds = new HashSet<string>();

        foreach (KeyValuePair<string, JSONNode> agent in agents.AsObject)
        {
            string id = agent.Key;
            receivedIds.Add(id); // Añade el ID actual al conjunto de IDs recibidos

            Vector3 position = new Vector3(agent.Value["pos"]["x"].AsFloat, 0, agent.Value["pos"]["y"].AsFloat);

            // Registra el ID de cada coche y la posición actualizada para depuración
            Debug.Log($"ID del coche: {id}, Posición: {position}");

            if (!activeCars.ContainsKey(id))
            {
                // Instancia un nuevo coche si aún no existe
                GameObject newCar = Instantiate(carPrefab, position, Quaternion.identity);
                activeCars[id] = newCar;
            }
            else
            {
                // Actualiza la posición del coche existente
                activeCars[id].transform.position = position;
            }

            // Actualiza la rotación del coche basada en 'direccion'
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
                // Añade más casos según sea necesario para diferentes direcciones
            }
        }

        // Identifica y elimina los coches que ya no están en la simulación
        List<string> idsToRemove = new List<string>();
        foreach (var carId in activeCars.Keys)
        {
            if (!receivedIds.Contains(carId))
            {
                idsToRemove.Add(carId);
            }
        }

        foreach (var idToRemove in idsToRemove)
        {
            Destroy(activeCars[idToRemove]); // Elimina el GameObject de la escena
            activeCars.Remove(idToRemove); // Elimina la entrada del diccionario
        }
    }
}
