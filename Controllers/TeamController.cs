using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        
        
        [HttpGet]
        public JsonResult QueryTeam(string username, int teamId)
        {
            var checkResult = _permissionCheck.CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team != null)
            {
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
            else
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }
        }

        
        [HttpPost]
        public JsonResult CreateTeam(string username, string teamName)
        {
            var checkResult = _permissionCheck.CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            var team = _meshContext.Teams.FirstOrDefault(t => t.Name == teamName);
            if (team != null)
            {
                return JsonReturnHelper.ErrorReturn(301, "TeamName already exists.");
            }

            var user = _meshContext.Users.First(u => u.Nickname == username);
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
            teamMembers.Add(new Member(){Username = user.Nickname,Id = user.Id});
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
            var checkUsernameResult = _permissionCheck.CheckUsername(username);
            if (checkUsernameResult != null)
            {
                return checkUsernameResult;
            }

            if (inviteName == null || inviteName.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(108, "Invalid inviteName");
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

            var inviteUser = _meshContext.Users.FirstOrDefault(u => u.Nickname == inviteName);
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