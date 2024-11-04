import asyncio
from bleak import BleakScanner
from datetime import datetime

class BluetoothScanner:
    def __init__(self):
        self.FADMIN = 0
        self.FUSER = 0
        self.Name = ""
        self.device_address = None 

    async def scan_bluetooth_devices(self):
        print("Scanning for bluetooth devices...")
        scan_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        try:
            devices = await BleakScanner.discover()

            print(f"\nScan completed at {scan_time}")
            print(f"Found {len(devices)} devices:\n")
            
            for device in devices:
                print(f"Device Name: {device.name}")
                print(f"MAC Address: {device.address}")

            for device in devices:
                if device.address == "D8:E2:3F:F1:93:9D" or device.name == "U-AC939C":
                    self.FADMIN = 1
                    self.Name = device.name
                    self.device_address = device.address  
                    
                    break
                else:
                    self.FUSER = 1

            if self.FADMIN == 1:
                print(f"Welcome Admin {self.Name}")

            if self.FADMIN == 0:
                print(f"Welcome User {self.Name}")

        except Exception as e:
            print(f"An error occurred: {str(e)}")

    async def main(self):
        await self.scan_bluetooth_devices()
    
    def run(self):
        asyncio.run(self.main())

    def get_device_address(self):
        return self.device_address
