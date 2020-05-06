using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectManager : Base.Singleton<ProjectManager>
{
    IO.Swagger.Model.Project Project = null;

    /// <summary>
    /// Creates project from given json
    /// </summary>
    /// <param name="project"></param>
    public bool CreateProject(IO.Swagger.Model.Project project) {
        if (Project != null)
            return false;
        Project = project;
        return true;
    }

    /// <summary>
    /// Updates project from given json
    /// </summary>
    /// <param name="project"></param>
    public bool UpdateProject(IO.Swagger.Model.Project project) {
        return false;
    }

    public bool DestroyProject() {
        Debug.Assert(Project != null);
        Project = null;
        return true;
    }
}
