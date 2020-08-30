using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using Google.Protobuf.WellKnownTypes;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        }

        public class TeamProject
        {
            public int ProjectId { get; set; }
            public string ProjectName { get; set; }
            public string AdminName { get; set; }
        }
        
        public JsonResult CheckUsername(string username)
        {
            if (!CornerCaseCheckHelper.Check(username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
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
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId");
            }

            var user = _meshContext.Users.First(u => u.Email == username);
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
                        Username = u.Nickname
                    }).ToList();

            //Find projects of the team
            var project = _meshContext.Projects
                .Where(p => p.TeamId == teamId);
            var teamProjects = _meshContext.Users
                .Join(project, u => u.Id, p => p.AdminId, (u, p) =>
                    new TeamProject()
                    {
                        ProjectId = p.Id,
                        ProjectName = p.Name,
                        AdminName = u.Nickname
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
        public JsonResult CreateTeam(string username, string teamName)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(teamName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(310, "Invalid teamName.");
            }

            var team = _meshContext.Teams.FirstOrDefault(t => t.Name == teamName);
            if (team != null)
            {
                return JsonReturnHelper.ErrorReturn(301, "TeamName already exists.");
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            var createdTeam = new Team()
            {
                Name = teamName,
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
        public JsonResult InviteNewTeamMember(string username, int teamId, string inviteName)
        {
            var checkUsernameResult = CheckUsername(username);
            if (checkUsernameResult != null)
            {
                return checkUsernameResult;
            }

            if (!CornerCaseCheckHelper.Check(inviteName,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(108, "Invalid inviteName");
            }

            if (!CornerCaseCheckHelper.Check(teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId");
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

            var inviteUser = _meshContext.Users.FirstOrDefault(u => u.Email == inviteName);
            if (inviteUser==null)
            {
                return JsonReturnHelper.ErrorReturn(108, "Username or inviteName not exists.");
            }

            if (_permissionCheck.CheckTeamPermission(inviteName, team) != PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(306, "User already in team.");
            }
            
            var cooperation = new Cooperation()
            {
                TeamId = teamId,
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
        
    }
}