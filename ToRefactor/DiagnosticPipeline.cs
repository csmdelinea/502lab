using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ToRefactor
{
  public class DiagnosticPipeline
    {
        public static Task DiagnosticPipelineStep(HttpContext context, Func<Task> next)
        {
            // Can read data from the request via the context
            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            if (context.Response.StatusCode != 200)
            {
                var x = "Y";
            }
            // The context also stores a ReverseProxyFeature which holds proxy specific data such as the cluster, route and destinations
            //var proxyFeature = context.GetReverseProxyFeature();
            //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(proxyFeature.Route.Config));

            // Important - required to move to the next step in the proxy pipeline
            return next();
        }
    }
}
