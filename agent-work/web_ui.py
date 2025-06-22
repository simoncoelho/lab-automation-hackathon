import asyncio
from flask import Flask, request, jsonify, render_template_string
from agents import Runner
from agents import RunHooks
from agent import create_mcp_servers, create_lab_manager
import json
from datetime import datetime

app = Flask(__name__)

class ToolCallLogger(RunHooks):
    def __init__(self):
        self.tool_calls = []
    
    def _get_server_name(self, tool):
        # Try to identify the MCP server from tool name patterns
        tool_name = tool.name.lower()
        if 'arm' in tool_name or 'move_plate' in tool_name:
            return 'arm'
        elif 'ot' in tool_name or 'pcr' in tool_name or 'opentrons' in tool_name:
            return 'ot'
        elif 'sensor' in tool_name or 'temperature' in tool_name or 'humidity' in tool_name:
            return 'sensor'
        elif 'error' in tool_name or 'notify' in tool_name:
            return 'error'
        elif 'status' in tool_name or 'location' in tool_name or 'plate' in tool_name:
            return 'lab_status'
        else:
            return 'unknown'
    
    async def on_tool_start(self, context, agent, tool):
        timestamp = datetime.now().isoformat()
        server_name = self._get_server_name(tool)
        print(f"🔧 [{timestamp}] Starting tool: {tool.name} on {agent.name} (server: {server_name})")
        print(f"🔍 Tool object: {tool}")
        print(f"🔍 Context object: {context}")
        tool_info = {
            "timestamp": timestamp,
            "type": "start",
            "tool_name": tool.name,
            "agent_name": agent.name,
            "server": server_name
        }
        self.tool_calls.append(tool_info)
    
    async def on_tool_end(self, context, agent, tool, result):
        timestamp = datetime.now().isoformat()
        server_name = self._get_server_name(tool)
        # Truncate long results for display
        result_preview = str(result)[:200] + "..." if len(str(result)) > 200 else str(result)
        
        # Extract descriptive message from tool result if available
        descriptive_message = self._extract_message_from_result(result)
        
        tool_info = {
            "timestamp": timestamp,
            "type": "end",
            "tool_name": tool.name,
            "agent_name": agent.name,
            "server": server_name,
            "result": result_preview,
            "description": descriptive_message
        }
        self.tool_calls.append(tool_info)
        print(f"✅ [{timestamp}] Completed tool: {tool.name} -> {result_preview} (server: {server_name})")
        if descriptive_message:
            print(f"📝 [{timestamp}] Message: {descriptive_message}")
        
    def _extract_message_from_result(self, result):
        """Extract descriptive message from tool result"""
        print(f"🔍 Extracting message from result: {result} (type: {type(result)})")
        
        # Handle the nested structure that agents library returns
        actual_result = result
        if isinstance(result, dict) and "text" in result:
            try:
                # Parse the JSON string inside the text field
                import json
                actual_result = json.loads(result["text"])
                print(f"🔍 Parsed inner result: {actual_result}")
            except:
                print("❌ Failed to parse inner JSON")
                return None
        
        if isinstance(actual_result, dict):
            # Try to get message or log_message from the result
            if "message" in actual_result and actual_result["message"] != actual_result.get("status"):
                print(f"📨 Found message: {actual_result['message']}")
                return actual_result["message"]
            elif "log_message" in actual_result:
                print(f"📨 Found log_message: {actual_result['log_message']}")
                return actual_result["log_message"]
        
        print("❌ No message found in result")
        # If no descriptive message found, return None to use result instead
        return None


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
        .tool-calls {
            margin-top: 20px;
            padding: 20px;
            background: white;
            border-radius: 8px;
            border-left: 4px solid #28a745;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }
        .tool-call {
            margin: 10px 0;
            padding: 10px;
            background: #f8f9fa;
            border-radius: 4px;
            border-left: 3px solid #17a2b8;
        }
        .tool-call.arm { border-left-color: #dc3545; }
        .tool-call.ot { border-left-color: #fd7e14; }
        .tool-call.sensor { border-left-color: #20c997; }
        .tool-call.error { border-left-color: #6f42c1; }
        .tool-call.lab_status { border-left-color: #0dcaf0; }
        .tool-call-header {
            font-weight: bold;
            color: #495057;
            margin-bottom: 5px;
        }
        .tool-call-result {
            font-size: 12px;
            color: #6c757d;
            background: #e9ecef;
            padding: 8px;
            border-radius: 3px;
            margin-top: 5px;
        }
        .tool-call.simple {
            padding: 8px 12px;
            font-size: 14px;
            color: #495057;
            line-height: 1.4;
        }
        .timestamp {
            font-size: 11px;
            color: #868e96;
            float: right;
        }
        .trace-toggle {
            background: #6c757d;
            color: white;
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            margin-bottom: 10px;
            float: right;
        }
        .trace-toggle:hover {
            background: #5a6268;
        }
        .tool-call.hidden {
            display: none;
        }
        .detail-only.hidden {
            display: none;
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
                    let html = `<div class="response"><strong>Agent Response:</strong><br><pre>${data.response}</pre></div>`;
                    
                    if (data.tool_calls && data.tool_calls.length > 0) {
                        html += '<div class="tool-calls">';
                        html += '<button class="trace-toggle" onclick="toggleTraceDetail()" id="traceToggle">Full Trace</button>';
                        html += '<strong>Tool Calls:</strong><div style="clear: both;"></div>';
                        
                        data.tool_calls.forEach(call => {
                            console.log('🔍 Processing tool call:', JSON.stringify(call, null, 2));
                            const serverClass = call.server && call.server !== 'unknown' ? call.server : 'default';
                            const isImportantCall = ['run_pcr_process', 'move_plate', 'get_sensor_data', 'notify_error'].includes(call.tool_name);
                            const hiddenClass = isImportantCall ? '' : ' detail-only hidden';
                            
                            if (call.type === 'end') {
                                html += `<div class="tool-call simple ${serverClass}${hiddenClass}">`;
                                html += `<span class="timestamp">${new Date(call.timestamp).toLocaleTimeString()}</span> `;
                                if (call.description && call.description !== 'null' && call.description !== '') {
                                    html += call.description;
                                } else if (call.result) {
                                    // Try to extract message from nested result structure
                                    try {
                                        // First parse the outer result JSON
                                        const outerResult = typeof call.result === 'string' ? JSON.parse(call.result) : call.result;
                                        console.log('🔍 Parsed outer result:', outerResult);
                                        
                                        // Then parse the inner text JSON if it exists
                                        if (outerResult && outerResult.text) {
                                            const innerResult = JSON.parse(outerResult.text);
                                            console.log('🔍 Parsed inner result:', innerResult);
                                            if (innerResult && innerResult.message && innerResult.message !== innerResult.status) {
                                                html += innerResult.message;
                                            } else {
                                                html += call.tool_name;
                                            }
                                        } else {
                                            html += call.tool_name;
                                        }
                                    } catch (e) {
                                        console.log('❌ Error parsing result:', e);
                                        html += call.tool_name;
                                    }
                                } else {
                                    html += call.tool_name;
                                }
                                html += '</div>';
                            }
                        });
                        html += '</div>';
                    }
                    
                    responseDiv.innerHTML = html;
                } else {
                    responseDiv.innerHTML = `<div class="response error"><strong>Error:</strong> ${data.error}</div>`;
                }
            } catch (error) {
                responseDiv.innerHTML = `<div class="response error"><strong>Error:</strong> Failed to communicate with server</div>`;
            }
            
            submitBtn.disabled = false;
        });
        
        let showFullTrace = false;
        
        function toggleTraceDetail() {
            const toggleBtn = document.getElementById('traceToggle');
            const detailCalls = document.querySelectorAll('.tool-call.detail-only');
            
            showFullTrace = !showFullTrace;
            
            if (showFullTrace) {
                toggleBtn.textContent = 'Hide Details';
                detailCalls.forEach(call => call.classList.remove('hidden'));
            } else {
                toggleBtn.textContent = 'Full Trace';
                detailCalls.forEach(call => call.classList.add('hidden'));
            }
        }
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
            tool_logger = ToolCallLogger()
            
            async with servers[0] as arm_server, servers[1] as ot_server, servers[2] as sensor_server, servers[3] as error_server, servers[4] as lab_status_server:
                lab_manager = create_lab_manager([arm_server, ot_server, sensor_server, error_server, lab_status_server])
                
                result = await Runner.run(lab_manager, input=prompt, hooks=tool_logger)
                return result.final_output, tool_logger.tool_calls
        
        final_output, tool_calls = asyncio.run(process_prompt())
        print(f"🔍 Total tool calls captured: {len(tool_calls)}")
        print(f"🔍 Sending tool_calls to frontend: {tool_calls}")
        return jsonify({'response': final_output, 'tool_calls': tool_calls})
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=8888)
