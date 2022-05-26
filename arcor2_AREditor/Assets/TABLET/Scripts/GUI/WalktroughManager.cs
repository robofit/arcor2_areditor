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
            PrimaryText = "Welcome to onboarding.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },
        
        new WalktroughStep {
            Order = 2,
            PrimaryText = "This button is used for adding scenes or projects.",
            SecondaryText = "Let's create our first scene.",
            Tip = "Scenes are used to create your workplace.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 3,
            PrimaryText = "Add a scene name, then press Create.",
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
            PrimaryText = "Great! \n Renaming, deleting, settings and other actions are available in Utility menu.",
            SecondaryText = "You can now position the virtual object to match the real robot.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 10,
            PrimaryText = "This tool allows you to rotate and position the object you are aiming at.",
            SecondaryText = "Let's pick the movement tool.",
            Tip = "Moving pricniples are the same throughout AREditor.",
            HighlitedButton = null,
        },





        new WalktroughStep {
            Order = 11,
            PrimaryText = "When you aim at one of the arrows, you can move or rotate the object, by using slider on the right.\n",
            SecondaryText = "Three arrows represent three axis of movement and rotation. Try to use slider and tap next.",
            Tip = "You can switch rotation or positioning under slider.",
            HighlitedButton = null,
        },

        // lepsie popisat slajder s ktorym sa hybe a co treba urobit pri tom zarovnani
        // zvyraznit sladjer a nie len cele prave okno
        // zoznamit uzivatela lepsie s rotovanim

        new WalktroughStep {
            Order = 12,
            PrimaryText = "Move around the workplace to see different perspectives.",
            SecondaryText = "Try moving the outline so it matches the real robot.\n If you think It's ready hit next.",
            Tip = "You can switch rotation or positioning under slider.",
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
            PrimaryText = "Good job! \n Look around for two arrows one GREEN and one RED.",
            SecondaryText = "GREEN arrow represents START.\n RED arrow represents the END.\n Now we can create the first program.",
            Tip = "",
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
            PrimaryText = "Press and hold the button, and try to move a robotic arm in any direction.",
            SecondaryText = "",
            Tip = "Some robots have this function as hardware button.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 21,
            Skippable = true,
            Progress = 23,
            PrimaryText = "Nicely done! \n Now we are ready to add our first ACTION POINT represented by BALL.",
            SecondaryText = "Aim at the robot while creating action point.",
            Tip = "Object have to be whitin reach of the robotic arm.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 22,
            Skippable = true,
            Progress = 23,
            PrimaryText = "In this menu you will find adding actions, action points and connections.",
            SecondaryText = "You can add ACTION POINT - (BALL) on the tip of robotic arm.",
            Tip = "You can see the purple arrow showing position and orientation of robotic arm.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 23,
            PrimaryText = "You can use the default name.",
            SecondaryText = "Then click done.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 24,
            PrimaryText = "Nice! Let's create blank ACTION POINT represented by BALL.",
            SecondaryText = "Tap the Add button.",
            Tip = "Every ACTION - (YELLOW ARROW) is bound to the ACTION POINT (BALL).",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 25,
            Skippable = true,
            Progress = 43,
            PrimaryText = "When adding ACTION POINT, ensure its within the reach of the robot.",
            SecondaryText = "",
            Tip = "ACTION POINT will be added right in front of you, so try to stand and aim near the robotic arm.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 26,
            PrimaryText = "You can use the default name.",
            SecondaryText = "Next we need to add ORIENTATION.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 27,
            PrimaryText = "Orientation of the ACTION POINT means the direction from which will robot position the tool.",
            SecondaryText = "Let's add the orientation to the ACTION POINT.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 28,
            PrimaryText = "Orientation can be found in right panel with other UTILITIES.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },


        new WalktroughStep {
            Order = 29,
            PrimaryText = "Tap on ROUND ARROW in the BOTTOM OF THE RIGHT MENU to access orientaions.",
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
            PrimaryText = "Setting Y to -90.0 will add orientation heading straight down.",
            SecondaryText = "",
            Tip = "If any object has red background in right panel, that means locked selection, you can TAP to LOCK or UNLOCK the object. Try it!",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 32,
            Progress = -1,
            PrimaryText = "Cool! \n ADD button contains creation tools for: ACTION (YELLOW ARROW), connetion and ACTION POINT (BALL).",
            SecondaryText = "Now we can add ACTION (YELLOW ARROW) to our ACTION POINT (BALL).",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 33,
            PrimaryText = "Actions like move, pick and place can be added here.",
            SecondaryText = "ACTION is represented by PUCK in left menu.",
            Tip = "",
            HighlitedButton = null,
        },

            
        
        new WalktroughStep {
            Order = 34,
            PrimaryText = "Let's click on robot you added earlier to see available actions",
            SecondaryText = "After opening the ACTION MENU click Next",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 35,
            PrimaryText = "Pick MOVE action from the menu.",
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
            PrimaryText = "Awesome! \n To make it more interesting let's use COPY tool on created ACTION POINT (BALL).",
            SecondaryText = "So we can create movement from one point to the other.",
            Tip = "If any object has red background in right panel, that means locked selection, you can TAP to LOCK or UNLOCK the object.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 38,
            PrimaryText = "Aim the crosshair on manualy created ACTION POINT (BALL) and then click on highlighted button.",
            SecondaryText = "",
            Tip = "If any object has red background in right panel, that means locked selection, you can TAP to LOCK or UNLOCK the object.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 39,
            PrimaryText = "Name second action point. But you can leave the default.",
            SecondaryText = "",
            Tip = "Copying ACTION POINT (BALL) will copy also its ORIENTATION and ACTION (YELLOW ARROW).",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 40,
            PrimaryText = "Tap the UTILITY menu that contains all adjustment functionalities.",
            SecondaryText = "Let's position the copied object away from the original ACTION POINT (BALL) .",
            Tip = "",
            HighlitedButton = null,
        },

        // upravit tie dialogy

        new WalktroughStep {
            Order = 41,
            PrimaryText = "Now pick the MOVEMENT tool.",
            SecondaryText = "",
            Tip = "You need to aim at the ACTION POINT (BALL) you want to move.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 42,
            PrimaryText = "By aiming the crosshair on arrows you can select direction of movement or point of rotation. \n Adjusting is possible by rotating wheel in right menu.",
            SecondaryText = "Just move It away but not too far and hit next.",
            Tip = "You can also use hand button for rough placment. Try it!",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 43,
            PrimaryText = "Great! \n We are ready to connect ACTIONS (YELLOW ARROWS) from START (GREEN ARROW) to the END (RED ARROW).",
            SecondaryText = "Click on highlighted button.",
            Tip = "Connecting actions between START and END will create loop in which will be actions executed.",
            HighlitedButton = null,
        },

        // rozhodit na viac krokov a probnejsie vysvetlit
        // lepsie zvyraznit ktore je start a ktore je end action
        // 

        new WalktroughStep {
            Order = 44,
            PrimaryText = "Connections are used to specify order of execution.",
            SecondaryText = "Aim crosshair on START (GREEN ARROW) and then click on highlighted button.",
            Tip = "Connection begins from action that you aim at.",
            HighlitedButton = null,
        },

       new WalktroughStep {
            Order = 45,
            PrimaryText = "Now you can see line from START (GREEN ARROW) and we can drag this line to first move ACTION (YELLOW ARROW).",
            SecondaryText = "When aiming at the first move action tap on It in right menu to create connection. \n Then tap NEXT",
            Tip = "If you already see action in right menu just tap on it.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 46,
            PrimaryText = "Now we have to aim at ACTION (YELLOW ARROW) you connected in previous step.",
            SecondaryText = "Then click on highlighted button to start connection and drag it to the copied move ACTION (YELLOW ARROW).",
            Tip = "Draging is only necesery If you can't already see second action in left menu.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 47,
            PrimaryText = "Taping on the ACTION (YELLOW ARROW) in right panel will create connection.",
            SecondaryText = "When aiming at the copied move ACTION (YELLOW ARROW) tap on It in right menu to create connection. \n Then tap NEXT.",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 48,
            PrimaryText = "Aim the crosshair on copied move action and press highlighted button.\n",
            SecondaryText = "Find END (RED ARROW) with your crosshair.",
            Tip = "To cancel connection there is red button down at the screen.",
            HighlitedButton = null,
        },


        new WalktroughStep {
            Order = 49,
            PrimaryText = "All actions near crosshair are shown in left panel. ",
            SecondaryText = "Tap on the END action in right panel to finish connection.\n Then tap NEXT.",
            Tip = "To change connection you can just create new from ACTION (YELLOW ARROW) it starts from.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 50,
            PrimaryText = "Great Job! \n Now we are going to save the file and try your first program.",
            SecondaryText = "",
            Tip = "Without saving you are unable to run program.",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 51,
            PrimaryText = "Just hit the save button.",
            SecondaryText = "",
            Tip = "",
            HighlitedButton = null,
        },

        new WalktroughStep {
            Order = 53,
            PrimaryText = "And now you can enjoy watching your programed task.",
            SecondaryText = "Aim at START (GREEN ARROW) and click RUN button.",
            Tip = "If you have problems finding funcitionality just use the TOOL TIPS.",
            HighlitedButton = null,
        }
    };

    public void Awake() {
        Instance = this;
        Order = 1;
    }
}
