using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using System.Threading;
using System;
using Base;

public class ProjectManager : Base.Singleton<ProjectManager>
{
    IO.Swagger.Model.Project Project = null;

    /// <summary>
    /// Creates project from given json
    /// </summary>
    /// <param name="project"></param>
    public async Task<bool> CreateProject(IO.Swagger.Model.Project project, int timeout) {
        if (Project != null)
            return false;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        if (sw.ElapsedMilliseconds > timeout)
            throw new TimeoutException();
        while (!ActionsManager.Instance.ActionsReady) {
            Thread.Sleep(100);
        }

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
        Project = null;
        return true;
    }
}
