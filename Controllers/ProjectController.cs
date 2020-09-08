using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
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
        public JsonResult CheckUsername(string username)
        {
            if (!CornerCaseCheckHelper.Check(username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }

        
        public class MemInfo
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Avatar { get; set; }
        }

        public class ProjectRequest
        {
            public string username { get; set; }
            public int teamId { get; set; }
            public int projectId { get; set; }
            public string projectName { get; set; }
            public string adminName { get; set; }
            public bool isPublic { get; set; }
            public string inviteName { get; set; }
        }

        
        public JsonResult ProjectResult(Project project,string name)
        {
            var develops = _meshContext.Develops
                .Where(d => d.ProjectId == project.Id)
                .Join(_meshContext.Users, d => d.UserId, u => u.Id, (d, u) => new MemInfo()
                {
                    UserId = u.Id,
                    Username = u.Nickname,
                    Avatar = u.Avatar
                })
                .ToList();
            return Json(new
            {
                err_code = 0,
                data = new
                {
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
                }
            });
        }
        
        [HttpPost]
        public JsonResult CreateProject(ProjectRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId.");
            }

            if (!CornerCaseCheckHelper.Check(request.projectName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(710, "Invalid projectName.");
            }

            if (!CornerCaseCheckHelper.Check(request.adminName, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(711, "Invalid adminName");
            }
            
            
            var user = _meshContext.Users.First(u => u.Email == request.username);
            
            //Check if admin exists
            var admin = _meshContext.Users.FirstOrDefault(a => a.Email == request.adminName);
            if (admin == null)
            {
                return JsonReturnHelper.ErrorReturn(704, "Admin does not exist.");
            }

            //Check if team exists
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == request.teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team does not exist.");
            }
            
            //Check if admin is in the team
            var teamCheckResult = _permissionCheck.CheckTeamPermission(request.adminName, team);
            if (teamCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(702, "Invalid admin.");
            }
            
            //Check if user is the admin of the team
            teamCheckResult = _permissionCheck.CheckTeamPermission(request.username, team);
            if (teamCheckResult != PermissionCheckHelper.TeamAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");

            }

            var newProject = new Project()
            {
                Name = request.projectName,
                AdminId = admin.Id,
                TeamId = team.Id,
                Publicity = request.isPublic
            };

            var members = new List<MemInfo> {new MemInfo() {UserId = admin.Id, Username = admin.Email}};
            
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
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId.");
            }
            
            if (!CornerCaseCheckHelper.Check(projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
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
        public JsonResult InviteNewProjectMember(ProjectRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(request.teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId.");
            }
            
            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }

            if (!CornerCaseCheckHelper.Check(request.inviteName, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid inviteName");
            }
            
            //Check if inviteUser exists
            var inviteUser = _meshContext.Users.FirstOrDefault(a => a.Email == request.inviteName);
            if (inviteUser == null)
            {
                return JsonReturnHelper.ErrorReturn(704, "Admin does not exist.");
            }
            
            //Check if team exists
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == request.teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team does not exist.");
            }
            
            //Check if user in the team
            var teamUserCheckResult = _permissionCheck.CheckTeamPermission(request.username, team);
            if (teamUserCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied");
            }
            
            //Check if inviteUser in the team
            var teamCheckResult = _permissionCheck.CheckTeamPermission(request.inviteName, team);
            if (teamCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "InviteUser is not in the team");
            }
            
            //Check if project exists
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }
            
            //Check if user is the admin of the project
            var projectCheckResult = _permissionCheck.CheckProjectPermission(request.username, project);
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
            var checkResult = CheckUsername(username);
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
        public JsonResult UpdateProject(ProjectRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(request.teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId.");
            }

            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }
            
            if (!CornerCaseCheckHelper.Check(request.projectName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(710, "Invalid projectName.");
            }
            
            //Check if team exists
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == request.teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }
            
            //Check if project exists
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
            }
            
            //Check if user in the team
            var teamCheckResult = _permissionCheck.CheckTeamPermission(request.username, team);
            if (teamCheckResult ==PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(702, "Invalid username.");
            }
            
            //Check if user is the admin of the project
            var projectCheckResult = _permissionCheck.CheckProjectPermission(request.username, project);
            if (projectCheckResult != PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            
            try
            {
                project.Publicity = request.isPublic;
                project.Name = request.projectName;
                _meshContext.Projects.Update(project);
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