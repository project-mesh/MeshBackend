using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy.Contributors;
using Google.Protobuf.WellKnownTypes;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh/project")]
    [Produces("application/json")]
    public class ProjectController:Controller
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly MeshContext _meshContext;
        private readonly PermissionCheckHelper _permissionCheck;
        public ProjectController(ILogger<ProjectController> logger, MeshContext meshContext)
        {
            _logger = logger;
            _meshContext = meshContext;
            _permissionCheck = new PermissionCheckHelper(meshContext);
        }
        
        public class MemInfo
        {
            public int UserId { get; set; }
            public string Username { get; set; }
        }
        

        
        public JsonResult ProjectResult(Project project,string name)
        {
            var develops = _meshContext.Develops
                .Where(d => d.ProjectId == project.Id)
                .Join(_meshContext.Users, d => d.UserId, u => u.Id, (d, u) => new MemInfo()
                {
                    UserId = u.Id,
                    Username = u.Nickname
                })
                .ToList();
            return Json(new
            {
                err_code = 0,
                isSuccess = true,
                msg = "",
                project = new
                {
                    projectId = project.Id,
                    projectName = project.Name,
                    adminName = name,
                    isPublic = project.Publicity,
                    members = develops
                }
            });
        }
        
        [HttpPost]
        public JsonResult CreateProject(string username, int teamId, string projectName, string adminName)
        {
            var checkResult = _permissionCheck.CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            var user = _meshContext.Users.First(u => u.Nickname == username);
            
            //Check if admin exists
            var admin = _meshContext.Users.FirstOrDefault(a => a.Nickname == adminName);
            if (admin == null)
            {
                return JsonReturnHelper.ErrorReturn(704, "Invalid adminName.");
            }

            //Check if team exists
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }
            
            //Check if admin is in the team
            var teamCheckResult = _permissionCheck.CheckTeamPermission(adminName, team);
            if (teamCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(702, "Invalid admin.");
            }
            
            //Check if user is the admin of the team
            teamCheckResult = _permissionCheck.CheckTeamPermission(username, team);
            if (teamCheckResult != PermissionCheckHelper.TeamAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");

            }

            var newProject = new Project()
            {
                Name = projectName,
                AdminId = admin.Id,
                TeamId = team.Id,
                Publicity = true,
            };

            var members = new List<MemInfo> {new MemInfo() {UserId = admin.Id, Username = admin.Nickname}};
            
            //Start a transaction to save the project
            using (var transaction = _meshContext.Database.BeginTransaction())
            {
                try
                {
                    _meshContext.Projects.Add(newProject);
                    _meshContext.SaveChanges();
                    _meshContext.Develops.Add(new Develop()
                    {
                        ProjectId = newProject.Id,
                        UserId = admin.Id
                    });
                    _meshContext.BulletinBoards.Add(new BulletinBoard()
                    {
                        ProjectId = newProject.Id
                    });
                    _meshContext.TaskBoards.Add(new TaskBoard()
                    {
                        ProjectId = newProject.Id
                    });
                    _meshContext.ProjectMemoCollections.Add(new ProjectMemoCollection()
                    {
                        ProjectId = newProject.Id
                    });
                    _meshContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
                }
            }

            return ProjectResult(newProject, admin.Nickname);
        }

        [HttpDelete]
        public JsonResult DeleteProject(string username, int teamId, int projectId)
        {
            var checkResult = _permissionCheck.CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            //Check if team exists
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }
            
            //Check if project exists
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
            }
            
            //Check if user in the team
            var teamCheckResult = _permissionCheck.CheckTeamPermission(username, team);
            if (teamCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(702, "Invalid username.");
            }
            
            //Check if user is the admin of the project
            var projectCheckResult = _permissionCheck.CheckProjectPermission(username, project);
            if (projectCheckResult != PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            //Remove the project
            try
            {
                _meshContext.Projects.Remove(project);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return JsonReturnHelper.SuccessReturn();
        }

        [HttpPost]
        [Route("invite")]
        public JsonResult InviteNewProjectMember(string username, int teamId, int projectId, string inviteName)
        {
            var checkResult = _permissionCheck.CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            //Check if inviteUser exists
            var inviteUser = _meshContext.Users.FirstOrDefault(a => a.Nickname == inviteName);
            if (inviteUser == null)
            {
                return JsonReturnHelper.ErrorReturn(704, "Invalid adminName.");
            }
            
            //Check if team exists
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }
            
            //Check if user in the team
            var teamUserCheckResult = _permissionCheck.CheckTeamPermission(username, team);
            if (teamUserCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(702, "Invalid username.");
            }
            
            //Check if inviteUser in the team
            var teamCheckResult = _permissionCheck.CheckTeamPermission(inviteName, team);
            if (teamCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(702, "Invalid inviteName.");
            }
            
            //Check if project exists
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
            }
            
            //Check if user is the admin of the project
            var projectCheckResult = _permissionCheck.CheckProjectPermission(username, project);
            if (projectCheckResult != PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            try
            {
                _meshContext.Develops.Add(new Develop()
                {
                    UserId = inviteUser.Id,
                    ProjectId = project.Id
                });
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return JsonReturnHelper.SuccessReturn();
        }

        [HttpGet]
        public JsonResult QueryProject(string username, int projectId)
        {
            var checkResult = _permissionCheck.CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Check if project exists
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
            }
            
            //Check if user is in the project
            var projectCheckResult = _permissionCheck.CheckProjectPermission(username, project);
            if (projectCheckResult == PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            var user = _meshContext.Users.First(u => u.Id == project.AdminId);
            return ProjectResult(project, user.Nickname);
        }

        [HttpPatch]
        public JsonResult UpdateProject(string username, int teamId, int projectId, bool isPublic,
            string projectName)
        {
            var checkResult = _permissionCheck.CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            
            //Check if team exists
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }
            
            //Check if project exists
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
            }
            
            //Check if user in the team
            var teamCheckResult = _permissionCheck.CheckTeamPermission(username, team);
            if (teamCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(702, "Invalid username.");
            }
            
            //Check if user is the admin of the project
            var projectCheckResult = _permissionCheck.CheckProjectPermission(username, project);
            if (projectCheckResult != PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            
            try
            {
                project.Publicity = isPublic;
                project.Name = projectName;
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }
            
            var user = _meshContext.Users.First(u => u.Id == project.AdminId);
            return ProjectResult(project, user.Nickname);
        }
        
    }
}