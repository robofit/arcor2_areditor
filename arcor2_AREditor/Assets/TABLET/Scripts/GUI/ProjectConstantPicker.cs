using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base;
using IO.Swagger.Model;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;



//please remove this file if forgotten..



[Obsolete]
public class ProjectConstantPicker : Singleton<ProjectConstantPicker> {
    public GameObject Content, ConstantButtonPrefab;
    public CanvasGroup CanvasGroup;
    private Action3D currentAction;
    public ButtonWithTooltip SaveParametersBtn;
    private List<IParameter> actionParameters = new List<IParameter>();
    private bool parametersChanged;
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;
    private EditProjectParameterDialog EditConstantDialog;
    private bool isMenuOpened;
    private Action<object> constantPickedCallback = null;
    private string filterType = null;

    private void Start() {
        EditConstantDialog = AREditorResources.Instance.EditProjectParameterDialog;
    }

    public void Show(string type = null, Action<object> constantPickedCallback = null) {
        if (isMenuOpened) {
            Hide();
            return;
        }
        AREditorResources.Instance.LeftMenuProject.UpdateVisibility(false);

        this.constantPickedCallback = constantPickedCallback;
        filterType = type;
        
        if (String.IsNullOrEmpty(type)) {
            GenerateConstantButtons();
        } else {
            GenerateConstantButtons(type);
        }

        WebsocketManager.Instance.OnProjectParameterAdded += OnConstantAdded;
        WebsocketManager.Instance.OnProjectParameterRemoved += OnConstantRemoved;

        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        isMenuOpened = true;
    }



    private void OnConstantRemoved(object sender, ProjectParameterEventArgs args) {
        ProjectParameterButton[] btns = Content.GetComponentsInChildren<ProjectParameterButton>();
        if (btns != null) {
            foreach (ProjectParameterButton btn in btns.Where(o => o.Id == args.ProjectParameter.Id)){
                Destroy(btn.gameObject);
                return;
            }
        }
    }

    private void OnConstantAdded(object sender, ProjectParameterEventArgs args) {
        GenerateConstantButton(args.ProjectParameter);
    }

    private void GenerateConstantButtons() {
        foreach (var constant in ProjectManager.Instance.ProjectParameters) {
            GenerateConstantButton(constant);
        }
    }

    private void GenerateConstantButtons(string type) {
        foreach (var constant in ProjectManager.Instance.ProjectParameters.Where(c => c.Type == type)) {
            GenerateConstantButton(constant);
        }
    }

    private ProjectParameterButton GenerateConstantButton(ProjectParameter constant) {
        ProjectParameterButton btn = Instantiate(ConstantButtonPrefab, Content.transform).GetComponent<ProjectParameterButton>();
        btn.Id = constant.Id;
        btn.SetName(constant.Name);
        btn.SetValue(Base.Parameter.GetValue<string>(constant.Value)); //TODO fix other types than string
        btn.Button.onClick.AddListener(() => {
            constantPickedCallback(constant.Value);
            Hide();
        });
        //btn.Button.onClick.AddListener(async () => {
        //    if (!await EditConstantDialog.Init(Show, constant))
        //        return;
        //    Hide();
        //    EditConstantDialog.Open();
        //});
        return btn;
    }

    public async void Hide(bool unlock = true) {
        DestroyConstantButtons();

        EditorHelper.EnableCanvasGroup(CanvasGroup, false);
        if (currentAction != null) {
            currentAction.CloseMenu();
            if (unlock)
                await currentAction.WriteUnlock();
            currentAction = null;
        }

        WebsocketManager.Instance.OnProjectParameterAdded -= OnConstantAdded;
        WebsocketManager.Instance.OnProjectParameterRemoved -= OnConstantRemoved;

        AREditorResources.Instance.LeftMenuProject.UpdateVisibility();
        isMenuOpened = false;
    }

    private void DestroyConstantButtons() {
        RectTransform[] transforms = Content.GetComponentsInChildren<RectTransform>();
        if (transforms != null) {
            foreach (RectTransform o in transforms) {
                if (o.gameObject.tag != "Persistent") {
                    Destroy(o.gameObject);
                }
            }
        }
    }

    public bool IsVisible() {
        return CanvasGroup.alpha > 0;
    }

    public async void ShowNewConstantDialog() {
        Hide();
        if (!await EditConstantDialog.Init(() => Show(filterType, constantPickedCallback)))
            return;
        EditConstantDialog.Open();
    }

    public static ProjectParameterTypes ConvertStringConstantToEnum(string type) {
        return (ProjectParameterTypes) Enum.Parse(typeof(ProjectParameterTypes), type);
    }

    public static object GetValue(string value, ProjectParameterTypes type) {
        object toReturn = null;
        switch (type) {
            case ProjectParameterTypes.integer:
                toReturn = JsonConvert.DeserializeObject<int>(value);
                break;
            case ProjectParameterTypes.@string:
                toReturn = JsonConvert.DeserializeObject<string>(value);
                break;
            case ProjectParameterTypes.boolean:
                toReturn = JsonConvert.DeserializeObject<bool>(value);
                break;
            case ProjectParameterTypes.@double:
                toReturn = JsonConvert.DeserializeObject<double>(value);
                break;
        }
        return toReturn;
    }
}
