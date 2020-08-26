using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
            if (username.IsNullOrEmpty() || username.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }

        public class MemoInfo
        {
            public int KnowledgeId { get; set; }
            public string KnowledgeName { get; set; }
            public string HyperLink { get; set; }
            public DateTime CreateTime { get; set; }
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
                    knowledge = memoList
                }
            });
        }
        
        [Route("project")]
        [HttpPost]
        public JsonResult CreateProjectKB(string username, int projectId, string knowledgeName, string hyperlink)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (knowledgeName==null||knowledgeName.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (hyperlink==null||hyperlink.Length > 100)
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
            }

            var memoCollection = _meshContext.ProjectMemoCollections.First(p => p.ProjectId == project.Id);
            if (_permissionCheck.CheckProjectPermission(username, project) == PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var newMemo = new ProjectMemo()
            {
                Title = knowledgeName,
                CollectionId = memoCollection.Id,
                Text = hyperlink,
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
                KnowledgeName = knowledgeName,
                HyperLink = hyperlink,
                CreateTime = newMemo.CreatedTime,
                KnowledgeId = newMemo.Id,
                UploaderName = username
            });
        }
        
        [Route("team")]
        [HttpPost]
        public JsonResult CreateTeamKB(string username, int teamId, string knowledgeName, string hyperlink)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (knowledgeName==null||knowledgeName.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (hyperlink==null||hyperlink.Length > 100)
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }

            var memoCollection = _meshContext.TeamMemoCollections.First(p => p.TeamId == team.Id);
            if (_permissionCheck.CheckTeamPermission(username, team) == PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(801, "Permission denied.");
            }

            var newMemo = new TeamMemo()
            {
                Title = knowledgeName,
                CollectionId = memoCollection.Id,
                Text = hyperlink,
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
                KnowledgeName = knowledgeName,
                HyperLink = hyperlink,
                CreateTime = newMemo.CreatedTime,
                KnowledgeId = newMemo.Id,
                UploaderName = username
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

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
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

            var team = _meshContext.Teams.FirstOrDefault(p => p.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
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
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
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
                    HyperLink = m.Text,
                    CreateTime  = m.CreatedTime,
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
            var team = _meshContext.Projects.FirstOrDefault(p => p.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }
            
            if (_permissionCheck.CheckProjectPermission(username, team) == PermissionCheckHelper.TeamOutsider)
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
                    HyperLink = m.Text,
                    CreateTime  = m.CreatedTime,
                    UploaderName = u.Nickname
                }).ToList();

            return MemoListResult(memoLists);
        }


        [HttpPatch]
        [Route("project")]
        public JsonResult UpdateProjectKB(string username, int projectId, int knowledgeId, string knowledgeName,
            string hyperlink)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (knowledgeName.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (hyperlink.Length > 100)
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }
            
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
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

            var uploader = _meshContext.Users.First(u => u.Id == knowledge.UserId).Nickname;
            
            try
            {
                if (!knowledgeName.IsNullOrEmpty())
                {
                    knowledge.Title = knowledgeName;
                }

                if (!hyperlink.IsNullOrEmpty())
                {
                    knowledge.Text = hyperlink;
                }

                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return MemoResult(new MemoInfo()
            {
                CreateTime = knowledge.CreatedTime,
                HyperLink = knowledge.Text,
                KnowledgeId = knowledge.Id,
                KnowledgeName = knowledge.Title,
                UploaderName = uploader
            });
        }
        
        
        [HttpPatch]
        [Route("team")]
        public JsonResult UpdateTeamKB(string username, int teamId, int knowledgeId, string knowledgeName,
            string hyperlink)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (knowledgeName.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(802, "Invalid KnowledgeName.");
            }

            if (hyperlink.Length > 100)
            {
                return JsonReturnHelper.ErrorReturn(803, "Invalid hyperlink.");
            }
            
            var team = _meshContext.Teams.FirstOrDefault(p => p.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid teamId.");
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

            var uploader = _meshContext.Users.First(u => u.Id == knowledge.UserId).Nickname;
            
            try
            {
                if (!knowledgeName.IsNullOrEmpty())
                {
                    knowledge.Title = knowledgeName;
                }

                if (!hyperlink.IsNullOrEmpty())
                {
                    knowledge.Text = hyperlink;
                }

                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return MemoResult(new MemoInfo()
            {
                CreateTime = knowledge.CreatedTime,
                HyperLink = knowledge.Text,
                KnowledgeId = knowledge.Id,
                KnowledgeName = knowledge.Title,
                UploaderName = uploader
            });
        }
    }
}