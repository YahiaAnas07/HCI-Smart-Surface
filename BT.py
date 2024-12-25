import asyncio
from bleak import BleakScanner
from datetime import datetime

class BluetoothScanner:
    def __init__(self):
        self.FADMIN = 0
        self.FUSER = 0
        self.Name = ""
        self.device_address = None
        self.devices = [] 
        self.scan_duration = 5
    
    async def scan_bluetooth_devices(self):
        print("Scanning for Bluetooth devices...")
        self.scan_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        try:
            self.devices = await BleakScanner.discover(timeout=self.scan_duration)

            print(f"\nScan completed at {self.scan_time}")
            print(f"Found {len(self.devices)} devices:\n")
            
            for device in self.devices:
                print(f"Device Name: {device.name}")
                print(f"MAC Address: {device.address}")

                if device.address == "D8:E2:3F:F1:93:9D" or device.name == "U-AC939C":
                    self.FADMIN = 1
                    self.Name = device.name
                    self.device_address = device.address  
                    break
                if device.address == "AC:12:2F:50:B0:17":
                    device.name = "SoundCore"
                    self.device_address = device.address  
                    
                    
            else:
                self.FUSER = 1

            if self.FADMIN == 1:
                print(f"Welcome Admin {self.Name}")
            elif self.FUSER == 1:
                print(f"Welcome User {self.Name}")

        except Exception as e:
            print(f"An error occurred: {str(e)}")

    async def main(self):
        await self.scan_bluetooth_devices()
    
    def run(self):
        asyncio.run(self.main())
    
        devices_data = {}
        try:
            with open("found_devices.txt", "r") as file:
                for line in file:
                    timestamp, name_part, mac_part = line.strip().split(", ")
                    name = name_part.split(": ")[1]
                    mac = mac_part.split(": ")[1]
                    devices_data[mac] = (timestamp, name)
        except FileNotFoundError:
            pass  

        with open("found_devices.txt", "w") as file:
            for device in self.devices:
                name = device.name if device.name else "Unknown"
                if name == "Unknown":
                    continue
                address = device.address
                if address in devices_data:
                    devices_data[address] = (self.scan_time, name)
                else:
                    devices_data[address] = (self.scan_time, name)

            for mac, (timestamp, name) in devices_data.items():
                file.write(f"{timestamp}, Device Name: {name}, MAC Address: {mac}\n")
            
            print("Device information written to 'found_devices.txt'.")

    def get_device_address(self):
        return self.devices
