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
}