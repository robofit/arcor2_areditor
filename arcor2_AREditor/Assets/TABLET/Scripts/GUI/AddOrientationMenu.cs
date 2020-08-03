using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System.Linq;
using UnityEngine.UI;
using DanielLochner.Assets.SimpleSideMenu;
using Base;
using System;
using Packages.Rider.Editor.UnitTesting;
using OrbCreationExtensions;
using UnityEngine.Events;

[RequireComponent(typeof(SimpleSideMenu))]
public class AddOrientationMenu : MonoBehaviour, IMenu {
    public Base.ActionPoint CurrentActionPoint;

    [SerializeField]
    private TMPro.TMP_Text NoOrientation, NoJoints, ActionPointName;


   
    [SerializeField]
    private InputDialog inputDialog;

    private SimpleSideMenu SideMenu;

   
    private void Start() {
        SideMenu = GetComponent<SimpleSideMenu>();
        //ProjectManager.Instance.OnActionPointUpdated += OnActionPointUpdated;
    }

  

    public void UpdateMenu(string preselectedOrientation = null) {
        //ActionPointName.text = CurrentActionPoint.Data.Name;

        /*
        CustomDropdown robotsListDropdown = RobotsList.Dropdown;
        CustomDropdown positionRobotsListDropdown = PositionRobotsList.Dropdown;
        robotsListDropdown.dropdownItems.Clear();
        positionRobotsListDropdown.dropdownItems.Clear();

        RobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, false);
        if (robotsListDropdown.dropdownItems.Count == 0) {
            UpdatePositionBlock.SetActive(false);
        } else {
            OnRobotChanged((string) RobotsList.GetValue());
            UpdatePositionBlock.SetActive(true);

        }
        PositionRobotsList.gameObject.GetComponent<DropdownRobots>().Init(OnRobotChanged, false);
        if (positionRobotsListDropdown.dropdownItems.Count == 0) {
            UpdatePositionBlock.SetActive(false);
        } else {
            OnRobotChanged((string) PositionRobotsList.GetValue());
            UpdatePositionBlock.SetActive(true);

        }
        */

    }


    public void ShowMenu(Base.ActionPoint actionPoint, string preselectedOrientation = null) {
        CurrentActionPoint = actionPoint;
        UpdateMenu(preselectedOrientation);
        SideMenu.Open();
    }

    public void Close() {
        SideMenu.Close();
    }

    public void UpdateMenu() {
        UpdateMenu(null);
    }

    
}
