using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MeshBackend.Helpers
{ 
    public static class JsonReturnHelper
    {
        public static JsonResult ErrorReturn(int code, string msg)
        {
            return new JsonResult(new
                {
                    err_code = code ,
                    data = new
                    {
                        isScuess = false,
                        msg = msg
                    }
                });
        }

        
        public static JsonResult SuccessReturn()
        {
            return new JsonResult(new
                {
                    err_code = 0,
                    data = new
                    {
                        isSuccess = true
                    }
                });
        }
    }

    public class UserInfo
    {
        public string username { get; set; }
        public string nickname { get; set; }
        public int gender { get; set; }
        public int status { get; set; }
        public string address { get; set; }
        public string description { get; set; }
        public string birthday { get; set; }
        public string avatar { get; set; }
        public string role { get; set; }
        public UserPreference preference { get; set; }
        public List<TeamInfo> teams { get; set; }
    }

    public class UserPreference
    {
        public string preferenceShowMode { get; set; }
        public string preferenceColor { get; set; }
        public string preferenceLayout { get; set; }
        public int preferenceTeam { get; set; }
    }

    public class TeamInfo
    { 
        public string TeamName { get; set; }
        public int TeamId { get; set; }
        public int AdminId { get; set; }
        
        public string AdminName { get; set; }
        
        public string CreatedTIme { get; set; }
        
    }
    
}