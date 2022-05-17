using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WalktroughManager : MonoBehaviour {

    [SerializeField]
    public static WalktroughManager Instance {
        get; private set;
    }
    public List<GameObject> Buttons;
    public int Order;
    public float Progress;

    public List<WalktroughStep> WalktroughSteps =  new List<WalktroughStep>{

        new WalktroughStep {
            Order = 0,
            PrimaryText = "",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 1,
            Progress = -1,
            PrimaryText = "Blue panel will tell you all about highlighted functionality.",
            SecondaryText = "Green is focused on the task and worflow.",
            Tip = "You will find usefull tips and tricks here.",
            HighlitedButton = null,
        },
        
        new WalktroughStep {
            Order = 2,
            PrimaryText = "This button is used for adding scenes or procejts",
            SecondaryText = "Let's create our first scene.",
            Tip = "Scenes are used to create your workplace.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 3,
            PrimaryText = "Add a scene name, then press Done.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 4,
            Progress = -1,
            PrimaryText = "Nice! \n The highlighted crosshair is your main selector.",
            SecondaryText = "Tap on the next button.",
            Tip = "Aim it on the object you want to work with.",
            //Tip = "Left menu contains tools and functionalities and right menu holds objects which are placed in the scene.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 5,
            PrimaryText = "Using the ADD button allows you to add objects.",
            SecondaryText = "We are going to add new virtual object representing the robot",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 6,
            PrimaryText = "This button is used to add predefined objects like robots.",
            SecondaryText = "Let's add the robot.",
            Tip = "Second button allows you to create collision objects of basic shapes.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 7,
            PrimaryText = "Right menu contains objects in scenes or detailed functionality options .",
            SecondaryText = "Now we can choose the robot or device that we have available.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 8,
            PrimaryText = "",
            SecondaryText = "",
            Tip = "Enter information in the input fields and create an object.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 9,
            Progress = -1,
            PrimaryText = "Great! \n Renaming, deleting, settings and other actions are avaiable in Utility menu.",
            SecondaryText = "You can now position the virtual object to match the real robot.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 10,
            PrimaryText = "This tool allows you to rotate and position the object you are aiming at.",
            SecondaryText = "Let's pick movement tool.",
            Tip = "Moving pricniples are the same throughout AREditor.",
            HighlitedButton = null,
        },





        new WalktroughStep {
            Order = 11,
            PrimaryText = "When you aim at one of the arrows, you can move or rotate thr object the using slider on the left.\n You can switch rotation or positioning under slider.",
            SecondaryText = "Tap the next button.",
            Tip = "Three arrows represent three axis of movement and rotation",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 12,
            PrimaryText = "Move around the workplace to see different perspectives.",
            SecondaryText = "Try aligning the bases to see if the outline matches the edges of the actual base.\n If you think It's ready hit next.",
            Tip = "Keep the calibration QR-code in sight.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 13,
            PrimaryText = "Awesome! \n Home button contains save funcion, settings and exit scene button.",
            SecondaryText = "Now let's create new project in this scene.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 14,
            PrimaryText = "Click on the highlighted icon.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 15,
            PrimaryText = "Name your first project.",
            SecondaryText = "We will create simple movement of the robot arm.",
            Tip = "Project contains actions connected together that form the final program.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 16,
            Progress = -1,
            PrimaryText = "Good job! \n We can already see 2 actions.",
            SecondaryText = "Now we can create your first program.",
            Tip = "START is beggining of the program and END marks its end.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 17,
            Skippable = true,
            Progress = 23,
            PrimaryText = "Firstly we have to be connected to the robot.",
            SecondaryText = "If you're having trouble connecting, just skip to action point creation.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 18,
            Skippable = true,
            Progress = 23,
            PrimaryText = "We can move a robot arm using hands or slider.",
            SecondaryText = "Let's add the first action point using a robotic arm.",
            Tip = "Action point is anchor in 3D space that will hold action like move or pick-up.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 19,
            Skippable = true,
            Progress = 23,
            PrimaryText = "This function allows you to move robotic arm in desired position.",
            SecondaryText = "",
            Tip = "If you're having trouble connecting, just skip to action point creation.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 20,
            Skippable = true,
            Progress = 23,
            PrimaryText = "Hold the button pressed until you're finished with positioning.",
            SecondaryText = "While holding the button, try to move a robotic arm in any direction then press next.",
            Tip = "Some robots have this function as hardware button.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 21,
            Skippable = true,
            Progress = 23,
            PrimaryText = "Nicely done! \n Now we are ready to add our first action point.",
            SecondaryText = "When creating an action point, aim close to the virtual robot object.",
            Tip = "Object have to be whitin reach of the robotic arm.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 22,
            Skippable = true,
            Progress = 23,
            PrimaryText = "In this menu you will find adding actions, action points and connections.",
            SecondaryText = "Let's add action point at position of the robotic arm tool that we set earlier.",
            Tip = "You can see the purple arrow showing position and orientation of robot arm tool.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 23,
            Skippable = true,
            PrimaryText = "You can use the default name.",
            SecondaryText = "Then click done.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 24,
            PrimaryText = "Nice! Let's create action point from scratch.",
            SecondaryText = "Tap the Add button.",
            Tip = "Every action is bound to the action point.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 25,
            PrimaryText = "Orientation will need to be added to this action point.",
            SecondaryText = "",
            Tip = "Whitout orientation, you can't perform actions like move.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 26,
            PrimaryText = "You can use the default name.",
            SecondaryText = "Than click done.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 27,
            PrimaryText = "Orientation of the action point means the direction from which will robot position the tool.",
            SecondaryText = "Let's add the orientation to the action point.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 28,
            PrimaryText = "Orientation can be found in right panel with other Utility tools.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },


        new WalktroughStep {
            Order = 29,
            PrimaryText = "Tap on round arrow in the bottom of the right menu to access orientaions.",
            SecondaryText = "",
            Tip = "You can use robot to add orientation too.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 30,
            PrimaryText = "Pick manual setting of orientation.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 31,
            PrimaryText = "Setting Y to -90.0 will add orientation straight down.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 32,
            Progress = -1,
            PrimaryText = "Cool! \n Add button contains action, connetion and action point creation buttons.",
            SecondaryText = "Now we can add action to our action point.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 33,
            PrimaryText = "Actions like move, pick and place can be added here.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 34,
            PrimaryText = "Let's click on our earlier added robot to see avaiable actions",
            SecondaryText = "After opening the action menu click Next",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 35,
            PrimaryText = "Pick move action from the menu.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 36,
            PrimaryText = "And name it: first move.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 37,
            Progress = -1,
            PrimaryText = "Awesome! \n To make it more interesting let's use copy tool on created action point.",
            SecondaryText = "So we can create movement from one point to the other.",
            Tip = "If any object has red background in right panel it means it's locked just tap tu unlock it",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 38,
            PrimaryText = "Aim the crosshair on manualy created action point and then click on highlighted button.",
            SecondaryText = "",
            Tip = "If any object has red background in right panel it means it's locked just tap tu unlock it.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 39,
            PrimaryText = "Name second action point. But you can leave the default.",
            SecondaryText = "",
            Tip = "Copying action point will copy also its orientation and action.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 40,
            PrimaryText = "Tap the Utility menu that contains all adjustment functionalities.",
            SecondaryText = "Let's position the copied object away from the original action point.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 41,
            PrimaryText = "Now pick the movement tool.",
            SecondaryText = "",
            Tip = "You need to aim at the action point you want to move.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 42,
            PrimaryText = "By aiming the crosshair on arrow you can select direction of movement. \n Adjusting is possible by rotating wheel in right menu.",
            SecondaryText = "Just move It away but not too far and hit next.",
            Tip = "You can also use hand button for rough placment.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 43,
            PrimaryText = "Great! \n We are ready to connect actions from START to the END.",
            SecondaryText = "Click on highlighted button.",
            Tip = "Connecting actions between START and END will create loop in which will be actions executed.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 44,
            PrimaryText = "Connections are used to specify order of execution.",
            SecondaryText = "Aim crosshair on START action and then click on highlighted button.",
            Tip = "Connection begins from action that you aim at.",
            HighlitedButton = null,
        },

       new WalktroughStep {
            Order = 45,
            PrimaryText = "Now you can see line from START and we can drag this line to first move action.",
            SecondaryText = "When aiming at the first move action tap on It in right menu to create connection.",
            Tip = "If you already see action in right menu just tap on it.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 46,
            PrimaryText = "Now we have to aim at action you connected in previous step.",
            SecondaryText = "Then click on highlighted button to start connection and drag it to the copied move action.",
            Tip = "Draging is only necesery If you can't already see second action in left menu.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 47,
            PrimaryText = "Aim the crosshair on copied move action and press highlighted button.\n",
            SecondaryText = "Tap on the END action in right panel to finish connection.",
            Tip = "To cancel connection there is red button down at the screen.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 48,
            PrimaryText = "Great Job! \n Now we are going to save the file and try your first program.",
            SecondaryText = "",
            Tip = "Whitout saving you are unable to run program.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 49,
            PrimaryText = "Just hit the save button.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 50,
            PrimaryText = "Now let's click on home button.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 51,
            PrimaryText = "And now you can enjoy watching your programed task.",
            SecondaryText = "",
            Tip = "If you struggle to find any functionality just hold your finger on button to see tooltip.",
            HighlitedButton = null,
        }
    };

    public void Awake() {
        Instance = this;
        Order = 1;
    }
}
