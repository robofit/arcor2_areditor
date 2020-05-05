using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using System.Globalization;
using Michsky.UI.ModernUIPack;

public class NewObjectTypeMenu : Base.Singleton<NewObjectTypeMenu>, IMenu {
    [SerializeField]
    private GameObject BoxMenu, CylinderMenu, SphereMenu, MeshMenu;
    [SerializeField]
    private Dictionary<string, GameObject> ModelMenus;
    public TMPro.TMP_InputField NameInput, BoxX, BoxY, BoxZ, SphereRadius, CylinderHeight, CylinderRadius, MeshId;
    public DropdownParameter ParentsList, ModelsList;
    [SerializeField]
    private Button CreateNewObjectBtn;

    [SerializeField]
    private TooltipContent buttonTooltip;



    private void Awake() {
        ModelMenus = new Dictionary<string, GameObject>() {
            { "None", null },
            { "Box", BoxMenu },
            { "Sphere", SphereMenu },
            { "Cylinder", CylinderMenu },
            { "Mesh", MeshMenu },
            
        };
    }

    // Start is called before the first frame update
    private void Start() {
        //TODO: find out why start is called twice
        Base.ActionsManager.Instance.OnActionObjectsUpdated += UpdateObjectsList;
        buttonTooltip.descriptionText = TooltipRef.Instance.Text;
        buttonTooltip.tooltipRect = TooltipRef.Instance.Tooltip;
    }

    // Update is called once per frame
    private void Update() {

    }

    public void UpdateMenu() {
        CreateNewObjectBtn.interactable = false;
        ValidateFields();
    }

    public void UpdateModelsMenu() {
        ValidateFields();
        string modelType = (string) ModelsList.GetValue();
        if (ModelMenus.TryGetValue(modelType, out GameObject menu)) {
            ShowModelMenu(menu);
        }
    }

    private void ShowModelMenu(GameObject menu) {
        BoxMenu.SetActive(false);
        CylinderMenu.SetActive(false);
        SphereMenu.SetActive(false);
        MeshMenu.SetActive(false);
        if (menu != null)
            menu.SetActive(true);
    }

    public void UpdateObjectsList(object sender, Base.StringEventArgs eventArgs) {
        string originalValue = "";
        if (ParentsList.Dropdown.dropdownItems.Count > 0)
            originalValue = (string) ParentsList.GetValue();

        List<string> values = new List<string>();
        foreach (Base.ActionObjectMetadata actionObjectMetadata in Base.ActionsManager.Instance.ActionObjectMetadata.Values) {
            values.Add(actionObjectMetadata.Type);
        }
        ParentsList.PutData(values, originalValue, null);
        
    }

    public async void ValidateFields() {
        bool interactable = true;
        if (string.IsNullOrEmpty(NameInput.text)) {
            buttonTooltip.description = "Name is required parameter";
            interactable = false;
        }
        if (interactable) {
            string modelType = (string) ModelsList.GetValue();
            switch (modelType) {
                case "Box":
                    if (string.IsNullOrEmpty(BoxX.text) ||
                        string.IsNullOrEmpty(BoxY.text) ||
                        string.IsNullOrEmpty(BoxZ.text))
                        interactable = false;
                    break;
                case "Sphere":
                    if (string.IsNullOrEmpty(SphereRadius.text))
                        interactable = false;
                    break;
                case "Cylinder":
                    if (string.IsNullOrEmpty(CylinderHeight.text) ||
                        string.IsNullOrEmpty(CylinderRadius.text))
                        interactable = false;
                    break;
                case "Mesh":
                    if (string.IsNullOrEmpty(MeshId.text))
                        interactable = false;
                    break;
            }
            if (!interactable) {
                buttonTooltip.description = "Some parameters has invalid value";
            }
        }
        if (interactable) {
            try {
                await Base.WebsocketManager.Instance.CreateNewObjectType(CreateObjectTypeMeta(), true);
            } catch (Base.RequestFailedException ex) {
                buttonTooltip.description = ex.Message;
                interactable = false;
            }
        }

        buttonTooltip.enabled = !interactable;
        
        CreateNewObjectBtn.interactable = interactable;
    }

