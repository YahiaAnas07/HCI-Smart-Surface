from dollarpy import Recognizer, Template, Point
import os
import cv2
import mediapipe as mp

# Initialize Pose estimator
mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose
mp_hands = mp.solutions.hands

pose = mp_pose.Pose(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5)
hands = mp_hands.Hands(
    static_image_mode=False,
    max_num_hands=2,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5)

def StartTest(directory):
    f = open("TemplatesDollar.Py", "a")
    recstring = ""
    for file_name in os.listdir(directory):
        if os.path.isfile(os.path.join(directory, file_name)):
            if file_name.endswith(".mp4"):
                f.write("\nresult = recognizer.recognize([")
                # Create capture object
                print(file_name)
                cap = cv2.VideoCapture(directory + "/" + file_name)
                framecnt = 0
                while cap.isOpened():
                    # Read frame from capture object
                    ret, frame = cap.read()
                    if not ret:
                        print("Can't receive frame (stream end?). Exiting ...")
                        break
                    frame = cv2.resize(frame, (480, 320))
                    framecnt += 1
                    try:
                        # Convert the frame to RGB format
                        RGB = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                        print(framecnt)
                        # Process the RGB frame to get the pose and hand results
                        pose_results = pose.process(RGB)
                        hand_results = hands.process(RGB)

                        # Get pose landmarks for wrists
                        image_height, image_width, _ = frame.shape
                        if pose_results.pose_landmarks:
                            right_wrist_x = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].x * image_width)
                            right_wrist_y = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].y * image_height)
                            f.write(f"Point({right_wrist_x},{right_wrist_y}, 1),\n")

                            left_wrist_x = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].x * image_width)
                            left_wrist_y = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].y * image_height)
                            f.write(f"Point({left_wrist_x},{left_wrist_y}, 1),\n")

                        # Get hand landmarks if detected
                        if hand_results.multi_hand_landmarks:
                            for hand_landmarks in hand_results.multi_hand_landmarks:
                                for i, landmark in enumerate(hand_landmarks.landmark):
                                    x = int(landmark.x * image_width)
                                    y = int(landmark.y * image_height)
                                    f.write(f"Point({x},{y}, {i+1}),\n")
                                    # Optionally, draw landmarks
                                mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                        # Draw pose landmarks on the frame
                        mp_drawing.draw_landmarks(frame, pose_results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
                        # Show the final output
                        cv2.imshow('Output', frame)
                    except:
                        break
                    if cv2.waitKey(1) == ord('q'):
                        break
                f.write("])\n")
                f.write("print(result)\n")
                cap.release()
                cv2.destroyAllWindows()
    f.close()

def loop_files(directory):
    f = open(directory + "TestDollar.Py", "w")
    f.write("from dollarpy import Recognizer, Template, Point\n")
    recstring = ""
    for file_name in os.listdir(directory):
        if os.path.isfile(os.path.join(directory, file_name)):
            if file_name.endswith(".mp4"):
                print(file_name)
                foo = file_name[:-4]
                recstring += foo + ","
                f.write(f"{foo} = Template('{foo}', [\n")
                # Create capture object
                cap = cv2.VideoCapture(directory + "" + file_name)
                framecnt = 0
                while cap.isOpened():
                    # Read frame from capture object
                    ret, frame = cap.read()
                    if not ret:
                        print("Can't receive frame (stream end?). Exiting ...")
                        break
                    frame = cv2.resize(frame, (480, 320))
                    framecnt += 1

                    # Convert the frame to RGB format
                    RGB = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                    print(framecnt)
                    # Process the RGB frame to get the pose and hand results
                    pose_results = pose.process(RGB)
                    hand_results = hands.process(RGB)
                    image_height, image_width, _ = frame.shape

                    # Get pose landmarks for wrists
                    if pose_results.pose_landmarks:
                        right_wrist_x = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].x * image_width)
                        right_wrist_y = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].y * image_height)
                        f.write(f"Point({right_wrist_x},{right_wrist_y}, 1),\n")

                        left_wrist_x = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].x * image_width)
                        left_wrist_y = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].y * image_height)
                        f.write(f"Point({left_wrist_x},{left_wrist_y}, 1),\n")

                    # Get hand landmarks if detected
                    if hand_results.multi_hand_landmarks:
                        for hand_landmarks in hand_results.multi_hand_landmarks:
                            for i, landmark in enumerate(hand_landmarks.landmark):
                                x = int(landmark.x * image_width)
                                y = int(landmark.y * image_height)
                                f.write(f"Point({x},{y}, {i+1}),\n")
                                # Optionally, draw landmarks
                            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                    # Draw pose landmarks on the frame
                    mp_drawing.draw_landmarks(frame, pose_results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
                    # Show the final output
                    cv2.imshow('Output', frame)

                    if cv2.waitKey(1) == ord('q'):
                        break
                f.write("])\n")
                cap.release()
                cv2.destroyAllWindows()
    recstring = recstring[:-1]
    f.write(f"recognizer = Recognizer([{recstring}])\n")
    f.close()

# Example usage
directory_path = "C:/Users/Pierre/Desktop/dollarpy/Videos/"
loop_files(directory_path)


