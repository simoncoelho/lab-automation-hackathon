import asyncio
from flask import Flask, request, jsonify, render_template_string
from agents import Runner
from agent import create_mcp_servers, create_lab_manager

app = Flask(__name__)


HTML_TEMPLATE = """
<!DOCTYPE html>
<html>
<head>
    <title>Lab Automation Agent</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            margin: 0; 
            padding: 20px; 
            min-height: 100vh; 
            background: #f0f2f5; 
        }
        .container { 
            background: #f5f5f5; 
            padding: 30px; 
            border-radius: 12px; 
            max-width: 1200px; 
            margin: 0 auto; 
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); 
        }
        textarea { 
            width: 100%; 
            height: 120px; 
            padding: 15px; 
            border: 1px solid #ddd; 
            border-radius: 8px; 
            font-family: Arial, sans-serif; 
            font-size: 14px; 
            resize: vertical; 
            box-sizing: border-box; 
        }
        button { 
            background: #007cba; 
            color: white; 
            padding: 12px 24px; 
            border: none; 
            border-radius: 6px; 
            cursor: pointer; 
            font-size: 16px; 
            font-weight: 500; 
        }
        button:hover { background: #005a87; }
        button:disabled { background: #ccc; cursor: not-allowed; }
        .response { 
            margin-top: 20px; 
            padding: 20px; 
            background: white; 
            border-radius: 8px; 
            border-left: 4px solid #007cba; 
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); 
        }
        .response pre { 
            white-space: pre-wrap; 
            word-wrap: break-word; 
            overflow-wrap: break-word; 
            margin: 10px 0 0 0; 
            padding: 15px; 
            background: #f8f9fa; 
            border-radius: 6px; 
            font-family: 'Courier New', monospace; 
            font-size: 13px; 
            line-height: 1.5; 
            max-width: 100%; 
            overflow-x: auto; 
        }
        .loading { color: #666; font-style: italic; }
        .error { border-left-color: #dc3545; background: #f8d7da; }
        h1 { 
            text-align: center; 
            color: #333; 
            margin-bottom: 30px; 
            font-size: 2.5em; 
        }
        label { 
            font-weight: 500; 
            color: #555; 
            font-size: 16px; 
        }
    </style>
</head>
<body>
    <h1>Lab Automation Agent</h1>
    <div class="container">
        <form id="promptForm">
            <label for="prompt">Enter your lab automation request:</label><br><br>
            <textarea id="prompt" name="prompt" placeholder="e.g., Move plate from device A slot 1 to device B slot 2"></textarea><br><br>
            <button type="submit" id="submitBtn">Send Request</button>
        </form>
        <div id="response"></div>
    </div>

    <script>
        document.getElementById('promptForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const prompt = document.getElementById('prompt').value;
            const responseDiv = document.getElementById('response');
            const submitBtn = document.getElementById('submitBtn');
            
            if (!prompt.trim()) {
                responseDiv.innerHTML = '<div class="response error">Please enter a request.</div>';
                return;
            }
            
            submitBtn.disabled = true;
            responseDiv.innerHTML = '<div class="response loading">Processing your request...</div>';
            
            try {
                const response = await fetch('/api/prompt', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ prompt: prompt })
                });
                
                const data = await response.json();
                
                if (response.ok) {
                    responseDiv.innerHTML = `<div class="response"><strong>Agent Response:</strong><br><pre>${data.response}</pre></div>`;
                } else {
                    responseDiv.innerHTML = `<div class="response error"><strong>Error:</strong> ${data.error}</div>`;
                }
            } catch (error) {
                responseDiv.innerHTML = `<div class="response error"><strong>Error:</strong> Failed to communicate with server</div>`;
            }
            
            submitBtn.disabled = false;
        });
    </script>
</body>
</html>
"""

@app.route('/')
def index():
    return render_template_string(HTML_TEMPLATE)

@app.route('/api/prompt', methods=['POST'])
def handle_prompt():
    try:
        data = request.get_json()
        prompt = data.get('prompt', '').strip()
        
        if not prompt:
            return jsonify({'error': 'No prompt provided'}), 400
        
        async def process_prompt():
            servers = create_mcp_servers()
            
            async with servers[0] as arm_server, servers[1] as ot_server, servers[2] as sensor_server, servers[3] as error_server, servers[4] as lab_status_server:
                lab_manager = create_lab_manager([arm_server, ot_server, sensor_server, error_server, lab_status_server])
                
                result = await Runner.run(lab_manager, input=prompt)
                return result.final_output
        
        response = asyncio.run(process_prompt())
        return jsonify({'response': response})
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=8888)