    public async void CreateNewObjectType() {
        Debug.Assert(ModelsList.Dropdown.dropdownItems.Count > 0, "No models");
        Debug.Assert(ParentsList.Dropdown.dropdownItems.Count > 0, "No parent objects");
        Base.GameManager.Instance.StartLoading();
        CreateNewObjectBtn.interactable = false;
        
        bool success = await Base.GameManager.Instance.CreateNewObjectType(CreateObjectTypeMeta());
        if (success) {
            MenuManager.Instance.NewObjectTypeMenu.Close();
        }
        CreateNewObjectBtn.interactable = true;
        Base.GameManager.Instance.EndLoading();
    }

    public IO.Swagger.Model.ObjectTypeMeta CreateObjectTypeMeta() {
        string objectId = NameInput.text;

        IO.Swagger.Model.ObjectModel objectModel = new IO.Swagger.Model.ObjectModel();
        string modelTypeString = (string) ModelsList.GetValue();
        IO.Swagger.Model.ObjectTypeMeta objectTypeMeta;
        if (ModelMenus.TryGetValue(modelTypeString, out GameObject type) && type != null) {
            IO.Swagger.Model.ObjectModel.TypeEnum modelType = new IO.Swagger.Model.ObjectModel.TypeEnum();
            switch (modelTypeString) {
                case "Box":
                    modelType = IO.Swagger.Model.ObjectModel.TypeEnum.Box;
                    decimal sizeX = decimal.Parse(BoxX.text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    decimal sizeY = decimal.Parse(BoxY.text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    decimal sizeZ = decimal.Parse(BoxZ.text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    // no need to transform from Unity to ROS, because we are setting those scales in ROS already
                    //(sizeX, sizeY, sizeZ) = TransformConvertor.UnityToROSScale(sizeX, sizeY, sizeZ);
                    IO.Swagger.Model.Box box = new IO.Swagger.Model.Box(objectId, sizeX, sizeY, sizeZ);
                    objectModel.Box = box;
                    break;
                case "Sphere":
                    modelType = IO.Swagger.Model.ObjectModel.TypeEnum.Sphere;
                    decimal radius = decimal.Parse(SphereRadius.text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    IO.Swagger.Model.Sphere sphere = new IO.Swagger.Model.Sphere(objectId, radius);
                    objectModel.Sphere = sphere;
                    break;
                case "Cylinder":
                    modelType = IO.Swagger.Model.ObjectModel.TypeEnum.Cylinder;
                    decimal cylinderRadius = decimal.Parse(CylinderRadius.text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    decimal cylinderHeight = decimal.Parse(CylinderHeight.text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    IO.Swagger.Model.Cylinder cylinder = new IO.Swagger.Model.Cylinder(objectId, cylinderHeight, cylinderRadius);
                    objectModel.Cylinder = cylinder;
                    break;
                case "Mesh":
                    modelType = IO.Swagger.Model.ObjectModel.TypeEnum.Mesh;
                    string meshId = MeshId.text;
                    IO.Swagger.Model.Mesh mesh = new IO.Swagger.Model.Mesh(id: meshId, focusPoints: new List<IO.Swagger.Model.Pose>(), uri: "");
                    objectModel.Mesh = mesh;
                    break;
                default:
                    Debug.LogError("Model not defined!");
                    return null;
            }
            objectModel.Type = modelType;
            objectTypeMeta = new IO.Swagger.Model.ObjectTypeMeta(builtIn: false, description: "", type: objectId, objectModel: objectModel,
                _base: (string) ParentsList.GetValue(), needsServices: new List<string>());
        } else {
            objectTypeMeta = new IO.Swagger.Model.ObjectTypeMeta(builtIn: false, description: "", type: objectId,
                _base: (string) ParentsList.GetValue(), needsServices: new List<string>());
        }

        return objectTypeMeta;
    }

}
