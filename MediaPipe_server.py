import socket
import cv2
import mediapipe as mp

mp_hands = mp.solutions.hands
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=1, min_detection_confidence=0.7)

mySocket = socket.socket()
mySocket.bind(('localhost', 5000))
mySocket.listen(5)
print("Waiting for a client to connect...")

conn, addr = mySocket.accept()
print(f"Device connected from {addr}")

cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret:
        break
    frame = cv2.flip(frame, 1)
    
    frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(frame_rgb)
    
    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            index_finger_tip = hand_landmarks.landmark[8]
            

            x, y, z = index_finger_tip.x, index_finger_tip.y, index_finger_tip.z
            coords = f"{x:.4f},{y:.4f},{z:.4f}"
            
            conn.send(coords.encode('utf-8'))
    
    cv2.imshow("Hand Tracking", frame)
    
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break


cap.release()
conn.close()
mySocket.close()
cv2.destroyAllWindows()
