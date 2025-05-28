using Microsoft.SemanticKernel;
using MultiAgentResearch.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAgentResearch.Plugins
{
    public class StaffLookupPlugin
    {
        [KernelFunction]
        [Description("Retrieves the provided users details")]
        public StaffDetails GetStaffDetails(string name)
        {
            switch (name.ToLower())
            {
                case "john doe":
                    return new StaffDetails(name, "Software Engineer", "Development", $"john.doe@benemanuel.net", "123-456-7890");
                case "jane smith":
                    return new StaffDetails(name, "Software Engineer", "Development", $"jane.smith@benemanuel.net", "123-456-7890");
            }

            return null;
        }
    }
}
