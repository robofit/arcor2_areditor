using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using System.Globalization;

public class NewObjectTypeMenu : Base.Singleton<NewObjectTypeMenu> {
    [SerializeField]
    private GameObject BoxMenu, CylinderMenu, SphereMenu, MeshMenu, NameInput, ParentsList, ModelsList;
    [SerializeField]
    private GameObject BoxX, BoxY, BoxZ, SphereRadius, CylinderHeight, CylinderRadius, MeshId;
    private Dictionary<string, GameObject> ModelMenus;
    
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
    void Start() {
        //TODO: find out why start is called twice
        Base.ActionsManager.Instance.OnActionObjectsUpdated += UpdateObjectsList;
    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateMenu() {

    }

    public void UpdateModelsMenu(int value) {
        string modelType = ModelsList.GetComponent<Dropdown>().options[value].text;
        if (ModelMenus.TryGetValue(modelType, out GameObject menu)) {
            ShowModelMenu(menu);
        }
    }

    private void ShowModelMenu(GameObject menu) {
        BoxMenu.SetActive(false);
        CylinderMenu.SetActive(false);
        SphereMenu.SetActive(false);
        MeshMenu.SetActive(false);
        menu?.SetActive(true);
    }

    public void UpdateObjectsList(object sender, EventArgs eventArgs) {
        string originalValue = "";
        if (ParentsList.GetComponent<Dropdown>().options.Count > 0) {
            originalValue = ParentsList.GetComponent<Dropdown>().options[ParentsList.GetComponent<Dropdown>().value].text;
            ParentsList.GetComponent<Dropdown>().options.Clear();
        }         
        foreach (Base.ActionObjectMetadata actionObjectMetadata in Base.ActionsManager.Instance.ActionObjectMetadata.Values) {
            ParentsList.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(actionObjectMetadata.Type));
        }
        if (originalValue != "") {
            // TODO: try if indexof works!
            int index = ParentsList.GetComponent<Dropdown>().options.IndexOf(new Dropdown.OptionData(originalValue));
            if (index >= 0) {
                ParentsList.GetComponent<Dropdown>().value = index;
            }
        }
        try {
            ParentsList.GetComponent<Dropdown>().captionText.text = ParentsList.GetComponent<Dropdown>().options[ParentsList.GetComponent<Dropdown>().value].text;
        } catch (ArgumentOutOfRangeException) {

        }
        
    }

    public void CreateNewObjectType() {
        string objectId = NameInput.GetComponent<InputField>().text;

        IO.Swagger.Model.ObjectModel objectModel = new IO.Swagger.Model.ObjectModel();
        string modelTypeString = ModelsList.GetComponent<Dropdown>().options[ModelsList.GetComponent<Dropdown>().value].text;
        if (ModelMenus.TryGetValue(modelTypeString, out GameObject type) && type != null) {
            IO.Swagger.Model.MetaModel3d.TypeEnum modelType = new IO.Swagger.Model.MetaModel3d.TypeEnum();
            switch (modelTypeString) {
                case "Box":
                    modelType = IO.Swagger.Model.MetaModel3d.TypeEnum.Box;
                    decimal sizeX = decimal.Parse(BoxX.GetComponent<InputField>().text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    decimal sizeY = decimal.Parse(BoxY.GetComponent<InputField>().text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    decimal sizeZ = decimal.Parse(BoxZ.GetComponent<InputField>().text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    IO.Swagger.Model.Box box = new IO.Swagger.Model.Box(objectId, sizeX, sizeY, sizeZ);
                    objectModel.Box = box;
                    break;
                case "Sphere":
                    modelType = IO.Swagger.Model.MetaModel3d.TypeEnum.Sphere;
                    decimal radius = decimal.Parse(SphereRadius.GetComponent<InputField>().text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    IO.Swagger.Model.Sphere sphere = new IO.Swagger.Model.Sphere(objectId, radius);
                    objectModel.Sphere = sphere;
                    break;
                case "Cylinder":
                    modelType = IO.Swagger.Model.MetaModel3d.TypeEnum.Cylinder;
                    decimal cylinderRadius = decimal.Parse(CylinderRadius.GetComponent<InputField>().text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    decimal cylinderHeight = decimal.Parse(CylinderHeight.GetComponent<InputField>().text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                    IO.Swagger.Model.Cylinder cylinder = new IO.Swagger.Model.Cylinder(objectId, cylinderHeight, cylinderRadius);
                    objectModel.Cylinder = cylinder;
                    break;
                case "Mesh":
                    modelType = IO.Swagger.Model.MetaModel3d.TypeEnum.Mesh;
                    string meshId = MeshId.GetComponent<InputField>().text;
                    IO.Swagger.Model.Mesh mesh = new IO.Swagger.Model.Mesh() {
                        Id = meshId
                    };
                    objectModel.Mesh = mesh;
                    break;
                default:
                    Debug.LogError("Model not defined!");
                    return;
            }
            
        }
        IO.Swagger.Model.ObjectTypeMeta objectTypeMeta = new IO.Swagger.Model.ObjectTypeMeta(builtIn: false, description: "", type: objectId, objectModel: objectModel,
                _base: ParentsList.GetComponent<Dropdown>().options[ParentsList.GetComponent<Dropdown>().value].text);
        Base.GameManager.Instance.CreateNewObjectType(objectTypeMeta);
    }

}
