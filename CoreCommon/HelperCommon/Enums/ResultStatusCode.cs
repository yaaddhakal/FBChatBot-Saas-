using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCommon.HelperCommon.Enums
{
    public enum ResultStatusCode
    {
        Ok = 200,//successful operations (e.g., user authenticated, token issued).
        BadRequest = 400,//client-side errors (e.g., validation failures, malformed requests).
        Unauthorized = 401,//access token issues (e.g., missing, invalid, expired).
        NotFound = 404,//resource not found (e.g., user doesn't exist).
        Forbidden = 403,//access denied (e.g., insufficient permissions).
        InternalServerError = 500 //unexpected server errors (e.g., database connection failures, unhandled exceptions).
    }


}
