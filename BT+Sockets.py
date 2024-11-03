import asyncio
from bleak import BleakScanner
from datetime import datetime
import tkinter as tk
from tkinter import messagebox
import socket

async def scan_bluetooth_devices():
    global FADMIN, FUSER, Name
    print("Scanning for Bluetooth devices...")
    scan_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    try:
        devices = await BleakScanner.discover()
        print(f"\nScan completed at {scan_time}")
        print(f"Found {len(devices)} devices:\n")
        
        for device in devices:
            print(f"Device Name: {device.name}")
            print(f"MAC Address: {device.address}")

            if device.address == "D8:AA:59:AA:A0:1D" or device.name == "OPPO Enco Air2 Pro":
                FADMIN = 1
                Name = device.name
                break
            else:
                FUSER = 1

        # Show welcome message based on device type
        if FADMIN == 1:
            messagebox.showinfo("Welcome Message", f"Welcome Admin {Name}")
        else:
            messagebox.showinfo("Welcome Message", f"Welcome User {Name}")

        # Set up socket connection
        mySocket = socket.socket()
        mySocket.bind(('localhost', 5000))
        mySocket.listen(5)
        print("Waiting for a client to connect...")

        conn, addr = mySocket.accept()
        conn.send(str(addr).encode('utf-8'))
        print(f"Device connected from {addr}")

        conn.close()
        mySocket.close()

    except Exception as e:
        print(f"An error occurred: {str(e)}")

async def main():
    print("Starting Bluetooth scan...")
    await scan_bluetooth_devices()

# Initialize variables
FADMIN = 0
FUSER = 0
Name = ""

# Run the async function
asyncio.run(main())

# Tkinter setup
root = tk.Tk()
root.withdraw()  
root.destroy()
