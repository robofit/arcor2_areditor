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

public class ProjectConstantPicker : Singleton<ProjectConstantPicker> {
    public GameObject Content, ConstantButtonPrefab;
    public CanvasGroup CanvasGroup;
    private Action3D currentAction;
    public ButtonWithTooltip SaveParametersBtn;
    private List<IParameter> actionParameters = new List<IParameter>();
    private bool parametersChanged;
    public VerticalLayoutGroup DynamicContentLayout;
    public GameObject CanvasRoot;
    private EditConstantDialog EditConstantDialog;
    private bool isMenuOpened;
    private Action<object> constantPickedCallback = null;
    private string filterType = null;

    private void Start() {
        EditConstantDialog = AREditorResources.Instance.EditConstantDialog;
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

        WebsocketManager.Instance.OnProjectConstantAdded += OnConstantAdded;
        WebsocketManager.Instance.OnProjectConstantRemoved += OnConstantRemoved;

        EditorHelper.EnableCanvasGroup(CanvasGroup, true);
        isMenuOpened = true;
    }



    private void OnConstantRemoved(object sender, ProjectConstantEventArgs args) {
        ProjectConstantButton[] btns = Content.GetComponentsInChildren<ProjectConstantButton>();
        if (btns != null) {
            foreach (ProjectConstantButton btn in btns.Where(o => o.Id == args.ProjectConstant.Id)){
                Destroy(btn.gameObject);
                return;
            }
        }
    }

    private void OnConstantAdded(object sender, ProjectConstantEventArgs args) {
        GenerateConstantButton(args.ProjectConstant);
    }

    private void GenerateConstantButtons() {
        foreach (var constant in ProjectManager.Instance.ProjectConstants) {
            GenerateConstantButton(constant);
        }
    }

    private void GenerateConstantButtons(string type) {
        foreach (var constant in ProjectManager.Instance.ProjectConstants.Where(c => c.Type == type)) {
            GenerateConstantButton(constant);
        }
    }

    private ProjectConstantButton GenerateConstantButton(ProjectParameter constant) {
        ProjectConstantButton btn = Instantiate(ConstantButtonPrefab, Content.transform).GetComponent<ProjectConstantButton>();
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

        WebsocketManager.Instance.OnProjectConstantAdded -= OnConstantAdded;
        WebsocketManager.Instance.OnProjectConstantRemoved -= OnConstantRemoved;

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

    public static ProjectConstantTypes ConvertStringConstantToEnum(string type) {
        return (ProjectConstantTypes) Enum.Parse(typeof(ProjectConstantTypes), type);
    }

    public static object GetValue(string value, ProjectConstantTypes type) {
        object toReturn = null;
        switch (type) {
            case ProjectConstantTypes.integer:
                toReturn = JsonConvert.DeserializeObject<int>(value);
                break;
            case ProjectConstantTypes.@string:
                toReturn = JsonConvert.DeserializeObject<string>(value);
                break;
            case ProjectConstantTypes.boolean:
                toReturn = JsonConvert.DeserializeObject<bool>(value);
                break;
            case ProjectConstantTypes.@double:
                toReturn = JsonConvert.DeserializeObject<double>(value);
                break;
        }
        return toReturn;
    }
}
