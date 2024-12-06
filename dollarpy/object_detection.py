import torch
import cv2
import numpy as np


def load_model_and_reference_images(model_path='ultralytics/yolov5', model_name='yolov5s', reference_image_paths=None):
    if reference_image_paths is None:
        reference_image_paths = ["images/Product1.jpeg", "images/Product2.jpeg", "images/Product3.jpeg"]

    print("Loading YOLO model...")
    model = torch.hub.load(model_path, model_name)

    print("Loading reference images...")
    reference_products = []
    for idx, path in enumerate(reference_image_paths):
        img = cv2.imread(path, cv2.IMREAD_GRAYSCALE)
        if img is None:
            raise FileNotFoundError(f"Reference image {idx+1} not found at {path}.")
        reference_products.append(img)

    print("Initializing ORB detector...")
    orb = cv2.ORB_create()
    return model, reference_products, orb


def find_matches(reference_products, cropped_object, orb, threshold=50):
    max_matches = 0
    best_index = 0

    for idx, reference_product in enumerate(reference_products):
        keypoints1, descriptors1 = orb.detectAndCompute(reference_product, None)
        keypoints2, descriptors2 = orb.detectAndCompute(cropped_object, None)

        if descriptors1 is None or descriptors2 is None:
            continue

        bf = cv2.BFMatcher(cv2.NORM_HAMMING, crossCheck=True)
        matches = bf.match(descriptors1, descriptors2)

        if len(matches) > max_matches:
            max_matches = len(matches)
            best_index = idx + 1

    return best_index if max_matches > threshold else 0






