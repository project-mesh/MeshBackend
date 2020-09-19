using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh/knowledgebase")]
    [Produces("application/json")]
    public class MemoController:Controller
    {
        private readonly ILogger<MemoController> _logger;
        private readonly MeshContext _meshContext;
        private readonly PermissionCheckHelper _permissionCheck;
        
        public MemoController(ILogger<MemoController> logger, MeshContext meshContext)
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
        public class MemoInfo
        {
            public int KnowledgeId { get; set; }
            public string KnowledgeName { get; set; }
            public string Hyperlink { get; set; }
            public long CreateTime { get; set; }
            public String UploaderName { get; set; }
        }

        public JsonResult MemoResult(MemoInfo memo)
        {
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    knowledge = memo
                }
            });
        }

        public JsonResult MemoListResult(List<MemoInfo> memoList)
        {
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    knowledgeBase = memoList
                }
            });
        }

        public class MemoRequest
        {
            public string username { get; set; }
            public int projectId { get; set; }
            public int teamId { get; set; }
            public int knowledgeId { get; set; }
            public string knowledgeName { get; set; }
            public string hyperlink { get; set; }
        }
        
        [Route("project")]
        [HttpPost]
        public JsonResult CreateProjectKB(MemoRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId");
            }

            if (!CornerCaseCheckHelper.Check(request.knowledgeName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (!CornerCaseCheckHelper.Check(request.hyperlink, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }
            

            var user = _meshContext.Users.First(u => u.Email == request.username);
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }

            var memoCollection = _meshContext.ProjectMemoCollections.First(p => p.ProjectId == project.Id);
            if (_permissionCheck.CheckProjectPermission(request.username, project) == PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var newMemo = new ProjectMemo()
            {
                Title = request.knowledgeName,
                CollectionId = memoCollection.Id,
                Text = request.hyperlink,
                UserId = user.Id
            };
            
            try
            {
                _meshContext.ProjectMemos.Add(newMemo);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return MemoResult(new MemoInfo()
            {
                KnowledgeName = request.knowledgeName,
                Hyperlink = request.hyperlink,
                CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(newMemo.CreatedTime),
                KnowledgeId = newMemo.Id,
                UploaderName = request.username
            });
        }
        
        [Route("team")]
        [HttpPost]
        public JsonResult CreateTeamKB(MemoRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId");
            }

            if (!CornerCaseCheckHelper.Check(request.knowledgeName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (!CornerCaseCheckHelper.Check(request.hyperlink, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }


            var user = _meshContext.Users.First(u => u.Email == request.username);
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == request.teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team does not exist.");
            }

            var memoCollection = _meshContext.TeamMemoCollections.First(p => p.TeamId == team.Id);
            if (_permissionCheck.CheckTeamPermission(request.username, team) == PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var newMemo = new TeamMemo()
            {
                Title = request.knowledgeName,
                CollectionId = memoCollection.Id,
                Text = request.hyperlink,
                UserId = user.Id
            };
            
            try
            {
                _meshContext.TeamMemos.Add(newMemo);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return MemoResult(new MemoInfo()
            {
                KnowledgeName = request.knowledgeName,
                Hyperlink = request.hyperlink,
                CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(newMemo.CreatedTime),
                KnowledgeId = newMemo.Id,
                UploaderName = request.username
            });
        }

        [HttpDelete]
        [Route("project")]
        public JsonResult DeleteProjectKB(string username, int projectId, int knowledgeId)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }

            if (!CornerCaseCheckHelper.Check(knowledgeId, 50, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(807, "Invalid knowledgeId.");
            }

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }

            var memoCollection = _meshContext.ProjectMemoCollections.First(p => p.ProjectId == projectId);
            var knowledge =
                _meshContext.ProjectMemos.FirstOrDefault(
                    m => m.Id == knowledgeId && m.CollectionId == memoCollection.Id);
            if (knowledge == null)
            {
                return JsonReturnHelper.ErrorReturn(805, "Invalid knowledgeId.");
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            if (_permissionCheck.CheckProjectPermission(username, project) != PermissionCheckHelper.ProjectAdmin ||
                knowledge.UserId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            try
            {
                _meshContext.ProjectMemos.Remove(knowledge);
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
        [Route("team")]
        public JsonResult DeleteTeamKB(string username, int teamId, int knowledgeId)
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

            if (!CornerCaseCheckHelper.Check(knowledgeId, 50, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(807, "Invalid knowledgeId.");
            }

            var team = _meshContext.Teams.FirstOrDefault(p => p.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team does not exist.");
            }

            var memoCollection = _meshContext.TeamMemoCollections.First(p => p.TeamId == teamId);
            var knowledge =
                _meshContext.TeamMemos.FirstOrDefault(
                    m => m.Id == knowledgeId && m.CollectionId == memoCollection.Id);
            if (knowledge == null)
            {
                return JsonReturnHelper.ErrorReturn(805, "Invalid knowledgeId.");
            }
            
            var user = _meshContext.Users.First(u => u.Email == username);
            if (_permissionCheck.CheckTeamPermission(username, team) != PermissionCheckHelper.TeamAdmin ||
                knowledge.UserId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            try
            {
                _meshContext.TeamMemos.Remove(knowledge);
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
        [Route("project")]
        public JsonResult QueryProjectKB(string username, int projectId)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }

            
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }
            
            if (_permissionCheck.CheckProjectPermission(username, project) == PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var collectionId = _meshContext.ProjectMemoCollections.First(c => c.ProjectId == project.Id).Id;
            var memoLists = _meshContext.ProjectMemos
                .Where(m => m.CollectionId == collectionId)
                .Join(_meshContext.Users, m => m.UserId, u => u.Id, (m, u) => new MemoInfo()
                {
                    KnowledgeId = m.Id,
                    KnowledgeName = m.Title,
                    Hyperlink = m.Text,
                    CreateTime  = TimeStampConvertHelper.ConvertToTimeStamp(m.CreatedTime),
                    UploaderName = u.Nickname
                }).ToList();

            return MemoListResult(memoLists);
        }
        
        [HttpGet]
        [Route("team")]
        public JsonResult QueryTeamKB(string username, int teamId)
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

            
            var team = _meshContext.Teams.FirstOrDefault(p => p.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Team does not exist.");
            }
            
            if (_permissionCheck.CheckTeamPermission(username, team) == PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var collectionId = _meshContext.TeamMemoCollections.First(c => c.TeamId == team.Id).Id;
            var memoLists = _meshContext.TeamMemos
                .Where(m => m.CollectionId == collectionId)
                .Join(_meshContext.Users, m => m.UserId, u => u.Id, (m, u) => new MemoInfo()
                {
                    KnowledgeId = m.Id,
                    KnowledgeName = m.Title,
                    Hyperlink = m.Text,
                    CreateTime  = TimeStampConvertHelper.ConvertToTimeStamp(m.CreatedTime),
                    UploaderName = u.Nickname
                }).ToList();

            return MemoListResult(memoLists);
        }


        [HttpPatch]
        [Route("project")]
        public JsonResult UpdateProjectKB(MemoRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId");
            }

            if (!CornerCaseCheckHelper.Check(request.knowledgeName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (!CornerCaseCheckHelper.Check(request.hyperlink, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }
            
            
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }

            var memoCollection = _meshContext.ProjectMemoCollections.First(p => p.ProjectId == request.projectId);
            var knowledge =
                _meshContext.ProjectMemos.FirstOrDefault(
                    m => m.Id == request.knowledgeId && m.CollectionId == memoCollection.Id);
            if (knowledge == null)
            {
                return JsonReturnHelper.ErrorReturn(805, "Invalid knowledgeId.");
            }

            var user = _meshContext.Users.First(u => u.Email == request.username);
            if (_permissionCheck.CheckProjectPermission(request.username, project) != PermissionCheckHelper.ProjectAdmin ||
                knowledge.UserId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var uploader = _meshContext.Users.First(u => u.Id == knowledge.UserId).Nickname;
            
            try
            {
                knowledge.Title = request.knowledgeName;
                knowledge.Text = request.hyperlink;
                _meshContext.ProjectMemos.Update(knowledge);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return MemoResult(new MemoInfo()
            {
                CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(knowledge.CreatedTime),
                Hyperlink = knowledge.Text,
                KnowledgeId = knowledge.Id,
                KnowledgeName = knowledge.Title,
                UploaderName = uploader
            });
        }
        
        
        [HttpPatch]
        [Route("team")]
        public JsonResult UpdateTeamKB(MemoRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.teamId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId");
            }

            if (!CornerCaseCheckHelper.Check(request.knowledgeName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (!CornerCaseCheckHelper.Check(request.hyperlink, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }
            var team = _meshContext.Teams.FirstOrDefault(p => p.Id == request.teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid teamId.");
            }

            var memoCollection = _meshContext.TeamMemoCollections.First(p => p.TeamId == request.teamId);
            var knowledge =
                _meshContext.TeamMemos.FirstOrDefault(
                    m => m.Id == request.knowledgeId && m.CollectionId == memoCollection.Id);
            if (knowledge == null)
            {
                return JsonReturnHelper.ErrorReturn(805, "Invalid knowledgeId.");
            }

            var user = _meshContext.Users.First(u => u.Email == request.username);
            if (_permissionCheck.CheckTeamPermission(request.username, team) != PermissionCheckHelper.TeamAdmin ||
                knowledge.UserId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var uploader = _meshContext.Users.First(u => u.Id == knowledge.UserId).Nickname;
            
            try
            {
                knowledge.Title = request.knowledgeName;
                knowledge.Text = request.hyperlink;
                _meshContext.TeamMemos.Update(knowledge);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return MemoResult(new MemoInfo()
            {
                CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(knowledge.CreatedTime),
                Hyperlink = knowledge.Text,
                KnowledgeId = knowledge.Id,
                KnowledgeName = knowledge.Title,
                UploaderName = uploader
            });
        }
    }
}