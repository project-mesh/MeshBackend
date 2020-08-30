using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            public string Title { get; set; }
            public DateTime CreatedTime { get; set; }
            public string Founder { get; set; }
            public List<string> Principal { get; set; }
            public string Description { get; set; }
        }

        public class TaskInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Founder { get; set; }
            public string Principal { get; set; }
            public string Description { get; set; }

            public List<SubTaskInfo> SubTasks;
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
                    Title = s.Title,
                    TaskId = s.TaskId,
                    CreatedTime = s.CreatedTime,
                    Description = s.Description,
                    Founder = founder,
                    Principal = _meshContext.Assigns
                        .Where(a => a.TaskId == taskId && a.Title == s.Title)
                        .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Nickname)
                        .ToList()
                })
                .ToList();
            return subTasks;
        }

        public List<string> GetSubTaskPrincipals(int taskId, string title)
        {
            return _meshContext.Assigns
                .Where(a => a.TaskId == taskId && a.Title == title)
                .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Nickname)
                .ToList();
        }
        
        [HttpPost]
        [Route("task")]
        public JsonResult CreateTask(string username, int projectId, string taskName, int priority, string deadline,
            string description, string principal)
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

            if (!CornerCaseCheckHelper.Check(taskName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(601, "Invalid taskName.");
            }

            if (!CornerCaseCheckHelper.Check(description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
            }

            if (!CornerCaseCheckHelper.Check(deadline, 0, CornerCaseCheckHelper.Time))
            {
                return JsonReturnHelper.ErrorReturn(610, "Invalid deadline.");
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(707, "Project does not exist.");
            }

            if (_permissionCheck.CheckProjectPermission(username,project) !=PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }
            
            var taskBoard = _meshContext.TaskBoards.First(b => b.ProjectId == projectId);
            
            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == principal);
            if (principalUser == null)
            {
                return JsonReturnHelper.ErrorReturn(603, "Principal does not exist.");
            }

            var endTime = Convert.ToDateTime(deadline);
            
            
            var task = new Task()
            {
                BoardId = taskBoard.Id,
                Description = description,
                LeaderId = principalUser.Id,
                Name = taskName,
                Priority = priority,
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
                Name = task.Name,
                CreatedTime = task.CreatedTime,
                Description = task.Description,
                EndTime = task.EndTime,
                Founder = user.Nickname,
                Id = task.Id,
                Principal = principalUser.Nickname,
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
        public JsonResult UpdateTask(string username, int projectId, int taskId, bool isFinished, int priority,
            string deadline, string description, string principal)
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

            if (!CornerCaseCheckHelper.Check(description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
            }

            if (!CornerCaseCheckHelper.Check(deadline, 0, CornerCaseCheckHelper.Time))
            {
                return JsonReturnHelper.ErrorReturn(610, "Invalid deadline.");
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

            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == principal);
            if (principalUser == null || _permissionCheck.CheckProjectPermission(principal, project) ==
                PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(608, "Principal does not exist.");
            }

            var endTime = Convert.ToDateTime(deadline);

            try
            {
                task.Finished = isFinished;
                task.Priority = priority;
                task.EndTime = endTime;
                task.Description = description;
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
                Id = task.Id,
                CreatedTime = task.CreatedTime,
                Description = task.Description,
                EndTime = task.EndTime,
                Founder = founder.Nickname,
                Name = task.Name,
                Principal = principalUser.Nickname,
                SubTasks = GetSubTasks(task.Id,founder.Nickname)
            });
        }

        [HttpPost]
        [Route("subtask")]
        public JsonResult CreateSubTask(string username, int projectId, int taskId, string subTaskName,
            string description, string principal)
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

            if (!CornerCaseCheckHelper.Check(description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
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

            if (_permissionCheck.CheckProjectPermission(username,project) !=PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(701, "Permission denied.");
            }

            var subTask = _meshContext.Subtasks.FirstOrDefault(s => s.TaskId == task.Id && s.Title == subTaskName);
            if (subTask != null)
            {
                return JsonReturnHelper.ErrorReturn(608, "subTaskName already exists.");
            }

            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == principal);
            if (principalUser == null)
            {
                return JsonReturnHelper.ErrorReturn(603, "Principal does not exist.");
            }

            var newSubTask = new Subtask()
            {
                TaskId = task.Id,
                Title = subTaskName,
                Description = description
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
                        UserId = principalUser.Id
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
                CreatedTime = newSubTask.CreatedTime,
                Description = newSubTask.Description,
                Founder = user.Nickname,
                Title = newSubTask.Title,
                Principal = GetSubTaskPrincipals(newSubTask.TaskId,newSubTask.Title)
            });

        }

        [HttpDelete]
        [Route("subtask")]
        public JsonResult DeleteSubTask(string username, int projectId, int taskId, string subTaskName)
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
        public JsonResult UpdateSubTask(string username, int projectId, int taskId, string subTaskName, bool isFinished,
            string description, string principal)
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

            if (!CornerCaseCheckHelper.Check(description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(602, "Invalid description.");
            }

            if (!CornerCaseCheckHelper.Check(principal, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(101, "Invalid principal.");
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

            var principalUser = _meshContext.Users.FirstOrDefault(u => u.Email == principal);
            if (principalUser == null || _permissionCheck.CheckProjectPermission(principal, project) ==
                PermissionCheckHelper.ProjectOutsider)
            {
                return JsonReturnHelper.ErrorReturn(608, "Principal does not exist.");
            }

            var subTask = _meshContext.Subtasks.FirstOrDefault(s => s.TaskId == taskId && s.Title == subTaskName);
            if (subTask == null)
            {
                return JsonReturnHelper.ErrorReturn(606, "SubTask does not exist.");
            }

            var assign = _meshContext.Assigns.First(a => a.TaskId == subTask.TaskId && a.Title == subTask.Title);

            try
            {
                subTask.Finished = isFinished;
                subTask.Description = description;
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
                    CreatedTime = subTask.CreatedTime,
                    Description = subTask.Description,
                    Founder = user.Nickname,
                    TaskId = subTask.TaskId,
                    Title = subTask.Title,
                    Principal = GetSubTaskPrincipals(subTask.TaskId,subTask.Title)
                }
            );
        }

        [HttpGet]
        [Route("task/project")]
        public JsonResult QueryProjectTasks(string username, int projectId)
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
                    Id = s.Id,
                    CreatedTime = s.CreatedTime,
                    Description = s.Description,
                    EndTime = s.EndTime,
                    Founder = founder.Nickname,
                    Name = s.Name,
                    Principal = _meshContext.Users.First(u=>u.Id==s.LeaderId).Nickname,
                    SubTasks = _meshContext.Subtasks
                        .Where(b => b.TaskId == s.Id)
                        .Select(e => new SubTaskInfo()
                        {
                            Title = e.Title,
                            TaskId = e.TaskId,
                            CreatedTime = e.CreatedTime,
                            Description = e.Description,
                            Founder = founder.Nickname,
                            Principal = _meshContext.Assigns
                                .Where(a => a.TaskId == e.TaskId && a.Title == e.Title)
                                .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Nickname)
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();
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
                    projectAdminId = p.AdminId,
                    boardId = t.Id
                })
                .Join(_meshContext.Tasks, t => t.boardId, s => s.BoardId, (t, s) => new
                {
                    t.projectAdminId,
                    s
                })
                .Join(_meshContext.Users, t => t.projectAdminId, u => u.Id, (t, u) => new
                {
                    Founder = u.Nickname,
                    task = t.s
                })
                .Select(m => new TaskInfo()
                {
                    Id = m.task.Id,
                    CreatedTime = m.task.CreatedTime,
                    Description = m.task.Description,
                    EndTime = m.task.EndTime,
                    Founder = m.Founder,
                    Name = m.task.Name,
                    Principal = _meshContext.Users.First(u => u.Id == m.task.LeaderId).Nickname,
                })
                .ToList();

            foreach (var task in tasks)
            {
                task.SubTasks = _meshContext.Subtasks
                    .Where(b => b.TaskId == task.Id)
                    .Select(s => new SubTaskInfo()
                    {
                        Title = s.Title,
                        TaskId = s.TaskId,
                        CreatedTime = s.CreatedTime,
                        Description = s.Description,
                        Founder = task.Founder,
                        Principal = _meshContext.Assigns
                            .Where(a => a.TaskId == s.TaskId && a.Title == s.Title)
                            .Join(_meshContext.Users, n => n.UserId, u => u.Id, (n, u) => u.Nickname)
                            .ToList()
                    })
                    .ToList();
            }
            
            return TaskListResult(tasks);
        }
        
    }
}