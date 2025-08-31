using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        //Relacion de tablas
        public ICollection<ProjectProposal> ProjectProposals { get; set; }
        public ICollection<ProjectApprovalStep> ProjectApprovalSteps { get; set; }
    }
}
