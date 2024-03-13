from flask import Flask, request, jsonify
import json
app = Flask(__name__)

# Información que se enviará a la simulación
simulation_data = {}

@app.route('/')
def home():
    return "Intersection Simulation API"

@app.route('/api/vehicle_data', methods=['POST'])
def vehicle_data():
    data = request.get_json()
    simulation_data['vehicle_data'] = data
    # Guarda la información
    print("Received vehicle data:", data)
    return jsonify({"status": "success", "message": "Vehicle data received"}), 200

@app.route('/api/get_vehicle_data', methods=['GET'])
def get_vehicle_data():
    try:
        # Abre el archivo simulation_data.json y regresa su contenido
        with open('simulation_data.json', 'r') as json_file:
            data = json.load(json_file)
        return jsonify(data), 200
    except FileNotFoundError:
        return jsonify({"error": "Simulation data not found"}), 404

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5002)
