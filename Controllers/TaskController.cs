using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh")]
    [Produces("application/json")]
    public class TaskController:Controller
    {
        private readonly MeshContext _meshContext;
        private readonly ILogger<TaskController> _logger;
        private readonly PermissionCheckHelper _permissionCheck;

        public enum Status
        {
            Expired,
            Completed,
            Developing
        }

        public TaskController(ILogger<TaskController> logger, MeshContext meshContext)
        {
            _logger = logger;
            _meshContext = meshContext;
            _permissionCheck=new PermissionCheckHelper(meshContext);
        }
        
        public JsonResult CheckUsername(string username)
        {
            if (!CornerCaseCheckHelper.Check(username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }
        public class SubTaskInfo
        {
            public int TaskId { get; set; }
            public string TaskName { get; set; }
            public long CreateTime { get; set; }
            public string Founder { get; set; }
            public bool isFinished { get; set; }
            public Status Status { get; set; }
            public string Principal { get; set; }
            public string Description { get; set; }
        }

        public class TaskInfo
        {
            public int TaskId { get; set; }
            public string TaskName { get; set; }
            public long CreateTime { get; set; }
            public string Deadline { get; set; }
            public string Founder { get; set; }
            public string Principal { get; set; }
            public string Description { get; set; }
            public Status Status { get; set; }
            public bool isFinished { get; set; }
            
            public int Priority { get; set; }
            
            public List<SubTaskInfo> SubTasks { get; set; }
        }

        public class TeamTaskInfo
        {
            public int TaskId { get; set; }
            public string TaskName { get; set; }
            public int ProjectId { get; set; }
            public string ProjectName { get; set; }
            public long CreateTime { get; set; }
            public string Deadline { get; set; }
            public string Founder { get; set; }
            public int Priority { get; set; }
            public string Principal { get; set; }
            public string Description { get; set; }
            public Status Status { get; set; }
            public bool IsFinished { get; set; }
            public List<SubTaskInfo> SubTasks { get; set; }
        }
        
        private static Status GetStatus(DateTime time,bool isFinished)
        {
            if (isFinished)
            {
                return Status.Completed;
            }

            return time > DateTime.Now ? Status.Developing : Status.Expired;
        }
        
        public JsonResult TaskResult(TaskInfo task)
        {
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    task = task
                }
            });
        }

        public JsonResult TaskListResult(List<TaskInfo> tasks)
        {
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    tasks = tasks
                }
            });
        }

        public JsonResult TeamTaskListResult(List<TeamTaskInfo> tasks)
        {
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    tasks = tasks
                }
            });
        }
        
        public JsonResult SubTaskResult(SubTaskInfo subTask)
        {
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    subTask = subTask
                }
            });
        }
        
        public List<SubTaskInfo> GetSubTasks(int taskId,string founder)
        {
            var subTasks = _meshContext.Subtasks
                .Where(s => s.TaskId == taskId)
                .Select(s => new SubTaskInfo()
                {
                    TaskName = s.Title,
                    TaskId = s.TaskId,
                    CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(s.CreatedTime),
                    Description = s.Description,
                    Founder = founder,
                    Principal = _meshContext.Assigns
                        .Where(a => a.TaskId == taskId && a.Title == s.Title)
                        .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Email)
                        .First()
                })
                .ToList();
            return subTasks;
        }

        public string GetSubTaskPrincipals(int taskId, string title)
        {
            return _meshContext.Assigns
                .Where(a => a.TaskId == taskId && a.Title == title)
                .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Email)
                .First();
        }
        
        public class TaskRequest
        {
            public string username { get; set; }
            public int projectId { get; set; }
            public string taskName { get; set; }
            public int taskId { get; set; }
            public bool isFinished { get; set; }
            public int priority { get; set; }
            public string deadline { get; set; }
            public string description { get; set; }
            public string principal { get; set; }
        }

        public class SubTaskRequest
        {
            public string username { get; set; }
            public int projectId { get; set; }
            public int taskId { get; set; }
            public string subTaskName { get; set; }
            public string description { get; set; }
            public string principal { get; set; }
            public bool isFinished { get; set; }
        }
        
        
        [HttpPost]
        [Route("task")]
        public JsonResult CreateTask(TaskRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }

            if (!CornerCaseCheckHelper.Check(request.taskName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(601, "Invalid taskName.");
            }

            if (!CornerCaseCheckHelper.Check(request.description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(request.principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
            }

            if (!CornerCaseCheckHelper.Check(request.deadline, 0, CornerCaseCheckHelper.Time))
            {
                return JsonReturnHelper.ErrorReturn(610, "Invalid deadline.");
            }

            var user = _meshContext.Users.First(u => u.Email == request.username);
            
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }

            if (_permissionCheck.CheckProjectPermission(request.username,project) !=PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }
            
            var taskBoard = _meshContext.TaskBoards.First(b => b.ProjectId == request.projectId);
            
            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == request.principal);
            if (principalUser == null)
            {
                return JsonReturnHelper.ErrorReturn(603, "Principal does not exist.");
            }

            var endTime = Convert.ToDateTime(request.deadline);
            
            
            var task = new Task()
            {
                BoardId = taskBoard.Id,
                Description = request.description,
                LeaderId = principalUser.Id,
                Name = request.taskName,
                Priority = request.priority,
                EndTime = endTime,
                StartTime = DateTime.Now
            };
            //Start Transaction to save the task
            using (var transaction = _meshContext.Database.BeginTransaction())
            {
                try
                {
                    _meshContext.Tasks.Add(task);
                    _meshContext.SaveChanges();
                    _meshContext.TaskFeeds.Add(new TaskFeed()
                    {
                        UserId = principalUser.Id,
                        TaskId = task.Id
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

            return TaskResult(new TaskInfo()
            {
                TaskName = task.Name,
                CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(task.CreatedTime),
                Description = task.Description,
                Deadline = task.EndTime.ToString("yyyy-MM-dd"),
                Founder = user.Nickname,
                TaskId = task.Id,
                Priority = task.Priority,
                isFinished = task.Finished,
                Principal = principalUser.Nickname,
                Status = GetStatus(task.EndTime,task.Finished),
                SubTasks = new List<SubTaskInfo>()
            });
        }

        [HttpDelete]
        [Route("task")]
        public JsonResult DeleteTask(string username, int projectId, int taskId)
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

            if (!CornerCaseCheckHelper.Check(taskId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(501, "Invalid taskId.");
            }


            var user = _meshContext.Users.First(u => u.Email == username);

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }
            
            var task = _meshContext.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return JsonReturnHelper.ErrorReturn(604, "Task does not exist.");
            }

            if (_permissionCheck.CheckProjectPermission(username, project) != PermissionCheckHelper.ProjectAdmin &&
                task.LeaderId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            try
            {
                _meshContext.Tasks.Remove(task);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return JsonReturnHelper.SuccessReturn();
        }

        [HttpPatch]
        [Route("task")]
        public JsonResult UpdateTask(TaskRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }

            if (!CornerCaseCheckHelper.Check(request.taskId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(501, "Invalid taskId.");
            }

            if (!CornerCaseCheckHelper.Check(request.description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(request.principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
            }

            if (!CornerCaseCheckHelper.Check(request.deadline, 0, CornerCaseCheckHelper.Time))
            {
                return JsonReturnHelper.ErrorReturn(610, "Invalid deadline.");
            }
            
            
            var user = _meshContext.Users.First(u => u.Email == request.username);

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }
            
            var task = _meshContext.Tasks.FirstOrDefault(t => t.Id == request.taskId);
            if (task == null)
            {
                return JsonReturnHelper.ErrorReturn(604, "Task does not exist.");
            }

            if (_permissionCheck.CheckProjectPermission(request.username, project) != PermissionCheckHelper.ProjectAdmin &&
                task.LeaderId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == request.principal);
            if (principalUser == null || _permissionCheck.CheckProjectPermission(request.principal, project) ==
                PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(608, "Principal does not exist.");
            }

            var endTime = Convert.ToDateTime(request.deadline);

            try
            {
                task.Finished = request.isFinished;
                task.Priority = request.priority;
                task.EndTime = endTime;
                task.Description = request.description;
                task.LeaderId = principalUser.Id;
                _meshContext.Tasks.Update(task);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            var founder = _meshContext.Users.First(u => u.Id == project.AdminId);

            
            return TaskResult(new TaskInfo()
            {    
                TaskId = task.Id,
                CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(task.CreatedTime),
                Description = task.Description,
                Deadline = task.EndTime.ToString("yyyy-MM-dd"),
                Founder = founder.Nickname,
                TaskName = task.Name,
                isFinished = task.Finished,
                Status = GetStatus(task.EndTime,task.Finished),
                Principal = principalUser.Nickname,
                Priority = task.Priority,
                SubTasks = GetSubTasks(task.Id,founder.Nickname)
            });
        }

        [HttpPost]
        [Route("subtask")]
        public JsonResult CreateSubTask(SubTaskRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }

            if (!CornerCaseCheckHelper.Check(request.taskId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(501, "Invalid taskId.");
            }

            if (!CornerCaseCheckHelper.Check(request.subTaskName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(511, "Invalid subTaskName.");
            }

            if (!CornerCaseCheckHelper.Check(request.description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(request.principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
            }
            
            var user = _meshContext.Users.First(u => u.Email == request.username);

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }
            
            var task = _meshContext.Tasks.FirstOrDefault(t => t.Id == request.taskId);
            if (task == null)
            {
                return JsonReturnHelper.ErrorReturn(604, "Task does not exist.");
            }

            if (_permissionCheck.CheckProjectPermission(request.username,project) !=PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            var subTask = _meshContext.Subtasks.FirstOrDefault(s => s.TaskId == task.Id && s.Title == request.subTaskName);
            if (subTask != null)
            {
                return JsonReturnHelper.ErrorReturn(608, "subTaskName already exists.");
            }
            
            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == request.principal);
            if (principalUser == null && !request.principal.IsNullOrEmpty())
            {
                return JsonReturnHelper.ErrorReturn(603, "Principal does not exist.");
            }

            var newSubTask = new Subtask()
            {
                TaskId = task.Id,
                Title = request.subTaskName,
                Description = request.description
            };
            
            using (var transaction = _meshContext.Database.BeginTransaction())
            {
                try
                {
                    _meshContext.Subtasks.Add(newSubTask);
                    _meshContext.Assigns.Add(new Assign()
                    {
                        TaskId = newSubTask.TaskId,
                        Title = newSubTask.Title,
                        UserId = principalUser?.Id ?? user.Id
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
            
            
            return SubTaskResult(new SubTaskInfo()
            {
                TaskId = newSubTask.TaskId,
                CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(newSubTask.CreatedTime),
                Description = newSubTask.Description,
                Founder = user.Nickname,
                TaskName = newSubTask.Title,
                isFinished = newSubTask.Finished,
                Status = GetStatus(task.EndTime,newSubTask.Finished),
                Principal = GetSubTaskPrincipals(newSubTask.TaskId,newSubTask.Title)
            });

        }

        [HttpDelete]
        [Route("subtask")]
        public JsonResult DeleteSubTask(string username, int projectId,int taskId,string subTaskName)
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

            if (!CornerCaseCheckHelper.Check(taskId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(501, "Invalid taskId.");
            }

            if (!CornerCaseCheckHelper.Check(subTaskName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(511, "Invalid subTaskName.");
            }

            var user = _meshContext.Users.First(u => u.Email == username);

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }


            var task = _meshContext.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return JsonReturnHelper.ErrorReturn(604, "Task does not exist.");
            }
            

            if (_permissionCheck.CheckProjectPermission(username, project) != PermissionCheckHelper.ProjectAdmin &&
                task.LeaderId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }
            
            var subTask = _meshContext.Subtasks.FirstOrDefault(s => s.TaskId == taskId && s.Title == subTaskName);
            if (subTask == null)
            {
                return JsonReturnHelper.ErrorReturn(606, "SubTask does not exist.");
            }

            try
            {
                _meshContext.Subtasks.Remove(subTask);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return JsonReturnHelper.SuccessReturn();
        }

        [HttpPatch]
        [Route("subtask")]
        public JsonResult UpdateSubTask(SubTaskRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(701, "Invalid projectId.");
            }

            if (!CornerCaseCheckHelper.Check(request.taskId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(501, "Invalid taskId.");
            }

            if (!CornerCaseCheckHelper.Check(request.subTaskName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(511, "Invalid subTaskName.");
            }

            if (!CornerCaseCheckHelper.Check(request.description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(request.principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
            }

            
            var user = _meshContext.Users.First(u => u.Email == request.username);

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }

            var task = _meshContext.Tasks.FirstOrDefault(t => t.Id == request.taskId);
            if (task == null)
            {
                return JsonReturnHelper.ErrorReturn(604, "Task does not exist.");
            }

            if (_permissionCheck.CheckProjectPermission(request.username, project) != PermissionCheckHelper.ProjectAdmin &&
                task.LeaderId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == request.principal);
            if (principalUser == null || _permissionCheck.CheckProjectPermission(request.principal, project) ==
                PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(608, "Principal does not exist.");
            }

            var subTask = _meshContext.Subtasks.FirstOrDefault(s => s.TaskId == request.taskId && s.Title == request.subTaskName);
            if (subTask == null)
            {
                return JsonReturnHelper.ErrorReturn(606, "SubTask does not exist.");
            }

            var assign = _meshContext.Assigns.First(a => a.TaskId == subTask.TaskId && a.Title == subTask.Title);

            try
            {
                subTask.Finished = request.isFinished;
                subTask.Description = request.description;
                _meshContext.Update(subTask);
                _meshContext.Assigns.Remove(assign);
                _meshContext.Assigns.Add(new Assign()
                {
                    UserId = principalUser.Id,
                    TaskId = subTask.TaskId,
                    Title = subTask.Title
                });
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }


            return SubTaskResult(new SubTaskInfo()
                {
                    CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(subTask.CreatedTime),
                    Description = subTask.Description,
                    Founder = user.Nickname,
                    TaskId = subTask.TaskId,
                    TaskName = subTask.Title,
                    isFinished = subTask.Finished,
                    Status = GetStatus(task.EndTime,subTask.Finished),
                    Principal = GetSubTaskPrincipals(subTask.TaskId,subTask.Title)
                }
            );
        }

        [HttpGet]
        [Route("task/project")]
        public JsonResult QueryProjectTask(string username, int projectId)
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

            var project = _meshContext.Projects.FirstOrDefault(p => p.Id ==projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Invalid projectId.");
            }

            if (_permissionCheck.CheckProjectPermission(username, project) == PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            var founder = _meshContext.Users.First(u => u.Id == project.AdminId);
            
            var board = _meshContext.TaskBoards.First(b => b.ProjectId == projectId);
            var tasks = _meshContext.Tasks
                .Where(s => s.BoardId == board.Id)
                .Select(s=>new TaskInfo()
                {
                    TaskId = s.Id,
                    CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(s.CreatedTime),
                    Description = s.Description,
                    Deadline = s.EndTime.ToString("yyyy-MM-dd"),
                    Founder = founder.Nickname,
                    isFinished = s.Finished,
                    TaskName = s.Name,
                    Priority = s.Priority,
                    Principal = _meshContext.Users.First(u=>u.Id==s.LeaderId).Nickname,
                    SubTasks = _meshContext.Subtasks
                        .Where(b => b.TaskId == s.Id)
                        .Select(e => new SubTaskInfo()
                        {
                            TaskName = e.Title,
                            TaskId = e.TaskId,
                            CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(e.CreatedTime),
                            Description = e.Description,
                            isFinished = e.Finished,
                            Founder = founder.Nickname,
                            Principal = _meshContext.Assigns
                                .Where(a => a.TaskId == e.TaskId && a.Title == e.Title)
                                .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Email)
                                .First()
                        })
                        .ToList()
                })
                .ToList();

            foreach (var task in tasks)
            {
                task.Status = GetStatus(Convert.ToDateTime(task.Deadline), task.isFinished);
                foreach (var subTask in task.SubTasks)
                {
                    subTask.Status = GetStatus(Convert.ToDateTime(task.Deadline), subTask.isFinished);
                }
            }
            
            return TaskListResult(tasks);
        }

        [HttpGet]
        [Route("task/team")]
        public JsonResult QueryTeamTasks(string username, int teamId)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(teamId,0,CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(301, "Invalid teamId.");
            }
            

            var user = _meshContext.Users.First(u => u.Email == username);

            var team = _meshContext.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                return JsonReturnHelper.ErrorReturn(302, "Invalid teamId.");
            }

            if (_permissionCheck.CheckTeamPermission(username, team) == PermissionCheckHelper.TeamOutsider)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            var tasks = _meshContext.Develops
                .Where(d => d.UserId == user.Id)
                .Join(_meshContext.TaskBoards, d => d.ProjectId, t => t.ProjectId, (d, t) => t)
                .Join(_meshContext.Projects, t => t.ProjectId, p => p.Id, (t, p) => new
                {
                    projectId = p.Id,
                    projectName = p.Name,
                    projectAdminId = p.AdminId,
                    boardId = t.Id
                })
                .Join(_meshContext.Tasks, t => t.boardId, s => s.BoardId, (t, s) => new
                {
                    t,
                    s
                })
                .Join(_meshContext.Users, pp => pp.t.projectAdminId, u => u.Id, (t, u) => new
                {
                    Founder = u.Nickname,
                    task = t.s,
                    projectId = t.t.projectId,
                    projectName = t.t.projectName
                })
                .Select(m => new TeamTaskInfo()
                {
                    ProjectId = m.projectId,
                    ProjectName = m.projectName,
                    TaskId = m.task.Id,
                    CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(m.task.CreatedTime),
                    Description = m.task.Description,
                    Deadline = m.task.EndTime.ToString("yyyy-MM-dd"),
                    Founder = m.Founder,
                    TaskName = m.task.Name,
                    IsFinished = m.task.Finished,
                    Principal = _meshContext.Users.First(u => u.Id == m.task.LeaderId).Nickname,
                    Priority = m.task.Priority
                })
                .ToList();

            foreach (var task in tasks)
            {
                task.Status = GetStatus(Convert.ToDateTime(task.Deadline), task.IsFinished);
                task.SubTasks = _meshContext.Subtasks
                    .Where(b => b.TaskId == task.TaskId)
                    .Select(s => new SubTaskInfo()
                    {
                        TaskName = s.Title,
                        TaskId = s.TaskId,
                        CreateTime = TimeStampConvertHelper.ConvertToTimeStamp(s.CreatedTime),
                        Description = s.Description,
                        Founder = task.Founder, 
                        Principal = _meshContext.Assigns
                            .Where(a => a.TaskId == s.TaskId && a.Title == s.Title)
                            .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Email)
                            .First()
                    })
                    .ToList();
                foreach (var subTask in task.SubTasks)
                {
                    subTask.Status = GetStatus(Convert.ToDateTime(task.Deadline), subTask.isFinished);
                }
            }
            
            return TeamTaskListResult(tasks);
        }
        
    }
}