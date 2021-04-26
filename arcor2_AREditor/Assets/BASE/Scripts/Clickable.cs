using System;
using UnityEngine;

namespace Base
{
    public abstract class Clickable : MonoBehaviour
    {
        public enum Click {
            MOUSE_LEFT_BUTTON = 0,
            MOUSE_RIGHT_BUTTON = 1,
            MOUSE_MIDDLE_BUTTON = 2,
            MOUSE_HOVER = 3,
            TOUCH = 4,
            LONG_TOUCH = 5,
            TOUCH_ENDED = 6,
            
        }

        public bool Enabled = true;

        // Call using SendMessage("OnClick", Base.Clickable.Click.MOUSE_LEFT_BUTTON) to specify which button caused the click.
        // Implement by inheriting from Clickable abstract class.
        public abstract void OnClick(Click type);

        public abstract void OnHoverStart();

        public abstract void OnHoverEnd();

        
    }
}
