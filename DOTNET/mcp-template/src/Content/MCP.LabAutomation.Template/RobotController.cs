using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MCP.LabAutomation
{
    public sealed class DeviceController : IDisposable
    {        
        private bool _isInitialized = false;
        private bool _doorOpen = false;

        public void InitializeDevice()
        {
            Console.WriteLine("Initializing device...");
            // Simulate device initialization
            Thread.Sleep(1000);
            Console.WriteLine("Device initialized successfully.");
            _isInitialized = true;
        }

        public void OpenDoor(){
            if (!_isInitialized)
                throw new InvalidOperationException("Device not initialized. Please initialize the device first.");
            if (_doorOpen)
            {
                Console.WriteLine("Door is already open.");
                return;
            }
            Console.WriteLine("Opening door...");
            // Simulate door opening
            Thread.Sleep(500);
            _doorOpen = true;
            Console.WriteLine("Door opened successfully.");
        }

        public void CloseDoor()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Device not initialized. Please initialize the device first.");
            if (!_doorOpen)
            {
                Console.WriteLine("Door is already closed.");
                return;
            }
            Console.WriteLine("Closing door...");
            // Simulate door closing
            Thread.Sleep(500);
            _doorOpen = false;
            Console.WriteLine("Door closed successfully.");
        }

        public string GetDeviceStatus()
        {
            if (!_isInitialized)
                return "Device not initialized.";
            return _doorOpen ? "Device is ready with door open." : "Device is ready with door closed.";
        }
    }
}
