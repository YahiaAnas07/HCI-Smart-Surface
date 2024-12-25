from BT import BluetoothScanner
from face import analyze_faces_with_deepface, recognize_faces, load_faces, load_deepface
from Swiple_Left.Swipe_Left_Temple import Return_Swipe_Left_Temp
from Thumbs_UP.Thumbs_Up_Temp import Return_Thumbs_Up_Temp
from Thumbs_Down.Thumbs_Down_Temp import Return_Thumbs_Down_Temp
from Right_Swipe.Right_Swipe_Temp import Return_Swipe_Right_Temp
from object_detection import load_model_and_reference_images, find_matches
from gaze import get_gaze_ratio

import numpy as np
from dollarpy import Point
import cv2
import mediapipe as mp
import socket
import time
import threading
import json
import dlib
import os

thumbs_up_recognizer=Return_Thumbs_Up_Temp()
thumbs_down_recognizer=Return_Thumbs_Down_Temp()
Swipe_Left_recognizer=Return_Swipe_Left_Temp()
Swipe_Right_recognizer=Return_Swipe_Right_Temp()


model, reference_product, orb = load_model_and_reference_images()
print("loading DeepFace model...")
load_deepface()
print("DeepFace load complete.")

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose
mp_hands = mp.solutions.hands

detector = dlib.get_frontal_face_detector()
predictor = dlib.shape_predictor("shape_predictor_68_face_landmarks.dat")

pose = mp_pose.Pose(min_detection_confidence=0.5, min_tracking_confidence=0.5)
hands = mp_hands.Hands(min_detection_confidence=0.7, min_tracking_confidence=0.7)

user_data = {
    "Khaled Mohamed" : "Product A, Product B, Product C, 21, male, dry" , 
    ##############################
    "Aly Essam" : "Product 1, Product 2, Product 3, 19 ,male, oily"  ,
    ##############################
    "Pierre Nabil" : "Product 4 ,Product 5, Product 6, 21, Male, Normal",
    #############################
    "SoundCore" : " Product X, Product Y, Product Z, 23, female, normal"  , 
}

gest = {
    ##########
    "TUP1":3,
    "TUP2":3,
    "TUP3":3,
    "TUP4":3,
    "TUP5":3,
    "TUP6":3,
    "TUP7":3,
    ###########
    "TD1":4,
    "TD2":4,
    "TD3":4,
    "TD4":4,
    "TD5":4,
    "TD6":4,
    "TD7":4,
}

live_gest = [-1, -1, -1]
mySocket = socket.socket()
mySocket.bind(('localhost', 5000))
mySocket.listen(5)
print("Waiting for a client to connect...")

conn, addr = mySocket.accept()
print(f"Device connected from {addr}")

cap = cv2.VideoCapture(0)

framecnt = 0
Allpoints = []
value = 0
hand_value = 0
hand_points = []
num_fingers=0
device_address=0
data = ""  
dev = ""
start = False
start_eye_typing = False
known_face_encodings, known_face_names = load_faces()
face_locations = []
face_names = []
process_this_frame = True
frame_count = 0 
deepface_results = []
msg_ct = -1
prev_msg = " " 

def get_BT():
    global device_address, dev
    dev = None  

    scanner = BluetoothScanner()
    scanner.run()  
    device_address = scanner.get_device_address()

    if device_address:
        print(f"Found device address: {device_address}")
        for device in device_address:
            if device.name != "None" and device.name: 
                dev = device.name
                break  
        if dev:
            return dev
        else:
            return "None"
    else:
        print("No target device found.")

def count_fingers(hand_landmarks):
    thumb = 1 if hand_landmarks[4].x < hand_landmarks[3].x else 0
    index = 1 if hand_landmarks[8].y < hand_landmarks[6].y else 0
    middle = 1 if hand_landmarks[12].y < hand_landmarks[10].y else 0
    ring = 1 if hand_landmarks[16].y < hand_landmarks[14].y else 0
    pinky = 1 if hand_landmarks[20].y < hand_landmarks[18].y else 0
    return thumb + index + middle + ring + pinky

def face_recognition_processing(frame, known_face_encodings, known_face_names, process_this_frame):
    face_locations = []
    face_names = []
    small_frame = cv2.resize(frame, (0, 0), fx=0.25, fy=0.25)
    rgb_small_frame = cv2.cvtColor(small_frame, cv2.COLOR_BGR2RGB)
    face_locations, face_names = recognize_faces(rgb_small_frame, known_face_encodings, known_face_names)
    process_this_frame = not process_this_frame
    return face_locations, face_names, process_this_frame

