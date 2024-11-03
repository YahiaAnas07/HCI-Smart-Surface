import asyncio
from bleak import BleakScanner
from datetime import datetime
import tkinter as tk
from tkinter import messagebox

async def scan_bluetooth_devices(FADMIN,FUSER,Name):
    print("Scanning for bluetooth devices...")
    scan_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    try:
        devices = await BleakScanner.discover()

        print(f"\nScan completed at {scan_time}")
        print(f"Found {len(devices)} devices:\n")
        
        for device in devices:
            print(f"Device Name: {device.name}")
            print(f"MAC Address: {device.address}")

        for i in devices:
            if i.address=="D8:AA:59:AA:A0:1D " or i.name=="OPPO Enco Air2 Pro":
                FADMIN=1
                Name=i.name
                break
            else:
                FUSER=1
        if FADMIN==1:
            messagebox.showinfo("welcome message", f"Welcome Admin {Name}")

        if FADMIN==0:
            messagebox.showinfo("welcome message", f"Welcome User {Name}")



    except Exception as e:
        print(f"An error occurred: {str(e)}")

async def main():
    print("Starting Bluetooth scan...")
    await scan_bluetooth_devices(FADMIN,FUSER,Name)


    # Run the async function
FADMIN=0
FUSER=0
Name=""
asyncio.run(main())
root = tk.Tk()
root.withdraw()  # Hide the main window
# Run the application
root.destroy()