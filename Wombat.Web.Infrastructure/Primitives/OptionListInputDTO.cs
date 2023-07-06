using System.Collections.Generic;

namespace Wombat.Web.Infrastructure
{
    public class OptionListInputDTO
    {
        public List<string> selectedValues { get; set; }
        public string q { get; set; }
    }
}