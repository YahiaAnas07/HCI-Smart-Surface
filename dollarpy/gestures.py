from dollarpy import Recognizer, Template, Point

tmpl_1 = Template('X', [
    Point(0, 0, 1),
    Point(1, 1, 1),
    Point(0, 1, 2),
    Point(1, 0, 2)])
tmpl_2 = Template('line', [
    Point(0, 0),
    Point(1, 0)])

HCI_LEC= Template('HCI',[
    Point( 100, 450 ,1),
    Point( 102, 460 ,1),
    Point( 103, 470 ,1),
])
recognizer = Recognizer([tmpl_1, tmpl_2,HCI_LEC])

# Call 'recognize(...)' to match a list of 'Point' elements to the previously defined templates.
result = recognizer.recognize([
    Point( 100, 450, 1),
    Point(102, 460, 1)])
print(result)  # Output: ('X', 0.733770116545184)