import face_recognition
import cv2
import numpy as np
from deepface import DeepFace

def analyze_faces_with_deepface(frame, face_locations):
    deepface_results = []
    for location in face_locations:
        top, right, bottom, left = [v * 4 for v in location]
        face_image = frame[top:bottom, left:right]

        try:
            result = DeepFace.analyze(face_image, actions=['age', 'gender', 'emotion'], enforce_detection=False)
            if isinstance(result, list):
                result = result[0] 
            deepface_results.append(result)
        except Exception as e:
            
            deepface_results.append(None)
    return deepface_results

def load_deepface():
    dummy_image = np.zeros((224, 224, 3), dtype=np.uint8)
    try:
        DeepFace.analyze(img_path=dummy_image, actions=['age', 'gender', 'emotion'], enforce_detection=False)
        print("DeepFace model preloaded successfully.")
    except Exception as e:
        print(f"Error during loading DeepFace: {e}")
        
def recognize_faces(rgb_frame, known_face_encodings, known_face_names):
    face_locations = face_recognition.face_locations(rgb_frame)
    face_encodings = face_recognition.face_encodings(rgb_frame, face_locations)

    face_names = []
    for face_encoding in face_encodings:
        matches = face_recognition.compare_faces(known_face_encodings, face_encoding)
        name = "Unknown"

        face_distances = face_recognition.face_distance(known_face_encodings, face_encoding)
        best_match_index = np.argmin(face_distances)
        if matches[best_match_index]:
            name = known_face_names[best_match_index]

        face_names.append(name)

    return face_locations, face_names

def load_faces():
    khaled_image = face_recognition.load_image_file(r"C:\Users\Asus\source\repos\HCI-Smart-Surface\images\khaled.jpg")
    khaled_face_encoding = face_recognition.face_encodings(khaled_image)[0]

    aly_image = face_recognition.load_image_file(r"C:\Users\Asus\source\repos\HCI-Smart-Surface\images\khaled.jpg")
    aly_face_encoding = face_recognition.face_encodings(aly_image)[0]

    known_face_encodings = [khaled_face_encoding, aly_face_encoding]
    known_face_names = ["Khaled Mohamed", "Aly Essam"]
    
    return known_face_encodings, known_face_names
    

