using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using Google.Protobuf.WellKnownTypes;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh/team")]
    [Produces("application/json")]
    public class TeamController:Controller
    {
        private readonly ILogger<TeamController> _logger;
        private readonly MeshContext _meshContext;
        private readonly PermissionCheckHelper _permissionCheck;

        public TeamController(ILogger<TeamController> logger, MeshContext meshContext)
        {
            _logger = logger;
            _meshContext = meshContext;
            _permissionCheck = new PermissionCheckHelper(meshContext);
        }
        
        public class Member
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Nickname { get; set; }
            public string Avatar { get; set; }
        }

        public class TeamProject
        {
            public int ProjectId { get; set; }
            public string ProjectName { get; set; }
            public string AdminName { get; set; }
            public string ProjectLogo { get; set; }
        }
        
        public JsonResult CheckUsername(string username)
        {
            if (!CornerCaseCheckHelper.Check(username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
  
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }
        
        
        public class TeamRequest
        {
            public string username { get; set; }
            
            public int teamId { get; set; }
            public string teamName { get; set; }
        }

        public class InviteRequest
        {
            public string username { get; set; }
            public int teamId { get; set; }
            public string inviteName { get; set; }
            
        }
        
        [HttpGet]
        public JsonResult QueryTeam(string username, int teamId)
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

            var user = _meshContext.Users.First(u => u.Email ==username);
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team does not exist.");
            }

            if (_permissionCheck.CheckTeamPermission(username, team) == PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(305, "Permission denied.");
            }
            
            //Find team members
            var teamCooperation = _meshContext.Cooperations
                .Where(c => c.TeamId == team.Id);
            var adminName = _meshContext.Users.First(u => u.Id == team.AdminId).Nickname;
            var members = _meshContext.Users
                .Join(teamCooperation, u => u.Id, c => c.UserId, (u, c) =>
                    new Member()
                    {
                        Id = u.Id,
                        Username = u.Email,
                        Nickname = u.Nickname,
                        Avatar = AvatarSaveHelper.GetObject(u.Avatar)
                    }).ToList();

            //Find projects of the team
            var project = _meshContext.Projects
                .Where(p => p.TeamId ==teamId);
            var teamProjects = _meshContext.Users
                .Join(project, u => u.Id, p => p.AdminId, (u, p) =>
                    new TeamProject()
                    {
                        ProjectId = p.Id,
                        ProjectName = p.Name,
                        AdminName = u.Nickname,
                        ProjectLogo = AvatarSaveHelper.GetObject(p.Icon)
                    }).ToList();

            var userTeamCooperation = teamCooperation.First(c => c.UserId == user.Id);

            try
            {
                userTeamCooperation.AccessCount += 1;
                _meshContext.Cooperations.Update(userTeamCooperation);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }
            
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    team = new
                    {
                        teamId = team.Id,
                        teamName = team.Name,
                        createTime = team.CreatedTime,
                        adminName = adminName,
                        members = members,
                        teamProjects = teamProjects
                    }
                }
            });

        }


        [HttpPost]
        public JsonResult CreateTeam(TeamRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.teamName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(310, "Invalid teamName.");
            }
            
            var user = _meshContext.Users.First(u => u.Email == request.username);
            var createdTeam = new Team()
            {
                Name = request.teamName,
                AdminId = user.Id
            };
            
            //Start transaction to save the new team
            using (var transaction = _meshContext.Database.BeginTransaction())
            {
                try
                {
                    _meshContext.Teams.Add(createdTeam);
                    _meshContext.SaveChanges();
                    _meshContext.Cooperations.Add(new Cooperation()
                    {
                        TeamId = createdTeam.Id,
                        UserId = user.Id
                    });
                    _meshContext.TeamMemoCollections.Add(new TeamMemoCollection()
                    {
                        TeamId = createdTeam.Id
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
            
            //Return team members
            var teamMembers = new List<Member>();
            teamMembers.Add(new Member(){Username = user.Email,Id = user.Id});
            return Json(new 
                {
                    err_code = 0,
                    data = new
                    {
                        isSuccess = true,
                        team = new 
                        {
                            teamId = createdTeam.Id,
                            createTime = createdTeam.CreatedTime,
                            teamName = createdTeam.Name,
                            adminName = user.Nickname,
                            members = teamMembers
                        }
                    }
                }
            );
        }


        [HttpPost]
        [Route("invite")]
        public JsonResult InviteNewTeamMember(InviteRequest request)
        {
            var checkUsernameResult = CheckUsername(request.username);
            if (checkUsernameResult != null)
            {
                return checkUsernameResult;
            }

            if (!CornerCaseCheckHelper.Check(request.inviteName,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(108, "Invalid inviteName");
            }

            if (!CornerCaseCheckHelper.Check(request.teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId");
            }
            
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == request.teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team not exist.");
            }

            if (_permissionCheck.CheckTeamPermission(request.username, team) != PermissionCheckHelper.TeamAdmin)
            {
                return JsonReturnHelper.ErrorReturn(305, "Permission denied.");
            }

            var inviteUser = _meshContext.Users.FirstOrDefault(u => u.Email == request.inviteName);
            if (inviteUser==null)
            {
                return JsonReturnHelper.ErrorReturn(108, "Username or inviteName not exists.");
            }

            if (_permissionCheck.CheckTeamPermission(request.inviteName, team) != PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(306, "User already in team.");
            }
            
            var cooperation = new Cooperation()
            {
                TeamId = request.teamId,
                UserId = inviteUser.Id
            };
            try
            {
                _meshContext.Cooperations.Add(cooperation);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return JsonReturnHelper.SuccessReturn();
        }

        [HttpDelete]
        public JsonResult DeleteTeam(string username, int teamId)
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

            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team not exist.");
            }

            if (_permissionCheck.CheckTeamPermission(username, team) != PermissionCheckHelper.TeamAdmin)
            {
                return JsonReturnHelper.ErrorReturn(305, "Permission denied.");
            }

            try
            {
                _meshContext.Teams.Remove(team);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(2, "Unexpected error.");
            }

            return JsonReturnHelper.SuccessReturn();

        }

        [HttpDelete]
        [Route("quit")]
        public JsonResult QuitTeam(string username, int teamId)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            
            if (!CornerCaseCheckHelper.Check(teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId.");
            }

            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team not exist.");
            }

            if (_permissionCheck.CheckTeamPermission(username, team) != PermissionCheckHelper.TeamMember)
            {
                return JsonReturnHelper.ErrorReturn(305, "Permission denied.");
            }

            var cooperation = _meshContext.Cooperations.First(c => c.UserId == user.Id && c.TeamId == teamId);
            var tasks = _meshContext.Tasks
                .Where(t => t.LeaderId == user.Id);
            var subTasks = _meshContext.Assigns
                .Where(s => s.UserId == user.Id);

            try
            {
                _meshContext.Cooperations.Remove(cooperation);
                foreach (var task in tasks)
                {
                    task.LeaderId = team.AdminId;
                }
                _meshContext.Tasks.UpdateRange(tasks);
                foreach (var subTask in subTasks)
                {
                    subTask.UserId = team.AdminId;
                }
                _meshContext.Assigns.UpdateRange(subTasks);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
               _logger.LogError(e.ToString());
               return JsonReturnHelper.ErrorReturn(2, "Unexpected Error.");
            }
            
            return JsonReturnHelper.SuccessReturn();

        }

        [HttpDelete]
        [Route("remove")]
        public JsonResult RemoveTeamMember(string username, int teamId, string removeName)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            
            if (!CornerCaseCheckHelper.Check(teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId.");
            }

            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team not exist.");
            }

            if (_permissionCheck.CheckTeamPermission(username, team) != PermissionCheckHelper.TeamAdmin)
            {
                return JsonReturnHelper.ErrorReturn(305, "Permission denied.");
            }

            if (!CornerCaseCheckHelper.Check(removeName, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid removeName");
            }

            var removeUser = _meshContext.Users.FirstOrDefault(u => u.Email == removeName);
            if (removeUser == null)
            {
                return JsonReturnHelper.ErrorReturn(104, "RemoveName does not exist.");
            }

            if (_permissionCheck.CheckTeamPermission(removeName, team) != PermissionCheckHelper.TeamMember)
            {
                return JsonReturnHelper.ErrorReturn(305, "RemoveUser can not be removed.");
            }
            
            
            var cooperation = _meshContext.Cooperations.First(c => c.UserId == removeUser.Id && c.TeamId == teamId);
            var tasks = _meshContext.Tasks
                .Where(t => t.LeaderId == removeUser.Id);
            var subTasks = _meshContext.Assigns
                .Where(s => s.UserId == removeUser.Id);

            try
            {
                _meshContext.Cooperations.Remove(cooperation);
                foreach (var task in tasks)
                {
                    task.LeaderId = team.AdminId;
                }
                _meshContext.Tasks.UpdateRange(tasks);
                foreach (var subTask in subTasks)
                {
                    subTask.UserId = team.AdminId;
                }
                _meshContext.Assigns.UpdateRange(subTasks);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(2, "Unexpected Error.");
            }
            
            return JsonReturnHelper.SuccessReturn();
        }

        [HttpPatch]
        public JsonResult UpdateTeam(TeamRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(311, "Invalid teamId.");
            }
            
            if (!CornerCaseCheckHelper.Check(request.teamName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(310, "Invalid teamName.");
            }

            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == request.teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team does not exist.");
            }
    
            try
            {
                team.Name = request.teamName;
                _meshContext.Teams.Update(team);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(2, "Unexpected error.");
            }

            //Find team members
            var teamCooperation = _meshContext.Cooperations
                .Where(c => c.TeamId == team.Id);
            var adminName = _meshContext.Users.First(u => u.Id == team.AdminId).Nickname;
            var members = _meshContext.Users
                .Join(teamCooperation, u => u.Id, c => c.UserId, (u, c) =>
                    new Member()
                    {
                        Id = u.Id,
                        Username = u.Email,
                        Nickname = u.Nickname,
                        Avatar = AvatarSaveHelper.GetObject(u.Avatar)
                    }).ToList();

            //Find projects of the team
            var project = _meshContext.Projects
                .Where(p => p.TeamId == team.Id);
            var teamProjects = _meshContext.Users
                .Join(project, u => u.Id, p => p.AdminId, (u, p) =>
                    new TeamProject()
                    {
                        ProjectId = p.Id,
                        ProjectName = p.Name,
                        AdminName = u.Nickname,
                        ProjectLogo = AvatarSaveHelper.GetObject(p.Icon)
                    }).ToList();

            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    team = new
                    {
                        teamId = team.Id,
                        teamName = team.Name,
                        createTime = team.CreatedTime,
                        adminName = adminName,
                        members = members,
                        teamProjects = teamProjects
                    }
                }
            });
            
        }
        
    }
}