def gesture_control(hand_points):
    Thumbs_Up_results_Hand = thumbs_up_recognizer.recognize(hand_points)
    Thumbs_down_results_Hand = thumbs_down_recognizer.recognize(hand_points)
    results = [
        ("TUP", Thumbs_Up_results_Hand[0], Thumbs_Up_results_Hand[1]), 
        ("TD", Thumbs_down_results_Hand[0], Thumbs_down_results_Hand[1]),
    ]

    best_result = max(results, key=lambda x: x[2]) 
    gesture_name, gesture_identifier, gesture_score = best_result
    return gesture_name
    
def send_data(data):
    if framecnt >0  and framecnt <=1:
        data = "!"
    conn.send(data.encode('utf-8'))
    print(f"Data sent: {data}")

def read_data():
    prev_msg = ""
    msg_ct = -1
    while True:
        try:
            data = conn.recv(1024)
            if data:
                try:
                    msg = data.decode('utf-8')
                    if msg != prev_msg:
                        prev_msg = msg
                        msg_ct += 1
                    print("Received data:", msg)
                except UnicodeDecodeError as decode_error:
                    send_data("error")
                    print(f"Error decoding data: {decode_error}")
                    continue
                   
                if start_eye_typing:                
                    try:

                        gaze_dir = r"C:/Users/Pierre/Desktop/HCI/dollarpy/gaze_points"
                        filename = f'p{msg_ct}.txt'
                        full_path = os.path.join(gaze_dir, filename)
                        
                        if not os.path.exists(full_path):
                            with open(full_path, 'w') as f:
                                pass 
                                
                        with open(full_path, 'a') as log_file:
                            if eye_points is not None:
                                log_file.write(eye_points)
                    except Exception as e:
                        print(f"Error logging to file: {e}")
                    
                    
                if msg == "1":
                    try:
                        send_msg = str(count_fingers(hand_landmarks.landmark))
                        send_data(send_msg)
                    except Exception as e:
                        send_data("error")
                        print(f"Error processing finger count: {e}")
                        continue
                        
                        
                elif msg == "2":
                    try:
                        send_msg = gesture_control(hand_points)
                        send_data(send_msg)
                        hand_points.clear()
                    except Exception as e:
                        send_data("error")
                        print(f"Error processing gesture control: {e}")
                        
                        
                        
                elif msg == "3":
                    try:
                        face_locations, face_names, process_this_frame = face_recognition_processing(
                            frame, known_face_encodings, known_face_names, True
                        )
                        if face_names != "Unknown":
                            recognized_person = face_names[0]
                            if recognized_person in user_data:
                                product_list = user_data[recognized_person]
                                data = f"{recognized_person}, {product_list}"
                            else:
                                data = recognized_person
                        else:
                            data = "Unknown"
                        send_data(data)
                        face_names.clear()
                    except Exception as e:
                        send_data("error")
                        print(f"Error in face recognition processing: {e}")
                        

                elif msg == "4":
                    try:
                        face_locations, face_names, process_this_frame = face_recognition_processing(
                            frame, known_face_encodings, known_face_names, True
                        )
                        deepface_results = analyze_faces_with_deepface(frame, face_locations)
                        filtered_results = [
                            {
                               "emotion": result.get("dominant_emotion"),
                            }
                            for result in deepface_results
                        ]
                        data_string = json.dumps(filtered_results)
                        data = json.loads(data_string)
                        emotion = data[0]["emotion"]
                        print(data_string)
                        print(type(emotion))
                        send_data(emotion)
                    except Exception as e:
                        send_data("error")
                        print(f"Error in DeepFace analysis: {e}")

                        
                        
                elif msg == "5":
                    try:
                        device_name = get_BT()
                        if device_name in user_data:
                            product_list = user_data[device_name]
                            data = f"{device_name}, {product_list}"
                        else:
                            data = device_name
                        send_data(data)
                    except Exception as e:
                        send_data("error")
                        print(f"Error sending confirmation for message '5': {e}")
                        
                        
                elif msg == "6":
                    try:
                        product_number = find_matches(reference_product, cropped_object_gray, orb)
                        send_data(str(product_number))
                    except Exception as e:
                        send_data("error")                        
                        print(f"Error in Object Detection : {e}")
                    
                        
                else:
                    send_data("error") 
                    print("Unknown message received.")
            else:
                print("No data received.")
                
                break
        except ConnectionError as conn_error:
            send_data("error")
            print(f"Connection error: {conn_error}")
            break
        except Exception as e:
            send_data("error")
            print(f"Unhandled error: {e}")
            break


