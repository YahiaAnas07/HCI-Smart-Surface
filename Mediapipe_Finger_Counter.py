import cv2
import mediapipe as mp
mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
def count_fingers(hand_landmarks):
    thumb= 1 if hand_landmarks[4].x < hand_landmarks[3].x else 0
    index = 1 if hand_landmarks[8].y < hand_landmarks[6].y else 0
    middle = 1 if hand_landmarks[12].y < hand_landmarks[10].y else 0
    ring = 1 if hand_landmarks[16].y < hand_landmarks[14].y else 0
    pinky = 1 if hand_landmarks[20].y < hand_landmarks[18].y else 0

    return thumb + index + middle + ring + pinky

cap = cv2.VideoCapture(0)

with mp_hands.Hands(min_detection_confidence=0.7, min_tracking_confidence=0.7) as hands:
    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break
        frame = cv2.flip(frame, 1)
        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        result = hands.process(frame_rgb)

        if result.multi_hand_landmarks:
            for hand_landmarks in result.multi_hand_landmarks:
                mp_drawing.draw_landmarks(
                    frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
                num_fingers = count_fingers(hand_landmarks.landmark)
                cv2.putText(frame, f'Fingers: {num_fingers}', (10, 50),
                            cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)
        cv2.imshow('Finger Counter', frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break


cap.release()
cv2.destroyAllWindows()
