using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XTI_App.Api;

namespace XTI_TempLog.Api
{
    public sealed class MoveToPermanentAction : AppAction<EmptyRequest, EmptyActionResult>
    {
        public async Task<EmptyActionResult> Execute(EmptyRequest model)
        {
            return new EmptyActionResult();
        }
    }
}
