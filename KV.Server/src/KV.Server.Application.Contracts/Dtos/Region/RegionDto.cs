using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KV.Server.Dtos.Region;
public class RegionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
}
