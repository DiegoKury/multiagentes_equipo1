#pip install flask 
from flask import Flask, request, jsonify

@app.route('/api/update', methods=['POST', 'GET'])
def update():
    global simulation_data  # Declarada como global
    
    try:
        if request.method == 'POST': # Esta parte se llama desde la simulacion de mesa de python
            # Parse jSON
            simulation_data = request.json
            
            # Imprimir informacion recibida
            print("Received Data:")
            print(simulation_data)
            
            return jsonify({"status": "success"}), 200

        elif request.method == 'GET': # Esta parte se llama desde unity
            # Return the last updated simulation data
            print("Sending Data:")
            print(simulation_data)
            return jsonify(simulation_data), 200

    except Exception as e:
        print(f"An error occurred: {e}")
        return jsonify({"status": "error"}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)