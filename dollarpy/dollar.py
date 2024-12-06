import os
import cv2
import mediapipe as mp

# Initialize Pose and Hands estimators
mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose
mp_hand = mp.solutions.hands

pose = mp_pose.Pose(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

hand = mp_hand.Hands(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

def loop_files(directory):
    f = open(directory + "Test.py", "w")
    f.write("from dollarpy import Recognizer, Template, Point\n")
    recstring = ""
    
    for file_name in os.listdir(directory):
        if os.path.isfile(os.path.join(directory, file_name)) and file_name.endswith(".mp4"):
            print(file_name)
            foo = file_name[:-4]
            recstring += foo + ","
            f.write(f"{foo} = Template('{foo}', [\n")
            
            # Create capture object
            cap = cv2.VideoCapture(os.path.join(directory, file_name))
            framecnt = 0
            
            while cap.isOpened():
                # Read frame from capture object
                ret, frame = cap.read()
                if not ret:
                    print("Can't receive frame (stream end?). Exiting ...")
                    break

                frame = cv2.resize(frame, (480, 320))
                framecnt += 1
                print(f"Frame {framecnt}")

                # Convert the frame to RGB format
                RGB = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

                # Process the RGB frame to get results
                pose_results = pose.process(RGB)
                hand_results = hand.process(RGB)
                
                # Ensure landmarks are detected before processing
                image_height, image_width, _ = frame.shape
                
                # if pose_results.pose_landmarks:
                #     # Extract pose landmarks (e.g., wrists)
                #     x_right = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].x * image_width)
                #     y_right = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.RIGHT_WRIST].y * image_height)
                #     f.write(f"Point({x_right},{y_right}, 1),\n")
                    
                #     x_left = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].x * image_width)
                #     y_left = int(pose_results.pose_landmarks.landmark[mp_pose.PoseLandmark.LEFT_WRIST].y * image_height)
                    # f.write(f"Point({x_left},{y_left}, 1),\n")

                if hand_results.multi_hand_landmarks:
                    # Extract thumb tip landmarks
                    for hand_landmarks in hand_results.multi_hand_landmarks:
                        thumb_tip = hand_landmarks.landmark[4]  # Landmark index for thumb tip
                        x_thumb = int(thumb_tip.x * image_width)
                        y_thumb = int(thumb_tip.y * image_height)
                        
                        # Write thumb tip coordinates to file
                        f.write(f"Point({x_thumb}, {y_thumb}, 0),\n")
                        
                        # Draw a circle at the thumb tip for visualization
                        cv2.circle(frame, (x_thumb, y_thumb), 10, (0, 255, 0), -1)
                    
                    # Optionally, draw the complete hand landmarks for context
                    mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hand.HAND_CONNECTIONS)


                # Draw pose landmarks on the frame
                # if pose_results.pose_landmarks:
                #     mp_drawing.draw_landmarks(frame, pose_results.pose_landmarks, mp_pose.POSE_CONNECTIONS)
                
                # Show the final output
                cv2.imshow('Output', frame)
                
                if cv2.waitKey(1) == ord('q'):
                    break

            f.write("])\n")
            cap.release()
            cv2.destroyAllWindows()
    
    recstring = recstring[:-1]  # Remove trailing comma
    f.write(f"recognizer = Recognizer([{recstring}])\n")
    f.close()

# Example usage
directory_path = "C:/Users/Pierre/Desktop/HCI/dollarpy/Thumbs_Left_Dynamic"
loop_files(directory_path)
