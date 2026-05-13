using CoreCommon.HelperCommon.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCommon.HelperCommon
{
    public static class ActionResultHelper
    {
        public static IActionResult FromResult<T>(ControllerBase controller, ResultData<T> result)
        {
            var errorResponse = ApiResponse<object>.ErrorResponse(
                result.Error ?? "Invalid username or password",
                (ResultStatusCode)result.StatusCode
            );

            if (result.Success && result.Data != null)
            {
                return controller.Ok(ApiResponse<T>.SuccessResponse(
                    result.Data,
                    "Operation successful",
                    StatusCodes.Status200OK
                ));
            }

            if (result.StatusCode == (int)ResultStatusCode.BadRequest)
                return controller.BadRequest(errorResponse);

            if (result.StatusCode == (int)ResultStatusCode.Unauthorized)
                return controller.Unauthorized();

            if (result.StatusCode == (int)ResultStatusCode.NotFound)
                return controller.NotFound(errorResponse);

            return controller.StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