read_thread = threading.Thread(target=read_data)
read_thread.start()

while cap.isOpened():
    ret, frame = cap.read()
    if framecnt == 0:
        send_data("?")
    
    if not ret:
        print("Can't receive frame (stream end?). Exiting ...")
        break
        
    frame = cv2.resize(frame, (480, 320))
    frame = cv2.flip(frame, 1)  
    framecnt += 1

    RGB = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    
    faces = detector(gray)
    if faces:
        face = faces[0]
        landmarks = predictor(gray, face)
        
        gaze_ratio_left_eye, left_eye_coords = get_gaze_ratio([36, 37, 38, 39, 40, 41], landmarks, frame, gray)
        gaze_ratio_right_eye, right_eye_coords = get_gaze_ratio([42, 43, 44, 45, 46, 47], landmarks, frame, gray)
        
        cv2.line(frame, 
         (int(left_eye_coords[0] - 5), int(left_eye_coords[1])),
         (int(left_eye_coords[0] + 5), int(left_eye_coords[1])),
         (0, 255, 0), 1)  
        cv2.line(frame, 
         (int(left_eye_coords[0]), int(left_eye_coords[1] - 5)),
         (int(left_eye_coords[0]), int(left_eye_coords[1] + 5)),
         (0, 255, 0), 1)  

        cv2.line(frame, 
         (int(right_eye_coords[0] - 5), int(right_eye_coords[1])),
         (int(right_eye_coords[0] + 5), int(right_eye_coords[1])),
         (0, 255, 0), 1) 
        cv2.line(frame, 
         (int(right_eye_coords[0]), int(right_eye_coords[1] - 5)),
         (int(right_eye_coords[0]), int(right_eye_coords[1] + 5)),
         (0, 255, 0), 1) 
        
        x_eye = (left_eye_coords [0] + right_eye_coords [0]) / 2
        y_eye = (left_eye_coords [1] + right_eye_coords [1]) /2
        
        eye_points = f"{x_eye}, {y_eye}, \n"
        start_eye_typing = True

        pose_results = pose.process(RGB)
        hand_results = hands.process(RGB)
        original_frame = frame.copy()
        results = model(frame)
        detections = results.xyxy[0].numpy()
        for detection in detections:
            x1, y1, x2, y2, confidence, class_id = detection
            if confidence < 0.5:
                continue
            x1, y1, x2, y2 = map(int, (x1, y1, x2, y2))
            cv2.rectangle(original_frame, (x1, y1), (x2, y2), (0, 255, 0), 2)
            label = f"Confidence: {confidence:.2f}"
            cv2.putText(
                original_frame,
                label,
                (x1, y1 - 10),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.5,
                (0, 255, 0),
                2,
            )
            cropped_object = frame[y1:y2, x1:x2]
            cropped_object_gray = cv2.cvtColor(cropped_object, cv2.COLOR_BGR2GRAY)
        try:
            gesture_key = None
            num_fingers = 0
            if pose_results.pose_landmarks:
                image_height, image_width, _ = frame.shape
                if hand_results.multi_hand_landmarks:
                    for hand_landmarks in hand_results.multi_hand_landmarks:
                        thumb_tip = hand_landmarks.landmark[4] 
                        x_thumb = int(thumb_tip.x * image_width)
                        y_thumb = int(thumb_tip.y * image_height)
                        hand_points.append(Point(x_thumb, y_thumb, 1))

            if hand_results.multi_hand_landmarks:
                for hand_landmarks in hand_results.multi_hand_landmarks:
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
                    num_fingers = count_fingers(hand_landmarks.landmark)
                    num_fingers = 3 if num_fingers >= 3 else num_fingers
                    hand_value = -1 if num_fingers >= 3  else hand_value
                    cv2.putText(frame, f'Fingers: {num_fingers}', (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)
            cv2.imshow('Gesture & Finger Counter', frame)

        except Exception as e:
            print('Error:', e)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

cap.release()
cv2.destroyAllWindows()
conn.close()
mySocket.close()